using CarniSys.NG.Application.Products;

namespace CarniSys.NG.Web.Models;

public sealed class ProductDetailPageViewModel
{
    public required ProductDetailItem Item { get; init; }

    public bool CanEdit { get; init; }
}
