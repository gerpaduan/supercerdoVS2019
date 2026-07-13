using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarniSys.NG.Web.Models;

public sealed class PersonEditViewModel
{
    public int PersonId { get; set; }

    public bool IsEdit { get; set; }

    public bool IsInitiallyReadOnly { get; set; }

    public bool HasMovements { get; set; }

    public bool IsAdministrator { get; set; }

    public bool CanManageCurrentAccount { get; set; }

    public bool CanEditProtectedFields { get; set; } = true;

    public string RestrictionMessage { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificacion es obligatoria.")]
    public string Identification { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razon social es obligatoria.")]
    public string BusinessName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una condicion frente al IVA.")]
    public int? VatId { get; set; }

    public string TaxId { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public bool HasCurrentAccount { get; set; }

    public string DiscountText { get; set; } = "0";

    public List<SelectListItem> VatOptions { get; set; } = [];
}
