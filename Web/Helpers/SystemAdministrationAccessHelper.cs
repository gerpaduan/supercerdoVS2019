using System;
using System.Web;
using System.Web.SessionState;

namespace Web.Helpers
{
    public static class SystemAdministrationAccessHelper
    {
        public static bool PuedeAdministrarSistema(HttpSessionStateBase session)
        {
            var usuario = session != null ? session["Usuario"] as Entidades.Usuario : null;
            if (usuario == null)
                return false;

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
