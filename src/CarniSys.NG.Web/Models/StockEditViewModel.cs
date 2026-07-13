using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Web.Models;

public sealed class StockEditViewModel
{
    public static readonly IReadOnlyCollection<string> DefaultOperationTypes =
    [
        "Ingreso Stock",
        "Egreso Stock",
        "Cierre Stock",
        "Ajuste Stock",
        "Pesaje Cortes"
    ];

    public int StockId { get; init; }

    public bool IsEdit { get; init; }

    public bool IsReadOnlyInitially { get; init; }

    public bool CanEnableEditing { get; init; } = true;

    public string StockOperationType { get; set; } = "Ingreso Stock";

    public int BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    public DateTime OperationDate { get; set; } = DateTime.Now;

    public string Notes { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public string SupplierTaxId { get; set; } = string.Empty;

    public int? HalfCarcassCount { get; set; }

    public decimal? HalfCarcassWeightKg { get; set; }

    public bool SaveWithoutWeighing { get; set; }

    public int? LinkedWeighingId { get; set; }

    public DateTime? LinkedPurchaseDate { get; set; }

    public string LinkedPurchaseSupplierName { get; set; } = string.Empty;

    public int? LinkedPurchaseHalfCarcassCount { get; set; }

    public decimal? LinkedPurchaseWeightKg { get; set; }

    public string LinkedPurchaseStatus { get; set; } = string.Empty;

    public DateTime? AdjustedWeighingDate { get; set; }

    public string AdjustedWeighingSupplierName { get; set; } = string.Empty;

    public int? AdjustedWeighingHalfCarcassCount { get; set; }

    public decimal? AdjustedWeighingWeightKg { get; set; }

    public string AdjustedWeighingStatus { get; set; } = string.Empty;

    public string CreatedAtLabel { get; set; } = string.Empty;

    public string CreatedByLabel { get; set; } = string.Empty;

    public string UpdatedAtLabel { get; set; } = string.Empty;

    public string UpdatedByLabel { get; set; } = string.Empty;

    public int ItemCount { get; set; }

    public decimal TotalQuantityKg { get; set; }

    public IReadOnlyCollection<BranchLookupItem> Branches { get; set; } = [];

    public IReadOnlyCollection<string> OperationTypes { get; set; } = [];

    public List<MovementProductLookupItem> QuickProducts { get; set; } = [];

    public List<StockEditLineViewModel> Lines { get; set; } = [];
}

public sealed class StockEditLineViewModel
{
    public int Index { get; set; }

    public int? ProductId { get; set; }

    public long? Code { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public decimal QuantityKg { get; set; }

    public bool ScaleWeight { get; set; } = true;

    public string CreatedLabel { get; set; } = string.Empty;

    public bool IsWeighable { get; set; }

    public string SourceType { get; set; } = string.Empty;

    public int? SourceStockId { get; set; }

    public string SourceLabel { get; set; } = string.Empty;

    public int? LinkedWeighingId { get; set; }

    public string LinkedWeighingLabel { get; set; } = string.Empty;
}
