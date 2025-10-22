using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace Utilidades
{
    public class VerificadorConexion
    {
        /// <summary>
        /// Verifica conexión a Internet y disponibilidad del servicio AFIP.
        /// </summary>
        /// <returns>
        /// Devuelve un string con el resultado:
        /// "OK" si todo está correcto, o el mensaje de error correspondiente.
        /// </returns>
        public static async Task<string> VerificarAsync(bool produccion = false)
        {
            // 1️⃣ Verificar Internet
            if (!await HayInternetAsync())
                return "No hay conexión a Internet.";

            // 2️⃣ Verificar AFIP
            if (!await AfipDisponibleAsync(produccion))
                return "El servicio de AFIP no está disponible.";

            return "OK";
        }

        private static async Task<bool> HayInternetAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(3);
                    var response = await client.GetAsync("https://www.google.com");
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        private static async Task<bool> AfipDisponibleAsync(bool produccion)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(5);

                    string urlAfip = produccion
                        ? "https://wsaa.afip.gov.ar/ws/services/LoginCms" // Producción
                        : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms"; // Homologación

                    var response = await client.GetAsync(urlAfip);
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
