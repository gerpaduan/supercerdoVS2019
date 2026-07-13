using System.ComponentModel.DataAnnotations;

namespace CarniSys.NG.Web.Models;

public sealed class ProductEditViewModel
{
    public int ProductId { get; set; }

    public long Code { get; set; }

    public string Description { get; set; } = string.Empty;

    public int? BrandId { get; set; }

    public string BrandName { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El precio debe ser mayor o igual a cero.")]
    public decimal PricePerKilogram { get; set; }

    public bool Weighable { get; set; }

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El promedio debe ser mayor o igual a cero.")]
    public decimal AverageWeight { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El punto stock debe ser mayor o igual a cero.")]
    public int StockPoint { get; set; }

    public bool UseBranchStockPoints { get; set; }

    public List<ProductBranchStockPointEditViewModel> BranchStockPoints { get; set; } = [];

    public bool IncludedInStockClosing { get; set; }

    public bool Enabled { get; set; }

    public bool QuickElaboratedEntry { get; set; }
}

public sealed class ProductBranchStockPointEditViewModel
{
    public int BranchId { get; set; }

    public string BranchName { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "El punto stock por sucursal debe ser mayor o igual a cero.")]
    public int StockPoint { get; set; }
}
