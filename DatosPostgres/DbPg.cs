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

        // Ejecuta las sentencias de preparacion (tablas temporales, indices, ANALYZE) en orden,
        // en la conexion/transaccion ya abierta. Cada sentencia recibe los mismos parametros que la
        // consulta principal (Npgsql ignora los que una sentencia no referencia).
        // Requiere transaccion propia: con un TransactionScope ambiente (transaccion == null) las
        // tablas ON COMMIT DROP sobrevivirian hasta que cierre el scope y una segunda llamada
        // fallaria con "relation already exists"; por eso se rechaza de forma explicita.
        private static void EjecutarPreparacion(NpgsqlConnection cn, NpgsqlTransaction tx, IReadOnlyList<string> sentenciasPreparacion, Action<NpgsqlParameterCollection> setParams)
        {
            if (tx == null)
                throw new InvalidOperationException("Las consultas con preparacion requieren transaccion propia; no usar dentro de un TransactionScope ambiente.");

            foreach (var sentencia in sentenciasPreparacion)
            {
                using (var cmdPrep = new NpgsqlCommand(sentencia, cn, tx))
                {
                    setParams?.Invoke(cmdPrep.Parameters);
                    cmdPrep.ExecuteNonQuery();
                }
            }
        }

        // Variante de Reader para consultas que necesitan "preparar" la sesion antes del SELECT
        // (hoy: CortePg.ObtenerExistenciaPorSucursalesPlano). Misma mecanica que
        // DataTableConPreparacion: todo en la MISMA conexion + transaccion de tenant.
        public static List<T> ReaderConPreparacion<T>(string connectionString, int idEmpresa, IReadOnlyList<string> sentenciasPreparacion, string sql, Func<NpgsqlDataReader, T> map, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    EjecutarPreparacion(cn, tx, sentenciasPreparacion, setParams);

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
                    tx?.Rollback();
                    throw;
                }
            }
        }

        // Variante de DataTable para consultas que necesitan "preparar" la sesion antes del SELECT
        // (hoy: CortePg.CierreStockWeb, que materializa 2 tablas temporales con estadisticas para
        // que el planificador no elija bucles anidados sobre CTEs mal estimados).
        // Todo corre en la MISMA conexion + transaccion de tenant: las tablas creadas con
        // ON COMMIT DROP se destruyen solas al commit/rollback, asi que nunca quedan en una
        // conexion reciclada por el pool. Ver EjecutarPreparacion para los detalles.
        public static DataTable DataTableConPreparacion(string connectionString, int idEmpresa, IReadOnlyList<string> sentenciasPreparacion, string sql, Action<NpgsqlParameterCollection> setParams = null)
        {
            using (var cn = ConexionPg.AbrirConTenant(connectionString, idEmpresa, out var tx))
            {
                try
                {
                    // Pasos de preparacion (tablas temporales, indices, ANALYZE), en orden.
                    EjecutarPreparacion(cn, tx, sentenciasPreparacion, setParams);

                    // Consulta principal, ya leyendo de las tablas temporales.
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
