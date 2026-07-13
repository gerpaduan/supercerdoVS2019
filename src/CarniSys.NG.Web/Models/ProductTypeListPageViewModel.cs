using CarniSys.NG.Application.Products;

namespace CarniSys.NG.Web.Models;

public sealed class ProductTypeListPageViewModel
{
    public string SearchText { get; init; } = string.Empty;

    public bool IsGlobalCatalog { get; init; }

    public bool CanManageTypes { get; init; }

    public IReadOnlySet<string> ExistingCompanyTypes { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

    public required IReadOnlyCollection<ProductTypeListItem> Items { get; init; }
}
