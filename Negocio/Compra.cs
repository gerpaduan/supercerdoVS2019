using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Compra
    {
        Datos.Compra oCompraD = new Datos.Compra();
        
        public int agregarCompra(Entidades.Compra oCompraE)
        {
           return oCompraD.agregarCompra(oCompraE);
            
        }

        public void modificarCompra(Entidades.Compra oCompraE)
        {
            oCompraD.ModificarCompra(oCompraE);
        }

        public void modificarPrecioMedia(int idCompra, float precioKg)
        {
            oCompraD.modificarPrecioMedia(idCompra, precioKg);
        }
        
        public void agregarMedias(Entidades.MediaRes oMediaResE)
        {
            oCompraD.agregarMediaRes(oMediaResE);
            //oCompraD.actualizarStockCortesPrimarios(oMediaResE);

        }

        public void agregarCortePorCompra(Entidades.CortePorCompra oCortePorCompraE)
        {
            oCompraD.agregarCortePorCompra(oCortePorCompraE);
        }

        public int obtenerUltimaCompra()
        {
            return oCompraD.obtenerIdUltimaCompra();
        }

        public DataTable obtenerCompras(string tipoCompra, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCompraD.obtenerCompras(tipoCompra, texto,fechaDesde,fechaHasta);
        }

        public DataTable obtenerCortesPorCompra(int idCompra)
        {
            return oCompraD.obtenerCortesPorCompra(idCompra);
        }

        public DataTable obtenerMediasPorCompra(int idCompra)
        {
            return oCompraD.obtenerMediasPorCompra(idCompra);
        }

        public void anularCompra(int idCompra)
        {
            oCompraD.anularCompra(idCompra);
        }

        public void modificarMediaPorCompra(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.modificarMediaPorCompra(oMediaResE, idCompra);
            oCompraD.actualizarStockCortesPrimarios(oMediaResE);
        }

        public void modificarCortePorCompra(Entidades.CortePorCompra oCortePorCompraE, int idCompra)
        {
            oCompraD.modificarCortePorCompra(oCortePorCompraE,idCompra);
        }

        public void quitarStockMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.quitarStockMedia(oMediaResE, idCompra);
        }

        public void quitarStockTeoricoMedia(Entidades.MediaRes oMediaResE, int idCompra)
        {
            oCompraD.quitarStockTeoricoMedia(oMediaResE, idCompra);
        }

        public void quitarStockCorte(Entidades.CortePorCompra oCorteE, int idCompra)
        {
            oCompraD.quitarStockCorte(oCorteE, idCompra);
        }

        public DataTable porcentajeCortesPorCompra(int idCompra)
        {
            return oCompraD.porcentajeCortesPorCompra(idCompra);

        }

        #region Pagos

        public void agregarPago(Entidades.Pagos oPagoE)
        {
            oCompraD.agregarPago(oPagoE);
        }

        public void modificarPago(Entidades.Pagos oPagoE)
        {
            oCompraD.modificarPago(oPagoE);
        }

        public void eliminarPago(Entidades.Pagos oPagoE)
        {
            oCompraD.eliminarPago(oPagoE);
        }

        public DataTable obtenerPagos(string tipoTramite, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCompraD.obtenerPagos(tipoTramite, texto, fechaDesde, fechaHasta);
        }

        public Entidades.Pagos buscarPago(Entidades.Pagos oPagoE)
        {
            return oCompraD.buscarPago(oPagoE);
        }

        public void backup(string destino)
        {
            oCompraD.backup(destino);
        }

        public void restaurarBD(string bdAuxiliar, string rutaOrigen)
        {
            oCompraD.restaurarBD(bdAuxiliar, rutaOrigen);
        }

        #endregion


    }
}
