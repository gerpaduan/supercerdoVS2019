namespace CarniSys.NG.Application.Movements;

public sealed class MovementSaveLineRequest
{
    public int ProductId { get; init; }

    public int QuantityUnits { get; init; }

    public decimal QuantityWeightKg { get; init; }

    public bool ScaleWeight { get; init; }

    public bool AllowEntry { get; init; }
}
