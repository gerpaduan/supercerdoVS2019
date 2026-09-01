// Port de Web/Models/SystemAdministrationVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
// Este turno solo porta el slice de Empresas -- las clases de Sucursal/Usuario/AltaRapida se
// copian igual (se necesitan para que el archivo compile igual que el original y para no romper
// el port de esos slices cuando se hagan), pero el repositorio/controller de WebCore todavia no
// las usa.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class SystemAdministrationEmpresaResumenVm
    {
        public int IdEmpresa { get; set; }
        public string RazonSocialAfip { get; set; } = "";
        public string NombreFantasia { get; set; } = "";
        public long Cuit { get; set; }
        public string CondicionIVA { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Activa { get; set; }
    }

    public class SystemAdministrationEmpresaIndexVm
    {
        public List<SystemAdministrationEmpresaResumenVm> Items { get; set; } = new List<SystemAdministrationEmpresaResumenVm>();
    }

    public class SystemAdministrationEmpresaEditVm
    {
        public bool EsEdicion { get; set; }
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "La razon social es obligatoria.")]
        public string RazonSocialAfip { get; set; } = "";

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        public string Cuit { get; set; } = "";

        public string NombreFantasia { get; set; } = "";
        public string Slogan1 { get; set; } = "";
        public string Slogan2 { get; set; } = "";
        public string Slogan3 { get; set; } = "";
        public string Iibb { get; set; } = "";

        [Required(ErrorMessage = "La condicion frente al IVA es obligatoria.")]
        public string CondicionIVA { get; set; } = "";

        public DateTime? InicioActividad { get; set; }
        public string TenantSlug { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string Pais { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string BasePath { get; set; } = "";
        public bool EsRRII { get; set; }
        public string NombreCertificadoPfx { get; set; } = "";
        public string EntornoHomoProd { get; set; } = "";
        public string BaseDatosNombre { get; set; } = "";
        public bool Activa { get; set; }
        public string Observaciones { get; set; } = "";

        public long CodigoGenericoCodigo { get; set; } = 999999;
        public string CodigoGenericoNombre { get; set; } = "Codigo Generico";
        public int CodigoGenericoIdAlicuotaIva { get; set; } = 4;
    }

    public class SystemAdministrationAlicuotaIvaVm
    {
        public int IdAlicuotaIva { get; set; }
        public double Alicuota { get; set; }
    }

    public class SystemAdministrationCondicionIvaVm
    {
        public int Id { get; set; }
        public string Descripcion { get; set; } = "";
    }

    public class SystemAdministrationSucursalResumenVm
    {
        public int IdSucursal { get; set; }
        public int IdEmpresa { get; set; }
        public string EmpresaNombre { get; set; } = "";
        public string Sucursal { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Localidad { get; set; } = "";
        public int CodPuntoVentaAfip { get; set; }
        public string Telefono { get; set; } = "";
        public bool Activa { get; set; }
    }

    public class SystemAdministrationSucursalIndexVm
    {
        public int FiltroEmpresaId { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneActiva { get; set; }
        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();
        public List<SystemAdministrationSucursalResumenVm> Items { get; set; } = new List<SystemAdministrationSucursalResumenVm>();
    }

    public class SystemAdministrationSucursalEditVm
    {
        public bool EsEdicion { get; set; }
        public int IdSucursal { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria.")]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        public string Sucursal { get; set; } = "";

        public string Direccion { get; set; } = "";
        public string Localidad { get; set; } = "";
        public string Provincia { get; set; } = "";
        public string Pais { get; set; } = "";
        public int? CodPuntoVentaAfip { get; set; }
        public string Telefono { get; set; } = "";
        public bool Activa { get; set; } = true;
        public string Observaciones { get; set; } = "";
        public bool TieneTelefono { get; set; }
        public bool TieneActiva { get; set; }
        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();
        public int? IdSucursalOrigenPuntoStock { get; set; }
        public List<SelectListItem> SucursalesParaCopiarPuntoStock { get; set; } = new List<SelectListItem>();
    }

    public class SystemAdministrationUsuarioResumenVm
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public string EmpresaNombre { get; set; } = "";
        public int IdSucursalUser { get; set; }
        public string SucursalNombre { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Admin { get; set; }
        public bool Activo { get; set; }
    }

    public class SystemAdministrationUsuarioIndexVm
    {
        public int FiltroEmpresaId { get; set; }
        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();
        public List<SystemAdministrationUsuarioResumenVm> Items { get; set; } = new List<SystemAdministrationUsuarioResumenVm>();
    }

    public class SystemAdministrationUsuarioEditVm
    {
        public bool EsEdicion { get; set; }
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; } = "";

        public string Clave { get; set; } = "";
        public string ConfirmarClave { get; set; } = "";
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La empresa es obligatoria.")]
        public int IdEmpresa { get; set; }

        public int IdSucursalUser { get; set; }
        public bool Admin { get; set; }
        public bool Activo { get; set; } = true;
        public bool PermitirLoginFueraSucursal { get; set; }
        public List<SelectListItem> Empresas { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> Sucursales { get; set; } = new List<SelectListItem>();
    }

    public class SystemAdministrationAltaRapidaVm
    {
        public bool TieneTelefonoSucursal { get; set; }
        public bool TieneActivaSucursal { get; set; }
        public SystemAdministrationEmpresaEditVm Empresa { get; set; } = new SystemAdministrationEmpresaEditVm { Activa = true };
        public SystemAdministrationSucursalEditVm Sucursal { get; set; } = new SystemAdministrationSucursalEditVm { Activa = true };
        public SystemAdministrationUsuarioEditVm Usuario { get; set; } = new SystemAdministrationUsuarioEditVm { Activo = true, Admin = true };
    }
}
