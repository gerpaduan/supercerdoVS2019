namespace CarniSys.NG.Application.People;

public sealed class PersonDetailItem
{
    public int PersonId { get; init; }

    public int CompanyId { get; init; }

    public string Identification { get; init; } = string.Empty;

    public string BusinessName { get; init; } = string.Empty;

    public int VatId { get; init; }

    public string VatLabel { get; init; } = string.Empty;

    public string TaxId { get; init; } = string.Empty;

    public string Phone { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public string City { get; init; } = string.Empty;

    public bool HasCurrentAccount { get; init; }

    public decimal Discount { get; init; }

    public string Notes { get; init; } = string.Empty;

    public bool IsBrand { get; init; }

    public int? OwnerId { get; init; }

    public string OwnerName { get; init; } = string.Empty;

    public DateTime CreatedAt { get; init; }
}
