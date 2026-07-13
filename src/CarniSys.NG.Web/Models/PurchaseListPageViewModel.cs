using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Purchases;

namespace CarniSys.NG.Web.Models;

public sealed class PurchaseListPageViewModel
{
    public int BranchId { get; init; }

    public string PurchaseType { get; init; } = string.Empty;

    public string SearchText { get; init; } = string.Empty;

    public DateTime DateFrom { get; init; }

    public DateTime DateTo { get; init; }

    public int TotalHalfCarcassCount { get; init; }

    public decimal TotalKg { get; init; }

    public decimal TotalAmount { get; init; }

    public bool CanCreatePurchase { get; init; }

    public required IReadOnlyCollection<BranchLookupItem> Branches { get; init; }

    public required IReadOnlyCollection<PurchaseListItem> Items { get; init; }
}
