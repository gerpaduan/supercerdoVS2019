using System.ComponentModel.DataAnnotations;

namespace CarniSys.NG.Web.Models;

public sealed class UserPermissionEditViewModel
{
    public int UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserLogin { get; set; } = string.Empty;

    public List<UserPermissionEditItemViewModel> Items { get; set; } = [];
}

public sealed class UserPermissionEditItemViewModel
{
    public int FormId { get; set; }

    public string FormName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool CanRead { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los dias de lectura deben ser cero o mayores.")]
    public int ReadDays { get; set; }

    public bool CanEdit { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Los dias de edicion deben ser cero o mayores.")]
    public int EditDays { get; set; }

    public bool OwnRecordsOnly { get; set; } = true;
}
