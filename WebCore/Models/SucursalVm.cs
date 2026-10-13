using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebCore.Models
{
    // Port de Web/Models/SucursalVm.cs (pantalla "Mis Sucursales", admin de la propia empresa).
    public class SucursalIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<SucursalResumenVm> Items { get; set; } = new List<SucursalResumenVm>();
    }

    public class SucursalResumenVm
    {
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Localidad { get; set; } = "";
        public bool ValidarUbicacionLogin { get; set; }
    }

    public class SucursalEditVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; } = "";

        public int IdSucursal { get; set; }

        [Required(ErrorMessage = "Ingresá el nombre de la sucursal.")]
        [Display(Name = "Nombre")]
        public string SucursalNombre { get; set; } = "";

        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = "";

        [Display(Name = "Localidad")]
        public string Localidad { get; set; } = "";

        [Display(Name = "Provincia")]
        public string Provincia { get; set; } = "";

        [Display(Name = "País")]
        public string Pais { get; set; } = "";

        [Display(Name = "Latitud")]
        public string Latitud { get; set; } = "";

        [Display(Name = "Longitud")]
        public string Longitud { get; set; } = "";

        [Range(1, 100000, ErrorMessage = "El radio debe ser un valor positivo (en metros).")]
        [Display(Name = "Radio permitido (metros)")]
        public int RadioLoginMetros { get; set; }

        [Display(Name = "Validar ubicación al iniciar sesión")]
        public bool ValidarUbicacionLogin { get; set; }

        // Titular de facturación de la sucursal (titulares fiscales, 2026-10-09). null = no aplica (SQL Server).
        public SucursalTitularBloqueVm? Titular { get; set; }
    }

    // Bloque "Titular de facturación" de Mis Sucursales: quien figura como emisor de las facturas de esta sucursal.
    public class SucursalTitularBloqueVm
    {
        public int IdTitularActual { get; set; }
        public int PuntoVentaActual { get; set; }
        public List<SucursalTitularOpcionVm> Opciones { get; set; } = new List<SucursalTitularOpcionVm>();
    }

    public class SucursalTitularOpcionVm
    {
        public int IdTitular { get; set; }
        public string Nombre { get; set; } = "";
        public long Cuit { get; set; }
        public bool EsPrincipal { get; set; }
        public bool Habilitado { get; set; }
        public bool TieneCertificado { get; set; }
        // Puntos de venta habilitados del titular (vacio en el principal: usa el de la sucursal).
        public List<int> PuntosVenta { get; set; } = new List<int>();
    }
}
