using CarniSys.NG.Domain.Authentication;

namespace CarniSys.NG.Application.Authentication;

public interface IAuthenticationService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<AuthenticatedUser?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
}
