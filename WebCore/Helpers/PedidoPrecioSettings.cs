// Limite de antiguedad del precio de compra que usa un pedido para estimar importes, leido de App.config (appSettings, clave
// "Pedidos:DiasPrecioViejo"). Mismo mecanismo que ExistenciaCargaSettings: sin clave vale el default; un valor invalido o fuera de
// rango se recorta al limite en vez de romper la pantalla (CLAUDE.md 1.1: nada hardcodeado en la logica). Que hace: docs/DECISIONS.md (2026-10-19, Pedidos).
using System.Globalization;

namespace WebCore.Helpers
{
    public static class PedidoPrecioSettings
    {
        private const int DiasPrecioViejoDefault = 90;
        private const int DiasPrecioMaximoDefault = 365;

        // Un precio de ultima compra con mas de estos dias (respecto de la fecha del pedido) se marca como "viejo" en pantallas y archivos.
        public static int DiasPrecioViejo => LeerEntero("Pedidos:DiasPrecioViejo", DiasPrecioViejoDefault, 1, 3650);

        // Un precio de una compra de hace MAS de estos dias (respecto de hoy) NO se usa para estimar el importe: el producto queda "sin precio".
        // Evita que precios de hace años (inflacion) subestimen el total. Va aparte de "viejo": viejo avisa y suma; maximo no suma.
        public static int DiasPrecioMaximo => LeerEntero("Pedidos:DiasPrecioMaximo", DiasPrecioMaximoDefault, 30, 36500);

        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            string texto = (System.Configuration.ConfigurationManager.AppSettings[clave] ?? "").Trim();
            if (!int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor)) return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
