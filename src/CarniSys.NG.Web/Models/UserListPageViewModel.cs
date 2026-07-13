using CarniSys.NG.Application.Users;

namespace CarniSys.NG.Web.Models;

public sealed class UserListPageViewModel
{
    public string SearchText { get; init; } = string.Empty;

    public bool OnlyActive { get; init; }

    public bool CanManageUsers { get; init; }

    public required IReadOnlyCollection<UserListItem> Items { get; init; }
}
