using System;
using System.Web;
using System.Web.Mvc;
using Entidades;
using Negocio;
using Utilidades;
using Web; // EmpresaContextWeb

namespace Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequirePermissionAttribute : AuthorizeAttribute
    {
        private readonly bool _validarEdicion;

        public RequirePermissionAttribute(bool validarEdicion = false)
        {
            _validarEdicion = validarEdicion;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            // 1. Permite anónimo
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true) ||
                filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true))
            {
                return;
            }

            var session = filterContext.HttpContext.Session;
            var usuario = session["Usuario"] as Entidades.Usuario;

            // 2. Usuario no logueado
            if (usuario == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Index" }
                    });
                return;
            }

            // 3. Crear contexto empresa (igual que BaseController)
            IEmpresaContext empresa;
            try
            {
                empresa = new EmpresaContextWeb();
            }
            catch
            {
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Index" }
                    });
                return;
            }

            // 4. Datos de validación
            string nombreForm = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            DateTime fechaDesde = DateTime.Today;
            int idCreador = _validarEdicion ? usuario.Id : -1;

            // 5. Negocio con empresa
            var seguridadNegocio = new Negocio.Usuario(empresa);

            bool tienePermiso = seguridadNegocio.tienePermiso(
                usuario,
                nombreForm,
                fechaDesde,
                idCreador
            );

            if (!tienePermiso)
            {
                filterContext.Result = new ViewResult
                {
                    ViewName = "~/Views/Shared/AccesoDenegado.cshtml"
                };
            }
        }
    }
}
