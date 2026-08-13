using System;

namespace Web.Models
{
    // Fila del historial de "último precio por producto" de un cliente en el POS (ver docs/DECISIONS.md).
    public class HistorialPrecioProductoVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public float PrecioKg { get; set; }
        public DateTime? FechaVenta { get; set; }
    }
}
