using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using iTextSharp.text.pdf;
using iTextSharp.text;
using Presentacion.Reportes;
using System.IO;

namespace Presentacion.Cortes
{
    public partial class formReporteStock : Form
    {
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        DataTable dtSucursales;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Compra oCompraN = new Negocio.Compra();

        DataTable dtGrillaReporte = new DataTable();
        bool stockActual = false;
        bool stockProgresivo = false;
        bool acumVentas = false;
        bool combosCierresCargados = false;

        public formReporteStock()
        {
            InitializeComponent();
            cargarSucursales();
            cargarGrilla();
        }

        public void obtenerParametros(int sucursalParam, DateTime fechaDesdeParam, DateTime fechaHastaParam, int tipoReporteParam, string textoParam)
        {
            try
            {
                comboSucursal.SelectedIndex = sucursalParam-1;
                fechaDesde.Value = fechaDesdeParam;
                fechaHasta.Value = fechaHastaParam;
                comboTipoReporte.SelectedIndex = tipoReporteParam;
                txtDescripcion.Text = textoParam;

                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void imprimirReporte()
        {
            try
            {
                //Reporte Stock Actual
                if (comboTipoReporte.Text == "Stock Actual")
                {
                    Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
                    tipoTicket.stockActual(comboInicioStock.Text, comboCierreStock.Text, grillaReportes);
                    return;
                }

                //Reporte Cierre Stock
                if (comboTipoReporte.Text == "Cierre Stock")
                {
                    Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
                    tipoTicket.cierreStock(comboInicioStock.Text, comboCierreStock.Text, grillaReportes);
                    return;
                    #region reporte anterior
                    //ReportesDataSet.dtCierreStockDataTable dtCierreStock = new ReportesDataSet.dtCierreStockDataTable();

                    //string titulo = "Reporte Cierre Stock";
                    //foreach (DataRow fila in dtGrillaReporte.Rows)
                    //{
                    //    DataRow dsFila = dtCierreStock.NewRow();
                    //    dsFila["Codigo"] = fila["Codigo"];
                    //    dsFila["Corte"] = fila["Corte"];
                    //    dsFila["Sucursal"] = fila["Sucursal"];
                    //    dsFila["TotalIngresado"] = fila["Total Ingresado"];
                    //    dsFila["KgsEnEmbutidos"] = fila["Kgs En Embutidos"];
                    //    dsFila["TotalVendido"] = fila["Total Vendido"];
                    //    dsFila["StockTeorico"] = fila["Stock Teorico"];
                    //    dsFila["StockReal"] = fila["Stock Real"];
                    //    dsFila["Faltante"] = fila["Faltante"];

                    //    dtCierreStock.Rows.Add(dsFila);
                    //}
                    //ReporteCierreStock reporte = new ReporteCierreStock();
                    //FormReportes frmReportes = new FormReportes(reporte, titulo, dtCierreStock, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    //frmReportes.Show();
                    #endregion
                }

                if (comboTipoReporte.SelectedIndex == 6)
                {
                    ReportesDataSet.dtReporteTeoricoRealDataTable dtTeoricoReal = new ReportesDataSet.dtReporteTeoricoRealDataTable();

                    string titulo = "Reporte Kg. Corte Teórico - Real";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtTeoricoReal.NewRow();

                        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                        {
                            dsFila[col] = fila[col];
                        }
                        dtTeoricoReal.Rows.Add(dsFila);
                    }
                    Reportes.Reportes reporte = new Reportes.Reportes();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTeoricoReal, fechaDesde.Value.Date, fechaHasta.Value.Date);

                    frmReportes.Show();
                }


                if (comboTipoReporte.Text == "Cierre Stock 2")
                {
                    ReportesDataSet.dtCierreStockDataTable dtCierreStock = new ReportesDataSet.dtCierreStockDataTable();

                    string titulo = "Reporte Cierre Stock 2";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtCierreStock.NewRow();
                        dsFila["Codigo"] = fila["Codigo"];
                        dsFila["Corte"] = fila["Corte"];
                        dsFila["Sucursal"] = fila["Sucursal"];
                        dsFila["TotalIngresado"] = fila["Total Ingresado"];
                        dsFila["KgsEnEmbutidos"] = fila["Kgs En Embutidos"];
                        dsFila["TotalVendido"] = fila["Total Vendido"];
                        dsFila["StockTeorico"] = fila["Stock Teorico"];
                        dsFila["StockReal"] = fila["Stock Real"];
                        dsFila["Faltante"] = fila["Faltante"];

                        dtCierreStock.Rows.Add(dsFila);
                    }
                    ReporteCierreStock reporte = new ReporteCierreStock();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtCierreStock, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    frmReportes.Show();
                }

                //Reporte Ingreso-Egreso
                if (comboTipoReporte.SelectedIndex == 2)
                {
                    ReportesDataSet.dtIngresoEgresoDataTable dtIngresoEgreso = new ReportesDataSet.dtIngresoEgresoDataTable();

                    string titulo = "Reporte Ingreso - Egreso";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtIngresoEgreso.NewRow();
                        dsFila["Codigo"] = fila["Codigo"];
                        dsFila["Corte"] = fila["Corte"];
                        dsFila["Sucursal"] = fila["Sucursal"];
                        dsFila["TotalIngresado"] = fila["Total Ingresado"];
                        dsFila["KgsEnEmbutidos"] = fila["Kgs En Embutidos"];
                        dsFila["TotalVendido"] = fila["Total Vendido"];
                        dsFila["DiferenciaStock"] = fila["Diferencia Stock"];
                    
                        dtIngresoEgreso.Rows.Add(dsFila);
                    }
                    ReporteIngresoEgreso reporte = new ReporteIngresoEgreso();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtIngresoEgreso, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    frmReportes.Show();
                }

                if (comboTipoReporte.SelectedIndex == 3)
                {
                    ReportesDataSet.dtTotalPorCortesDataTable dtTotalPorCortes = new ReportesDataSet.dtTotalPorCortesDataTable();

                    string titulo = "Reporte Total Cortes Vendidos";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtTotalPorCortes.NewRow();

                        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                        {
                            dsFila[col] = fila[col];
                        }
                        dtTotalPorCortes.Rows.Add(dsFila);
                    }
                    ReporteTotalPorCortes reporte = new ReporteTotalPorCortes();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalPorCortes, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    frmReportes.Show();
                }

                if (comboTipoReporte.SelectedIndex == 4)
                {
                    ReportesDataSet.dtTotalCortePorCompraDataTable dtTotalCortePorCompra = new ReportesDataSet.dtTotalCortePorCompraDataTable();

                    string titulo = "Reporte Total Kgs Corte Por Compra";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtTotalCortePorCompra.NewRow();

                        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                        {
                            dsFila[col] = fila[col];
                        }
                        dtTotalCortePorCompra.Rows.Add(dsFila);                        
                    }
                    ReporteKgsCortePorCompra reporte = new ReporteKgsCortePorCompra();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalCortePorCompra, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    frmReportes.Show();
                }

                if (comboTipoReporte.SelectedIndex == 5)
                {
                    ReportesDataSet.dtTotalMovimientosDataTable dtTotalMovimientos = new ReportesDataSet.dtTotalMovimientosDataTable();

                    string titulo = "Total Movimiento Por Corte";
                    foreach (DataRow fila in dtGrillaReporte.Rows)
                    {
                        DataRow dsFila = dtTotalMovimientos.NewRow();

                        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                        {
                            dsFila[col] = fila[col];
                        }
                        dtTotalMovimientos.Rows.Add(dsFila);
                    }
                    ReporteMovimientosPorCorte reporte = new ReporteMovimientosPorCorte();
                    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalMovimientos, fechaDesde.Value.Date, fechaHasta.Value.Date);
                    frmReportes.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarGrilla()
        {
            lblActualizar.Visible = false;
            //reporteTeoricoReal
            if (comboTipoReporte.Text.Equals("Teorico - Real"))
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.reporteTeoricoReal(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);

                foreach (DataRow fila in dtGrillaReporte.Rows)
                {
                    if (fila["Stock Teórico"].ToString() == null || fila["Stock Teórico"].ToString() == "")
                    {
                        fila["Stock Teórico"] = 0;
                    }

                    if (fila["Stock Real"].ToString() == null || fila["Stock Real"].ToString() == "")
                    {
                        fila["Stock Real"] = 0;
                    }
                    string stockTeorico, stockReal;

                    stockTeorico = Convert.ToString(fila["Stock Teórico"]);
                    stockReal = Convert.ToString(fila["Stock Real"]);

                    fila["Diferencia"] = Convert.ToDecimal(stockTeorico) - Convert.ToDecimal(stockReal);
                }
                grillaReportes.DataSource = dtGrillaReporte;
            }

            //Cierre Stock
            if (comboTipoReporte.Text == "Cierre Stock" || comboTipoReporte.Text == "Stock Actual" ||
                comboTipoReporte.Text == "Stock Progresivo")
            {
                try
                {
                    if (!comboInicioStock.Enabled || !comboCierreStock.Enabled || comboInicioStock.Text.Equals("") || 
                        (comboCierreStock.Text.Equals("") && !stockProgresivo))
                    {
                        //MessageBox.
                    }
                    else
                    {
                        grillaReportes.DataSource = null;
                        dtGrillaReporte = null;

                        string fechaHastaString = stockProgresivo ? txtFechaHastaProgresivo.Text : comboCierreStock.Text;
                        dtGrillaReporte = oCorteN.CierreStock(1, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                            Convert.ToDateTime(comboInicioStock.Text), Convert.ToDateTime(fechaHastaString));
                        foreach (DataRow fila in dtGrillaReporte.Rows)
                        {
                            decimal TotINGR = 0, TotEGR =0;
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

                            if (fila["Ingr.Emb"].ToString() == null || fila["Ingr.Emb"].ToString() == "")
                            {
                                fila["Ingr.Emb"] = 0;
                            }
                            else
                            {
                                TotINGR += Convert.ToDecimal(fila["Ingr.Emb"]);
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

                            if (fila["Egr.Emb"].ToString() == null || fila["Egr.Emb"].ToString() == "")
                            {
                                fila["Egr.Emb"] = 0;
                            }
                            else
                            {
                                TotEGR += Convert.ToDecimal(fila["Egr.Emb"]);
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

                        }
                        grillaReportes.DataSource = dtGrillaReporte;

                        System.Drawing.Font fuente = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                        grillaReportes.Columns["Codigo"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Corte"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                        grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                        grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.BackColor = Color.LightBlue;
                        grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Faltante"].DefaultCellStyle.Font = fuente;

                        //si es consulta Stock Actual
                        if (stockActual)
                        {
                            grillaReportes.Columns["DIF"].Visible = !stockActual;
                            grillaReportes.Columns["Stock.Cierre"].Visible = !stockActual;
                            grillaReportes.Columns["Faltante"].HeaderText = "Stock Actual";
                        }

                        //fuente = new System.Drawing.Font("Microsoft Sans Serif", 9.50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //grillaReportes.AlternatingRowsDefaultCellStyle.Font = fuente;
                    }
                }
                catch (Exception)
                {
                    //MessageBox.Show(ex.Message);
                }
            }

            //Cierre Stock
            if (comboTipoReporte.Text == "Cierre Stock 2")
            {
                //DataTable dtTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.CierreStock(2, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);
                foreach (DataRow fila in dtGrillaReporte.Rows)
                {
                    if (fila["Kgs En Embutidos"].ToString() == null || fila["Kgs En Embutidos"].ToString() == "")
                    {
                        fila["Kgs En Embutidos"] = 0;
                    }

                    if (fila["Total Vendido"].ToString() == null || fila["Total Vendido"].ToString() == "")
                    {
                        fila["Total Vendido"] = 0;
                    }

                    if (fila["Stock Real"].ToString() == null || fila["Stock Real"].ToString() == "")
                    {
                        fila["Stock Real"] = 0;
                    }


                    string totalIngresado, kgsEnEmbutido, totalVendido, stockReales;

                    totalIngresado = Convert.ToString(fila["Total Ingresado"]);
                    kgsEnEmbutido = Convert.ToString(fila["Kgs En Embutidos"]);
                    totalVendido = Convert.ToString(fila["Total Vendido"]);
                    stockReales = Convert.ToString(fila["Stock Real"]);

                    decimal stockTeorico, stockReal, faltante;

                    stockTeorico = Convert.ToDecimal(totalIngresado) - Convert.ToDecimal(kgsEnEmbutido) - Convert.ToDecimal(totalVendido);
                    stockReal = Convert.ToDecimal(stockReales);

                    fila["Stock Teorico"] = stockTeorico;

                    faltante = stockTeorico - stockReal;

                    fila["Faltante"] = faltante;

                }

                grillaReportes.DataSource = dtGrillaReporte;
            }

            //StockIngresoEgreso
            if (comboTipoReporte.Text.Equals("Ingreso - Egreso"))
            {
                //DataTable dtTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.StockIngresoEgreso(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);
                foreach (DataRow fila in dtGrillaReporte.Rows)
                {
                    if (fila["Kgs En Embutidos"].ToString() == null || fila["Kgs En Embutidos"].ToString() == "")
                    {
                        fila["Kgs En Embutidos"] = 0;
                    }

                    if (fila["Total Vendido"].ToString() == null || fila["Total Vendido"].ToString() == "")
                    {                       
                        fila["Total Vendido"] = 0;                      
                    }

                    string totalIngresado, kgsEnEmbutido, totalVendido;

                    totalIngresado = Convert.ToString(fila["Total Ingresado"]);
                    kgsEnEmbutido = Convert.ToString(fila["Kgs En Embutidos"]);
                    totalVendido = Convert.ToString(fila["Total Vendido"]);

                    fila["Diferencia Stock"] = Convert.ToDecimal(totalIngresado) - Convert.ToDecimal(kgsEnEmbutido) - Convert.ToDecimal(totalVendido);
                }
                grillaReportes.DataSource = dtGrillaReporte;
            }

            //Acumulado de ventas
            if (comboTipoReporte.Text.Equals("Acum. Ventas"))
            {
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.acum_Ventas(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesdeProgresivo.Value, txtFechaHastaProgresivo.Value);

                DataTable dtStockActual = oCorteN.CierreStock(1, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                            Convert.ToDateTime(comboInicioStock.Text), DateTime.Now);

                foreach (DataRow fila in dtStockActual.Rows)
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

                    if (fila["Ingr.Emb"].ToString() == null || fila["Ingr.Emb"].ToString() == "")
                    {
                        fila["Ingr.Emb"] = 0;
                    }
                    else
                    {
                        TotINGR += Convert.ToDecimal(fila["Ingr.Emb"]);
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

                    if (fila["Egr.Emb"].ToString() == null || fila["Egr.Emb"].ToString() == "")
                    {
                        fila["Egr.Emb"] = 0;
                    }
                    else
                    {
                        TotEGR += Convert.ToDecimal(fila["Egr.Emb"]);
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
                }

                foreach (DataRow fila in dtGrillaReporte.Rows)
                {
                    if (fila["Ventas"].ToString() == null || fila["Ventas"].ToString() == ""
                        || fila["Ventas"] == DBNull.Value)
                    {
                        fila["Ventas"] = 0;
                    }

                    foreach (DataRow filaStock in dtStockActual.Rows)
                    {
                        if (filaStock["Codigo"].ToString().Equals(fila["Codigo"].ToString()))
                        {
                            fila["DIF"] = Convert.ToDecimal(filaStock["Faltante"]) - Convert.ToDecimal(fila["Ventas"]);
                            break;
                        }
                    }
                }

                grillaReportes.DataSource = dtGrillaReporte;
            }

            //TotalPorCortesVendidos
            if (comboTipoReporte.Text.Equals("Total Cortes Vendidos"))
            {
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;
                if (Presentacion.FormPrincipal.logueado == false)
                {
                    MessageBox.Show("No está logueado!.\nInicie sesión y vuelva a intentar.");
                }
                else
                {
                    //si está logueado
                    if (Presentacion.FormPrincipal.logueado)
                    {
                        dtGrillaReporte = oCorteN.TotalPorCortesVendidos(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);
                    }
                    grillaReportes.DataSource = dtGrillaReporte;
                }
            }
            
            //TotalKgsCortePorCompra
            if (comboTipoReporte.Text.Equals("Total Kgs Corte/Compra"))
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalKgsCortePorCompra(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesde.Value.Date, fechaHasta.Value.Date);
                grillaReportes.DataSource = dtGrillaReporte;

            }

            //TotalMovimientosPorCorte
            if (comboTipoReporte.Text.Equals("Movimiento/Corte")) 
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalMovimientosPorCorte(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);


                grillaReportes.DataSource = dtGrillaReporte;
                //grillaReportes.AlternatingRowsDefaultCellStyle.BackColor = grillaReportes.AlternatingRowsDefaultCellStyle.BackColor.Name{"0"};
                System.Drawing.Font fuente = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                grillaReportes.Columns["Codigo"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Corte"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Total Unidades"].DefaultCellStyle.BackColor = Color.PaleGreen;
                grillaReportes.Columns["Total Unidades"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Total Kgs"].DefaultCellStyle.BackColor = Color.LightBlue;
                grillaReportes.Columns["Total Kgs"].DefaultCellStyle.Font = fuente;
            }

            try
            {
                switch (comboOrdenStock.Text)
                {
                    case "Ascendente":
                        grillaReportes.Sort(grillaReportes.Columns["Faltante"], ListSortDirection.Ascending);
                        break;
                    case "Descendente":
                        grillaReportes.Sort(grillaReportes.Columns["Faltante"], ListSortDirection.Descending);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
                
            }
        }
        
        private void cargarSucursales()
        {
            dtSucursales = oSucursalN.obtenerSucursales();

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idsucursal";
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            imprimirReporte();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formReporteStock_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            comboOrdenStock.SelectedIndex = 0;
        }

        private void cargarComboCierreStock()
        {
            if (comboSucursal.ValueMember != "" && (comboTipoReporte.Text.Equals("Cierre Stock") ||
                comboTipoReporte.Text.Equals("Stock Actual") || comboTipoReporte.Text.Equals("Stock Progresivo") ||
                comboTipoReporte.Text.Equals("Acum. Ventas")))
            {
                DateTime desde = DateTime.Today.Date.AddYears(-10);
                DateTime hasta = DateTime.Today.Date.AddDays(1);

                DataTable dtInicioStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta);
                comboInicioStock.DataSource = dtInicioStock;
                comboInicioStock.DisplayMember = "fechaCompra";
                comboInicioStock.ValueMember = "idCompra";
                comboInicioStock.SelectedIndex = dtInicioStock.Rows.Count > 1 && !stockActual && !stockProgresivo ? 1 : comboInicioStock.SelectedIndex;// dtInicioStock.Rows.Count > 1 ? 1 : -1;

                fechaDesdeProgresivo.Visible = acumVentas;
                txtFechaHastaProgresivo.Visible = stockProgresivo || acumVentas;
                DataTable dtCierreStock;
                if (stockProgresivo || acumVentas)
                {
                    fechaDesdeProgresivo.Value = DateTime.Now;
                    txtFechaHastaProgresivo.Value = DateTime.Now; 
                }
                else
                {
                    if (stockActual)
                    {
                        dtCierreStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), DateTime.Now, DateTime.Now);
                        DataRow fechaActual = dtCierreStock.NewRow();
                        fechaActual["idCompra"] = 0;
                        fechaActual["fechaCompra"] = DateTime.Now;
                        dtCierreStock.Rows.Add(fechaActual);
                    }
                    else
                    {
                        dtCierreStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta);
                    }
                    combosCierresCargados = true;
                    comboCierreStock.DataSource = dtCierreStock;
                    comboCierreStock.DisplayMember = "fechaCompra";
                    comboCierreStock.ValueMember = "idCompra";
                }
            }
        }

        private void comboInicioStock_SelectedValueChanged(object sender, EventArgs e)
        {
            if (combosCierresCargados)
            {
                cargarGrilla();
            }
        }

        private void comboCierreStock_SelectedValueChanged(object sender, EventArgs e)
        {
            if (combosCierresCargados)
            {
                cargarGrilla();
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                //Creating iTextSharp Table from the DataTable data
                PdfPTable pdfTable = new PdfPTable(grillaReportes.ColumnCount);
                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 100;
                pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                iTextSharp.text.Font fontsubtit = FontFactory.GetFont("Arial", 9);

                string encabezado = "";

                //Adding Header row
                foreach (DataGridViewColumn column in grillaReportes.Columns)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(column.HeaderText, fontsubtit));
                    cell.BackgroundColor = new iTextSharp.text.BaseColor(240, 240, 240);  //.text.Color(240, 240, 240);

                    encabezado = comboTipoReporte.Text + "\n Sucursal: " + comboSucursal.Text;
                    if (comboTipoReporte.Text == "Cierre Stock")
                    {
                        encabezado += " ||| Desde: " + comboInicioStock.Text +
                            " | Hasta: " + comboCierreStock.Text + "\n\n";
                    }
                    else
                    {
                        encabezado += " ||| Desde: " + fechaDesde.Text +
                            " | Hasta: " + fechaHasta.Text + "\n\n";
                    }
                    pdfTable.AddCell(cell);
                }

                //Adding DataRow
                foreach (DataGridViewRow row in grillaReportes.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        string valueCell = "";
                        if (cell.ValueType.Name.Equals("Double") || cell.ValueType.Name.Equals("Decimal"))
                        {
                            valueCell = String.Format("{0:0.00}", cell.Value);
                        }
                        else
                        {
                            valueCell = cell.Value.ToString();
                            valueCell = (valueCell.Length > 6) ? valueCell.Substring(0, 6) : valueCell;
                        }
                        pdfTable.AddCell(new Phrase(valueCell, fontsubtit));
                    }
                }

                //agregando encabezado
                Paragraph parrafo = new Paragraph();
                parrafo.Alignment = Element.ALIGN_CENTER;
                parrafo.Font = FontFactory.GetFont("Arial", 9);
                parrafo.Add(encabezado);

                string fileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                    PdfWriter.GetInstance(pdfDoc, stream);
                    pdfDoc.Open();
                    pdfDoc.Add(parrafo);
                    pdfDoc.Add(pdfTable);
                    pdfDoc.Close();
                    stream.Close();

                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = fileName;
                    prc.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }            
        }

        private void comboTipoReporte_SelectedValueChanged(object sender, EventArgs e)
        {
            combosCierresCargados = false;
            if (comboTipoReporte.Text.Equals("Cierre Stock") ||
                comboTipoReporte.Text.Equals("Stock Actual") ||
                comboTipoReporte.Text.Equals("Stock Progresivo") ||
                comboTipoReporte.Text.Equals("Acum. Ventas"))
            {
                stockActual = comboTipoReporte.Text.Equals("Stock Actual");
                acumVentas = comboTipoReporte.Text.Equals("Acum. Ventas");
                stockProgresivo = comboTipoReporte.Text.Equals("Stock Progresivo");
                comboInicioStock.Visible = !acumVentas;
                comboCierreStock.Visible = !stockProgresivo && !acumVentas;
                cargarComboCierreStock();
            }
            else
            {
                comboInicioStock.Visible = false;
                comboCierreStock.Visible = false;
            }
        }

        private void txtFechaHastaProgresivo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void txtFechaHastaProgresivo_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void fechaDesdeProgresivo_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }
    }
}
