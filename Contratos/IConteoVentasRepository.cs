using System;

namespace Contratos
{
    // Conteo de SOLO LECTURA de lo que traeria un listado de ventas, para avisar antes de una busqueda pesada
    // (2026-11-26, ver docs/DECISIONS.md). Interfaz propia, no parte de IVentaRepository, a proposito: esa interfaz la
    // comparten Datos.Venta (SQL Server, clasico/WinForms) y VentaPg. Solo existe implementacion Postgres.
    public interface IConteoVentasRepository
    {
        // Ventas del periodo (desde y hasta INCLUIDOS, igual que filtran Ventas/Index y Lineas). idSucursal = -1 trae
        // todas. idsExcluidos: ventas que el usuario no puede ver (cuentas corrientes reservadas); puede venir vacio.
        long ContarVentas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, int[] idsExcluidos);

        // Mismo filtro, contando las LINEAS de esas ventas (una fila de "Ver lineas" por cada una).
        long ContarLineas(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, int[] idsExcluidos);
    }
}
