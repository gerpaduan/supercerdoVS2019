namespace CarniSys.NG.Application.Users;

public sealed class SaveUserPermissionItem
{
    public int FormId { get; init; }

    public bool CanRead { get; init; }

    public int ReadDays { get; init; }

    public bool CanEdit { get; init; }

    public int EditDays { get; init; }

    public bool OwnRecordsOnly { get; init; }
}
