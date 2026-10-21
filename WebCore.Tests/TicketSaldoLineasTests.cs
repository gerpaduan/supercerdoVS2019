// Tests del bloque de saldo del ticket de venta (WebCore.Services.TicketSaldoLineas): saldo anterior, total con saldo y,
// desde el cobro junto con la venta, "Su pago" y el saldo actual. Ver docs/DECISIONS.md (2026-10-19 "Saldo del cliente en
// el POS y en el ticket" y 2026-10-21 "Cobrar el saldo del cliente al finalizar la venta"). Los importes se comparan con el
// mismo formato "N2" que usa el ticket (depende de la cultura del proceso).
using WebCore.Services;

namespace WebCore.Tests
{
    public class TicketSaldoLineasTests
    {
        private static string N2(decimal importe) => importe.ToString("N2");

        private static TicketSaldoLineas.Cobro Cobro(string forma, decimal importe) => new TicketSaldoLineas.Cobro(forma, importe);

        // --- Sin cobros: igual que antes de esta funcionalidad ---

        [Fact]
        public void SinCobros_VentaACtaCte_MuestraSaldoAnteriorYTotalConSaldo()
        {
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 43, sumarVenta: true);

            Assert.Equal(3, lineas.Count);
            Assert.StartsWith("Saldo anterior (debe)", lineas[0].Texto);
            Assert.StartsWith("TOTAL c/saldo", lineas[2].Texto);
            Assert.EndsWith(N2(7000m), lineas[2].Texto);
            Assert.True(lineas[2].EsTotal);
        }

        [Fact]
        public void SinCobros_VentaQueNoEsCtaCte_SoloMuestraElSaldoAnterior()
        {
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 43, sumarVenta: false);

            Assert.Single(lineas);
            Assert.StartsWith("Saldo anterior (debe)", lineas[0].Texto);
        }

        // --- Con cobros ---

        [Fact]
        public void ConCobro_AgregaSuPagoYElSaldoActualEnNegrita()
        {
            // Debia 5.000, compro 2.000 a cta cte (total 7.000) y pago 3.000 en efectivo: queda debiendo 4.000.
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 43, true, new[] { Cobro("Efectivo", 3000m) });

            Assert.StartsWith("Saldo anterior (debe)", lineas[0].Texto);
            Assert.StartsWith("TOTAL c/saldo", lineas[2].Texto);
            Assert.False(lineas[2].EsTotal);

            Assert.StartsWith("Su pago Efectivo", lineas[3].Texto);
            Assert.EndsWith(N2(3000m), lineas[3].Texto);

            var ultima = lineas[^1];
            Assert.StartsWith("Saldo actual (debe)", ultima.Texto);
            Assert.EndsWith(N2(4000m), ultima.Texto);
            Assert.True(ultima.EsTotal);
            Assert.Single(lineas, l => l.EsTotal);
        }

        [Fact]
        public void ConCobro_QueSaldaLaCuenta_MuestraSaldoActualEnCeroSinDebeNiAFavor()
        {
            // Caso del pedido: tenia 389,19 a favor, compro 20.580,48 y paga lo que debe (20.191,29).
            var lineas = TicketSaldoLineas.Construir(20580.48m, 389.19m, 43, true, new[] { Cobro("Debito", 20191.29m) });

            var ultima = lineas[^1];
            Assert.StartsWith("Saldo actual", ultima.Texto);
            Assert.DoesNotContain("(debe)", ultima.Texto);
            Assert.DoesNotContain("(a favor)", ultima.Texto);
            Assert.EndsWith(N2(0m), ultima.Texto);
        }

        [Fact]
        public void ConCobro_MayorQueLaDeuda_QuedaAFavor()
        {
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 43, true, new[] { Cobro("Efectivo", 8000m) });

            var ultima = lineas[^1];
            Assert.StartsWith("Saldo actual (a favor)", ultima.Texto);
            Assert.EndsWith(N2(1000m), ultima.Texto);
        }

        [Fact]
        public void PagoMixto_MuestraUnRenglonPorCadaCobroYSumaAmbos()
        {
            var cobros = new[] { Cobro("Efectivo", 4000m), Cobro("Debito", 3000m) };
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 43, true, cobros);

            Assert.Contains(lineas, l => l.Texto.StartsWith("Su pago Efectivo") && l.Texto.EndsWith(N2(4000m)));
            Assert.Contains(lineas, l => l.Texto.StartsWith("Su pago Debito") && l.Texto.EndsWith(N2(3000m)));
            // 5.000 de deuda + 2.000 de venta - 7.000 cobrados = cuenta saldada.
            Assert.EndsWith(N2(0m), lineas[^1].Texto);
        }

        // --- Ancho del papel ---

        [Theory]
        [InlineData(32)]
        [InlineData(43)]
        public void NingunaLineaSePasaDelAncho(int ancho)
        {
            var cobros = new[] { Cobro("Transferencia", 1234567.89m), Cobro("Efectivo", 123456.78m) };
            var lineas = TicketSaldoLineas.Construir(1234567.89m, -1234567.89m, ancho, true, cobros);

            Assert.All(lineas, l => Assert.True(l.Texto.Length <= ancho, "Linea de " + l.Texto.Length + " caracteres en ancho " + ancho + ": " + l.Texto));
        }

        [Fact]
        public void En58mm_UsaEtiquetasCortas()
        {
            var lineas = TicketSaldoLineas.Construir(2000m, -5000m, 32, true, new[] { Cobro("Efectivo", 3000m) });

            Assert.StartsWith("Saldo ant.", lineas[0].Texto);
            Assert.Contains(lineas, l => l.Texto.StartsWith("Pago Efectivo"));
            Assert.StartsWith("Saldo act.", lineas[^1].Texto);
        }
    }
}
