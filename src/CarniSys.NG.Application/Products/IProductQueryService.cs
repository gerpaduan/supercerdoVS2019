namespace CarniSys.NG.Application.Products;

public interface IProductQueryService
{
    Task<IReadOnlyCollection<ProductListItem>> GetProductsAsync(
        int companyId,
        ProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailItem?> GetProductByIdAsync(
        int companyId,
        int productId,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateProductBasicsAsync(
        ProductEditRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductBranchStockPointResult> GetBranchStockPointsAsync(
        int companyId,
        int productId,
        int legacyStockPoint,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductListItem>> GetGlobalProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default);

    Task<ProductDetailItem?> GetGlobalProductByIdAsync(
        int productId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductTypeListItem>> GetCompanyProductTypesAsync(
        int companyId,
        ProductTypeListQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ProductTypeListItem>> GetGlobalProductTypesAsync(
        ProductTypeListQuery query,
        CancellationToken cancellationToken = default);
}
