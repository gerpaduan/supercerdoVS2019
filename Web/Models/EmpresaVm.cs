using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    // ViewModel de la pantalla "Mi Empresa" (admin de la propia empresa). Deliberadamente NO
    // incluye campos AFIP/infraestructura del tenant (RazonSocialAfip, Cuit, Iibb, CondicionIVA,
    // InicioActividad, TenantSlug, BasePath, EsRRII, NombreCertificado_pfx, Entorno_HOMO_PROD,
    // BaseDatosNombre, Activa) -- esos quedan reservados a SystemAdministrationController
    // (super-admin de plataforma). RazonSocialAfip/Cuit se muestran de solo lectura como
    // referencia, no se editan desde aca.
    public class EmpresaIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; }

        public string RazonSocialAfip { get; set; }
        public long Cuit { get; set; }

        [Display(Name = "Nombre de fantasía")]
        public string NombreFantasia { get; set; }

        [Display(Name = "Slogan 1")]
        public string Slogan1 { get; set; }

        [Display(Name = "Slogan 2")]
        public string Slogan2 { get; set; }

        [Display(Name = "Slogan 3")]
        public string Slogan3 { get; set; }

        [Display(Name = "Domicilio")]
        public string Domicilio { get; set; }

        [Display(Name = "Ciudad")]
        public string Ciudad { get; set; }

        [Display(Name = "País")]
        public string Pais { get; set; }

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        // Formato "HH:mm", ligados a <input type="time">. Default 00:00/23:59 = sin restricción.
        [Display(Name = "Jornada diurna - desde")]
        public string HorarioDiurnoDesde { get; set; }

        [Display(Name = "Jornada diurna - hasta")]
        public string HorarioDiurnoHasta { get; set; }

        [Display(Name = "Jornada tarde - desde")]
        public string HorarioTardeDesde { get; set; }

        [Display(Name = "Jornada tarde - hasta")]
        public string HorarioTardeHasta { get; set; }
    }
}
