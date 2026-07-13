namespace CarniSys.NG.Application.Stock;

public sealed class StockMatrixResult
{
    public required IReadOnlyCollection<StockBranchColumnItem> Columns { get; init; }

    public required IReadOnlyCollection<StockProductMatrixItem> Items { get; init; }

    public string Message { get; init; } = string.Empty;
}
