using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Web.Models
{
    public class SystemAdministrationEmpresaResumenVm
    {
        public int IdEmpresa { get; set; }
        public string RazonSocialAfip { get; set; }
        public string NombreFantasia { get; set; }
        public long Cuit { get; set; }
        public string CondicionIVA { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activa { get; set; }
    }

    public class SystemAdministrationEmpresaIndexVm
    {
        public SystemAdministrationEmpresaIndexVm()
        {
            Items = new List<SystemAdministrationEmpresaResumenVm>();
        }

        public List<SystemAdministrationEmpresaResumenVm> Items { get; set; }
    }

    public class SystemAdministrationEmpresaEditVm
    {
        public bool EsEdicion { get; set; }
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "La razon social es obligatoria.")]
        public string RazonSocialAfip { get; set; }

        [Required(ErrorMessage = "El CUIT es obligatorio.")]
        public string Cuit { get; set; }

        public string NombreFantasia { get; set; }
        public string Slogan1 { get; set; }
        public string Slogan2 { get; set; }
        public string Slogan3 { get; set; }
        public string Iibb { get; set; }

        [Required(ErrorMessage = "La condicion frente al IVA es obligatoria.")]
        public string CondicionIVA { get; set; }

        public DateTime? InicioActividad { get; set; }
        public string TenantSlug { get; set; }
        public string Domicilio { get; set; }
        public string Ciudad { get; set; }
        public string Pais { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string BasePath { get; set; }
        public bool EsRRII { get; set; }
        public string NombreCertificadoPfx { get; set; }
        public string EntornoHomoProd { get; set; }
        public string BaseDatosNombre { get; set; }
        public bool Activa { get; set; }
        public string Observaciones { get; set; }

        // Producto "Codigo Generico" precargado en el alta de empresa (editable). Se ignora en
        // modo edicion (EsEdicion=true) -- solo aplica al crear la empresa. Ver
        // SystemAdministrationRepository.CrearProductoCodigoGenericoInterna.
        public long CodigoGenericoCodigo { get; set; } = 999999;
        public string CodigoGenericoNombre { get; set; } = "Codigo Generico";
        public int CodigoGenericoIdAlicuotaIva { get; set; } = 4; // 10,5%
    }

    public class SystemAdministrationAlicuotaIvaVm
    {
        public int IdAlicuotaIva { get; set; }
        public double Alicuota { get; set; }
    }

    public class SystemAdministrationCondicionIvaVm
    {
        public int Id { get; set; }
        public string Descripcion { get; set; }
    }

    public class SystemAdministrationSucursalResumenVm
    {
        public int IdSucursal { get; set; }
        public int IdEmpresa { get; set; }
        public string EmpresaNombre { get; set; }
        public string Sucursal { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public int CodPuntoVentaAfip { get; set; }
        public string Telefono { get; set; }
        public bool Activa { get; set; }
    }

    public class SystemAdministrationSucursalIndexVm
    {
        public SystemAdministrationSucursalIndexVm()
        {
            Empresas = new List<SelectListItem>();
            Items = new List<SystemAdministrationSucursalResumenVm>();
        }

        public int FiltroEmpresaId { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneActiva { get; set; }
        public List<SelectListItem> Empresas { get; set; }
        public List<SystemAdministrationSucursalResumenVm> Items { get; set; }
    }

    public class SystemAdministrationSucursalEditVm
    {
        public SystemAdministrationSucursalEditVm()
        {
            Empresas = new List<SelectListItem>();
            SucursalesParaCopiarPuntoStock = new List<SelectListItem>();
            Activa = true;
        }

        public bool EsEdicion { get; set; }
        public int IdSucursal { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria.")]
        public int IdEmpresa { get; set; }

        [Required(ErrorMessage = "El nombre de la sucursal es obligatorio.")]
        public string Sucursal { get; set; }

        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }
        public string Pais { get; set; }
        public int? CodPuntoVentaAfip { get; set; }
        public string Telefono { get; set; }
        public bool Activa { get; set; }
        public string Observaciones { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneActiva { get; set; }
        public List<SelectListItem> Empresas { get; set; }

        // Solo se usa en el alta (EsEdicion = false): de que sucursal existente de la misma
        // empresa copiar los puntos de stock por producto. 0/null = todos los productos
        // arrancan en punto de stock 0 para esta sucursal nueva.
        public int? IdSucursalOrigenPuntoStock { get; set; }
        public List<SelectListItem> SucursalesParaCopiarPuntoStock { get; set; }
    }

    public class SystemAdministrationUsuarioResumenVm
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public string EmpresaNombre { get; set; }
        public int IdSucursalUser { get; set; }
        public string SucursalNombre { get; set; }
        public string Nombre { get; set; }
        public string Usuario { get; set; }
        public string Email { get; set; }
        public bool Admin { get; set; }
        public bool Activo { get; set; }
    }

    public class SystemAdministrationUsuarioIndexVm
    {
        public SystemAdministrationUsuarioIndexVm()
        {
            Empresas = new List<SelectListItem>();
            Items = new List<SystemAdministrationUsuarioResumenVm>();
        }

        public int FiltroEmpresaId { get; set; }
        public List<SelectListItem> Empresas { get; set; }
        public List<SystemAdministrationUsuarioResumenVm> Items { get; set; }
    }

    public class SystemAdministrationUsuarioEditVm
    {
        public SystemAdministrationUsuarioEditVm()
        {
            Empresas = new List<SelectListItem>();
            Sucursales = new List<SelectListItem>();
            Activo = true;
        }

        public bool EsEdicion { get; set; }
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string Usuario { get; set; }

        public string Clave { get; set; }
        public string ConfirmarClave { get; set; }
        public string Email { get; set; }

        [Required(ErrorMessage = "La empresa es obligatoria.")]
        public int IdEmpresa { get; set; }

        public int IdSucursalUser { get; set; }
        public bool Admin { get; set; }
        public bool Activo { get; set; }
        public bool PermitirLoginFueraSucursal { get; set; }
        public List<SelectListItem> Empresas { get; set; }
        public List<SelectListItem> Sucursales { get; set; }
    }

    public class SystemAdministrationAltaRapidaVm
    {
        public SystemAdministrationAltaRapidaVm()
        {
            Empresa = new SystemAdministrationEmpresaEditVm
            {
                Activa = true
            };
            Sucursal = new SystemAdministrationSucursalEditVm
            {
                Activa = true
            };
            Usuario = new SystemAdministrationUsuarioEditVm
            {
                Activo = true,
                Admin = true
            };
        }

        public bool TieneTelefonoSucursal { get; set; }
        public bool TieneActivaSucursal { get; set; }
        public SystemAdministrationEmpresaEditVm Empresa { get; set; }
        public SystemAdministrationSucursalEditVm Sucursal { get; set; }
        public SystemAdministrationUsuarioEditVm Usuario { get; set; }
    }
}
