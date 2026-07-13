namespace CarniSys.NG.Application.People;

public sealed class PersonSaveRequest
{
    public int CompanyId { get; init; }

    public int PersonId { get; init; }

    public string Identification { get; init; } = string.Empty;

    public string BusinessName { get; init; } = string.Empty;

    public int VatId { get; init; }

    public string TaxId { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public bool HasCurrentAccount { get; init; }

    public decimal Discount { get; init; }

    public bool IsAdministrator { get; init; }

    public bool CanManageCurrentAccount { get; init; }
}
