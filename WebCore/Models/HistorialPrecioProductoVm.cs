using System;

namespace WebCore.Models
{
    // Fila del historial de "ultimo precio por producto" de un cliente en el POS (F8), 2026-09-06
    // (retomado -- ver docs/DECISIONS.md). Port literal de Web/Models/HistorialPrecioProductoVm.cs.
    public class HistorialPrecioProductoVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public float PrecioKg { get; set; }
        public DateTime? FechaVenta { get; set; }
    }
}
