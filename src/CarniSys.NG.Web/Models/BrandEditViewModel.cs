using System.ComponentModel.DataAnnotations;

namespace CarniSys.NG.Web.Models;

public sealed class BrandEditViewModel
{
    public int BrandId { get; set; }

    public bool IsEdit { get; set; }

    [Required(ErrorMessage = "El campo Nombre Marca no puede estar vacio.")]
    public string BrandName { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public int? OwnerId { get; set; }

    public string OwnerName { get; set; } = string.Empty;

    public bool IsAdministrator { get; set; }

    public bool IsNameReadOnly { get; set; }

    public bool ConfirmSimilarBrands { get; set; }
}
