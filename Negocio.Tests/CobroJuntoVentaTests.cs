using Xunit;

namespace NegocioTests
{
    // Reglas del cobro que se registra junto con la venta en el modal de forma de pago (2026-10-21,
    // ver docs/DECISIONS.md "Cobrar el saldo del cliente al finalizar la venta") y del saldo que
    // queda tras la venta y el cobro. Convencion de signo: saldo negativo = el cliente debe.
    public class CobroJuntoVentaTests
    {
        // --- ImportePagaTodo ---

        [Fact]
        public void PagaTodo_ConDeuda_EsLaDeudaEnPositivo()
        {
            Assert.Equal(20191.29m, Negocio.CobroJuntoVenta.ImportePagaTodo(-20191.29m));
        }

        [Fact]
        public void PagaTodo_SinDeudaOConSaldoAFavor_EsCero()
        {
            Assert.Equal(0m, Negocio.CobroJuntoVenta.ImportePagaTodo(0m));
            Assert.Equal(0m, Negocio.CobroJuntoVenta.ImportePagaTodo(350m));
        }

        [Fact]
        public void PagaTodo_RedondeaACentavos()
        {
            Assert.Equal(100.13m, Negocio.CobroJuntoVenta.ImportePagaTodo(-100.125m));
        }

        // --- Partes: cobro simple ---

        [Theory]
        [InlineData("Efectivo", "Efectivo")]
        [InlineData("debito", "Debito")]
        [InlineData("CREDITO", "Credito")]
        [InlineData("Qr", "Qr")]
        [InlineData("Transferencia", "Transferencia")]
        public void CobroSimple_GuardaUnaParteConLaFormaElegida(string forma, string esperada)
        {
            var r = Negocio.CobroJuntoVenta.Partes(1500m, forma, esMixto: false, efectivo: 0m);

            Assert.True(r.Ok);
            var parte = Assert.Single(r.Partes);
            Assert.Equal(esperada, parte.FormaPago);
            Assert.Equal(1500m, parte.Importe);
        }

        [Theory]
        [InlineData("CtaCte")]
        [InlineData("Otro")]
        [InlineData("")]
        [InlineData(null)]
        public void CobroSimple_ConFormaNoPermitida_EsInvalido(string forma)
        {
            var r = Negocio.CobroJuntoVenta.Partes(1500m, forma, esMixto: false, efectivo: 0m);

            Assert.False(r.Ok);
            Assert.Empty(r.Partes);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-10)]
        public void CobroSimple_ConImporteNoPositivo_EsInvalido(int importe)
        {
            Assert.False(Negocio.CobroJuntoVenta.Partes(importe, "Efectivo", false, 0m).Ok);
        }

        // --- Partes: pago mixto = dos cobros ---

        [Fact]
        public void Mixto_GeneraUnCobroEnEfectivoYOtroEnElMedioElegido()
        {
            var r = Negocio.CobroJuntoVenta.Partes(10000m, "Debito", esMixto: true, efectivo: 4000m);

            Assert.True(r.Ok);
            Assert.Equal(2, r.Partes.Count);
            Assert.Equal("Efectivo", r.Partes[0].FormaPago);
            Assert.Equal(4000m, r.Partes[0].Importe);
            Assert.Equal("Debito", r.Partes[1].FormaPago);
            Assert.Equal(6000m, r.Partes[1].Importe);
        }

        [Fact]
        public void Mixto_ConOtroMedioEfectivo_EsInvalido()
        {
            Assert.False(Negocio.CobroJuntoVenta.Partes(10000m, "Efectivo", true, 4000m).Ok);
        }

        [Fact]
        public void Mixto_ConCtaCteComoOtroMedio_EsInvalido()
        {
            Assert.False(Negocio.CobroJuntoVenta.Partes(10000m, "CtaCte", true, 4000m).Ok);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10000)]
        [InlineData(12000)]
        public void Mixto_ConAlgunaParteSinImporte_EsInvalido(int efectivo)
        {
            Assert.False(Negocio.CobroJuntoVenta.Partes(10000m, "Debito", true, efectivo).Ok);
        }

        [Fact]
        public void Mixto_LaSumaDeLasPartesSiempreDaElImporte()
        {
            var r = Negocio.CobroJuntoVenta.Partes(100.01m, "Qr", true, 33.33m);

            Assert.True(r.Ok);
            Assert.Equal(100.01m, r.Partes[0].Importe + r.Partes[1].Importe);
        }

        // --- Saldo con cobros ---

        [Fact]
        public void SaldoPrevioConCobros_SacaLaVentaYElCobroDelSaldoActual()
        {
            // Debia 5.000; venta a cta cte de 2.000 (-> -7.000) y cobro de 3.000 (-> -4.000): el saldo previo es -5.000.
            decimal previo = Negocio.CalculoSaldoCliente.SaldoPrevioVentaConCobros(-4000m, ventaEnCtaCte: true, importeMovimientoVenta: 2000m, totalCobros: 3000m);

            Assert.Equal(-5000m, previo);
        }

        [Fact]
        public void SaldoTrasCobros_CobroTotal_DejaLaCuentaEnCero()
        {
            // Caso de la captura del usuario: tenia 389,19 a favor; venta de 20.580,48 a cta cte -> debe 20.191,29; paga todo.
            decimal saldoPrevio = 389.19m;
            decimal deuda = Negocio.CobroJuntoVenta.ImportePagaTodo(saldoPrevio - 20580.48m);

            decimal final = Negocio.CalculoSaldoCliente.SaldoTrasCobros(saldoPrevio, 20580.48m, sumarVenta: true, totalCobros: deuda);

            Assert.Equal(20191.29m, deuda);
            Assert.Equal(0m, final);
        }

        [Fact]
        public void SaldoTrasCobros_CobroParcial_QuedaDebiendoLaDiferencia()
        {
            decimal final = Negocio.CalculoSaldoCliente.SaldoTrasCobros(-5000m, 2000m, true, 3000m);

            Assert.Equal(-4000m, final);
        }

        [Fact]
        public void SaldoTrasCobros_CobroMayorQueLaDeuda_QuedaAFavor()
        {
            decimal final = Negocio.CalculoSaldoCliente.SaldoTrasCobros(-5000m, 2000m, true, 8000m);

            Assert.Equal(1000m, final);
        }

        [Fact]
        public void SaldoTrasCobros_ConVentaQueNoEsCtaCte_NoDescuentaLaVenta()
        {
            decimal final = Negocio.CalculoSaldoCliente.SaldoTrasCobros(-5000m, 2000m, sumarVenta: false, totalCobros: 1000m);

            Assert.Equal(-4000m, final);
        }
    }
}
