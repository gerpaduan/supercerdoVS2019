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

        // Igual, pero con la FECHA EXACTA de inicio (ventana e historia = desde esa fecha hasta ahora). Lo usa Existencia por
        // sucursales (filtro "Con movimiento" y sugerencias de pedido); la version por dias sigue siendo la de "Que pedir".
        // No admite fechas futuras.
        List<Entidades.ActividadStockVm> ObtenerActividadStock(int idSucursal, System.DateTime desde);

        // Igual, pero acotada ARRIBA: cuenta lo ocurrido entre `desde` y `hasta` (ambos inclusive; `hasta` es un instante, normalmente el
        // ultimo del dia elegido, ver Negocio.ExistenciaPeriodo). Sirve para un periodo ya cerrado (ej. el mes pasado). No admite
        // `desde` futuro ni `hasta` anterior a `desde`.
        List<Entidades.ActividadStockVm> ObtenerActividadStock(int idSucursal, System.DateTime desde, System.DateTime hasta);

        // Un proveedor por producto: el de la compra más reciente (corteproveedor). Si idProveedorPreferido > 0 y el
        // producto también se le compra a ese proveedor, se devuelve ese (para que filtrar por proveedor muestre al
        // proveedor filtrado). 0 = sin preferencia.
        List<Entidades.ProveedorHabitualVm> ObtenerProveedoresHabituales(int idProveedorPreferido);

        // La ULTIMA COMPRA real de cada producto (lineas de compras de tipo Cortes no anuladas con precio por kilo > 0): su precio, su
        // fecha y el proveedor de esa compra, sin importar el proveedor (un solo registro por producto). Es la fuente del precio de
        // "Ingresar pedido" y de Pedidos; NO reemplaza a ObtenerProveedoresHabituales (que sigue usando "Que pedir"). A diferencia de
        // corteproveedor, no depende de una tabla auxiliar incompleta. Ver docs/DECISIONS.md (2026-10-19, "Pedidos: fuente del precio").
        List<Entidades.ProveedorHabitualVm> ObtenerUltimasComprasPorProducto();
    }
}
