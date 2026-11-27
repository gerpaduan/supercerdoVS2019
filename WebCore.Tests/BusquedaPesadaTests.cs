// Tests de los limites del aviso de busqueda pesada (2026-11-26): lectura de la configuracion y cuando se advierte.
using WebCore.Helpers;
using Xunit;

namespace WebCore.Tests
{
    public class BusquedaPesadaTests
    {
        [Theory]
        [InlineData(null, 2000)]          // sin clave: default
        [InlineData("", 2000)]
        [InlineData("muchas", 2000)]      // invalido: default
        [InlineData(" 500 ", 500)]
        [InlineData("0", 0)]              // 0 apaga el aviso
        [InlineData("-10", 0)]
        [InlineData("99999999", 10000000)]
        public void Interpretar_RecortaYUsaElDefault(string? texto, int esperado)
        {
            Assert.Equal(esperado, BusquedaPesadaSettings.Interpretar(texto, 2000));
        }

        [Theory]
        [InlineData(2000, 2000, false)]   // el limite todavia no avisa
        [InlineData(2001, 2000, true)]
        [InlineData(0, 2000, false)]
        [InlineData(5000000, 0, false)]   // aviso apagado
        public void EsPesada_SoloSiSuperaUnLimiteEncendido(long cantidad, int limite, bool esperado)
        {
            Assert.Equal(esperado, BusquedaPesadaSettings.EsPesada(cantidad, limite));
        }
    }
}
