// Configuracion del login por huella (passkeys WebAuthn) leida de App.config, ver docs/DECISIONS.md
// "Login por huella (passkeys)". Nada hardcodeado: dominio y origenes son propios de cada ambiente.
using WebCore.Infrastructure;

namespace WebCore.Helpers
{
    public static class PasskeySettings
    {
        // Habilitado solo si: (a) el motor es Postgres (la tabla usuariopasskeys no existe en
        // SQL Server), (b) Passkeys:Enabled=true y (c) hay dominio y origenes configurados.
        // WebAuthn exige HTTPS con un hostname real (o localhost): en un ambiente por IP con
        // certificado autofirmado (Servidor SM, San Lorenzo) se deja Passkeys:Enabled sin activar.
        public static bool Habilitado =>
            NegocioFactory.UsarPostgres
            && string.Equals(Leer("Passkeys:Enabled"), "true", StringComparison.OrdinalIgnoreCase)
            && !string.IsNullOrWhiteSpace(ServerDomain)
            && Origenes.Count > 0;

        // "Relying Party ID": el dominio (sin esquema ni puerto). Las huellas quedan atadas a el, asi
        // que mientras el dominio no cambie sobreviven a mover el sitio de un servidor a otro.
        public static string ServerDomain => Leer("Passkeys:ServerDomain");

        // Nombre visible en el dialogo del navegador/sistema operativo.
        public static string ServerName
        {
            get
            {
                string nombre = Leer("Passkeys:ServerName");
                return string.IsNullOrWhiteSpace(nombre) ? "CarniSys" : nombre;
            }
        }

        // Origenes permitidos (esquema+host[+puerto]), separados por coma. Ej. "https://carnisys.com".
        public static HashSet<string> Origenes
        {
            get
            {
                var origenes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (string origen in Leer("Passkeys:Origins").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    origenes.Add(origen);
                return origenes;
            }
        }

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }
    }
}
