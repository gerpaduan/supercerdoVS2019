namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseListQuery
{
    public int BranchId { get; init; }

    public string PurchaseType { get; init; } = string.Empty;

    public string SearchText { get; init; } = string.Empty;

    public DateTime DateFrom { get; init; } = DateTime.Today;

    public DateTime DateTo { get; init; } = DateTime.Today;
}
