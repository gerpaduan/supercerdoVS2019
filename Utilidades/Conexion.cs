using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace Utilidades
{
    public class Conexion
    {
        public enum tipoConexion
        {
            local,
            sanMartin,
            sanMartinRemoto,
            sanLorenzo,
            sanLorenzoRemoto,
            servidor
        }
        public static tipoConexion tipoConn;
        //public static string connStringActual = ConfigurationManager.AppSettings["connString"].ToString();
        //public static int idSucursalAppConfig = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
        //string conString = ConfigurationManager.ConnectionStrings[connStringActual.ToString()].ToString();
        //public static bool soyYo = ConfigurationManager.AppSettings["cuitCliente"].ToString().Equals("20306210786") ? true : false;
        //public static int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["timeOut"].ToString());

        // Lee el nombre de la cadena desde AppSettings
        public static string connStringActual = ConfigurationManager.AppSettings["connString"];

        // Usa ese nombre para buscar la cadena real en ConnectionStrings
        public static string conString = ConfigurationManager
                                            .ConnectionStrings[connStringActual]
                                            .ConnectionString;

        public static int idSucursalAppConfig = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"]);

        public static bool soyYo = ConfigurationManager.AppSettings["cuitCliente"] == "20306210786";

        public static int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["timeOut"]);


        SqlConnection conn;
        //public SqlConnection conectar()
        //{
        //    //conString = getConnString();
        //    conn = new SqlConnection(conString);
        //    return conn;
        //}

        //public SqlConnection conectar(string conexionSucursal)
        //{
        //    conString = ConfigurationManager.ConnectionStrings[conexionSucursal].ToString();
        //    conn = new SqlConnection(conString);
        //    return conn;
        //}
        private static string GetConnectionStringFromConfig(string csNameOverride = null)
        {
            // Si te pasan un nombre de connectionString explícito, usa ese.
            // Si no, usa el nombre guardado en AppSettings["connString"].
            string csName = csNameOverride;

            if (string.IsNullOrWhiteSpace(csName))
                csName = ConfigurationManager.AppSettings["connString"];

            if (string.IsNullOrWhiteSpace(csName))
                throw new ConfigurationErrorsException("Falta AppSettings key 'connString' en el .config.");

            var settings = ConfigurationManager.ConnectionStrings[csName];
            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new ConfigurationErrorsException("No existe la connectionString '" + csName + "' en <connectionStrings>.");

            return settings.ConnectionString;
        }
        public SqlConnection conectar(IEmpresaContext empresa)
        {
            string cs = GetConnectionStringFromConfig();

            conn = new SqlConnection(cs);
            conn.Open();

            SetEmpresaSession(conn, empresa);

            return conn;
        }

        public SqlConnection conectar(string conexionSucursal, IEmpresaContext empresa)
        {
            // "conexionSucursal" debe ser el NOMBRE de una connectionString
            // Ej: "carnisys_local" (WinForms) o "ConexionPrincipal" (Web)
            string cs = GetConnectionStringFromConfig(conexionSucursal);

            conn = new SqlConnection(cs);
            conn.Open();

            SetEmpresaSession(conn, empresa);

            return conn;
        }


        // ---------------------------
        // SESSION_CONTEXT
        // ---------------------------
        private void SetEmpresaSession(
            SqlConnection cn,
            IEmpresaContext empresa)
        {
            if (empresa == null)
                throw new ArgumentNullException(nameof(empresa));

            using (var cmd = new SqlCommand(
                "EXEC sp_set_session_context 'IdEmpresa', @IdEmpresa",
                cn))
            {
                cmd.Parameters.AddWithValue(
                    "@IdEmpresa", empresa.IdEmpresa);

                cmd.ExecuteNonQuery();
            }
        }

        public void cerraConexion()
        {
            conn.Close();
        }

        public int TimeOut()
        {
            return timeOut;
        }

        public static string getConnString()
        {
            string connString = "";
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    connString = "local";
                    break;
                case Conexion.tipoConexion.sanMartin:
                    connString = "sanMartin";
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    connString = "sanMartinRemoto";
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    connString = "sanLorenzo";
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    connString = "sanLorenzoRemoto";
                    break;
                case Conexion.tipoConexion.servidor:
                    connString = "Servidor";
                    break;
            }
            connString = ConfigurationManager.ConnectionStrings[connString.ToString()].ToString();
            return connString;
        }

        public static tipoConexion getTipoConexion()
        {
            tipoConn = Conexion.tipoConexion.local;
            switch (connStringActual)
            {
                case "local":
                    tipoConn = Conexion.tipoConexion.local;
                    break;
                case "sanMartin":
                    tipoConn = Conexion.tipoConexion.sanMartin;
                    break;
                case "sanMartinRemoto":
                    tipoConn = Conexion.tipoConexion.sanMartinRemoto;
                    break;
                case "sanLorenzo":
                    tipoConn = Conexion.tipoConexion.sanLorenzo;
                    break;
                case "sanLorenzoRemoto":
                    tipoConn = Conexion.tipoConexion.sanLorenzoRemoto;
                    break;
                case "servidor":
                    tipoConn = Conexion.tipoConexion.servidor;
                    break;
            }
            return tipoConn;
        }

        public static string getSucursalConexion()
        {
            getTipoConexion();
            string sucursalConexion = " | Conn. ";
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    sucursalConexion += "local";
                    break;
                case Conexion.tipoConexion.sanMartin:
                    sucursalConexion += "San Martín";
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    sucursalConexion += "San Martín";
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    sucursalConexion += "San Lorenzo";
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    sucursalConexion += "San Lorenzo";
                    break;
                case Conexion.tipoConexion.servidor:
                    sucursalConexion += "Servidor";
                    break;
            }
             return sucursalConexion;
        }

        //Se obtiene el id de Sucursal par a la conexión actual
        public static int getIdSucursalConexion()
        {
            int idSucursal = 0;
            switch (tipoConn)
            {
                case Conexion.tipoConexion.local:
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                case Conexion.tipoConexion.sanMartin:
                    idSucursal = 2;
                    break;
                case Conexion.tipoConexion.sanMartinRemoto:
                    idSucursal = 2;
                    break;
                case Conexion.tipoConexion.sanLorenzo:
                    idSucursal = 1;
                    break;
                case Conexion.tipoConexion.sanLorenzoRemoto:
                    idSucursal = 1;
                    break;
                default:
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
            }
            return idSucursal;
        }

        //Se obtiene el id de Sucursal par a la conexión actual
        public static int getIdSucursalConexion(string conexionSucursal)
        {
            int idSucursal = 0;
            switch (conexionSucursal)
            {
                case "local":
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                case "sanMartin":
                    idSucursal = 2;
                    break;
                case "sanMartinRemoto":
                    idSucursal = 2;
                    break;
                case "sanLorenzo":
                    idSucursal = 1;
                    break;
                case "sanLorenzoRemoto":
                    idSucursal = 1;
                    break;
                case "servidor":
                    idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
                    break;
                default: 
                    idSucursal = idSucursalAppConfig;
                    break;
            }
            return idSucursal;
        }
     }
    
}
