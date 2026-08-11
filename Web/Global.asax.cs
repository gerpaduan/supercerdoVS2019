using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
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
            ConfigurarLogDeErrores();
        }

        // Escucha global de Trace.TraceError (usado en ErrorController y ProductosController) hacia un
        // archivo en App_Data -- sin esto, esos logs no persistian a ningun lado (deuda documentada en
        // docs/DECISIONS.md, 2026-08-11). Se resuelve la ruta fisica igual que PerformanceInstrumentation
        // (HttpRuntime.AppDomainAppPath en vez de Server.MapPath: Application_Start corre sin HttpContext).
        private static void ConfigurarLogDeErrores()
        {
            try
            {
                string relativePath = ConfigurationManager.AppSettings["ErrorLogPath"];
                if (string.IsNullOrWhiteSpace(relativePath))
                    relativePath = "~/App_Data/error-web.log";

                string appRoot = HttpRuntime.AppDomainAppPath ?? AppDomain.CurrentDomain.BaseDirectory;
                string relativeToRoot = relativePath.Replace("~/", "").Replace('/', Path.DirectorySeparatorChar);
                string physicalPath = Path.Combine(appRoot, relativeToRoot);

                string directory = Path.GetDirectoryName(physicalPath);
                if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Trace.Listeners.Add(new TextWriterTraceListener(physicalPath, "ErrorFileListener"));
                Trace.AutoFlush = true;
            }
            catch
            {
                // No bloquear el arranque de la app si el logging de errores no se pudo configurar.
            }
        }

        protected void Application_End()
        {
            Trace.Flush();
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
