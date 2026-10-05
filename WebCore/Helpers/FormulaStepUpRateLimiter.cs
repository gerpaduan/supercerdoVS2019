// Limita los intentos fallidos del re-login de "Ver formula" (formula secreta, 2026-10-04, ver
// docs/DECISIONS.md "Formula secreta con re-login"). Mismo patron exacto que CierreCajaStepUpRateLimiter
// (clave por sesion), pero deliberadamente NO se reusa esa clase: mezclaria los intentos fallidos de
// dos flujos de autorizacion distintos dentro de la misma sesion (un operador podria auto-bloquearse
// el cierre de caja por fallar aca, o viceversa). Config keys propias: Security:FormulaStepUp*.
using System.Collections.Concurrent;
using System.Globalization;

namespace WebCore.Helpers
{
    public static class FormulaStepUpRateLimiter
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

        // true (y cuanto falta) si la sesion esta bloqueada por demasiados intentos fallidos.
        public static bool IsBlocked(string sessionId, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            CleanupExpiredEntries();

            if (!Attempts.TryGetValue(Key(sessionId), out var state) || !state.LockedUntilUtc.HasValue)
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

                // Un bloqueo ya vencido, o una ventana de conteo vencida, reinician el contador.
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
            Attempts.TryRemove(Key(sessionId), out _);
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
                    Attempts.TryRemove(pair.Key, out _);
            }
        }

        private static int GetMaxAttempts()
        {
            return GetInt("Security:FormulaStepUpMaxAttempts", 3, 1, 10);
        }

        private static TimeSpan GetWindow()
        {
            return TimeSpan.FromMinutes(GetInt("Security:FormulaStepUpWindowMinutes", 5, 1, 120));
        }

        private static TimeSpan GetLockout()
        {
            return TimeSpan.FromMinutes(GetInt("Security:FormulaStepUpLockoutMinutes", 5, 1, 240));
        }

        private static int GetInt(string key, int fallback, int min, int max)
        {
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return fallback;

            return Math.Min(Math.Max(value, min), max);
        }
    }
}
