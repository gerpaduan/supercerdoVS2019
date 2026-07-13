namespace CarniSys.NG.Application.Products;

public sealed class ProductTypeListItem
{
    public string TypeName { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public bool IsReserved { get; init; }

    public DateTime? CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }

    public bool IsGlobal { get; init; }
}
