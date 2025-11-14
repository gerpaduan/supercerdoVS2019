using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Entidades; // Ajustá según dónde tengas tu clase Usuario
using Negocio;

namespace Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        //[RequirePermission(validarEdicion: false)] // solo consulta
        //[RequirePermission(validarEdicion: true)]  // requiere permiso de edición
        private readonly bool _validarEdicion;

        public RequirePermissionAttribute(bool validarEdicion = false)
        {
            _validarEdicion = validarEdicion;
        }

        private int GetCurrentUserId()
        {
            var usuario = (Entidades.Usuario)HttpContext.Current.Session["Usuario"];
            return usuario.Id;
        }

        //public override void OnAuthorization(AuthorizationContext filterContext)
        //{
        //    var sessionUser = filterContext.HttpContext.Session["usuario"] as Usuario;

        //    // 1. Usuario no logueado → Login
        //    if (sessionUser == null)
        //    {
        //        filterContext.Result = new RedirectResult("/Login/Index");
        //        return;
        //    }

        //    // 2. No tiene ese permiso → Página sin permiso
        //    var tienePermiso = sessionUser.Permisos != null &&
        //                       sessionUser.Permisos.Any(p => p.Formulario == _permiso);

        //    if (!tienePermiso)
        //    {
        //        filterContext.Result = new RedirectResult("/Home/SinPermiso");
        //    }
        //}
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // 1. Si la acción permite acceso anónimo → no validar
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return;
            }

            // 2. Verificar si hay usuario logueado en Session
            var usuario = (Entidades.Usuario)filterContext.HttpContext.Session["Usuario"];

            if (usuario == null)
            {
                // Sesión expirada → redirigir al login
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                { "controller", "Home" },
                { "action", "AccesoDenegado" }
                    });
                return;
            }

            // 3. Nombre del formulario (generalmente coincide con el nombre del controlador)
            string nombreForm = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            // 4. Fecha desde (si no te la dan se puede usar hoy)
            DateTime fechaDesde = DateTime.Today;

            // 5. idCreador: si estamos validando edición, enviamos el ID del usuario actual,
            //    si no, enviamos -1 indicando que queremos validar SÓLO permiso de consulta.
            int idCreador = _validarEdicion ? usuario.Id : -1;

            var _seguridadNegocio = new Negocio.Usuario();
            // 6. Ejecutar la validación de permisos
            bool tienePermiso = _seguridadNegocio.tienePermiso(usuario, nombreForm, fechaDesde, idCreador);

            if (!tienePermiso)
            {
                // Sin permiso → mostrar página de acceso denegado
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccesoDenegado.cshtml"
                };
            }
        }

    }
}
