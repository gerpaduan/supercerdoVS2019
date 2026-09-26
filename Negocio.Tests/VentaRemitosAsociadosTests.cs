using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre en Negocio.Venta: la leyenda "REMITOS ASOCIADOS: a / b" que se agrega a las
    // observaciones de la venta al guardarla con expendios del sector REMITOS, y el numero de
    // remito sugerido por sucursal (proximoNroRemito).
    public class VentaRemitosAsociadosTests
    {
        private static Venta CrearVenta(params int[] idsExpendios) => new Venta
        {
            IdVenta = 0,
            FormaPago = Venta.FormaPagoEnum.Efectivo.ToString(),
            LineasVenta = new List<LineaVenta>(),
            ListaExpendios = new List<int>(idsExpendios),
            Observaciones = "",
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = 13 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = new DateTime(2026, 9, 26, 10, 0, 0),
            Creado = new DateTime(2026, 9, 26, 10, 0, 0),
        };

        private static Venta Expendio(string sector, string nroRemito) => new Venta { Sector = sector, NroRemito = nroRemito };

        private static Negocio.Venta CrearVentaN(FakeVentaRepository repo) =>
            new Negocio.Venta(
                repo,
                new EmpresaContextFake(1),
                new FakeParametrosContext(),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)),
                cierreCajaN: new Negocio.CierreCaja(new FakeCierreCajaRepository(), new EmpresaContextFake(1)));

        [Fact]
        public void Un_remito_agrega_la_leyenda_a_las_observaciones()
        {
            var repo = new FakeVentaRepository();
            repo.Expendios[10] = Expendio("REMITOS", "0001-00000005");
            var venta = CrearVenta(10);

            CrearVentaN(repo).agregarVenta(venta);

            Assert.Equal("REMITOS ASOCIADOS: 0001-00000005", venta.Observaciones);
        }

        [Fact]
        public void Varios_remitos_se_separan_con_barra_y_conservan_lo_escrito()
        {
            var repo = new FakeVentaRepository();
            repo.Expendios[10] = Expendio("REMITOS", "0001-00000005");
            repo.Expendios[11] = Expendio("REMITOS", "0001-00000006");
            var venta = CrearVenta(10, 11);
            venta.Observaciones = "Entregar por la tarde";

            CrearVentaN(repo).agregarVenta(venta);

            Assert.Equal("Entregar por la tarde\nREMITOS ASOCIADOS: 0001-00000005 / 0001-00000006", venta.Observaciones);
        }

        [Fact]
        public void Expendios_de_otros_sectores_no_aportan_remitos()
        {
            var repo = new FakeVentaRepository();
            repo.Expendios[10] = Expendio("CARNICERIA", "");
            repo.Expendios[11] = Expendio("PRESUPUESTO", "X-1");
            var venta = CrearVenta(10, 11);

            CrearVentaN(repo).agregarVenta(venta);

            Assert.Equal("", venta.Observaciones);
        }

        [Fact]
        public void Modificar_no_duplica_la_leyenda_y_suma_remitos_nuevos()
        {
            var repo = new FakeVentaRepository();
            repo.Expendios[10] = Expendio("REMITOS", "A");
            repo.Expendios[11] = Expendio("REMITOS", "B");
            var venta = CrearVenta(10);
            var negocio = CrearVentaN(repo);

            negocio.agregarVenta(venta);
            venta.ListaExpendios = new List<int> { 10, 11 };
            negocio.modificarVenta(venta, 1, false, null);

            Assert.Equal("REMITOS ASOCIADOS: A / B", venta.Observaciones);
        }

        [Fact]
        public void Sin_expendios_no_consulta_la_base_ni_toca_las_observaciones()
        {
            var repo = new FakeVentaRepository();
            var venta = CrearVenta();
            venta.Observaciones = "Nota";

            CrearVentaN(repo).agregarVenta(venta);

            Assert.Equal("Nota", venta.Observaciones);
        }

        [Fact]
        public void ProximoNroRemito_es_el_siguiente_al_ultimo_de_la_sucursal()
        {
            var repo = new FakeVentaRepository { OnUltimoCorrelativoRemito = (suc, prefijo) => prefijo == "0003" ? 41 : 0 };

            Assert.Equal("0003-00000042", CrearVentaN(repo).proximoNroRemito(3));
            Assert.Equal("0004-00000001", CrearVentaN(repo).proximoNroRemito(4));
        }
    }
}
