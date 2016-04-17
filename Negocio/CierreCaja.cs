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
            Entidades.CierreCaja cierreCaja = listCierreCaja.Count > 0 ? listCierreCaja[0] : null;
            return cierreCaja;
        }

        private static List<Entidades.CierreCaja> convertDatatableToList(DataTable dtCierreCaja)
        {
            List<Entidades.CierreCaja> listCierreCaja = new List<Entidades.CierreCaja>();
            Entidades.CierreCaja oCierreE = null;
            if (dtCierreCaja.Rows.Count > 0)
            {
                Datos.Sucursal oSucursalD = new Datos.Sucursal();
                Entidades.Sucursal oSucursalE = oSucursalD.findById(Convert.ToInt32(dtCierreCaja.Rows[0]["idSucursal"]));
                
                Negocio.Usuario oUsuarioN = new Negocio.Usuario();
                List<Entidades.Usuario> listUsers = oUsuarioN.convertDatatableToList();

                foreach (DataRow drCierreCaja in dtCierreCaja.Rows)
                {
                    oCierreE = new Entidades.CierreCaja();
                    oCierreE.Id = Convert.ToInt32(drCierreCaja["id"]);
                    oCierreE.Sucursal = oSucursalE;
                    foreach (Entidades.Usuario user in listUsers)
                    {
                        if (Convert.ToInt32(drCierreCaja["usuarioInicio"]).Equals(user.Id))
                        {
                            oCierreE.UsuarioInicio = user;
                            break;
                        }
                    }
                    foreach (Entidades.Usuario user in listUsers)
                    {
                        if (Convert.ToInt32(drCierreCaja["usuarioCierre"]).Equals(user.Id))
                        {
                            oCierreE.UsuarioCierre = user;
                            break;
                        }
                    }
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

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            return oCierreD.findCierreCajaMultiples(listaCierreCaja);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaInicioCaja, DateTime? fechaCierreCaja)
        {
            return oVentaN.obtenerTotalVentas(idVendedor, idSucursal, fechaInicioCaja, fechaCierreCaja);
        }
        #region TipoGasto
        public DataTable obtenerTipoGasto()
        {
            return oCierreD.obtenerTipoGasto();
        }

        public DataTable obtenerGastos(int idSucursal, int idTipoGasto, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCierreD.obtenerGastos(idSucursal, idTipoGasto, texto, fechaDesde, fechaHasta);
        }

        public void addOrEditGasto(Entidades.Gasto oGasto)
        {
            oCierreD.addOrEditGasto(oGasto);
        }
        public Entidades.Gasto getGastoById(int idGasto)
        {
            Entidades.Gasto oGasto = oCierreD.getGastoById(idGasto);

            if (oGasto != null)
            {
                Negocio.Usuario oUserN = new Usuario();

                oGasto.CreadoPorUser = oUserN.getUserById(oGasto.CreadoPor);
                oGasto.ActualizadoPorUser = oUserN.getUserById(oGasto.ActualizadoPor);
            }

            return oGasto;
        }

        public float getMontoGastosVendedor(Entidades.CierreCaja oCierreE)
        {
            return  oCierreD.getMontoGastosVendedor(oCierreE);
        }

        public DataTable getGastosVendedor(Entidades.CierreCaja oCierreE)
        {
            return oCierreD.getGastosVendedor(oCierreE);
        }
        #endregion
    }
}
