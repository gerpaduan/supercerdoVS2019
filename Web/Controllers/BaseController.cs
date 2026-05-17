using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;

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

        protected ActionResult VistaAccesoDenegado(string seccion, string permiso = null, System.DateTime? fecha = null, int idCreador = -1)
        {
            ViewBag.Title = seccion;
            ViewBag.Seccion = seccion;
            ViewBag.MensajePermiso = ConstruirMensajePermisoFecha(permiso, fecha, idCreador);
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        protected string ConstruirMensajePermisoFecha(string permiso, System.DateTime? fecha, int idCreador = -1)
        {
            if (string.IsNullOrWhiteSpace(permiso) || !fecha.HasValue)
                return null;

            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Value.Date >= fechaMinima.Value.Date)
                return null;

            return idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected bool AjustarFechaSiNoTienePermiso(string permiso, ref System.DateTime fecha, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue || fecha.Date >= fechaMinima.Value.Date)
                return false;

            fecha = fechaMinima.Value.Date;
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Permisos";
            TempData["AlertMsg"] = idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
            return true;
        }

        protected void ConfigurarAdvertenciaFechaEnVivo(string inputId, string permiso, int idCreador = -1)
        {
            var fechaMinima = PermisosHelper.ObtenerFechaMinimaPermitida(Session, permiso, idCreador);
            if (!fechaMinima.HasValue)
                return;

            ViewBag.PermisoFechaInputId = inputId;
            ViewBag.PermisoFechaMinimaIso = fechaMinima.Value.ToString("yyyy-MM-dd");
            ViewBag.PermisoFechaMinimaIsoDateTime = fechaMinima.Value.ToString("yyyy-MM-ddT00:00:00");
            ViewBag.PermisoFechaMensaje = idCreador >= 0
                ? "No tiene permiso para crear o modificar registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + "."
                : "No tiene permiso para ver registros anteriores a " + fechaMinima.Value.ToString("dd/MM/yyyy") + ".";
        }

        protected string RenderPartialViewToString(string viewName, object model)
        {
            if (!string.IsNullOrWhiteSpace(viewName))
            {
                ViewData.Model = model;
            }

            using (var sw = new StringWriter())
            {
                ViewEngineResult viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                if (viewResult.View == null)
                    throw new InvalidOperationException("No se encontró la vista parcial '" + viewName + "'.");

                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
