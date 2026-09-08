using System;

namespace WebCore.Models
{
    // Un evento del modulo "Actividades" (solo admin, 2026-09-07, pedido explicito del usuario --
    // ver docs/DECISIONS.md). Representa cualquiera de las 6 fuentes heterogeneas (cambio de
    // precio, venta anulada, venta con bonificacion manual, egreso de caja, movimiento, compra,
    // formula) ya normalizadas a un formato comun para poder mezclarlas y ordenarlas por fecha.
    public class ActividadItemVm
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = "";
        public string Descripcion { get; set; } = "";

        // Si el evento esta asociado a una venta puntual, permite el link "ver resumen" que abre
        // Ventas/DetalleVenta?id=X&modal=true dentro de un modal (2026-09-07, pedido explicito).
        public int? IdVenta { get; set; }
    }

    public class ActividadesIndexVm
    {
        public string FechaDesde { get; set; } = "";
        public string FechaHasta { get; set; } = "";
        public int Pagina { get; set; } = 1;
        public int TotalPaginas { get; set; }
        public int TotalItems { get; set; }
        public System.Collections.Generic.List<ActividadItemVm> Items { get; set; } = new();
    }
}
