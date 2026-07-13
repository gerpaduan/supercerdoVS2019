namespace CarniSys.NG.Application.Movements;

public sealed class MovementDetailItem
{
    public int MovementId { get; init; }

    public DateTime MovementDate { get; init; }

    public int OriginBranchId { get; init; }

    public string OriginBranchName { get; init; } = string.Empty;

    public int DestinationBranchId { get; init; }

    public string DestinationBranchName { get; init; } = string.Empty;

    public int? OriginMovementId { get; init; }

    public string Notes { get; init; } = string.Empty;

    public DateTime? CreatedAt { get; init; }

    public string CreatedByName { get; init; } = string.Empty;

    public DateTime? UpdatedAt { get; init; }

    public string UpdatedByName { get; init; } = string.Empty;

    public IReadOnlyCollection<MovementDetailLineItem> Lines { get; init; } = [];
}
