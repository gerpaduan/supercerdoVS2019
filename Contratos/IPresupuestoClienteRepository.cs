using System.Data;

namespace Contratos
{
    // Consulta de SOLO LECTURA de los presupuestos (expendios del sector PRESUPUESTO) de un
    // cliente, para el historial de precios (F8) de Ventas y de Expendio (2026-10-04, ver
    // docs/DECISIONS.md). Interfaz propia, no parte de IVentaRepository, a proposito: esa interfaz
    // la comparten Datos.Venta (SQL Server, clasico/WinForms) y VentaPg, y agregarle un metodo
    // obligaria a tocar codigo del clasico. Solo existe implementacion Postgres.
    public interface IPresupuestoClienteRepository
    {
        // Una fila por producto de cada presupuesto del cliente, del presupuesto con vigencia mas
        // reciente al mas viejo. Columnas: idexpendio, fechaexpendio (vigencia), fechacaducidad
        // (puede ser DBNull), importe, idcorte, codigo, producto, preciokg, cantkg. Un presupuesto
        // sin lineas (o con producto borrado) igual devuelve una fila con idcorte DBNull.
        DataTable obtenerPresupuestosPorCliente(int idPersona);
    }
}
