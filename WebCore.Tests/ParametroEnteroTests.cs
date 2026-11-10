// Tests del formato de codProdGenerico (2026-11-10): entero positivo de hasta 13 digitos, porque se compara con el
// codigo que se ingresa en el POS (corte.codigo es bigint). Vacio es valido (= sin producto de precio libre).
using WebCore.Models;
using Xunit;

namespace WebCore.Tests
{
    public class ParametroEnteroTests
    {
        private const int Max = ParametroAyudaCatalogo.EnteroMaxDigitos13;

        [Fact]
        public void Catalogo_CodProdGenerico_EsEnteroDe13Digitos()
        {
            var ayuda = ParametroAyudaCatalogo.Obtener("codProdGenerico", "");

            Assert.Equal(13, ayuda.EnteroMaxDigitos);
        }

        [Theory]
        [InlineData("999999", "999999")]
        [InlineData("  123 ", "123")]
        [InlineData("0012", "12")]
        [InlineData("7790000000010", "7790000000010")]   // 13 digitos
        [InlineData("9999999999999", "9999999999999")]   // 13 digitos, tope
        public void ValidarEntero_Valido_NormalizaSinCerosAIzquierda(string entrada, string esperado)
        {
            string error = ParametroAyudaCatalogo.ValidarEntero(entrada, Max, out string normalizado);

            Assert.Equal("", error);
            Assert.Equal(esperado, normalizado);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void ValidarEntero_Vacio_EsValido(string? entrada)
        {
            string error = ParametroAyudaCatalogo.ValidarEntero(entrada, Max, out string normalizado);

            Assert.Equal("", error);
            Assert.Equal("", normalizado);
        }

        [Theory]
        [InlineData("12345678901234")]   // 14 digitos
        [InlineData("12.5")]
        [InlineData("12,5")]
        [InlineData("-5")]
        [InlineData("+5")]
        [InlineData("abc")]
        [InlineData("12a")]
        [InlineData("1 2")]
        [InlineData("0")]
        [InlineData("0000")]
        [InlineData("١٢٣")]                // digitos arabigo-indicos: no son 0-9
        public void ValidarEntero_Invalido_DevuelveMensaje(string entrada)
        {
            string error = ParametroAyudaCatalogo.ValidarEntero(entrada, Max, out _);

            Assert.False(string.IsNullOrEmpty(error));
            Assert.Contains("13", error);
        }
    }
}
