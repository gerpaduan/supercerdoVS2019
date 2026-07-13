using Microsoft.AspNetCore.Mvc;

namespace CarniSys.NG.Web.Authorization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute : TypeFilterAttribute
{
    public RequirePermissionAttribute(string resource, PermissionMode mode)
        : base(typeof(RequirePermissionFilter))
    {
        Arguments = new object[] { resource, mode };
    }
}
