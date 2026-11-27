// Configuracion del POS sin conexion (2026-11-26), leida de App.config (appSettings, claves "PosOffline:*"). Mismo
// mecanismo que PosBorradorSettings. Viene HABILITADO en todas las sucursales (decision del usuario 2026-11-26):
// "PosOffline:Habilitado=false" es solo el freno de emergencia. Si la base no tiene la tabla posclaveunica la funcion
// queda apagada sola (ver PosOfflineDisponibilidad), nunca rompe las ventas.
// Ver docs/DECISIONS.md "POS sin conexion" y docs/03-modulos/ventas-y-pos.md.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class PosOfflineSettings
    {
        private const int DiasVigenciaDefault = 15;
        private const int HorasAvisoPreciosDefault = 24;
        private const int MinutosToleranciaFuturoDefault = 10;
        // Mayor que DiasVigencia: una PC puede vender sin conexion hasta DiasVigencia y subir las ventas despues.
        private const int DiasMaximosAntiguedadDefault = 20;
        private const int MaxLineasDefault = 300;
        private const int MinutosRefrescoCatalogoDefault = 10;
        private const int FallasSeguidasParaCorteDefault = 2;
        private const int SegundosTimeoutPingDefault = 4;

        // Freno de emergencia: prendido por defecto. Quien necesite saber si la funcion rige usa
        // PosOfflineDisponibilidad (que ademas mira que exista la tabla).
        public static bool Habilitado => LeerBool("PosOffline:Habilitado", true);

        // Dias que una PC puede vender sin conexion desde la ultima vez que un usuario inicio sesion (o uso el sistema
        // con la sesion abierta) en ella. Pasados, la PC borra las claves guardadas y no deja vender hasta conectarse.
        public static int DiasVigencia => LeerEntero("PosOffline:DiasVigencia", DiasVigenciaDefault, 1, 60);

        // Antiguedad de los precios guardados a partir de la cual la pantalla sin conexion avisa en rojo.
        public static int HorasAvisoPrecios => LeerEntero("PosOffline:HorasAvisoPrecios", HorasAvisoPreciosDefault, 1, 720);

        // Minutos que la hora de la PC puede adelantar a la del servidor sin rechazar la venta.
        public static int MinutosToleranciaFuturo => LeerEntero("PosOffline:MinutosToleranciaFuturo", MinutosToleranciaFuturoDefault, 0, 240);

        // Una venta offline mas vieja que esto al llegar se rechaza y queda en la PC para que la revise un admin.
        public static int DiasMaximosAntiguedad => LeerEntero("PosOffline:DiasMaximosAntiguedad", DiasMaximosAntiguedadDefault, 1, 90);

        public static int MaxLineas => LeerEntero("PosOffline:MaxLineas", MaxLineasDefault, 10, 2000);

        // Cada cuantos minutos una PC con sesion vuelve a bajar catalogo y usuarios mientras hay conexion.
        public static int MinutosRefrescoCatalogo => LeerEntero("PosOffline:MinutosRefrescoCatalogo", MinutosRefrescoCatalogoDefault, 1, 240);

        // Fallas de red seguidas antes de pasar a "sin conexion" (evita que una red lenta se tome como corte).
        public static int FallasSeguidasParaCorte => LeerEntero("PosOffline:FallasSeguidasParaCorte", FallasSeguidasParaCorteDefault, 1, 10);

        public static int SegundosTimeoutPing => LeerEntero("PosOffline:SegundosTimeoutPing", SegundosTimeoutPingDefault, 1, 30);

        // URL externa que el POS consulta (fetch no-cors) para distinguir "sin internet en el local" de "el servidor no
        // responde". Vacia = no se distingue (la franja dice "sin conexion" a secas). No se hardcodea ninguna.
        public static string UrlChequeoInternet => Leer("PosOffline:UrlChequeoInternet").Trim();

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }

        private static bool LeerBool(string clave, bool porDefecto)
        {
            string valor = Leer(clave).Trim();
            if (valor.Length == 0) return porDefecto;
            return string.Equals(valor, "true", System.StringComparison.OrdinalIgnoreCase);
        }

        // Entero de config; si falta, es invalido o cae fuera de [min, max], usa el default o el limite.
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            if (!int.TryParse(Leer(clave).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
