using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Embutidos;
using Presentacion.Caja;

namespace Presentacion
{
    public partial class formEmbutidos : Form, InterfaceUsuario
    {
        bool esVentaClientes=false;

        public bool EsVentaClientes
        {
            get { return esVentaClientes; }
            set { esVentaClientes = value; }
        }
        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtEmbutidos = new DataTable();

        DataTable dtSucursales;
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        Entidades.Usuario oUsuario;

        bool cargar = false;
        public formEmbutidos()
        {
            InitializeComponent();        
        }

        public void cargarGrilla()
        {
            if (cargar)
            {
                try
                {
                    dtEmbutidos = null;
                    grillaEmbutidos.DataSource = null;
                    grillaEmbutidos.AutoGenerateColumns = true;
                    dtEmbutidos = oCorteN.buscarEmbutido(Convert.ToInt32(comboSucursal.SelectedValue), txtDescripcion.Text.Trim(), fechaDesde.Value, fechaHasta.Value);
                    grillaEmbutidos.DataSource = dtEmbutidos;

                    formatearGrilla();

                    cargarCampoTotal();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar la grilla.(Método: cargarGrilla()).\n\n" + ex.Message);
                }
            }
        }

        private void formatearGrilla()
        {
            for (int index = 0; index < grillaEmbutidos.Rows.Count; index++)
            {
                if (!string.IsNullOrEmpty(grillaEmbutidos["Estado", index].Value.ToString().ToString()))
                {
                    grillaEmbutidos.Rows[index].DefaultCellStyle.BackColor = Color.SandyBrown;
                }
            }
            grillaEmbutidos.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            grillaEmbutidos.Columns["Kgs"].DefaultCellStyle.Format = "F3";
            //formato para columna de fechas
            grillaEmbutidos.Columns["Fecha"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEmbutidos.Columns["Creado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
            grillaEmbutidos.Columns["Actualizado"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
        }

        private void cargarCampoTotal()
        {
            float totalKg = 0;
            foreach (DataRow fila in dtEmbutidos.Rows)
            {
                if (fila["Estado"] == null || fila["Estado"].ToString() == "")
                {
                    totalKg = totalKg + float.Parse(fila["Kgs"].ToString());
                }
            }
            txtTotalKg.Text = Convert.ToString(totalKg);        
        }

        private void informacionEmbutido()
        {
            if (Application.OpenForms["formInfoEmbutido"] != null)
            {
                Application.OpenForms["formInfoEmbutido"].Activate();
                Application.OpenForms["formInfoEmbutido"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formInfoEmbutido frmInfoEmbutido = new formInfoEmbutido();
                frmInfoEmbutido.frmEmbutidos = this;
                frmInfoEmbutido.idEmbutido_ = Convert.ToInt32(grillaEmbutidos.CurrentRow.Cells["Id"].Value.ToString());
                frmInfoEmbutido.Show();
            }            
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            
            if (Application.OpenForms["formIngresoEmbutido"] != null)
            {

                Application.OpenForms["formIngresoEmbutido"].Activate();
                Application.OpenForms["formIngresoEmbutido"].WindowState = FormWindowState.Normal;
            }
            else
            {
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.ShowDialog(this);
                formIngresoEmbutido frmIngresoEmbutido = new formIngresoEmbutido();
                frmIngresoEmbutido.oUsuario = oUsuario;
                frmIngresoEmbutido.frmEmbutidos = this;
                frmIngresoEmbutido.Show();
            }
            oUsuario = null;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void cargarSucursal()
        {
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            //comboSucursal.SelectedValue = Utilidades.Util_Form.idSucursalAppConfig();
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscaProd_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaEmbutidos_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            informacionEmbutido();
        }

        private void modificar_Click(object sender, EventArgs e)
        {
            informacionEmbutido();
        }

        private void formEmbutidos_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (EsVentaClientes)
            {
                this.Text = "Embutidos/Ventas Clientes/Otros";
            }
            DateTime today = DateTime.Today;
            fechaHasta.Value = today.AddDays(1).AddSeconds(-1);
            fechaDesde.Value = today.AddDays(-8);            
            cargarSucursal();
            cargar = true;
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
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaEmbutidos_Sorted(object sender, EventArgs e)
        {
            formatearGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            informacionEmbutido();
        }
    }
}
