using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class CierreCaja
    {
        Datos.CierreCaja oCierreD = new Datos.CierreCaja();
        Negocio.Venta oVentaN = new Negocio.Venta();

        public Entidades.CierreCaja findByIdOrLast(Entidades.CierreCaja oCierre, Entidades.CierreCaja.tipoBusqueda tipoBusqueda, string texto)
        {
            DataTable dtCierreCaja = findCierreCaja(oCierre, tipoBusqueda, texto);
            List<Entidades.CierreCaja> listCierreCaja = convertDatatableToList(dtCierreCaja);

            return listCierreCaja[0];
        }

        private static List<Entidades.CierreCaja> convertDatatableToList(DataTable dtCierreCaja)
        {
            List<Entidades.CierreCaja> listCierreCaja = new List<Entidades.CierreCaja>();
            Entidades.CierreCaja oCierreE = null;
            if (dtCierreCaja.Rows.Count > 0)
            {
                Datos.Sucursal oSucursalD = new Datos.Sucursal();
                Entidades.Sucursal oSucursalE = oSucursalD.findById(Convert.ToInt32(dtCierreCaja.Rows[0]["idSucursal"]));
                foreach (DataRow drCierreCaja in dtCierreCaja.Rows)
                {
                    oCierreE = new Entidades.CierreCaja();
                    oCierreE.Id = Convert.ToInt32(drCierreCaja["id"]);
                    oCierreE.Sucursal = oSucursalE;
                    oCierreE.UsuarioInicio = Convert.ToString(drCierreCaja["usuarioInicio"]);
                    oCierreE.UsuarioCierre = Convert.ToString(drCierreCaja["usuarioCierre"]);
                    oCierreE.FechaHoraInicio = Convert.ToDateTime(drCierreCaja["fechaHoraInicio"]);
                    oCierreE.FechaHoraCierre = string.IsNullOrEmpty(drCierreCaja["fechaHoraCierre"].ToString()) ? (DateTime?)null : Convert.ToDateTime(drCierreCaja["fechaHoraCierre"].ToString());
                    oCierreE.CajaInicio = string.IsNullOrEmpty(drCierreCaja["cajaInicio"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaInicio"].ToString());
                    oCierreE.Ventas = string.IsNullOrEmpty(drCierreCaja["ventas"].ToString()) ? (float?)null : float.Parse(drCierreCaja["ventas"].ToString());
                    oCierreE.Gastos = string.IsNullOrEmpty(drCierreCaja["gastos"].ToString()) ? (float?)null : float.Parse(drCierreCaja["gastos"].ToString());
                    oCierreE.CajaCierre = string.IsNullOrEmpty(drCierreCaja["cajaCierre"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaCierre"].ToString());
                    oCierreE.Diferencia = string.IsNullOrEmpty(drCierreCaja["diferencia"].ToString()) ? (float?)null : float.Parse(drCierreCaja["diferencia"].ToString());
                    oCierreE.CajaInicioSiguiente = string.IsNullOrEmpty(drCierreCaja["cajaInicioSiguiente"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaInicioSiguiente"].ToString());
                    oCierreE.ImporteRetirado = string.IsNullOrEmpty(drCierreCaja["importeRetirado"].ToString()) ? (float?)null : float.Parse(drCierreCaja["importeRetirado"].ToString());

                    listCierreCaja.Add(oCierreE);
                }
            }
            return listCierreCaja;
        }

        public DataTable findCierreCaja(Entidades.CierreCaja oCierre, Entidades.CierreCaja.tipoBusqueda tipoBusqueda, string texto)
        {
            return oCierreD.findCierreCaja(oCierre, tipoBusqueda, texto);
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreE)
        {
            oCierreD.addOrEditCierreCaja(oCierreE);
        }

        public float obtenerTotalVentas(int idSucursal, DateTime? fechaInicioCaja, DateTime? fechaCierreCaja)
        { 
            return oVentaN.obtenerTotalVentas(idSucursal, fechaInicioCaja, fechaCierreCaja);
        }

        
    }
}
