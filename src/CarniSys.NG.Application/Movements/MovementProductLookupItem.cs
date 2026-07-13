namespace CarniSys.NG.Application.Movements;

public sealed class MovementProductLookupItem
{
    public int ProductId { get; init; }

    public long Code { get; init; }

    public string Description { get; init; } = string.Empty;

    public string Type { get; init; } = string.Empty;

    public bool Weighable { get; init; }

    public decimal AverageWeight { get; init; }
}
