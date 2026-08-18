using System;
using System.Data;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IOtrasClasesRepository (Licencias + VencimientosLicencia).
    // Ver docs/DECISIONS.md 2026-08-18. La tabla Claves (metodo Login en Datos.OtrasClases) no
    // esta en el alcance -- confirmado sin ningun caller en todo el repo, excluida de la migracion.
    public class OtrasClasesPg : Contratos.IOtrasClasesRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public OtrasClasesPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public bool existeLicencia(string nroLicencia)
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(1) FROM licencias WHERE nrolicencia = @nroLicencia;",
                p => p.AddWithValue("nroLicencia", nroLicencia ?? ""));

            return Convert.ToInt32(result) > 0;
        }

        public void agregarLicencia(string nroLicencia, string identificacion)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "INSERT INTO licencias (nrolicencia, identificacion, creado, idempresa) VALUES (@nroLicencia, @identificacion, @creado, @idEmpresa);",
                p =>
                {
                    p.AddWithValue("nroLicencia", nroLicencia ?? "");
                    p.AddWithValue("identificacion", identificacion ?? "");
                    p.AddWithValue("creado", DateTime.Now);
                    p.AddWithValue("idEmpresa", _idEmpresa);
                });
        }

        public DataTable obtenerVencimientoLicencia(DateTime fechaDesde)
        {
            const string sql = @"
                SELECT
                    fechavencimiento,
                    CASE WHEN pagado THEN 'PAGADO' ELSE 'PENDIENTE' END AS pagado,
                    fechapago
                FROM vencimientoslicencia
                WHERE (NOT pagado OR fechavencimiento > @fechaDesde)
                  AND (fechavencimiento < (now() + interval '2 months'))
                ORDER BY fechavencimiento;";

            return DbPg.DataTable(_connectionString, _idEmpresa, sql,
                p => p.AddWithValue("fechaDesde", fechaDesde));
        }

        public DateTime fechaVencimientoLicencia()
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT fechavencimiento FROM vencimientoslicencia WHERE NOT pagado ORDER BY fechavencimiento LIMIT 1;");

            return (result == null || result == DBNull.Value) ? DateTime.MinValue : Convert.ToDateTime(result);
        }

        public bool existePagoLicenciaHoy()
        {
            object result = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT COUNT(1) FROM vencimientoslicencia WHERE fechapago = @fechaPago;",
                p => p.AddWithValue("fechaPago", DateTime.Today));

            return Convert.ToInt32(result) > 0;
        }

        public void agregaVencimientosLicencia(DateTime fechaDesde)
        {
            // Mismo criterio que la version SQL Server: 500 inserts, uno por mes -- se prioriza
            // fidelidad de comportamiento sobre performance (operacion administrativa infrecuente).
            for (int i = 0; i < 500; i++)
            {
                DbPg.NonQuery(_connectionString, _idEmpresa,
                    "INSERT INTO vencimientoslicencia (fechavencimiento, pagado, idempresa) VALUES (@fechaVencimiento, false, @idEmpresa);",
                    p =>
                    {
                        p.AddWithValue("fechaVencimiento", fechaDesde.AddMonths(i));
                        p.AddWithValue("idEmpresa", _idEmpresa);
                    });
            }
        }

        public void agregarPagoCuota(DateTime fechaVencimiento)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa,
                "UPDATE vencimientoslicencia SET pagado = true, fechapago = @fechaPago WHERE fechavencimiento = @fechaVencimiento;",
                p =>
                {
                    p.AddWithValue("fechaVencimiento", fechaVencimiento);
                    p.AddWithValue("fechaPago", DateTime.Today);
                });
        }
    }
}
