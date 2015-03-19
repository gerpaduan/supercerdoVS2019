using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Datos
{
    class CierreCaja
    {
        Utilidades.Conexion conn = new Utilidades.Conexion();
        SqlDataAdapter daCierreCaja;
        SqlCommand cmCierreCaja;

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreCajaE)
        {
            cmCierreCaja = new SqlCommand();

            cmCierreCaja.Connection = conn.conectar();
            cmCierreCaja.Connection.Open();
            cmCierreCaja.CommandType = CommandType.StoredProcedure;
            cmCierreCaja.CommandText = "addOrEditCierreCaja";
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Id);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Sucursal.IdSucursal);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.FechaHoraInicio);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.FechaHoraCierre);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.CajaInicio);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Ventas);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Gastos);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.SaldoCaja);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.CajaCierre);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Diferencia);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.CajaInicioSiguiente);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.ImporteRetirado);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.UsuarioInicio);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.UsuarioCierre);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Creado);
            cmCierreCaja.Parameters.AddWithValue("@", oCierreCajaE.Actualizado);

            cmCierreCaja.ExecuteNonQuery();
            cmCierreCaja.Connection.Close();

        }
    }
}
