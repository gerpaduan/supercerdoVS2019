using System.Web.Mvc;

namespace Web.Controllers
{
    public class ErrorController : Controller
    {
        [AllowAnonymous]
        public ActionResult NotFound()
        {
            Response.StatusCode = 404;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.RequestedUrl = Request != null ? Request.RawUrl : "";
            return View();
        }

        [AllowAnonymous]
        public ActionResult General()
        {
            Response.StatusCode = 500;
            Response.TrySkipIisCustomErrors = true;
            ViewBag.RequestedUrl = Request != null ? Request.RawUrl : "";
            return View();
        }
    }
}
