using System;

namespace Entidades
{
    public class ConfiguracionWhatsApp
    {
        public int IdConfiguracionWhatsApp { get; set; }
        public int IdEmpresa { get; set; }
        public bool Activo { get; set; }
        public string MetaApiVersion { get; set; }
        public string PhoneNumberId { get; set; }
        public string BusinessAccountId { get; set; }
        public string AccessToken { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime FechaModificacion { get; set; }
        public int? IdUsuarioModificacion { get; set; }
    }

    public class WhatsAppEnvio
    {
        public int IdWhatsAppEnvio { get; set; }
        public int IdEmpresa { get; set; }
        public int? IdVenta { get; set; }
        public int? IdPersona { get; set; }
        public string TelefonoOriginal { get; set; }
        public string TelefonoFormateado { get; set; }
        public string NombreArchivo { get; set; }
        public string MediaId { get; set; }
        public string Estado { get; set; }
        public bool Exito { get; set; }
        public string MensajeError { get; set; }
        public string RespuestaApi { get; set; }
        public DateTime FechaAlta { get; set; }
        public int? IdUsuarioAlta { get; set; }
    }
}
