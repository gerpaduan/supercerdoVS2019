namespace CarniSys.NG.Application.People;

public sealed class BrandListItem
{
    public int BrandId { get; init; }

    public int CompanyId { get; init; }

    public string BrandName { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public string OwnerName { get; init; } = string.Empty;

    public string OwnerTaxId { get; init; } = string.Empty;

    public string OwnerPhone { get; init; } = string.Empty;

    public string OwnerAddress { get; init; } = string.Empty;

    public string OwnerCity { get; init; } = string.Empty;
}
