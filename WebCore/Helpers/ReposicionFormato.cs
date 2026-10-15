// Formato de números, importes y fechas relativas de las pantallas de Reposicion ("Qué pedir" y "Sugerir puntos de stock"),
// en es-AR. Lo usan las vistas parciales (_Tabla, _TablaPuntos) para no repetir las mismas funciones en cada una.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class ReposicionFormato
    {
        private static readonly CultureInfo Cultura = new CultureInfo("es-AR");

        // Cantidad con los decimales justos (5,734 / 20): sin ceros de relleno.
        public static string Cant(float valor)
        {
            return valor.ToString("0.###", Cultura);
        }

        // Un solo decimal (días que alcanza, unidades aproximadas).
        public static string Decimal1(float valor)
        {
            return valor.ToString("0.#", Cultura);
        }

        public static string Unidad(bool pesable)
        {
            return pesable ? "kg" : "un.";
        }

        // Importe en pesos sin centavos ($ 1.234).
        public static string Plata(decimal valor)
        {
            return valor.ToString("C0", Cultura);
        }

        // "hoy" / "ayer" / "hace 8 d" / "hace 3 meses".
        public static string Hace(int dias)
        {
            if (dias <= 0) return "hoy";
            if (dias == 1) return "ayer";
            if (dias < 60) return "hace " + dias + " d";
            if (dias < 730) return "hace " + (dias / 30) + " meses";
            return "hace más de 2 años";
        }
    }
}
