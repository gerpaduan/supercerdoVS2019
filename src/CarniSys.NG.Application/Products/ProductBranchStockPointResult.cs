namespace CarniSys.NG.Application.Products;

public sealed class ProductBranchStockPointResult
{
    public bool HasCustomBranchPoints { get; init; }

    public required IReadOnlyCollection<ProductBranchStockPointItem> Items { get; init; }
}
