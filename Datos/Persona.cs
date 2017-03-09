using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class Persona
    {
        Utilidades.Conexion conn= new Utilidades.Conexion();
        SqlDataAdapter daPersona;
        SqlCommand cmPersona;

        public void agregarPersona(Entidades.Persona oPersonaE)
        {
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "agregarPersona";
            cmPersona.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial);
            cmPersona.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos);
            cmPersona.Parameters.AddWithValue("@tipo", oPersonaE.tipo);

            cmPersona.ExecuteNonQuery();
            cmPersona.Connection.Close();

        }

        public void modificarProveedor(Entidades.Persona oPersonaE)
        {
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "modificarPersona";
            cmPersona.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);
            cmPersona.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos);
            cmPersona.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial);
            cmPersona.Parameters.AddWithValue("@tipo", oPersonaE.tipo);

            cmPersona.ExecuteNonQuery();
            cmPersona.Connection.Close();
        }

        public void addOrEditPersona(Entidades.Persona oPersonaE)
        {
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "addOrEditPersona";
            cmPersona.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);
            cmPersona.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos);
            cmPersona.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial);
            cmPersona.Parameters.AddWithValue("@tipo", oPersonaE.tipo);
            cmPersona.Parameters.AddWithValue("@ctaCte", oPersonaE.CtaCte);
            cmPersona.Parameters.AddWithValue("@bonificacion", oPersonaE.Bonificacion);

            cmPersona.ExecuteNonQuery();
            cmPersona.Connection.Close();

        }

        public void eliminarPersona(Entidades.Persona oPersonaE)
        {
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "eliminarPersona";
            cmPersona.Parameters.AddWithValue("@idPersona", oPersonaE.idPersona);            

            cmPersona.ExecuteNonQuery();
            cmPersona.Connection.Close();
        }

        public Entidades.Persona findById(int id)
        {
            DataTable dtPersona = new DataTable();
            SqlDataAdapter daPersona;
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            daPersona = new SqlDataAdapter("select * from Personas where idPersona = " + id, conn.conectar());
            daPersona.Fill(dtPersona);

            Entidades.Persona oPersona = new Entidades.Persona();
            if (dtPersona.Rows.Count > 0)
            {                
                oPersona.idPersona = Convert.ToInt32(dtPersona.Rows[0]["idPersona"].ToString());
                oPersona.tipo = dtPersona.Rows[0]["tipo"].ToString();
                oPersona.razonSocial = dtPersona.Rows[0]["razonSocial"].ToString();
                oPersona.CtaCte = !dtPersona.Rows[0].Equals(DBNull.Value) ? Convert.ToBoolean(dtPersona.Rows[0]["ctaCte"]) : false;
                oPersona.Bonificacion = !dtPersona.Rows[0]["bonificacion"].ToString().Equals(DBNull.Value) ? float.Parse(dtPersona.Rows[0]["bonificacion"].ToString()) : 0;
                oPersona.OtrosDatos = dtPersona.Rows[0]["otrosDatos"].ToString();
            }
            conn.cerraConexion();

            return oPersona;
        }

        public DataTable buscarProveedor(string buscarTexto)
        {
            DataTable dtProveedores = new DataTable();
            daPersona = new SqlDataAdapter();
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "buscarProveedor";
            cmPersona.Parameters.AddWithValue("@texto", buscarTexto);
            daPersona.SelectCommand = cmPersona;

            daPersona.Fill(dtProveedores);
           
            return dtProveedores;
        }

        public DataTable buscarPersona(string buscarTexto)
        {
            DataTable dtPersonas = new DataTable();
            daPersona = new SqlDataAdapter();
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "buscarPersona";
            cmPersona.Parameters.AddWithValue("@texto", buscarTexto);
            daPersona.SelectCommand = cmPersona;

            daPersona.Fill(dtPersonas);

            return dtPersonas;
        }

        public DataTable obtenerProveedores()
        {
            
            DataTable dtProveedores = new DataTable();
            daPersona = new SqlDataAdapter();
            cmPersona = new SqlCommand();
            cmPersona.Connection = conn.conectar();
            cmPersona.Connection.Open();
            cmPersona.CommandType = CommandType.StoredProcedure;
            cmPersona.CommandText = "buscarPersona";

            daPersona.SelectCommand = cmPersona;

            daPersona.Fill(dtProveedores);

            cmPersona.Connection.Close();

            return dtProveedores;
        }
        
    }
}
