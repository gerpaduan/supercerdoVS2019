// Tests de los textos y reglas del precio estimado de un pedido (Negocio.PedidoTextos): lo que el usuario lee junto al total.
using System;
using Negocio;
using Xunit;

namespace Negocio.Tests
{
    public class PedidoTextosTests
    {
        private static readonly DateTime Referencia = new DateTime(2026, 10, 19, 14, 30, 0);

        [Fact]
        public void AclaracionPrecio_DiceQueEsLaUltimaCompraSinTenerEnCuentaElProveedor()
        {
            Assert.Contains("última compra", PedidoTextos.AclaracionPrecio);
            Assert.Contains("sin tener en cuenta el proveedor", PedidoTextos.AclaracionPrecio);
        }

        [Theory]
        [InlineData(0, "Total estimado")]
        [InlineData(1, "Total estimado (parcial)")]
        [InlineData(7, "Total estimado (parcial)")]
        public void EtiquetaTotal_EsParcialSoloSiFaltanPrecios(int sinPrecio, string esperado)
        {
            Assert.Equal(esperado, PedidoTextos.EtiquetaTotal(sinPrecio));
        }

        [Theory]
        [InlineData(0, "")]
        [InlineData(1, "1 línea sin precio de compra: no suma al total.")]
        [InlineData(3, "3 líneas sin precio de compra: no suman al total.")]
        public void AvisoSinPrecio_ConcuerdaEnSingularYPlural(int sinPrecio, string esperado)
        {
            Assert.Equal(esperado, PedidoTextos.AvisoSinPrecio(sinPrecio));
        }

        [Fact]
        public void Aclaracion_SinFaltantesEsSoloLaAclaracionFija()
        {
            Assert.Equal(PedidoTextos.AclaracionPrecio, PedidoTextos.Aclaracion(0));
        }

        [Fact]
        public void Aclaracion_ConFaltantesAgregaCuantosNoSuman()
        {
            string texto = PedidoTextos.Aclaracion(2);

            Assert.StartsWith(PedidoTextos.AclaracionPrecio, texto);
            Assert.EndsWith("2 líneas sin precio de compra: no suman al total.", texto);
        }

        [Fact]
        public void AvisoPrecioViejo_NombraElLimite()
        {
            Assert.Equal("Los precios marcados son de hace más de 90 días.", PedidoTextos.AvisoPrecioViejo(90));
            Assert.Equal("Los precios marcados son de hace más de 1 día.", PedidoTextos.AvisoPrecioViejo(1));
        }

        [Theory]
        [InlineData("2026-10-19", 0)]
        [InlineData("2026-10-18", 1)]
        [InlineData("2026-07-21", 90)]
        public void DiasDelPrecio_CuentaDiasCalendarioContraLaReferencia(string fecha, int esperado)
        {
            Assert.Equal(esperado, PedidoTextos.DiasDelPrecio(DateTime.Parse(fecha), Referencia));
        }

        [Fact]
        public void DiasDelPrecio_IgnoraLaHoraYNoDaNegativos()
        {
            // Compra de hoy a las 23:59 con el pedido a las 00:01 del mismo dia: 0 dias, no -1.
            Assert.Equal(0, PedidoTextos.DiasDelPrecio(new DateTime(2026, 10, 19, 23, 59, 0), new DateTime(2026, 10, 19, 0, 1, 0)));
            // Una compra posterior a la referencia cuenta como 0.
            Assert.Equal(0, PedidoTextos.DiasDelPrecio(new DateTime(2026, 10, 25), Referencia));
        }

        [Fact]
        public void DiasDelPrecio_SinFechaEsNulo()
        {
            Assert.Null(PedidoTextos.DiasDelPrecio(null, Referencia));
        }

        [Fact]
        public void PrecioViejo_ElLimiteEsExclusivo()
        {
            // Justo 90 dias NO es viejo; 91 si.
            Assert.False(PedidoTextos.PrecioViejo(Referencia.Date.AddDays(-90), Referencia, 90));
            Assert.True(PedidoTextos.PrecioViejo(Referencia.Date.AddDays(-91), Referencia, 90));
        }

        [Fact]
        public void PrecioViejo_SinFechaNoSeMarca()
        {
            Assert.False(PedidoTextos.PrecioViejo(null, Referencia, 90));
        }

        [Theory]
        [InlineData("Cingolani", "2026-09-16", "Cingolani · 16/09/2026")]
        [InlineData("Cingolani", null, "Cingolani")]
        [InlineData("", "2026-09-16", "16/09/2026")]
        [InlineData("  ", null, "")]
        [InlineData(null, null, "")]
        public void DescribirOrigenPrecio_ArmaSoloLoQueSeSabe(string proveedor, string fecha, string esperado)
        {
            DateTime? fechaPrecio = fecha == null ? (DateTime?)null : DateTime.Parse(fecha);

            Assert.Equal(esperado, PedidoTextos.DescribirOrigenPrecio(proveedor, fechaPrecio));
        }
    }
}
