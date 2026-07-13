namespace CarniSys.NG.Application.Products;

public sealed class ProductEditRequest
{
    public int ProductId { get; init; }

    public int CompanyId { get; init; }

    public int? BrandId { get; init; }

    public decimal PricePerKilogram { get; init; }

    public bool Weighable { get; init; }

    public decimal AverageWeight { get; init; }

    public int StockPoint { get; init; }

    public bool UseBranchStockPoints { get; init; }

    public IReadOnlyCollection<ProductBranchStockPointItem> BranchStockPoints { get; init; } = Array.Empty<ProductBranchStockPointItem>();

    public bool IncludedInStockClosing { get; init; }

    public bool Enabled { get; init; }

    public bool QuickElaboratedEntry { get; init; }
}
