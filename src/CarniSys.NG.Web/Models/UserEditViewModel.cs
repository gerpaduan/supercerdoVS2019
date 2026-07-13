using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CarniSys.NG.Web.Models;

public sealed class UserEditViewModel
{
    public int UserId { get; set; }

    public bool IsEdit { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    public string Login { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool IsAdministrator { get; set; }

    public bool IsActive { get; set; } = true;

    public string Email { get; set; } = string.Empty;

    public int BranchId { get; set; }

    public bool CanLoginOutsideBranch { get; set; }

    public int CompanyId { get; set; }

    public List<SelectListItem> Branches { get; set; } = [];
}
