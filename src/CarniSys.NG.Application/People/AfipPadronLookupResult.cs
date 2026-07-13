namespace CarniSys.NG.Application.People;

public sealed class AfipPadronLookupResult
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public string Identification { get; init; } = string.Empty;

    public string BusinessName { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public int SuggestedVatId { get; init; }

    public string VatCondition { get; init; } = string.Empty;

    public string TaxStatus { get; init; } = string.Empty;

    public string MainActivity { get; init; } = string.Empty;

    public string MessageType { get; init; } = "error";
}
