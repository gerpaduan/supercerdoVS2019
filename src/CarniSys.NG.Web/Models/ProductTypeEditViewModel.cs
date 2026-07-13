using System.ComponentModel.DataAnnotations;

namespace CarniSys.NG.Web.Models;

public sealed class ProductTypeEditViewModel
{
    public string OriginalTypeName { get; set; } = string.Empty;

    public bool IsEdit { get; set; }

    [Required(ErrorMessage = "El campo Tipo no puede ser vacio.")]
    public string TypeName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "El campo Orden debe ser un numero entero mayor a cero.")]
    public int SortOrder { get; set; } = 100;

    public bool IsReserved { get; set; }
}
