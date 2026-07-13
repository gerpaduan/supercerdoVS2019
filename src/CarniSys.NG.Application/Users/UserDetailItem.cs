namespace CarniSys.NG.Application.Users;

public sealed class UserDetailItem
{
    public int UserId { get; init; }

    public int CompanyId { get; init; }

    public int BranchId { get; init; }

    public string FullName { get; init; } = string.Empty;

    public string Login { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public string BranchName { get; init; } = string.Empty;

    public bool IsAdministrator { get; init; }

    public bool IsActive { get; init; }

    public bool CanLoginOutsideBranch { get; init; }
}
