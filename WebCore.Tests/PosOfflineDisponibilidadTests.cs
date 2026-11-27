// POS sin conexion (2026-11-26): sin la tabla posclaveunica la funcion queda apagada y no rompe las ventas.
namespace WebCore.Tests
{
    public class PosOfflineDisponibilidadTests
    {
        private static readonly DateTime T0 = new DateTime(2026, 11, 26, 12, 0, 0, DateTimeKind.Utc);

        [Fact]
        public void ConTabla_QuedaDisponibleYNoVuelveAConsultar()
        {
            var cache = new WebCore.Helpers.PosOfflineDisponibilidad.Cache();
            int consultas = 0;
            Assert.True(cache.TablaDisponible(() => { consultas++; return true; }, T0));
            Assert.True(cache.TablaDisponible(() => { consultas++; return false; }, T0.AddDays(1)));
            Assert.Equal(1, consultas);
        }

        [Fact]
        public void SinTabla_ApagadaYReintentaRecienPasadoElPlazo()
        {
            var cache = new WebCore.Helpers.PosOfflineDisponibilidad.Cache();
            int consultas = 0;
            Assert.False(cache.TablaDisponible(() => { consultas++; return false; }, T0));
            Assert.False(cache.TablaDisponible(() => { consultas++; return true; }, T0.AddMinutes(1)));
            Assert.Equal(1, consultas);
            Assert.True(cache.TablaDisponible(() => { consultas++; return true; },
                T0 + WebCore.Helpers.PosOfflineDisponibilidad.ReintentoSinTabla));
            Assert.Equal(2, consultas);
        }

        [Fact]
        public void FallaLaConsulta_QuedaApagadaSinTirarError()
        {
            var cache = new WebCore.Helpers.PosOfflineDisponibilidad.Cache();
            Assert.False(cache.TablaDisponible(() => throw new InvalidOperationException("base caida"), T0));
        }
    }
}
