using System;
using System.Data;

namespace Contratos
{
    // Busqueda de expendios por cliente real (expendios.idpersona), de SOLO LECTURA, para el
    // filtro "Persona" del modal de expendios del POS de Ventas (2026-10-04, ver docs/DECISIONS.md).
    // Interfaz propia, no parte de IVentaRepository, a proposito: esa interfaz la comparten
    // Datos.Venta (SQL Server, clasico/WinForms) y VentaPg, y agregarle un metodo obligaria a tocar
    // codigo del clasico. Solo existe implementacion Postgres.
    public interface IExpendioClienteRepository
    {
        // Mismas columnas que IVentaRepository.obtenerExpendiosAvanzado (una fila por producto de
        // cada expendio, ordenado por fecha), pero solo los expendios guardados con ese cliente.
        // idSucursal = 0 trae todas las sucursales; fechaHasta null = sin tope.
        DataTable obtenerExpendiosPorPersona(int idPersona, DateTime fechaDesde, DateTime? fechaHasta, int idSucursal);
    }
}
