namespace CarniSys.NG.Application.Users;

public sealed class UserPermissionPage
{
    public int UserId { get; init; }

    public string UserName { get; init; } = string.Empty;

    public string UserLogin { get; init; } = string.Empty;

    public required IReadOnlyCollection<UserPermissionItem> Items { get; init; }
}
