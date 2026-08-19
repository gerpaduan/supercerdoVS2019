using System;
using System.Collections.Generic;
using Npgsql;

namespace DatosPostgres
{
    // Implementacion Postgres de Contratos.ICortePuntoStockSucursalRepository (3/3 metodos).
    // cortepuntostocksucursal SI tiene RLS en Postgres (mejora deliberada -- el original en
    // SQL Server no la tiene, mismo criterio ya confirmado para empresaparametros). Ver
    // docs/DECISIONS.md.
    public class CortePuntoStockSucursalPg : Contratos.ICortePuntoStockSucursalRepository
    {
        private readonly string _connectionString;
        private readonly int _idEmpresa;

        public CortePuntoStockSucursalPg(string connectionString, int idEmpresa)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) throw new ArgumentNullException(nameof(connectionString));
            _connectionString = connectionString;
            _idEmpresa = idEmpresa;
        }

        public void CrearParaTodasLasSucursales(int idEmpresa, int idCorte, int puntoStockInicial)
        {
            DbPg.NonQuery(_connectionString, _idEmpresa, @"
                INSERT INTO cortepuntostocksucursal (idempresa, idcorte, idsucursal, puntostock)
                SELECT @idEmpresa, @idCorte, s.idsucursal, @puntoStockInicial
                FROM sucursal s
                WHERE s.idempresa = @idEmpresa
                  AND NOT EXISTS (
                      SELECT 1 FROM cortepuntostocksucursal existente
                      WHERE existente.idempresa = @idEmpresa
                        AND existente.idcorte = @idCorte
                        AND existente.idsucursal = s.idsucursal
                  );",
                p =>
                {
                    p.AddWithValue("idEmpresa", idEmpresa);
                    p.AddWithValue("idCorte", idCorte);
                    p.AddWithValue("puntoStockInicial", puntoStockInicial);
                });
        }

        // Upsert nativo (ON CONFLICT) en vez del MERGE original -- mismo efecto, dentro de una
        // transaccion explicita (mismo criterio del original: todas las sucursales o ninguna).
        public void GuardarPuntosStockLote(int idEmpresa, int idCorte, List<(int idSucursal, int puntoStock)> valores)
        {
            using (var con = ConexionPg.AbrirConTenant(_connectionString, _idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO cortepuntostocksucursal (idempresa, idcorte, idsucursal, puntostock)
                        VALUES (@idEmpresa, @idCorte, @idSucursal, @puntoStock)
                        ON CONFLICT (idempresa, idcorte, idsucursal) DO UPDATE SET
                            puntostock = EXCLUDED.puntostock,
                            actualizado = now();", con, tx))
                    {
                        cmd.Parameters.Add("idEmpresa", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Parameters.Add("idCorte", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Parameters.Add("idSucursal", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Parameters.Add("puntoStock", NpgsqlTypes.NpgsqlDbType.Integer);
                        cmd.Prepare();

                        foreach (var valor in valores)
                        {
                            cmd.Parameters["idEmpresa"].Value = idEmpresa;
                            cmd.Parameters["idCorte"].Value = idCorte;
                            cmd.Parameters["idSucursal"].Value = valor.idSucursal;
                            cmd.Parameters["puntoStock"].Value = valor.puntoStock;
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

        public Dictionary<int, int> FindPorSucursal(int idSucursal)
        {
            var resultado = new Dictionary<int, int>();

            DbPg.Reader(_connectionString, _idEmpresa,
                "SELECT idcorte, puntostock FROM cortepuntostocksucursal WHERE idsucursal = @idSucursal;",
                dr =>
                {
                    resultado[Convert.ToInt32(dr["idcorte"])] = Convert.ToInt32(dr["puntostock"]);
                    return (object)null;
                },
                p => p.AddWithValue("idSucursal", idSucursal));

            return resultado;
        }
    }
}
