using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.IParametrosRepository (5/5 metodos). ObtenerValor/
    // SetValor no tienen caller real hoy (solo Negocio.Parametros usa ObtenerGrid/GuardarGrid/
    // ObtenerDiccionario) -- se migran igual por completitud. Ver docs/DECISIONS.md.
    //
    // "parametros" es catalogo global (sin idempresa, sin RLS). "empresaparametros" SI tiene
    // idempresa y en Postgres SI lleva RLS -- mejora deliberada confirmada con el usuario: el
    // original en SQL Server no tiene RLS en esta tabla (a diferencia de las demas tablas
    // multi-tenant de la migracion), simplemente nunca se le agrego. El aislamiento por
    // aplicacion (idEmpresa explicito en cada query) se preserva igual que el original, mas el
    // backstop de RLS como capa adicional.
    public class ParametrosPg : Contratos.IParametrosRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public ParametrosPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public DataTable ObtenerGrid()
        {
            return DbPg.DataTable(_connectionString, _idEmpresa, @"
                SELECT
                    p.idparametro AS ""idParametro"",
                    p.nombre AS ""nombre"",
                    p.descripcion AS ""descripcion"",
                    p.tipo AS ""tipo"",
                    ep.valor AS ""valor""
                FROM parametros p
                LEFT JOIN empresaparametros ep
                       ON ep.idparametro = p.idparametro
                      AND ep.idempresa = @idEmpresa
                ORDER BY p.nombre;",
                p => p.AddWithValue("idEmpresa", _idEmpresa));
        }

        // Mismo criterio del original: transaccion explicita para el guardado masivo.
        public void GuardarGrid(DataTable dtParametros)
        {
            if (dtParametros == null) throw new ArgumentNullException(nameof(dtParametros));
            if (!dtParametros.Columns.Contains("idParametro")) throw new ArgumentException("Falta la columna idParametro");
            if (!dtParametros.Columns.Contains("valor")) throw new ArgumentException("Falta la columna valor");

            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO empresaparametros (idempresa, idparametro, valor)
                        VALUES (@idEmpresa, @idParametro, @valor)
                        ON CONFLICT (idempresa, idparametro) DO UPDATE SET valor = EXCLUDED.valor;", con, tx))
                    {
                        cmd.Parameters.Add("idEmpresa", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Parameters.Add("idParametro", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Parameters.Add("valor", NpgsqlTypes.NpgsqlDbType.Text);
                        cmd.Prepare();

                        foreach (DataRow row in dtParametros.Rows)
                        {
                            if (row.RowState == DataRowState.Deleted) continue;
                            if (row["idParametro"] == DBNull.Value) continue;

                            cmd.Parameters["idEmpresa"].Value = _idEmpresa;
                            cmd.Parameters["idParametro"].Value = Convert.ToInt32(row["idParametro"]);

                            object valObj = row["valor"];
                            cmd.Parameters["valor"].Value = (valObj == DBNull.Value || valObj == null) ? (object)DBNull.Value : valObj.ToString();

                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
                catch
                {
                    try { tx.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public Dictionary<string, string> ObtenerDiccionario()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            DbPg.Reader(_connectionString, _idEmpresa, @"
                SELECT p.nombre, ep.valor
                FROM parametros p
                LEFT JOIN empresaparametros ep
                       ON ep.idparametro = p.idparametro
                      AND ep.idempresa = @idEmpresa;",
                dr =>
                {
                    string nombre = dr["nombre"] == DBNull.Value ? "" : Convert.ToString(dr["nombre"]);
                    string valor = dr["valor"] == DBNull.Value ? "" : Convert.ToString(dr["valor"]);
                    if (!string.IsNullOrEmpty(nombre))
                        dict[nombre] = valor;
                    return (object)null;
                },
                p => p.AddWithValue("idEmpresa", _idEmpresa));

            return dict;
        }

        public string ObtenerValor(string nombreParametro)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío", nameof(nombreParametro));

            object obj = DbPg.Scalar(_connectionString, _idEmpresa, @"
                SELECT ep.valor
                FROM parametros p
                LEFT JOIN empresaparametros ep
                       ON ep.idparametro = p.idparametro
                      AND ep.idempresa = @idEmpresa
                WHERE p.nombre = @nombre;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("nombre", nombreParametro);
                });

            return (obj == null || obj == DBNull.Value) ? null : obj.ToString();
        }

        public void SetValor(string nombreParametro, string valor)
        {
            if (string.IsNullOrWhiteSpace(nombreParametro))
                throw new ArgumentException("nombreParametro vacío", nameof(nombreParametro));

            object objId = DbPg.Scalar(_connectionString, _idEmpresa,
                "SELECT idparametro FROM parametros WHERE nombre = @nombre;",
                p => p.AddWithValue("nombre", nombreParametro));

            if (objId == null || objId == DBNull.Value)
                throw new InvalidOperationException("No existe el parámetro '" + nombreParametro + "' en parametros.");

            int idParametro = Convert.ToInt32(objId);

            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO empresaparametros (idempresa, idparametro, valor)
                VALUES (@idEmpresa, @idParametro, @valor)
                ON CONFLICT (idempresa, idparametro) DO UPDATE SET valor = EXCLUDED.valor;",
                p =>
                {
                    p.AddWithValue("idEmpresa", _idEmpresa);
                    p.AddWithValue("idParametro", idParametro);
                    p.AddWithValue("valor", (object)valor ?? DBNull.Value);
                });
        }
    }
}
