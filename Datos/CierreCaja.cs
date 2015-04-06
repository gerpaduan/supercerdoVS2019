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
                        selectText = "select * from CierreCaja where idSucursal = "
                            + oCierreParam.Sucursal.idSucursal + " and id =  " + oCierreParam.Id;
                        break;
                case Entidades.CierreCaja.tipoBusqueda.FindLast:
                        selectText = "select top 1 * from CierreCaja where idSucursal = "
                            + oCierreParam.Sucursal.idSucursal + " and usuarioInicio = "+ oCierreParam.UsuarioInicio.Id +" order by id desc";
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
    }
}
