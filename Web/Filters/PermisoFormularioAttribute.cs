using System;
using System.Web;
using System.Web.Mvc;
using Entidades;
using Negocio;
using Utilidades;
using Web; // donde está EmpresaContextWeb

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
            var session = filterContext.HttpContext.Session;

            var usuario = session["Usuario"] as Entidades.Usuario;

            // 1. No logueado → Login
            if (usuario == null)
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }

            // 2. Empresa desde Session (como BaseController)
            IEmpresaContext empresa;
            try
            {
                empresa = new EmpresaContextWeb();
            }
            catch
            {
                filterContext.Result = new RedirectResult("/Login/Index");
                return;
            }

            // 3. Negocio con empresa
            var usuarioNeg = new Negocio.Usuario(empresa);

            // 4. Fecha desde
            DateTime fechaDesde = DateTime.Today;

            // 5. idCreador
            int idCreador = _validarEdicion ? usuario.Id : -1;

            bool tiene = usuarioNeg.tienePermiso(
                usuario,
                _nombreForm,
                fechaDesde,
                idCreador
            );

            if (!tiene)
            {
                filterContext.Result = new RedirectResult("/Home/SinPermiso");
            }
        }
    }
}
