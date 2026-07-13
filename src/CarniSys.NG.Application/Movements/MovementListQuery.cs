namespace CarniSys.NG.Application.Movements;

public sealed class MovementListQuery
{
    public int OriginBranchId { get; init; }

    public int DestinationBranchId { get; init; }

    public DateTime DateFrom { get; init; }

    public DateTime DateTo { get; init; }
}
