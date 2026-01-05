using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class SessionController : Controller
    {
        [HttpGet]
        public ActionResult KeepAlive()
        {
            if (Session["Usuario"] == null)
            {
                Response.SuppressFormsAuthenticationRedirect = true;
                return new HttpStatusCodeResult(401);
            }

            return new HttpStatusCodeResult(200);
        }

    }
}