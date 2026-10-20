// Limitador de calculos pesados para las actualizaciones AUTOMATICAS de una pantalla (hoy: Existencia por sucursales).
// Mide lo que importa -- cuantos calculos hay corriendo y cuanto tardo el ultimo -- en vez de mirar CPU/RAM del servidor
// (dependen del sistema operativo y del motor de base, y un pico ajeno como un backup daria falsos bloqueos).
// Reglas (docs/DECISIONS.md 2026-10-19):
//  - Solo se rechazan las llamadas AUTOMATICAS; las manuales (el usuario apreto Buscar) entran siempre, pero cuentan.
//  - Una automatica se rechaza si ya hay `maxConcurrentes` calculos en curso, o si el ultimo tardo mas que `umbralLatencia`
//    y todavia no pasaron `reintento` desde esa medicion.
//  - Pasada esa ventana deja entrar UNA sonda (reserva la ventana para que no entren varias juntas); la duracion de la sonda
//    REEMPLAZA a la anterior, asi un calculo lento puntual no bloquea las automaticas para siempre.
// Estado en memoria del proceso: un reinicio lo pone en cero (cada servidor corre una sola instancia). Con tests:
// Negocio.Tests/LimitadorCalculoTests.cs. Lo usa WebCore.Helpers.ExistenciaCargaGuard.
//
// Sintaxis a proposito conservadora (C# 7.3): Negocio tambien compila para net472.
using System;

namespace Negocio
{
    public sealed class LimitadorCalculo
    {
        private readonly object _candado = new object();
        private readonly Func<DateTime> _ahora;

        private int _enCurso;
        private bool _hayMedicion;
        private TimeSpan _ultimaDuracion;
        private DateTime _ultimaMedicion;

        // `ahora` se inyecta solo para poder probar el paso del tiempo; null = reloj real.
        public LimitadorCalculo(Func<DateTime> ahora = null)
        {
            _ahora = ahora ?? (() => DateTime.UtcNow);
        }

        // Calculos en curso en este momento (para diagnostico y tests).
        public int EnCurso
        {
            get { lock (_candado) { return _enCurso; } }
        }

        // Pide permiso para calcular. Si devuelve true, el llamador DEBE llamar a Salir (try/finally). Si devuelve false,
        // `segundosReintento` es en cuanto conviene volver a intentar (>= 1).
        public bool TryEntrar(bool automatica, int maxConcurrentes, TimeSpan umbralLatencia, TimeSpan reintento, out int segundosReintento)
        {
            if (maxConcurrentes < 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrentes));

            segundosReintento = 0;
            lock (_candado)
            {
                if (automatica)
                {
                    if (_enCurso >= maxConcurrentes)
                    {
                        segundosReintento = SegundosMinimo1(reintento);
                        return false;
                    }

                    if (_hayMedicion && _ultimaDuracion > umbralLatencia)
                    {
                        TimeSpan transcurrido = _ahora() - _ultimaMedicion;
                        if (transcurrido < reintento)
                        {
                            segundosReintento = SegundosMinimo1(reintento - transcurrido);
                            return false;
                        }

                        // Ventana cumplida: entra UNA sonda. Se reserva la ventana ahora para que otra automatica que llegue
                        // mientras la sonda corre no entre tambien; Salir() la reemplaza por la medicion real.
                        _ultimaMedicion = _ahora();
                    }
                }

                _enCurso++;
                return true;
            }
        }

        // Registra el fin de un calculo (con o sin error) y su duracion.
        public void Salir(TimeSpan duracion)
        {
            lock (_candado)
            {
                if (_enCurso > 0) _enCurso--;
                _hayMedicion = true;
                _ultimaDuracion = duracion;
                _ultimaMedicion = _ahora();
            }
        }

        private static int SegundosMinimo1(TimeSpan tiempo)
        {
            int segundos = (int)Math.Ceiling(tiempo.TotalSeconds);
            return segundos < 1 ? 1 : segundos;
        }
    }
}
