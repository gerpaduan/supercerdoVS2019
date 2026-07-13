namespace CarniSys.NG.Application.Products;

public sealed class ProductTypeSaveResult
{
    public bool Success { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static ProductTypeSaveResult Ok() => new() { Success = true };

    public static ProductTypeSaveResult Failure(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage ?? string.Empty
    };
}
