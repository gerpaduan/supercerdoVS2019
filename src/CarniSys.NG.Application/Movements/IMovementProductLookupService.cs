namespace CarniSys.NG.Application.Movements;

public interface IMovementProductLookupService
{
    Task<IReadOnlyCollection<MovementProductLookupItem>> GetQuickProductsAsync(
        int companyId,
        int maxCode,
        int limit,
        CancellationToken cancellationToken = default);

    Task<MovementProductLookupItem?> FindProductByCodeAsync(
        int companyId,
        long code,
        CancellationToken cancellationToken = default);
}
