namespace CarniSys.NG.Application.People;

public interface IBrandQueryService
{
    Task<IReadOnlyCollection<BrandListItem>> GetBrandsAsync(
        BrandListQuery query,
        CancellationToken cancellationToken = default);

    Task<BrandDetailItem?> GetBrandByIdAsync(
        int brandId,
        CancellationToken cancellationToken = default);
}
