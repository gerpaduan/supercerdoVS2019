namespace CarniSys.NG.Application.Stock;

public sealed class StockBranchDetailItem
{
    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public DateTime? LastStockClosingDate { get; init; }

    public decimal StockInitial { get; init; }

    public decimal TotalEntries { get; init; }

    public decimal TotalExits { get; init; }

    public decimal StockActual { get; init; }

    public decimal StockPoint { get; init; }

    public decimal DifferenceFromPoint { get; init; }

    public string StockState { get; init; } = string.Empty;
}
