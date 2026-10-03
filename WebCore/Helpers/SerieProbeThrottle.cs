// Limita cuantas veces una misma IP puede "probar" un ID de hardware por la URL del login por CUIT
// (/Login/{cuit}?serie=...) para ver la lista de usuarios (2026-10-02, Fase 1 de docs/DECISIONS.md
// "Login por CUIT, clave rapida (PIN) y politica de clave"). En memoria, ventana fija: se pierde al
// reiniciar el proceso (mismo criterio que LoginRateLimiter). No comparte contadores con el limite de
// intentos de login: probar IDs no debe bloquear a nadie para ingresar.
using System.Collections.Concurrent;

namespace WebCore.Helpers
{
    public static class SerieProbeThrottle
    {
        // Un equipo legitimo sin autorizar hace 1 consulta por visita; 20 en 10 minutos es de sobra.
        private const int MaxIntentosPorVentana = 20;
        private static readonly TimeSpan Ventana = TimeSpan.FromMinutes(10);

        private sealed class Estado
        {
            public int Intentos;
            public DateTimeOffset Inicio;
        }

        private static readonly ConcurrentDictionary<string, Estado> PorIp = new ConcurrentDictionary<string, Estado>();

        public static bool EstaBloqueada(string ip)
        {
            return PorIp.TryGetValue(ip, out var e) && DateTimeOffset.UtcNow - e.Inicio < Ventana && e.Intentos >= MaxIntentosPorVentana;
        }

        public static void RegistrarIntentoFallido(string ip)
        {
            var ahora = DateTimeOffset.UtcNow;
            PorIp.AddOrUpdate(
                ip,
                _ => new Estado { Intentos = 1, Inicio = ahora },
                (_, e) =>
                {
                    lock (e)
                    {
                        if (ahora - e.Inicio >= Ventana) { e.Intentos = 0; e.Inicio = ahora; }
                        e.Intentos++;
                    }
                    return e;
                });

            // Limpieza oportunista para que el diccionario no crezca sin limite.
            if (PorIp.Count > 5000)
            {
                foreach (var par in PorIp)
                {
                    if (ahora - par.Value.Inicio >= Ventana)
                        PorIp.TryRemove(par.Key, out _);
                }
            }
        }
    }
}
