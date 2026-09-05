// Port de Web/Models/ReciboPagoVm.cs (AddOrEditPago, ver docs/10-migracion-aspnet-core/
// PLAN-ADDOREDITPAGO.md). Sin cambios de forma.
using Entidades;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class ReciboPagoVm
    {
        public Pago Pago { get; set; }
        public Empresa Empresa { get; set; }
        public decimal Saldo { get; set; }
        public bool TieneSaldo { get; set; }
        public string TipoOperacion { get; set; }
        public string PersonaEtiqueta { get; set; }
        public string DetalleOperacion { get; set; }
        public string UrlPdfAbsoluta { get; set; }
        public List<string> ComprobantesRelacionados { get; set; }
    }
}
