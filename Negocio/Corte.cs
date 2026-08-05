using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Negocio
{
    public class Corte
    {
        private readonly Datos.Corte oCorteD;
        private readonly Datos.CortePuntoStockSucursal oCortePuntoStockSucursalD;

        IEmpresaContext _empresa;private readonly IParametrosContext _param;
        public Corte(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oCorteD = new Datos.Corte(empresa, param);
            oCortePuntoStockSucursalD = new Datos.CortePuntoStockSucursal(empresa, param);
        }

        //Mantuve el metodo con este nombre getCorteById para no modificar toda la capa presentacion
        public Entidades.Corte getCorteById(int id, bool cargarMaestro)
        {
            return findCorteById(id, cargarMaestro);
        }
        public Entidades.Corte findCorteById(int idCorte, bool buscarMaestro)
        {
            return oCorteD.findCorteById(idCorte, buscarMaestro);
        }
        public Entidades.Corte findCorteByCodigo(Int64 codigo, bool buscarMaestro)
        {
            return oCorteD.findCorteByCodigo(codigo, buscarMaestro);
        }

        public List<Entidades.Corte> ObtenerCortesPorEmpresa(int idEmpresa, bool buscarMaestro)
        {
            return oCorteD.ObtenerCortesPorEmpresa(idEmpresa, buscarMaestro);
        }

        public Entidades.Corte findCorteByCodigoEmpresa(long codigo, int idEmpresa, bool buscarMaestro)
        {
            return oCorteD.findCorteByCodigoEmpresa(codigo, idEmpresa, buscarMaestro);
        }

        public List<Entidades.Corte> ObtenerCortesListado(int idEmpresa, int idSucursal)
        {
            List<Entidades.Corte> listaCortes = idEmpresa > 0
                ? oCorteD.ObtenerCortesPorEmpresaListado(idEmpresa)
                : oCorteD.findAllCortesListado();

            if (idSucursal > 0)
            {
                DateTime fechaUltimoCierreStock = oCorteD.fechaUltimoCierreStock_Sucursal(idSucursal);
                DataTable dtCortesStock = CierreStock(1, "", idSucursal, fechaUltimoCierreStock, DateTime.Now, null, "", 0, 0);
                var dictStocks = dtCortesStock.AsEnumerable()
                    .ToDictionary(
                        row => row.Field<int>("idCorte"),
                        row => row["DIF"] == DBNull.Value ? null : row["DIF"].ToString()
                    );

                var dictStockUn = dtCortesStock.AsEnumerable()
                    .ToDictionary(
                        row => row.Field<int>("idCorte"),
                        row => row["Stock.Un"] == DBNull.Value ? null : row["Stock.Un"].ToString()
                    );

                foreach (var corte in listaCortes)
                {
                    if (dictStocks.TryGetValue(corte.idCorte, out var stock))
                    {
                        if (double.TryParse(stock, out double stockNum))
                        {
                            corte.Stock_EnString = corte.Pesable ? stockNum.ToString("N3") : stockNum.ToString("N0");
                            if (dictStockUn.TryGetValue(corte.idCorte, out string stockUn))
                                corte.StockUnidades = stockUn.Contains("u") ? " (" + stockUn + ")" : "";
                        }
                    }
                    else
                    {
                        corte.Stock_EnString = "-";
                        corte.StockUnidades = "";
                    }
                }
            }

            return listaCortes;
        }

        public void AsegurarTablaImportacionCatalogoGlobal()
        {
            oCorteD.AsegurarTablaImportacionCatalogoGlobal();
        }

        public List<Entidades.CatalogoGlobalImportacionProducto> ObtenerImportacionesCatalogoGlobal(IEnumerable<int> idsProductosGlobales = null)
        {
            return oCorteD.ObtenerImportacionesCatalogoGlobal(idsProductosGlobales);
        }

        public void GuardarImportacionCatalogoGlobal(int idProductoGlobal, int idProductoEmpresa, int? idUsuarioAlta)
        {
            oCorteD.GuardarImportacionCatalogoGlobal(idProductoGlobal, idProductoEmpresa, idUsuarioAlta);
        }

        /// <summary>
        /// Listado Productos desde WEB
        /// </summary>
        /// <param name="buscarMaestro"></param>
        /// <param name="idSucursal"></param>
        /// <returns></returns>
        public List<Entidades.Corte> findAllCortes(bool buscarMaestro, int idSucursal)
        {
            List<Entidades.Corte> listaCortes = oCorteD.findAllCortes(buscarMaestro);
            if (idSucursal > 0)
            {
                DateTime fechaUltimoCierreStock = oCorteD.fechaUltimoCierreStock_Sucursal(idSucursal);
                DataTable dtCortesStock = CierreStock(1, "", idSucursal, fechaUltimoCierreStock, DateTime.Now, null, "", 0, 0);
                // Crear diccionario: id -> stock
                var dictStocks = dtCortesStock.AsEnumerable()
                    .ToDictionary(
                        row => row.Field<int>("idCorte"),
                        row => row["DIF"] == DBNull.Value ? null : row["DIF"].ToString()
                    );

                // id -> Stock.Un
                var dictStockUn = dtCortesStock.AsEnumerable()
                    .ToDictionary(
                        row => row.Field<int>("idCorte"),
                        row => row["Stock.Un"] == DBNull.Value ? null : row["Stock.Un"].ToString()
                    );

                // Actualizar los cortes
                foreach (var corte in listaCortes)
                {
                    if (dictStocks.TryGetValue(corte.idCorte, out var stock))
                    {
                        if (double.TryParse(stock, out double stockNum))
                        {
                            corte.Stock_EnString = corte.Pesable
                                ? stockNum.ToString("N3")  // 3 decimales
                                : stockNum.ToString("N0"); // entero

                            if (dictStockUn.TryGetValue(corte.idCorte, out string stockUn))
                            {
                                corte.StockUnidades = stockUn.Contains("u") ? " (" + stockUn + ")" : "";
                            }
                        }
                    }
                    else
                    {
                        corte.Stock_EnString = "-"; // si no está en el DataTable
                        corte.StockUnidades = "";
                    }
                }
            }
            return listaCortes;
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

        public int InsertarCorteEnEmpresa(Entidades.Corte oCorteE)
        {
            return oCorteD.InsertarCorteEnEmpresa(oCorteE);
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
            DataTable dtCortes = oCorteD.buscarCorte(txtBusqueda);
            
            //Datos.OtrasClases oOtrasClasesD = new Datos.OtrasClases(_empresa);
            //DataTable dtParametros = oOtrasClasesD.obtenerParametros();
            float porcAjEfectivo, porcAjDebito, porcAjCredito, porcAjCtaCte, porcAjQr, porcAjTranf;
            porcAjEfectivo=porcAjDebito=porcAjCredito=porcAjCtaCte=porcAjQr=porcAjTranf=0;

            porcAjEfectivo = _param.GetFloat(Entidades.ParamKeys.PorcAjEfectivo, 0f);
            porcAjDebito = _param.GetFloat(Entidades.ParamKeys.PorcAjDebito, 0f);
            porcAjCredito = _param.GetFloat(Entidades.ParamKeys.PorcAjCredito, 0f);
            porcAjCtaCte = 1;//no se obtiene el valor desde parametros
            porcAjQr = _param.GetFloat(Entidades.ParamKeys.PorcAjQr, 0f);
            porcAjTranf = _param.GetFloat(Entidades.ParamKeys.PorcAjTranf, 0f);

            //for (int fila = 0; fila < dtParametros.Rows.Count; fila++)
            //{
            //    switch (dtParametros.Rows[fila]["nombre"].ToString())
            //    {
            //        case "porcAjEfectivo":
            //            porcAjEfectivo = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //        case "porcAjDebito":
            //            porcAjDebito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //        case "porcAjCredito":
            //            porcAjCredito = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //        case "porcAjCtaCte":
            //            porcAjCtaCte = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //        case "porcAjQr":
            //            porcAjQr = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //        case "porcAjTranf":
            //            porcAjTranf = float.Parse(dtParametros.Rows[fila]["valor"].ToString());
            //            break;
            //    }                            
            //}

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
            return oCorteD.buscarCorteSinMaestro(txtBusqueda);
        }

        public DataTable buscarCodigoCorte(long codigo)
        {
            
            return oCorteD.buscarCodigoCorte(codigo);
        }

        public void eliminarCorte(Entidades.Corte oCorteE)
        {
            
            oCorteD.eliminarCorte(oCorteE);
        }

        public DataTable obtenerCortes()
        {
            DataTable dtCorte = new DataTable();
            
            dtCorte=oCorteD.obtenerCortes();
           
            return dtCorte;
        }
        public DataTable cargarDtCortes()
        {
            
            return oCorteD.cargarDtCortes();
        }

        public DataTable obtenerInfoCorte(int idCorte)
        {
            DataTable dtCorte = new DataTable();
            
            dtCorte = oCorteD.obtenerInfoCorte(idCorte);

            return dtCorte;
        }


        public DataTable obtenerCorteProveedor(int idCorte)
        {
            return oCorteD.obtenerCorteProveedor(idCorte);
        }
        public DataTable obtenerCortesPorProveedor(int idProveedor)
        {
            return oCorteD.obtenerCortesPorProveedor(idProveedor);
        }
        public DataTable obtenerTiposProducto(bool mostrarTodos)
        {
            DataTable dtAlicuotasIva = new DataTable();
            
            dtAlicuotasIva = oCorteD.obtenerTiposProducto(mostrarTodos);
            return dtAlicuotasIva;
        }
        public DataTable obtenerAlicuotasIva(bool mostrarTodos)
        {
            DataTable dtAlicuotasIva = new DataTable();
            
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
            
            dtCorte = oCorteD.obtenerEmbutidos(txtBusqueda);

            return dtCorte;
        }

        public DataTable getListaElegirEmbutido()
        {
            DataTable dtCorte = new DataTable();
            
            dtCorte = oCorteD.getListaElegirEmbutido();

            return dtCorte;
        }

        public DataTable buscarEmbutido(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.buscarEmbutido(idSucursal, texto, fechaDesde, fechaHasta);
        }

        public DataTable obtenerUltimosElaboradosDashboard(int cantidad, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            return oCorteD.obtenerUltimosElaboradosDashboard(cantidad, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable obtenerLineasEmb(int idSucursal, string texto, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.obtenerLineasEmb(idSucursal, texto, fechaDesde, fechaHasta);
        }

        public HashSet<int> ObtenerIdsEmbutidosIngresoRapido(IEnumerable<int> idsEmbutidos)
        {
            return oCorteD.ObtenerIdsEmbutidosIngresoRapido(idsEmbutidos);
        }

         public int agregarEmbutido(Entidades.Embutido oEmbutido)
         {
             
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

        public bool FormulaUsaUnidades(Entidades.Corte elaborado)
        {
            return elaborado != null && !elaborado.Pesable;
        }

        public float ConvertirFormulaParaVisualizacion(Entidades.Corte elaborado, float valorGuardado)
        {
            return FormulaUsaUnidades(elaborado) ? (valorGuardado / 100f) : valorGuardado;
        }

        public float ConvertirFormulaParaPersistencia(Entidades.Corte elaborado, float valorVisual)
        {
            return FormulaUsaUnidades(elaborado) ? (valorVisual * 100f) : valorVisual;
        }

        public Entidades.Corte ObtenerProductoGenerico()
        {
            long codigoGenerico = _param != null ? _param.GetLong(Entidades.ParamKeys.CodProdGenerico, 0L) : 0L;
            return codigoGenerico > 0 ? findCorteByCodigo(codigoGenerico, false) : null;
        }

        public List<Entidades.CortePorFormula> NormalizarFormulaElaborado(Entidades.Corte elaborado, List<Entidades.CortePorFormula> lineas)
        {
            if (elaborado == null) throw new ArgumentNullException(nameof(elaborado));
            if (lineas == null) lineas = new List<Entidades.CortePorFormula>();

            var resultado = new List<Entidades.CortePorFormula>();
            var productoGenerico = ObtenerProductoGenerico();

            foreach (var item in lineas)
            {
                if (item == null || item.CorteEnFormula == null || item.CorteEnFormula.IdCorte <= 0)
                    continue;

                bool esAjuste = productoGenerico != null && item.CorteEnFormula.IdCorte == productoGenerico.IdCorte;
                if (esAjuste)
                    continue;

                resultado.Add(new Entidades.CortePorFormula
                {
                    Formula = item.Formula,
                    CorteEnFormula = item.CorteEnFormula,
                    AgregarAuto = item.AgregarAuto,
                    Porcentaje = ConvertirFormulaParaPersistencia(elaborado, item.Porcentaje)
                });
            }

            if (elaborado.IngresoRapidoEmbutido)
            {
                if (productoGenerico == null || productoGenerico.IdCorte <= 0)
                    throw new InvalidOperationException("No existe el código genérico configurado para realizar el ajuste de fórmula.");

                float total = resultado.Sum(x => x.Porcentaje);
                float ajuste = 100f - total;

                resultado.Insert(0, new Entidades.CortePorFormula
                {
                    Formula = lineas.FirstOrDefault(x => x != null)?.Formula,
                    CorteEnFormula = productoGenerico,
                    AgregarAuto = true,
                    Porcentaje = ajuste
                });
            }

            return resultado;
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
             
             return oCorteD.obtenerMovimientos(sucOrigen,sucDestino, fechaDesde,fechaHasta,texto);
         }

         public DataTable obtenerUltimosMovimientosDashboard(int cantidad)
         {
             return oCorteD.obtenerUltimosMovimientosDashboard(cantidad);
         }

         public DataTable obtenerLineasMov(string sucOrigen, string sucDestino, DateTime fechaDesde, DateTime fechaHasta, string texto)
         {
             
             return oCorteD.obtenerLineasMov(sucOrigen, sucDestino, fechaDesde, fechaHasta, texto);
         }

         public Entidades.Movimiento cargarMovimiento(int idMovimiento, bool acumulado)
         {
             
             return oCorteD.cargarMovimiento(idMovimiento, acumulado);
         }

         //public void quitarCortesPorMovimiento(Entidades.Movimiento oMovimientoE)
         //{
         //    
         //    oCorteD.quitarCortesPorMovimiento(oMovimientoE);
         //}

         public List<Entidades.CortePorMovimiento> cargarCortesPorMovimiento(int idMovimiento, bool acumulado)
         {
             
             return oCorteD.cargarCortesPorMovimiento(idMovimiento, acumulado);
         }

         public Dictionary<int, Tuple<decimal, decimal>> ObtenerTotalesPorMovimiento(IEnumerable<int> idsMovimiento)
         {
             return oCorteD.ObtenerTotalesPorMovimiento(idsMovimiento);
         }

         public void reiniciarStockReal(int idSucursal)
         {
             oCorteD.reiniciarStockReal(idSucursal);
         }

         public void reiniciarStockTeorico(int idSucursal)
        {
            
            oCorteD.reiniciarStockTeorico(idSucursal);
        }

         public DataTable reporteTeoricoReal(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             
             return oCorteD.reporteTeoricoReal(texto, idSucursal, fechaDesde, fechaHasta);

         }


         public DataTable CierreStock(int nroCierre, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string conexionSucursal, string tipo, int idProveedor, int idMarca)
         {
             
             DataTable dtGrillaReporte = oCorteD.CierreStock(nroCierre, texto, idSucursal, fechaDesde, fechaHasta, conexionSucursal, tipo, idProveedor, idMarca);

            // El SP a_CierreStock devuelve Pto.Stock leyendo Corte.puntoStock (valor global,
            // no por sucursal). Se sobrescribe aca con el valor real de la tabla intermedia
            // Producto x Sucursal, sin tocar el SP (ver docs/DECISIONS.md: su script versionado
            // esta desactualizado respecto a la firma real y no se puede editar con confianza).
            if (idSucursal > 0)
            {
                var puntosStockSucursal = oCortePuntoStockSucursalD.FindPorSucursal(idSucursal);
                foreach (DataRow fila in dtGrillaReporte.Rows)
                {
                    int idCorteFila = fila.Field<int>("idCorte");
                    fila["Pto.Stock"] = puntosStockSucursal.TryGetValue(idCorteFila, out int puntoStockSucursal)
                        ? (object)puntoStockSucursal
                        : 0;
                }
            }

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

        // Reemplazo de CierreStock exclusivo para Web (ver Datos/Corte.cs:CierreStockWeb y
        // docs/DECISIONS.md). A diferencia del viejo, Tot.INGR/Tot.EGR/DIF/Faltante/Stock.Un ya
        // vienen calculados desde el SP -- solo hace falta sobrescribir Pto.Stock con el valor
        // real por sucursal (mismo motivo que CierreStock) y recalcular Falta, que depende de
        // Pto.Stock. idSucursal=0 trae varias sucursales en una sola llamada, por eso el override
        // se agrupa por la columna idSucursal del resultado en vez de un solo idSucursal fijo.
        public DataTable CierreStockWeb(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
        {
            DataTable dtGrillaReporte = oCorteD.CierreStockWeb(texto, _empresa.IdEmpresa, idSucursal, fechaDesde, fechaHasta, tipo, idProveedor, idMarca);

            var idsSucursalesEnResultado = dtGrillaReporte.Rows
                .Cast<DataRow>()
                .Select(r => r.Field<int>("idSucursal"))
                .Distinct()
                .ToList();

            var puntosStockPorSucursal = new Dictionary<(int idCorte, int idSucursal), int>();
            foreach (var idSuc in idsSucursalesEnResultado)
            {
                foreach (var par in oCortePuntoStockSucursalD.FindPorSucursal(idSuc))
                    puntosStockPorSucursal[(par.Key, idSuc)] = par.Value;
            }

            foreach (DataRow fila in dtGrillaReporte.Rows)
            {
                int idCorteFila = fila.Field<int>("idCorte");
                int idSucursalFila = fila.Field<int>("idSucursal");
                int puntoStockReal = puntosStockPorSucursal.TryGetValue((idCorteFila, idSucursalFila), out int p) ? p : 0;

                fila["Pto.Stock"] = (decimal)puntoStockReal;

                decimal dif = Convert.ToDecimal(fila["DIF"]);
                fila["Falta"] = puntoStockReal > 0 && (dif < 0 || puntoStockReal > dif) ? "X" : "";
            }

            return dtGrillaReporte;
        }

         public DataTable acum_Ventas(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo , int idProveedor, int idMarca)
         {
             
             return oCorteD.acum_Ventas(texto, idSucursal, fechaDesde, fechaHasta, tipo, idProveedor, idMarca);
         }

         public DataTable StockIngresoEgreso(string texto,int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
         {
             
             return oCorteD.StockIngresoEgreso(texto, idSucursal, fechaDesde, fechaHasta);
         }

         public DataTable TotalPorCortesVendidos(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idProveedor, int idMarca)
         {
             
             return oCorteD.TotalPorCortesVendidos(texto, idSucursal, fechaDesde, fechaHasta, tipo, idProveedor, idMarca);
         }

         public DataTable ObtenerSerieVentasPorCorte(int idCorte, int idSucursal, DateTime fechaDesde, DateTime fechaHasta, string tipo, int idMarca, string agrupacionTemporal)
         {
             return oCorteD.ObtenerSerieVentasPorCorte(idCorte, idSucursal, fechaDesde, fechaHasta, tipo, idMarca, agrupacionTemporal);
         }

        public DataTable imprimirTeoricoReal(DataTable dtTeoricoReal, string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.imprimirTeoricoReal(dtTeoricoReal, texto, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable TotalKgsCortePorCompra(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.TotalKgsCortePorCompra(texto, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable TotalMovimientosPorCorte(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.TotalMovimientosPorCorte(texto, idSucursal, fechaDesde, fechaHasta);
        }

        public DataTable Balance(string texto, int idSucursal, DateTime fechaDesde, DateTime fechaHasta)
        {
            
            return oCorteD.Balance(texto, idSucursal, fechaDesde, fechaHasta);
        }

        public Entidades.ExistenciaPorSucursalesVm ObtenerMatrizExistenciaPorSucursales(Entidades.ExistenciaStockPorSucursalFiltroVm filtro)
        {
            if (filtro == null)
                filtro = new Entidades.ExistenciaStockPorSucursalFiltroVm();

            var resultado = new Entidades.ExistenciaPorSucursalesVm();
            resultado.Filtro = filtro;
            resultado.ConsultaRealizada = true;

            var plano = oCorteD.ObtenerExistenciaPorSucursalesPlano(
                filtro.Texto ?? "",
                filtro.IdSucursal,
                filtro.FechaHasta,
                filtro.Tipo ?? "",
                filtro.IdProveedor,
                filtro.IdMarca,
                filtro.IdCorte,
                filtro.SoloConStock) ?? new List<Entidades.ExistenciaStockPorSucursalPlanoVm>();

            // El SP devuelve PuntoStock desde el campo legacy Corte.puntoStock (global, no por
            // sucursal). Se sobrescribe aca con el valor real de la tabla intermedia Producto x
            // Sucursal, para las filas ya devueltas por el SP (chico: solo los productos de esta
            // empresa que ya pasaron los filtros), sin tocar la logica SQL del SP en si.
            var idsSucursalesEnResultado = plano.Select(x => x.IdSucursal).Distinct().ToList();
            var puntosStockPorSucursal = new Dictionary<(int idCorte, int idSucursal), int>();
            foreach (var idSucursal in idsSucursalesEnResultado)
            {
                foreach (var par in oCortePuntoStockSucursalD.FindPorSucursal(idSucursal))
                {
                    puntosStockPorSucursal[(par.Key, idSucursal)] = par.Value;
                }
            }

            foreach (var fila in plano)
            {
                fila.PuntoStock = puntosStockPorSucursal.TryGetValue((fila.IdCorte, fila.IdSucursal), out int puntoStockReal)
                    ? puntoStockReal
                    : 0;
            }

            resultado.Columnas = plano
                .GroupBy(x => new { x.IdSucursal, Nombre = x.Sucursal ?? "" })
                .Select(g => new Entidades.SucursalColumnaStockVm
                {
                    IdSucursal = g.Key.IdSucursal,
                    Sucursal = g.Key.Nombre
                })
                .OrderBy(x => x.Sucursal)
                .ToList();

            var gruposProductos = plano
                .GroupBy(x => new { x.IdCorte, x.Codigo, Nombre = x.Corte ?? "" })
                .OrderBy(g => g.Key.Codigo)
                .ThenBy(g => g.Key.Nombre);

            foreach (var grupo in gruposProductos)
            {
                var producto = new Entidades.ProductoStockPorSucursalVm
                {
                    IdCorte = grupo.Key.IdCorte,
                    Codigo = grupo.Key.Codigo,
                    Corte = grupo.Key.Nombre
                };

                producto.Detalles = grupo
                    .OrderBy(x => x.Sucursal)
                    .Select(x => new Entidades.DetalleStockSucursalVm
                    {
                        IdSucursal = x.IdSucursal,
                        Sucursal = x.Sucursal ?? "",
                        FechaUltimoCierre = x.FechaUltimoCierre,
                        StockInicial = x.StockInicial,
                        Compras = x.Compras,
                        IngresoElaborado = x.IngresoElaborado,
                        IngresoStock = x.IngresoStock,
                        IngresoMovimiento = x.IngresoMovimiento,
                        AjusteStock = x.AjusteStock,
                        TotalIngresos = x.TotalIngresos,
                        EgresoStock = x.EgresoStock,
                        EgresoMovimiento = x.EgresoMovimiento,
                        EgresoElaborado = x.EgresoElaborado,
                        Ventas = x.Ventas,
                        TotalEgresos = x.TotalEgresos,
                        StockActual = x.StockActual,
                        Promedio = x.Promedio,
                        PuntoStock = x.PuntoStock,
                        Pesable = x.Pesable,
                        EstadoStock = NormalizarEstadoStock(x.EstadoStock)
                    })
                    .ToList();

                // el producto es el mismo para todas las sucursales del grupo:
                // "pesable" y "promedio" son propiedades del corte, no de la sucursal.
                var primeraFila = grupo.FirstOrDefault();
                bool productoPesable = primeraFila != null && primeraFila.Pesable;
                float productoPromedio = primeraFila != null ? primeraFila.Promedio : 0f;

                foreach (var columna in resultado.Columnas)
                {
                    var filaSucursal = grupo.FirstOrDefault(x => x.IdSucursal == columna.IdSucursal);
                    producto.Celdas.Add(new Entidades.StockSucursalCeldaVm
                    {
                        IdSucursal = columna.IdSucursal,
                        Sucursal = columna.Sucursal,
                        StockActual = filaSucursal != null ? filaSucursal.StockActual : 0f,
                        Promedio = filaSucursal != null ? filaSucursal.Promedio : productoPromedio,
                        Pesable = filaSucursal != null ? filaSucursal.Pesable : productoPesable,
                        EstadoStock = filaSucursal != null
                            ? NormalizarEstadoStock(filaSucursal.EstadoStock)
                            : "SIN STOCK"
                    });

                    if (filaSucursal == null)
                    {
                        producto.Detalles.Add(new Entidades.DetalleStockSucursalVm
                        {
                            IdSucursal = columna.IdSucursal,
                            Sucursal = columna.Sucursal,
                            StockActual = 0f,
                            Promedio = productoPromedio,
                            PuntoStock = 0f,
                            Pesable = productoPesable,
                            EstadoStock = "SIN STOCK"
                        });
                    }
                }

                producto.Detalles = producto.Detalles
                    .OrderBy(x => x.Sucursal)
                    .ToList();

                resultado.Productos.Add(producto);
            }

            if (resultado.Productos.Count == 0)
                resultado.Mensaje = "No se encontraron datos para los filtros seleccionados.";

            return resultado;
        }
        #endregion

        #region Tipos Producto/Corte
        public DataTable obtenerTiposProductoGrilla(string buscarText)
        {
            
            return oCorteD.obtenerTiposProductoGrilla(buscarText);
        }

        public DataTable obtenerTiposProductoGrillaEmpresa(string buscarText)
        {
            return oCorteD.obtenerTiposProductoGrillaEmpresa(buscarText);
        }

        public DataTable obtenerTiposProductoCatalogoGlobal(string buscarText)
        {
            return oCorteD.obtenerTiposProductoCatalogoGlobal(buscarText);
        }

        public string importarTiposProductoGlobales(IEnumerable<string> tiposProducto, int? idUsuarioAlta)
        {
            return oCorteD.importarTiposProductoGlobales(tiposProducto, idUsuarioAlta);
        }

        public string addOrEditTipoProducto(string tiposProducto, string orden, bool esInsert, string tipoToUpdate)
        {
            
            return oCorteD.addOrEditTipoProducto(tiposProducto,orden, esInsert, tipoToUpdate);
        }

        public string eliminarTipoProducto(string tiposProducto)
        {
            
            return oCorteD.eliminarTipoProducto(tiposProducto);
        }


        /// <summary>
        /// Sugiere el menor codigo libre segun el tipo de producto
        /// </summary>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public long sugerirCodigo(string tipo)
        {
            
            return oCorteD.sugerirCodigo(tipo);
        }
        #endregion

        public int obtenerNivelCorte(int idCorteMaestro)
        {
            
            return oCorteD.obtenerNivelCorte(idCorteMaestro);
        }

        private static string NormalizarEstadoStock(string estadoStock)
        {
            string estado = (estadoStock ?? "").Trim().ToUpperInvariant();
            return string.IsNullOrWhiteSpace(estado) ? "SIN STOCK" : estado;
        }
    }
}
