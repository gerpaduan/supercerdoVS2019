namespace CarniSys.NG.Application.Stock;

public sealed class StockGenerateWeighingAdjustmentRequest
{
    public int CompanyId { get; init; }

    public int UserId { get; init; }

    public int WeighingId { get; init; }
}
