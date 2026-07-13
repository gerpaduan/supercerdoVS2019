namespace CarniSys.NG.Application.Movements;

public sealed class MovementSaveResult
{
    private MovementSaveResult(bool success, int movementId, string errorMessage)
    {
        Success = success;
        MovementId = movementId;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public int MovementId { get; }

    public string ErrorMessage { get; }

    public static MovementSaveResult Ok(int movementId)
    {
        return new MovementSaveResult(true, movementId, string.Empty);
    }

    public static MovementSaveResult Failure(string errorMessage)
    {
        return new MovementSaveResult(false, 0, errorMessage);
    }
}
