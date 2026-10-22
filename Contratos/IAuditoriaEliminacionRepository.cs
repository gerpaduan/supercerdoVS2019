using System.Data;

namespace Contratos
{
    // Lectura de la auditoria de registros eliminados (Compras, Stock y Movimientos). La escritura NO esta aca: la hace
    // el mismo metodo que borra el registro (ICompraRepository.eliminarCompraConAuditoria /
    // ICorteRepository.eliminarMovimientoConAuditoria), en la misma transaccion, para que lo auditado sea exactamente
    // lo que se borro. Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
    public interface IAuditoriaEliminacionRepository
    {
        // modulo vacio = todos. texto busca en el motivo, el nro de remito, el id del registro y los nombres de usuario/persona.
        // Columnas: id, modulo, idRegistro, tipoRegistro, fechaRegistro, sucursal, persona, importe, cantLineas,
        // usuarioElimino, usuarioSesion, fechaEliminacion, motivo.
        DataTable listar(string modulo, System.DateTime fechaDesde, System.DateTime fechaHasta, string texto);

        // Cabecera + lineas con nombres resueltos, o null si no existe.
        Entidades.AuditoriaEliminacion obtener(int id);

        // La eliminacion mas reciente de un registro (los ids no se reutilizan, pero se busca por la dudas), o null.
        Entidades.AuditoriaEliminacion obtenerPorRegistro(string modulo, int idRegistro);
    }
}
