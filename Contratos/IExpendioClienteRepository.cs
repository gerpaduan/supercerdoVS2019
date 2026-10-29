using System;
using System.Data;

namespace Contratos
{
    // Busqueda de expendios por persona, de SOLO LECTURA, para el filtro "Persona" del modal de
    // expendios del POS de Ventas (2026-10-04, ver docs/DECISIONS.md; coincidencia ampliada el 2026-10-29).
    // Interfaz propia, no parte de IVentaRepository, a proposito: esa interfaz la comparten
    // Datos.Venta (SQL Server, clasico/WinForms) y VentaPg, y agregarle un metodo obligaria a tocar
    // codigo del clasico. Solo existe implementacion Postgres.
    public interface IExpendioClienteRepository
    {
        // Mismas columnas que IVentaRepository.obtenerExpendiosAvanzado (una fila por producto de
        // cada expendio, ordenado por fecha), pero solo los expendios de esa persona: los guardados con
        // su id (expendios.idpersona) y los cuya Identificacion cliente es igual a la identificacion de la persona.
        // idSucursal = 0 trae todas las sucursales; fechaHasta null = sin tope.
        DataTable obtenerExpendiosPorPersona(int idPersona, DateTime fechaDesde, DateTime? fechaHasta, int idSucursal);
    }
}
