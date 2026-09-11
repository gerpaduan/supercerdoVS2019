using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Negocio
{
    // Cliente del flujo OAuth authorization_code + PKCE de Mercado Pago (una sola Aplicacion
    // "CarniSys" para toda la plataforma, ver docs/DECISIONS.md 2026-08-31). Solo arma la URL de
    // autorizacion y hace el intercambio code->tokens contra la API de Mercado Pago -- no
    // conoce Web.config ni persiste nada (eso lo hace MercadoPagoConfig + el controller).
    //
    // Fuente de los parametros/campos: documentacion oficial de Mercado Pago Developers
    // (OAuth authorization_code + PKCE), consultada el 2026-08-31.
    public class MercadoPagoOAuthClient
    {
        private const string UrlAutorizacion = "https://auth.mercadopago.com/authorization";
        private const string UrlToken = "https://api.mercadopago.com/oauth/token";

        public string ConstruirUrlAutorizacion(string clientId, string redirectUri, string state, string codeChallenge)
        {
            if (string.IsNullOrWhiteSpace(clientId)) throw new ArgumentException("clientId vacío.", nameof(clientId));
            if (string.IsNullOrWhiteSpace(redirectUri)) throw new ArgumentException("redirectUri vacío.", nameof(redirectUri));
            if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("state vacío.", nameof(state));
            if (string.IsNullOrWhiteSpace(codeChallenge)) throw new ArgumentException("codeChallenge vacío.", nameof(codeChallenge));

            return UrlAutorizacion +
                "?client_id=" + Uri.EscapeDataString(clientId) +
                "&response_type=code" +
                "&platform_id=mp" +
                "&state=" + Uri.EscapeDataString(state) +
                "&redirect_uri=" + Uri.EscapeDataString(redirectUri) +
                "&code_challenge=" + Uri.EscapeDataString(codeChallenge) +
                "&code_challenge_method=S256";
        }

        // modoPrueba=true agrega test_token=true (sandbox, sin autorizacion real de por medio) --
        // decision confirmada con el usuario para arrancar probando Fase 2 (ver plan de sesion).
        public MercadoPagoTokenResult IntercambiarCodigoPorTokens(string clientId, string clientSecret,
            string code, string redirectUri, string codeVerifier, bool modoPrueba)
        {
            var body = new Dictionary<string, object>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "redirect_uri", redirectUri },
                { "code_verifier", codeVerifier }
            };
            if (modoPrueba) body["test_token"] = "true";

            var serializer = new JavaScriptSerializer();
            string json = serializer.Serialize(body);

            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);

                HttpResponseMessage respuesta;
                string contenidoRespuesta;
                try
                {
                    var contenido = new StringContent(json, Encoding.UTF8, "application/json");
                    respuesta = Task.Run(() => http.PostAsync(UrlToken, contenido)).GetAwaiter().GetResult();
                    contenidoRespuesta = Task.Run(() => respuesta.Content.ReadAsStringAsync()).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("No se pudo contactar a la API de Mercado Pago (oauth/token).", ex);
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        "Mercado Pago rechazó el intercambio de tokens (HTTP " + (int)respuesta.StatusCode + "): " + contenidoRespuesta);
                }

                var datos = serializer.Deserialize<Dictionary<string, object>>(contenidoRespuesta);
                return new MercadoPagoTokenResult
                {
                    AccessToken = ObtenerString(datos, "access_token"),
                    RefreshToken = ObtenerString(datos, "refresh_token"),
                    ExpiresInSeconds = ObtenerLong(datos, "expires_in"),
                    UserId = ObtenerString(datos, "user_id"),
                    Scope = ObtenerString(datos, "scope"),
                    LiveMode = datos.ContainsKey("live_mode") && Convert.ToBoolean(datos["live_mode"])
                };
            }
        }

        private static string ObtenerString(Dictionary<string, object> datos, string clave)
        {
            return datos.ContainsKey(clave) && datos[clave] != null ? datos[clave].ToString() : "";
        }

        private static long ObtenerLong(Dictionary<string, object> datos, string clave)
        {
            return datos.ContainsKey(clave) && datos[clave] != null ? Convert.ToInt64(datos[clave]) : 0;
        }
    }

    public class MercadoPagoTokenResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public long ExpiresInSeconds { get; set; }
        public string UserId { get; set; }
        public string Scope { get; set; }
        public bool LiveMode { get; set; }
    }
}
