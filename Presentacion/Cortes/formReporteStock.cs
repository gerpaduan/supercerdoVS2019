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

using System.IO;
using OfficeOpenXml;
using System.Configuration;
using Presentacion.Personas;

namespace Presentacion.Cortes
{
    public partial class formReporteStock : Form, InterfacePersona
    {
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Persona oMarcaE = new Entidades.Persona();
        Entidades.Persona oProveedor = new Entidades.Persona();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        DataTable dtSucursales;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Compra oCompraN = new Negocio.Compra();

        string cargarPersona = "";
        string tipo = "";
        int idMarca, idProveedor;

        DataTable dtGrillaReporte = new DataTable();
        bool stockActual = false;
        bool stockProgresivo = false;
        bool acumVentas = false;
        bool combosCierresCargados = false;
        string[] arrayRowFilter = new string[] { "1 = 1", "1 = 1", "1 = 1", "1 = 1" };
        string consultaRowFilter = "";
        DateTime fechaUltimoCierreStock;

        public formReporteStock()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            cargarSucursales();
            cargarGrilla();
        }

        public void obtenerParametros(int sucursalParam, DateTime fechaDesdeParam, DateTime fechaHastaParam, int tipoReporteParam, string textoParam)
        {
            try
            {
                comboSucursal.SelectedIndex = sucursalParam-1;
                fechaDesdeProgresivo.Value = fechaDesdeParam;
                txtFechaHastaProgresivo.Value = fechaHastaParam;
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

                //Reporte Acum
                if (comboTipoReporte.Text == "Proyeccion Ventas vs Stock")
                {
                    Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
                    tipoTicket.acumVentas(fechaDesdeProgresivo.Value.ToString(), txtFechaHastaProgresivo.Value.ToString(), grillaReportes);
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
                    //FormReportes frmReportes = new FormReportes(reporte, titulo, dtCierreStock, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                    //frmReportes.Show();
                    #endregion
                }

                #region Codigo comentado

                //if (comboTipoReporte.SelectedIndex == 6)
                //{
                //    ReportesDataSet.dtReporteTeoricoRealDataTable dtTeoricoReal = new ReportesDataSet.dtReporteTeoricoRealDataTable();

                //    string titulo = "Reporte Kg. Corte Teórico - Real";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtTeoricoReal.NewRow();

                //        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                //        {
                //            dsFila[col] = fila[col];
                //        }
                //        dtTeoricoReal.Rows.Add(dsFila);
                //    }
                //    Reportes.Reportes reporte = new Reportes.Reportes();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTeoricoReal, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);

                //    frmReportes.Show();
                //}


                //if (comboTipoReporte.Text == "Cierre Stock 2")
                //{
                //    ReportesDataSet.dtCierreStockDataTable dtCierreStock = new ReportesDataSet.dtCierreStockDataTable();

                //    string titulo = "Reporte Cierre Stock 2";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtCierreStock.NewRow();
                //        dsFila["Codigo"] = fila["Codigo"];
                //        dsFila["Corte"] = fila["Corte"];
                //        dsFila["Sucursal"] = fila["Sucursal"];
                //        dsFila["TotalIngresado"] = fila["Total Ingresado"];
                //        dsFila["KgsEnEmbutidos"] = fila["Kgs En Embutidos"];
                //        dsFila["TotalVendido"] = fila["Total Vendido"];
                //        dsFila["StockTeorico"] = fila["Stock Teorico"];
                //        dsFila["StockReal"] = fila["Stock Real"];
                //        dsFila["Faltante"] = fila["Faltante"];

                //        dtCierreStock.Rows.Add(dsFila);
                //    }
                //    ReporteCierreStock reporte = new ReporteCierreStock();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtCierreStock, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                //    frmReportes.Show();
                //}

                ////Reporte Ingreso-Egreso
                //if (comboTipoReporte.SelectedIndex == 2)
                //{
                //    ReportesDataSet.dtIngresoEgresoDataTable dtIngresoEgreso = new ReportesDataSet.dtIngresoEgresoDataTable();

                //    string titulo = "Reporte Ingreso - Egreso";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtIngresoEgreso.NewRow();
                //        dsFila["Codigo"] = fila["Codigo"];
                //        dsFila["Corte"] = fila["Corte"];
                //        dsFila["Sucursal"] = fila["Sucursal"];
                //        dsFila["TotalIngresado"] = fila["Total Ingresado"];
                //        dsFila["KgsEnEmbutidos"] = fila["Kgs En Embutidos"];
                //        dsFila["TotalVendido"] = fila["Total Vendido"];
                //        dsFila["DiferenciaStock"] = fila["Diferencia Stock"];
                    
                //        dtIngresoEgreso.Rows.Add(dsFila);
                //    }
                //    ReporteIngresoEgreso reporte = new ReporteIngresoEgreso();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtIngresoEgreso, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                //    frmReportes.Show();
                //}

                //if (comboTipoReporte.SelectedIndex == 3)
                //{
                //    ReportesDataSet.dtTotalPorCortesDataTable dtTotalPorCortes = new ReportesDataSet.dtTotalPorCortesDataTable();

                //    string titulo = "Reporte Total Cortes Vendidos";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtTotalPorCortes.NewRow();

                //        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                //        {
                //            dsFila[col] = fila[col];
                //        }
                //        dtTotalPorCortes.Rows.Add(dsFila);
                //    }
                //    ReporteTotalPorCortes reporte = new ReporteTotalPorCortes();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalPorCortes, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                //    frmReportes.Show();
                //}

                //if (comboTipoReporte.SelectedIndex == 4)
                //{
                //    ReportesDataSet.dtTotalCortePorCompraDataTable dtTotalCortePorCompra = new ReportesDataSet.dtTotalCortePorCompraDataTable();

                //    string titulo = "Reporte Total Kgs Corte Por Compra";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtTotalCortePorCompra.NewRow();

                //        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                //        {
                //            dsFila[col] = fila[col];
                //        }
                //        dtTotalCortePorCompra.Rows.Add(dsFila);                        
                //    }
                //    ReporteKgsCortePorCompra reporte = new ReporteKgsCortePorCompra();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalCortePorCompra, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                //    frmReportes.Show();
                //}

                //if (comboTipoReporte.SelectedIndex == 5)
                //{
                //    ReportesDataSet.dtTotalMovimientosDataTable dtTotalMovimientos = new ReportesDataSet.dtTotalMovimientosDataTable();

                //    string titulo = "Total Movimiento Por Corte";
                //    foreach (DataRow fila in dtGrillaReporte.Rows)
                //    {
                //        DataRow dsFila = dtTotalMovimientos.NewRow();

                //        for (int col = 0; col < dtGrillaReporte.Columns.Count; col++)
                //        {
                //            dsFila[col] = fila[col];
                //        }
                //        dtTotalMovimientos.Rows.Add(dsFila);
                //    }
                //    ReporteMovimientosPorCorte reporte = new ReporteMovimientosPorCorte();
                //    FormReportes frmReportes = new FormReportes(reporte, titulo, dtTotalMovimientos, fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                //    frmReportes.Show();
                //}

                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarGrilla()
        {
            if (!Usuarios.FormValidarPermiso.validarPermiso(comboTipoReporte.Text))
                return;

            Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso("Cargando reporte", "Cargando...");
            barraProgreso.Show();

            lblActualizar.Visible = false;
            checkSoloFaltantes.Checked = false;
            checkOcultarColumnas.Checked = false;
            checkOcultarPtoStock.Checked = false;
            //checkOcultarPtoStock.Visible = false;
            ///reporteTeoricoReal
            if (comboTipoReporte.Text.Equals("Teorico - Real"))
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.reporteTeoricoReal(txtDescripcion.Text.Trim(), 
                    Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);

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

            ///Cierre Stock
            if (comboTipoReporte.Text == "Cierre Stock" || comboTipoReporte.Text == "Stock Actual" ||
                comboTipoReporte.Text == "Stock Retroactivo")
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
                            Convert.ToDateTime(comboInicioStock.Text), Convert.ToDateTime(fechaHastaString), null, tipo , idProveedor, idMarca);
                        #region pasado a capa Negocio
                        //foreach (DataRow fila in dtGrillaReporte.Rows)
                        //{
                        //    decimal TotINGR = 0, TotEGR =0;
                        //    if (fila["Stock.Ini"].ToString() == null || fila["Stock.Ini"].ToString() == "")
                        //    {
                        //        fila["Stock.Ini"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Stock.Ini"]);
                        //    }

                        //    if (fila["Compras"].ToString() == null || fila["Compras"].ToString() == "")
                        //    {
                        //        fila["Compras"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Compras"]);
                        //    }

                        //    if (fila["Ingr.Elab"].ToString() == null || fila["Ingr.Elab"].ToString() == "")
                        //    {
                        //        fila["Ingr.Elab"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Ingr.Elab"]);
                        //    }

                        //    if (fila["Ingr.Stock"].ToString() == null || fila["Ingr.Stock"].ToString() == "")
                        //    {
                        //        fila["Ingr.Stock"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Ingr.Stock"]);
                        //    }

                        //    if (fila["Ingr. Mov"].ToString() == null || fila["Ingr. Mov"].ToString() == "")
                        //    {
                        //        fila["Ingr. Mov"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Ingr. Mov"]);
                        //    }

                        //    if (fila["Ajus.Stock"].ToString() == null || fila["Ajus.Stock"].ToString() == "")
                        //    {
                        //        fila["Ajus.Stock"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotINGR += Convert.ToDecimal(fila["Ajus.Stock"]);
                        //    }

                        //    if (fila["Egr.Stock"].ToString() == null || fila["Egr.Stock"].ToString() == "")
                        //    {
                        //        fila["Egr.Stock"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotEGR += Convert.ToDecimal(fila["Egr.Stock"]);
                        //    }

                        //    if (fila["Egr.Mov"].ToString() == null || fila["Egr.Mov"].ToString() == "")
                        //    {
                        //        fila["Egr.Mov"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotEGR += Convert.ToDecimal(fila["Egr.Mov"]);
                        //    }

                        //    if (fila["Egr.Elab"].ToString() == null || fila["Egr.Elab"].ToString() == "")
                        //    {
                        //        fila["Egr.Elab"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotEGR += Convert.ToDecimal(fila["Egr.Elab"]);
                        //    }

                        //    if (fila["Ventas"].ToString() == null || fila["Ventas"].ToString() == "")
                        //    {
                        //        fila["Ventas"] = 0;
                        //    }
                        //    else
                        //    {
                        //        TotEGR += Convert.ToDecimal(fila["Ventas"]);
                        //    }

                        //    if (fila["Stock.Cierre"].ToString() == null || fila["Stock.Cierre"].ToString() == "")
                        //    {
                        //        fila["Stock.Cierre"] = 0;
                        //    }

                        //    fila["Tot.INGR"] = TotINGR;
                        //    fila["Tot.EGR"] = TotEGR;
                        //    fila["DIF"] = TotINGR - TotEGR;

                        //    fila["Faltante"] = Convert.ToDecimal(fila["DIF"]) - Convert.ToDecimal(fila["Stock.Cierre"]);

                        //    float stockKg = Utilidades.Util_Form.convertFloat(fila["Faltante"].ToString(), false);
                        //    //string stockUn = Math.Round(Convert.ToDecimal(stockKg / float.Parse(fila["promedio"].ToString()))).ToString() + " u";
                        //    string stock = Convert.ToDecimal(fila["promedio"]) == 0 ? stockKg.ToString("F2") :
                        //        Math.Round(Convert.ToDecimal(stockKg / float.Parse(fila["promedio"].ToString()))).ToString() + " u";//stockUn;// stockUn.ToString("F1") + " u";
                        //    fila["Stock.Un"] = stock;

                        //    //Si Punto Stock mayor a cero significa que se necesita saber el faltante del producto
                        //    //
                        //    fila["Falta"] = Convert.ToDecimal(fila["Pto.Stock"]) > 0 && ((Convert.ToDecimal(fila["DIF"]) < 0) || (Convert.ToDecimal(fila["Pto.Stock"]) - (Convert.ToDecimal(fila["DIF"])) <= 0)) ? "X" : "";
                        //}
                        #endregion

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
                        grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.Format = "F4";
                        grillaReportes.Columns["Faltante"].DefaultCellStyle.Font = fuente;

                        grillaReportes.Columns["Stock.Un"].DefaultCellStyle.BackColor = Color.LightBlue;
                        grillaReportes.Columns["Stock.Un"].DefaultCellStyle.Font = fuente;
                        
                        grillaReportes.Columns["promedio"].Visible = false;
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

            ///Cierre Stock 2
            if (comboTipoReporte.Text == "Cierre Stock 2")
            {
                //DataTable dtTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.CierreStock(2, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date, null, tipo, idProveedor, idMarca);
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

            ///StockIngresoEgreso
            if (comboTipoReporte.Text.Equals("Ingreso - Egreso"))
            {
                //DataTable dtTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.StockIngresoEgreso(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
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

            ///Acumulado de ventas
            if (comboTipoReporte.Text.Equals("Proyeccion Ventas vs Stock"))
            {
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.acum_Ventas(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesdeProgresivo.Value, txtFechaHastaProgresivo.Value, tipo, idProveedor, idMarca);

                DataTable dtStockActual = oCorteN.CierreStock(1, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                            fechaUltimoCierreStock, DateTime.Now, null, tipo, idProveedor, idMarca);


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
                            fila["StockActual"] = Convert.ToDecimal(filaStock["Faltante"]);
                            fila["DIF"] = Convert.ToDecimal(fila["StockActual"]) - Convert.ToDecimal(fila["Ventas"]);
                            break;
                        }
                    }
                }
                grillaReportes.DataSource = dtGrillaReporte;
            }

            ///TotalPorCortesVendidos
            if (comboTipoReporte.Text.Equals("Ventas por Producto"))
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
                        dtGrillaReporte = oCorteN.TotalPorCortesVendidos(txtDescripcion.Text.Trim(), 
                            Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesdeProgresivo.Value,
                            txtFechaHastaProgresivo.Value, tipo, idProveedor, idMarca);
                    }
                    grillaReportes.DataSource = dtGrillaReporte;
                }
            }
            
            ///TotalKgsCortePorCompra
            if (comboTipoReporte.Text.Equals("Total Kgs Corte/Compra"))
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalKgsCortePorCompra(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                grillaReportes.DataSource = dtGrillaReporte;

            }

            ///TotalMovimientosPorCorte
            if (comboTipoReporte.Text.Equals("Movimiento/Corte")) 
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalMovimientosPorCorte(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);


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

            ///Balance
            if (comboTipoReporte.Text.Equals("Balance Económico"))
            {
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.Balance(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesdeProgresivo.Value.Date, txtFechaHastaProgresivo.Value.Date);
                grillaReportes.DataSource = dtGrillaReporte;


                //formato filas
                for(int i = 0; grillaReportes.Rows.Count > i; i++)
                {
                    grillaReportes.Rows[0].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 12, FontStyle.Bold);
                    grillaReportes.Rows[0].DefaultCellStyle.BackColor = Color.LightGray;

                    if (grillaReportes.Rows[i].Cells["Descripcion"].Value.ToString().Contains("COMPRAS") ||
                        grillaReportes.Rows[i].Cells["Descripcion"].Value.ToString().Contains("VENTAS A CONS") ||
                        grillaReportes.Rows[i].Cells["Descripcion"].Value.ToString().Contains("GASTOS"))
                    {
                        grillaReportes.Rows[i].DefaultCellStyle.Font = new System.Drawing.Font("Arial", 9, FontStyle.Italic);
                        grillaReportes.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                    }
                }

            }


            txtCantItems.Text = grillaReportes.Rows.Count.ToString();
        }
        
        private void cargarSucursales()
        {
            int idSucursal = Utilidades.Conexion.getIdSucursalConexion();
            dtSucursales = oSucursalN.obtenerSucursales();

            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idsucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;
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
            //this.comboTipoReporte.Items.AddRange(new object[] {
            //"Stock Actual",
            //"Cierre Stock",
            //"Stock Retroactivo",
            //"Cierre Stock 2",
            //"Ingreso - Egreso",
            //"Proyeccion Ventas vs Stock",
            //"Ventas por Producto",
            //"Total Kgs Corte/Compra",
            //"Movimiento/Corte",
            //"Teorico - Real"}); 
            this.comboTipoReporte.Items.AddRange(new object[] {
            "Stock Actual",
            "Cierre Stock",
            "Stock Retroactivo",
            "Proyeccion Ventas vs Stock",
            "Ventas por Producto",
            "Balance Económico"});

            this.Text += Utilidades.Conexion.getSucursalConexion();

            comboTipo.DataSource = oCorteN.obtenerTiposProducto(true);
            comboTipo.DisplayMember = "tipo";
            comboTipo.ValueMember = "tipo";
            comboTipo.SelectedIndex = 0;
        }

        private void cargarComboCierreStock()
        {
            if (comboSucursal.ValueMember != "" && (comboTipoReporte.Text.Equals("Cierre Stock") ||
                comboTipoReporte.Text.Equals("Stock Actual") || comboTipoReporte.Text.Equals("Stock Retroactivo") ||
                comboTipoReporte.Text.Equals("Proyeccion Ventas vs Stock")))
            {
                checkSoloFaltantes.Visible = true;
                checkOcultarColumnas.Visible = true;
                checkOcultarPtoStock.Visible = true;
                DateTime desde = DateTime.Today.Date.AddYears(-10);
                DateTime hasta = DateTime.Today.Date.AddDays(1);

                DataTable dtInicioStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta, null);
                comboInicioStock.DataSource = dtInicioStock;
                comboInicioStock.DisplayMember = "fechaCompra";
                comboInicioStock.ValueMember = "idCompra";
                comboInicioStock.SelectedIndex = dtInicioStock.Rows.Count > 1 && !stockActual && !stockProgresivo && !acumVentas ? 1 : comboInicioStock.SelectedIndex;// dtInicioStock.Rows.Count > 1 ? 1 : -1;

                //setea ultima fecha de cierre para obtener stock actual para comparar en AcumVentas
                fechaUltimoCierreStock = dtInicioStock.Rows.Count > 0 ? Convert.ToDateTime(dtInicioStock.Rows[0]["fechaCompra"]) : fechaUltimoCierreStock;

                fechaDesdeProgresivo.Visible = !(stockProgresivo || comboTipoReporte.Text.Equals("Cierre Stock") || stockActual);
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
                        dtCierreStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), DateTime.Now, DateTime.Now, null);
                        DataRow fechaActual = dtCierreStock.NewRow();
                        fechaActual["idCompra"] = 0;
                        fechaActual["fechaCompra"] = DateTime.Now.AddMinutes(1);//sumo un minuto para q tome las ventas actuales y no esperar un minuto
                        dtCierreStock.Rows.Add(fechaActual);
                    }
                    else
                    {
                        dtCierreStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta, null);
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
                //cargarGrilla(); //se comenta para evitar que se cargue grilla erronemente al seleccionar un filtro mal y perder tiempo de espera

                lblActualizar.Visible = true;
            }
        }

        private void comboCierreStock_SelectedValueChanged(object sender, EventArgs e)
        {
            if (combosCierresCargados)
            {
                //cargarGrilla(); //se comenta para evitar que se cargue grilla erronemente al seleccionar un filtro mal y perder tiempo de espera

                lblActualizar.Visible = true;
            }
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            try
            {
                ////Creating iTextSharp Table from the DataTable data
                //PdfPTable pdfTable = new PdfPTable(grillaReportes.ColumnCount);
                
                // Crea una tabla en el PDF con la misma cantidad de columnas visibles en el DataGridView
                PdfPTable pdfTable = new PdfPTable(grillaReportes.Columns.GetColumnCount(DataGridViewElementStates.Visible));

                pdfTable.DefaultCell.Padding = 3;
                pdfTable.WidthPercentage = 100;
                pdfTable.HorizontalAlignment = Element.ALIGN_LEFT;
                iTextSharp.text.Font fontsubtit = FontFactory.GetFont("Arial", 9);

                string encabezado = "";

                //Adding Header row
                foreach (DataGridViewColumn column in grillaReportes.Columns)
                {
                    if (column.Visible)
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
                            encabezado += " ||| Desde: " + fechaDesdeProgresivo.Text +
                                " | Hasta: " + txtFechaHastaProgresivo.Text + "\n\n";
                        }
                        pdfTable.AddCell(cell);
                    }
                }

                //Adding DataRow
                foreach (DataGridViewRow row in grillaReportes.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (grillaReportes.Columns[cell.ColumnIndex].Visible) // Solo columnas visibles
                        {
                            string valueCell = "";
                            if (cell.ValueType.Name.Equals("Double") || cell.ValueType.Name.Equals("Decimal"))
                            {
                                valueCell = String.Format("{0:0.00}", cell.Value);
                            }
                            else
                            {
                                int cantColVisibles = grillaReportes.Columns.GetColumnCount(DataGridViewElementStates.Visible);
                                valueCell = cell.Value.ToString();
                                valueCell = (valueCell.Length > 6 && cantColVisibles > 10) ? valueCell.Substring(0, 6) : valueCell;
                                
                            }
                            pdfTable.AddCell(new Phrase(valueCell, fontsubtit));
                        }
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
            comboTipoReporteCambiaValor();
        }

        private void comboTipoReporteCambiaValor()
        {
            combosCierresCargados = false;
            checkSoloFaltantes.Visible = checkOcultarColumnas.Visible = checkOcultarPtoStock.Visible = false;
            checkSoloFaltantes.Checked = checkOcultarColumnas.Checked = checkOcultarPtoStock.Checked = false;
            if (comboTipoReporte.Text.Equals("Cierre Stock") ||
                comboTipoReporte.Text.Equals("Stock Actual") ||
                comboTipoReporte.Text.Equals("Stock Retroactivo") ||
                comboTipoReporte.Text.Equals("Proyeccion Ventas vs Stock"))
            {
                stockActual = comboTipoReporte.Text.Equals("Stock Actual");
                acumVentas = comboTipoReporte.Text.Equals("Proyeccion Ventas vs Stock");
                stockProgresivo = comboTipoReporte.Text.Equals("Stock Retroactivo");
                comboInicioStock.Visible = !acumVentas;
                comboCierreStock.Visible = !stockProgresivo && !acumVentas;
                cargarComboCierreStock();
                checkSoloFaltantes.Visible = checkOcultarColumnas.Visible =
                    checkOcultarPtoStock.Visible = !comboTipoReporte.Text.Equals("Proyeccion Ventas vs Stock");
                //checkOcultarColumnas.Visible = true;
                //checkOcultarPtoStock.Visible = true;
            }
            else
            {
                comboInicioStock.Visible = false;
                comboCierreStock.Visible = false;
                fechaDesdeProgresivo.Visible = true;
                txtFechaHastaProgresivo.Visible = true;
            }

            if (comboTipoReporte.Text.Equals("Balance Económico"))
            {
                // Si la condición es verdadera, se oculta la hora mostrando solo la fecha
                fechaDesdeProgresivo.Format = DateTimePickerFormat.Short;  // Muestra solo la fecha (dd/MM/yyyy)
                txtFechaHastaProgresivo.Format = DateTimePickerFormat.Short;  // Muestra solo la fecha (dd/MM/yyyy)
            }
            else
            {
                // Si la condición es falsa, se muestra la fecha y la hora
                fechaDesdeProgresivo.Format = DateTimePickerFormat.Custom;  // Formato personalizado
                fechaDesdeProgresivo.CustomFormat = "dd/MM/yyyy HH:mm:ss";     // Muestra fecha y hora
                txtFechaHastaProgresivo.Format = DateTimePickerFormat.Custom;  // Formato personalizado
                txtFechaHastaProgresivo.CustomFormat = "dd/MM/yyyy HH:mm:ss";     // Muestra fecha y hora
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

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                //comboTipoReporteCambiaValor();
                cargarGrilla();
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void checkSoloFaltantes_CheckedChanged(object sender, EventArgs e)
        {
            string nombreCol = "Falta";
            string consulta = "1 <> 1"; 
            if (checkSoloFaltantes.Checked)
                consulta += " OR " + nombreCol + " = 'X'";
            else
            {
                consulta = "1 = 1";
            }

            arrayRowFilter[2] = consulta;
            aplicarRowFilter();
        }
        private void aplicarRowFilter()
        {
            consultaRowFilter = "";

            for (int i = 0; i < arrayRowFilter.Length; i++)
            {
                string and = (i != arrayRowFilter.Length - 1) ? " AND " : "";
                consultaRowFilter += "( " + arrayRowFilter[i] + " )" + and;
            }

            (grillaReportes.DataSource as DataTable).DefaultView.RowFilter = string.Format(consultaRowFilter);
        }

        private void checkOcultarColumnas_CheckedChanged(object sender, EventArgs e)
        {
            OcultarColDetalle();
        }

        private void OcultarColDetalle()
        {
            if (!combosCierresCargados)
                return;

            for (int i = 0; i < grillaReportes.Columns.Count; i++)
            {
                bool visible = !checkOcultarColumnas.Checked ? true :
                   ((grillaReportes.Columns[i].HeaderText == "Codigo" || grillaReportes.Columns[i].HeaderText == "Corte" ||
                   grillaReportes.Columns[i].HeaderText == "Faltante" || grillaReportes.Columns[i].HeaderText == "Stock Actual"
                   || grillaReportes.Columns[i].HeaderText == "Falta" || grillaReportes.Columns[i].HeaderText == "Pto.Stock") ? true : false);

                grillaReportes.Columns[i].Visible = visible;

                if (grillaReportes.Columns[i].HeaderText == "Falta" ||
                   grillaReportes.Columns[i].HeaderText == "Pto.Stock")
                    OcultarColPtoStock();

                grillaReportes.Columns[i].Visible = (grillaReportes.Columns[i].HeaderText == "promedio") ? false : grillaReportes.Columns[i].Visible;
            }
        }

        private void checkOcultarPtoStock_CheckedChanged(object sender, EventArgs e)
        {
            OcultarColPtoStock();
        }

        private void OcultarColPtoStock()
        {
            if (!combosCierresCargados)
                return;

            for (int i = 0; i < grillaReportes.Columns.Count; i++)
            {
                if (grillaReportes.Columns[i].HeaderText == "Falta" ||
                   grillaReportes.Columns[i].HeaderText == "Pto.Stock")
                    grillaReportes.Columns[i].Visible = !checkOcultarPtoStock.Checked;
            }
        }

        private void exportExcel_Click(object sender, EventArgs e)
        {
            ExportarDataTableAExcel();
        }

        public void ExportarDataTableAExcel()
        {
            try
            {
                // Crear el formulario para pedir el nombre del archivo
                string nombreArchivo = MostrarDialogoNombreArchivo();

                //si es null se aborta la accion
                if (nombreArchivo == null)
                    return;

                // Establecer el contexto de la licencia para evitar la excepción
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

                nombreArchivo += ".xlsx";
                string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
                string rutaArchivo = @ruta + "\\" + nombreArchivo;

                // Verificar si la carpeta existe, si no, crearla
                if (!Directory.Exists(@ruta))
                    Directory.CreateDirectory(@ruta);

                // Crear el archivo de Excel
                FileInfo archivo = new FileInfo(rutaArchivo);

                // Verificar si el archivo ya existe; si es así, eliminarlo
                if (archivo.Exists)
                {
                    archivo.Delete();
                }

                // Crear y llenar el archivo Excel
                using (ExcelPackage excel = new ExcelPackage(archivo))
                {
                    // Crear una hoja de trabajo
                    ExcelWorksheet hoja = excel.Workbook.Worksheets.Add(DateTime.Now.ToShortDateString());

                    int fila = 1; // Fila inicial en Excel
                    int columna = 1; // Columna inicial en Excel

                    // Exportar encabezados visibles
                    foreach (DataGridViewColumn col in grillaReportes.Columns)
                    {
                        if (col.Visible) // Solo columnas visibles
                        {
                            hoja.Cells[fila, columna].Value = col.HeaderText;
                            columna++;
                        }
                    }

                    fila++; // Avanzar a la siguiente fila (datos)
                    // Exportar filas visibles
                    foreach (DataGridViewRow row in grillaReportes.Rows)
                    {
                        if (!row.IsNewRow) // Evitar la fila vacía al final
                        {
                            columna = 1; // Reiniciar columna
                            foreach (DataGridViewColumn col in grillaReportes.Columns)
                            {
                                if (col.Visible) // Solo columnas visibles
                                {
                                    var value = row.Cells[col.Index].Value;

                                    // Verifica si el valor es de tipo DateTime
                                    if (value is DateTime dateTimeValue)
                                    {
                                        // Aplica el formato deseado para las fechas
                                        hoja.Cells[fila, columna].Value = dateTimeValue.ToString("dd/MM/yyyy HH:mm"); // Cambia el formato según necesidad
                                    }
                                    else
                                    {
                                        hoja.Cells[fila, columna].Value = value;
                                    }


                                    //hoja.Cells[fila, columna].Value = row.Cells[col.Index].Value?.ToString();
                                    columna++;
                                }
                            }
                            fila++;
                        }
                    }

                    //#########################3

                    //// Agregar encabezados
                    //for (int i = 0; i < dtGrillaReporte.Columns.Count; i++)
                    //{
                    //    hoja.Cells[1, i + 1].Value = dtGrillaReporte.Columns[i].ColumnName;
                    //}

                    //// Agregar datos
                    //for (int i = 0; i < dtGrillaReporte.Rows.Count; i++)
                    //{
                    //    for (int j = 0; j < dtGrillaReporte.Columns.Count; j++)
                    //    {
                    //        var value = dtGrillaReporte.Rows[i][j];

                    //        // Verifica si el valor es de tipo DateTime
                    //        if (value is DateTime dateTimeValue)
                    //        {
                    //            // Aplica el formato deseado para las fechas
                    //            hoja.Cells[i + 2, j + 1].Value = dateTimeValue.ToString("dd/MM/yyyy HH:mm"); // Cambia el formato según necesidad
                    //        }
                    //        else
                    //        {
                    //            hoja.Cells[i + 2, j + 1].Value = value;
                    //        }

                    //        //hoja.Cells[i + 2, j + 1].Value = dtGrillaReporte.Rows[i][j];
                    //    }
                    //}

                    // Guardar el archivo
                    excel.Save();
                    MessageBox.Show("La exportación se realizó correctamente.\n\n", "", MessageBoxButtons.OK, MessageBoxIcon.Information);

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al exportar lista.\n\n" + ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private string MostrarDialogoNombreArchivo()
        {
            // Crear un formulario para ingresar el nombre
            Form dialogo = new Form
            {
                Width = 400,
                Height = 150,
                Text = "Nombre del archivo",
                StartPosition = FormStartPosition.CenterParent
            };

            Label lblNombre = new Label
            {
                Text = "Ingrese el nombre del archivo:",
                Top = 10,
                Left = 10,
                Width = 360
            };

            TextBox txtNombre = new TextBox
            {
                Top = 40,
                Left = 10,
                Width = 360
            };

            Button btnAceptar = new Button
            {
                Text = "Aceptar",
                Top = 80,
                Left = 150,
                DialogResult = DialogResult.OK
            };

            dialogo.Controls.Add(lblNombre);
            dialogo.Controls.Add(txtNombre);
            dialogo.Controls.Add(btnAceptar);
            dialogo.AcceptButton = btnAceptar;

            if (dialogo.ShowDialog() == DialogResult.OK)
            {
                if (string.IsNullOrEmpty(txtNombre.Text))
                {
                    MessageBox.Show("Debe ingresar un nombre para el archivo a exportar");
                    return null;
                }
                return txtNombre.Text.Trim();
            }

            return null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cargarPersona = "Proveedor";
            buscarPersona();
        }

        private void btnBuscarMarca_Click(object sender, EventArgs e)
        {
            cargarPersona = "Marca";
            buscarPersona();
        }

        private void buscarPersona()
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        ///TODO: recuperar marca, proveedor y tipo en dtcierre stock para poder filtrar <summary>
        /// analizar si conviene armar otro reporte que se llame faltantes o lista a pedir, etc

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            if (idMarca != 0)//validacion para evitar conexion a la BD
            {
                idMarca = 0;
                oMarcaE = null;
                txtMarca.Text = "TODAS";
                lblActualizar.Visible = true;
            }
            btnTodasMarcas.Visible = idMarca != 0;
        }

        private void btnTodosProveedores_Click(object sender, EventArgs e)
        {
            if (idProveedor != 0)//validacion para evitar conexion a la BD
            {
                idProveedor = 0;
                oProveedor = null;
                txtProveedor.Text = "TODOS";
                lblActualizar.Visible = true;
            }
            btnTodosProveedores.Visible = idProveedor != 0;
        }

        private void comboTipo_SelectedIndexChanged(object sender, EventArgs e)
        {
            tipo = string.IsNullOrEmpty(comboTipo.Text) || comboTipo.Text.ToUpper() == "TODOS" ? "" : comboTipo.Text;
            if (combosCierresCargados)
            {
                lblActualizar.Visible = true;
            }
        }

        private void grillaReportes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (grillaReportes.Columns[e.ColumnIndex].Name == "Stock.Cierre" && e.Value != null)
            {
                if (decimal.TryParse(e.Value.ToString(), out decimal valor))
                {
                    e.Value = valor.ToString("N3"); // 3 decimales
                    e.FormattingApplied = true;
                }
            }
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            if (cargarPersona == "Marca")
            {
                this.txtMarca.Text = persona.Identificacion;
                oMarcaE = persona;
                idMarca = persona.idPersona;
            }
            if (cargarPersona == "Proveedor")
            {
                this.txtProveedor.Text = persona.Identificacion;
                oProveedor = persona;
                idProveedor = persona.idPersona;
            }

            btnTodosProveedores.Visible = idProveedor != 0;
            btnTodasMarcas.Visible = idMarca != 0;
            //cargarGrilla();
            //filtarGrilla();
        }
    }
}
