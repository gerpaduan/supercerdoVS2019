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
            switch (oVentaE.FormaPago.ToString())
            {
                case "Efectivo":
                    oVentaE.ComisionTarjeta = 0;
                    break;
                case "Debito":
                    oVentaE.ComisionTarjeta = Entidades.Parametros.comisionDebito;
                    break;
                case "Credito":
                    oVentaE.ComisionTarjeta = Entidades.Parametros.comisionCredito;
                    break;
                default:
                    oVentaE.ComisionTarjeta = 0;
                    break;
            }
            oVentaE.IdVenta = oVentaD.agregarVenta(oVentaE);
            return oVentaE.IdVenta;
        }

        public void modificarVenta(Entidades.Venta oVentaE, int SucAnterior, bool eliminarLineas)
        {
            switch (oVentaE.FormaPago.ToString())
            {
                case "Efectivo":
                    oVentaE.ComisionTarjeta = 0;
                    break;
                case "Debito":
                    oVentaE.ComisionTarjeta = Entidades.Parametros.comisionDebito;
                    break;
                case "Credito":
                    oVentaE.ComisionTarjeta = Entidades.Parametros.comisionCredito;
                    break;
                default:
                    oVentaE.ComisionTarjeta = 0;
                    break;
            }
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

        public float getTotalKgsVenta(int idVenta)
        {
            return oVentaD.getTotalKgsVenta(idVenta);
        }

        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados)
        {
            return oVentaD.obtenerVentas(idSucursal, idCliente, idVendedor, fechaDesde, fechaHasta, texto, soloAnulados);
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
            oLineaE.AjustePrecio = oLineaE.PrecioKg - oLineaE.Corte.precioKgReferencia;
            return oVentaD.agregarLineaVenta(oLineaE);
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

        public void egresoCajaPagoTarjeta(int idVenta, Entidades.Usuario oUsuario)
        {
            Entidades.Venta oVentaConEgresoCaja = getVentaById(idVenta);

            bool esEfectivo = oVentaConEgresoCaja.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString());                   
            //se genera el egreso de caja si paga con tarjeta
            if (!esEfectivo)
            {
                float totalS = 0, totalKgs = 0;// getTotalVenta(oVentaConEgresoCaja.IdVenta);
                foreach (Entidades.LineaVenta linea in oVentaConEgresoCaja.LineasVenta)
                {
                    totalKgs += linea.CantKg;
                    totalS += (linea.CantKg * linea.PrecioKg); 
                }

                Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

                oEgresoCajaE.Fecha = oVentaConEgresoCaja.FechaVenta;
                oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idPagoTarjeta;
                oEgresoCajaE.Descripcion = "Venta " + oVentaConEgresoCaja.FormaPago.ToString() + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                oEgresoCajaE.Monto = totalS;// oVentaN.getTotalVenta(oVentaConEgresoCaja.IdVenta);
                oEgresoCajaE.Detalle = " | Kgs: " + totalKgs.ToString("N3") +
                    " | Precio: " + (totalS / totalKgs).ToString("N3") +
                    " | TOT: " + totalS.ToString("N3");
                oEgresoCajaE.Sucursal = oVentaConEgresoCaja.Sucursal;
                oEgresoCajaE.IdCompra = 0;
                oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Ventas.ToString();
                oEgresoCajaE.IdTabla = oVentaConEgresoCaja.IdVenta;
                oEgresoCajaE.CreadoPor = oVentaConEgresoCaja.Vendedor.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? (oUsuario != null ? oUsuario.Id : -1) : -1;

                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
            }
        }
    }
}
