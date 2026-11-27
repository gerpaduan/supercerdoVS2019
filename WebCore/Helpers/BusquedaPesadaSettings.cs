// Limites a partir de los cuales Ventas y Lineas de venta advierten antes de cargar (appSettings "BusquedaPesada:*" de
// App.config). Mismo mecanismo que ExistenciaCargaSettings: sin clave vale el default; un valor invalido o fuera de rango
// se recorta. 0 apaga el aviso de esa pantalla. Que hace cada uno: docs/DECISIONS.md 2026-11-26 y el manual.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class BusquedaPesadaSettings
    {
        // Calibrados el 2026-11-27 en la PC de desarrollo (apuntan a unos 6 segundos de espera): ver docs/DECISIONS.md.
        public const int LimiteVentasDefault = 5000;
        public const int LimiteLineasDefault = 1500;
        private const int LimiteMaximo = 10000000;

        // Ventas/Index: cantidad de ventas del periodo a partir de la cual se pregunta antes de cargar.
        public static int LimiteVentas => Interpretar(Leer("BusquedaPesada:LimiteVentas"), LimiteVentasDefault);

        // Ventas/Lineas: cantidad de lineas (filas de la tabla) a partir de la cual se pregunta.
        public static int LimiteLineas => Interpretar(Leer("BusquedaPesada:LimiteLineas"), LimiteLineasDefault);

        // Texto de la configuracion -> limite valido.
        public static int Interpretar(string texto, int porDefecto)
        {
            if (!int.TryParse((texto ?? "").Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < 0) return 0;
            return valor > LimiteMaximo ? LimiteMaximo : valor;
        }

        // Se advierte solo si el aviso esta encendido (limite > 0) y la cantidad SUPERA el limite.
        public static bool EsPesada(long cantidad, int limite) => limite > 0 && cantidad > limite;

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }
    }
}
