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

        public Entidades.Usuario getUsuarioById(int idUsuario)
        {
            cmUsuario = new SqlCommand();
            cmUsuario.Connection = conn.conectar();
            cmUsuario.CommandType = CommandType.Text;
            cmUsuario.CommandText = "Select Usuarios.* from Usuarios where id =" + idUsuario;

            Entidades.Usuario oUsuarioE = new Entidades.Usuario();

            try
            {
                cmUsuario.Connection.Open();
                SqlDataReader drUsuario = cmUsuario.ExecuteReader();

                using (drUsuario)
                {
                    while (drUsuario.Read())
                    {
                        oUsuarioE.Id = Convert.ToInt32(drUsuario["id"]);
                        oUsuarioE.Nombre = Convert.ToString(drUsuario["nombre"]);
                        oUsuarioE.User = Convert.ToString(drUsuario["usuario"]);
                        oUsuarioE.Clave = Convert.ToString(drUsuario["clave"]);
                        oUsuarioE.Admin = Convert.ToBoolean(drUsuario["admin"]);
                        oUsuarioE.ColorForm = Convert.ToString(drUsuario["colorForm"]);	
                    }
                    return oUsuarioE;
                }
            }
            finally
            {
                cmUsuario.Connection.Close();
                oUsuarioE = null;
            }
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
