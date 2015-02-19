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
            string conString = ConfigurationManager.ConnectionStrings["connecString"].ToString();
  
            SqlConnection conn;
            public SqlConnection conectar()
            {
                conn = new SqlConnection(conString);
                return conn;

            }
            public void cerraConexion()
            {
                conn.Close();
            }


     }
    
}
