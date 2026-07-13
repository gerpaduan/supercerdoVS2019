namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseListItem
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

    public int HalfCarcassCount { get; init; }

    public decimal TotalKg { get; init; }

    public decimal TotalAmount { get; init; }
}
