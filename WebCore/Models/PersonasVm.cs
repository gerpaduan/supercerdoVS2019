// Port de Web/Models/PersonasVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class PersonaIndexVm
    {
        public string Filtro { get; set; } = "";
        public List<PersonaResumenVm> Items { get; set; } = new List<PersonaResumenVm>();
    }

    public class PersonaResumenVm
    {
        public int IdPersona { get; set; }
        public int IdEmpresa { get; set; }
        public string Identificacion { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public string Iva { get; set; } = "";
        public string Cuit { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string OtrosDatos { get; set; } = "";
        public bool CtaCte { get; set; }
        public float Bonificacion { get; set; }
        public bool PuedeModificar { get; set; }
    }

    public class PersonaEditVm
    {
        public int IdPersona { get; set; }
        public bool EsEdicion { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public bool TieneMovimientos { get; set; }
        public bool EsAdministrador { get; set; }
        public bool PuedeGestionarCuentaCorriente { get; set; }
        public bool PuedeEditarCamposProtegidos { get; set; }
        public string MensajeRestriccion { get; set; } = "";
        public string Identificacion { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public int? IdIva { get; set; }
        public string Cuit { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string OtrosDatos { get; set; } = "";
        public bool CtaCte { get; set; }
        public string BonificacionTexto { get; set; } = "0";
        public List<SelectListItem> Ivas { get; set; } = new List<SelectListItem>();
    }
}
