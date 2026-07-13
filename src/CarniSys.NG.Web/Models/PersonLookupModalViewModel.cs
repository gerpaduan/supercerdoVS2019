namespace CarniSys.NG.Web.Models;

public sealed class PersonLookupModalViewModel
{
    public string ModalId { get; init; } = "personLookupModal";

    public string Title { get; init; } = "Buscar persona";

    public string SearchUrl { get; init; } = string.Empty;

    public string SearchPlaceholder { get; init; } = "Nombre, identificacion o CUIT";

    public string NameColumnTitle { get; init; } = "Nombre";

    public bool ShowTaxId { get; init; } = true;

    public bool ShowIdentification { get; init; }
}
