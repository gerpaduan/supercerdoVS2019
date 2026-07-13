namespace CarniSys.NG.Application.Purchases;

public interface IPurchaseQueryService
{
    Task<IReadOnlyCollection<PurchaseListItem>> GetPurchasesAsync(
        PurchaseListQuery query,
        CancellationToken cancellationToken = default);

    Task<PurchaseDetailItem?> GetPurchaseByIdAsync(
        int purchaseId,
        CancellationToken cancellationToken = default);
}
