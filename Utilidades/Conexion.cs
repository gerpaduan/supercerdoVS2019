using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;
using System.Data.Common;

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
            sanLorenzoRemoto
        }
        public static tipoConexion tipoConn;
        public static string connStringActual = ConfigurationManager.AppSettings["connString"].ToString();
        string conString = ConfigurationManager.ConnectionStrings[connStringActual.ToString()].ToString();

        SqlConnection conn;
        public SqlConnection conectar()
        {
            conString = getConnString();
            conn = new SqlConnection(conString);
            return conn;

        }
        public void cerraConexion()
        {
            conn.Close();
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
            }
            connString = ConfigurationManager.ConnectionStrings[connString.ToString()].ToString();
            return connString;
        }

        public static tipoConexion getTipoConexion()
        {
            tipoConexion tipoConn = Conexion.tipoConexion.local;
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
            }
            return tipoConn;
        }

        public static string getSucursalConexion()
        {
            string sucursalConexion = " | Suc. ";
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
                    idSucursal = 1;
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
            }
            return idSucursal;
        }
     }
    
}
