using CarniSys.NG.Application.Products;

namespace CarniSys.NG.Web.Models;

public sealed class ProductListPageViewModel
{
    public string SearchText { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public required IReadOnlyCollection<ProductListItem> Items { get; init; }
}
