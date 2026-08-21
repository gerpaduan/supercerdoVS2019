using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Primer test de esta suite sobre logica de negocio que NO es el contrato IUnitOfWork:
    // el calculo de ComisionTarjeta dentro de agregarVenta/modificarVenta (switch sobre
    // FormaPago, parametrizado por comisionDebito/comisionCredito -- Entidades.ParamKeys).
    // Regla de negocio real, con calculo derivado de parametros -- exactamente el tipo de logica
    // que CLAUDE.md (Seccion 2.3) pide cubrir con tests.
    //
    // El switch vive dentro del metodo privado EjecutarAgregarVenta/EjecutarModificarVenta, sin
    // extraerlo a un metodo propio (no se refactoriza de paso solo para testear mas facil) --
    // se prueba de forma indirecta llamando a agregarVenta/modificarVenta y verificando el
    // efecto observable: oVentaE.ComisionTarjeta queda seteado en el mismo objeto pasado por
    // referencia.
    public class VentaComisionTarjetaTests
    {
        private static Venta CrearVenta(string formaPago) => new Venta
        {
            IdVenta = 0,
            FormaPago = formaPago,
            EnCtaCte = true,
            LineasVenta = new List<LineaVenta>(),
            ListaExpendios = null,
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = 13 },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRemito = "R-001",
        };

        // cierreCajaN inyectado: FormaPago Debito/Credito (a diferencia de Efectivo) hace que
        // egresoCajaPagoTarjeta llame a oCierreN.addOrEditEgresoCaja -- sin este fake, caeria al
        // constructor SQL-Server-only de siempre e intentaria abrir una conexion real.
        private static Negocio.Venta CrearVentaN(FakeParametrosContext param) =>
            new Negocio.Venta(
                new FakeVentaRepository(),
                new EmpresaContextFake(1),
                param,
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)),
                cierreCajaN: new Negocio.CierreCaja(new FakeCierreCajaRepository(), new EmpresaContextFake(1)));

        [Fact]
        public void Efectivo_ComisionSiempreCero()
        {
            var param = new FakeParametrosContext().ConFloat(ParamKeys.ComisionDebito, 3.5f).ConFloat(ParamKeys.ComisionCredito, 5f);
            var venta = CrearVenta(Venta.FormaPagoEnum.Efectivo.ToString());

            CrearVentaN(param).agregarVenta(venta);

            Assert.Equal(0, venta.ComisionTarjeta);
        }

        [Fact]
        public void Debito_TomaElPorcentajeDelParametroComisionDebito()
        {
            var param = new FakeParametrosContext().ConFloat(ParamKeys.ComisionDebito, 3.5f);
            var venta = CrearVenta(Venta.FormaPagoEnum.Debito.ToString());

            CrearVentaN(param).agregarVenta(venta);

            Assert.Equal(3.5f, venta.ComisionTarjeta);
        }

        [Fact]
        public void Credito_TomaElPorcentajeDelParametroComisionCredito()
        {
            var param = new FakeParametrosContext().ConFloat(ParamKeys.ComisionCredito, 5f);
            var venta = CrearVenta(Venta.FormaPagoEnum.Credito.ToString());

            CrearVentaN(param).agregarVenta(venta);

            Assert.Equal(5f, venta.ComisionTarjeta);
        }

        [Fact]
        public void OtraFormaDePago_ComisionCero()
        {
            var param = new FakeParametrosContext().ConFloat(ParamKeys.ComisionDebito, 3.5f).ConFloat(ParamKeys.ComisionCredito, 5f);
            var venta = CrearVenta(Venta.FormaPagoEnum.CtaCte.ToString());

            CrearVentaN(param).agregarVenta(venta);

            Assert.Equal(0, venta.ComisionTarjeta);
        }

        [Fact]
        public void ModificarVenta_TambienCalculaLaComision()
        {
            var param = new FakeParametrosContext().ConFloat(ParamKeys.ComisionCredito, 5f);
            var venta = CrearVenta(Venta.FormaPagoEnum.Credito.ToString());
            venta.IdVenta = 61;

            CrearVentaN(param).modificarVenta(venta, SucAnterior: 1, eliminarLineas: false, lineaNuevosAnulados: null);

            Assert.Equal(5f, venta.ComisionTarjeta);
        }
    }
}
