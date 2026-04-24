using System;
using System.Web;
using Utilidades;

namespace Web.Helpers
{
    public static class PermisosHelper
    {
        /// <summary>
        /// De aquí se llama a Negocio.Usuario
        /// Se valida si el oUsuario tiene permiso en el formulario, por defecto pasar Fecha Actual,
        /// idCreador, pasar -1 si no se quiere verificar la Edicion
        /// </summary>
        /// <param name="oUser"></param>
        /// <param name="nombreForm"></param>
        /// <param name="fechaDesde"></param>
        /// <returns></returns>
        public static bool TienePermiso(HttpSessionStateBase session, string permiso, DateTime? fechaDesde, int idCreador = -1)
        {
            var fechaDesde_ = fechaDesde ?? DateTime.Today;
            var user = ObtenerUsuario(session);

            if (user == null)
                return false;

            // EmpresaContextWeb lee Session["IdEmpresa"]
            IEmpresaContext empresa = new EmpresaContextWeb();

            return TienePermiso(user, empresa, permiso, fechaDesde_, idCreador);
        }

        public static bool TienePermisoVer(HttpSessionStateBase session, string permiso, DateTime? fechaDesde = null)
        {
            return TienePermiso(session, permiso, fechaDesde, -1);
        }

        public static bool TienePermisoEditar(HttpSessionStateBase session, string permiso, DateTime fechaDesde, int idCreador)
        {
            return TienePermiso(session, permiso, fechaDesde, idCreador);
        }

        public static bool TienePermiso(Entidades.Usuario user, IEmpresaContext empresa, string permiso, DateTime fechaDesde, int idCreador = -1)
        {
            if (user == null || empresa == null)
                return false;

            var oUsuarioN = new Negocio.Usuario(empresa);

            return oUsuarioN.tienePermiso(
                user,
                permiso,
                fechaDesde,
                idCreador
            );
        }

        public static Entidades.Usuario ObtenerUsuario(HttpSessionStateBase session)
        {
            return session == null ? null : session["Usuario"] as Entidades.Usuario;
        }
    }
}
