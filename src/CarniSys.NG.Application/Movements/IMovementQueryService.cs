namespace CarniSys.NG.Application.Movements;

public interface IMovementQueryService
{
    Task<IReadOnlyCollection<MovementListItem>> GetMovementsAsync(
        int companyId,
        MovementListQuery query,
        CancellationToken cancellationToken = default);

    Task<MovementDetailItem?> GetMovementByIdAsync(
        int movementId,
        CancellationToken cancellationToken = default);
}
