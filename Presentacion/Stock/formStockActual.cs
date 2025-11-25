using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Cortes
{
    public partial class formStockActual : Form
    {
        string ultimaConnSelect;
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        DataTable dtConexiones;
        DataTable dtSucursales;

        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Compra oCompraN = new Negocio.Compra();

        DataTable dtGrillaReporte = new DataTable();
        int idSucursal;
        string conexion;
        string[] arrayRowFilter = new string[] { "1 = 1", "1 = 1", "1 = 1", "1 = 1" };
        string consultaRowFilter = "";
        bool combosCierresCargados = false;

        public formStockActual()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formStockActual_Load(object sender, EventArgs e)
        {
            comboConexion.Text = Utilidades.Conexion.connStringActual;
            comboOrdenStock.SelectedIndex = 0;
            cargarComboFechaDesde();
            lblError.Visible = false;
            cargarSucursal();
            cargarGrilla();
            timer1.Start();
        }

        private void cargarSucursal()
        {
            try
            {
                if (FormPrincipal.soyYo)
                {
                    dtConexiones = new DataTable();
                    //oSucursalN = new Negocio.Sucursal();
                    dtConexiones = oSucursalN.obtenerConexiones(null, true);
                    comboConexion.DataSource = dtConexiones;
                    comboConexion.ValueMember = "name";
                    comboConexion.DisplayMember = "nombre";
                    comboConexion.SelectedIndex = 0;
                }
                else
                {
                    idSucursal = Utilidades.Conexion.getIdSucursalConexion();
                    dtSucursales = oSucursalN.obtenerSucursales();

                    comboConexion.DataSource = dtSucursales;
                    comboConexion.DisplayMember = "sucursal";
                    comboConexion.ValueMember = "idsucursal";
                    comboConexion.SelectedValue  = idSucursal;
                }

                combosCierresCargados = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al cargar sucursales\n\n"+
                    "Mensaje: "+ex.Message+
                    "\n\nSource : "+ex.Source+
                    "\n\nStackTrace : "+ex.StackTrace+
                    "\n\nData: "+ex.Data+
                    "\n\nTargetSite : "+ex.TargetSite);                    
            }
        }

        private void cargarComboFechaDesde()
        {
            try
            {
                DateTime desde = DateTime.Today.Date.AddYears(-1);
                DateTime hasta = DateTime.Today.Date.AddDays(1);

                conexion = FormPrincipal.soyYo ? comboConexion.SelectedValue.ToString() : null;
                DataTable dtInicioStock = oCompraN.obtenerCompras(idSucursal, 
                    Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock), 
                    txtDescripcion.Text.Trim(), desde, hasta, conexion);
                comboInicioStock.DataSource = dtInicioStock;
                comboInicioStock.DisplayMember = "fechaCompra";
                comboInicioStock.ValueMember = "idCompra";
                comboInicioStock.SelectedIndex = comboInicioStock.SelectedIndex;// dtInicioStock.Rows.Count > 1 ? 1 : -1;
            }
            catch (Exception)
            {
                lblError.Visible = true;
            }
        }

        private void comboInicioStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void comboOrdenStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            imprimirReporte();
        }

        private void comboConexion_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboConexion.ValueMember.ToString()))
                return;

            lblActualizar.Visible = true;
            idSucursal = FormPrincipal.soyYo ? oSucursalN.getIdSucursalByConexion(comboConexion.SelectedValue.ToString()) : 
                (int)comboConexion.SelectedValue;
            cargarComboFechaDesde();
        }


        private void cargarGrilla()
        {
            lblActualizar.Visible = false;
            lblError.Visible = false;

            string tipoReporte = "Stock Actual";

            try
            {
                grillaReportes.DataSource = null;
                dtGrillaReporte = null;

                dtGrillaReporte = oCorteN.CierreStock(1, txtDescripcion.Text.Trim(), idSucursal,
                    Convert.ToDateTime(comboInicioStock.Text), DateTime.Now, conexion, "", 0, 0);

                grillaReportes.DataSource = dtGrillaReporte;

                System.Drawing.Font fuente = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));

                grillaReportes.Columns["Codigo"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Codigo"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                grillaReportes.Columns["Corte"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                grillaReportes.Columns["Tot.INGR"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.BackColor = Color.PaleGreen;
                grillaReportes.Columns["Tot.EGR"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.BackColor = Color.LightBlue;
                grillaReportes.Columns["Stock.Cierre"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Faltante"].DefaultCellStyle.Font = fuente;

                grillaReportes.Columns["Stock.Un"].DefaultCellStyle.BackColor = Color.LightBlue;
                grillaReportes.Columns["Stock.Un"].DefaultCellStyle.Font = fuente;
                grillaReportes.Columns["Stock.Un"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

                grillaReportes.Columns["promedio"].Visible = false;

                grillaReportes.Columns["DIF"].Visible = false;
                grillaReportes.Columns["Stock.Cierre"].Visible = false;
                grillaReportes.Columns["Faltante"].HeaderText = "Stock.Kgs";
                grillaReportes.Columns["Faltante"].DefaultCellStyle.BackColor = Color.PaleGreen;
                grillaReportes.Columns["Faltante"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;


                grillaReportes.Columns["Falta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                grillaReportes.Columns["Pto.Stock"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

                for (int i = 0; i < grillaReportes.Columns.Count; i++)
                {
                    bool mostrar = false;

                    switch (grillaReportes.Columns[i].Name)
                    {
                        case "Codigo":
                            mostrar = true;
                            break;
                        case "Corte":
                            mostrar = true;
                            break;
                        case "Faltante":
                            mostrar = checkColStockKg.Checked;
                            break;
                        case "Stock.Un":
                            mostrar = true;
                            break;
                        case "Falta":
                            mostrar = true;
                            break;
                        case "Pto.Stock":
                            mostrar = true;
                            break;
                        default:
                            break;
                    }
                    grillaReportes.Columns[i].Visible = mostrar;  

                }                
                txtUltimaActualizacion.Text = DateTime.Now.ToShortTimeString();

                SoloFaltantes();
                OcultarColPtoStock();
            }
            catch (Exception)
            {
                lblError.Visible = true;
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

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formStockActual frmStockActual = new formStockActual();
            frmStockActual.Show();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void imprimirReporte()
        {
            try
            {
                //Reporte Stock Actual
                Ticket.formTipoTicket tipoTicket = new Presentacion.Ticket.formTipoTicket();
                string fecha = DateTime.Today.ToShortDateString();
                fecha += " " + DateTime.Now.ToShortTimeString();
                tipoTicket.stockActual(comboInicioStock.Text, fecha, grillaReportes);
                return;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void checkActualizacionAuto_CheckedChanged(object sender, EventArgs e)
        {
            if (checkActualizacionAuto.Checked)
                timer1.Start();
            else
                timer1.Stop();
        }

        private void checkColStockKg_CheckedChanged(object sender, EventArgs e)
        {
            if(grillaReportes.Rows.Count > 0)
                grillaReportes.Columns["Faltante"].Visible = checkColStockKg.Checked;
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void checkOcultarPtoStock_CheckedChanged(object sender, EventArgs e)
        {
            OcultarColPtoStock();
        }

        private void OcultarColPtoStock()
        {
            for (int i = 0; i < grillaReportes.Columns.Count; i++)
            {
                if (grillaReportes.Columns[i].HeaderText == "Falta" ||
                   grillaReportes.Columns[i].HeaderText == "Pto.Stock")
                    grillaReportes.Columns[i].Visible = !checkOcultarPtoStock.Checked;
            }
        }
        private void checkSoloFaltantes_CheckedChanged(object sender, EventArgs e)
        {
            SoloFaltantes();
        }

        private void SoloFaltantes()
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
    }
}
