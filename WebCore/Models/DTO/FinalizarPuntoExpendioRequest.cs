// Port de Web/Models/DTO/FinalizarPuntoExpendioRequest.cs (2026-09-04, PLAN-POS.md batch 7) --
// payload que posea Views/PuntosExpendio/POS.cshtml al finalizar un expendio. Reusa
// WebCore.Models.DTO.LineaVentaDto (ya portado para el flujo de facturacion, mismos campos que el
// original) en vez de duplicar la clase.
using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    public class FinalizarPuntoExpendioRequest
    {
        public DateTime? FechaExpendio { get; set; }
        public string Sector { get; set; }
        public string IdentificacionCliente { get; set; }
        public string Observaciones { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
        public string PosInstanceId { get; set; }
    }
}
