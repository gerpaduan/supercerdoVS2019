using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Domain.Authentication;
using CarniSys.NG.Domain.Companies;
using CarniSys.NG.Domain.Permissions;

namespace CarniSys.NG.Infrastructure;

internal sealed class PlaceholderAuthenticationService : IAuthenticationService
{
    public Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(LoginResult.Failure("Usuario y contrasena son obligatorios."));
        }

        var user = new AuthenticatedUser(
            1,
            new UserLoginName(request.UserName),
            "Usuario NG",
            new CompanyContext(1, "Empresa NG"),
            new BranchContext(1, "Sucursal Central"),
            new[]
            {
                new PermissionGrant("productos", 30, 7, RecordOwnershipScope.All),
                new PermissionGrant("pos", 0, 0, RecordOwnershipScope.All)
            },
            false);

        return Task.FromResult(LoginResult.Success(user));
    }

    public Task<AuthenticatedUser?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return Task.FromResult<AuthenticatedUser?>(null);
        }

        AuthenticatedUser user = new(
            userId,
            new UserLoginName($"user{userId}"),
            "Usuario NG",
            new CompanyContext(1, "Empresa NG"),
            new BranchContext(1, "Sucursal Central"),
            new[]
            {
                new PermissionGrant("productos", 30, 7, RecordOwnershipScope.All),
                new PermissionGrant("pos", 0, 0, RecordOwnershipScope.All)
            },
            false);

        return Task.FromResult<AuthenticatedUser?>(user);
    }
}
