namespace CarniSys.NG.Application.Stock;

public sealed class StockMatrixQuery
{
    public string SearchText { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public DateTime? UntilDate { get; init; }

    public string Type { get; init; } = string.Empty;

    public bool OnlyWithStock { get; init; }

    public string StockState { get; init; } = "Todos";
}
