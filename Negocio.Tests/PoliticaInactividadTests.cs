using Xunit;

namespace NegocioTests
{
    // Regla del bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"):
    // 0 = nunca; produccion y dispositivos "No bloquear sesion" quedan exentos sea cual sea el valor.
    public class PoliticaInactividadTests
    {
        [Fact]
        public void UsuarioNormal_DevuelveSusMinutos()
        {
            Assert.Equal(60, Negocio.PoliticaInactividad.MinutosEfectivos(false, 60, false));
        }

        [Fact]
        public void CeroMinutos_NuncaBloquea()
        {
            Assert.Equal(0, Negocio.PoliticaInactividad.MinutosEfectivos(false, 0, false));
        }

        [Fact]
        public void UsuarioDeProduccion_EstaExentoAunqueTengaMinutos()
        {
            Assert.Equal(0, Negocio.PoliticaInactividad.MinutosEfectivos(true, 60, false));
        }

        [Fact]
        public void DispositivoSinBloqueo_EstaExentoAunqueTengaMinutos()
        {
            Assert.Equal(0, Negocio.PoliticaInactividad.MinutosEfectivos(false, 15, true));
        }

        [Fact]
        public void MinutosNegativos_SeTratanComoCero()
        {
            // Dato corrupto: ante la duda no se bloquea a nadie por error.
            Assert.Equal(0, Negocio.PoliticaInactividad.MinutosEfectivos(false, -5, false));
        }

        [Fact]
        public void UsuarioNuevo_ArrancaConElDefaultDe60Minutos()
        {
            Assert.Equal(60, new Entidades.Usuario().MinutosInactividad);
            Assert.Equal(Entidades.Usuario.MinutosInactividadPorDefecto, new Entidades.Usuario().MinutosInactividad);
        }
    }
}
