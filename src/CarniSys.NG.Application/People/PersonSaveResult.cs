namespace CarniSys.NG.Application.People;

public sealed class PersonSaveResult
{
    private PersonSaveResult(bool success, string errorMessage)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public bool Success { get; }

    public string ErrorMessage { get; }

    public static PersonSaveResult Ok()
    {
        return new PersonSaveResult(true, string.Empty);
    }

    public static PersonSaveResult Failure(string errorMessage)
    {
        return new PersonSaveResult(false, errorMessage);
    }
}
