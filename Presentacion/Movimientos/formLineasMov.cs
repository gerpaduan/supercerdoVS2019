using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Movimientos;

using Presentacion.Cortes;
using System.Configuration;

namespace Presentacion
{
    public partial class formLineasMov : formBaseColor
    {
        Negocio.Corte oCorteN = new Negocio.Corte();

        Entidades.Corte oCorteE = new Entidades.Corte();
        Entidades.Sucursal oSucursalOrigen = new Entidades.Sucursal();
        Entidades.Sucursal oSucursalDestino = new Entidades.Sucursal();
        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();

        DataTable dtLineasMov = new DataTable();
        DataTable dtSucursalOrigen = new DataTable();
        DataTable dtSucursalDestino = new DataTable();
 
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;

        int cantServidores = Convert.ToInt32(ConfigurationManager.AppSettings["cantServidores"].ToString());

        public formLineasMov()
        {
            InitializeComponent();
        }
        
        private void formLineasMov_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                cargarSucursales();
                txtFechaDesde.Value = txtFechaHasta.Value.AddDays(-txtFechaHasta.Value.Day - 30);
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                if (Utilidades.Util_Form.errorConexionBD_Return(ex.Message))
                    formLineasMov_Load(null, null);

                this.Close();
            }
        
        }  

        private void cargarSucursales()
        {
            //Suc. Origen
            dtSucursalOrigen = oSucursalN.obtenerSucursalesConTodas();
            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";
            comboSucOrigen.SelectedIndex = 0;//todas            

            //Suc. destino
            dtSucursalDestino = oSucursalN.obtenerSucursalesConTodas();
            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";
            comboSucDestino.SelectedIndex = 0;//Todas
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {

                    lblCargando.Visible = true;
                    lblActualizar.Visible = false;

                    grillaMovimientos.DataSource = null;

                    string sucOrigen, SucDestino;

                    sucOrigen = (Convert.ToInt32(comboSucOrigen.SelectedValue.ToString()) > 0) ? comboSucOrigen.Text : "";
                    SucDestino = (Convert.ToInt32(comboSucDestino.SelectedValue.ToString()) > 0) ? comboSucDestino.Text : "";

                    dtLineasMov = oCorteN.obtenerLineasMov(sucOrigen, SucDestino, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date, txtDescripcion.Text.Trim());
                    grillaMovimientos.DataSource = dtLineasMov;
                    formatearGrilla();

                    lblCargando.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formatearGrilla()
        {
            grillaMovimientos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            grillaMovimientos.Columns["observaciones"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;

            //formato para columna de fechas
            grillaMovimientos.Columns["Fecha Movimiento"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            //grillaCompras.Columns["fechaCompra"].DefaultCellStyle.Format = "ddd dd MMM HH:mm:ss";
            grillaMovimientos.Columns["creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaMovimientos.Columns["actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";

            grillaMovimientos.Columns["Id Origen"].Visible = cantServidores > 1;
            grillaMovimientos.Columns["Estado"].Visible = cantServidores > 1;
        }

        private void infoMovimiento()
        {
            try
            {
                int idMovimiento = Convert.ToInt32(grillaMovimientos.CurrentRow.Cells["Id Movimiento"].Value.ToString());
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formInfoMovimiento))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idMovimientoLabel") && ctrl.Text.Equals(idMovimiento.ToString()))
                            {
                                frm.BringToFront();
                                formAbierto = true;
                                break;
                            }
                        }
                    }
                }
                if (!formAbierto)
                {
                    formInfoMovimiento frmInfoMovimiento = new formInfoMovimiento();
                    frmInfoMovimiento.idMovimiento = idMovimiento;
                    //frmInfoMovimiento.frmMovimiento = this;
                    frmInfoMovimiento.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoMovimiento();
        }
        
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaMovimientos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoMovimiento();
        }

        private void comboSucOrigen_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboSucDestino_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void txtFechaDesde_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }
   
    }
}
