using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Usuario
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daUsuario;

        public DataTable obtenerUsuarios()
        {
            DataTable dtUsuario = new DataTable();
            daUsuario = new SqlDataAdapter("Select * from Usuarios", conn.conectar());
            daUsuario.Fill(dtUsuario);

            return dtUsuario;
        }
    }
}
