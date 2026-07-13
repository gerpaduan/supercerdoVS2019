namespace CarniSys.NG.Application.Stock;

public sealed class StockSaveResult
{
    private StockSaveResult(bool success, int stockId, string errorMessage)
    {
        Success = success;
        StockId = stockId;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public int StockId { get; }

    public string ErrorMessage { get; }

    public static StockSaveResult Ok(int stockId)
    {
        return new StockSaveResult(true, stockId, string.Empty);
    }

    public static StockSaveResult Failure(string errorMessage)
    {
        return new StockSaveResult(false, 0, errorMessage);
    }
}
