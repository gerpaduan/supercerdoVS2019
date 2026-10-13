using System.Globalization;
using Contratos;
using Xunit;

namespace NegocioTests
{
    // Cubre Contratos.CodigoProducto.SinCerosIzquierda (docs/DECISIONS.md 2026-10-12): corte.codigo es
    // bigint, asi que un EAN-13 con 0 inicial queda guardado de 12 digitos; las busquedas de texto
    // comparan tambien contra el valor sin ceros a la izquierda.
    public class CodigoProductoTests
    {
        [Theory]
        [InlineData("0123456789012", "123456789012")]
        [InlineData("123456789012", "123456789012")]
        [InlineData("0001", "1")]
        [InlineData("000", "0")]
        [InlineData("0", "0")]
        [InlineData("7791234567898", "7791234567898")]
        public void SoloDigitos_QuitaCerosIzquierdaDejandoAlMenosUno(string entrada, string esperado)
        {
            Assert.Equal(esperado, CodigoProducto.SinCerosIzquierda(entrada));
        }

        [Theory]
        [InlineData("0A1")]
        [InlineData("007G3")]
        [InlineData("00.5")]
        [InlineData("jamon")]
        public void ConCaracteresNoNumericos_DevuelveElMismoTexto(string entrada)
        {
            Assert.Equal(entrada, CodigoProducto.SinCerosIzquierda(entrada));
        }

        [Theory]
        [InlineData(null, "")]
        [InlineData("", "")]
        [InlineData("   ", "")]
        public void NuloOVacio_DevuelveVacio(string entrada, string esperado)
        {
            Assert.Equal(esperado, CodigoProducto.SinCerosIzquierda(entrada));
        }

        [Fact]
        public void EanConCeroInicial_Y_Su12Digitos_ResuelvenAlMismoLong()
        {
            // Es lo que garantiza que el alta (long) y la busqueda del POS (Convert.ToInt64) coincidan.
            long conCero = long.Parse("0123456789012", NumberStyles.Integer, CultureInfo.InvariantCulture);
            long sinCero = long.Parse("123456789012", NumberStyles.Integer, CultureInfo.InvariantCulture);

            Assert.Equal(sinCero, conCero);
            Assert.Equal(CodigoProducto.SinCerosIzquierda("0123456789012"), sinCero.ToString(CultureInfo.InvariantCulture));
        }
    }
}
