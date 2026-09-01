using System.ComponentModel.DataAnnotations;

namespace WebCore.Models
{
    // Port de Web/Models/EmpresaVm.cs (pantalla "Mi Empresa", admin de la propia empresa).
    public class EmpresaIndexVm
    {
        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; } = "";

        public string RazonSocialAfip { get; set; } = "";
        public long Cuit { get; set; }

        [Display(Name = "Nombre de fantasía")]
        public string NombreFantasia { get; set; } = "";

        [Display(Name = "Slogan 1")]
        public string Slogan1 { get; set; } = "";

        [Display(Name = "Slogan 2")]
        public string Slogan2 { get; set; } = "";

        [Display(Name = "Slogan 3")]
        public string Slogan3 { get; set; } = "";

        [Display(Name = "Domicilio")]
        public string Domicilio { get; set; } = "";

        [Display(Name = "Ciudad")]
        public string Ciudad { get; set; } = "";

        [Display(Name = "País")]
        public string Pais { get; set; } = "";

        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = "";

        [Display(Name = "Email")]
        public string Email { get; set; } = "";

        [Display(Name = "Jornada diurna - desde")]
        public string HorarioDiurnoDesde { get; set; } = "";

        [Display(Name = "Jornada diurna - hasta")]
        public string HorarioDiurnoHasta { get; set; } = "";

        [Display(Name = "Jornada tarde - desde")]
        public string HorarioTardeDesde { get; set; } = "";

        [Display(Name = "Jornada tarde - hasta")]
        public string HorarioTardeHasta { get; set; } = "";
    }
}
