using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Parametros
    {
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        public Parametros(IEmpresaContext empresaContext)
        {
            _empresa = empresaContext ?? throw new ArgumentNullException(nameof(empresaContext));
        }

        public DataTable ObtenerGrid()
        {
            bool tieneColumnaTipo = TieneColumna("dbo.Parametros", "tipo");

            string sql = @"
                SELECT
                    p.idParametro,
                    p.nombre,
                    p.descripcion,
                    " + (tieneColumnaTipo ? "ISNULL(p.tipo, 0)" : "CAST(0 AS int)") + @" AS tipo,
                    ep.valor
                FROM dbo.Parametros p
                LEFT JOIN dbo.EmpresaParametros ep
                       ON ep.idParametro = p.idParametro
                      AND ep.idEmpresa = @idEmpresa
                ORDER BY p.nombre;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                }
            );
        }

        public void GuardarGrid(DataTable dtParametros)
        {
            if (dtParametros == null) throw new ArgumentNullException(nameof(dtParametros));
            if (!dtParametros.Columns.Contains("idParametro")) throw new ArgumentException("Falta la columna idParametro");
            if (!dtParametros.Columns.Contains("valor")) throw new ArgumentException("Falta la columna valor");

            const string mergeSql = @"
                MERGE dbo.EmpresaParametros AS T
                USING (SELECT @idEmpresa AS idEmpresa, @idParametro AS idParametro, @valor AS valor) AS S
                ON (T.idEmpresa = S.idEmpresa AND T.idParametro = S.idParametro)
                WHEN MATCHED THEN UPDATE SET valor = S.valor
                WHEN NOT MATCHED THEN INSERT (idEmpresa, idParametro, valor)
                                   VALUES (S.idEmpresa, S.idParametro, S.valor);";

            // Mantengo tu transacción (es lo correcto para guardado masivo)
            using (SqlConnection cn = Db.Open(_empresa))
            using (SqlTransaction tx = cn.BeginTransaction())
            using (SqlCommand cmd = new SqlCommand(mergeSql, cn, tx))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;

                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
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

        public Dictionary<string, string> ObtenerDiccionario()
        {
            const string sql = @"
                SELECT p.nombre, ep.valor
                FROM dbo.Parametros p
                LEFT JOIN dbo.EmpresaParametros ep
                       ON ep.idParametro = p.id
                      AND ep.idEmpresa = @idEmpresa;";

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            // Usamos Db.Reader para mapear y armar el diccionario
            Db.Reader<object>(
                _empresa,
                sql,
                CommandType.Text,
                map: dr =>
                {
                    string nombre = dr.IsDBNull(0) ? "" : dr.GetString(0);
                    string valor = dr.IsDBNull(1) ? "" : dr.GetString(1);
                    if (!string.IsNullOrEmpty(nombre))
                        dict[nombre] = valor;
                    return null;
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa
            );

            return dict;
        }

        // Puede devolver null si no existe valor para esa empresa
        public string ObtenerValor(string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío", nameof(nombreParametro));

            const string sql = @"
                SELECT ep.valor
                FROM dbo.Parametros p
                LEFT JOIN dbo.EmpresaParametros ep
                       ON ep.idParametro = p.idParametro
                      AND ep.idEmpresa = @idEmpresa
                WHERE p.nombre = @nombre;";

            object obj = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombreParametro;
                }
            );

            return (obj == null || obj == DBNull.Value) ? null : obj.ToString();
        }

        private bool TieneColumna(string nombreTabla, string nombreColumna)
        {
            const string sql = @"
                SELECT CASE
                           WHEN COL_LENGTH(@tabla, @columna) IS NULL THEN 0
                           ELSE 1
                       END;";

            object obj = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@tabla", SqlDbType.NVarChar, 256).Value = nombreTabla;
                    p.Add("@columna", SqlDbType.NVarChar, 128).Value = nombreColumna;
                }
            );

            return obj != null && obj != DBNull.Value && Convert.ToInt32(obj) == 1;
        }

        public void SetValor(string nombreParametro, string valor)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío", nameof(nombreParametro));

            // 1) Obtener idParametro
            const string sqlGetId = "SELECT idParametro FROM dbo.Parametros WHERE nombre = @nombre";

            object objId = Db.Scalar(
                _empresa,
                sqlGetId,
                CommandType.Text,
                setParams: p => p.Add("@nombre", SqlDbType.NVarChar, 100).Value = nombreParametro
            );

            if (objId == null || objId == DBNull.Value)
                throw new InvalidOperationException("No existe el parámetro '" + nombreParametro + "' en dbo.Parametros.");

            int idParametro = Convert.ToInt32(objId);

            // 2) MERGE (upsert) en EmpresaParametros
            const string mergeSql = @"
                MERGE dbo.EmpresaParametros AS T
                USING (SELECT @idEmpresa AS idEmpresa, @idParametro AS idParametro, @valor AS valor) AS S
                ON (T.idEmpresa = S.idEmpresa AND T.idParametro = S.idParametro)
                WHEN MATCHED THEN UPDATE SET valor = S.valor
                WHEN NOT MATCHED THEN INSERT (idEmpresa, idParametro, valor)
                                   VALUES (S.idEmpresa, S.idParametro, S.valor);";

            Db.NonQuery(
                _empresa,
                mergeSql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa;
                    p.Add("@idParametro", SqlDbType.Int).Value = idParametro;
                    p.Add("@valor", SqlDbType.NVarChar, 200).Value = (object)valor ?? DBNull.Value;
                }
            );
        }
    }
}
