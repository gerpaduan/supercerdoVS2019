namespace CarniSys.NG.Application.Stock;

public sealed class StockGenerateWeighingAdjustmentResult
{
    private StockGenerateWeighingAdjustmentResult(bool success, int adjustmentId, string status, string errorMessage)
    {
        Success = success;
        AdjustmentId = adjustmentId;
        Status = status;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public int AdjustmentId { get; }

    public string Status { get; }

    public string ErrorMessage { get; }

    public static StockGenerateWeighingAdjustmentResult Ok(int adjustmentId, string status)
    {
        return new StockGenerateWeighingAdjustmentResult(true, adjustmentId, status, string.Empty);
    }

    public static StockGenerateWeighingAdjustmentResult Failure(string errorMessage)
    {
        return new StockGenerateWeighingAdjustmentResult(false, 0, string.Empty, errorMessage);
    }
}
