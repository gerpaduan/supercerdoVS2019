using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;

namespace Presentacion
{
    public partial class formVentas : Form
    {
        private bool logueado = false;

        public bool Logueado
        {
            get { return logueado; }
            set { logueado = value; }
        }

        public DataTable dtSucursales;

        public Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        public Negocio.Venta oVentaN = new Negocio.Venta();
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        public DataTable dtVentas;

        bool cargar = false;
        bool soloAnulados = false;
        public formVentas()
        {
            InitializeComponent();
        }

        public void cargarGrilla()
        {
            try
            {
                if (!FormPrincipal.logueado)
                {
                    MessageBox.Show("No está logueado");
                    return;
                }

                if (cargar)
                {
                    lblActualizar.Visible = false;
                    dtVentas = new DataTable();
                    dtVentas = oVentaN.obtenerVentas(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), 
                        Convert.ToInt32(comboUsuario.SelectedValue.ToString()), fechaDesde.Value, fechaHasta.Value, 
                        txtDescripcion.Text.Trim(), soloAnulados);

                    grillaVentas.AutoGenerateColumns = false;
                    grillaVentas.DataSource = null;
                    grillaVentas.DataSource = dtVentas;
                    grillaVentas.Columns["totalKg"].Visible = !soloAnulados;
                    grillaVentas.Columns["totalS"].Visible = !soloAnulados;

                    if (Presentacion.FormPrincipal.logueado == false)
                    {
                        foreach (DataGridViewColumn col in grillaVentas.Columns)
                        {
                            if (col.Name.Equals("totalS"))
                            {
                                col.Visible = false;
                            }
                        }
                    }
                    cargarTotales();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Se produjo un error al cargar las ventas.\n\n"+ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs=0,totalS=0;

            foreach (DataRow venta in dtVentas.Rows)
            {
                totalKgs += float.Parse(venta["totalKg"].ToString());
                totalS += float.Parse(venta["totalS"].ToString());
            }
            txtCantItems.Text = dtVentas.Rows.Count.ToString();
            txtTotalKgs.Text = String.Format("{0:0.000}", totalKgs);
            if (Presentacion.FormPrincipal.logueado)
            {
                txtTotalS.Text = String.Format("{0:0.00}", totalS );
            }
        }

        private void cargarSucursal()
        {
            dtSucursales = new DataTable();
            oSucursalN = new Negocio.Sucursal();
            dtSucursales = oSucursalN.obtenerSucursalesConTodas();
            comboSucursal.DataSource = dtSucursales;
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
        }

        private void infoVenta()
        {
            int idVenta = Convert.ToInt32(grillaVentas.CurrentRow.Cells["idVenta"].Value.ToString());
            
            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formInfoVenta))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("idVentaLabel") && ctrl.Text.Equals(idVenta.ToString()))
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
                formInfoVenta frmInfoVenta = new formInfoVenta();
                frmInfoVenta.idVenta = idVenta;
                frmInfoVenta.frmVentas = this;
                frmInfoVenta.Show();
            }
        }

        private void nuevaVenta()
        {
            if (Application.OpenForms["formNuevaVenta"] != null)
            {
                Application.OpenForms["formNuevaVenta"].Activate();
                Application.OpenForms["formNuevaVenta"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formNuevaVenta frmNuevaVenta = new formNuevaVenta();
                frmNuevaVenta.asigarFormVentas(this);
                frmNuevaVenta.Show();
            }
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            nuevaVenta();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoVenta();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaVentas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoVenta();
        }

        private void formVentas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
                nuevaVenta();
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void formVentas_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                DateTime today = DateTime.Today.Date.AddHours(24);
                fechaHasta.Value = today.AddMilliseconds(-1);
                fechaDesde.Value = today.AddDays(-1);
                cargarSucursal();
                cargarComboVendedor();
                cargar = true;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Utilidades.Util_Form.errorConexionBD(ex.Message));
            }
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos();
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedIndex = 0; 
        }

        private void btnVerTodas_Click(object sender, EventArgs e)
        {
            if (soloAnulados)
            {
                soloAnulados = false;
                btnVerTodas.Text = "Ver &anulados";
            }
            else
            {
                soloAnulados = true;
                btnVerTodas.Text = "Ver &todas";
            }
            cargarGrilla();
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void fechaDesde_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formVentas frmVentaDuplicar = new formVentas();
            frmVentaDuplicar.Show();
        }
    }
}
