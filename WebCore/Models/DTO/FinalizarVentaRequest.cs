// Port de Web/Models/DTO/FinalizarVentaRequest.cs -- ver docs/10-migracion-aspnet-core/PLAN-POS.md.
// Sin PosInstanceId: era para identificar la pestaña de POS dentro de Session (multi-instancia,
// ver pos-multi-instance.js), que no se porta en este slice bajo el diseño sin estado de servidor
// confirmado en el plan.
using System.Collections.Generic;

namespace WebCore.Models.DTO
{
    public class FinalizarVentaRequest
    {
        public int IdVenta { get; set; }
        public string FormaPago { get; set; }
        public bool EsPagoMixto { get; set; }
        public float Efectivo { get; set; }
        public int IdPersona { get; set; }
        public int IdSucursalPOS { get; set; }
        public bool SoloFormaPago { get; set; }
        public string Observaciones { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
        public List<int> ListaExpendios { get; set; }
    }
}
