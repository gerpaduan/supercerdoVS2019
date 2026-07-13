using CarniSys.NG.Application.Permissions;
using CarniSys.NG.Domain.Authentication;
using CarniSys.NG.Domain.Permissions;

namespace CarniSys.NG.Infrastructure;

internal sealed class PermissionService : IPermissionService
{
    public bool CanRead(AuthenticatedUser user, string resource, DateOnly operationDate)
    {
        if (user.IsAdministrator)
        {
            return true;
        }

        var permission = FindPermission(user, resource);
        if (permission is null)
        {
            return false;
        }

        return IsWithinWindow(operationDate, permission.ReadWindowInDays);
    }

    public bool CanEdit(AuthenticatedUser user, string resource, DateOnly operationDate, int ownerUserId)
    {
        if (user.IsAdministrator)
        {
            return true;
        }

        var permission = FindPermission(user, resource);
        if (permission is null || !IsWithinWindow(operationDate, permission.EditWindowInDays))
        {
            return false;
        }

        return permission.OwnershipScope == RecordOwnershipScope.All || ownerUserId == user.UserId;
    }

    private static PermissionGrant? FindPermission(AuthenticatedUser user, string resource)
    {
        return user.Permissions.FirstOrDefault(x => string.Equals(x.Resource, resource, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsWithinWindow(DateOnly operationDate, int allowedDays)
    {
        if (allowedDays < 0)
        {
            return false;
        }

        if (allowedDays == 0)
        {
            return operationDate == DateOnly.FromDateTime(DateTime.Today);
        }

        var oldestAllowed = DateOnly.FromDateTime(DateTime.Today.AddDays(-allowedDays));
        return operationDate >= oldestAllowed;
    }
}
