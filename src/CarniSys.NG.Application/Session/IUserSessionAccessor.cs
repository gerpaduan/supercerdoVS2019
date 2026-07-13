using CarniSys.NG.Domain.Authentication;

namespace CarniSys.NG.Application.Session;

public interface IUserSessionAccessor
{
    AuthenticatedUser? CurrentUser { get; }
}
