using WebCore.Helpers;
using Xunit;

namespace WebCore.Tests
{
    // Importes de Cerrar Caja (Caja Cierre / Importe Retirado): un valor que no es numero no se guarda como 0.
    public class ImporteCierreParserTests
    {
        [Theory]
        [InlineData("1234.50", 1234.5f)]
        [InlineData("0.00", 0f)]
        [InlineData("0", 0f)]
        [InlineData("  250.25 ", 250.25f)]
        [InlineData("-10.00", -10f)]
        [InlineData("1234,5", 1234.5f)]
        public void NumeroValido_SeLee(string texto, float esperado)
        {
            Assert.True(ImporteCierreParser.TryParse(texto, out float importe));
            Assert.Equal(esperado, importe);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("abc")]
        [InlineData("12abc")]
        [InlineData("-")]
        [InlineData(".")]
        [InlineData("1.234,56")]
        [InlineData("NaN")]
        [InlineData("Infinity")]
        [InlineData("-Infinity")]
        [InlineData("1e5")]
        public void NoNumerico_SeRechaza(string texto)
        {
            Assert.False(ImporteCierreParser.TryParse(texto, out float importe));
            Assert.Equal(0f, importe);
        }
    }
}
