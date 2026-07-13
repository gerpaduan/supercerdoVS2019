using CarniSys.NG.Application.People;

namespace CarniSys.NG.Web.Models;

public sealed class BrandListPageViewModel
{
    public string SearchText { get; init; } = string.Empty;

    public required IReadOnlyCollection<BrandListItem> Items { get; init; }
}
