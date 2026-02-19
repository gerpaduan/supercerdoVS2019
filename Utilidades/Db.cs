using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Utilidades
{
    /// <summary>
    /// Helper central para ejecutar SQL con multi-tenant (RLS).
    /// SIEMPRE abre conexión, setea SESSION_CONTEXT('IdEmpresa') y cierra al final.
    /// </summary>
    public static class Db
    {
        /// <summary>
        /// Abre una conexión ya con SESSION_CONTEXT seteado.
        /// Usar cuando necesitás manejar transacción o varios comandos en la misma conexión.
        /// </summary>
        public static SqlConnection Open(IEmpresaContext empresa, string conexionSucursal = null)
        {
            var cx = new Conexion();
            return string.IsNullOrWhiteSpace(conexionSucursal)
                ? cx.conectar(empresa)
                : cx.conectar(conexionSucursal, empresa);
        }

        /// <summary>
        /// Ejecuta un ExecuteNonQuery (INSERT/UPDATE/DELETE o SP) y devuelve filas afectadas.
        /// </summary>
        public static int NonQuery(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null)
        {
            using (var con = Open(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                setParams?.Invoke(cmd.Parameters);

                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Ejecuta ExecuteScalar y devuelve object (vos lo convertís).
        /// </summary>
        public static object Scalar(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null)
        {
            using (var con = Open(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                setParams?.Invoke(cmd.Parameters);

                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Ejecuta un lector y te deja mapear fila por fila.
        /// </summary>
        public static List<T> Reader<T>(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Func<SqlDataReader, T> map,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            var list = new List<T>();

            using (var con = Open(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                setParams?.Invoke(cmd.Parameters);

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                        list.Add(map(dr));
                }
            }

            return list;
        }

        /// <summary>
        /// Ejecuta un DataTable (para tus grillas / reportes).
        /// </summary>
        public static DataTable DataTable(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null)
        {
            var dt = new DataTable();

            using (var con = Open(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            using (var da = new SqlDataAdapter(cmd))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                setParams?.Invoke(cmd.Parameters);

                da.Fill(dt);
            }

            return dt;
        }
    }
}
