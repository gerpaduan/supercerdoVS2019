namespace CarniSys.NG.Application.Stock;

public sealed class StockWeighingPercentagesItem
{
    public bool HasRequiredMediaData { get; init; }

    public string Status { get; init; } = string.Empty;

    public StockModalTableItem AverageHalfCarcassesTable { get; init; } = new();

    public StockModalTableItem CutPercentagesTable { get; init; } = new();
}

public sealed class StockModalTableItem
{
    public IReadOnlyCollection<StockModalTableColumnItem> Columns { get; init; } = Array.Empty<StockModalTableColumnItem>();

    public IReadOnlyCollection<string[]> Rows { get; init; } = Array.Empty<string[]>();
}

public sealed class StockModalTableColumnItem
{
    public string Name { get; init; } = string.Empty;

    public bool Hidden { get; init; }

    public bool RightAligned { get; init; }

    public bool ThreeDecimalFormat { get; init; }
}
