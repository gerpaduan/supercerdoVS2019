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
            remota,
        }
        public static tipoConexion tipoConn;
        string valueConnString = "";
        string conString = ConfigurationManager.ConnectionStrings["connecString"].ToString();

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
                    connString = "connecString";
                        break;
                case Conexion.tipoConexion.remota:
                        connString = "connecStringRemota";
                        break;
            }
            connString = ConfigurationManager.ConnectionStrings[connString.ToString()].ToString();
            return connString;
        }
     }
    
}
