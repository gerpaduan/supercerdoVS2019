using System;
using System.Collections.Generic;
using System.Linq;
using Entidades;
using Xunit;

namespace NegocioTests
{
    // Compras: titular propio y comprobantes del proveedor (2026-11-09, ver docs/DECISIONS.md): clasificacion de los pagos en
    // bancarizado / efectivo y resumen de la cuenta corriente de un proveedor por titular propio (deuda + control fiscal).
    public class ResumenCtaCtePorTitularTests
    {
        private const int Titular1 = 11;
        private const int Titular2 = 12;
        private static readonly DateTime Desde = new DateTime(2026, 10, 1);

        private static readonly Dictionary<int, string> Nombres = new Dictionary<int, string>
        {
            { Titular1, "Titular 1" }, { Titular2, "Titular 2" }
        };

        // ------------------------------------------------------------------ clasificacion de medios de pago

        [Theory]
        [InlineData("Efectivo", 1000, 0, 0, 1000)]
        [InlineData("Otro", 1000, 0, 0, 1000)]
        [InlineData("Transferencia", 1000, 0, 1000, 0)]
        [InlineData("Trasferencia", 1000, 0, 1000, 0)]      // variante mal escrita de los datos historicos
        [InlineData("Debito", 1000, 0, 1000, 0)]
        [InlineData("Credito", 1000, 0, 1000, 0)]
        [InlineData("Qr", 1000, 0, 1000, 0)]
        [InlineData("Cheque", 1000, 0, 1000, 0)]
        [InlineData("EftvoCheque", 1000, 300, 700, 300)]
        [InlineData("Eftvo+Cheque", 1000, 300, 700, 300)]  // variante historica
        [InlineData("Efectivo + Cheques", 1000, 300, 700, 300)]
        [InlineData("Desconocido", 1000, 0, 0, 1000)]      // lo que no se reconoce no se asume bancarizado
        [InlineData(null, 1000, 0, 0, 1000)]
        public void Clasificar_SegunFormaDePago(string forma, decimal importe, decimal efectivo, decimal bancarizadoEsperado, decimal efectivoEsperado)
        {
            decimal bancarizado, enEfectivo;
            Negocio.ClasificacionMedioPago.Clasificar(forma, importe, efectivo, out bancarizado, out enEfectivo);

            Assert.Equal(bancarizadoEsperado, bancarizado);
            Assert.Equal(efectivoEsperado, enEfectivo);
            Assert.Equal(importe, bancarizado + enEfectivo);
        }

        [Fact]
        public void Clasificar_MixtoConEfectivoMayorAlImporte_NoDaBancarizadoNegativo()
        {
            decimal bancarizado, enEfectivo;
            Negocio.ClasificacionMedioPago.Clasificar("EftvoCheque", 1000m, 5000m, out bancarizado, out enEfectivo);
            Assert.Equal(0m, bancarizado);
            Assert.Equal(1000m, enEfectivo);
        }

        // ------------------------------------------------------------------ resumen por titular

        private static MovimientoResumen Compra(int titular, decimal importe, string fecha)
        { return new MovimientoResumen { Tabla = "Compras", Importe = importe, IdTitular = titular, Fecha = DateTime.Parse(fecha) }; }

        private static MovimientoResumen Pago(int titular, decimal importe, string fecha)
        { return new MovimientoResumen { Tabla = "Pagos", Importe = -importe, IdTitular = titular, Fecha = DateTime.Parse(fecha) }; }

        private static PagoResumen PagoDetalle(int titular, string forma, decimal importe, string fecha, decimal efectivo = 0m)
        { return new PagoResumen { FormaPago = forma, Importe = importe, Efectivo = efectivo, IdTitular = titular, Fecha = DateTime.Parse(fecha) }; }

        private static FacturadoResumen Facturado(int titular, decimal totalConSigno, string fechaCompra)
        { return new FacturadoResumen { IdTitular = titular, TotalConSigno = totalConSigno, FechaCompra = DateTime.Parse(fechaCompra) }; }

        [Fact]
        public void Armar_PagoCruzado_LosSubSaldosNoCierranEnCeroPeroElTotalEsLaDeudaReal()
        {
            // Se compra 1.000 con el titular 1 y 500 con el titular 2, pero se paga 1.200 con el titular 2.
            var movimientos = new[]
            {
                Compra(Titular1, 1000m, "2026-10-05"), Compra(Titular2, 500m, "2026-10-06"), Pago(Titular2, 1200m, "2026-10-20")
            };
            var pagos = new[] { PagoDetalle(Titular2, "Transferencia", 1200m, "2026-10-20") };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, pagos, new FacturadoResumen[0], Desde, Nombres, 0);

            var t1 = r.Filas.Single(f => f.IdTitular == Titular1);
            var t2 = r.Filas.Single(f => f.IdTitular == Titular2);
            Assert.Equal(1000m, t1.Saldo);     // debe 1.000 solo por el titular 1
            Assert.Equal(-700m, t2.Saldo);     // pago de mas con el titular 2: queda a favor
            Assert.Equal(300m, r.Total.Saldo); // la deuda real del proveedor: 1.500 - 1.200
            Assert.Equal(1500m, r.Total.Comprado);
            Assert.Equal(1200m, r.Total.Pagado);
            Assert.Equal("Titular 1", t1.NombreTitular);
        }

        [Fact]
        public void Armar_LoAnteriorAlPeriodoEntraSoloComoSaldoAnterior()
        {
            var movimientos = new[]
            {
                Compra(Titular1, 400m, "2026-09-15"),    // antes del periodo
                Compra(Titular1, 100m, "2026-10-10")
            };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, new PagoResumen[0], new FacturadoResumen[0], Desde, Nombres, 0);

            var f = r.Filas.Single();
            Assert.Equal(400m, f.SaldoAnterior);
            Assert.Equal(100m, f.Comprado);
            Assert.Equal(500m, f.Saldo);
        }

        [Fact]
        public void Armar_OtrosMovimientosAfectanElSaldoPeroNoElComprado()
        {
            var movimientos = new[]
            {
                Compra(Titular1, 1000m, "2026-10-05"),
                new MovimientoResumen { Tabla = "MovCtaCte", Importe = -50m, IdTitular = Titular1, Fecha = new DateTime(2026, 10, 8) }
            };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, new PagoResumen[0], new FacturadoResumen[0], Desde, Nombres, 0);

            Assert.Equal(1000m, r.Total.Comprado);
            Assert.Equal(-50m, r.Total.Otros);
            Assert.Equal(950m, r.Total.Saldo);
        }

        [Fact]
        public void Armar_ControlFiscal_FacturaMasNotaDeCredito_RestaDelFacturado()
        {
            var facturado = new[]
            {
                Facturado(Titular1, 1210m, "2026-10-05"),    // factura
                Facturado(Titular1, -121m, "2026-10-05"),    // nota de credito
                Facturado(Titular1, 999m, "2026-09-01")      // compra de antes del periodo: no entra
            };
            var movimientos = new[] { Compra(Titular1, 1500m, "2026-10-05") };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, new PagoResumen[0], facturado, Desde, Nombres, 0);

            var f = r.Filas.Single();
            Assert.Equal(1089m, f.FacturadoNeto);
            Assert.Equal(411m, f.SinFactura);   // 1.500 comprado - 1.089 facturado
        }

        [Fact]
        public void Armar_PagadoBancarizadoMayorAlFacturado_QuedaMarcado_YElEfectivoNo()
        {
            var movimientos = new[]
            {
                Compra(Titular1, 2000m, "2026-10-05"), Pago(Titular1, 1500m, "2026-10-12"), Pago(Titular1, 300m, "2026-10-13")
            };
            var pagos = new[]
            {
                PagoDetalle(Titular1, "Transferencia", 1500m, "2026-10-12"),
                PagoDetalle(Titular1, "Efectivo", 300m, "2026-10-13")
            };
            var facturado = new[] { Facturado(Titular1, 1000m, "2026-10-05") };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, pagos, facturado, Desde, Nombres, 0);

            var f = r.Filas.Single();
            Assert.Equal(1500m, f.PagadoBancarizado);
            Assert.Equal(300m, f.PagadoEfectivo);
            Assert.Equal(-500m, f.DiferenciaFacturadoBancarizado);   // facturado 1.000 - bancarizado 1.500
            Assert.True(f.PagadoBancarizadoExcedeFacturado);
            Assert.True(r.Total.PagadoBancarizadoExcedeFacturado);
        }

        [Fact]
        public void Armar_BancarizadoMenorAlFacturado_NoSeMarca()
        {
            var pagos = new[] { PagoDetalle(Titular1, "Transferencia", 500m, "2026-10-12") };
            var facturado = new[] { Facturado(Titular1, 1000m, "2026-10-05") };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(new MovimientoResumen[0], pagos, facturado, Desde, Nombres, 0);

            Assert.False(r.Filas.Single().PagadoBancarizadoExcedeFacturado);
            Assert.Equal(500m, r.Filas.Single().DiferenciaFacturadoBancarizado);
        }

        [Fact]
        public void Armar_FiltroDeTitular_DejaSoloEseTitularYSuTotal()
        {
            var movimientos = new[] { Compra(Titular1, 1000m, "2026-10-05"), Compra(Titular2, 500m, "2026-10-06") };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, new PagoResumen[0], new FacturadoResumen[0], Desde, Nombres, Titular2);

            Assert.Single(r.Filas);
            Assert.Equal(Titular2, r.Filas[0].IdTitular);
            Assert.Equal(500m, r.Total.Comprado);
        }

        [Fact]
        public void Armar_SinDatos_DaTotalEnCero()
        {
            var r = Negocio.ResumenCtaCtePorTitular.Armar(null, null, null, Desde, null, 0);
            Assert.Empty(r.Filas);
            Assert.Equal(0m, r.Total.Saldo);
            Assert.False(r.Total.PagadoBancarizadoExcedeFacturado);
        }

        [Fact]
        public void Armar_PagadoDeLaDeudaCoincideConBancarizadoMasEfectivo()
        {
            // Mismos pagos vistos por la cuenta corriente (movimientos) y por el detalle de pagos (clasificados).
            var movimientos = new[]
            {
                Pago(Titular1, 1000m, "2026-10-12"), Pago(Titular1, 600m, "2026-10-13"), Pago(Titular1, 400m, "2026-10-14")
            };
            var pagos = new[]
            {
                PagoDetalle(Titular1, "Transferencia", 1000m, "2026-10-12"),
                PagoDetalle(Titular1, "EftvoCheque", 600m, "2026-10-13", efectivo: 100m),
                PagoDetalle(Titular1, "Efectivo", 400m, "2026-10-14")
            };

            var r = Negocio.ResumenCtaCtePorTitular.Armar(movimientos, pagos, new FacturadoResumen[0], Desde, Nombres, 0);

            Assert.Equal(2000m, r.Total.Pagado);
            Assert.Equal(r.Total.Pagado, r.Total.PagadoBancarizado + r.Total.PagadoEfectivo);
        }
    }
}
