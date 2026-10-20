// Datos para armar el ticket de APERTURA de caja (2026-10-17, ver docs/DECISIONS.md). CierreCaja trae solo ids: la
// sucursal y el usuario se resuelven en el controller y se dejan ya como texto para que el ticket no tenga que
// volver a buscarlos. Mismo patron que ReciboConteoCierreVm.
using Entidades;

namespace WebCore.Models
{
    public class ReciboAperturaCajaVm
    {
        // La caja recien abierta: Id, FechaHoraInicio, CajaInicio (monto inicial) y ConteoBilletesApertura (puede ser null).
        public CierreCaja Cierre { get; set; }
        public Empresa Empresa { get; set; }
        public string SucursalNombre { get; set; }
        // Usuario que abrio la caja (en la cuenta compartida de produccion, la persona elegida como operador).
        public string UsuarioNombre { get; set; }
    }
}
