using System.ComponentModel.DataAnnotations;
using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Web.Models;

public sealed class MovementEditViewModel
{
    public int MovementId { get; set; }

    public bool IsEdit { get; set; }

    public bool IsInitiallyReadOnly { get; set; }

    public bool CanEnableEditing { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar la sucursal origen.")]
    public int OriginBranchId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar la sucursal destino.")]
    public int DestinationBranchId { get; set; }

    public DateTime MovementDate { get; set; } = DateTime.Now;

    public string Notes { get; set; } = string.Empty;

    public string UserDisplayName { get; set; } = string.Empty;

    public string CreatedAtText { get; set; } = string.Empty;

    public string CreatedByName { get; set; } = string.Empty;

    public string UpdatedAtText { get; set; } = string.Empty;

    public string UpdatedByName { get; set; } = string.Empty;

    public string OriginMovementReference { get; set; } = string.Empty;

    public string DestinationMovementReference { get; set; } = string.Empty;

    public List<MovementEditLineViewModel> Lines { get; set; } = [];

    public List<BranchLookupItem> Branches { get; set; } = [];

    public List<MovementProductLookupItem> QuickProducts { get; set; } = [];
}

public sealed class MovementEditLineViewModel
{
    public int ProductId { get; set; }

    public long Code { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public bool Weighable { get; set; }

    public decimal AverageWeight { get; set; }

    public int QuantityUnits { get; set; }

    public decimal QuantityWeightKg { get; set; }

    public bool ScaleWeight { get; set; }

    public bool AllowEntry { get; set; }
}
