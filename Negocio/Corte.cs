using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Negocio
{
    public class Corte
    {
        Datos.Corte oCorteD=new Datos.Corte();

        public void agregarCorte(Entidades.Corte oCorteE)
        {
            oCorteD.agregarCorte(oCorteE);
        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarCorte(txtBusqueda);

        }
        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarCorteSinMaestro(txtBusqueda);
        }

        public DataTable buscarCodigoCorte(int codigo)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarCodigoCorte(codigo);
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            oCorteD = new Datos.Corte();
            oCorteD.eliminarCorte(oCorteE);
        }

        public void modificarCorte(Entidades.Corte oCorteE)
        {
            oCorteD = new Datos.Corte();
            oCorteD.modificarCorte(oCorteE);
        }

        public DataTable obtenerCortes()
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte=oCorteD.obtenerCortes();
           
            return dtCorte;
        }

        

        public DataTable obtenerInfoCorte(int idCorte)
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte = oCorteD.obtenerInfoCorte(idCorte);

            return dtCorte;
        }

        public DataTable obtenerEmbutidos(string txtBusqueda)
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte = oCorteD.obtenerEmbutidos(txtBusqueda);

            return dtCorte;
        }

         public DataTable buscarEmbutido(string sucursal,string texto, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.buscarEmbutido(sucursal, texto,fechaDesde,fechaHasta);
         }

         public int agregarEmbutido(Entidades.Embutido oEmbutido)
         {
             oCorteD = new Datos.Corte();

             return oCorteD.agregarEmbutido(oEmbutido);
         
         }
         public void anularEmbutido(Entidades.Embutido oEmbutidoE)
         {
             oCorteD.anularEmbutido(oEmbutidoE);
         }

         public DataTable obtenerCortesPorEmbutidos(Entidades.Embutido oEmbutidoE)
         {
             
             return oCorteD.obtenerCortesPorEmbutidos(oEmbutidoE);
         }

         public void agregarCortePorEmbutido(Entidades.CortePorEmbutido oCortePorEmbutido)
         {
             oCorteD = new Datos.Corte();

             oCorteD.agregarCortePorEmbutido(oCortePorEmbutido);
         }

         public void actualizarStockEmbutido(DataRow cortePorEmbutido, Entidades.Embutido oEmbutidoE)
         {
             oCorteD = new Datos.Corte();

             oCorteD.actualizarStockEmbutido(cortePorEmbutido, oEmbutidoE);
         }

         #region Movimiento

         public int agregarMovimiento(Entidades.Movimiento oMovimientoE)
         {
             return oCorteD.agregarMovimiento(oMovimientoE);
         }

         public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
         {
             oCorteD.agregarCortePorMovimiento(cortePorMovimiento);
         }

         public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
         {
             oCorteD.modificarMovimiento(oMovimientoE);
         }

         public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
         {
             oCorteD = new Datos.Corte();

             return oCorteD.obtenerMovimientos(sucOrigen,sucDestino, fechaDesde,fechaHasta,texto);
         }

         public Entidades.Movimiento cargarMovimiento(int idMovimiento)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.cargarMovimiento(idMovimiento);
         }

         public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
         {
             oCorteD = new Datos.Corte();
             oCorteD.quitarCortesPorMovimiento(oMovimientoE);
         }
         public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.cargarCortesPorMovimiento(idMovimiento);
         }

         public int agregarActualizacionStock(DateTime fechaActualizacion, string observaciones)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.agregarActualizacionStock(fechaActualizacion, observaciones);
         
         }

         public void actualizarStockPorCorte(int idActualizacion, Entidades.StockCorteSucursal stockCorte)
         {
             oCorteD = new Datos.Corte();
             oCorteD.actualizarStockPorCorte(idActualizacion, stockCorte);
         }

         public void actualizacionStockTotal(int idActualizacion)
         {
             oCorteD = new Datos.Corte();
             oCorteD.actualizacionStockTotal(idActualizacion);
         }

         public void actualizacionStockTeoricoTotal(int idActualizacion)
         {
             oCorteD = new Datos.Corte();
             oCorteD.actualizacionStockTeoricoTotal(idActualizacion);
         }

         public void reiniciarStockReal(int idSucursal)
         {
             oCorteD=new Datos.Corte();
             oCorteD.reiniciarStockReal(idSucursal);
         }

         public void reiniciarStockTeorico(int idSucursal)
        {
            oCorteD = new Datos.Corte();
            oCorteD.reiniciarStockTeorico(idSucursal);
        }

         public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.reporteTeoricoReal(texto, idSucursal, fechaDesde, fechaHasta);

         }

         public DataTable CierreStock(int nroCierre, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.CierreStock(nroCierre, texto, idSucursal, fechaDesde, fechaHasta);
         }

         public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.StockIngresoEgreso(texto, idSucursal, fechaDesde, fechaHasta);
         }

         public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.TotalPorCortesVendidos(texto, idSucursal, fechaDesde, fechaHasta);
         }

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.imprimirTeoricoReal(dtTeoricoReal, texto, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.TotalKgsCortePorCompra(texto, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.TotalMovimientosPorCorte(texto, idSucursal, fechaDesde, fechaHasta);
        }
         #endregion
    }
}
