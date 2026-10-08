// Datos para armar el comprobante (ticket termico, PDF y mail) del conteo de cierre que declara el cajero
// (pre-cierre, 2026-10-06, ver docs/DECISIONS.md). CierreCaja trae solo ids: la sucursal y el cajero se resuelven
// en el controller y se dejan ya como texto para que ticket y PDF no tengan que volver a buscarlos.
using Entidades;

namespace WebCore.Models
{
    public class ReciboConteoCierreVm
    {
        public CierreCaja Cierre { get; set; }
        public Empresa Empresa { get; set; }
        public string SucursalNombre { get; set; }
        // Cajero dueno de la caja (el que conto el efectivo).
        public string CajeroNombre { get; set; }
    }
}
