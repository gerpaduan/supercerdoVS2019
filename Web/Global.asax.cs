using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Helpers;
using Utilidades;
using Web.Helpers;

namespace Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
        }

        protected void Application_BeginRequest()
        {
            if (SecurityRuntime.EnforceHttps && !Request.IsLocal && !SecurityRuntime.IsSecureRequest(Request))
            {
                var targetUrl = SecurityRuntime.BuildHttpsUrl(Request);
                if (!string.IsNullOrWhiteSpace(targetUrl))
                {
                    Response.StatusCode = 301;
                    Response.RedirectLocation = targetUrl;
                    CompleteRequest();
                    return;
                }
            }

            SecurityRuntime.ApplyCors(Context, SecurityRuntime.IsSecureRequest(Request));
            PerformanceInstrumentation.BeginWebRequest(HttpContext.Current);
        }

        protected void Application_EndRequest()
        {
            PerformanceInstrumentation.EndWebRequest(HttpContext.Current);
            SecurityRuntime.ApplyResponseHeaders(HttpContext.Current);
        }

        protected void Application_Error()
        {
            var exception = Server.GetLastError();
            if (!(exception is HttpAntiForgeryException))
                return;

            Server.ClearError();

            Response.Clear();
            Response.TrySkipIisCustomErrors = true;
            Response.StatusCode = 400;
            Response.ContentType = "text/plain";
            Response.Write("La solicitud no pasó la validación de seguridad.");
            CompleteRequest();
        }
    }
}
