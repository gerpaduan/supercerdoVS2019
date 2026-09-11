using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace Negocio
{
    // Cliente de las APIs de Mercado Pago para dar de alta Sucursal (Store) y Caja (POS), y
    // consultar/activar la terminal fisica vinculada a esa Caja (Fase 3, ver docs/DECISIONS.md
    // 2026-09-01). Usa el access_token de la empresa (ya descifrado por quien llama,
    // Negocio.MercadoPagoConfig.ObtenerPorEmpresa) -- esta clase no conoce nada de cifrado ni
    // de la DB, solo llama a la API.
    //
    // Fuente de los endpoints/campos: documentacion oficial de Mercado Pago Developers
    // (Store/POS/Terminal), consultada el 2026-09-01.
    public class MercadoPagoPointClient
    {
        private const string UrlBase = "https://api.mercadopago.com";

        public class DatosStore
        {
            public string Nombre { get; set; }
            public string ExternalId { get; set; }
            public string Calle { get; set; }
            public string Numero { get; set; }
            public string Ciudad { get; set; }
            public string Provincia { get; set; }
            public decimal? Latitud { get; set; }
            public decimal? Longitud { get; set; }
        }

        public string CrearStore(string accessToken, string mpUserId, DatosStore datos)
        {
            if (string.IsNullOrWhiteSpace(mpUserId)) throw new ArgumentException("Falta el mpUserId de la empresa (no se conectó todavía o falta reconectar).", nameof(mpUserId));

            var body = new Dictionary<string, object>
            {
                { "name", datos.Nombre },
                { "external_id", datos.ExternalId },
                { "location", new Dictionary<string, object>
                    {
                        { "street_name", datos.Calle },
                        { "street_number", datos.Numero },
                        { "city_name", datos.Ciudad },
                        { "state_name", datos.Provincia },
                        { "latitude", datos.Latitud },
                        { "longitude", datos.Longitud }
                    }
                }
            };

            var respuesta = EjecutarJson(accessToken, HttpMethod.Post, UrlBase + "/users/" + Uri.EscapeDataString(mpUserId) + "/stores", body);
            return ObtenerString(respuesta, "id");
        }

        public string CrearPos(string accessToken, string storeId, string nombre, string externalId)
        {
            var body = new Dictionary<string, object>
            {
                { "name", nombre },
                { "store_id", storeId },
                { "external_id", externalId }
            };

            var respuesta = EjecutarJson(accessToken, HttpMethod.Post, UrlBase + "/v2/pos", body, idempotencyKey: Guid.NewGuid().ToString("N"));
            return ObtenerString(respuesta, "id");
        }

        // Devuelve el id de HARDWARE (formato "MARCA_MODELO__SERIAL") de la primera terminal
        // vinculada a esa Caja, o "" si todavia no se emparejó ninguna (paso manual con la app
        // de Mercado Pago, fuera de CarniSys).
        public string BuscarTerminalVinculada(string accessToken, string storeId, string posId)
        {
            string url = UrlBase + "/terminals/v1/list?store_id=" + Uri.EscapeDataString(storeId) + "&pos_id=" + Uri.EscapeDataString(posId);
            var respuesta = EjecutarJson(accessToken, HttpMethod.Get, url, null);

            if (respuesta.ContainsKey("terminals") && respuesta["terminals"] is object[] terminales && terminales.Length > 0)
            {
                var primera = terminales[0] as Dictionary<string, object>;
                return primera != null ? ObtenerString(primera, "id") : "";
            }
            return "";
        }

        public void CambiarOperatingMode(string accessToken, string terminalIdHardware, string modo)
        {
            var body = new Dictionary<string, object>
            {
                { "terminals", new object[]
                    {
                        new Dictionary<string, object> { { "id", terminalIdHardware }, { "operating_mode", modo } }
                    }
                }
            };

            EjecutarJson(accessToken, new HttpMethod("PATCH"), UrlBase + "/terminals/v1/setup", body);
        }

        private Dictionary<string, object> EjecutarJson(string accessToken, HttpMethod metodo, string url, Dictionary<string, object> body, string idempotencyKey = null)
        {
            if (string.IsNullOrWhiteSpace(accessToken)) throw new ArgumentException("Falta el access_token de la empresa.", nameof(accessToken));

            var serializer = new JavaScriptSerializer();

            using (var http = new HttpClient())
            {
                http.Timeout = TimeSpan.FromSeconds(30);
                http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                if (!string.IsNullOrWhiteSpace(idempotencyKey))
                    http.DefaultRequestHeaders.Add("X-Idempotency-Key", idempotencyKey);

                var request = new HttpRequestMessage(metodo, url);
                if (body != null)
                {
                    request.Content = new StringContent(serializer.Serialize(body), Encoding.UTF8, "application/json");
                }

                HttpResponseMessage respuesta;
                string contenidoRespuesta;
                try
                {
                    respuesta = Task.Run(() => http.SendAsync(request)).GetAwaiter().GetResult();
                    contenidoRespuesta = Task.Run(() => respuesta.Content.ReadAsStringAsync()).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("No se pudo contactar a la API de Mercado Pago (" + url + ").", ex);
                }

                if (!respuesta.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(
                        "Mercado Pago rechazó la operación (HTTP " + (int)respuesta.StatusCode + ", " + url + "): " + contenidoRespuesta);
                }

                if (string.IsNullOrWhiteSpace(contenidoRespuesta)) return new Dictionary<string, object>();
                return serializer.Deserialize<Dictionary<string, object>>(contenidoRespuesta);
            }
        }

        private static string ObtenerString(Dictionary<string, object> datos, string clave)
        {
            return datos != null && datos.ContainsKey(clave) && datos[clave] != null ? datos[clave].ToString() : "";
        }
    }
}
