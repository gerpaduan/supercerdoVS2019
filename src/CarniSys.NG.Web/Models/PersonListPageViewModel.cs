using CarniSys.NG.Application.People;

namespace CarniSys.NG.Web.Models;

public sealed class PersonListPageViewModel
{
    public string SearchText { get; init; } = string.Empty;

    public required IReadOnlyCollection<PersonListItem> Items { get; init; }
}
