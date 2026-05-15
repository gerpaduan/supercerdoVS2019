using System.Web.Mvc;

namespace Web.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.StatusCode = 200;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.RequestedUrl = Request != null ? Request.RawUrl : "";
            return View();
        }

        [AllowAnonymous]
        public ActionResult General()
        {
            Response.StatusCode = 200;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.RequestedUrl = Request != null ? Request.RawUrl : "";
            var ex = Server != null ? Server.GetLastError() : null;
            if (ex != null)
            {
                ViewBag.ErrorMessage = ex.Message;
                ViewBag.ErrorType = ex.GetType().FullName;
                ViewBag.ErrorStack = ex.StackTrace;
                ViewBag.InnerErrorMessage = ex.InnerException != null ? ex.InnerException.Message : "";
                ViewBag.InnerErrorType = ex.InnerException != null ? ex.InnerException.GetType().FullName : "";
            }
            return View();
        }
    }
}
