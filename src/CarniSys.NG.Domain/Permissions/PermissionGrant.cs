namespace CarniSys.NG.Domain.Permissions;

public sealed class PermissionGrant
{
    public PermissionGrant(string resource, int readWindowInDays, int editWindowInDays, RecordOwnershipScope ownershipScope)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resource);
        if (readWindowInDays < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(readWindowInDays));
        }

        if (editWindowInDays < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(editWindowInDays));
        }

        Resource = resource.Trim();
        ReadWindowInDays = readWindowInDays;
        EditWindowInDays = editWindowInDays;
        OwnershipScope = ownershipScope;
    }

    public string Resource { get; }

    public int ReadWindowInDays { get; }

    public int EditWindowInDays { get; }

    public RecordOwnershipScope OwnershipScope { get; }
}
