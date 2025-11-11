using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;

namespace Negocio
{
    public class Corte
    {
        Datos.Corte oCorteD=new Datos.Corte();

        //Mantuve el metodo con este nombre getCorteById para no modificar toda la capa presentacion
        public Entidades.Corte getCorteById(int id, bool cargarMaestro)
        {
            return findCorteById(id, cargarMaestro);
        }
        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            return oCorteD.findCorteById(idCorte, buscarMaestro);
        }
        public List<Entidades.Corte> findAllCortes(bool buscarMaestro)
        {
            return oCorteD.findAllCortes(buscarMaestro);
        }

        public void editPrecioCorte(Entidades.Corte oCorteE)
        {
            oCorteD.editPrecioCorte(oCorteE);
        }

        public void addOrEditCorte(Entidades.Corte oCorteE)
        {
            ///CARGA EXHAUSTIVA
            ///
            //oCorteE.codigo = 12500500;
            //for (int i = 0; i < 1000; i++)
            //{
            //    oCorteE.codigo++;
            //    oCorteE.corte = oCorteE.corte + i.ToString() + " Prov Marca 01";
            //    //oCorteE.tipo = ;
            //    oCorteE.Marca.idPersona = 316;

            //    oCorteD.addOrEditCorte(oCorteE);
            //}
            ///FIN CARGA EXHAUSITIVA
            ///

            if (oCorteE.Presentacion)
            {
                float[] valoresPresentacion = oCorteE.SetearValoresPresentacion(oCorteE.porcentaje);
                oCorteE.porcentaje = valoresPresentacion[0];
                oCorteE.porcentajeHueso = valoresPresentacion[1];
            }

            oCorteD.addOrEditCorte(oCorteE);
        }

        public DataTable lista_precios()
        {
            DataTable dtCortes = buscarCorte("");

            string[] columnasPermitidas = { "codigo", "corte", "precioKg", "efectivo", "debito", "credito", "Qr", "Transf" };

            // Crear una lista temporal para almacenar las columnas a eliminar
            List<DataColumn> columnasAEliminar = new List<DataColumn>();

            // Recorrer todas las columnas en el DataTable
            foreach (DataColumn columna in dtCortes.Columns)
            {
                // Si el nombre de la columna no está en el array de columnas permitidas, agregarla a la lista de eliminación
                if (!Array.Exists(columnasPermitidas, columnaPermitida => columnaPermitida == columna.ColumnName))
                {
                    columnasAEliminar.Add(columna);
                }
            }

            // Eliminar todas las columnas que no están en la lista de columnas permitidas
            foreach (DataColumn columna in columnasAEliminar)
            {
                dtCortes.Columns.Remove(columna);
            }
            return dtCortes;
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


        public DataTable obtenerCorteProveedor(int idCorte)
        {
            return oCorteD.obtenerCorteProveedor(idCorte);
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
        /// <summary>
        /// Busca formula segun el ID por parámetro
        /// </summary>
        /// <param name="idFormula"></param>
        /// <param name="idEmbutido"></param>
        /// <returns></returns>
        public Entidades.Formula findFormulaByID(int idFormula, int idEmbutido)
        {
            return oCorteD.findFormulaByID(idFormula, idEmbutido);
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

         public DataTable CierreStock(int nroCierre, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal, string tipo, int idProveedor, int idMarca)
         {
             oCorteD = new Datos.Corte();
             DataTable dtGrillaReporte = oCorteD.CierreStock(nroCierre, texto, idSucursal, fechaDesde, fechaHasta, conexionSucursal, tipo, idProveedor, idMarca);

            foreach (DataRow fila in dtGrillaReporte.Rows)
            {
                decimal TotINGR = 0, TotEGR = 0;
                if (fila["Stock.Ini"].ToString() == null || fila["Stock.Ini"].ToString() == "")
                {
                    fila["Stock.Ini"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Stock.Ini"]);
                }

                if (fila["Compras"].ToString() == null || fila["Compras"].ToString() == "")
                {
                    fila["Compras"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Compras"]);
                }

                if (fila["Ingr.Elab"].ToString() == null || fila["Ingr.Elab"].ToString() == "")
                {
                    fila["Ingr.Elab"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Ingr.Elab"]);
                }

                if (fila["Ingr.Stock"].ToString() == null || fila["Ingr.Stock"].ToString() == "")
                {
                    fila["Ingr.Stock"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Ingr.Stock"]);
                }

                if (fila["Ingr. Mov"].ToString() == null || fila["Ingr. Mov"].ToString() == "")
                {
                    fila["Ingr. Mov"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Ingr. Mov"]);
                }

                if (fila["Ajus.Stock"].ToString() == null || fila["Ajus.Stock"].ToString() == "")
                {
                    fila["Ajus.Stock"] = 0;
                }
                else
                {
                    TotINGR += Convert.ToDecimal(fila["Ajus.Stock"]);
                }

                if (fila["Egr.Stock"].ToString() == null || fila["Egr.Stock"].ToString() == "")
                {
                    fila["Egr.Stock"] = 0;
                }
                else
                {
                    TotEGR += Convert.ToDecimal(fila["Egr.Stock"]);
                }

                if (fila["Egr.Mov"].ToString() == null || fila["Egr.Mov"].ToString() == "")
                {
                    fila["Egr.Mov"] = 0;
                }
                else
                {
                    TotEGR += Convert.ToDecimal(fila["Egr.Mov"]);
                }

                if (fila["Egr.Elab"].ToString() == null || fila["Egr.Elab"].ToString() == "")
                {
                    fila["Egr.Elab"] = 0;
                }
                else
                {
                    TotEGR += Convert.ToDecimal(fila["Egr.Elab"]);
                }

                if (fila["Ventas"].ToString() == null || fila["Ventas"].ToString() == "")
                {
                    fila["Ventas"] = 0;
                }
                else
                {
                    TotEGR += Convert.ToDecimal(fila["Ventas"]);
                }

                if (fila["Stock.Cierre"].ToString() == null || fila["Stock.Cierre"].ToString() == "")
                {
                    fila["Stock.Cierre"] = 0;
                }

                fila["Tot.INGR"] = TotINGR;
                fila["Tot.EGR"] = TotEGR;
                fila["DIF"] = TotINGR - TotEGR;

                fila["Faltante"] = Convert.ToDecimal(fila["DIF"]) - Convert.ToDecimal(fila["Stock.Cierre"]);

                float stockKg = float.Parse(fila["Faltante"].ToString());
                //string stockUn = Math.Round(Convert.ToDecimal(stockKg / float.Parse(fila["promedio"].ToString()))).ToString() + " u";
                string stock = Convert.ToDecimal(fila["promedio"]) == 0 ? stockKg.ToString("F2") :
                    Math.Round(Convert.ToDecimal(stockKg / float.Parse(fila["promedio"].ToString()))).ToString() + " u";//stockUn;// stockUn.ToString("F1") + " u";
                fila["Stock.Un"] = stock;

                //Si Punto Stock mayor a cero significa que se necesita saber el faltante del producto
                //si Stock es menor a cero o Pto Stock es mayor 
                fila["Falta"] = Convert.ToDecimal(fila["Pto.Stock"]) > 0 && ((Convert.ToDecimal(fila["DIF"]) < 0) || (Convert.ToDecimal(fila["Pto.Stock"]) > (Convert.ToDecimal(fila["DIF"])))) ? "X" : "";

            }
            return dtGrillaReporte;
        }

         public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo , int idProveedor, int idMarca)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.acum_Ventas(texto, idSucursal, fechaDesde, fechaHasta, tipo, idProveedor, idMarca);
         }

         public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.StockIngresoEgreso(texto, idSucursal, fechaDesde, fechaHasta);
         }

         public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
         {
             oCorteD = new Datos.Corte();
             return oCorteD.TotalPorCortesVendidos(texto, idSucursal, fechaDesde, fechaHasta, tipo, idProveedor, idMarca);
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

        #region Tipos Producto/Corte
        public DataTable obtenerTiposProductoGrilla(string buscarText)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.obtenerTiposProductoGrilla(buscarText);
        }

        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.addOrEditTipoProducto(tiposProducto,orden, esInsert, tipoToUpdate);
        }

        public string eliminarTipoProducto(string tiposProducto)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.eliminarTipoProducto(tiposProducto);
        }


        /// <summary>
        /// Sugiere el menor codigo libre segun el tipo de producto
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public long sugerirCodigo(string tipo)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.sugerirCodigo(tipo);
        }
        #endregion

        public int obtenerNivelCorte(int idCorteMaestro)
        {
            oCorteD = new Datos.Corte();
            return oCorteD.obtenerNivelCorte(idCorteMaestro);
        }
    }
}
