using System.Globalization;

namespace WebCore.Helpers
{
    // Lee decimales posteados por formularios donde el punto (o la coma) es SIEMPRE el separador
    // decimal y nunca de miles (ej. Kgs.Medias: "1267.00" = 1267 kg). No usa la cultura del proceso: en es-AR
    // el punto es separador de miles y "1267.00" se leia como 126700.
    public static class NumeroDecimalPunto
    {
        // "1267.5", "1267,5" y "1 267.5" -> 1267.5. Con varios separadores ("1.267,5") el ultimo es el decimal
        // y los anteriores se descartan. Devuelve false si esta vacio o no es un numero.
        public static bool TryParse(string raw, out float value)
        {
            value = 0f;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            string texto = raw.Replace(" ", "").Replace(',', '.');

            int ultimoPunto = texto.LastIndexOf('.');
            if (ultimoPunto >= 0)
                texto = texto.Substring(0, ultimoPunto).Replace(".", "") + "." + texto.Substring(ultimoPunto + 1);

            return float.TryParse(texto, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }
    }
}
