namespace CarniSys.NG.Web.Models;

public sealed class PermissionProbeViewModel
{
    public required string Label { get; init; }

    public required string Resource { get; init; }

    public bool CanRead { get; init; }

    public bool CanEdit { get; init; }
}
