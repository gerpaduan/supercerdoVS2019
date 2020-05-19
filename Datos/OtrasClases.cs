using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class OtrasClases
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlCommand cmOtrasClases;
        SqlDataAdapter daOtrasClases;

        public bool Login(string clave)
        {
            cmOtrasClases = new SqlCommand();

            cmOtrasClases.Connection = conn.conectar();
            cmOtrasClases.Connection.Open();
            cmOtrasClases.CommandText = "select * from Claves where Clave="+clave;

            bool resp= Convert.ToBoolean(cmOtrasClases.ExecuteScalar());
            cmOtrasClases.Connection.Close();

            //Utilidades.FormLogin frmLogin = new Utilidades.FormLogin();
            //frmLogin.Logueado = resp;

            return resp;

        }

        public DataTable obtenerParametros()
        {
            DataTable dtParametros = new DataTable();
            daOtrasClases = new SqlDataAdapter("Select * from Parametros", conn.conectar());
            daOtrasClases.Fill(dtParametros);

            return dtParametros;
        }
    }
}
