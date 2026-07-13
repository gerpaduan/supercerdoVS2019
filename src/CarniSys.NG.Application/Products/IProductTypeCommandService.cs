namespace CarniSys.NG.Application.Products;

public interface IProductTypeCommandService
{
    Task<ProductTypeSaveResult> SaveCompanyProductTypeAsync(
        ProductTypeEditRequest request,
        CancellationToken cancellationToken = default);

    Task<ProductTypeSaveResult> ImportGlobalProductTypesAsync(
        int companyId,
        IReadOnlyCollection<string> typeNames,
        CancellationToken cancellationToken = default);

    Task<ProductTypeSaveResult> DeleteCompanyProductTypeAsync(
        int companyId,
        string typeName,
        CancellationToken cancellationToken = default);
}
