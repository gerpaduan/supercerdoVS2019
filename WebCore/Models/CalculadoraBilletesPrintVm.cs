// Port de Web/Models/CalculadoraBilletesPrintVm.cs (batch 7, POS -- ver
// docs/10-migracion-aspnet-core/PLAN-POS-UI.md). Sin cambios de forma, solo namespace/sintaxis C# 12.
using System.Collections.Generic;

namespace WebCore.Models
{
    public class CalculadoraBilletesPrintVm
    {
        public int TicketMm { get; set; }
        public decimal Monedas { get; set; }
        public decimal Total { get; set; }
        public string? Titulo { get; set; }
        public string? DetalleTexto { get; set; }
        public string? Whatsapp { get; set; }
        public List<CalculadoraBilletesDenominacionVm>? Denominaciones { get; set; }
    }

    public class CalculadoraBilletesDenominacionVm
    {
        public int Denominacion { get; set; }
        public int Cantidad { get; set; }
    }
}
