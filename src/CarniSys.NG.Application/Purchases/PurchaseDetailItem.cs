namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseDetailItem
{
    public int PurchaseId { get; init; }

    public DateTime PurchaseDate { get; init; }

    public string PurchaseType { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public int SupplierId { get; init; }

    public string SupplierName { get; init; } = string.Empty;

    public string SupplierTaxId { get; init; } = string.Empty;

    public string ReceiptNumber { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public string Status { get; init; } = string.Empty;

    public bool CurrentAccount { get; init; }

    public int? HalfCarcassCount { get; init; }

    public decimal? HalfCarcassWeightKg { get; init; }

    public decimal TotalKg { get; init; }

    public decimal TotalAmount { get; init; }

    public DateTime? CreatedAt { get; init; }

    public int CreatedByUserId { get; init; }

    public string CreatedByName { get; init; } = string.Empty;

    public DateTime? UpdatedAt { get; init; }

    public string UpdatedByName { get; init; } = string.Empty;

    public IReadOnlyCollection<PurchaseDetailLineItem> Lines { get; init; } = Array.Empty<PurchaseDetailLineItem>();
}
