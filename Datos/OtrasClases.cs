using System;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class OtrasClases
    {
        private readonly IEmpresaContext _empresa;

        public OtrasClases(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
        }

        // =========================
        // LOGIN (arreglo Injection)
        // =========================
        public bool Login(string clave)
        {
            const string sql = "SELECT COUNT(1) FROM Claves WHERE Clave = @clave;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@clave", SqlDbType.NVarChar, 50).Value = clave ?? ""
            );

            int count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return count > 0;
        }

        #region Licencia

        public bool existeLicencia(string nroLicencia)
        {
            const string sql = "SELECT COUNT(1) FROM Licencias WHERE nroLicencia = @nroLicencia;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@nroLicencia", SqlDbType.NVarChar, 50).Value = nroLicencia ?? ""
            );

            int count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return count > 0;
        }

        public void agregarLicencia(string nroLicencia, string identificacion)
        {
            const string sql = @"
                INSERT INTO Licencias (NroLicencia, identificacion, creado)
                VALUES (@nroLicencia, @identificacion, @creado);";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@nroLicencia", SqlDbType.NVarChar, 50).Value = nroLicencia ?? "";
                    p.Add("@identificacion", SqlDbType.NVarChar, 200).Value = identificacion ?? "";
                    p.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;
                }
            );
        }

        #endregion

        #region VencimientosLicencia

        public DataTable obtenerVencimientoLicencia(DateTime fechaDesde)
        {
            const string sql = @"
                SELECT
                    fechaVencimiento,
                    CASE WHEN pagado = 1 THEN 'PAGADO' ELSE 'PENDIENTE' END AS pagado,
                    fechaPago
                FROM VencimientosLicencia
                WHERE (pagado = 0 OR fechaVencimiento > @fechaDesde)
                  AND (fechaVencimiento < DATEADD(MONTH, 2, GETDATE()))
                ORDER BY fechaVencimiento;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde
            );
        }

        /// <summary>
        /// Devuelve el próximo vencimiento impago.
        /// Si no hay registros impagos, devuelve DateTime.MinValue.
        /// (Si preferís null, lo cambio a DateTime?).
        /// </summary>
        public DateTime fechaVencimientoLicencia()
        {
            const string sql = @"
                SELECT TOP 1 fechaVencimiento
                FROM VencimientosLicencia
                WHERE pagado = 0
                ORDER BY fechaVencimiento;";

            object result = Db.Scalar(_empresa, sql, CommandType.Text);

            return (result == null || result == DBNull.Value)
                ? DateTime.MinValue
                : Convert.ToDateTime(result);
        }

        public bool existePagoLicenciaHoy()
        {
            const string sql = "SELECT COUNT(1) FROM VencimientosLicencia WHERE fechaPago = @fechaPago;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@fechaPago", SqlDbType.Date).Value = DateTime.Today
            );

            int count = (result == null || result == DBNull.Value) ? 0 : Convert.ToInt32(result);
            return count > 0;
        }

        public void agregaVencimientosLicencia(DateTime fechaDesde)
        {
            const string sql = @"
                INSERT INTO VencimientosLicencia (fechaVencimiento, pagado)
                VALUES (@fechaVencimiento, @pagado);";

            // Acá conviene mantener una sola conexión abierta para el loop (más rápido).
            using (SqlConnection cn = Db.Open(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;

                // parámetros una sola vez
                var pFecha = cmd.Parameters.Add("@fechaVencimiento", SqlDbType.DateTime);
                var pPagado = cmd.Parameters.Add("@pagado", SqlDbType.Bit);

                for (int i = 0; i < 500; i++)
                {
                    pFecha.Value = fechaDesde.AddMonths(i);
                    pPagado.Value = false;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void agregarPagoCuota(DateTime fechaVencimiento)
        {
            const string sql = @"
                UPDATE VencimientosLicencia
                SET pagado = @pagado,
                    fechaPago = @fechaPago
                WHERE fechaVencimiento = @fechaVencimiento;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@fechaVencimiento", SqlDbType.DateTime).Value = fechaVencimiento;
                    p.Add("@pagado", SqlDbType.Bit).Value = true;
                    p.Add("@fechaPago", SqlDbType.Date).Value = DateTime.Today;
                }
            );
        }

        #endregion
    }
}
