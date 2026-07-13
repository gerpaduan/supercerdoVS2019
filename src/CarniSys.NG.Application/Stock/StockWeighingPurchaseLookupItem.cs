namespace CarniSys.NG.Application.Stock;

public sealed class StockWeighingPurchaseLookupItem
{
    public int StockId { get; init; }

    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public string SupplierTaxId { get; init; } = string.Empty;

    public DateTime OperationDate { get; init; }

    public string StockOperationType { get; init; } = string.Empty;

    public int? HalfCarcassCount { get; init; }

    public decimal? HalfCarcassWeightKg { get; init; }

    public decimal TotalQuantityKg { get; init; }
}
