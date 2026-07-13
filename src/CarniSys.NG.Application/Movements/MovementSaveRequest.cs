namespace CarniSys.NG.Application.Movements;

public sealed class MovementSaveRequest
{
    public int CompanyId { get; init; }

    public int UserId { get; init; }

    public int MovementId { get; init; }

    public int OriginBranchId { get; init; }

    public int DestinationBranchId { get; init; }

    public DateTime MovementDate { get; init; }

    public string Notes { get; init; } = string.Empty;

    public IReadOnlyCollection<MovementSaveLineRequest> Lines { get; init; } = [];
}
