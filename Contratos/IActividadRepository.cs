using System;
using System.Data;

namespace Contratos
{
    // Modulo "Actividades" (solo admin, 2026-09-07 -- ver docs/DECISIONS.md). Espeja los 6 metodos
    // reales de DatosPostgres.ActividadPg (reporting de solo lectura cross-dominio, sin logica de
    // negocio transaccional -- por eso las implementaciones no pasan por Negocio/). Cada metodo
    // devuelve un DataTable crudo; el mapeo a texto legible lo arma
    // WebCore/Services/ActividadesFeedService.cs, que consume ambas implementaciones (Postgres/SQL
    // Server) sin diferenciar motor -- las columnas del SELECT deben aliasearse identico en las dos.
    //
    // ObtenerCambiosPrecio: SQL Server no tiene historial de precios (Corte solo guarda el precio
    // vigente, editPrecioCorte no loguea nada) -- la implementacion SQL Server devuelve un
    // DataTable vacio con las columnas esperadas (ver docs/GAPS.md), no lanza ni omite el metodo.
    public interface IActividadRepository
    {
        DataTable ObtenerCambiosPrecio(DateTime desde, DateTime hasta);
        DataTable ObtenerVentasConLineasAnuladas(DateTime desde, DateTime hasta);
        DataTable ObtenerVentasConBonificacionManual(DateTime desde, DateTime hasta);
        DataTable ObtenerMovimientos(DateTime desde, DateTime hasta);
        DataTable ObtenerCompras(DateTime desde, DateTime hasta);
        DataTable ObtenerFormulas(DateTime desde, DateTime hasta);
    }
}
