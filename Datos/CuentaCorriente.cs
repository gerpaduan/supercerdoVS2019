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

        public DataTable obtenerCtasCtes(string txtBusqueda)
        {
            DataTable dtCtasCtes = new DataTable();
            string consulta = "SELECT dbo.Personas.idPersona as IdPersona, dbo.Personas.razonSocial AS [Razon Social], SUM(dbo.MovCtaCte.importe) AS Saldo " +
                                "FROM dbo.Personas INNER JOIN dbo.MovCtaCte ON dbo.Personas.idPersona = dbo.MovCtaCte.idPersona "+
                                "Where  dbo.Personas.razonSocial like '%"+txtBusqueda+"%' "+
                                "GROUP BY dbo.Personas.idPersona, dbo.Personas.razonSocial";
            daCtaCte = new SqlDataAdapter(consulta, conn.conectar());
            daCtaCte.Fill(dtCtasCtes);

            return dtCtasCtes;
        }

        public DataTable getCtaCteByIdPersona(int idPersona, DateTime fechaDesde)
        {
            DataTable dtMovCtaCte = new DataTable();
            daCtaCte = new SqlDataAdapter();

            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.Connection.Open();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "getCtaCteByIdPersona";
            cmCtaCte.Parameters.AddWithValue("@idPersona", idPersona);
            cmCtaCte.Parameters.AddWithValue("@fechaDesde", fechaDesde);

            daCtaCte.SelectCommand = cmCtaCte;
            daCtaCte.Fill(dtMovCtaCte);

            cmCtaCte.Connection.Close();

            return dtMovCtaCte;
        }
        public Entidades.MovCtaCte getMovCtaCteBy(int id, Entidades.MovCtaCte.tablas tabla, int idTabla, Entidades.MovCtaCte.getBy getBy)
        {
	        cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.Text;
            if (getBy.Equals(Entidades.MovCtaCte.getBy.Id))
            {
                cmCtaCte.CommandText = "Select top 1 MovCtaCte.* from MovCtaCte where id = "+id+" order by id desc";
            } 
            if (getBy.Equals(Entidades.MovCtaCte.getBy.TablaAndId))
            {
                cmCtaCte.CommandText = "Select top 1 MovCtaCte.* from MovCtaCte where tabla = \'" + tabla.ToString() + "\' and idTabla = " + idTabla + " order by id desc";
            }

            Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
            try
            {
	            cmCtaCte.Connection.Open();
                SqlDataReader drMovCtaCte = cmCtaCte.ExecuteReader();
                using (drMovCtaCte)
                {
	                while(drMovCtaCte.Read())
                    {
                        oMovCtaCteE.Id = Convert.ToInt32(drMovCtaCte["id"]);
                        Datos.Persona oPersonaD = new Datos.Persona();
                        oMovCtaCteE.Persona = oPersonaD.findById(Convert.ToInt32(drMovCtaCte["idPersona"]));

                        oMovCtaCteE.Fecha = Convert.ToDateTime(drMovCtaCte["fecha"]);
                        oMovCtaCteE.Tabla = Convert.ToString(drMovCtaCte["tabla"]);
                        oMovCtaCteE.IdTabla = Convert.ToInt32(drMovCtaCte["idTabla"]);
                        oMovCtaCteE.Detalle = Convert.ToString(drMovCtaCte["detalle"]);
                        oMovCtaCteE.Tipo = Convert.ToString(drMovCtaCte["tipo"]);
                        oMovCtaCteE.Importe = float.Parse(drMovCtaCte["importe"].ToString());

                        Datos.Sucursal oSucursalD = new Sucursal();
                        oMovCtaCteE.Sucursal = oSucursalD.findById(Convert.ToInt32(drMovCtaCte["idSucursal"]));

                        oMovCtaCteE.QuitadoCtaCta = drMovCtaCte["quitadoCtaCte"].Equals(DBNull.Value) ? false : Convert.ToBoolean(drMovCtaCte["quitadoCtaCte"]);

                        oMovCtaCteE.Creado = Convert.ToDateTime(drMovCtaCte["creado"]);
                        oMovCtaCteE.Actualizado = drMovCtaCte["actualizado"].Equals(DBNull.Value) ? null : (DateTime?)(drMovCtaCte["actualizado"]);

                        Datos.Usuario oUsuarioD = new Usuario();
                        oMovCtaCteE.CreadoPor = oUsuarioD.getUsuarioById(Convert.ToInt32(drMovCtaCte["creadoPor"]));
                        oMovCtaCteE.ActualizadoPor = drMovCtaCte["actualizadoPor"].Equals(DBNull.Value) ? null : oUsuarioD.getUsuarioById(Convert.ToInt32(drMovCtaCte["actualizadoPor"]));

                    }
                    return oMovCtaCteE;
                }
            }
            finally
            {
	            cmCtaCte.Connection.Close();
                oMovCtaCteE = null;
            }
        }


        public Entidades.MovCtaCte addOrEditMovCtaCte(Entidades.MovCtaCte oMovCtaCteE)
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "addOrEditMovCtaCte";

            cmCtaCte.Parameters.AddWithValue("@id", oMovCtaCteE.Id);
            cmCtaCte.Parameters.AddWithValue("@idPersona", oMovCtaCteE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@fecha", oMovCtaCteE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@tabla", oMovCtaCteE.Tabla);
            cmCtaCte.Parameters.AddWithValue("@idTabla", oMovCtaCteE.IdTabla);
            cmCtaCte.Parameters.AddWithValue("@detalle", oMovCtaCteE.Detalle);
            cmCtaCte.Parameters.AddWithValue("@tipo", oMovCtaCteE.Tipo);
            cmCtaCte.Parameters.AddWithValue("@importe", oMovCtaCteE.Importe);
            cmCtaCte.Parameters.AddWithValue("@quitadoCtaCte", oMovCtaCteE.QuitadoCtaCta);
            cmCtaCte.Parameters.AddWithValue("@idSucursal", oMovCtaCteE.Sucursal.idSucursal);
            cmCtaCte.Parameters.AddWithValue("@creadoPor", oMovCtaCteE.CreadoPor.Id);
            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oMovCtaCteE.ActualizadoPor != null ? oMovCtaCteE.ActualizadoPor.Id : -1);

            cmCtaCte.Connection.Open();
            oMovCtaCteE.Id = (int)cmCtaCte.ExecuteScalar();
            cmCtaCte.Connection.Close();

            return oMovCtaCteE;
        }

        #region Pagos

        public Entidades.Pago addOrEditPago(Entidades.Pago oPagoE)
        {
            cmCtaCte = new SqlCommand();
            cmCtaCte.Connection = conn.conectar();
            cmCtaCte.CommandType = CommandType.StoredProcedure;
            cmCtaCte.CommandText = "addOrEditPago";

            cmCtaCte.Parameters.AddWithValue("@id", oPagoE.Id);
            cmCtaCte.Parameters.AddWithValue("@nroRecibo", oPagoE.NroRecibo);
            cmCtaCte.Parameters.AddWithValue("@fecha", oPagoE.Fecha);
            cmCtaCte.Parameters.AddWithValue("@idPersona", oPagoE.Persona.idPersona);
            cmCtaCte.Parameters.AddWithValue("@aProveedor", oPagoE.AProveedor);
            cmCtaCte.Parameters.AddWithValue("@formaPago", oPagoE.FormaPago);
            cmCtaCte.Parameters.AddWithValue("@banco", oPagoE.Banco);
            cmCtaCte.Parameters.AddWithValue("@nroCheque", oPagoE.NroCheque);
            cmCtaCte.Parameters.AddWithValue("@titularCheque", oPagoE.TitularCheque);
            cmCtaCte.Parameters.AddWithValue("@importe", oPagoE.Importe);
            cmCtaCte.Parameters.AddWithValue("@observaciones", oPagoE.Observaciones);
            cmCtaCte.Parameters.AddWithValue("@idSucursal", oPagoE.Sucursal.idSucursal);
            cmCtaCte.Parameters.AddWithValue("@creado", oPagoE.Creado);
            cmCtaCte.Parameters.AddWithValue("@creadoPor", oPagoE.CreadoPor);
            cmCtaCte.Parameters.AddWithValue("@actualizado", oPagoE.Actualizado);
            cmCtaCte.Parameters.AddWithValue("@actualizadoPor", oPagoE.ActualizadoPor);

            cmCtaCte.Connection.Open();
            cmCtaCte.ExecuteNonQuery();
            oPagoE.Id = (int)cmCtaCte.ExecuteScalar();
            cmCtaCte.Connection.Close();

            return oPagoE;
        }



        public void eliminarPago(Entidades.Pago oPagoE)
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

        public Entidades.Pago buscarPago(Entidades.Pago oPagoE)
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
                oPagoE.FormaPago = drPago["tipoPago"].ToString();
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
