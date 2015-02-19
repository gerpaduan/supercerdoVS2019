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
