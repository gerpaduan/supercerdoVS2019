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
            DataTable dtCierreCaja = findCierreCaja(oCierre, tipoBusqueda, texto, null);
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
                    oCierreE.UsuarioCierre = new Entidades.Usuario();
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
                    oCierreE.EgresosCaja = string.IsNullOrEmpty(drCierreCaja["gastos"].ToString()) ? (float?)null : float.Parse(drCierreCaja["gastos"].ToString());
                    oCierreE.CajaCierre = string.IsNullOrEmpty(drCierreCaja["cajaCierre"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaCierre"].ToString());
                    oCierreE.Diferencia = string.IsNullOrEmpty(drCierreCaja["diferencia"].ToString()) ? (float?)null : float.Parse(drCierreCaja["diferencia"].ToString());
                    oCierreE.CajaInicioSiguiente = string.IsNullOrEmpty(drCierreCaja["cajaInicioSiguiente"].ToString()) ? (float?)null : float.Parse(drCierreCaja["cajaInicioSiguiente"].ToString());
                    oCierreE.ImporteRetirado = string.IsNullOrEmpty(drCierreCaja["importeRetirado"].ToString()) ? (float?)null : float.Parse(drCierreCaja["importeRetirado"].ToString());

                    listCierreCaja.Add(oCierreE);
                }
            }
            return listCierreCaja;
        }

        public DataTable findCierreCaja(Entidades.CierreCaja oCierre, Entidades.CierreCaja.tipoBusqueda tipoBusqueda, string texto, DateTime? fechaDesde)
        {
            return oCierreD.findCierreCaja(oCierre, tipoBusqueda, texto, fechaDesde);
        }

        public void addOrEditCierreCaja(Entidades.CierreCaja oCierreE)
        {
            oCierreD.addOrEditCierreCaja(oCierreE);
        }
        public Entidades.CierreCaja.ResultadoOperacion addOrEditCierreCaja_Result(Entidades.CierreCaja oCierreE)
        {

            var result = new Entidades.CierreCaja.ResultadoOperacion();
            try
            {
                addOrEditCierreCaja(oCierreE);

                result.Ok = true;
                result.Mensaje = "Cierre de caja registrado correctamente.";
            }
            catch (Exception ex)
            {
                result.Ok = false;
                result.Mensaje = ex.Message; // o un mensaje personalizado
            }

            return result;
        }

        public DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja)
        {
            return oCierreD.findCierreCajaMultiples(listaCierreCaja);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaInicioCaja, DateTime? fechaCierreCaja)
        {
            return oVentaN.obtenerTotalVentas(idVendedor, idSucursal, fechaInicioCaja, fechaCierreCaja);
        }

        #region TipoEgresoCaja
        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso)
        {
            return oCierreD.obtenerTiposEgresoCaja(buscarText, idTipoEgreso);
        }
        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto)
        {
            oCierreD.addOrEditTipoEgreso(id, tipoEgresoCaja,esGasto);
        }
        public void eliminarTipoEgreso(int id)
        {
            oCierreD.eliminarTipoEgreso(id);
        }

        public int getIdEgresoCajaPorCompra()
        {
            //DataTable tiposEgresos = obtenerTiposEgresoCaja();
            int idTipoEgreso = Entidades.Parametros.idCompraEgresoCaja;
            //foreach (DataRow row in tiposEgresos.Rows)
            //{
            //    if (!string.IsNullOrEmpty(row["esCompra"].ToString()) && !row["esCompra"].ToString().Equals("0"))
            //        idTipoEgreso = Convert.ToInt32(row["id"].ToString());
            //}
            return idTipoEgreso;
        }

        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCierreD.obtenerEgresosCaja(idSucursal, idUsuario, idTipoEgresoCaja, texto, fechaDesde, fechaHasta);
        }

        public Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja)
        {
            return oCierreD.addOrEditEgresoCaja(oEgresoCaja);
        }

        public Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja)
        {
            Entidades.EgresoCaja oEgresoCaja = oCierreD.getEgresoCajaById(idEgresoCaja);

            if (oEgresoCaja != null)
            {
                Negocio.Usuario oUserN = new Usuario();

                oEgresoCaja.CreadoPorUser = oUserN.getUserById(oEgresoCaja.CreadoPor);
                oEgresoCaja.ActualizadoPorUser = oUserN.getUserById(oEgresoCaja.ActualizadoPor);
            }

            return oEgresoCaja;
        }

        public Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID)
        {
            return oCierreD.findEgresoCajaByTablaYId(tabla, tablaID);
        }

        public float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierreE)
        {
            return  oCierreD.getMontoEgresosCajaVendedor(oCierreE);
        }

        public DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierreE)
        {
            return oCierreD.getEgresosCajaVendedor(oCierreE);
        }

        public bool validarCajaAbiertaVendedor(DateTime fechaHoraRegistro, Entidades.Sucursal oSucursalE, Entidades.Usuario oUsuario)
        {
            bool resp = true;
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0) || oCierreE.FechaHoraInicio > fechaHoraRegistro || fechaHoraRegistro > DateTime.Now)
            {
                resp = false;
                //MessageBox.Show("La fecha y hora del egreso de caja (" + Utilidades.Util_Form.fechaFormato24Horas(txtFechaEgresoCaja.Value) + ") debe ser mayor a la fecha de apertura de caja (" +
                //Utilidades.Util_Form.fechaFormato24Horas(oCierreE.FechaHoraInicio) + ")",
                //    "Mensaje de Error", MessageBoxButtons.OK, MessageBoxIcon.Stop);

            }

            return resp;
        }
        #endregion
    }
}
