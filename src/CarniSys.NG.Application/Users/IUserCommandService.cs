namespace CarniSys.NG.Application.Users;

public interface IUserCommandService
{
    Task<SaveUserResult> SaveUserAsync(
        SaveUserRequest request,
        CancellationToken cancellationToken = default);
}
