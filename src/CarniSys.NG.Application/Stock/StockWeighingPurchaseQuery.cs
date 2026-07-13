namespace CarniSys.NG.Application.Stock;

public sealed class StockWeighingPurchaseQuery
{
    public int BranchId { get; init; }

    public int CurrentStockId { get; init; }

    public string SupplierSearchText { get; init; } = string.Empty;

    public DateTime? FromDate { get; init; }

    public DateTime? ToDate { get; init; }

    public bool OnlyWeighings { get; init; }
}
