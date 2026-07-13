namespace CarniSys.NG.Application.People;

public interface IBrandCommandService
{
    Task<BrandSaveResult> SaveBrandAsync(
        BrandSaveRequest request,
        CancellationToken cancellationToken = default);

    Task<BrandSaveResult> DeleteBrandAsync(
        int companyId,
        int brandId,
        CancellationToken cancellationToken = default);
}
