namespace CarniSys.NG.Application.Users;

public interface IUserPermissionCommandService
{
    Task<bool> SaveUserPermissionsAsync(
        SaveUserPermissionsRequest request,
        CancellationToken cancellationToken = default);
}
