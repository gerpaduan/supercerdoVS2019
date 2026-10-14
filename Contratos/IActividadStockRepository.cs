// Contrato de lectura para "Qué pedir": actividad reciente (salidas) por producto/sucursal y proveedor habitual.
// Solo lo implementa DatosPostgres.CortePg (la funcionalidad es solo Postgres, ver docs/DECISIONS.md 2026-10-13).
// Sin escritura: son consultas de solo lectura.
using System.Collections.Generic;

namespace Contratos
{
    public interface IActividadStockRepository
    {
        // Salidas (ventas + consumo por elaboración) por producto de stock y sucursal.
        // idSucursal: 0 = todas. diasVentana: días hacia atrás para sumar el consumo. diasHistoria: hasta dónde mirar
        // para encontrar la fecha de la última salida (>= diasVentana). Solo devuelve pares con alguna salida en
        // diasHistoria; los que no figuran no tuvieron salidas.
        List<Entidades.ActividadStockVm> ObtenerActividadStock(int idSucursal, int diasVentana, int diasHistoria);

        // Un proveedor por producto: el de la compra más reciente (corteproveedor). Si idProveedorPreferido > 0 y el
        // producto también se le compra a ese proveedor, se devuelve ese (para que filtrar por proveedor muestre al
        // proveedor filtrado). 0 = sin preferencia.
        List<Entidades.ProveedorHabitualVm> ObtenerProveedoresHabituales(int idProveedorPreferido);
    }
}
