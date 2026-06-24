using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Utilidades;

namespace Web.Controllers
{
    [AllowAnonymous]
    [SessionState(SessionStateBehavior.ReadOnly)]
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

        [HttpPost]
        public ActionResult ClientPerf(string categoria, string nombre, long? totalMs, string detalle, string reqId, string url)
        {
            PerformanceInstrumentation.LogClientEvent(
                categoria,
                nombre,
                totalMs ?? 0,
                detalle,
                reqId,
                url);

            return new HttpStatusCodeResult(204);
        }

    }
}
