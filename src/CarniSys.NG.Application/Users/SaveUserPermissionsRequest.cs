namespace CarniSys.NG.Application.Users;

public sealed class SaveUserPermissionsRequest
{
    public int CompanyId { get; init; }

    public int UserId { get; init; }

    public required IReadOnlyCollection<SaveUserPermissionItem> Items { get; init; }
}
