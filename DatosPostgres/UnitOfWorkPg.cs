using System;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion real de Contratos.IUnitOfWork -- una conexion+transaccion Postgres
    // explicita, abierta una sola vez y compartida a mano entre varios repos dentro de una
    // misma operacion de negocio (ver Contratos/IUnitOfWork.cs para el porque). Reemplaza a
    // TransactionScope para el camino Postgres de Negocio.Venta/CierreCaja/CuentaCorriente.
    public sealed class UnitOfWorkPg : Contratos.IUnitOfWork
    {
        public NpgsqlConnection Connection { get; }
        public NpgsqlTransaction Transaction { get; }

        private bool _completado;
        private bool _disposed;

        private UnitOfWorkPg(NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            Connection = connection;
            Transaction = transaction;
        }

        public static UnitOfWorkPg Iniciar(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var cn = new NpgsqlConnection(connectionString);
            cn.Open();
            var tx = cn.BeginTransaction();

            // Mismo mecanismo que ConexionPg.AbrirConTenant (set_config con is_local=true) --
            // se setea una sola vez aca, y vive mientras dure esta transaccion explicita, sin
            // importar cuantos repos Postgres distintos la reusen.
            using (var cmd = new NpgsqlCommand("SELECT set_config('app.id_empresa', @idEmpresa, true);", cn, tx))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa.ToString());
                cmd.ExecuteNonQuery();
            }

            return new UnitOfWorkPg(cn, tx);
        }

        public void Completar()
        {
            Transaction.Commit();
            _completado = true;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (!_completado)
            {
                try { Transaction.Rollback(); } catch { /* ya pudo haber fallado la conexion */ }
            }

            Transaction.Dispose();
            Connection.Dispose();
        }
    }
}
