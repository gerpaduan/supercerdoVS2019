namespace CarniSys.NG.Application.Purchases;

public sealed class PurchaseDetailLineItem
{
    public int LineId { get; init; }

    public string LineType { get; init; } = string.Empty;

    public int? ProductId { get; init; }

    public string Code { get; init; } = string.Empty;

    public string ProductName { get; init; } = string.Empty;

    public decimal QuantityKg { get; init; }

    public decimal Price { get; init; }

    public decimal Total { get; init; }

    public bool ScaleWeight { get; init; }
}
