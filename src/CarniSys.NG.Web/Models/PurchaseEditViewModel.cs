using System.ComponentModel.DataAnnotations;
using CarniSys.NG.Application.Companies;

namespace CarniSys.NG.Web.Models;

public sealed class PurchaseEditViewModel
{
    public int PurchaseId { get; set; }

    public bool IsEdit { get; set; }

    public string PurchaseType { get; set; } = "Cortes";

    public bool CanUseHalfCarcass { get; set; } = true;

    public IReadOnlyCollection<string> AvailablePurchaseTypes { get; set; } = ["Cortes", "Media Res"];

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar la sucursal.")]
    public int BranchId { get; set; }

    [Display(Name = "Fecha y hora")]
    public DateTime PurchaseDate { get; set; } = DateTime.Now;

    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar el proveedor.")]
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = string.Empty;

    public string SupplierTaxId { get; set; } = string.Empty;

    public bool CurrentAccount { get; set; }

    public int? HalfCarcassCount { get; set; }

    public string ReceiptNumber { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public string CreatedAtLabel { get; set; } = string.Empty;

    public string CreatedByLabel { get; set; } = string.Empty;

    public string UpdatedAtLabel { get; set; } = string.Empty;

    public string UpdatedByLabel { get; set; } = string.Empty;

    public IReadOnlyCollection<BranchLookupItem> Branches { get; set; } = [];

    public List<PurchaseEditLineViewModel> Lines { get; set; } = [];

    public decimal TotalKg => Lines.Sum(x => x.QuantityKg);

    public decimal TotalAmount => Lines.Sum(x => x.LineTotal);
}

public sealed class PurchaseEditLineViewModel
{
    public string LineType { get; set; } = "Corte";

    public int ProductId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string TroopNumber { get; set; } = string.Empty;

    public decimal QuantityKg { get; set; }

    public decimal PricePerKg { get; set; }

    public decimal LineTotal => QuantityKg * PricePerKg;

    public bool IsHalfCarcass =>
        string.Equals(LineType, "MediaRes", StringComparison.OrdinalIgnoreCase);
}
