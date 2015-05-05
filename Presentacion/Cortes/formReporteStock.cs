using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Reportes;

namespace Presentacion.Cortes
{
    public partial class formReporteStock : Form
    {
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        DataTable dtSucursales;

        DataTable dtCierreStock;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Compra oCompraN = new Negocio.Compra();

        DataTable dtGrillaReporte = new DataTable();

        public formReporteStock()
        {
            InitializeComponent();
            cargarSucursales();
            cargarGrilla();
        }

        public void obtenerParametros(int sucursalParam, DateTime fechaDesdeParam, DateTime fechaHastaParam, int tipoReporteParam, string textoParam)
        {
            comboSucursal.SelectedIndex = sucursalParam-1;
            fechaDesde.Value = fechaDesdeParam;
            fechaHasta.Value = fechaHastaParam;
            comboTipoReporte.SelectedIndex = tipoReporteParam;
            txtDescripcion.Text = textoParam;

            cargarGrilla();
        
        }

        private void imprimirReporte()
        {
            try
            {
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
                //Reporte Cierre Stock
                if (comboTipoReporte.Text == "Cierre Stock")
                {
                    ReportesDataSet.dtCierreStockDataTable dtCierreStock = new ReportesDataSet.dtCierreStockDataTable();

                    string titulo = "Reporte Cierre Stock";
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

            //reporteTeoricoReal
            if (comboTipoReporte.SelectedIndex == 6)
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
            if (comboTipoReporte.Text == "Cierre Stock")
            {
                try
                {
                    if (!comboInicioStock.Enabled || !comboCierreStock.Enabled || comboInicioStock.Text.Equals("") || comboCierreStock.Text.Equals(""))
                    {
                        //MessageBox.
                    }
                    else
                    {
                        grillaReportes.DataSource = null;
                        dtGrillaReporte = null;

                        dtGrillaReporte = oCorteN.CierreStock(1, txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                            Convert.ToDateTime(comboInicioStock.Text), Convert.ToDateTime(comboCierreStock.Text));
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

                        Font fuente = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                        grillaReportes.Columns["Codigo"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Corte"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                        grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                        grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.BackColor = Color.LightBlue;
                        grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.Font = fuente;
                        grillaReportes.Columns["Faltante"].DefaultCellStyle.Font = fuente;

                        //fuente = new System.Drawing.Font("Microsoft Sans Serif", 9.50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                        //grillaReportes.AlternatingRowsDefaultCellStyle.Font = fuente;
                    }
                }
                catch (Exception ex)
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
            if (comboTipoReporte.SelectedIndex == 2)
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

            //TotalPorCortesVendidos
            if (comboTipoReporte.SelectedIndex == 3)
            {
                //DataTable dtTotalPorCortesVendidos = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;
                
                //si está logueado
                if (Presentacion.FormPrincipal.logueado)
                {
                    dtGrillaReporte = oCorteN.TotalPorCortesVendidos(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);
                }
                grillaReportes.DataSource = dtGrillaReporte;
            }
            
            //TotalKgsCortePorCompra
            if (comboTipoReporte.SelectedIndex == 4)
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalKgsCortePorCompra(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()),
                    fechaDesde.Value.Date, fechaHasta.Value.Date);


                grillaReportes.DataSource = dtGrillaReporte;

            }


            //TotalMovimientosPorCorte
            if (comboTipoReporte.SelectedIndex == 5) 
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalMovimientosPorCorte(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);


                grillaReportes.DataSource = dtGrillaReporte;
                //grillaReportes.AlternatingRowsDefaultCellStyle.BackColor = grillaReportes.AlternatingRowsDefaultCellStyle.BackColor.Name{"0"};
                Font fuente = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                grillaReportes.Columns["Codigo"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Corte"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Total Unidades"].DefaultCellStyle.BackColor = Color.PaleGreen;
                grillaReportes.Columns["Total Unidades"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Total Kgs"].DefaultCellStyle.BackColor = Color.LightBlue;
                grillaReportes.Columns["Total Kgs"].DefaultCellStyle.Font = fuente;

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
            cargarComboCierreStock();
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
        }

        private void cargarComboCierreStock()
        {
            if (comboSucursal.ValueMember != "")
            {
                DateTime desde = DateTime.Today.Date.AddYears(-10);
                DateTime hasta = DateTime.Today.Date.AddDays(1);

                if (comboInicioStock.SelectedIndex == -1)
                {
                    DataTable dtInicioStock = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta);
                    comboInicioStock.DataSource = dtInicioStock;
                    comboInicioStock.DisplayMember = "fechaCompra";
                    comboInicioStock.ValueMember = "idCompra";
                    comboInicioStock.SelectedIndex = dtInicioStock.Rows.Count > 1 ? 1 : -1;                    
                }
                if (comboCierreStock.SelectedIndex == -1)
                {                    
                    comboCierreStock.DataSource = oCompraN.obtenerCompras(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), txtDescripcion.Text.Trim(), desde, hasta);
                    comboCierreStock.DisplayMember = "fechaCompra";
                    comboCierreStock.ValueMember = "idCompra";
                }
            }
        }

        private void comboTipoReporte_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTipoReporte.Text.Equals("Cierre Stock"))
            {
                comboInicioStock.Visible = true;
                comboCierreStock.Visible = true;
                cargarComboCierreStock();
            }
            else
            {
                comboInicioStock.Visible = false;
                comboCierreStock.Visible = false;
            }
        }

      

    }
}
