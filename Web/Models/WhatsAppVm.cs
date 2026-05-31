using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class WhatsAppConfiguracionVm
    {
        public int IdConfiguracionWhatsApp { get; set; }
        public int IdEmpresa { get; set; }

        [Display(Name = "Activo")]
        public bool Activo { get; set; }

        [Display(Name = "Versión API")]
        [StringLength(20, ErrorMessage = "La versión API no puede superar los 20 caracteres.")]
        public string MetaApiVersion { get; set; }

        [Display(Name = "Phone Number ID")]
        [StringLength(100, ErrorMessage = "El Phone Number ID no puede superar los 100 caracteres.")]
        public string PhoneNumberId { get; set; }

        [Display(Name = "Business Account ID")]
        [StringLength(100, ErrorMessage = "El Business Account ID no puede superar los 100 caracteres.")]
        public string BusinessAccountId { get; set; }

        [Display(Name = "Access Token")]
        [StringLength(500, ErrorMessage = "El Access Token no puede superar los 500 caracteres.")]
        public string AccessToken { get; set; }

        public bool PuedeAdministrar { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public string MensajePermiso { get; set; }
    }
}
