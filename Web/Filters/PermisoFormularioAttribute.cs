using System;
using System.Web;
using System.Web.Mvc;
using Entidades;
using Negocio; // donde está tu clase que tiene tienePermiso()

namespace Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PermisoFormularioAttribute : ActionFilterAttribute
    {
        private readonly string _nombreForm;
        private readonly bool _validarEdicion;

        public PermisoFormularioAttribute(string nombreForm, bool validarEdicion = false)
        {
            _nombreForm = nombreForm;
            _validarEdicion = validarEdicion;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var sessionUser = filterContext.HttpContext.Session["usuario"] as Entidades.Usuario;

            // 1. No logueado → Login
            if (sessionUser == null)
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }

            // 2. Objeto de negocio que contiene tienePermiso()
            var usuarioNeg = new Negocio.Usuario();

            // 3. Fecha desde: si no aplica edición, se pasa Today
            DateTime fechaDesde = DateTime.Today;

            // 4. idCreador: si solo miramos consulta → se pasa -1
            var usuario = (Entidades.Usuario)HttpContext.Current.Session["Usuario"];
            int idUsuario = usuario.Id;

            int idCreador = _validarEdicion ? idUsuario : -1;

            bool tiene = usuarioNeg.tienePermiso(sessionUser, _nombreForm, fechaDesde, idCreador);

            if (!tiene)
            {
                filterContext.Result = new RedirectResult("/Home/SinPermiso");
            }
        }
    }
}
