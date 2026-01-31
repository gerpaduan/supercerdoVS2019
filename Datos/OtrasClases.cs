using System;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class OtrasClases
    {
        private readonly Utilidades.Conexion conn;
        private readonly IEmpresaContext _empresa;

        public OtrasClases(IEmpresaContext empresa)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa));
            conn = new Utilidades.Conexion();
        }

        // =========================
        // LOGIN (arreglo Injection)
        // =========================
        public bool Login(string clave)
        {
            const string sql = "SELECT COUNT(1) FROM Claves WHERE Clave = @clave;";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@clave", SqlDbType.NVarChar, 50).Value = clave ?? "";

                if (cn.State != ConnectionState.Open) cn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        #region Licencia

        public bool existeLicencia(string nroLicencia)
        {
            const string sql = "SELECT COUNT(1) FROM Licencias WHERE nroLicencia = @nroLicencia;";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@nroLicencia", SqlDbType.NVarChar, 50).Value = nroLicencia ?? "";

                if (cn.State != ConnectionState.Open) cn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public void agregarLicencia(string nroLicencia, string identificacion)
        {
            const string sql = @"
                INSERT INTO Licencias (NroLicencia, identificacion, creado)
                VALUES (@nroLicencia, @identificacion, @creado);";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.Add("@nroLicencia", SqlDbType.NVarChar, 50).Value = nroLicencia ?? "";
                cmd.Parameters.Add("@identificacion", SqlDbType.NVarChar, 200).Value = identificacion ?? "";
                cmd.Parameters.Add("@creado", SqlDbType.DateTime2).Value = DateTime.Now;

                if (cn.State != ConnectionState.Open) cn.Open();
                cmd.ExecuteNonQuery();
            }
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

            var dt = new DataTable();

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = fechaDesde;

                da.Fill(dt);
            }

            return dt;
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

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                if (cn.State != ConnectionState.Open) cn.Open();
                object result = cmd.ExecuteScalar();

                return (result == null || result == DBNull.Value)
                    ? DateTime.MinValue
                    : Convert.ToDateTime(result);
            }
        }

        public bool existePagoLicenciaHoy()
        {
            const string sql = "SELECT COUNT(1) FROM VencimientosLicencia WHERE fechaPago = @fechaPago;";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();
                cmd.Parameters.Add("@fechaPago", SqlDbType.Date).Value = DateTime.Today;

                if (cn.State != ConnectionState.Open) cn.Open();
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        public void agregaVencimientosLicencia(DateTime fechaDesde)
        {
            const string sql = @"
                INSERT INTO VencimientosLicencia (fechaVencimiento, pagado)
                VALUES (@fechaVencimiento, @pagado);";

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                // Creamos parámetros una sola vez
                var pFecha = cmd.Parameters.Add("@fechaVencimiento", SqlDbType.DateTime);
                var pPagado = cmd.Parameters.Add("@pagado", SqlDbType.Bit);

                if (cn.State != ConnectionState.Open) cn.Open();

                for (int i = 0; i < 400; i++)
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

            using (SqlConnection cn = conn.conectar(_empresa))
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = conn.TimeOut();

                cmd.Parameters.Add("@fechaVencimiento", SqlDbType.DateTime).Value = fechaVencimiento;
                cmd.Parameters.Add("@pagado", SqlDbType.Bit).Value = true;
                cmd.Parameters.Add("@fechaPago", SqlDbType.Date).Value = DateTime.Today;

                if (cn.State != ConnectionState.Open) cn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}
