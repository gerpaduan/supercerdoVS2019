// Configuracion del POS sin conexion (2026-11-26), leida de App.config (appSettings, claves "PosOffline:*"). Mismo
// mecanismo que PosBorradorSettings. Con PosOffline:Habilitado apagado (default) no se registra el service worker, no
// se baja el catalogo y el servidor no toca la tabla posclaveunica: el POS se comporta igual que antes.
// Ver docs/DECISIONS.md "POS sin conexion" y docs/03-modulos/ventas-y-pos.md.
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace WebCore.Helpers
{
    public static class PosOfflineSettings
    {
        private const int MinutosToleranciaFuturoDefault = 10;
        private const int DiasMaximosAntiguedadDefault = 7;
        private const int MaxLineasDefault = 300;
        private const int MinutosRefrescoCatalogoDefault = 10;
        private const int FallasSeguidasParaCorteDefault = 2;
        private const int SegundosTimeoutPingDefault = 4;

        // Interruptor general. Apagado por defecto: hay que haber corrido antes la migracion de posclaveunica en esa base
        // (DatosPostgres/DB-Migrations/20261126-Create_posclaveunica.sql o su espejo de SQL Server).
        public static bool HabilitadoGlobal => LeerBool("PosOffline:Habilitado", false);

        // Sucursales donde rige (CSV de idSucursal). Vacio = todas. Sirve para prenderlo primero en un solo local.
        public static IReadOnlyCollection<int> SoloSucursales
        {
            get
            {
                return Leer("PosOffline:SoloSucursales")
                    .Split(',')
                    .Select(s => int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int id) ? id : 0)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
            }
        }

        // true si el POS sin conexion rige para esa sucursal.
        public static bool HabilitadoPara(int idSucursal)
        {
            if (!HabilitadoGlobal || idSucursal <= 0) return false;
            var solo = SoloSucursales;
            return solo.Count == 0 || solo.Contains(idSucursal);
        }

        // Minutos que la hora de la PC puede adelantar a la del servidor sin rechazar la venta.
        public static int MinutosToleranciaFuturo => LeerEntero("PosOffline:MinutosToleranciaFuturo", MinutosToleranciaFuturoDefault, 0, 240);

        // Una venta offline mas vieja que esto al llegar se rechaza y queda en la PC para que la revise un admin.
        public static int DiasMaximosAntiguedad => LeerEntero("PosOffline:DiasMaximosAntiguedad", DiasMaximosAntiguedadDefault, 1, 60);

        public static int MaxLineas => LeerEntero("PosOffline:MaxLineas", MaxLineasDefault, 10, 2000);

        // Cada cuantos minutos el POS vuelve a bajar el catalogo mientras hay conexion.
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
