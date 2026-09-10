// Rate limiter para LoginController.ForgotPassword (2026-09-09, Batch E, ver docs/DECISIONS.md
// "Batch E: recuperacion de contraseña"). Mejora de seguridad nueva, sin equivalente en el
// clasico -- ver decision del usuario de mejorar este flujo en vez de portarlo 1:1. Sin esto,
// cualquiera podia hacer POST a /Login/ForgotPassword sin limite, disparando un mail real por
// intento (spam a la bandeja del usuario legitimo) y generando tokens sin limite en la tabla
// UsuarioPasswordResetTokens. Mismo patron en memoria (ConcurrentDictionary, sin persistencia)
// que WebCore/Helpers/LoginRateLimiter.cs, pero mas simple: solo cuenta por IP (no hace falta
// distinguir por usuario/email, el objetivo es limitar volumen total desde un mismo origen, no
// proteger una cuenta puntual contra fuerza bruta).
using System.Collections.Concurrent;
using System.Globalization;

namespace WebCore.Helpers
{
    public static class PasswordResetRateLimiter
    {
        private sealed class AttemptState
        {
            public int Solicitudes { get; set; }
            public DateTimeOffset PrimeraSolicitudUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptState> Solicitudes =
            new ConcurrentDictionary<string, AttemptState>(StringComparer.OrdinalIgnoreCase);

        private static readonly object Sync = new object();

        public static bool IsBlocked(string ip, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            CleanupExpiredEntries();

            var window = GetWindow();
            var maxSolicitudes = GetMaxSolicitudes();
            var key = "ip:" + (ip ?? "unknown");

            if (!Solicitudes.TryGetValue(key, out var state))
                return false;

            var elapsed = DateTimeOffset.UtcNow - state.PrimeraSolicitudUtc;
            if (elapsed > window)
                return false;

            if (state.Solicitudes < maxSolicitudes)
                return false;

            retryAfter = window - elapsed;
            return true;
        }

        public static void RegisterRequest(string ip)
        {
            var now = DateTimeOffset.UtcNow;
            var window = GetWindow();
            var key = "ip:" + (ip ?? "unknown");

            lock (Sync)
            {
                var state = Solicitudes.GetOrAdd(key, _ => new AttemptState { Solicitudes = 0, PrimeraSolicitudUtc = now });

                if ((now - state.PrimeraSolicitudUtc) > window)
                {
                    state.Solicitudes = 0;
                    state.PrimeraSolicitudUtc = now;
                }

                state.Solicitudes++;
            }
        }

        private static void CleanupExpiredEntries()
        {
            var now = DateTimeOffset.UtcNow;
            var window = GetWindow();

            foreach (var pair in Solicitudes.ToArray())
            {
                if ((now - pair.Value.PrimeraSolicitudUtc) > window)
                    Solicitudes.TryRemove(pair.Key, out _);
            }
        }

        private static int GetMaxSolicitudes()
        {
            return GetInt("Security:PasswordResetMaxRequests", 3, 1, 20);
        }

        private static TimeSpan GetWindow()
        {
            return TimeSpan.FromMinutes(GetInt("Security:PasswordResetWindowMinutes", 15, 1, 240));
        }

        private static int GetInt(string key, int fallback, int min, int max)
        {
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return fallback;

            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
