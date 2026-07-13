using CarniSys.NG.Domain.Authentication;

namespace CarniSys.NG.Application.Permissions;

public interface IPermissionService
{
    bool CanRead(AuthenticatedUser user, string resource, DateOnly operationDate);

    bool CanEdit(AuthenticatedUser user, string resource, DateOnly operationDate, int ownerUserId);
}
