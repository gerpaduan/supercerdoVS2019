using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class UsuarioIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<UsuarioResumenVm> Items { get; set; } = new List<UsuarioResumenVm>();
    }

    public class UsuarioResumenVm
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Usuario { get; set; } = "";
        public bool Admin { get; set; }
        public bool Activo { get; set; }
        public string Email { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public string SucursalNombre { get; set; } = "";
        public int IdEmpresa { get; set; }
        public bool Bloqueado { get; set; }
    }

    public class UsuarioEditVm
    {
        public int Id { get; set; }
        public bool EsEdicion { get; set; }
        public bool SoloLectura { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; } = "";

        public string Clave { get; set; } = "";
        public bool Admin { get; set; }
        public bool Activo { get; set; } = true;
        public string Email { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public bool PermitirLoginFueraSucursal { get; set; }
        public bool EsUsuarioProduccion { get; set; }
        public bool PuedeOperarPOS { get; set; } = true;
        public int IdEmpresa { get; set; }
        public List<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    }

    public class UsuarioPermisosVm
    {
        public int IdUsuario { get; set; }
        public string UsuarioNombre { get; set; } = "";
        public string UsuarioLogin { get; set; } = "";
        public bool SoloLectura { get; set; }
        public List<UsuarioPermisoItemVm> Items { get; set; } = new List<UsuarioPermisoItemVm>();
    }

    public class UsuarioPermisoItemVm
    {
        public int IdForm { get; set; }
        public string NombrePermiso { get; set; } = "";
        public string Detalle { get; set; } = "";
        public bool PuedeVer { get; set; }
        public int DiasVer { get; set; }
        public bool PuedeEditar { get; set; }
        public int DiasEditar { get; set; }
        public bool SoloRegistrosPropios { get; set; }
    }
}
