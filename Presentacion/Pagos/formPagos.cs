using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Reportes;
using System.Configuration;

namespace Presentacion.Pagos
{
    public partial class formPagos : Form
    {
        Negocio.Compra oCompraN = new Negocio.Compra();
        Entidades.Pagos oPagoE=new Entidades.Pagos();

        DataTable dtPagos = new DataTable();

        DataGridViewRow fila;
        string tramite;

        bool cargar = false;
        public formPagos()
        {
            InitializeComponent();
            //cargarGrilla();
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            if (Application.OpenForms["formNuevoPago"] != null)
            {

                Application.OpenForms["formNuevoPago"].Activate();
                Application.OpenForms["formNuevoPago"].WindowState = FormWindowState.Normal;


            }
            else
            {

                formNuevoPago frmNuevoPago = new formNuevoPago();
                frmNuevoPago.asignarForm(this);
                frmNuevoPago.Show();

            }
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region Metodos
        public void cargarGrilla()
        {
            if (cargar)
            {

                string tipoTramite = "";
                string descripcion = txtDescripcion.Text.Trim();
                if (comboTipoTramite.Text == "Todos")
                {
                    tipoTramite = "";
                }
                else
                {
                    tipoTramite = comboTipoTramite.Text;
                }

                dtPagos = oCompraN.obtenerPagos(tipoTramite, descripcion, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date);
                grillaPagos.DataSource = null;
                grillaPagos.DataSource = dtPagos;
                foreach (DataGridViewRow filaPago in grillaPagos.Rows)
                {

                    if (filaPago.Cells["Tramite"].Value.ToString() == "Pago")
                    {
                        grillaPagos.Rows[filaPago.Index].DefaultCellStyle.BackColor = Color.FromArgb(209, 227, 254);

                    }
                }
                cargarTotales();
                formatearGrilla();
            }

            
        }

        private void cargarTotales()
        {
            float totalCompra = 0, totalPagos = 0, saldoAnterior=0, saldo=0;
            
        
            foreach (DataRow  fila in dtPagos.Rows)
            {
                
                if (fila["Tramite"].ToString() == "Saldo")
                {
                    saldoAnterior += float.Parse(fila["Importe"].ToString());
                }

                if (fila["Tramite"].ToString() == "Compra")
                {
                    totalCompra += float.Parse(fila["Importe"].ToString());
                }

                if (fila["Tramite"].ToString() == "Pago")
                {
                    totalPagos += float.Parse(fila["Importe"].ToString());
                }

                fila["Saldo"] = totalPagos - totalCompra + saldoAnterior;
                
            }

            txtPagos.Text = totalPagos.ToString();
            txtCompras.Text = totalCompra.ToString();
            txtSaldoAnterior.Text = saldoAnterior.ToString();

            saldo = totalPagos - totalCompra + saldoAnterior;
            if (saldo < 0)
            {
                saldo = saldo * -1; // quita el signo negativo
                this.txtSaldo.BackColor = System.Drawing.Color.Tomato;
            }
            else
	        {
                this.txtSaldo.BackColor = System.Drawing.Color.LightGreen;
	        }
            txtSaldo.Text = Convert.ToString(saldo);
        }

        private void modificarPago()
        {
            try
            {
                cargarFilaSeleccionada();
                if (tramite == "Pago")
                {
                    oPagoE.IdPago = Convert.ToInt32(fila.Cells["Id"].Value.ToString());
                    oPagoE = oCompraN.buscarPago(oPagoE);

                    if (Application.OpenForms["formNuevoPago"] != null)
                    {
                        Application.OpenForms["formNuevoPago"].Activate();
                        Application.OpenForms["formNuevoPago"].WindowState = FormWindowState.Normal;
                    }
                    else
                    {
                        formNuevoPago frmNuevoPago = new formNuevoPago();
                        frmNuevoPago.obtenerParametros(oPagoE, this);
                        frmNuevoPago.Show();

                    }
                }
                else
                {
                    MessageBox.Show("Sólo se pueden seleccionar Pagos para realizar la modificación.");
                }
            }
            catch (Exception)
            {
                throw;
            }
           
        }

        private void eliminarPago()
        {
            cargarFilaSeleccionada();
            if (tramite == "Pago")
            {
                DialogResult resp = MessageBox.Show("Está seguro que desea eliminar el Pago?.", "Eliminar Pago", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                if (resp == DialogResult.Yes)
                {
                    oPagoE.IdPago = Convert.ToInt32(fila.Cells["Id"].Value.ToString());
                    oPagoE = oCompraN.buscarPago(oPagoE);

                    oCompraN.eliminarPago(oPagoE);

                    cargarGrilla();
                }
            }
            else
            {
                MessageBox.Show("Sólo se pueden eliminar Pagos. Asegúrese de seleccionar un Pago.");
            }
            
        }

        private void cargarFilaSeleccionada()
        {
            if (grillaPagos.CurrentRow != null && grillaPagos.Rows.Count >0)
            {
                fila = grillaPagos.CurrentRow;
                tramite = fila.Cells["Tramite"].Value.ToString();
            }
            else
            {
                MessageBox.Show("Asegurese de seleccionar una fila de la grilla.");
            }
            
        }

        private void imprimirReporte()
        {
            ReportesDataSet.dtPagosCompraDataTable dtPagosCompra = new ReportesDataSet.dtPagosCompraDataTable();

            string titulo = "Reporte de Pagos";
            foreach (DataRow fila in dtPagos.Rows)
            {
                DataRow dsFila = dtPagosCompra.NewRow();

                dsFila["NroIdentificacion"] = fila["Nro Identificacion"];
                dsFila["Fecha"] = fila["Fecha"];
                dsFila["RazonSocial"] = fila["Razon Social"];
                dsFila["Tramite"] = fila["Tramite"];
                dsFila["Kgs"] = fila["Kgs"];
                dsFila["PrecioKg"] = fila["Precio/Kg"];

                if (dsFila["Tramite"].ToString()=="Compra")
                {
                    decimal importe = Convert.ToDecimal(fila["Importe"].ToString());
                    dsFila["Importe"] = -1*importe;
                }
                else
                {
                    dsFila["Importe"] = fila["Importe"];
                }

                dsFila["Saldo"] = fila["Saldo"];

                dtPagosCompra.Rows.Add(dsFila);
            }

            Reportes.ReportePagos reporte = new Reportes.ReportePagos();
            FormReportes frmReportes = new FormReportes(reporte, titulo, dtPagosCompra, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date);

            frmReportes.Show();
        
        }

        private void formatearGrilla()
        {
            if (dtPagos.Rows.Count > 0)
            {
                grillaPagos.Columns["Id"].Visible = false;

                //fecha
                System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
                dataGridViewCellStyle1.Format = "d";
                dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle1.NullValue = null;
                grillaPagos.Columns["Fecha"].DefaultCellStyle = dataGridViewCellStyle1;

                //Nro Id
                System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
               
                dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle2.NullValue = null;
                grillaPagos.Columns["Nro Identificacion"].DefaultCellStyle = dataGridViewCellStyle2;

                //Precio/Kg
                System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
                dataGridViewCellStyle4.Format = "N2";
                dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle4.NullValue = null;
                grillaPagos.Columns["Precio/Kg"].DefaultCellStyle = dataGridViewCellStyle4;

                //Importe
                System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3= new System.Windows.Forms.DataGridViewCellStyle();
                dataGridViewCellStyle3.Format = "N2";
                dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
                dataGridViewCellStyle3.NullValue = null;
                grillaPagos.Columns["Importe"].DefaultCellStyle = dataGridViewCellStyle3;

                foreach (DataGridViewRow  filaPago in grillaPagos.Rows)
                {

                    if (filaPago.Cells["Tramite"].Value.ToString() == "Pago")
                    {
                        grillaPagos.Rows[filaPago.Index].DefaultCellStyle.BackColor = Color.FromArgb(209, 227, 254);
                    
                    }
                    if (filaPago.Cells["Tramite"].Value.ToString() == "Saldo")
                    {
                        grillaPagos.Rows[filaPago.Index].DefaultCellStyle.BackColor = Color.Wheat;

                    }
                }

                //Saldo               
                grillaPagos.Columns["Saldo"].DefaultCellStyle = dataGridViewCellStyle3;
            }


            

        }
        #endregion

        private void comboTipoTramite_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {
            //cargarGrilla();
        }

        private void txtFechaDesde_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtFechaHasta_ValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            modificarPago();
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            eliminarPago();
        }

        private void Imprimir_Click(object sender, EventArgs e)
        {
            imprimirReporte();
        }

        private void formPagos_Load(object sender, EventArgs e)
        {
            //leo de App.config fecha Desde
            txtFechaDesde.Value =Convert.ToDateTime(ConfigurationManager.AppSettings["FechaDesdePago"].ToString());
            cargar = true;
            cargarGrilla();
        }

        private void grillaPagos_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            formatearGrilla();
        }

        private void txtDescripcion_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar==Convert.ToChar(Keys.Enter))
            {
                cargarGrilla();
            }
        }
    }
}
