using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;

namespace DatosPostgres
{
    // Equivalente a Utilidades/Db.cs para Postgres: abre conexion+transaccion con el
    // tenant seteado (ver ConexionPg), ejecuta, hace commit (o rollback si algo falla) y cierra.
    public static class DbPg
    {
        public static int NonQuery(string connectionString, int idEmpresa, string sql, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        int filas = cmd.ExecuteNonQuery();
                        tx.Commit();
                        return filas;
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static object Scalar(string connectionString, int idEmpresa, string sql, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        object resultado = cmd.ExecuteScalar();
                        tx.Commit();
                        return resultado;
                    }
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static List<T> Reader<T>(string connectionString, int idEmpresa, string sql, Func<NpgsqlDataReader, T> map, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    var lista = new List<T>();
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        using (var dr = cmd.ExecuteReader())
                        {
                            while (dr.Read())
                                lista.Add(map(dr));
                        }
                    }
                    tx.Commit();
                    return lista;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public static DataTable DataTable(string connectionString, int idEmpresa, string sql, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    var dt = new DataTable();
                    using (var cmd = new NpgsqlCommand(sql, cn, tx))
                    using (var da = new NpgsqlDataAdapter(cmd))
                    {
                        setParams?.Invoke(cmd.Parameters);
                        da.Fill(dt);
                    }
                    tx.Commit();
                    return dt;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }
    }
}
