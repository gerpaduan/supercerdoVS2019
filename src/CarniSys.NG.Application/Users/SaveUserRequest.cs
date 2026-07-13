namespace CarniSys.NG.Application.Users;

public sealed class SaveUserRequest
{
    public int CompanyId { get; init; }

    public int UserId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Login { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public bool IsAdministrator { get; init; }

    public bool IsActive { get; init; }

    public string Email { get; init; } = string.Empty;

    public int BranchId { get; init; }

    public bool CanLoginOutsideBranch { get; init; }
}
