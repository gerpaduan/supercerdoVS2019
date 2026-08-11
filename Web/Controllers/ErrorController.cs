using System;
using System.Web.Mvc;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class ErrorController : Controller
    {
        public ActionResult NotFound()
        {
            Response.TrySkipIisCustomErrors = true;
            var rawUrl = Request != null ? Request.RawUrl : "";
            if (EsRutaRaiz(rawUrl))
            {
                Response.StatusCode = 200;
                return Content("No se pudo cargar la aplicacion. Volve a intentar en unos segundos.", "text/plain");
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult General()
        {
            Response.TrySkipIisCustomErrors = true;
            var rawUrl = Request != null ? Request.RawUrl : "";
            var ex = Server != null ? Server.GetLastError() : null;
            if (ex != null)
            {
                System.Diagnostics.Trace.TraceError("Error/General - url={0}: {1}", rawUrl, ex);
            }
            if (EsRutaRaiz(rawUrl))
            {
                Response.StatusCode = 200;
                return Content("No se pudo cargar la aplicacion. Volve a intentar en unos segundos.", "text/plain");
            }
            return RedirectToAction("Index", "Home");
        }

        // Evita un loop infinito de redirects si la excepcion ocurre en el Home mismo.
        private static bool EsRutaRaiz(string rawUrl)
        {
            if (string.IsNullOrWhiteSpace(rawUrl)) return true;
            var path = rawUrl.Split('?')[0].TrimEnd('/');
            return path == "" || path.Equals("/Home", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/Home/Index", StringComparison.OrdinalIgnoreCase);
        }
    }
}
