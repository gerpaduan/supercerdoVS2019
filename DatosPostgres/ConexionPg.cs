using System;
using System.Transactions;
using Npgsql;

namespace DatosPostgres
{
    // Equivalente a Utilidades/Conexion.cs pero para Postgres. No lee configuracion por
    // su cuenta (queda netstandard2.0 puro, sin depender de System.Web/ConfigurationManager,
    // para poder reusarse igual desde un futuro proyecto ASP.NET Core) -- el caller (hoy
    // Web/, con .NET Framework) le pasa el connection string explicito.
    public sealed class ConexionPg
    {
        public static NpgsqlConnection AbrirConTenant(string connectionString, int idEmpresa, out NpgsqlTransaction transaccion)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString));

            var cn = new NpgsqlConnection(connectionString);
            cn.Open();

            // Si ya hay un TransactionScope ambiente (ej. Negocio.Compra/Venta/CuentaCorriente,
            // pensados para SQL Server), NO abrimos una transaccion explicita propia -- Npgsql
            // se auto-enlista en la ambiente. Abrir cn.BeginTransaction() ademas del enlistment
            // automatico produce "A transaction is already in progress" (encontrado en el
            // testeo profundo de escritura real, 2026-08-20, ver docs/DECISIONS.md). Sin
            // TransactionScope ambiente (el caso normal de un request web), comportamiento
            // identico a siempre: transaccion explicita propia.
            transaccion = System.Transactions.Transaction.Current == null
                ? cn.BeginTransaction()
                : null;

            // set_config(..., is_local=true) es el equivalente parametrizable de SET LOCAL
            // (SET LOCAL no acepta bind parameters). El valor vive y muere con la transaccion,
            // asi que una conexion reciclada por el pool nunca puede arrastrar el tenant de
            // una request anterior -- ver docs/06-datos-e-integraciones/rls-postgres.md.
            using (var cmd = new NpgsqlCommand("SELECT set_config('app.id_empresa', @idEmpresa, true);", cn, transaccion))
            {
                cmd.Parameters.AddWithValue("idEmpresa", idEmpresa.ToString());
                cmd.ExecuteNonQuery();
            }

            return cn;
        }
    }
}
