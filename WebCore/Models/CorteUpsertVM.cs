// Port de Web/Models/CorteUpsertVM.cs (ver docs/DECISIONS.md, migracion ASP.NET Core, Modulo 3).
// El original vive en el namespace global (sin "namespace Web.Models { }"); aca se lo mueve a
// WebCore.Models por consistencia con el resto del proyecto -- nada mas lo referencia por su
// namespace original.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebCore.Models
{
    public class CorteUpsertVM
    {
        public int IdCorte { get; set; }

        [Required]
        public long Codigo { get; set; }

        [Required, StringLength(50)]
        public string CorteDesc { get; set; } = "";

        public float PrecioKg { get; set; }

        public string Tipo { get; set; } = "";
        public IEnumerable<SelectListItem> Tipos { get; set; } = new List<SelectListItem>();

        public int? IdMarca { get; set; }
        public string MarcaNombre { get; set; } = "";

        public bool Pesable { get; set; }

        public float Promedio { get; set; }

        public int IdAlicuotaIva { get; set; }
        public IEnumerable<SelectListItem> AlicuotasIva { get; set; } = new List<SelectListItem>();

        public float AlicuotaIva { get; set; }

        public int PuntoStock { get; set; }
        public bool EnCierreStock { get; set; }
        public bool Habilitado { get; set; }
        public bool IngresoRapidoEmbutido { get; set; }

        public int Nivel { get; set; }

        public string ModoCorte { get; set; } = "Ninguno";

        public int? IdCorteMaestro { get; set; }
        public string CorteMaestroNombre { get; set; } = "";

        public float Porcentaje { get; set; }
        public float PorcentajeHueso { get; set; }

        public bool Independiente { get; set; } = true;
        public bool CargaContinua { get; set; }
        public int? SiguienteIdEdicion { get; set; }
        public int? UltimoProductoContinuoId { get; set; }
        public int? RetomarProductoId { get; set; }
        public string FlujoBaseContinuo { get; set; } = "";

        public float? PresentacionUnidades { get; set; }
    }
}
