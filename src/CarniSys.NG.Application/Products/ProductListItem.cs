namespace CarniSys.NG.Application.Products;

public sealed class ProductListItem
{
    public int ProductId { get; init; }

    public long Code { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public string BrandName { get; init; } = string.Empty;

    public decimal PricePerKilogram { get; init; }

    public bool Enabled { get; init; }

    public bool Weighable { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
