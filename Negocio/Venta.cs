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
           return oVentaD.agregarVenta(oVentaE);
        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior)
        {
            oVentaD.modificarVenta(oVentaE, SucAnterior);
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

        public void agregarLineaVenta(Entidades.LineaVenta oLineaE)
        {
            oVentaD.agregarLineaVenta(oLineaE);
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
    }
}
