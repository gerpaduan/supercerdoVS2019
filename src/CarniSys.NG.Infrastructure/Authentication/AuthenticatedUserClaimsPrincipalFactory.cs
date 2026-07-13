using System.Security.Claims;
using CarniSys.NG.Domain.Authentication;

namespace CarniSys.NG.Infrastructure;

public static class AuthenticatedUserClaimsPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUser user, string authenticationType)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.DisplayName),
            new(ClaimTypes.GivenName, user.Login.Value),
            new(CarniSysNgClaimTypes.CompanyId, user.Company.CompanyId.ToString()),
            new(CarniSysNgClaimTypes.CompanyName, user.Company.Name),
            new(CarniSysNgClaimTypes.BranchId, user.ActiveBranch.BranchId.ToString()),
            new(CarniSysNgClaimTypes.BranchName, user.ActiveBranch.Name),
            new(CarniSysNgClaimTypes.IsAdministrator, user.IsAdministrator.ToString())
        };

        foreach (var permission in user.Permissions)
        {
            claims.Add(new Claim(
                CarniSysNgClaimTypes.Permission,
                $"{permission.Resource}|{permission.ReadWindowInDays}|{permission.EditWindowInDays}|{(int)permission.OwnershipScope}"));
        }

        var identity = new ClaimsIdentity(claims, authenticationType);
        return new ClaimsPrincipal(identity);
    }
}
