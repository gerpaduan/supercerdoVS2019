namespace CarniSys.NG.Application.Stock;

public sealed class StockEditDetailItem
{
    public int StockId { get; init; }

    public string StockOperationType { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public DateTime OperationDate { get; init; }

    public string Notes { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public string SupplierTaxId { get; init; } = string.Empty;

    public int? HalfCarcassCount { get; init; }

    public decimal? HalfCarcassWeightKg { get; init; }

    public int? LinkedWeighingId { get; init; }

    public DateTime? LinkedPurchaseDate { get; init; }

    public string LinkedPurchaseSupplierName { get; init; } = string.Empty;

    public int? LinkedPurchaseHalfCarcassCount { get; init; }

    public decimal? LinkedPurchaseWeightKg { get; init; }

    public DateTime? AdjustedWeighingDate { get; init; }

    public string AdjustedWeighingSupplierName { get; init; } = string.Empty;

    public int? AdjustedWeighingHalfCarcassCount { get; init; }

    public decimal? AdjustedWeighingWeightKg { get; init; }

    public string CreatedAtLabel { get; init; } = string.Empty;

    public string CreatedByLabel { get; init; } = string.Empty;

    public string UpdatedAtLabel { get; init; } = string.Empty;

    public string UpdatedByLabel { get; init; } = string.Empty;

    public IReadOnlyCollection<StockEditLineItem> Lines { get; init; } = Array.Empty<StockEditLineItem>();
}

public sealed class StockEditLineItem
{
    public int Index { get; init; }

    public int ProductId { get; init; }

    public long Code { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal QuantityKg { get; init; }

    public bool ScaleWeight { get; init; }

    public string CreatedLabel { get; init; } = string.Empty;

    public bool IsWeighable { get; init; }
}
