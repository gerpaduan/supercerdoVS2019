// Tests del token que recuerda al operador de una pestana de POS mientras esta bloqueada (WebCore.Helpers.TokenOperadorPos).
// Ver docs/DECISIONS.md (2026-10-21, "Bloqueo por inactividad: recuperar al operador").
using Microsoft.AspNetCore.DataProtection;
using WebCore.Helpers;

namespace WebCore.Tests
{
    public class TokenOperadorPosTests
    {
        private static IDataProtectionProvider Proveedor() => new EphemeralDataProtectionProvider();

        [Fact]
        public void Token_VuelveAlMismoOperadorEnLaMismaPestana()
        {
            var proveedor = Proveedor();
            string token = TokenOperadorPos.Crear(proveedor, "abc123", 9102);

            Assert.NotEmpty(token);
            Assert.Equal(9102, TokenOperadorPos.Leer(proveedor, token, "abc123"));
        }

        [Fact]
        public void Token_DeOtraPestana_NoSirve()
        {
            var proveedor = Proveedor();
            string token = TokenOperadorPos.Crear(proveedor, "abc123", 9102);

            Assert.Equal(0, TokenOperadorPos.Leer(proveedor, token, "otraPestana"));
        }

        [Fact]
        public void Token_Alterado_NoSirve()
        {
            var proveedor = Proveedor();
            string token = TokenOperadorPos.Crear(proveedor, "abc123", 9102);
            string alterado = token.Substring(0, token.Length - 2) + (token.EndsWith("AA") ? "BB" : "AA");

            Assert.Equal(0, TokenOperadorPos.Leer(proveedor, alterado, "abc123"));
        }

        [Fact]
        public void Token_FirmadoPorOtrasClaves_NoSirve()
        {
            string token = TokenOperadorPos.Crear(Proveedor(), "abc123", 9102);

            Assert.Equal(0, TokenOperadorPos.Leer(Proveedor(), token, "abc123"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("no-es-un-token")]
        public void SinToken_NoHayOperador(string? token)
        {
            Assert.Equal(0, TokenOperadorPos.Leer(Proveedor(), token!, "abc123"));
        }

        [Fact]
        public void SinOperadorOSinPestana_NoSeCreaToken()
        {
            var proveedor = Proveedor();

            Assert.Equal("", TokenOperadorPos.Crear(proveedor, "abc123", 0));
            Assert.Equal("", TokenOperadorPos.Crear(proveedor, "", 9102));
        }
    }
}
