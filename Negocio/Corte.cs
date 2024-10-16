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

        public Entidades.Corte getCorteById(int id, bool cargarMaestro)
        {
            return oCorteD.getCorteById(id, cargarMaestro);
        }

        public void editPrecioCorte(Entidades.Corte oCorteE)
        {
            oCorteD.editPrecioCorte(oCorteE);
        }

        public void addOrEditCorte(Entidades.Corte oCorteE)
        {
            oCorteD.addOrEditCorte(oCorteE);
        }

        public DataTable buscarCorte(string txtBusqueda)
        {
            oCorteD = new Datos.Corte();
            DataTable dtCortes = oCorteD.buscarCorte(txtBusqueda);
            
            Datos.OtrasClases oOtrasClasesD = new Datos.OtrasClases();
            DataTable dtParametros = oOtrasClasesD.obtenerParametros();
            float porcAjEfectivo, porcAjDebito, porcAjCredito, porcAjCtaCte, porcAjQr, porcAjTranf;
            porcAjEfectivo=porcAjDebito=porcAjCredito=porcAjCtaCte=porcAjQr=porcAjTranf=0;
            for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
            {
                switch (dtParametros.Rows[fila]["nombre"].ToString())
                {
                    case "porcAjEfectivo":
                        porcAjEfectivo = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjDebito":
                        porcAjDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjCredito":
                        porcAjCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjCtaCte":
                        porcAjCtaCte = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjQr":
                        porcAjQr = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                    case "porcAjTranf":
                        porcAjTranf = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
                        break;
                }                            
            }

            for (int fila = 0; fila < dtCortes.Rows.Count; fila++)
            {
                dtCortes.Rows[fila]["efectivo"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjEfectivo).ToString("F2");
                dtCortes.Rows[fila]["debito"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjDebito).ToString("F2");
                dtCortes.Rows[fila]["credito"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjCredito).ToString("F2");
                //dtCortes.Rows[fila]["ctacte"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjCtaCte).ToString("F2");
                dtCortes.Rows[fila]["qr"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjQr).ToString("F2");
                dtCortes.Rows[fila]["transf"] = (float.Parse(dtCortes.Rows[fila]["precioKg"].ToString()) * porcAjTranf).ToString("F2");
            }

            return dtCortes;

        }
        public DataTable buscarCorteSinMaestro(string txtBusqueda)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarCorteSinMaestro(txtBusqueda);
        }

        public DataTable buscarCodigoCorte(long codigo)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarCodigoCorte(codigo);
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            oCorteD = new Datos.Corte();
            oCorteD.eliminarCorte(oCorteE);
        }

        public DataTable obtenerCortes()
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte=oCorteD.obtenerCortes();
           
            return dtCorte;
        }
        public DataTable cargarDtCortes()
        {
            oCorteD = new Datos.Corte();
            return oCorteD.cargarDtCortes();
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte = oCorteD.obtenerInfoCorte(idCorte);

            return dtCorte;
        }

        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            return oCorteD.findCorteById(idCorte, buscarMaestro);
        }
        public DataTable obtenerTiposProducto(bool mostrarTodos)
        {
            DataTable dtAlicuotasIva = new DataTable();
            oCorteD = new Datos.Corte();
            dtAlicuotasIva = oCorteD.obtenerTiposProducto(mostrarTodos);
            return dtAlicuotasIva;
        }
        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            DataTable dtAlicuotasIva = new DataTable();
            oCorteD = new Datos.Corte();
            dtAlicuotasIva = oCorteD.obtenerAlicuotasIva(mostrarTodos);

            return dtAlicuotasIva;
        }

        public Entidades.AlicuotaIva findAlicuotaIvaById(int idIva)
        {
            return oCorteD.findAlicuotaIvaById(idIva);
        }

        public Entidades.Embutido findEmbutidoById(int idEmbutido)
        {
            return oCorteD.findEmbutidoById(idEmbutido);
        }

        public DataTable obtenerEmbutidos(string txtBusqueda)
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte = oCorteD.obtenerEmbutidos(txtBusqueda);

            return dtCorte;
        }

        public DataTable getListaElegirEmbutido()
        {
            DataTable dtCorte = new DataTable();
            oCorteD = new Datos.Corte();
            dtCorte = oCorteD.getListaElegirEmbutido();

            return dtCorte;
        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.buscarEmbutido(idSucursal, texto, fechaDesde, fechaHasta);
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.obtenerLineasEmb(idSucursal, texto, fechaDesde, fechaHasta);
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

        #region Formulas
        public DataTable buscarFormula(string texto)
        {
            return oCorteD.buscarFormula(texto);
        }
        public Entidades.Formula findFormulaByID(int idFormula)
        {
            return oCorteD.findFormulaByID(idFormula);
        }
        public bool existeFormula(int idEmbutido)
        { 
            return (oCorteD.existeFormula(idEmbutido) > 0);
        }
            public int addOrEditFormula(Entidades.Formula oFormula, List<Entidades.CortePorFormula> listaCortesPorFormula)
        {
            return oCorteD.addOrEditFormula(oFormula, listaCortesPorFormula);
        }
        public void eliminarFormula(int idFormula)
        {
            oCorteD.eliminarFormula(idFormula);
        }

        public DataTable getFormulaEmbutido(int idEmbutido)
        {
            return oCorteD.getFormulaEmbutido(idEmbutido);
        }
            #endregion

            #region Movimiento

            public int addOrEditMovimiento(Entidades.Movimiento oMovimientoE)
         {
             return oCorteD.addOrEditMovimiento(oMovimientoE);
         }

         public void agregarCortePorMovimiento(Entidades.CortePorMovimiento cortePorMovimiento)
         {
             oCorteD.agregarCortePorMovimiento(cortePorMovimiento);
         }

         //public void modificarMovimiento(Entidades.Movimiento oMovimientoE)
         //{
         //    oCorteD.modificarMovimiento(oMovimientoE);
         //}

         public void eliminarMovimiento(int idMovimiento, Entidades.Usuario oUsuario)
         {
             oCorteD.eliminarMovimiento(idMovimiento, oUsuario);
         }

         public DataTable obtenerMovimientos(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.obtenerMovimientos(sucOrigen,sucDestino, fechaDesde,fechaHasta,texto);
         }

         public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.obtenerLineasMov(sucOrigen, sucDestino, fechaDesde, fechaHasta, texto);
         }

         public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.cargarMovimiento(idMovimiento, acumulado);
         }

         //public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
         //{
         //    oCorteD = new Datos.Corte();
         //    oCorteD.quitarCortesPorMovimiento(oMovimientoE);
         //}

         public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.cargarCortesPorMovimiento(idMovimiento, acumulado);
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

         public DataTable CierreStock(int nroCierre, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.CierreStock(nroCierre, texto, idSucursal, fechaDesde, fechaHasta, conexionSucursal);
         }

         public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.acum_Ventas(texto, idSucursal, fechaDesde, fechaHasta);
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

        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.Balance(texto, idSucursal, fechaDesde, fechaHasta);
        }
            #endregion
        }
}
