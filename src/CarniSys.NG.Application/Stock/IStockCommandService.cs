namespace CarniSys.NG.Application.Stock;

public interface IStockCommandService
{
    Task<StockSaveResult> SaveStockAsync(
        StockSaveRequest request,
        CancellationToken cancellationToken = default);

    Task<StockGenerateWeighingAdjustmentResult> GenerateWeighingAdjustmentAsync(
        StockGenerateWeighingAdjustmentRequest request,
        CancellationToken cancellationToken = default);
}
