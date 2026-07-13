namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseSaveLineRequest
{
    public string LineType { get; init; } = string.Empty;

    public int ProductId { get; init; }

    public string TroopNumber { get; init; } = string.Empty;

    public decimal QuantityKg { get; init; }

    public decimal PricePerKg { get; init; }
}
