namespace CarniSys.NG.Application.Movements;

public interface IMovementCommandService
{
    Task<MovementSaveResult> SaveMovementAsync(
        MovementSaveRequest request,
        CancellationToken cancellationToken = default);
}
