// Pantalla dividida (2026-11-20, docs/DECISIONS.md "Pantalla dividida"): convierte las URLs que muestran los paneles en pares
// (controller, action) para que PermisosHelper.LimpiarOperadoresFueraDe sepa que pantallas siguen abiertas.
// El navegador es input hostil: solo se aceptan rutas relativas del mismo sitio, con segmentos alfanumericos, y como mucho MaxPaneles.
using System;
using System.Collections.Generic;

namespace WebCore.Helpers
{
    public static class PantallaDivididaRutas
    {
        // Tope de paneles de la pantalla dividida (el JS usa el mismo valor: pantalla-dividida-arbol.js MAX_PANELES).
        public const int MaxPaneles = 6;

        // Devuelve las rutas validas ya convertidas; las invalidas se ignoran (un panel con una URL rara cuenta como "no abierto").
        // pathBase: prefijo de la app ("" o "/carnisys"), se quita si la ruta lo trae.
        public static IReadOnlyList<(string Controller, string Action)> Parsear(IEnumerable<string>? urls, string? pathBase)
        {
            var resultado = new List<(string Controller, string Action)>();
            if (urls == null) return resultado;

            var baseNormalizada = (pathBase ?? "").TrimEnd('/');

            foreach (var url in urls)
            {
                if (resultado.Count >= MaxPaneles) break;
                if (string.IsNullOrWhiteSpace(url)) continue;

                var ruta = url.Trim();
                // Solo rutas relativas del mismo sitio: "//host/..." y "http://..." quedan afuera.
                if (!ruta.StartsWith("/", StringComparison.Ordinal) || ruta.StartsWith("//", StringComparison.Ordinal)) continue;

                // Sin query ni fragmento.
                int corte = ruta.IndexOfAny(new[] { '?', '#' });
                if (corte >= 0) ruta = ruta.Substring(0, corte);

                if (baseNormalizada.Length > 0 && ruta.StartsWith(baseNormalizada, StringComparison.OrdinalIgnoreCase))
                {
                    var resto = ruta.Substring(baseNormalizada.Length);
                    // El prefijo debe terminar en limite de segmento ("/app" no es prefijo de "/apple/x").
                    if (resto.Length == 0 || resto[0] == '/') ruta = resto.Length == 0 ? "/" : resto;
                }

                var segmentos = ruta.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segmentos.Length > 2) segmentos = new[] { segmentos[0], segmentos[1] };   // el resto son parametros de ruta (id)
                if (!EsNombreValido(segmentos, out var controller, out var accion)) continue;

                resultado.Add((controller, accion));
            }

            return resultado;
        }

        // "/" -> Home/Index; "/Stock" -> Stock/Index; "/Stock/Index/5" -> Stock/Index.
        private static bool EsNombreValido(string[] segmentos, out string controller, out string accion)
        {
            controller = "Home";
            accion = "Index";
            if (segmentos.Length == 0) return true;

            controller = segmentos[0];
            accion = segmentos.Length > 1 ? segmentos[1] : "Index";
            return SoloAlfanumerico(controller) && SoloAlfanumerico(accion);
        }

        private static bool SoloAlfanumerico(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            foreach (var c in texto)
            {
                if (!(c >= 'a' && c <= 'z') && !(c >= 'A' && c <= 'Z') && !(c >= '0' && c <= '9') && c != '_' && c != '-') return false;
            }
            return true;
        }
    }
}
