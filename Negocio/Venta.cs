using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Venta
    {
        Datos.Venta oVentaD = new Datos.Venta();

        public int agregarVenta(Entidades.Venta oVentaE)
        {
            oVentaE.IdVenta = oVentaD.agregarVenta(oVentaE);
            return oVentaE.IdVenta;
        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas)
        {
            oVentaD.modificarVenta(oVentaE, SucAnterior, eliminarLineas);
        }

        public void crearMovCtaCteVenta(Entidades.Venta oVentaE)
        {
            oVentaE = oVentaD.getVentaById(oVentaE.IdVenta);
            Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
            oCtaCteN.crearMovCtaCte(oVentaE.Persona, oVentaE.FechaVenta, Entidades.MovCtaCte.tablas.Ventas, oVentaE.IdVenta, oVentaE.NroRemito,
                "", Entidades.MovCtaCte.tipoMov.Debito, oVentaE.LineasVenta.Count == 0 ? 0 : oVentaD.getTotalVenta(oVentaE.IdVenta), oVentaE.Sucursal,
                oVentaE.Creado, oVentaE.Vendedor, oVentaE.Actualizado, null, oVentaE.EnCtaCte);      
        }

        public float getTotalVenta(int idVenta)
        {
            return oVentaD.getTotalVenta(idVenta);
        }

        public DataTable obtenerVentas(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            return oVentaD.obtenerVentas(idSucursal, idVendedor, fechaDesde, fechaHasta, texto, soloAnulados);
        }

        public Entidades.Venta getVentaById(int idVenta)
        {
            return oVentaD.getVentaById(idVenta);
        }

        public DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados)
        {
            return oVentaD.getVentasVendedorCierreCaja(oCierreE, soloAnulados);
        }

        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            return oVentaD.obtenerTotalVentas(idVendedor, idSucursal, fechaDesde, fechaHasta);
        } 

        public Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            return oVentaD.agregarLineaVenta(oLineaE);
        }

        public void modificarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            oVentaD.modificarLineaVenta(oLineaE);
        }

        public  List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta)
        {
            return oVentaD.obtenerLineasVenta(idVenta);
        }

        public void agregarStockVenta(Entidades.Venta oVentaE)
        {
            oVentaD.agregarStockVenta(oVentaE);
        }

        public Entidades.Venta getUltimaVentaVendedor(int idVendedor)
        {
            return oVentaD.getUltimaVentaVendedor(idVendedor);
        }

        public void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV)
        {
            oVentaD.agregarTemporalLineaVenta(oTemporalLV);
        }

        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas)
        {
            return oVentaD.obtenerTemporalLineaVenta(idSucursal, idVendedor, fechaDesde, fechaHasta, texto, conVentas);
        }

        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto)
        {
            return oVentaD.getAllLineasVenta(idSucursal, idVendedor, fechaDesde, fechaHasta, texto);
        }

        public DataTable ultimasVentasCliente(int idSucursal, int idPersona)
        {
            return oVentaD.ultimasVentasCliente(idSucursal, idPersona);
        }
    }
}
