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
        public static string connStringActual = ConfigurationManager.AppSettings["connString"].ToString();
        public static int idSucursalAppConfig = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
        string conString = ConfigurationManager.ConnectionStrings[connStringActual.ToString()].ToString();
        public static bool soyYo = ConfigurationManager.AppSettings["cuitCliente"].ToString().Equals("20306210786") ? true : false;
        public static int timeOut = Convert.ToInt32(ConfigurationManager.AppSettings["timeOut"].ToString());

        SqlConnection conn;
        public SqlConnection conectar()
        {
            //conString = getConnString();
            conn = new SqlConnection(conString);
            return conn;
        }

        public SqlConnection conectar(string conexionSucursal)
        {
            conString = ConfigurationManager.ConnectionStrings[conexionSucursal].ToString();
            conn = new SqlConnection(conString);
            return conn;
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
