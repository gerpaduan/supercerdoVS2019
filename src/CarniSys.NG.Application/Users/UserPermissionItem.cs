namespace CarniSys.NG.Application.Users;

public sealed class UserPermissionItem
{
    public int FormId { get; init; }

    public string FormName { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public bool CanRead { get; init; }

    public int ReadDays { get; init; }

    public bool CanEdit { get; init; }

    public int EditDays { get; init; }

    public bool OwnRecordsOnly { get; init; }
}
