// Tests de Helpers/MovimientoTiempos (2026-11-11): llegada estimada de un movimiento nuevo (hora de guardado + tiempo de envio) y hora de
// carga de cada linea (ver docs/DECISIONS.md "Movimientos: tiempo de envio, llegada y hora de carga por linea").
using WebCore.Helpers;
using Xunit;

namespace WebCore.Tests
{
    public class MovimientoTiemposTests
    {
        private static readonly DateTime Ahora = new DateTime(2026, 11, 11, 15, 0, 0);

        [Theory]
        [InlineData(-5, 0)]
        [InlineData(0, 0)]
        [InlineData(45, 45)]
        [InlineData(1440, 1440)]
        [InlineData(1441, 1440)]
        [InlineData(int.MaxValue, 1440)]
        public void LimitarMinutos_AcotaEntreCeroYUnDia(int minutos, int esperado)
        {
            Assert.Equal(esperado, MovimientoTiempos.LimitarMinutos(minutos));
        }

        [Fact]
        public void CalcularLlegada_SumaLosMinutosALaHoraDeGuardado()
        {
            Assert.Equal(new DateTime(2026, 11, 11, 15, 45, 0), MovimientoTiempos.CalcularLlegada(Ahora, 45));
        }

        [Fact]
        public void CalcularLlegada_SinMinutos_EsLaHoraDeGuardado()
        {
            Assert.Equal(Ahora, MovimientoTiempos.CalcularLlegada(Ahora, 0));
        }

        [Fact]
        public void CalcularLlegada_PuedeCruzarLaMedianoche()
        {
            var tarde = new DateTime(2026, 11, 11, 23, 50, 0);

            Assert.Equal(new DateTime(2026, 11, 12, 0, 20, 0), MovimientoTiempos.CalcularLlegada(tarde, 30));
        }

        [Fact]
        public void CalcularLlegada_MinutosNegativosONoRazonables_SeAcotan()
        {
            Assert.Equal(Ahora, MovimientoTiempos.CalcularLlegada(Ahora, -30));
            Assert.Equal(Ahora.AddDays(1), MovimientoTiempos.CalcularLlegada(Ahora, 99999));
        }

        [Fact]
        public void LeerCreadoLinea_LeeElFormatoDeLaPantalla()
        {
            Assert.Equal(new DateTime(2026, 11, 11, 14, 58, 0), MovimientoTiempos.LeerCreadoLinea("11/11/2026 14:58", Ahora));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("ayer a la tarde")]
        [InlineData("2026-11-11 14:58")]
        public void LeerCreadoLinea_TextoVacioOInvalido_DevuelveNull(string? texto)
        {
            Assert.Null(MovimientoTiempos.LeerCreadoLinea(texto, Ahora));
        }

        [Fact]
        public void LeerCreadoLinea_RelojAdelantado_SeCorrigeAAhora()
        {
            // La PC del operador tenia el reloj 10 minutos adelantado: una linea no puede haberse cargado despues de guardarse.
            Assert.Equal(Ahora, MovimientoTiempos.LeerCreadoLinea("11/11/2026 15:10", Ahora));
        }

        [Fact]
        public void ResolverCreadoLinea_LineaNuevaConHora_UsaLaDeLaPantalla()
        {
            Assert.Equal(new DateTime(2026, 11, 11, 14, 58, 0), MovimientoTiempos.ResolverCreadoLinea("11/11/2026 14:58", 0, Ahora));
        }

        [Fact]
        public void ResolverCreadoLinea_LineaNuevaSinHora_UsaAhora()
        {
            Assert.Equal(Ahora, MovimientoTiempos.ResolverCreadoLinea("", 0, Ahora));
            Assert.Equal(Ahora, MovimientoTiempos.ResolverCreadoLinea(null, 0, Ahora));
        }

        [Fact]
        public void ResolverCreadoLinea_LineaExistenteConHora_LaConserva()
        {
            Assert.Equal(new DateTime(2026, 11, 10, 9, 30, 0), MovimientoTiempos.ResolverCreadoLinea("10/11/2026 09:30", 123, Ahora));
        }

        [Fact]
        public void ResolverCreadoLinea_LineaExistenteSinHora_QuedaSinHora()
        {
            // Movimiento anterior al cambio: al editarlo no se le inventa la hora de la edicion.
            Assert.Null(MovimientoTiempos.ResolverCreadoLinea("", 123, Ahora));
        }

        [Fact]
        public void CalcularInicio_EsLaHoraDeLaPrimeraLineaCargada()
        {
            var horas = new DateTime?[]
            {
                new DateTime(2026, 11, 11, 15, 2, 0),
                new DateTime(2026, 11, 11, 14, 58, 0),
                null,
                new DateTime(2026, 11, 11, 15, 5, 0)
            };

            Assert.Equal(new DateTime(2026, 11, 11, 14, 58, 0), MovimientoTiempos.CalcularInicio(horas));
        }

        [Fact]
        public void CalcularInicio_SinHorasONulo_DevuelveNull()
        {
            Assert.Null(MovimientoTiempos.CalcularInicio(new DateTime?[] { null, null }));
            Assert.Null(MovimientoTiempos.CalcularInicio(Array.Empty<DateTime?>()));
            Assert.Null(MovimientoTiempos.CalcularInicio(null!));
        }
    }
}
