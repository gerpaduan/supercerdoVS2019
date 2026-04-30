using System.Web;
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
            HttpSessionStateBase session = filterContext.HttpContext.Session;
            bool sesionInvalida = session == null
                || session["Usuario"] == null
                || session["IdEmpresa"] == null;

            if (sesionInvalida)
            {
                if (session != null)
                {
                    session.Remove("PARAM_CTX");
                    session.Remove("IdEmpresa");
                    session.Remove("Usuario");
                }

                TempData["Error"] = "La sesión venció o faltan datos de contexto. Iniciá sesión nuevamente.";
                string returnUrl = filterContext.HttpContext.Request.RawUrl ?? "";
                filterContext.Result = RedirectToAction("Index", "Login", new { returnUrl = returnUrl });
                return;
            }

            empresa = new EmpresaContextWeb();

            param = Session["PARAM_CTX"] as IParametrosContext;
            if (param == null)
            {
                param = new Negocio.Parametros(empresa);
                param.Reload();
                Session["PARAM_CTX"] = param;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
