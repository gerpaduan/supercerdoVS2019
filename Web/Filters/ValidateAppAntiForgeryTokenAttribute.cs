using System;
using System.Linq;
using System.Net;
using System.Web.Helpers;
using System.Web.Mvc;

namespace Web.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class ValidateAppAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        private static readonly string[] UnsafeMethods = { "POST", "PUT", "PATCH", "DELETE" };

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            var request = filterContext.HttpContext.Request;
            if (request == null || !UnsafeMethods.Contains((request.HttpMethod ?? "").ToUpperInvariant()))
                return;

            if (HasAttribute<SkipAppAntiForgeryValidationAttribute>(filterContext) ||
                HasAttribute<AllowAnonymousAttribute>(filterContext))
            {
                return;
            }

            string cookieToken = null;
            string formToken = request.Form["__RequestVerificationToken"];

            if (string.IsNullOrWhiteSpace(formToken))
                formToken = request.Headers["RequestVerificationToken"];

            var antiForgeryCookie = request.Cookies[AntiForgeryConfig.CookieName];
            if (antiForgeryCookie != null)
                cookieToken = antiForgeryCookie.Value;

            try
            {
                AntiForgery.Validate(cookieToken, formToken);
            }
            catch (HttpAntiForgeryException)
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.BadRequest, "La solicitud no pasó la validación de seguridad.");
            }
        }

        private static bool HasAttribute<T>(AuthorizationContext filterContext) where T : Attribute
        {
            return filterContext.ActionDescriptor.IsDefined(typeof(T), true) ||
                   filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(T), true);
        }
    }
}
