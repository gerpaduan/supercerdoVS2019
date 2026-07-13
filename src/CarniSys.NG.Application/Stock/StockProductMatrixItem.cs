namespace CarniSys.NG.Application.Stock;

public sealed class StockProductMatrixItem
{
    public int ProductId { get; init; }

    public long Code { get; init; }

    public string Description { get; init; } = string.Empty;

    public required IReadOnlyCollection<StockBranchCellItem> Cells { get; init; }

    public required IReadOnlyCollection<StockBranchDetailItem> Details { get; init; }

    public bool HasPositiveStock { get; init; }
}
