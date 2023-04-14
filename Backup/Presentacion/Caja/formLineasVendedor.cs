using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using System.Configuration;

namespace Presentacion
{
    public partial class formLineasVendedor : Form
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
        public Entidades.CierreCaja oCierreE;

        public DataTable dtVentas;

        bool cargar = false;
        public formLineasVendedor()
        {
            InitializeComponent();            
        }

        private void formLineasVendedor_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            DateTime today = DateTime.Today.Date.AddHours(24);
            fechaDesde.Value = oCierreE.FechaHoraInicio.Value;
            fechaHasta.Value = oCierreE.FechaHoraCierre != null ? oCierreE.FechaHoraCierre.Value : DateTime.Now;
            cargarSucursal();
            cargarComboVendedor();
            cargar = true;
            cargarGrilla();
        }

        public void cargarGrilla()
        {
            try
            {
                if (cargar)
                {
                    lblActualizar.Visible = false;
                    dtVentas = new DataTable();
                    dtVentas = oVentaN.getAllLineasVenta(Convert.ToInt32(comboSucursal.SelectedValue.ToString()), 
                        Convert.ToInt32(comboUsuario.SelectedValue.ToString()), fechaDesde.Value, fechaHasta.Value, 
                        txtDescripcion.Text.Trim());
                    grillaVentas.DataSource = dtVentas;
                    grillaVentas.Columns["fechaVenta"].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm:ss";
                    grillaVentas.Columns["fechaVenta"].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
                    grillaVentas.Columns["cantKg"].DefaultCellStyle.Format = "F3";
                    grillaVentas.Columns["precioKg"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["totalCorte"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["bonificacion"].DefaultCellStyle.Format = "F2";
                    grillaVentas.Columns["bonificacion"].HeaderText = "Bonif.";
                    grillaVentas.Columns["pesoBalanza"].HeaderText = "Balanza";
                    grillaVentas.Columns["idAnulado"].HeaderText = "Anulado";

                    grillaVentas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;

                    cargarTotales();
                } 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarTotales()
        {
            float totalKgs=0,totalS=0;

            foreach (DataRow venta in dtVentas.Rows)
            {
                totalKgs += float.Parse(venta["cantKg"].ToString());
                totalS += float.Parse(venta["totalCorte"].ToString());

            }
            txtTotalKgs.Text = String.Format("{0:0.00}", totalKgs);
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
            comboSucursal.SelectedValue = oCierreE.Sucursal.idSucursal;
        }

        private void infoVenta()
        {
            try
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
                    frmInfoVenta.Show();
                }
            }
            catch (Exception)
            {
                MessageBox.Show("No se pudo obtener la información de la venta.\nVerifique que el pesaje corresponda a una venta");
            }
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

        private void formLineasVendedor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode==Keys.N)
            {
            }
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void cargarComboVendedor()
        {
            comboUsuario.DataSource = oUsuarioN.obtenerUsuariosConTodos();
            comboUsuario.DisplayMember = "nombre";
            comboUsuario.ValueMember = "id";
            comboUsuario.SelectedValue = oCierreE.UsuarioInicio.Id;
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
            formLineasVendedor frmVentaDuplicar = new formLineasVendedor();
            frmVentaDuplicar.Show();
        }

    }
}
