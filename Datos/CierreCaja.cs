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
                    selectText = "select Usuarios.nombre as Iniciada_Por, fechaHoraInicio as Inicio, fechaHoraCierre as Cierre, " +
                        "round(cajaInicio, 2) as Caja_Inicial, round(ventas, 2) as Ventas, round(gastos, 2) as Gastos, round(cajaCierre, 2) as Caja_Cierre, round(diferencia, 2) as Diferencia, " +
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
            cmCierreCaja.Parameters.AddWithValue("@gastos", oCierreCajaE.Gastos);
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

        #region Gastos

        public DataTable obtenerTipoGasto()
        {
            string selectText = "Select * from TipoGasto";
            DataTable dtTipoGasto = new DataTable();
            SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            daCierreCaja.Fill(dtTipoGasto);           
            conn.cerraConexion();

            return dtTipoGasto;
        }

        public DataTable obtenerGastos(int idSucursal, int idTipoGasto, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            DataTable dtGastos = new DataTable();
            daCierreCaja = new SqlDataAdapter();

            cmCierreCaja = new SqlCommand();
            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerGastos";
            cmCierreCaja.Parameters.AddWithValue("@texto", texto);
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", fechaDesde);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", fechaHasta);
            cmCierreCaja.Parameters.AddWithValue("@idTipoGasto", idTipoGasto);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", idSucursal);

            daCierreCaja.SelectCommand = cmCierreCaja;
            daCierreCaja.Fill(dtGastos);

            cmCierreCaja.Connection.Close();

            return dtGastos;
        }

        public void addOrEditGasto(Entidades.Gasto oGasto)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "addOrEditGasto";
            cmCierreCaja.Parameters.AddWithValue("@id", oGasto.Id);
            cmCierreCaja.Parameters.AddWithValue("@fecha", oGasto.Fecha);
            cmCierreCaja.Parameters.AddWithValue("@idTipoGasto", oGasto.IdTipoGasto);
            cmCierreCaja.Parameters.AddWithValue("@descripcion", oGasto.Descripcion);
            cmCierreCaja.Parameters.AddWithValue("@detalle", oGasto.Detalle);
            cmCierreCaja.Parameters.AddWithValue("@monto", oGasto.Monto);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oGasto.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@creadoPor", oGasto.CreadoPor);
            cmCierreCaja.Parameters.AddWithValue("@actualizadoPor", oGasto.ActualizadoPor);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();
        }

        public Entidades.Gasto getGastoById(int idGasto)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerGastos";
            cmCierreCaja.Parameters.AddWithValue("@id", idGasto);

            SqlDataReader drGasto = cmCierreCaja.ExecuteReader();

            Entidades.Gasto oGasto = new Entidades.Gasto();
            Datos.Sucursal oSucD = new Datos.Sucursal();
            Datos.Usuario oUserD = new Datos.Usuario();

            while (drGasto.Read())
            {
                oGasto.Id = Convert.ToInt32(drGasto["id"].ToString());
                oGasto.Fecha = Convert.ToDateTime(drGasto["fechaHora"].ToString());
                oGasto.IdTipoGasto = Convert.ToInt32(drGasto["idTipoGasto"].ToString());
                oGasto.TipoGasto = drGasto["tipoGasto"].ToString();
                oGasto.Descripcion = drGasto["descripcion"].ToString();
                oGasto.Detalle = drGasto["detalle"].ToString();
                oGasto.Monto =  float.Parse(drGasto["monto"].ToString());
                oGasto.Sucursal = oSucD.findById(Convert.ToInt32(drGasto["idSucursal"].ToString()));
                oGasto.Creado = drGasto["creado"].Equals(null) ? (DateTime?)null : Convert.ToDateTime(drGasto["creado"].ToString());
                oGasto.CreadoPor = Convert.ToInt32(drGasto["creadoPor"].ToString());
                DateTime? fechaNull = null;
                oGasto.Actualizado = !String.IsNullOrEmpty(drGasto["actualizado"].ToString()) ? (Convert.ToDateTime(drGasto["actualizado"].ToString())) : fechaNull;
                oGasto.ActualizadoPor = drGasto["actualizadoPor"].ToString().Length > 0 ? Convert.ToInt32(drGasto["actualizadoPor"]) : 0;
            }

            cmCierreCaja.Connection.Close();
            return oGasto;
        }

        public float getMontoGastosVendedor(Entidades.CierreCaja oCierre)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerGastos";
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", DateTime.Now.Date);
            cmCierreCaja.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@montoGasto", true);

            SqlDataReader drGasto = cmCierreCaja.ExecuteReader();
            float gasto = 0;
            while (drGasto.Read())
            {
                if (drGasto["monto"] != DBNull.Value)
                {
                    gasto = float.Parse(drGasto["monto"].ToString());                    
                }
            }
            cmCierreCaja.Connection.Close();
            return gasto;
        }

        public DataTable getGastosVendedor(Entidades.CierreCaja oCierre)
        {
            DataTable dtGastos = new DataTable();
            daCierreCaja = new SqlDataAdapter();

            cmCierreCaja = new SqlCommand();
            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "obtenerGastos";
            cmCierreCaja.Parameters.AddWithValue("@fechaDesde", oCierre.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@fechaHasta", DateTime.Now.Date);
            cmCierreCaja.Parameters.AddWithValue("@idVendedor", oCierre.UsuarioInicio.Id);
            cmCierreCaja.Parameters.AddWithValue("@idSucursal", oCierre.Sucursal.idSucursal);
            cmCierreCaja.Parameters.AddWithValue("@verGasto", true);

            daCierreCaja.SelectCommand = cmCierreCaja;
            daCierreCaja.Fill(dtGastos);

            cmCierreCaja.Connection.Close();

            return dtGastos;
        }

        #endregion
    }
}
