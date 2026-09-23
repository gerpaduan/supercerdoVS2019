// Lectura del JSON del carrito que el POS guarda como "venta en curso" (misma forma que POSDraft del
// navegador: lineas[], idPersona, razonSocial, ...). Vive en WebCore porque Negocio compila tambien
// para net472 y no usa System.Text.Json. No confia en el navegador: solo lee y calcula (cantidad de
// lineas activas, total, cliente); un JSON invalido o de otra forma simplemente no se guarda.
using System.Globalization;
using System.Text.Json;

namespace WebCore.Services
{
    public static class BorradorPosPayload
    {
        private static readonly CultureInfo Es = new CultureInfo("es-AR");

        public sealed class Resumen
        {
            public int LineasActivas { get; set; }
            public int LineasTotales { get; set; }
            public decimal Total { get; set; }
            public int? IdPersona { get; set; }
            public string RazonSocial { get; set; } = "";
        }

        // Una linea legible para mostrar en el listado del POS y en el detalle del admin.
        public sealed class Item
        {
            public string Codigo { get; set; } = "";
            public string Producto { get; set; } = "";
            public string Cantidad { get; set; } = "";
            public string Precio { get; set; } = "";
            public string Importe { get; set; } = "";
            public bool Anulado { get; set; }
        }

        // Calcula el resumen del carrito. false si el payload no es un objeto con "lineas" (array).
        public static bool TryResumir(JsonElement payload, out Resumen resumen)
        {
            resumen = new Resumen();
            if (payload.ValueKind != JsonValueKind.Object) return false;
            if (!payload.TryGetProperty("lineas", out JsonElement lineas) || lineas.ValueKind != JsonValueKind.Array) return false;

            foreach (JsonElement linea in lineas.EnumerateArray())
            {
                // POSState puede tener huecos (null) donde se elimino una linea fisicamente.
                if (linea.ValueKind != JsonValueKind.Object) continue;

                resumen.LineasTotales++;
                if (LeerBool(linea, "anulado")) continue;

                resumen.LineasActivas++;
                resumen.Total += ImporteDeLinea(linea);
            }

            resumen.Total = Math.Round(resumen.Total, 2, MidpointRounding.AwayFromZero);

            if (payload.TryGetProperty("idPersona", out JsonElement idPersona))
            {
                string textoId = LeerTexto(idPersona);
                if (int.TryParse(textoId, NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) && id > 0)
                    resumen.IdPersona = id;
            }
            resumen.RazonSocial = LeerTextoDe(payload, "razonSocial");
            if (resumen.RazonSocial.Length > 200) resumen.RazonSocial = resumen.RazonSocial.Substring(0, 200);
            return true;
        }

        // Arma las lineas para mostrar a partir del payload ya guardado (texto JSON). Lista vacia si no se puede leer.
        public static List<Item> ConstruirItems(string payloadJson)
        {
            var items = new List<Item>();
            if (string.IsNullOrWhiteSpace(payloadJson)) return items;

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(payloadJson))
                {
                    if (doc.RootElement.ValueKind != JsonValueKind.Object) return items;
                    if (!doc.RootElement.TryGetProperty("lineas", out JsonElement lineas) || lineas.ValueKind != JsonValueKind.Array) return items;

                    foreach (JsonElement linea in lineas.EnumerateArray())
                    {
                        if (linea.ValueKind != JsonValueKind.Object) continue;

                        string producto = LeerTextoDe(linea, "producto");
                        if (producto.Length == 0) producto = LeerTextoDe(linea, "descripcion");

                        items.Add(new Item
                        {
                            Codigo = LeerTextoDe(linea, "codigo"),
                            Producto = producto,
                            Cantidad = ParsearDecimal(LeerTextoDe(linea, "cant")).ToString("N3", Es),
                            Precio = "$ " + ParsearDecimal(LeerTextoDe(linea, "precio")).ToString("N2", Es),
                            Importe = "$ " + ImporteDeLinea(linea).ToString("N2", Es),
                            Anulado = LeerBool(linea, "anulado")
                        });
                    }
                }
            }
            catch (JsonException)
            {
                // Payload corrupto: se muestra la venta sin detalle de items en vez de romper el listado.
                items.Clear();
            }

            return items;
        }

        // Importe de una linea: el subtotal que calculo el POS (ya incluye bonificacion) y, si falta,
        // cantidad x precio.
        private static decimal ImporteDeLinea(JsonElement linea)
        {
            decimal subtotal = ParsearDecimal(LeerTextoDe(linea, "subtotal"));
            if (subtotal != 0m) return subtotal;
            return ParsearDecimal(LeerTextoDe(linea, "cant")) * ParsearDecimal(LeerTextoDe(linea, "precio"));
        }

        // Numeros del POS ("$ 15.00", "1.500", "1,5"): el ULTIMO separador es el decimal (mismo criterio que
        // Helpers.NumeroDecimalPunto). 0 si no es un numero.
        public static decimal ParsearDecimal(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return 0m;

            string limpio = texto.Replace("$", "").Replace(" ", "").Replace(',', '.');
            int ultimoPunto = limpio.LastIndexOf('.');
            if (ultimoPunto >= 0)
                limpio = limpio.Substring(0, ultimoPunto).Replace(".", "") + "." + limpio.Substring(ultimoPunto + 1);

            return decimal.TryParse(limpio, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal valor) ? valor : 0m;
        }

        private static bool LeerBool(JsonElement obj, string propiedad)
        {
            if (!obj.TryGetProperty(propiedad, out JsonElement valor)) return false;
            if (valor.ValueKind == JsonValueKind.True) return true;
            if (valor.ValueKind == JsonValueKind.String) return string.Equals(valor.GetString(), "true", StringComparison.OrdinalIgnoreCase);
            return false;
        }

        private static string LeerTextoDe(JsonElement obj, string propiedad)
        {
            return obj.TryGetProperty(propiedad, out JsonElement valor) ? LeerTexto(valor) : "";
        }

        private static string LeerTexto(JsonElement valor)
        {
            switch (valor.ValueKind)
            {
                case JsonValueKind.String: return valor.GetString() ?? "";
                case JsonValueKind.Number: return valor.GetRawText();
                default: return "";
            }
        }
    }
}
