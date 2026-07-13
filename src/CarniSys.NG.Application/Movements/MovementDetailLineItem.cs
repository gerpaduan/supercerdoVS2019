namespace CarniSys.NG.Application.Movements;

public sealed class MovementDetailLineItem
{
    public int MovementLineId { get; init; }

    public int ProductId { get; init; }

    public long ProductCode { get; init; }

    public string ProductName { get; init; } = string.Empty;

    public decimal QuantityWeightKg { get; init; }

    public int QuantityUnits { get; init; }

    public bool ScaleWeight { get; init; }

    public bool AllowEntry { get; init; }
}
