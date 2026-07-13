namespace CarniSys.NG.Application.Stock;

public sealed class StockSaveRequest
{
    public int CompanyId { get; init; }

    public int UserId { get; init; }

    public int StockId { get; init; }

    public string StockOperationType { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public DateTime OperationDate { get; init; }

    public string Notes { get; init; } = string.Empty;

    public int SupplierId { get; init; }

    public int? HalfCarcassCount { get; init; }

    public decimal? HalfCarcassWeightKg { get; init; }

    public bool SaveWithoutWeighing { get; init; }

    public int? LinkedWeighingId { get; init; }

    public IReadOnlyCollection<StockSaveLineRequest> Lines { get; init; } = [];
}

public sealed class StockSaveLineRequest
{
    public int ProductId { get; init; }

    public decimal QuantityKg { get; init; }

    public bool ScaleWeight { get; init; }
}
