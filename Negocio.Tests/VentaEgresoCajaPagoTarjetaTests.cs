using System;
using System.Collections.Generic;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Cubre el calculo real de Negocio.Venta.egresoCajaPagoTarjeta (llamado siempre por
    // agregarVenta/modificarVenta salvo FormaPago=Efectivo): que EgresoCaja quede armado con
    // el Monto, IdTipoEgresoCaja y Descripcion correctos segun el caso -- CtaCte (no sale plata
    // de la caja, se informa aparte), tarjeta simple, y pago mixto tarjeta+efectivo.
    public class VentaEgresoCajaPagoTarjetaTests
    {
        private static Venta CrearVenta(string formaPago, float pagoMixtoEfectivo = 0) => new Venta
        {
            IdVenta = 61,
            FormaPago = formaPago,
            EnCtaCte = formaPago == Venta.FormaPagoEnum.CtaCte.ToString(),
            // KgsTotalCalculado, no CantKg: el propio EjecutarAgregarVenta pisa CantKg con
            // KgsTotalCalculado en su loop de lineas (linea.CantKg = linea.KgsTotalCalculado;),
            // ANTES de que egresoCajaPagoTarjeta calcule el total -- hallazgo real durante el
            // primer intento de este test (esperaba que CantKg quedara como se seteo aca).
            LineasVenta = new List<LineaVenta>
            {
                new LineaVenta { KgsTotalCalculado = 10, PrecioKg = 20, Corte = new Corte { precioKgReferencia = 20 } },
            },
            ListaExpendios = null,
            Vendedor = new Usuario { Id = 2 },
            Persona = new Persona { idPersona = 13, razonSocial = "Cliente Test" },
            Sucursal = new Sucursal { IdSucursal = 1 },
            FechaVenta = new DateTime(2026, 8, 20, 10, 0, 0),
            Creado = new DateTime(2026, 8, 20, 10, 0, 0),
            NroRemito = "R-001",
            PagoMixtoEfectivo = pagoMixtoEfectivo,
            Observaciones = "",
        };

        private static (Negocio.Venta venta, FakeCierreCajaRepository cierreRepo) CrearVentaN()
        {
            var cierreRepo = new FakeCierreCajaRepository();
            var venta = new Negocio.Venta(
                new FakeVentaRepository(),
                new EmpresaContextFake(1),
                new FakeParametrosContext(),
                ctaCteN: new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1)),
                cierreCajaN: new Negocio.CierreCaja(cierreRepo, new EmpresaContextFake(1)));
            return (venta, cierreRepo);
        }

        [Fact]
        public void Efectivo_NoGeneraEgresoDeCaja()
        {
            var (venta, cierreRepo) = CrearVentaN();

            venta.agregarVenta(CrearVenta(Venta.FormaPagoEnum.Efectivo.ToString()));

            Assert.False(cierreRepo.AddOrEditEgresoCajaFueLlamado);
        }

        [Fact]
        public void CtaCte_GeneraEgresoInformativoSinSacarPlataDeLaCaja()
        {
            var (venta, cierreRepo) = CrearVentaN();

            venta.agregarVenta(CrearVenta(Venta.FormaPagoEnum.CtaCte.ToString()));

            Assert.True(cierreRepo.AddOrEditEgresoCajaFueLlamado);
            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.Equal(EgresoCaja.idCtaCte, egreso.IdTipoEgresoCaja);
            Assert.Contains("Cliente Test", egreso.Descripcion);
        }

        [Fact]
        public void Credito_SinPagoMixto_MontoEsElTotalDeLasLineas()
        {
            var (venta, cierreRepo) = CrearVentaN();

            // 1 linea: 10 Kg * $20 = $200, sin pago mixto en efectivo.
            venta.agregarVenta(CrearVenta(Venta.FormaPagoEnum.Credito.ToString()));

            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.Equal(EgresoCaja.idPagoTarjeta, egreso.IdTipoEgresoCaja);
            Assert.Equal(200, egreso.Monto);
        }

        [Fact]
        public void Credito_ConPagoMixto_DescuentaLoPagadoEnEfectivoDelMonto()
        {
            var (venta, cierreRepo) = CrearVentaN();

            // 1 linea: 10 Kg * $20 = $200 total, $50 pagados en efectivo -> $150 va a tarjeta.
            venta.agregarVenta(CrearVenta(Venta.FormaPagoEnum.Credito.ToString(), pagoMixtoEfectivo: 50));

            var egreso = cierreRepo.UltimoEgresoCajaRecibido;
            Assert.Equal(150, egreso.Monto);
            Assert.Contains("Mixta", egreso.Descripcion);
        }
    }
}
