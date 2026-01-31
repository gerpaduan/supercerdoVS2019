using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Parametros
    {
        private readonly Conexion conn;
        private readonly IEmpresaContext empresa;

        public Parametros(IEmpresaContext empresaContext)
        {
            if (empresaContext == null) throw new ArgumentNullException("empresaContext");
            empresa = empresaContext;
            conn = new Conexion();
        }

        public DataTable ObtenerGrid()
        {
            DataTable dt = new DataTable();

            using (SqlConnection cn = conn.conectar(empresa))
            using (SqlDataAdapter da = new SqlDataAdapter(@"
                SELECT
                    p.idParametro,
                    p.nombre,
                    p.descripcion,
                    p.tipo,
                    ep.valor
                FROM dbo.Parametros p
                LEFT JOIN dbo.EmpresaParametros ep
                       ON ep.idParametro = p.idParametro
                      AND ep.idEmpresa = @idEmpresa
                ORDER BY p.nombre;", cn))
            {
                da.SelectCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = empresa.IdEmpresa;
                da.Fill(dt);
            }

            return dt;
        }

        public void GuardarGrid(DataTable dtParametros)
        {
            if (dtParametros == null) throw new ArgumentNullException("dtParametros");
            if (!dtParametros.Columns.Contains("idParametro")) throw new ArgumentException("Falta la columna idParametro");
            if (!dtParametros.Columns.Contains("valor")) throw new ArgumentException("Falta la columna valor");

            using (SqlConnection cn = conn.conectar(empresa))
            {
                if (cn.State != ConnectionState.Open) cn.Open();

                using (SqlTransaction tx = cn.BeginTransaction())
                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.Transaction = tx;
                    cmd.CommandText = @"
                        MERGE dbo.EmpresaParametros AS T
                        USING (SELECT @idEmpresa AS idEmpresa, @idParametro AS idParametro, @valor AS valor) AS S
                        ON (T.idEmpresa = S.idEmpresa AND T.idParametro = S.idParametro)
                        WHEN MATCHED THEN UPDATE SET valor = S.valor
                        WHEN NOT MATCHED THEN INSERT (idEmpresa, idParametro, valor)
                                           VALUES (S.idEmpresa, S.idParametro, S.valor);";

                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = empresa.IdEmpresa;
                    SqlParameter pIdParametro = cmd.Parameters.Add("@idParametro", SqlDbType.Int);
                    SqlParameter pValor = cmd.Parameters.Add("@valor", SqlDbType.NVarChar, 200);

                    try
                    {
                        foreach (DataRow row in dtParametros.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted) continue;
                            if (row["idParametro"] == DBNull.Value) continue;

                            pIdParametro.Value = Convert.ToInt32(row["idParametro"]);

                            object valObj = row["valor"];
                            if (valObj == DBNull.Value || valObj == null)
                                pValor.Value = DBNull.Value;
                            else
                                pValor.Value = valObj.ToString();

                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

        public Dictionary<string, string> ObtenerDiccionario()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            using (SqlConnection cn = conn.conectar(empresa))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                cmd.CommandText = @"
                    SELECT p.nombre, ep.valor
                    FROM dbo.Parametros p
                    LEFT JOIN dbo.EmpresaParametros ep
                           ON ep.idParametro = p.idParametro
                          AND ep.idEmpresa = @idEmpresa;";

                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = empresa.IdEmpresa;

                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    while (rd.Read())
                    {
                        string nombre = rd.GetString(0);
                        string valor = rd.IsDBNull(1) ? "" : rd.GetString(1);
                        dict[nombre] = valor;
                    }
                }
            }

            return dict;
        }

        // Puede devolver null si no existe valor para esa empresa
        public string ObtenerValor(string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío");

            using (SqlConnection cn = conn.conectar(empresa))
            using (SqlCommand cmd = cn.CreateCommand())
            {
                if (cn.State != ConnectionState.Open) cn.Open();
                cmd.CommandText = @"
                    SELECT ep.valor
                    FROM dbo.Parametros p
                    LEFT JOIN dbo.EmpresaParametros ep
                           ON ep.idParametro = p.idParametro
                          AND ep.idEmpresa = @idEmpresa
                    WHERE p.nombre = @nombre;";

                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = empresa.IdEmpresa;
                cmd.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombreParametro;

                object obj = cmd.ExecuteScalar();
                if (obj == null || obj == DBNull.Value) return null;
                return obj.ToString();
            }
        }

        public void SetValor(string nombreParametro, string valor)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío");

            using (SqlConnection cn = conn.conectar(empresa))
            {
                if (cn.State != ConnectionState.Open) cn.Open();

                int idParametro;
                using (SqlCommand cmdGet = cn.CreateCommand())
                {
                    cmdGet.CommandText = "SELECT idParametro FROM dbo.Parametros WHERE nombre = @nombre";
                    cmdGet.Parameters.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombreParametro;

                    object obj = cmdGet.ExecuteScalar();
                    if (obj == null || obj == DBNull.Value)
                        throw new InvalidOperationException("No existe el parámetro '" + nombreParametro + "' en dbo.Parametros.");

                    idParametro = Convert.ToInt32(obj);
                }

                using (SqlCommand cmd = cn.CreateCommand())
                {
                    cmd.CommandText = @"
                        MERGE dbo.EmpresaParametros AS T
                        USING (SELECT @idEmpresa AS idEmpresa, @idParametro AS idParametro, @valor AS valor) AS S
                        ON (T.idEmpresa = S.idEmpresa AND T.idParametro = S.idParametro)
                        WHEN MATCHED THEN UPDATE SET valor = S.valor
                        WHEN NOT MATCHED THEN INSERT (idEmpresa, idParametro, valor)
                                           VALUES (S.idEmpresa, S.idParametro, S.valor);";

                    cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = empresa.IdEmpresa;
                    cmd.Parameters.Add("@idParametro", SqlDbType.Int).Value = idParametro;

                    // si valor es null, guardamos NULL
                    cmd.Parameters.Add("@valor", SqlDbType.NVarChar, 200).Value =
                        (object)valor ?? DBNull.Value;

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
