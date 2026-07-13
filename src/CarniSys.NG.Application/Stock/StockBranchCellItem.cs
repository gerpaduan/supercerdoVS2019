namespace CarniSys.NG.Application.Stock;

public sealed class StockBranchCellItem
{
    public int BranchId { get; init; }

    public string BranchName { get; init; } = string.Empty;

    public decimal StockActual { get; init; }

    public string StockState { get; init; } = string.Empty;
}
