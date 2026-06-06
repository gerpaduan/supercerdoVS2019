using System;
using System.Web;
using System.Web.SessionState;

namespace Web.Helpers
{
    public static class SystemAdministrationAccessHelper
    {
        private const string UsuarioPrincipal = "german";
        private const string EmailPrincipal = "germanpaduan@gmail.com";

        public static bool PuedeAdministrarSistema(HttpSessionStateBase session)
        {
            var usuario = session != null ? session["Usuario"] as Entidades.Usuario : null;
            if (usuario == null)
                return false;

            if (string.Equals((usuario.User ?? "").Trim(), UsuarioPrincipal, StringComparison.OrdinalIgnoreCase))
                return true;

            if (string.Equals((usuario.Email ?? "").Trim(), EmailPrincipal, StringComparison.OrdinalIgnoreCase))
                return true;

            try
            {
                var repo = new SystemAdministrationRepository();
                return repo.EsSuperAdmin(usuario.Id);
            }
            catch
            {
                return false;
            }
        }

        public static bool PuedeAdministrarSistema(HttpSessionState session)
        {
            return session != null && PuedeAdministrarSistema(new HttpSessionStateWrapper(session));
        }
    }
}
