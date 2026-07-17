using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Web.Helpers
{
    internal static class LoginRateLimiter
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

        public static bool IsBlocked(HttpRequestBase request, string userName, out TimeSpan retryAfter)
        {
            retryAfter = TimeSpan.Zero;
            CleanupExpiredEntries();

            foreach (var key in BuildKeys(request, userName))
            {
                AttemptState state;
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

        public static void RegisterFailure(HttpRequestBase request, string userName)
        {
            var now = DateTimeOffset.UtcNow;
            var window = GetWindow();
            var maxAttempts = GetMaxAttempts();
            var lockout = GetLockout();

            lock (Sync)
            {
                foreach (var key in BuildKeys(request, userName))
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

        public static void Reset(HttpRequestBase request, string userName)
        {
            foreach (var key in BuildKeys(request, userName))
            {
                AttemptState removed;
                Attempts.TryRemove(key, out removed);
            }
        }

        private static string[] BuildKeys(HttpRequestBase request, string userName)
        {
            var ip = GetClientIp(request);
            var user = (userName ?? "").Trim().ToLowerInvariant();

            return new[]
            {
                "ip:" + ip,
                "ip-user:" + ip + ":" + user
            };
        }

        private static string GetClientIp(HttpRequestBase request)
        {
            if (request == null)
                return "unknown";

            var forwardedFor = request.Headers["X-Forwarded-For"];
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                var first = forwardedFor.Split(',').Select(x => x.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (!string.IsNullOrWhiteSpace(first))
                    return first;
            }

            var forwardedProto = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrWhiteSpace(forwardedProto))
            {
                var first = forwardedProto.Split(',').Select(x => x.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (!string.IsNullOrWhiteSpace(first))
                    return first;
            }

            return request.UserHostAddress ?? "unknown";
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
