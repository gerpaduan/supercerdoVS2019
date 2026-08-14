using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    // ViewModels de la pantalla "Mis Sucursales" (admin de la propia empresa). No confundir con
    // SystemAdministrationSucursalEditVm, que es el equivalente cross-tenant del super-admin de
    // plataforma. Deliberadamente NO incluye CodPuntoVentaAfip (unico campo AFIP de la entidad).
    public class SucursalIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public List<SucursalResumenVm> Items { get; set; }

        public SucursalIndexVm()
        {
            Items = new List<SucursalResumenVm>();
        }
    }

    public class SucursalResumenVm
    {
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public bool ValidarUbicacionLogin { get; set; }
    }

    public class SucursalEditVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; }

        public int IdSucursal { get; set; }

        [Required(ErrorMessage = "Ingresá el nombre de la sucursal.")]
        [Display(Name = "Nombre")]
        public string SucursalNombre { get; set; }

        [Display(Name = "Dirección")]
        public string Direccion { get; set; }

        [Display(Name = "Localidad")]
        public string Localidad { get; set; }

        [Display(Name = "Provincia")]
        public string Provincia { get; set; }

        [Display(Name = "País")]
        public string Pais { get; set; }

        // Texto libre ("-34.6037" / "-34,6037") para admitir coma o punto decimal, igual que el
        // resto de los campos de coordenadas de esta app (ver Web/Models/LoginVm.cs).
        [Display(Name = "Latitud")]
        public string Latitud { get; set; }

        [Display(Name = "Longitud")]
        public string Longitud { get; set; }

        [Range(1, 100000, ErrorMessage = "El radio debe ser un valor positivo (en metros).")]
        [Display(Name = "Radio permitido (metros)")]
        public int RadioLoginMetros { get; set; }

        [Display(Name = "Validar ubicación al iniciar sesión")]
        public bool ValidarUbicacionLogin { get; set; }
    }
}
