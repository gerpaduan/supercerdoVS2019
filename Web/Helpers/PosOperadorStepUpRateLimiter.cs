using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;

namespace Web.Helpers
{
    /// <summary>
    /// Limita los intentos de autorizacion de operador de POS para la cuenta compartida de
    /// produccion (ver VentasController/PuntosExpendioController.AutorizarOperadorPOS) --
    /// mismo patron que CierreCajaStepUpRateLimiter (duplicado, no generalizado, para no tocar
    /// codigo de seguridad ya probado de Cierre de Caja), con umbrales propios y clave por
    /// SESSION, no por usuario+IP: los intentos son totales para esa sesion en esta vista, sin
    /// importar a que usuario del combo se le probo la contraseña.
    /// </summary>
    internal static class PosOperadorStepUpRateLimiter
    {
        private sealed class AttemptState
        {
            public int Failures { get; set; }
            public DateTimeOffset FirstFailureUtc { get; set; }
            public DateTimeOffset? LockedUntilUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptState> Attempts =
            new ConcurrentDictionary<string, AttemptState>(StringComparer.Ordinal);

        private static readonly object Sync = new object();

        public static bool IsBlocked(string sessionId, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            CleanupExpiredEntries();

            AttemptState state;
            if (!Attempts.TryGetValue(Key(sessionId), out state) || !state.LockedUntilUtc.HasValue)
                return false;

            var remaining = state.LockedUntilUtc.Value - DateTimeOffset.UtcNow;
            if (remaining <= TimeSpan.Zero)
                return false;

            retryAfter = remaining;
            return true;
        }

        public static void RegisterFailure(string sessionId)
        {
            var now = DateTimeOffset.UtcNow;
            var window = GetWindow();
            var maxAttempts = GetMaxAttempts();
            var lockout = GetLockout();
            var key = Key(sessionId);

            lock (Sync)
            {
                var state = Attempts.GetOrAdd(key, _ => new AttemptState { Failures = 0, FirstFailureUtc = now });

                if (state.LockedUntilUtc.HasValue && state.LockedUntilUtc.Value <= now)
                {
                    state.Failures = 0;
                    state.FirstFailureUtc = now;
                    state.LockedUntilUtc = null;
                }

                if ((now - state.FirstFailureUtc) > window)
                {
                    state.Failures = 0;
                    state.FirstFailureUtc = now;
                    state.LockedUntilUtc = null;
                }

                state.Failures++;
                if (state.Failures >= maxAttempts)
                    state.LockedUntilUtc = now.Add(lockout);
            }
        }

        public static void Reset(string sessionId)
        {
            AttemptState removed;
            Attempts.TryRemove(Key(sessionId), out removed);
        }

        private static string Key(string sessionId)
        {
            return "session:" + (sessionId ?? "unknown");
        }

        private static void CleanupExpiredEntries()
        {
            var now = DateTimeOffset.UtcNow;
            var maxAge = GetLockout() > GetWindow() ? GetLockout() : GetWindow();

            foreach (var pair in Attempts.ToArray())
            {
                var state = pair.Value;
                if (state == null)
                    continue;

                var inactive = now - state.FirstFailureUtc;
                var expiredLock = !state.LockedUntilUtc.HasValue || state.LockedUntilUtc.Value <= now;
                if (expiredLock && inactive > maxAge)
                {
                    AttemptState removed;
                    Attempts.TryRemove(pair.Key, out removed);
                }
            }
        }

        private static int GetMaxAttempts()
        {
            return GetInt("Security:PosOperadorStepUpMaxAttempts", 3, 1, 10);
        }

        private static TimeSpan GetWindow()
        {
            return TimeSpan.FromMinutes(GetInt("Security:PosOperadorStepUpWindowMinutes", 5, 1, 120));
        }

        private static TimeSpan GetLockout()
        {
            return TimeSpan.FromMinutes(GetInt("Security:PosOperadorStepUpLockoutMinutes", 5, 1, 240));
        }

        private static int GetInt(string key, int fallback, int min, int max)
        {
            int value;
            if (!int.TryParse(ConfigurationManager.AppSettings[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return fallback;

            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
