// Identificacion del navegador para "dispositivo seguro" (login solo desde dispositivos
// seguros, 2026-09-19, ver docs/DECISIONS.md). Un navegador no puede leer el serial de la PC ni
// el IMEI del celular, asi que el servidor le asigna un token aleatorio de 128 bits en una cookie
// de larga duracion (HttpOnly: el JS de la pagina no puede leerla). En BD se guarda solo el hash
// (Negocio.DispositivoSeguro.SerieDeToken). Cookie emitida por el servidor (no localStorage/JS)
// porque Safari en iOS limita a 7 dias lo escrito por script.
using System.Security.Cryptography;

namespace WebCore.Helpers
{
    public static class DispositivoNavegador
    {
        public const string CookieToken = "cs_dev";
        public const string CookiePedido = "cs_dev_req";

        // Devuelve el token de este navegador; si no tiene (o esta corrupto) genera uno y lo deja
        // seteado en la respuesta -- el primer request ya lo usa, aunque todavia no este autorizado.
        public static string AsegurarToken(HttpContext contexto)
        {
            string? actual = contexto.Request.Cookies[CookieToken];
            if (EsTokenValido(actual))
                return actual!;

            byte[] bytes = new byte[16];
            RandomNumberGenerator.Fill(bytes);
            string token = Base64Url(bytes);

            contexto.Response.Cookies.Append(CookieToken, token, OpcionesCookie(contexto, TimeSpan.FromDays(730)));
            return token;
        }

        public static void GuardarNoncePedido(HttpContext contexto, string nonce)
        {
            contexto.Response.Cookies.Append(CookiePedido, nonce, OpcionesCookie(contexto, TimeSpan.FromMinutes(30)));
        }

        public static string? LeerNoncePedido(HttpContext contexto)
        {
            return contexto.Request.Cookies[CookiePedido];
        }

        public static void QuitarNoncePedido(HttpContext contexto)
        {
            contexto.Response.Cookies.Delete(CookiePedido);
        }

        private static CookieOptions OpcionesCookie(HttpContext contexto, TimeSpan duracion)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = duracion,
                IsEssential = true,
                // Detras de un reverse proxy (ARR en Servidor SM) la app ve HTTP aunque el usuario
                // use HTTPS: se mira tambien X-Forwarded-Proto.
                Secure = contexto.Request.IsHttps
                    || string.Equals(contexto.Request.Headers["X-Forwarded-Proto"].ToString(), "https", StringComparison.OrdinalIgnoreCase)
            };
        }

        private static bool EsTokenValido(string? token)
        {
            if (string.IsNullOrEmpty(token) || token.Length != 22)
                return false;

            foreach (char c in token)
            {
                bool ok = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!ok) return false;
            }

            return true;
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }
    }
}
