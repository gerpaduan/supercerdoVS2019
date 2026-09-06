// Port de Web/Models/DTO/FinalizarVentaRequest.cs -- ver docs/10-migracion-aspnet-core/PLAN-POS.md.
// FechaVenta/PosInstanceId agregados 2026-09-06 (Batch 5 del plan de login/permisos reales, ver
// docs/DECISIONS.md): el cliente (forma-pago.js) ya los mandaba desde antes de esta migracion --
// llegaban al servidor y se descartaban en silencio por no existir en el DTO. PosInstanceId
// identifica la pestaña de POS para ResolverOperadorPOS (usuario de produccion); FechaVenta
// habilita la edicion de fecha de venta real (PuedeEditarFechaVenta).
using System;
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    public class FinalizarVentaRequest
    {
        public int IdVenta { get; set; }
        public DateTime? FechaVenta { get; set; }
        public string FormaPago { get; set; }
        public bool EsPagoMixto { get; set; }
        public float Efectivo { get; set; }
        public int IdPersona { get; set; }
        public int IdSucursalPOS { get; set; }
        public bool SoloFormaPago { get; set; }
        public string Observaciones { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
        public List<int> ListaExpendios { get; set; }
        public string PosInstanceId { get; set; }
    }
}
