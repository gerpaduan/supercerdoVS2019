namespace CarniSys.NG.Application.Purchases;

public interface IPurchaseCommandService
{
    Task<PurchaseSaveResult> SavePurchaseAsync(PurchaseSaveRequest request, CancellationToken cancellationToken = default);
}
