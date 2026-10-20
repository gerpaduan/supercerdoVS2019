using System;
using Negocio;
using Xunit;

namespace NegocioTests
{
    // Cubre el limitador de actualizaciones automaticas de Existencia por sucursales (Negocio.LimitadorCalculo,
    // docs/DECISIONS.md 2026-10-19): concurrencia, latencia, sonda de recuperacion y que el Buscar manual nunca se rechace.
    public class LimitadorCalculoTests
    {
        private static readonly TimeSpan Umbral = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan Reintento = TimeSpan.FromSeconds(120);

        private class Reloj
        {
            public DateTime Ahora = new DateTime(2026, 10, 19, 10, 0, 0, DateTimeKind.Utc);
            public Func<DateTime> Func { get { return () => Ahora; } }
        }

        [Fact]
        public void Automatica_EntraSiNoHayNadaEnCurso()
        {
            var limitador = new LimitadorCalculo(new Reloj().Func);

            bool entro = limitador.TryEntrar(true, 2, Umbral, Reintento, out int seg);

            Assert.True(entro);
            Assert.Equal(0, seg);
            Assert.Equal(1, limitador.EnCurso);
        }

        [Fact]
        public void Automatica_SeRechazaConLosCalculosMaximosEnCurso_ConElTiempoDeReintento()
        {
            var limitador = new LimitadorCalculo(new Reloj().Func);
            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));
            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));

            bool entro = limitador.TryEntrar(true, 2, Umbral, Reintento, out int seg);

            Assert.False(entro);
            Assert.Equal(120, seg);
            Assert.Equal(2, limitador.EnCurso);
        }

        [Fact]
        public void Manual_NuncaSeRechaza_AunqueHayaMaximoEnCurso_PeroCuenta()
        {
            var limitador = new LimitadorCalculo(new Reloj().Func);
            Assert.True(limitador.TryEntrar(true, 1, Umbral, Reintento, out _));

            bool entro = limitador.TryEntrar(false, 1, Umbral, Reintento, out _);

            Assert.True(entro);
            Assert.Equal(2, limitador.EnCurso);
        }

        [Fact]
        public void Salir_LiberaElLugar()
        {
            var limitador = new LimitadorCalculo(new Reloj().Func);
            Assert.True(limitador.TryEntrar(true, 1, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(1));

            Assert.True(limitador.TryEntrar(true, 1, Umbral, Reintento, out _));
        }

        [Fact]
        public void Automatica_SeRechazaSiElUltimoTardoMasQueElUmbral_HastaQuePaseLaVentana()
        {
            var reloj = new Reloj();
            var limitador = new LimitadorCalculo(reloj.Func);
            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(8));

            reloj.Ahora = reloj.Ahora.AddSeconds(30);
            bool entro = limitador.TryEntrar(true, 2, Umbral, Reintento, out int seg);

            Assert.False(entro);
            Assert.Equal(90, seg);
        }

        [Fact]
        public void Automatica_LuegoDeLaVentana_DejaPasarUnaSolaSonda()
        {
            var reloj = new Reloj();
            var limitador = new LimitadorCalculo(reloj.Func);
            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(8));

            reloj.Ahora = reloj.Ahora.AddSeconds(121);

            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));      // la sonda
            Assert.False(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));     // otra automatica mientras corre: no
        }

        [Fact]
        public void Sonda_RapidaDevuelveElSistemaAlEstadoNormal()
        {
            var reloj = new Reloj();
            var limitador = new LimitadorCalculo(reloj.Func);
            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(8));
            reloj.Ahora = reloj.Ahora.AddSeconds(121);
            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(1));   // la sonda fue rapida: reemplaza la medicion lenta

            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));
        }

        [Fact]
        public void Sonda_LentaVuelveABloquearOtraVentana()
        {
            var reloj = new Reloj();
            var limitador = new LimitadorCalculo(reloj.Func);
            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(8));
            reloj.Ahora = reloj.Ahora.AddSeconds(121);
            Assert.True(limitador.TryEntrar(true, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(9));   // sigue lento

            Assert.False(limitador.TryEntrar(true, 2, Umbral, Reintento, out int seg));
            Assert.Equal(120, seg);
        }

        [Fact]
        public void Manual_AunConLatenciaAltaEntraSiempre()
        {
            var limitador = new LimitadorCalculo(new Reloj().Func);
            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
            limitador.Salir(TimeSpan.FromSeconds(30));

            Assert.True(limitador.TryEntrar(false, 2, Umbral, Reintento, out _));
        }

        [Fact]
        public void MaxConcurrentesCero_BloqueaTodasLasAutomaticas_PeroNoLasManuales()
        {
            // Es la forma de probar a mano el cartel "Servidor ocupado" poniendo MaxConcurrentes=0 en la config.
            var limitador = new LimitadorCalculo(new Reloj().Func);

            Assert.False(limitador.TryEntrar(true, 0, Umbral, Reintento, out _));
            Assert.True(limitador.TryEntrar(false, 0, Umbral, Reintento, out _));
        }
    }
}
