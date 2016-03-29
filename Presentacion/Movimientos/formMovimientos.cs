using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Movimientos;
using Presentacion.Reportes;
using Presentacion.Cortes;
using System.Configuration;

namespace Presentacion
{
    public partial class formMovimientos : formBaseColor
    {
        Negocio.Corte oCorteN = new Negocio.Corte();

        Entidades.Corte oCorteE = new Entidades.Corte();
        Entidades.Sucursal oSucursalOrigen = new Entidades.Sucursal();
        Entidades.Sucursal oSucursalDestino = new Entidades.Sucursal();
        Entidades.Movimiento oMovimientoE = new Entidades.Movimiento();

        DataTable dtMovimientos = new DataTable();
        DataTable dtSucursalOrigen = new DataTable();
        DataTable dtSucursalDestino = new DataTable();
 
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        bool cargar = false;

        public formMovimientos()
        {
            InitializeComponent();
            cargarSucursales();
            txtFechaDesde.Value = txtFechaHasta.Value.AddDays(-txtFechaHasta.Value.Day - 30);
            cargar = true;
            cargarGrilla();
        }

        private void cargarSucursales()
        {
            //Suc. Origen
            dtSucursalOrigen = oSucursalN.obtenerSucursales();

            DataRow nuevaFilaOrigen = dtSucursalOrigen.NewRow();

            nuevaFilaOrigen[0] = 3;
            nuevaFilaOrigen[1] = "Todas";

            dtSucursalOrigen.Rows.Add(nuevaFilaOrigen);

            comboSucOrigen.DataSource = dtSucursalOrigen;
            comboSucOrigen.DisplayMember = "sucursal";
            comboSucOrigen.ValueMember = "idSucursal";
            comboSucOrigen.SelectedIndex = 2;//todas
            

            //Suc. destino
            dtSucursalDestino = oSucursalN.obtenerSucursales();

            DataRow nuevaFilaDestino = dtSucursalDestino.NewRow();

            nuevaFilaDestino[0] = 3;
            nuevaFilaDestino[1] = "Todas";

            dtSucursalDestino.Rows.Add(nuevaFilaDestino);

            comboSucDestino.DataSource = dtSucursalDestino;
            comboSucDestino.DisplayMember = "sucursal";
            comboSucDestino.ValueMember = "idSucursal";
            comboSucDestino.SelectedIndex = 2;//Todas

        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {

                    grillaMovimientos.DataSource = null;

                    string sucOrigen = comboSucOrigen.Text, SucDestino = comboSucDestino.Text;

                    if (sucOrigen == "Todas")
                    {
                        sucOrigen = "";
                    }
                    if (SucDestino == "Todas")
                    {
                        SucDestino = "";
                    }
                    dtMovimientos = oCorteN.obtenerMovimientos(sucOrigen, SucDestino, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date, txtDescripcion.Text.Trim());
                    grillaMovimientos.DataSource = dtMovimientos;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void infoMovimiento()
        {
            cargarMovimiento();
            
            if (Application.OpenForms["formInfoMovimiento"] != null)
            {

                Application.OpenForms["formInfoMovimiento"].Activate();
                Application.OpenForms["formInfoMovimiento"].WindowState = FormWindowState.Normal;

            }
            else
            {

                formInfoMovimiento frmInfoMovimiento = new formInfoMovimiento();
                frmInfoMovimiento.obtenerParametros(this,oMovimientoE);
                frmInfoMovimiento.Show();

            }
        }

        private void cargarMovimiento()
        {
            int idMovimiento = Convert.ToInt32(grillaMovimientos.CurrentRow.Cells["Id Movimiento"].Value.ToString());

            oMovimientoE = oCorteN.cargarMovimiento(idMovimiento);

        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            
            if (Application.OpenForms["formNuevoMovimiento"] != null)
            {

                Application.OpenForms["formNuevoMovimiento"].Activate();
                Application.OpenForms["formNuevoMovimiento"].WindowState = FormWindowState.Normal;


            }
            else
            {

                formNuevoMovimiento frmNuevoMovimiento = new formNuevoMovimiento();
                frmNuevoMovimiento.obtenerForm(this);
                frmNuevoMovimiento.Show();

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

        private void pnlBuscar_Paint(object sender, PaintEventArgs e)
        {

        }

        private void comboSucDestino_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void comboSucOrigen_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboSucDestino_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formMovimientos_Load(object sender, EventArgs e)
        {
        }

        private void Reporte_Click(object sender, EventArgs e)
        {
            cargarReporte();
        }

        private void cargarReporte()
        {
            cargarMovimiento();
            int tipoReporte = 5;//nro perteneciente al reporte de los movimientos
            formReporteStock frmReporte = new formReporteStock();
            frmReporte.obtenerParametros(oMovimientoE.SucursalDestino.idSucursal, oMovimientoE.FechaMovimiento, oMovimientoE.FechaMovimiento, tipoReporte, oMovimientoE.IdMovimiento.ToString());
            frmReporte.Show();
        }

        private void actualizar_Click(object sender, EventArgs e)
        {
            try
            {
                string ruta = ConfigurationManager.AppSettings["rutaActualizarMovimientos"].ToString();
                System.Diagnostics.Process.Start(ruta);
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrió un error al actualizar los movimientos.\n\n" + ex.Message);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }     
    }
}
