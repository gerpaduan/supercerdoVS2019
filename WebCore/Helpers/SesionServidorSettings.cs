// Duracion de la Session de ASP.NET Core (estado transitorio del servidor: autorizacion del operador de la cuenta de
// produccion por pestana de POS, "operar sin caja", rate-limits de claves). Leida de App.config (appSettings, clave
// "Sesion:IdleTimeoutMinutos"), mismo mecanismo que PosBorradorSettings. Ver docs/DECISIONS.md "Autorizacion del operador:
// Session de 12 h".
using System.Globalization;

namespace WebCore.Helpers
{
    public static class SesionServidorSettings
    {
        // Igual que la cookie de inicio de sesion (Program.cs, ExpireTimeSpan): con el default de ASP.NET (20 min) la
        // autorizacion del operador vencia en medio del turno aunque el login siguiera vigente.
        private const int IdleTimeoutMinutosDefault = 720;
        private const int IdleTimeoutMinutosMin = 5;
        private const int IdleTimeoutMinutosMax = 1440;

        // Minutos sin que el navegador toque la Session para que el servidor la descarte. Se renueva con cada request
        // autenticado (ver Program.cs), asi que cuenta desde la ultima actividad del POS.
        public static TimeSpan IdleTimeout => TimeSpan.FromMinutes(
            LeerEntero("Sesion:IdleTimeoutMinutos", IdleTimeoutMinutosDefault, IdleTimeoutMinutosMin, IdleTimeoutMinutosMax));

        // Si falta, es invalido o cae fuera de [min, max], usa el default (o el limite) en vez de romper el arranque.
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            string texto = (System.Configuration.ConfigurationManager.AppSettings[clave] ?? "").Trim();
            if (!int.TryParse(texto, NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
