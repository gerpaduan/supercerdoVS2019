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
            var user = session["Usuario"] as Entidades.Usuario;

            if (user == null)
                return false;

            // EmpresaContextWeb lee Session["IdEmpresa"]
            IEmpresaContext empresa = new EmpresaContextWeb();

            var oUsuarioN = new Negocio.Usuario(empresa);

            return oUsuarioN.tienePermiso(
                user,
                permiso,
                fechaDesde_,
                idCreador
            );
        }
    }
}
