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
        // Cliente real fijado en el POS (cualquier sector). 0 = cliente manual (solo texto libre).
        // Opcional: un navegador con JS viejo en cache no lo manda y el expendio se guarda igual.
        public int IdPersona { get; set; }
        // Solo sector PRESUPUESTO: dias de validez desde la vigencia. null = por defecto (90).
        public int? DiasCaducidad { get; set; }
        public string Observaciones { get; set; }
        // Solo sector REMITOS (Negocio.NroRemito): numero sugerido por el POS, editable.
        public string NroRemito { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
        // > 0: en vez de crear, modifica ese expendio ya guardado (modal post-expendio, "Si, modificar").
        public int IdExpendioModificar { get; set; }
        public string PosInstanceId { get; set; }
    }
}
