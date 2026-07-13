namespace CarniSys.NG.Application.People;

public interface IPersonCommandService
{
    Task<PersonSaveResult> SavePersonAsync(
        PersonSaveRequest request,
        CancellationToken cancellationToken = default);
}
