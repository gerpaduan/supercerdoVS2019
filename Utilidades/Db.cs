using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;

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
            int? timeoutOverride = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            return PerformanceInstrumentation.MeasureDb(
                sqlOrSp,
                commandType,
                () =>
                {
                    using (var con = Open(empresa, conexionSucursal))
                    using (var cmd = new SqlCommand(sqlOrSp, con))
                    {
                        cmd.CommandType = commandType;
                        cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                        setParams?.Invoke(cmd.Parameters);

                        return cmd.ExecuteNonQuery();
                    }
                },
                rows => "rows=" + rows.ToString(),
                conexionSucursal,
                callerMemberName,
                callerFilePath);
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
            int? timeoutOverride = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            return PerformanceInstrumentation.MeasureDb(
                sqlOrSp,
                commandType,
                () =>
                {
                    using (var con = Open(empresa, conexionSucursal))
                    using (var cmd = new SqlCommand(sqlOrSp, con))
                    {
                        cmd.CommandType = commandType;
                        cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;

                        setParams?.Invoke(cmd.Parameters);

                        return cmd.ExecuteScalar();
                    }
                },
                result => result == null || result == DBNull.Value ? "scalar=null" : "scalar=" + Convert.ToString(result),
                conexionSucursal,
                callerMemberName,
                callerFilePath);
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
            int? timeoutOverride = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            return PerformanceInstrumentation.MeasureDb(
                sqlOrSp,
                commandType,
                () =>
                {
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
                },
                list => "rows=" + (list != null ? list.Count : 0).ToString(),
                conexionSucursal,
                callerMemberName,
                callerFilePath);
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
            int? timeoutOverride = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerFilePath] string callerFilePath = null)
        {
            return PerformanceInstrumentation.MeasureDb(
                sqlOrSp,
                commandType,
                () =>
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
                },
                dt => "rows=" + (dt != null ? dt.Rows.Count : 0).ToString(),
                conexionSucursal,
                callerMemberName,
                callerFilePath);
        }
    }
}
