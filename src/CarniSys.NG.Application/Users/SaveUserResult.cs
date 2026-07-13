namespace CarniSys.NG.Application.Users;

public sealed class SaveUserResult
{
    public bool Success { get; init; }

    public int UserId { get; init; }

    public string ErrorMessage { get; init; } = string.Empty;

    public static SaveUserResult Ok(int userId) => new() { Success = true, UserId = userId };

    public static SaveUserResult Failure(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage ?? string.Empty };
}
