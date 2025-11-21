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

        public DataTable findCierreCaja(Entidades.CierreCaja oCierreParam, Entidades.CierreCaja.tipoBusqueda tipoBusquedaParam, string texto, DateTime? fechaDesde)
        {
            
            //cmCierreCaja = new SqlCommand();
            string selectText = "";
            string fechaDesdeConver = "convert(varchar,'"+fechaDesde.ToString()+"',103)";

            //cmCierreCaja.Connection = conn.conectar();
            switch (tipoBusquedaParam)
            {
                case Entidades.CierreCaja.tipoBusqueda.FindAll:
                    selectText =
                            "select CierreCaja.id, Usuarios.nombre as Iniciada_Por, fechaHoraInicio as Inicio, fechaHoraCierre as Cierre, " +
                            "round(cajaInicio, 2) as Caja_Inicial, round(ventas, 2) as Ventas, round(gastos, 2) as EgresosCaja, round(cajaCierre, 2) as Caja_Cierre, round(diferencia, 2) as Diferencia, " +
                            "round(cajaInicioSiguiente, 2) as Caja_Ini_Sig, round(importeRetirado, 2) as Retirado, " +
                            "UsuarioCierre.nombre as Cerrada_Por " +
                            "from CierreCaja " +
                            "inner join Usuarios on CierreCaja.usuarioInicio = Usuarios.id " +
                            "inner join Usuarios as UsuarioCierre on CierreCaja.usuarioCierre = UsuarioCierre.id " +
                            "where idSucursal = @sucursal " +
                            "and fechaHoraInicio > @fechaDesde " +
                            "and Usuarios.nombre like @texto " +
                            "order by CierreCaja.id desc";
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
            //DataTable dtCierreCaja = new DataTable();
            //SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            //daCierreCaja.Fill(dtCierreCaja);           
            //conn.cerraConexion();
            DataTable dtCierreCaja = new DataTable();

            using (SqlConnection cn = conn.conectar())
            {
                using (SqlCommand cmd = new SqlCommand(selectText, cn))
                {
                    cmd.CommandType = CommandType.Text;

                    switch (tipoBusquedaParam)
                    {
                        case Entidades.CierreCaja.tipoBusqueda.FindAll:
                            cmd.Parameters.AddWithValue("@sucursal", oCierreParam.Sucursal.idSucursal);
                            cmd.Parameters.AddWithValue("@fechaDesde", fechaDesde);
                            cmd.Parameters.AddWithValue("@texto", "%" + texto + "%");
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindOpen:
                            // ESTA QUERY NO USA PARÁMETROS, pero deberías parametrizarla también
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindById:
                            // recomendación: parametrizar también
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindLast:
                            // idem
                            break;

                        case Entidades.CierreCaja.tipoBusqueda.FindLastOpen:
                            // idem
                            break;
                    }

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dtCierreCaja);
                    }
                }
            }
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

        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            string where = string.IsNullOrEmpty(buscarText) ?(idTipoEgreso > 0 ? $"WHERE id = "+ idTipoEgreso : string.Empty) : $"WHERE tipoEgresoCaja LIKE '%{buscarText}%'";
            string selectText = "Select  id, tipoEgresoCaja, esGasto as Es_Gasto, creado as Creado, actualizado as Actualizado, reservadoSistema as Reservado from TiposEgresoCaja "+ where +" order by orden, tipoEgresoCaja";
            DataTable dtTipoEgresoCaja = new DataTable();
            SqlDataAdapter daCierreCaja = new SqlDataAdapter(selectText, conn.conectar());
            daCierreCaja.Fill(dtTipoEgresoCaja);           
            conn.cerraConexion();

            return dtTipoEgresoCaja;
        }

        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            bool esInsert = false;
            //significa que es un nuevo registro
            if (id == -1)
            {
                esInsert = true;    
                // Consulta para obtener el ID más grande
                string selectQuery = "SELECT ISNULL(MAX(id), 0) FROM TiposEgresoCaja";

                // Obtener el valor más grande de Id
                SqlCommand cmCierreCaja1 = new SqlCommand(selectQuery);
                cmCierreCaja1.Connection = conn.conectar();
                cmCierreCaja1.Connection.Open();

                object result = cmCierreCaja1.ExecuteScalar(); // Obtener el resultado

                // Si hay resultados, aumentar en 1 el Id
                if (result != null)
                {
                    id = Convert.ToInt32(result) + 1;
                }

                cmCierreCaja1.Connection.Close();
            }

            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            string query = esInsert ? 
                $"INSERT INTO TiposEgresoCaja (id, tipoEgresoCaja, esGasto, orden, reservadoSistema, creado) VALUES (@id, @tipoEgresoCaja, @esGasto, 10, @reservadoSistema, @creado)" :
                $"UPDATE TiposEgresoCaja SET tipoEgresoCaja = @tipoEgresoCaja, esGasto = @esGasto, actualizado = @actualizado WHERE  id = @id";

            cmCierreCaja.CommandType = CommandType.Text;
            cmCierreCaja.CommandText = query;
            cmCierreCaja.Parameters.AddWithValue("@id", id);
            cmCierreCaja.Parameters.AddWithValue("@tipoEgresoCaja", tipoEgresoCaja);
            cmCierreCaja.Parameters.AddWithValue("@esGasto", esGasto);
            cmCierreCaja.Parameters.AddWithValue("@reservadoSistema", false);
            cmCierreCaja.Parameters.AddWithValue("@creado", DateTime.Now);
            cmCierreCaja.Parameters.AddWithValue("@actualizado", DateTime.Now);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();
        }

        public void eliminarTipoEgreso(int id)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();

            string query = $"DELETE FROM TiposEgresoCaja WHERE  id = @id";

            cmCierreCaja.CommandType = CommandType.Text;
            cmCierreCaja.CommandText = query;
            cmCierreCaja.Parameters.AddWithValue("@id", id);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
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
            cmCierreCaja.Parameters.AddWithValue("@idVendedor", idUsuario);
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
            Entidades.Sucursal oSucE = new Entidades.Sucursal();
            oEgresoCaja.Sucursal = oSucE;
            oEgresoCaja.Sucursal.idSucursal = Convert.ToInt32(drEgresoCaja["idSucursal"].ToString());// oSucD.findById(Convert.ToInt32(drEgresoCaja["idSucursal"].ToString()));
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
