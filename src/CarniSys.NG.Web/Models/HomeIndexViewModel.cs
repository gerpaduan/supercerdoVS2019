namespace CarniSys.NG.Web.Models;

public sealed class HomeIndexViewModel
{
    public required string UserName { get; init; }

    public required string Login { get; init; }

    public required string CompanyName { get; init; }

    public required string BranchName { get; init; }

    public required IReadOnlyCollection<PermissionProbeViewModel> PermissionProbes { get; init; }
}
