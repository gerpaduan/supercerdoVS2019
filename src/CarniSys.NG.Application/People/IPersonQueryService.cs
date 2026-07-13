namespace CarniSys.NG.Application.People;

public interface IPersonQueryService
{
    Task<IReadOnlyCollection<PersonListItem>> GetPeopleAsync(
        int companyId,
        PersonListQuery query,
        CancellationToken cancellationToken = default);

    Task<PersonDetailItem?> GetPersonByIdAsync(
        int personId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<PersonVatOption>> GetVatOptionsAsync(
        CancellationToken cancellationToken = default);

    Task<bool> HasSalesOrPurchasesAsync(
        int personId,
        CancellationToken cancellationToken = default);
}
