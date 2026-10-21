using WebCore.Helpers;
using Xunit;

namespace WebCore.Tests
{
    // Duracion de la Session del servidor (ver docs/DECISIONS.md "Autorizacion del operador: Session de 12 h").
    // El proceso de test no tiene la clave "Sesion:IdleTimeoutMinutos" en su config: debe aplicar el default.
    public class SesionServidorSettingsTests
    {
        [Fact]
        public void SinClaveEnConfig_DuraComoElLogin_12Horas()
        {
            Assert.Equal(System.TimeSpan.FromHours(12), SesionServidorSettings.IdleTimeout);
        }

        [Fact]
        public void EsMayorQueElDefaultDeAspNet_20Minutos()
        {
            Assert.True(SesionServidorSettings.IdleTimeout > System.TimeSpan.FromMinutes(20));
        }
    }
}
