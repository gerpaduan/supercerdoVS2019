using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Entidades;

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
            cmPersona.Parameters.AddWithValue("@identificacion", oPersonaE.Identificacion);
            cmPersona.Parameters.AddWithValue("@razonSocial", oPersonaE.razonSocial);
            cmPersona.Parameters.AddWithValue("@idIva", oPersonaE.IdIva);
            cmPersona.Parameters.AddWithValue("@cuit", oPersonaE.Cuit);
            cmPersona.Parameters.AddWithValue("@telefono", oPersonaE.Telefono);
            cmPersona.Parameters.AddWithValue("@domicilio", oPersonaE.Domicilio);
            cmPersona.Parameters.AddWithValue("@ciudad", oPersonaE.Ciudad);
            cmPersona.Parameters.AddWithValue("@otrosDatos", oPersonaE.otrosDatos);
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
            daPersona = new SqlDataAdapter("SELECT dbo.Personas.idPersona as idPersona, dbo.Personas.identificacion as identificacion, dbo.Personas.razonSocial as razonSocial, " +
                      " dbo.Personas.tipo as tipo, dbo.Personas.otrosDatos as otrosDatos, dbo.Personas.ctaCte as ctaCte, "+
                      " dbo.Personas.bonificacion as bonificacion, dbo.Personas.cuit as cuit, dbo.Personas.telefono as telefono, "+ 
                      " dbo.Personas.domicilio as domicilio, dbo.Personas.ciudad as ciudad, dbo.Personas.idIva as idIva, "+
                      " dbo.Iva.iva as iva FROM  dbo.Iva RIGHT OUTER JOIN " +
                      " dbo.Personas ON dbo.Iva.id = dbo.Personas.idIva where idPersona = " + id, conn.conectar());
            daPersona.Fill(dtPersona);

            Entidades.Persona oPersona = new Entidades.Persona();
            if (dtPersona.Rows.Count > 0)
            {                
                oPersona.idPersona = Convert.ToInt32(dtPersona.Rows[0]["idPersona"].ToString());
                oPersona.tipo = dtPersona.Rows[0]["tipo"].ToString();
                oPersona.Identificacion = dtPersona.Rows[0]["identificacion"].ToString();
                oPersona.razonSocial = dtPersona.Rows[0]["razonSocial"].ToString();
                oPersona.Iva = dtPersona.Rows[0]["iva"].ToString();
                oPersona.IdIva = string.IsNullOrEmpty(dtPersona.Rows[0]["idIva"].ToString()) ? 0 : Convert.ToInt32(dtPersona.Rows[0]["idIva"].ToString());
                oPersona.Cuit = dtPersona.Rows[0]["cuit"].ToString();
                oPersona.Telefono = dtPersona.Rows[0]["telefono"].ToString();
                oPersona.Domicilio = dtPersona.Rows[0]["domicilio"].ToString();
                oPersona.Ciudad = dtPersona.Rows[0]["ciudad"].ToString();
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

        public DataTable getIva()
        {
            DataTable dtIva = new DataTable();
            SqlDataAdapter daPersona;
            cmPersona = new SqlCommand();

            cmPersona.Connection = conn.conectar();
            daPersona = new SqlDataAdapter("select * from Iva", conn.conectar());
            daPersona.Fill(dtIva);

            return dtIva;
        }

        public int existeCuit(string cuit)
        {
            int idPersona = 0;
            cuit = !string.IsNullOrEmpty(cuit) ? cuit.ToString().Replace("-", "") : cuit;
            bool esNumerico = long.TryParse(cuit, out long result);
            if (string.IsNullOrEmpty(cuit) || !esNumerico)
                return 0;

            cmPersona = new SqlCommand();
            cmPersona.Connection = conn.conectar();
            cmPersona.CommandType = CommandType.Text;
            cmPersona.CommandText = "Select idPersona from Personas where REPLACE(cuit, '-', '') like " + cuit;//.ToString().Replace("-","");
            try
            {
                cmPersona.Connection.Open();
                SqlDataReader drVenta = cmPersona.ExecuteReader();
                using (drVenta)
                {
                    while (drVenta.Read())
                    {
                        idPersona = Convert.ToInt32(drVenta["idPersona"]);
                    }
                    return idPersona;
                }
            }
            finally
            {
                cmPersona.Connection.Close();
            }
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
