using System.Web.Mvc;
using Utilidades;

namespace Web.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IEmpresaContext empresa;
        protected IParametrosContext param;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session["Usuario"] == null)
            {
                filterContext.Result = RedirectToAction("Index", "Login");
                return;
            }

            // ✅ tu EmpresaContextWeb actual (usa Session["IdEmpresa"])
            empresa = new EmpresaContextWeb();

            // ✅ cargar parámetros por empresa una sola vez (por sesión)
            param = Session["PARAM_CTX"] as IParametrosContext;
            if (param == null)
            {
                param = new Negocio.Parametros(empresa);
                param.Reload(); // opcional
                Session["PARAM_CTX"] = param;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
