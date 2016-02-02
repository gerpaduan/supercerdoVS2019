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
        SqlCommand cmUsuario;

        public DataTable obtenerUsuarios()
        {
            DataTable dtUsuario = new DataTable();
            daUsuario = new SqlDataAdapter("Select * from Usuarios", conn.conectar());
            daUsuario.Fill(dtUsuario);

            return dtUsuario;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            cmUsuario = new SqlCommand();

            cmUsuario.Connection = conn.conectar();
            cmUsuario.Connection.Open();
            cmUsuario.CommandType = CommandType.StoredProcedure;
            cmUsuario.CommandText = "addOrEditUser";
            cmUsuario.Parameters.AddWithValue("@id", oUsuarioE.Id);
            cmUsuario.Parameters.AddWithValue("@nombre", oUsuarioE.Nombre);
            cmUsuario.Parameters.AddWithValue("@usuario", oUsuarioE.User);
            cmUsuario.Parameters.AddWithValue("@clave", oUsuarioE.Clave);
            cmUsuario.Parameters.AddWithValue("@admin", oUsuarioE.Admin);
            cmUsuario.Parameters.AddWithValue("@colorForm", oUsuarioE.ColorForm);

            cmUsuario.ExecuteNonQuery();
            cmUsuario.Connection.Close();
        }
    }
}
