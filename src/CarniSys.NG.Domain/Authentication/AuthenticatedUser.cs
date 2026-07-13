using CarniSys.NG.Domain.Companies;
using CarniSys.NG.Domain.Permissions;

namespace CarniSys.NG.Domain.Authentication;

public sealed record AuthenticatedUser(
    int UserId,
    UserLoginName Login,
    string DisplayName,
    CompanyContext Company,
    BranchContext ActiveBranch,
    IReadOnlyCollection<PermissionGrant> Permissions,
    bool IsAdministrator = false);
