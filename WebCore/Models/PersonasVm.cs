// Port de Web/Models/PersonasVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class PersonaIndexVm
    {
        public string Filtro { get; set; } = "";
        // Filtro por condicion de IVA: abreviatura (columna "iva" del listado, la devuelven los dos motores). "" = todas.
        public string Iva { get; set; } = "";
        public List<SelectListItem> Ivas { get; set; } = new List<SelectListItem>();
        // Muestra el icono de cuenta corriente en Acciones: usuario con permiso, o cuenta de produccion (el
        // permiso lo tiene el operador que se identifica al entrar, ver Finanzas/AutorizarModuloCtaCte).
        public bool MostrarCtaCte { get; set; }
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
        // Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01): se muestra como etiqueta.
        public bool CtaCteReservada { get; set; }
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
        // Eliminar persona (docs/DECISIONS.md, 2026-11-02): el boton aparece solo para un admin sobre una
        // persona existente; si hay motivo para no eliminarla se muestra deshabilitado con ese motivo.
        public bool MostrarEliminar { get; set; }
        public string MotivoNoEliminable { get; set; } = "";
        public string Identificacion { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public int? IdIva { get; set; }
        public string Cuit { get; set; } = "";
        public string Telefono { get; set; } = "";
        public string Email { get; set; } = "";
        public string Domicilio { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string OtrosDatos { get; set; } = "";
        // Cuenta corriente reservada (docs/DECISIONS.md, 2026-10-01). Reemplaza al viejo switch
        // "Cuenta corriente" (Persona.CtaCte), que ya no se edita desde la UI.
        public bool CtaCteReservada { get; set; }
        public string BonificacionTexto { get; set; } = "0";
        public List<SelectListItem> Ivas { get; set; } = new List<SelectListItem>();
    }
}
