namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseSaveRequest
{
    public int CompanyId { get; init; }

    public int PurchaseId { get; init; }

    public string PurchaseType { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public DateTime PurchaseDate { get; init; }

    public int SupplierId { get; init; }

    public string ReceiptNumber { get; init; } = string.Empty;

    public string Notes { get; init; } = string.Empty;

    public bool CurrentAccount { get; init; }

    public int? HalfCarcassCount { get; init; }

    public int UserId { get; init; }

    public IReadOnlyCollection<PurchaseSaveLineRequest> Lines { get; init; } = Array.Empty<PurchaseSaveLineRequest>();
}
