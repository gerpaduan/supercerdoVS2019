// ViewModels de Punto de Expendio > Expendios generados (listado JSON que consume la vista y base de los
// exports PDF/Excel). Los nombres se serializan en camelCase y son los que lee ExpendiosGenerados.cshtml.
using System.Collections.Generic;

namespace WebCore.Models
{
    // Un expendio ya generado, con sus lineas.
    public class ExpendioGeneradoVm
    {
        public int IdExpendio { get; set; }
        // ISO "yyyy-MM-ddTHH:mm:ss" (para ordenar) y los mismos datos ya formateados para mostrar.
        public string FechaExpendio { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Hora { get; set; } = "";
        public string IdentificacionExpendio { get; set; } = "";
        public string NroRemito { get; set; } = "";
        public string Sucursal { get; set; } = "";
        public string Sector { get; set; } = "";
        public string Usuario { get; set; } = "";
        public string CantItems { get; set; } = "0";
        public decimal TotalKg { get; set; }
        public decimal TotalImporte { get; set; }
        public int IdVenta { get; set; }
        // "Pendiente" o "Asignado" (ya cargado en una venta).
        public string Estado { get; set; } = "";
        public string PdfUrl { get; set; } = "";
        public string EmailUrl { get; set; } = "";
        // Persona guardada en el expendio (expendios.idpersona). Solo Postgres: en SQL Server es null.
        public int? IdPersona { get; set; }
        public List<ExpendioGeneradoLineaVm> Lineas { get; set; } = new List<ExpendioGeneradoLineaVm>();
    }

    public class ExpendioGeneradoLineaVm
    {
        public long Codigo { get; set; }
        public string Producto { get; set; } = "";
        public decimal CantKg { get; set; }
        public decimal PrecioKg { get; set; }
        public decimal Total { get; set; }
    }
}
