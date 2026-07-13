namespace CarniSys.NG.Application.Users;

public interface IUserQueryService
{
    Task<IReadOnlyCollection<UserListItem>> GetUsersAsync(
        int companyId,
        UserListQuery query,
        CancellationToken cancellationToken = default);

    Task<UserDetailItem?> GetUserByIdAsync(
        int companyId,
        int userId,
        CancellationToken cancellationToken = default);

    Task<UserPermissionPage?> GetUserPermissionsAsync(
        int companyId,
        int userId,
        CancellationToken cancellationToken = default);
}
