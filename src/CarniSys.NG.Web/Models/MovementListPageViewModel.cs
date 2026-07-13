using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Web.Models;

public sealed class MovementListPageViewModel
{
    public bool CanManageMovements { get; init; }

    public int OriginBranchId { get; init; }

    public int DestinationBranchId { get; init; }

    public DateTime DateFrom { get; init; }

    public DateTime DateTo { get; init; }

    public required IReadOnlyCollection<BranchLookupItem> Branches { get; init; }

    public required IReadOnlyCollection<MovementListItem> Items { get; init; }
}
