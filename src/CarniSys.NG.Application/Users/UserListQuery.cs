namespace CarniSys.NG.Application.Users;

public sealed class UserListQuery
{
    public string SearchText { get; init; } = string.Empty;

    public bool OnlyActive { get; init; }
}
