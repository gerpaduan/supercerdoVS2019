using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class CuentaCorriente
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daCtaCte;
        SqlCommand cmCtaCte;
        
        #region Pagos

        public void agregarPago(Entidades.Pagos oPagoE)
        {
            cmCtaCte = new SqlCommand();

            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "agregarPago";

            cmCtaCte.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo);
            cmCtaCte.Parameters.AddWithValue("@Fecha", oPagoE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@idProveedor", oPagoE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@tipoPago", oPagoE.TipoPago);
            cmCtaCte.Parameters.AddWithValue("@importe", oPagoE.Importe);
            cmCtaCte.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones);


            cmCtaCte.ExecuteNonQuery();
            cmCtaCte.Connection.Close();

            cmCtaCte = null;

        }

        public void modificarPago(Entidades.Pagos oPagoE)
        {
            cmCtaCte = new SqlCommand();

            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "modificarPago";

            cmCtaCte.Parameters.AddWithValue("@Id", oPagoE.Id);
            cmCtaCte.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo);
            cmCtaCte.Parameters.AddWithValue("@Fecha", oPagoE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@idProveedor", oPagoE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@tipoPago", oPagoE.TipoPago);
            cmCtaCte.Parameters.AddWithValue("@importe", oPagoE.Importe);
            cmCtaCte.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones);


            cmCtaCte.ExecuteNonQuery();
            cmCtaCte.Connection.Close();

            cmCtaCte = null;
        }

        public void eliminarPago(Entidades.Pagos oPagoE)
        {
            cmCtaCte = new SqlCommand();

            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "eliminarPago";

            cmCtaCte.Parameters.AddWithValue("@Id", oPagoE.Id);

            cmCtaCte.ExecuteNonQuery();
            cmCtaCte.Connection.Close();

            cmCtaCte = null;
        }

        public DataTable obtenerPagos(string tipoTramite, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtPagos = new DataTable();
            daCtaCte = new SqlDataAdapter();

            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            //cmCtaCte.CommandText = "obtenerPagos";
            cmCtaCte.CommandText = "obtenerPagos_1";
            cmCtaCte.Parameters.AddWithValue("@texto", texto);
            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCtaCte.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCtaCte.Parameters.AddWithValue("@tipoTramite", tipoTramite);

            daCtaCte.SelectCommand = cmCtaCte;
            daCtaCte.Fill(dtPagos);

            cmCtaCte.Connection.Close();

            return dtPagos;
        }

        public Entidades.Pagos buscarPago(Entidades.Pagos oPagoE)
        {
            cmCtaCte = new SqlCommand();

            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "buscarPago";

            cmCtaCte.Parameters.AddWithValue("@Id", oPagoE.Id);


            SqlDataReader drPago = cmCtaCte.ExecuteReader();

            while (drPago.Read())
            {
                oPagoE.NroRecibo = drPago["nroRecibo"].ToString();
                oPagoE.Fecha = Convert.ToDateTime(drPago["Fecha"].ToString());

                Entidades.Persona oProveedor = new Entidades.Persona();
                oProveedor.idPersona = Convert.ToInt32(drPago["idProveedor"].ToString());
                oProveedor.razonSocial = drPago["razonSocial"].ToString();

                oPagoE.Persona = oProveedor;
                oPagoE.TipoPago = drPago["tipoPago"].ToString();
                oPagoE.Importe = float.Parse(drPago["importe"].ToString());
                oPagoE.Observaciones = drPago["observaciones"].ToString();

            }

            cmCtaCte.Connection.Close();

            cmCtaCte = null;

            return oPagoE;

        }

        #endregion
    }
}
