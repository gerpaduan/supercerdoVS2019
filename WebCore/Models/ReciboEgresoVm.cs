// Datos para armar el comprobante (ticket termico, PDF y mail) de un egreso de caja. El egreso en si no trae
// el nombre de la sucursal ni los usuarios resueltos (getEgresoCajaById solo carga ids), por eso se arma aca.
using Entidades;

namespace WebCore.Models
{
    public class ReciboEgresoVm
    {
        public EgresoCaja Egreso { get; set; }
        public Empresa Empresa { get; set; }
        public string SucursalNombre { get; set; }
        // Usuario que cargo el egreso (el "Vendedor" del ticket del WinForms).
        public string VendedorNombre { get; set; }
        public string ActualizadoPorNombre { get; set; }
    }
}
