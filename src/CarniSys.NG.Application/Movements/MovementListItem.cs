namespace CarniSys.NG.Application.Movements;

public sealed class MovementListItem
{
    public int MovementId { get; init; }

    public DateTime MovementDate { get; init; }

    public string OriginBranchName { get; init; } = string.Empty;

    public string DestinationBranchName { get; init; } = string.Empty;

    public string OriginMovementReference { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public decimal TotalUnits { get; init; }

    public decimal TotalWeightKg { get; init; }

    public string Notes { get; init; } = string.Empty;
}
