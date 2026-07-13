namespace CarniSys.NG.Application.Products;

public sealed class ProductTypeEditRequest
{
    public int CompanyId { get; init; }

    public string OriginalTypeName { get; init; } = string.Empty;

    public string TypeName { get; init; } = string.Empty;

    public int SortOrder { get; init; }
}
