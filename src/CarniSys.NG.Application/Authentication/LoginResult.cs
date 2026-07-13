using CarniSys.NG.Domain.Authentication;

namespace CarniSys.NG.Application.Authentication;

public sealed record LoginResult(bool IsAuthenticated, AuthenticatedUser? User, string? FailureMessage)
{
    public static LoginResult Success(AuthenticatedUser user) => new(true, user, null);

    public static LoginResult Failure(string failureMessage) => new(false, null, failureMessage);
}
