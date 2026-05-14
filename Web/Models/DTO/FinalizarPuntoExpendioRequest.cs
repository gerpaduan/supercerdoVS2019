using System;
using System.Collections.Generic;

namespace Web.Models.DTO
{
    public class FinalizarPuntoExpendioRequest
    {
        public DateTime? FechaExpendio { get; set; }
        public string Sector { get; set; }
        public string IdentificacionCliente { get; set; }
        public string Observaciones { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
    }
}
