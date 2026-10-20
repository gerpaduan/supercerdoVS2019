using System;
using Entidades;
using NegocioTests.Fakes;
using Xunit;

namespace NegocioTests
{
    // Reglas del saldo del cliente en el POS y en el ticket (2026-10-19, ver docs/DECISIONS.md
    // "Saldo del cliente en el POS y en el ticket"). Convencion de signo: saldo negativo = el
    // cliente debe; positivo = a favor.
    public class CalculoSaldoClienteTests
    {
        // --- SaldoPrevioVenta: no contar dos veces la venta a cuenta corriente ---

        [Fact]
        public void VentaPagadaDeOtraForma_ElSaldoActualYaEsElPrevio()
        {
            // El cliente debia 5.000; la venta de 2.000 se pago en efectivo (no toca la cuenta).
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVenta(-5000m, ventaEnCtaCte: false, importeMovimientoVenta: 2000m);

            Assert.Equal(-5000m, previo);
        }

        [Fact]
        public void VentaACtaCte_SeSacaLaVentaDelSaldoActual()
        {
            // Debia 5.000, la venta de 2.000 fue a cuenta corriente: el saldo leido despues es -7.000.
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVenta(-7000m, ventaEnCtaCte: true, importeMovimientoVenta: 2000m);

            Assert.Equal(-5000m, previo);
        }

        [Fact]
        public void VentaACtaCte_DeUnClienteSinDeudaPrevia_DaSaldoPrevioCero()
        {
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVenta(-2000m, ventaEnCtaCte: true, importeMovimientoVenta: 2000m);

            Assert.Equal(0m, previo);
        }

        // --- TotalConSaldo: venta + lo que debia ---

        [Fact]
        public void ClienteQueDebe_SeSumaLaDeudaALaVenta()
        {
            Assert.Equal(7000m, Negocio.CalculoSaldoCliente.TotalConSaldo(2000m, -5000m));
        }

        [Fact]
        public void ClienteConSaldoAFavor_SeRestaDeLaVenta()
        {
            Assert.Equal(1000m, Negocio.CalculoSaldoCliente.TotalConSaldo(2000m, 1000m));
        }

        [Fact]
        public void SaldoAFavorMayorQueLaVenta_DaTotalNegativo()
        {
            // El ticket lo rotula "a favor" con el valor absoluto.
            Assert.Equal(-300m, Negocio.CalculoSaldoCliente.TotalConSaldo(2000m, 2300m));
        }

        [Fact]
        public void VentaACtaCte_TotalConSaldoEsLaDeudaActualDelCliente()
        {
            // Sin doble conteo: el total impreso coincide con lo que realmente debe despues de la venta.
            decimal saldoActual = -7000m;
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVenta(saldoActual, true, 2000m);

            Assert.Equal(-saldoActual, Negocio.CalculoSaldoCliente.TotalConSaldo(2000m, previo));
        }

        // --- SaldoSuperaUmbral: los montos chicos no se informan ---

        [Theory]
        [InlineData(-50, 100, false)]      // deuda chica
        [InlineData(-100, 100, false)]     // justo en el umbral: no se informa
        [InlineData(-100.01, 100, true)]   // apenas por encima
        [InlineData(-5000, 100, true)]
        [InlineData(50, 100, false)]       // saldo a favor chico: simetrico
        [InlineData(100.01, 100, true)]
        [InlineData(0, 100, false)]
        public void SaldoSuperaUmbral_ComparaElValorAbsoluto(double saldo, double umbral, bool esperado)
        {
            Assert.Equal(esperado, Negocio.CalculoSaldoCliente.SaldoSuperaUmbral((decimal)saldo, (decimal)umbral));
        }

        [Fact]
        public void UmbralCero_InformaCualquierSaldoDistintoDeCero()
        {
            Assert.True(Negocio.CalculoSaldoCliente.SaldoSuperaUmbral(-0.01m, 0m));
            Assert.False(Negocio.CalculoSaldoCliente.SaldoSuperaUmbral(0m, 0m));
        }

        [Fact]
        public void UmbralNegativo_SeTrataComoCero()
        {
            Assert.True(Negocio.CalculoSaldoCliente.SaldoSuperaUmbral(-1m, -500m));
            Assert.False(Negocio.CalculoSaldoCliente.SaldoSuperaUmbral(0m, -500m));
        }

        [Fact]
        public void RuidoDeFloatPorDebajoDelCentavo_NoCuentaComoSaldo()
        {
            // SUM(importe) de columnas float puede dejar 1e-9 de basura: a centavos es 0.
            Assert.False(Negocio.CalculoSaldoCliente.SaldoSuperaUmbral(0.000000001m, 0m));
        }

        // --- Integracion con el repositorio: el saldo de una venta a cta cte no se cuenta dos veces ---

        [Fact]
        public void ObtenerSaldoPersona_SinMovimientos_DevuelveNull()
        {
            var ctaCte = new Negocio.CuentaCorriente(new FakeCuentaCorrienteRepository(), new EmpresaContextFake(1));

            Assert.Null(ctaCte.obtenerSaldoPersona(13));
        }

        [Fact]
        public void VentaACtaCte_SaldoLeidoDespuesYSaldoPrevio_SonCoherentes()
        {
            var persona = new Persona { idPersona = 13 };
            var repo = new FakeCuentaCorrienteRepository();
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));

            // Deuda previa de 5.000 (otra venta a cta cte) y luego una venta de 2.000 tambien a cta cte,
            // generadas igual que Negocio.Venta.crearMovCtaCteVenta (Debito, enCtaCte = true).
            CrearVentaACtaCte(ctaCte, persona, idVenta: 1, importe: 5000f);
            CrearVentaACtaCte(ctaCte, persona, idVenta: 2, importe: 2000f);

            decimal saldoActual = ctaCte.obtenerSaldoPersona(13).Value;
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVenta(saldoActual, ventaEnCtaCte: true, importeMovimientoVenta: 2000m);

            Assert.Equal(-7000m, saldoActual);
            Assert.Equal(-5000m, previo);
            Assert.Equal(7000m, Negocio.CalculoSaldoCliente.TotalConSaldo(2000m, previo));
        }

        [Fact]
        public void ObtenerSaldoPersona_NoMezclaPersonas()
        {
            var repo = new FakeCuentaCorrienteRepository();
            var ctaCte = new Negocio.CuentaCorriente(repo, new EmpresaContextFake(1));
            CrearVentaACtaCte(ctaCte, new Persona { idPersona = 13 }, idVenta: 1, importe: 300f);
            CrearVentaACtaCte(ctaCte, new Persona { idPersona = 14 }, idVenta: 2, importe: 900f);

            Assert.Equal(-300m, ctaCte.obtenerSaldoPersona(13));
            Assert.Equal(-900m, ctaCte.obtenerSaldoPersona(14));
        }

        private static void CrearVentaACtaCte(Negocio.CuentaCorriente ctaCte, Persona persona, int idVenta, float importe) =>
            ctaCte.crearMovCtaCte(persona, new DateTime(2026, 10, 19, 10, 0, 0), MovCtaCte.tablas.Ventas, idVenta,
                nroDoc: "R-" + idVenta, detalle: "", tipoMov: MovCtaCte.tipoMov.Debito, importe: importe,
                oSucursalE: new Sucursal { IdSucursal = 1 }, creado: new DateTime(2026, 10, 19, 10, 0, 0),
                creadoPor: new Usuario { Id = 2 }, actualizado: null, actualizadoPor: null,
                crearMovCtaCte: true, oCierreCajaE: null, oPagoE: null, oPagoAnterior: null);
    }
}
