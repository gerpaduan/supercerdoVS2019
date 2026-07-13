namespace CarniSys.NG.Web.Models;

public sealed class ProductLookupModalViewModel
{
    public string ModalId { get; init; } = "productLookupModal";

    public string Title { get; init; } = "Buscar producto";

    public string SearchUrl { get; init; } = string.Empty;

    public bool ShowPrice { get; init; }

    public bool ShowType { get; init; }
}
