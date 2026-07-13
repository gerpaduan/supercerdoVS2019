namespace CarniSys.NG.Application.Products;

public sealed class ProductListQuery
{
    public string SearchText { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public int Skip { get; init; }

    public int Take { get; init; } = 50;
}
