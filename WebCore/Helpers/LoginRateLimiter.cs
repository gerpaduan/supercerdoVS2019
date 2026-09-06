// Port de Web/Helpers/LoginRateLimiter.cs -- rate limiter de login por IP, en memoria
// (ConcurrentDictionary, sin persistencia -- se resetea si el proceso reinicia, mismo criterio
// que el original). Unico cambio real: HttpRequestBase (System.Web, no existe en Core) -> recibe
// directo el string de IP ya resuelto por el caller (LoginController, via
// HttpContext.Connection.RemoteIpAddress + header X-Forwarded-For). Logica de ventana/intentos/
// lockout sin cambios.
using System.Collections.Concurrent;
using System.Globalization;

namespace WebCore.Helpers
{
    public static class LoginRateLimiter
    {
        private sealed class AttemptState
        {
            public int Failures { get; set; }
            public DateTimeOffset FirstFailureUtc { get; set; }
            public DateTimeOffset? LockedUntilUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, AttemptState> Attempts =
            new ConcurrentDictionary<string, AttemptState>(StringComparer.OrdinalIgnoreCase);

        private static readonly object Sync = new object();

        public static bool IsBlocked(string ip, string userName, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            CleanupExpiredEntries();

            foreach (var key in BuildKeys(ip, userName))
            {
                AttemptState? state;
                if (!Attempts.TryGetValue(key, out state) || !state.LockedUntilUtc.HasValue)
                    continue;

                var remaining = state.LockedUntilUtc.Value - DateTimeOffset.UtcNow;
                if (remaining > TimeSpan.Zero)
                {
                    retryAfter = remaining;
                    return true;
                }
            }

            return false;
        }

        public static void RegisterFailure(string ip, string userName)
        {
            var now = DateTimeOffset.UtcNow;
            var window = GetWindow();
            var maxAttempts = GetMaxAttempts();
            var lockout = GetLockout();

            lock (Sync)
            {
                foreach (var key in BuildKeys(ip, userName))
                {
                    var state = Attempts.GetOrAdd(key, _ => new AttemptState
                    {
                        Failures = 0,
                        FirstFailureUtc = now
                    });

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
        }

        public static void Reset(string ip, string userName)
        {
            foreach (var key in BuildKeys(ip, userName))
            {
                Attempts.TryRemove(key, out _);
            }
        }

        private static string[] BuildKeys(string ip, string userName)
        {
            var user = (userName ?? "").Trim().ToLowerInvariant();

            return new[]
            {
                "ip:" + ip,
                "ip-user:" + ip + ":" + user
            };
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
                    Attempts.TryRemove(pair.Key, out _);
                }
            }
        }

        private static int GetMaxAttempts()
        {
            return GetInt("Security:LoginMaxAttempts", 5, 3, 20);
        }

        private static TimeSpan GetWindow()
        {
            return TimeSpan.FromMinutes(GetInt("Security:LoginWindowMinutes", 10, 1, 120));
        }

        private static TimeSpan GetLockout()
        {
            return TimeSpan.FromMinutes(GetInt("Security:LoginLockoutMinutes", 15, 1, 240));
        }

        private static int GetInt(string key, int fallback, int min, int max)
        {
            int value;
            if (!int.TryParse(System.Configuration.ConfigurationManager.AppSettings[key], NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                return fallback;

            if (value < min)
                return min;

            if (value > max)
                return max;

            return value;
        }
    }
}
