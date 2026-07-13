namespace CarniSys.NG.Application.Products;

public sealed class ProductDetailItem
{
    public int ProductId { get; init; }

    public int CompanyId { get; init; }

    public long Code { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int? BrandId { get; init; }

    public string BrandName { get; init; } = string.Empty;

    public decimal PricePerKilogram { get; init; }

    public bool Weighable { get; init; }

    public decimal AverageWeight { get; init; }

    public int VatRateId { get; init; }

    public decimal VatRate { get; init; }

    public int StockPoint { get; init; }

    public bool IncludedInStockClosing { get; init; }

    public bool Enabled { get; init; }

    public bool QuickElaboratedEntry { get; init; }

    public int Level { get; init; }

    public bool Independent { get; init; }

    public string CutMode { get; init; } = "Ninguno";

    public int? MasterProductId { get; init; }

    public string MasterProductName { get; init; } = string.Empty;

    public decimal Percentage { get; init; }

    public decimal BonePercentage { get; init; }

    public decimal? PresentationUnits { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
