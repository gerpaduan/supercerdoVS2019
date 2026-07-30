using System;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Web.Helpers
{
    internal static class SecurityRuntime
    {
        public static bool EnforceHttps
        {
            get { return GetBool("Security:EnforceHttps", false); }
        }

        public static bool CookieRequireSsl
        {
            get { return GetBool("Security:CookieRequireSsl", false); }
        }

        public static bool EnableHsts
        {
            get { return GetBool("Security:EnableHsts", false); }
        }

        public static string[] AllowedCorsOrigins
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["Security:AllowedCorsOrigins"] ?? "";
                return raw
                    .Split(new[] { ',', ';', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        public static bool IsSecureRequest(HttpRequest request)
        {
            if (request == null)
                return false;

            if (request.IsSecureConnection)
                return true;

            var forwardedProto = request.Headers["X-Forwarded-Proto"] ?? request.ServerVariables["HTTP_X_FORWARDED_PROTO"];
            return string.Equals(forwardedProto, "https", StringComparison.OrdinalIgnoreCase);
        }

        public static string BuildHttpsUrl(HttpRequest request)
        {
            if (request == null || request.Url == null)
                return null;

            var builder = new UriBuilder(request.Url) { Scheme = Uri.UriSchemeHttps, Port = 443 };
            return builder.Uri.AbsoluteUri;
        }

        public static void ApplyResponseHeaders(HttpContext context)
        {
            if (context == null)
                return;

            var request = context.Request;
            var response = context.Response;
            var isSecure = IsSecureRequest(request);

            if (EnableHsts && isSecure)
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            response.Headers["X-Frame-Options"] = "SAMEORIGIN";
            response.Headers["X-Content-Type-Options"] = "nosniff";
            response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            response.Headers["Permissions-Policy"] = "accelerometer=(), autoplay=(), camera=(self), geolocation=(self), gyroscope=(), magnetometer=(), microphone=(), payment=(), usb=()";
            response.Headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "base-uri 'self'; " +
                "form-action 'self'; " +
                "frame-ancestors 'self'; " +
                "object-src 'none'; " +
                "img-src 'self' data: blob: https:; " +
                "font-src 'self' data: https://fonts.gstatic.com; " +
                "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://cdn.jsdelivr.net; " +
                "connect-src 'self';";
            response.Headers["X-Permitted-Cross-Domain-Policies"] = "none";
            response.Headers.Remove("X-Powered-By");
            response.Headers.Remove("Server");

            ApplyCors(context, isSecure);
            HardenCookies(response, isSecure);
        }

        public static void ApplyCors(HttpContext context, bool isSecureRequest)
        {
            if (context == null)
                return;

            var request = context.Request;
            var response = context.Response;
            var origin = request.Headers["Origin"];
            if (string.IsNullOrWhiteSpace(origin))
                return;

            if (!AllowedCorsOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
                return;

            response.Headers["Access-Control-Allow-Origin"] = origin;
            response.Headers["Vary"] = AppendCsvValue(response.Headers["Vary"], "Origin");
            response.Headers["Access-Control-Allow-Credentials"] = "true";

            if (string.Equals(request.HttpMethod, "OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                response.Headers["Access-Control-Allow-Methods"] = "GET, POST, PUT, DELETE, OPTIONS";
                response.Headers["Access-Control-Allow-Headers"] = "Content-Type, RequestVerificationToken, X-Requested-With";
                response.Headers["Access-Control-Max-Age"] = "600";
                response.StatusCode = 204;
                context.ApplicationInstance.CompleteRequest();
            }
        }

        private static void HardenCookies(HttpResponse response, bool isSecureRequest)
        {
            if (response == null)
                return;

            foreach (string key in response.Cookies.AllKeys)
            {
                var cookie = response.Cookies[key];
                if (cookie == null)
                    continue;

                cookie.HttpOnly = true;
                if (CookieRequireSsl && isSecureRequest)
                    cookie.Secure = true;

                if (cookie.SameSite == (SameSiteMode)(-1))
                    cookie.SameSite = SameSiteMode.Lax;
            }
        }

        private static string AppendCsvValue(string current, string value)
        {
            if (string.IsNullOrWhiteSpace(current))
                return value;

            var parts = current.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (!parts.Contains(value, StringComparer.OrdinalIgnoreCase))
                parts.Add(value);

            return string.Join(", ", parts);
        }

        private static bool GetBool(string key, bool fallback)
        {
            bool parsed;
            return bool.TryParse(ConfigurationManager.AppSettings[key], out parsed) ? parsed : fallback;
        }
    }
}
