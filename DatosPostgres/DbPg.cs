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
        // Overloads con unitOfWork: si se pasa una UnitOfWorkPg real (ver Contratos/IUnitOfWork.cs
        // y DatosPostgres/UnitOfWorkPg.cs), reusan esa conexion/transaccion compartida en vez de
        // abrir la propia -- sin commit/rollback local, eso lo maneja quien inicio la unidad de
        // trabajo. Si unitOfWork es null (o no es una UnitOfWorkPg), cae al comportamiento de
        // siempre. Necesario para que Negocio.Venta/CierreCaja/CuentaCorriente sean atomicos
        // entre si sin depender de TransactionScope (testeo profundo, 2026-08-20).
        public static int NonQuery(string connectionString, int idEmpresa, string sql, Action<NpgsqlParameterCollection> setParams, Contratos.IUnitOfWork unitOfWork)
        {
            var uow = unitOfWork as UnitOfWorkPg;
            if (uow == null) return NonQuery(connectionString, idEmpresa, sql, setParams);

            using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
            {
                setParams?.Invoke(cmd.Parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(string connectionString, int idEmpresa, string sql, Action<NpgsqlParameterCollection> setParams, Contratos.IUnitOfWork unitOfWork)
        {
            var uow = unitOfWork as UnitOfWorkPg;
            if (uow == null) return Scalar(connectionString, idEmpresa, sql, setParams);

            using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
            {
                setParams?.Invoke(cmd.Parameters);
                return cmd.ExecuteScalar();
            }
        }

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
                        tx?.Commit();
                        return filas;
                    }
                }
                catch
                {
                    tx?.Rollback();
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
                        tx?.Commit();
                        return resultado;
                    }
                }
                catch
                {
                    tx?.Rollback();
                    throw;
                }
            }
        }

        public static List<T> Reader<T>(string connectionString, int idEmpresa, string sql, Func<NpgsqlDataReader, T> map, Action<NpgsqlParameterCollection> setParams, Contratos.IUnitOfWork unitOfWork)
        {
            var uow = unitOfWork as UnitOfWorkPg;
            if (uow == null) return Reader(connectionString, idEmpresa, sql, map, setParams);

            var listaCompartida = new List<T>();
            using (var cmd = new NpgsqlCommand(sql, uow.Connection, uow.Transaction))
            {
                setParams?.Invoke(cmd.Parameters);
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        listaCompartida.Add(map(dr));
                }
            }
            return listaCompartida;
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
                    tx?.Commit();
                    return lista;
                }
                catch
                {
                    tx?.Rollback();
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
                    tx?.Commit();
                    return dt;
                }
                catch
                {
                    tx?.Rollback();
                    throw;
                }
            }
        }
    }
}
