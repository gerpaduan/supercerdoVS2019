namespace CarniSys.NG.Application.People;

public interface IAfipPadronLookupService
{
    Task<AfipPadronLookupResult> LookupAsync(
        int companyId,
        string taxId,
        CancellationToken cancellationToken = default);
}
