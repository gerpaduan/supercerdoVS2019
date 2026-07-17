using System;
using System.Web;
using System.Web.Security;

namespace Web.Helpers
{
    internal static class SessionSecurityHelper
    {
        public static void RenewAuthenticatedSession(HttpContextBase context, string userName)
        {
            if (context == null)
                return;

            FormsAuthentication.SignOut();

            var session = context.Session;
            if (session != null)
                session.Clear();

            FormsAuthentication.SetAuthCookie(userName ?? string.Empty, false);
        }

        public static void ClearAuthentication(HttpContextBase context)
        {
            FormsAuthentication.SignOut();

            if (context == null)
                return;

            ExpireCookie(context.Response, FormsAuthentication.FormsCookieName);
            ExpireCookie(context.Response, "ASP.NET_SessionId");
        }

        private static void ExpireCookie(HttpResponseBase response, string cookieName)
        {
            if (response == null || string.IsNullOrWhiteSpace(cookieName))
                return;

            var cookie = new HttpCookie(cookieName, string.Empty)
            {
                Expires = DateTime.UtcNow.AddDays(-1),
                HttpOnly = true,
                Path = FormsAuthentication.FormsCookiePath
            };

            response.Cookies.Set(cookie);
        }
    }
}
