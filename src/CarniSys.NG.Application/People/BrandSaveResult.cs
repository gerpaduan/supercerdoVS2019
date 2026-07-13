namespace CarniSys.NG.Application.People;

public sealed class BrandSaveResult
{
    public bool Success { get; init; }

    public bool RequiresConfirmation { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static BrandSaveResult Ok() => new() { Success = true };

    public static BrandSaveResult Failure(string errorMessage, bool requiresConfirmation = false) => new()
    {
        Success = false,
        RequiresConfirmation = requiresConfirmation,
        ErrorMessage = errorMessage ?? string.Empty
    };
}
