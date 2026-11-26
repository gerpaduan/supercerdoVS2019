// Acceso a posclaveunica (Postgres) / PosClaveUnica (SQL Server): clave unica de cada operacion del POS que trae un
// codigo del navegador. Implementaciones: DatosPostgres.PosClaveUnicaPg y Datos.PosClaveUnica. Ver
// docs/03-modulos/ventas-y-pos.md "POS sin conexion".
using System;

namespace Contratos
{
    public interface IPosClaveUnicaRepository
    {
        // Registro existente con ese clientId (de cualquier tipo), o null.
        Entidades.PosClaveUnica ObtenerPorClientId(Guid clientId);

        // Inserta la clave. Si unitOfWork no es null (Postgres) va en esa transaccion; en SQL Server se enlista en el
        // TransactionScope ambiente. Si el clientId ya existe tira ClaveDuplicadaException (y la transaccion de quien
        // llama queda para deshacer).
        void Insertar(Entidades.PosClaveUnica clave, IUnitOfWork unitOfWork = null);

        // Ventas subidas desde el POS sin conexion de un vendedor en una sucursal con fecha de venta (fechalocal) entre
        // desde y hasta (inclusive): las que caen en una caja. Para los avisos del cierre de caja.
        System.Collections.Generic.List<Entidades.PosClaveUnica> ListarVentasOfflineEnRango(int idUsuario, int idSucursal, DateTime desde, DateTime hasta);
    }

    // El clientId ya estaba registrado: la operacion ya se habia grabado antes (reintento).
    public class ClaveDuplicadaException : Exception
    {
        public Guid ClientId { get; }

        public ClaveDuplicadaException(Guid clientId, Exception inner)
            : base("La operacion con clave " + clientId + " ya estaba registrada.", inner)
        {
            ClientId = clientId;
        }
    }
}
