// TODO(claude): version TEMPORAL y PARCIAL de Utilidades/Db.cs para destrabar el spike de
// migracion a ASP.NET Core (ver docs/DECISIONS.md). NO es una copia fiel: el original envuelve
// cada operacion en PerformanceInstrumentation.MeasureDb() (registra X-CarniSys-Db-Ms/Db-Calls
// via System.Web.HttpContext.Current, que no existe en net10.0). Esta version ejecuta el SQL
// igual pero SIN esa instrumentacion -- el diseño real (con IHttpContextAccessor de ASP.NET
// Core) queda pendiente como tarea aparte, no resuelto aca. Misma firma publica que el original
// para que Datos/*.cs compile sin cambios en ambos TFMs.
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Utilidades
{
    public static class Db
    {
        public static SqlConnection Open(IEmpresaContext empresa, string conexionSucursal = null)
        {
            var cx = new Conexion();
            return string.IsNullOrWhiteSpace(conexionSucursal)
                ? cx.conectar(empresa)
                : cx.conectar(conexionSucursal, empresa);
        }

        public static SqlConnection OpenAdmin(IEmpresaContext empresa, string conexionSucursal = null)
        {
            var cx = new Conexion();
            return string.IsNullOrWhiteSpace(conexionSucursal)
                ? cx.conectarSinTenant(empresa)
                : cx.conectarSinTenant(conexionSucursal, empresa);
        }

        public static int NonQuery(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null,
            Func<IEmpresaContext, string, SqlConnection> openConnection = null)
        {
            using (var con = (openConnection ?? Open)(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;
                setParams?.Invoke(cmd.Parameters);
                return cmd.ExecuteNonQuery();
            }
        }

        public static object Scalar(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null,
            Func<IEmpresaContext, string, SqlConnection> openConnection = null)
        {
            using (var con = (openConnection ?? Open)(empresa, conexionSucursal))
            using (var cmd = new SqlCommand(sqlOrSp, con))
            {
                cmd.CommandType = commandType;
                cmd.CommandTimeout = timeoutOverride ?? Conexion.timeOut;
                setParams?.Invoke(cmd.Parameters);
                return cmd.ExecuteScalar();
            }
        }

        public static List<T> Reader<T>(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Func<SqlDataReader, T> map,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null,
            Func<IEmpresaContext, string, SqlConnection> openConnection = null)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            var list = new List<T>();
            using (var con = (openConnection ?? Open)(empresa, conexionSucursal))
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

        public static DataTable DataTable(
            IEmpresaContext empresa,
            string sqlOrSp,
            CommandType commandType,
            Action<SqlParameterCollection> setParams = null,
            string conexionSucursal = null,
            int? timeoutOverride = null,
            Func<IEmpresaContext, string, SqlConnection> openConnection = null)
        {
            var dt = new DataTable();
            using (var con = (openConnection ?? Open)(empresa, conexionSucursal))
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
