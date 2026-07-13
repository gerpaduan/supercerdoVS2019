namespace CarniSys.NG.Application.People;

public sealed class BrandListQuery
{
    public string SearchText { get; init; } = string.Empty;

    public int Skip { get; init; }

    public int Take { get; init; } = 50;
}
