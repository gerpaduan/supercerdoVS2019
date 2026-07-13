using System.Security.Claims;
using CarniSys.NG.Application.Session;
using CarniSys.NG.Domain.Authentication;
using CarniSys.NG.Domain.Companies;
using CarniSys.NG.Domain.Permissions;
using Microsoft.AspNetCore.Http;

namespace CarniSys.NG.Infrastructure;

internal sealed class HttpContextUserSessionAccessor(IHttpContextAccessor httpContextAccessor) : IUserSessionAccessor
{
    public AuthenticatedUser? CurrentUser
    {
        get
        {
            var principal = httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            var userId = ParseInt(principal, ClaimTypes.NameIdentifier);
            var companyId = ParseInt(principal, CarniSysNgClaimTypes.CompanyId);
            var branchId = ParseInt(principal, CarniSysNgClaimTypes.BranchId);
            var displayName = principal.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
            var login = principal.FindFirstValue(ClaimTypes.GivenName) ?? displayName;
            var companyName = principal.FindFirstValue(CarniSysNgClaimTypes.CompanyName) ?? "Empresa";
            var branchName = principal.FindFirstValue(CarniSysNgClaimTypes.BranchName) ?? "Sucursal";
            var isAdministrator = bool.TryParse(principal.FindFirstValue(CarniSysNgClaimTypes.IsAdministrator), out var adminValue) && adminValue;

            if (userId <= 0 || companyId <= 0 || branchId <= 0)
            {
                return null;
            }

            var permissions = principal.Claims
                .Where(x => x.Type == CarniSysNgClaimTypes.Permission)
                .Select(ParsePermission)
                .Where(x => x is not null)
                .Cast<PermissionGrant>()
                .ToArray();

            return new AuthenticatedUser(
                userId,
                new UserLoginName(login),
                displayName,
                new CompanyContext(companyId, companyName),
                new BranchContext(branchId, branchName),
                permissions,
                isAdministrator);
        }
    }

    private static int ParseInt(ClaimsPrincipal principal, string claimType)
    {
        var rawValue = principal.FindFirstValue(claimType);
        return int.TryParse(rawValue, out var value) ? value : 0;
    }

    private static PermissionGrant? ParsePermission(Claim claim)
    {
        var parts = (claim.Value ?? string.Empty).Split('|');
        if (parts.Length != 4)
        {
            return null;
        }

        if (!int.TryParse(parts[1], out var readWindowInDays)
            || !int.TryParse(parts[2], out var editWindowInDays)
            || !int.TryParse(parts[3], out var ownershipScopeValue)
            || !Enum.IsDefined(typeof(RecordOwnershipScope), ownershipScopeValue))
        {
            return null;
        }

        return new PermissionGrant(parts[0], readWindowInDays, editWindowInDays, (RecordOwnershipScope)ownershipScopeValue);
    }
}
