namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseSaveResult
{
    public bool Success { get; init; }

    public int PurchaseId { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;
}
