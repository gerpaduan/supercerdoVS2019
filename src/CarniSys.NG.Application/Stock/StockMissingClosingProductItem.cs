namespace CarniSys.NG.Application.Stock;

public sealed class StockMissingClosingProductItem
{
    public int ProductId { get; init; }

    public long Code { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal CurrentStock { get; init; }

    public bool Weighable { get; init; }

    public decimal AverageWeight { get; init; }
}
