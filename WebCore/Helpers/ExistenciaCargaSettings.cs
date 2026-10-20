// Limites de la actualizacion automatica de Existencia por sucursales, leidos de App.config (appSettings, claves
// "ExistenciaCarga:*"). Mismo mecanismo que BorradorGenericoSettings: sin clave valen los defaults de abajo; un valor
// invalido o fuera de rango se recorta al limite en vez de romper la pantalla. Nada hardcodeado en la logica
// (CLAUDE.md 1.1). Que hace cada uno: docs/DECISIONS.md 2026-10-19 y el manual de la pantalla.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class ExistenciaCargaSettings
    {
        private const int MaxConcurrentesDefault = 2;
        private const int UmbralLatenciaMsDefault = 5000;
        private const int ReintentoSegundosDefault = 120;
        private const int JitterInicialMaxSegundosDefault = 5;

        // Calculos de stock simultaneos a partir de los cuales las actualizaciones AUTOMATICAS se postergan.
        // 0 apaga todas las automaticas (sirve para probar el cartel "Servidor ocupado"). El Buscar manual nunca se rechaza.
        public static int MaxConcurrentes => LeerEntero("ExistenciaCarga:MaxConcurrentes", MaxConcurrentesDefault, 0, 50);

        // Si el ultimo calculo tardo mas que esto (ms), el servidor se considera cargado y las automaticas esperan.
        public static int UmbralLatenciaMs => LeerEntero("ExistenciaCarga:UmbralLatenciaMs", UmbralLatenciaMsDefault, 500, 120000);

        // Cada cuantos segundos reintenta una automatica rechazada (y cuanto dura la espera tras un calculo lento).
        public static int ReintentoSegundos => LeerEntero("ExistenciaCarga:ReintentoSegundos", ReintentoSegundosDefault, 10, 3600);

        // La 1.a busqueda automatica de cada pantalla espera entre 0 y esto (s) al azar, para que las pantallas abiertas a la
        // vez (apertura del turno) no lleguen todas juntas. 0 = sin espera.
        public static int JitterInicialMaxSegundos => LeerEntero("ExistenciaCarga:JitterInicialMaxSegundos", JitterInicialMaxSegundosDefault, 0, 60);

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }

        // Si falta, es invalido o cae fuera de [min, max], usa el default (o el limite).
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
