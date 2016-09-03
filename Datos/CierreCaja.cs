using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    public class CierreCaja
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daCierreCaja;
        SqlCommand cmCierreCaja;

        public DataTable findCierreCaja(Entidades.CierreCaja oCierreParam, Entidades.CierreCaja.tipoBusqueda tipoBusquedaParam, string texto)
        {
            
            //cmCierreCaja = new SqlCommand();
            string selectText = "";

            //cmCierreCaja.Connection = conn.conectar();
            switch (tipoBusquedaParam)
            {
                case Entidades.CierreCaja.tipoBusqueda.FindAll:
                    selectText = "select CierreCaja.id, Usuarios.nombre as Iniciada_Por, fechaHoraInicio as Inicio, fechaHoraCierre as Cierre, " +
                        "round(cajaInicio, 2) as Caja_Inicial, round(ventas, 2) as Ventas, round(gastos, 2) as EgresosCaja, round(cajaCierre, 2) as Caja_Cierre, round(diferencia, 2) as Diferencia, " +
                        "round(cajaInicioSiguiente, 2) as Caja_Ini_Sig, round(importeRetirado, 2) as Retirado, " +
                        "UsuarioCierre.nombre as Cerrada_Por from CierreCaja, Usuarios, Usuarios as UsuarioCierre " +
                        "where CierreCaja.usuarioInicio = Usuarios.id and CierreCaja.usuarioCierre = UsuarioCierre.id and idSucursal = "
                        + oCierreParam.Sucursal.idSucursal + " and Usuarios.nombre like '%" + texto + "%' order by CierreCaja.id desc ";
                    break;
                case Entidades.CierreCaja.tipoBusqueda.FindOpen:
                    selectText = "select CierreCaja.id, CierreCaja.usuarioInicio, Usuarios.nombre as vendedor, fechaHoraInicio, " +
                        "round(cajaInicio, 2) as cajaInicio from CierreCaja, Usuarios " +
                        "where CierreCaja.usuarioInicio = Usuarios.id and idSucursal = "
                        + oCierreParam.Sucursal.idSucursal + " and Usuarios.nombre like '%" + texto + "%' and CierreCaja.usuarioCierre = 0 ";
                    break;
                case Entidades.CierreCaja.tipoBusqueda.FindById:
                        selectText = "select * from CierreCaja where id =  " + oCierreParam.Id;
                        break;
                case Entidades.CierreCaja.tipoBusqueda.FindLast:
                        selectText = "select top 1 * from CierreCaja where idSucursal = "
                            + oCierreParam.Sucursal.idSucursal + " and usuarioInicio = "+ oCierreParam.UsuarioInicio.Id +" order by id desc";
                        break;
                case Entidades.CierreCaja.tipoBusqueda.FindLastOpen:
                        selectText = "select top 1 * from CierreCaja where usuarioInicio = " + oCierreParam.UsuarioInicio.Id +
                            " and id < " + oCierreParam.Id + " order by id desc";
                        break;
            }
            DataTable dtCierreCaja = new DataTable();
            SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            daCierreCaja.Fill(dtCierreCaja);           
            conn.cerraConexion();

            return dtCierreCaja;
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreCajaE)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "addOrEditCierreCaja";
            cmCierreCaja.Parameters.AddWithValue("@id", oCierreCajaE.Id);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oCierreCajaE.Sucursal.IdSucursal);
            cmCierreCaja.Parameters.AddWithValue("@fechaHoraInicio", oCierreCajaE.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@fechaHoraCierre", oCierreCajaE.FechaHoraCierre);
            cmCierreCaja.Parameters.AddWithValue("@cajaInicio", oCierreCajaE.CajaInicio);
            cmCierreCaja.Parameters.AddWithValue("@ventas", oCierreCajaE.Ventas);
            cmCierreCaja.Parameters.AddWithValue("@gastos", oCierreCajaE.EgresosCaja);
            cmCierreCaja.Parameters.AddWithValue("@cajaCierre", oCierreCajaE.CajaCierre);
            cmCierreCaja.Parameters.AddWithValue("@diferencia", oCierreCajaE.Diferencia);
            cmCierreCaja.Parameters.AddWithValue("@cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
            cmCierreCaja.Parameters.AddWithValue("@importeRetirado", oCierreCajaE.ImporteRetirado);
            cmCierreCaja.Parameters.AddWithValue("@usuarioInicio", oCierreCajaE.UsuarioInicio.Id);
            cmCierreCaja.Parameters.AddWithValue("@usuarioCierre", oCierreCajaE.UsuarioCierre.Id);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();
        }

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            string selectText = "select CierreCaja.*, Usuarios.* FROM CierreCaja INNER JOIN Usuarios ON CierreCaja.usuarioInicio = Usuarios.id where ";
            for (int nroIndex = 0; nroIndex < listaCierreCaja.Count; nroIndex++)
			{
                if(nroIndex > 0) selectText += " OR ";

                selectText += "CierreCaja.id = " + listaCierreCaja[nroIndex].Id;
			}
            DataTable dtCierreCaja = new DataTable();
            SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            daCierreCaja.Fill(dtCierreCaja);
            conn.cerraConexion();

            return dtCierreCaja;
        }

        #region EgresosCaja

        public DataTable obtenerTiposEgresoCaja()
        {
            string selectText = "Select * from TiposEgresoCaja order by orden, tipoEgresoCaja";
            DataTable dtTipoEgresoCaja = new DataTable();
            SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            daCierreCaja.Fill(dtTipoEgresoCaja);           
            conn.cerraConexion();

            return dtTipoEgresoCaja;
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtEgresosCaja = new DataTable();
            daCierreCaja = new SqlDataAdapter();

            cmCierreCaja = new SqlCommand();
            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerEgresosCaja";
            cmCierreCaja.Parameters.AddWithValue("@texto", texto);
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCierreCaja.Parameters.AddWithValue("@idTipoEgresoCaja", idTipoEgresoCaja);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", idSucursal);

            daCierreCaja.SelectCommand = cmCierreCaja;
            daCierreCaja.Fill(dtEgresosCaja);

            cmCierreCaja.Connection.Close();

            return dtEgresosCaja;
        }

        public Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "addOrEditEgresoCaja";
            cmCierreCaja.Parameters.AddWithValue("@id", oEgresoCaja.Id);
            cmCierreCaja.Parameters.AddWithValue("@fecha", oEgresoCaja.Fecha);
            cmCierreCaja.Parameters.AddWithValue("@idTipoEgresoCaja", oEgresoCaja.IdTipoEgresoCaja);
            cmCierreCaja.Parameters.AddWithValue("@descripcion", oEgresoCaja.Descripcion);
            cmCierreCaja.Parameters.AddWithValue("@detalle", oEgresoCaja.Detalle);
            cmCierreCaja.Parameters.AddWithValue("@monto", oEgresoCaja.Monto);
            cmCierreCaja.Parameters.AddWithValue("@idCompra", oEgresoCaja.IdCompra);
            cmCierreCaja.Parameters.AddWithValue("@tabla", oEgresoCaja.Tabla);
            cmCierreCaja.Parameters.AddWithValue("@idTabla", oEgresoCaja.IdTabla);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oEgresoCaja.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@creadoPor", oEgresoCaja.CreadoPor);
            cmCierreCaja.Parameters.AddWithValue("@actualizadoPor", oEgresoCaja.ActualizadoPor);

            oEgresoCaja.Id = (int)cmCierreCaja.ExecuteScalar();
            cmCierreCaja.Connection.Close();

            return oEgresoCaja;
        }

        public Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerEgresosCaja";
            cmCierreCaja.Parameters.AddWithValue("@id", idEgresoCaja);

            SqlDataReader drEgresoCaja = cmCierreCaja.ExecuteReader();

            Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();
            Datos.Sucursal oSucD = new Datos.Sucursal();
            Datos.Usuario oUserD = new Datos.Usuario();

            while (drEgresoCaja.Read())
            {
                oEgresoCaja.Id = Convert.ToInt32(drEgresoCaja["id"].ToString());
                oEgresoCaja.Fecha = Convert.ToDateTime(drEgresoCaja["fechaHora"].ToString());
                oEgresoCaja.IdTipoEgresoCaja = Convert.ToInt32(drEgresoCaja["idTipoEgresoCaja"].ToString());
                oEgresoCaja.TipoEgresoCaja = drEgresoCaja["tipoEgresoCaja"].ToString();
                oEgresoCaja.Descripcion = drEgresoCaja["descripcion"].ToString();
                oEgresoCaja.Detalle = drEgresoCaja["detalle"].ToString();
                oEgresoCaja.Monto = float.Parse(drEgresoCaja["monto"].ToString());
                oEgresoCaja.IdCompra = drEgresoCaja["idCompra"] != DBNull.Value ? Convert.ToInt32(drEgresoCaja["idCompra"].ToString()) : oEgresoCaja.IdCompra;
                //oEgresoCaja.Tabla = drEgresoCaja["tabla"].ToString();
                //oEgresoCaja.IdTabla = drEgresoCaja["idTabla"] != DBNull.Value ? Convert.ToInt32(drEgresoCaja["idTabla"].ToString()) : oEgresoCaja.IdCompra;
                oEgresoCaja.Sucursal = oSucD.findById(Convert.ToInt32(drEgresoCaja["idSucursal"].ToString()));
                oEgresoCaja.Creado = drEgresoCaja["creado"].Equals(null) ? (DateTime?)null : Convert.ToDateTime(drEgresoCaja["creado"].ToString());
                oEgresoCaja.CreadoPor = Convert.ToInt32(drEgresoCaja["creadoPor"].ToString());
                DateTime? fechaNull = null;
                oEgresoCaja.Actualizado = !String.IsNullOrEmpty(drEgresoCaja["actualizado"].ToString()) ? (Convert.ToDateTime(drEgresoCaja["actualizado"].ToString())) : fechaNull;
                oEgresoCaja.ActualizadoPor = drEgresoCaja["actualizadoPor"].ToString().Length > 0 ? Convert.ToInt32(drEgresoCaja["actualizadoPor"]) : -1;
            }

            cmCierreCaja.Connection.Close();
            return oEgresoCaja;
        }

        private Entidades.EgresoCaja cargarEgresoCajaDataReader(SqlDataReader drEgresoCaja)
        {
            Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();
            Datos.Sucursal oSucD = new Datos.Sucursal();
            Datos.Usuario oUserD = new Datos.Usuario();

            oEgresoCaja.Id = Convert.ToInt32(drEgresoCaja["id"].ToString());
            oEgresoCaja.Fecha = Convert.ToDateTime(drEgresoCaja["fechaHora"].ToString());
            oEgresoCaja.IdTipoEgresoCaja = Convert.ToInt32(drEgresoCaja["idTipoEgresoCaja"].ToString());
            //oEgresoCaja.TipoEgresoCaja = drEgresoCaja["tipoEgresoCaja"].ToString();
            oEgresoCaja.Descripcion = drEgresoCaja["descripcion"].ToString();
            oEgresoCaja.Detalle = drEgresoCaja["detalle"].ToString();
            oEgresoCaja.Monto = float.Parse(drEgresoCaja["monto"].ToString());
            oEgresoCaja.IdCompra = drEgresoCaja["idCompra"] != DBNull.Value ? Convert.ToInt32(drEgresoCaja["idCompra"].ToString()) : oEgresoCaja.IdCompra;
            //oEgresoCaja.Tabla = drEgresoCaja["tabla"].ToString();
            //oEgresoCaja.IdTabla = drEgresoCaja["idTabla"] != DBNull.Value ? Convert.ToInt32(drEgresoCaja["idTabla"].ToString()) : oEgresoCaja.IdCompra;
            oEgresoCaja.Sucursal = oSucD.findById(Convert.ToInt32(drEgresoCaja["idSucursal"].ToString()));
            oEgresoCaja.Creado = drEgresoCaja["creado"].Equals(null) ? (DateTime?)null : Convert.ToDateTime(drEgresoCaja["creado"].ToString());
            oEgresoCaja.CreadoPor = Convert.ToInt32(drEgresoCaja["creadoPor"].ToString());
            DateTime? fechaNull = null;
            oEgresoCaja.Actualizado = !String.IsNullOrEmpty(drEgresoCaja["actualizado"].ToString()) ? (Convert.ToDateTime(drEgresoCaja["actualizado"].ToString())) : fechaNull;
            oEgresoCaja.ActualizadoPor = drEgresoCaja["actualizadoPor"].ToString().Length > 0 ? Convert.ToInt32(drEgresoCaja["actualizadoPor"]) : -1;

            return oEgresoCaja;
        }

        public Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.CommandType = CommandType.Text;
            cmCierreCaja.CommandText = "SELECT  top 1 EgresosCaja.* "+
			                            "FROM EgresosCaja "+
			                            "WHERE     (tabla = '"+tabla+"') AND (idTabla = "+tablaID+") "+
			                            "ORDER BY EgresosCaja.id desc";

            Entidades.EgresoCaja oEgresoCaja = new Entidades.EgresoCaja();
            cmCierreCaja.Connection.Open();

            SqlDataReader drEgresoCaja = cmCierreCaja.ExecuteReader();
            while (drEgresoCaja.Read())
            {
                oEgresoCaja = cargarEgresoCajaDataReader(drEgresoCaja);
            }
            cmCierreCaja.Connection.Close();
            return oEgresoCaja;
        }

        public float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerEgresosCaja";
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
            cmCierreCaja.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@montoEgresoCaja", true);

            SqlDataReader drEgresoCaja = cmCierreCaja.ExecuteReader();
            float egresoCaja = 0;
            while (drEgresoCaja.Read())
            {
                if (drEgresoCaja["monto"] != DBNull.Value)
                {
                    egresoCaja = float.Parse(drEgresoCaja["monto"].ToString());                    
                }
            }
            cmCierreCaja.Connection.Close();
            return egresoCaja;
        }

        public DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierre)
        {
            DataTable dtEgresosCaja = new DataTable();
            daCierreCaja = new SqlDataAdapter();

            cmCierreCaja = new SqlCommand();
            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerEgresosCaja";
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", oCierre.FechaHoraCierre == null ? DateTime.Now : oCierre.FechaHoraCierre);
            cmCierreCaja.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@verEgresoCaja", true);

            daCierreCaja.SelectCommand = cmCierreCaja;
            daCierreCaja.Fill(dtEgresosCaja);

            cmCierreCaja.Connection.Close();

            return dtEgresosCaja;
        }

        #endregion
    }
}
