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

        Negocio.Corte oCorteN = new Negocio.Corte();

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
                if (comboTipoReporte.SelectedIndex == 5)
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

                //Reporte Ingreso-Egreso
                if (comboTipoReporte.SelectedIndex == 1)
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

                if (comboTipoReporte.SelectedIndex == 2)
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

                if (comboTipoReporte.SelectedIndex == 3)
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


                if (comboTipoReporte.SelectedIndex == 4)
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
            if (comboTipoReporte.SelectedIndex == 5)
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
                //DataTable dtTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.CierreStock(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);
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

                                       
                    string totalIngresado, kgsEnEmbutido, totalVendido,  stockReales ;

                    totalIngresado = Convert.ToString(fila["Total Ingresado"]);
                    kgsEnEmbutido = Convert.ToString(fila["Kgs En Embutidos"]);
                    totalVendido = Convert.ToString(fila["Total Vendido"]);
                    stockReales = Convert.ToString(fila["Stock Real"]);

                    decimal stockTeorico, stockReal, faltante;

                    stockTeorico = Convert.ToDecimal(totalIngresado) - Convert.ToDecimal(kgsEnEmbutido) - Convert.ToDecimal(totalVendido);
                    stockReal = Convert.ToDecimal(stockReales);
                    
                    fila["Stock Teorico"] = stockTeorico;

                    faltante = stockTeorico - stockReal;
                    
                    fila["Faltante"] =faltante;

                }

                grillaReportes.DataSource = dtGrillaReporte;
            }


            //StockIngresoEgreso
            if (comboTipoReporte.SelectedIndex == 1)
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
            if (comboTipoReporte.SelectedIndex == 2)
            {
                //DataTable dtTotalPorCortesVendidos = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalPorCortesVendidos(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);

                grillaReportes.DataSource = dtGrillaReporte;
            }
            
            //TotalKgsCortePorCompra
            if (comboTipoReporte.SelectedIndex == 3)
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalKgsCortePorCompra(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);


                grillaReportes.DataSource = dtGrillaReporte;

            }


            //TotalMovimientosPorCorte
            if (comboTipoReporte.SelectedIndex == 4) 
            {
                //DataTable dtReporteTeoricoReal = new DataTable();
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.TotalMovimientosPorCorte(txtDescripcion.Text.Trim(), Convert.ToInt32(comboSucursal.SelectedValue.ToString()), fechaDesde.Value.Date, fechaHasta.Value.Date);


                grillaReportes.DataSource = dtGrillaReporte;

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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void formReporteStock_Load(object sender, EventArgs e)
        {

        }

      

    }
}
