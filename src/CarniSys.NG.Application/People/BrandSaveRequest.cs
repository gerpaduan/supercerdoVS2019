namespace CarniSys.NG.Application.People;

public sealed class BrandSaveRequest
{
    public int BrandId { get; init; }

    public string BrandName { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public int? OwnerId { get; init; }

    public bool IsAdministrator { get; init; }

    public bool ConfirmSimilarBrands { get; init; }
}
