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
                    selectText = "select usuarioInicio as Iniciada_Por, fechaHoraInicio as Inicio, fechaHoraCierre as Cierre, " +
                        "round(cajaInicio, 2) as Caja_Inicial, round(ventas, 2) as Ventas, round(gastos, 2) as Gastos, round(cajaCierre, 2) as Caja_Cierre, round(diferencia, 2) as Diferencia, " +
                        "round(cajaInicioSiguiente, 2) as Caja_Ini_Sig, round(importeRetirado, 2) as Retirado, " +
                        "usuarioCierre as Cerrada_Por from CierreCaja where idSucursal = "
                        + oCierreParam.Sucursal.idSucursal + " and usuarioInicio like '%" + texto + "%' order by id desc ";
                        break;
                case Entidades.CierreCaja.tipoBusqueda.FindById:
                        selectText = "select * from CierreCaja where idSucursal = "
                            + oCierreParam.Sucursal.idSucursal + " id =  " + oCierreParam.Id;
                        break;
                case Entidades.CierreCaja.tipoBusqueda.FindLast:
                        selectText = "select top 1 * from CierreCaja where idSucursal = "
                            + oCierreParam.Sucursal.idSucursal + " order by id desc";
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
            cmCierreCaja.Parameters.AddWithValue("@saldoCaja", oCierreCajaE.SaldoCaja);
            cmCierreCaja.Parameters.AddWithValue("@cajaCierre", oCierreCajaE.CajaCierre);
            cmCierreCaja.Parameters.AddWithValue("@diferencia", oCierreCajaE.Diferencia);
            cmCierreCaja.Parameters.AddWithValue("@cajaInicioSiguiente", oCierreCajaE.CajaInicioSiguiente);
            cmCierreCaja.Parameters.AddWithValue("@importeRetirado", oCierreCajaE.ImporteRetirado);
            cmCierreCaja.Parameters.AddWithValue("@usuarioInicio", oCierreCajaE.UsuarioInicio);
            cmCierreCaja.Parameters.AddWithValue("@usuarioCierre", oCierreCajaE.UsuarioCierre);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();
        }
    }
}
