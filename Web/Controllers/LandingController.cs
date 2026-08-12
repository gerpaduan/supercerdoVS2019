using System.Web.Mvc;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class LandingController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            return View("~/Views/Landing/Index.cshtml");
        }
    }
}
