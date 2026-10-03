using Negocio;
using Xunit;

namespace NegocioTests
{
    // Politica de clave segura y reglas del PIN (ver Negocio/PoliticaClave.cs). Sin BD ni web.
    public class PoliticaClaveTests
    {
        [Theory]
        [InlineData("abcdefg1!")]      // letra + numero + especial, minusculas (no se exige mayuscula)
        [InlineData("Clave2026#x")]
        [InlineData("n0-espacios")]
        [InlineData("12345a6b!")]
        public void ValidarClave_AceptaClavesQueCumplenLaRegla(string clave)
        {
            Assert.Null(PoliticaClave.ValidarClave(clave));
        }

        [Theory]
        [InlineData("")]               // vacia
        [InlineData("a1!")]            // corta
        [InlineData("abcdefg1")]       // sin caracter especial
        [InlineData("abcdefgh!")]      // sin numero
        [InlineData("12345678!")]      // sin letra
        [InlineData("abc defg1!")]     // con espacio
        [InlineData("Password1!")]     // clave comun
        public void ValidarClave_RechazaClavesQueNoCumplen(string clave)
        {
            Assert.NotNull(PoliticaClave.ValidarClave(clave));
        }

        [Fact]
        public void ValidarClave_RechazaClaveIgualAlUsuario()
        {
            // Cumple la composicion pero es igual al nombre de acceso (sin distinguir mayusculas).
            Assert.NotNull(PoliticaClave.ValidarClave("juan.perez1!", "JUAN.PEREZ1!"));
            Assert.Null(PoliticaClave.ValidarClave("juan.perez1!", "jperez"));
        }

        [Fact]
        public void ValidarClave_RechazaLargoExcesivo()
        {
            string larga = new string('a', PoliticaClave.LargoMaximoClave) + "1!";
            Assert.NotNull(PoliticaClave.ValidarClave(larga));
        }

        [Theory]
        [InlineData("4829")]
        [InlineData("739150")]
        [InlineData("0731")]
        public void ValidarPin_AceptaPinesValidos(string pin)
        {
            Assert.Null(PoliticaClave.ValidarPin(pin, 26));
        }

        [Theory]
        [InlineData("")]
        [InlineData("123")]            // corto
        [InlineData("1234567")]        // largo
        [InlineData("12a4")]           // no numerico
        [InlineData("1111")]           // repetido
        [InlineData("1234")]           // secuencia ascendente
        [InlineData("9876")]           // secuencia descendente
        [InlineData("234567")]         // secuencia larga
        public void ValidarPin_RechazaPinesDebiles(string pin)
        {
            Assert.NotNull(PoliticaClave.ValidarPin(pin, 26));
        }

        [Fact]
        public void ValidarPin_RechazaPinIgualAlIdDelUsuario()
        {
            Assert.NotNull(PoliticaClave.ValidarPin("2600", 2600));
            Assert.Null(PoliticaClave.ValidarPin("2600", 26));
        }

        [Theory]
        [InlineData("4829", true)]
        [InlineData("739150", true)]
        [InlineData("123", false)]
        [InlineData("1234567", false)]
        [InlineData("12a4", false)]
        [InlineData("", false)]
        public void TieneFormaDePin_SoloDigitosDe4a6(string texto, bool esperado)
        {
            Assert.Equal(esperado, PoliticaClave.TieneFormaDePin(texto));
        }
    }
}
