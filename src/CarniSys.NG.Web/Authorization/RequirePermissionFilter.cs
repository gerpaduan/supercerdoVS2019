using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Application.Session;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarniSys.NG.Web.Authorization;

public sealed class RequirePermissionFilter(
    string resource,
    PermissionMode mode,
    IUserSessionAccessor userSessionAccessor,
    IPermissionService permissionService) : IAsyncAuthorizationFilter
{
    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var currentUser = userSessionAccessor.CurrentUser;
        if (currentUser is null)
        {
            context.Result = new ChallengeResult();
            return Task.CompletedTask;
        }

        var operationDate = DateOnly.FromDateTime(DateTime.Today);
        var isAuthorized = mode == PermissionMode.Read
            ? permissionService.CanRead(currentUser, resource, operationDate)
            : permissionService.CanEdit(currentUser, resource, operationDate, currentUser.UserId);

        if (!isAuthorized)
        {
            context.Result = new ForbidResult();
        }

        return Task.CompletedTask;
    }
}
