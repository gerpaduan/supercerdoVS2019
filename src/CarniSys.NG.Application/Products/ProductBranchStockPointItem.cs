namespace CarniSys.NG.Application.Products;

public sealed class ProductBranchStockPointItem
{
    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public int StockPoint { get; init; }
}
