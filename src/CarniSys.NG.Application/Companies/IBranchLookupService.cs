namespace CarniSys.NG.Application.Companies;

public interface IBranchLookupService
{
    Task<IReadOnlyCollection<BranchLookupItem>> GetBranchesAsync(
        int companyId,
        CancellationToken cancellationToken = default);
}
