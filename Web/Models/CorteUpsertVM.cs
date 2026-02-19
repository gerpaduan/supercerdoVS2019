using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

public class CorteUpsertVM
{
    public int IdCorte { get; set; }

    [Required]
    public long Codigo { get; set; }

    [Required, StringLength(50)]
    public string CorteDesc { get; set; }

    public float PrecioKg { get; set; }

    // Tipo se guarda como string (nvarchar(50)) pero se elige desde tabla
    public string Tipo { get; set; }
    public IEnumerable<SelectListItem> Tipos { get; set; }

    public int? IdMarca { get; set; }
    public string MarcaNombre { get; set; }

    public bool Pesable { get; set; }

    public float Promedio { get; set; }

    public int IdAlicuotaIva { get; set; }
    public IEnumerable<SelectListItem> AlicuotasIva { get; set; }

    // opcional (si querés guardar el % también)
    public float AlicuotaIva { get; set; }

    public int PuntoStock { get; set; }
    public bool EnCierreStock { get; set; }
    public bool Habilitado { get; set; }
    public bool IngresoRapidoEmbutido { get; set; }

    public int Nivel { get; set; }

    // Corte Maestro / Presentación
    public string ModoCorte { get; set; } = "Ninguno"; // Ninguno | CorteMaestro | Presentacion

    public int? IdCorteMaestro { get; set; }
    public string CorteMaestroNombre { get; set; }

    public float Porcentaje { get; set; }         // % (o 100 en presentación)
    public float PorcentajeHueso { get; set; }    // Desperdicio (o 100*(n-1) en presentación)

    public bool Independiente { get; set; } = true;

    // solo UI (no DB)
    public float? PresentacionUnidades { get; set; }
}
