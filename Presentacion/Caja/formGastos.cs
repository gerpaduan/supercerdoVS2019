using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace Presentacion.Caja
{
    public partial class formGastos : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.Gasto oGastoE = new Entidades.Gasto();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Usuario oUsuario;

        DataTable dtGastos = null;

        public formGastos()
        {
            InitializeComponent();
        }

        private void formGastos_Load(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            fechaHasta.Value = today.AddDays(1).AddSeconds(-1);
            fechaDesde.Value = today.AddDays(-8); 
            cargarSucursal();
            cargarTipoGasto();
            cargarGrilla();
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oGastoE.Sucursal = oSucursalE;
                cargarGrilla();
            }
        }

        public void cargarGrilla()
        {
            lblActualizar.Visible = false;
            dtGastos = oCierreN.obtenerGastos(oGastoE.Sucursal.idSucursal, oGastoE.IdTipoGasto, txtDescripcion.Text, fechaDesde.Value, fechaHasta.Value);
            grillaGastos.DataSource = dtGastos;
            grillaGastos.Columns["Detalle"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            grillaGastos.Columns["Monto"].DefaultCellStyle.Format = "F2";

            decimal total = 0;
            foreach (DataGridViewRow row in grillaGastos.Rows)
            {
                total = total + Convert.ToDecimal(row.Cells["monto"].Value.ToString());
            }
            txtTotalS.Text = total.ToString("F2");
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oGastoE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void cargarTipoGasto()
        {
            comboTipoGasto.DataSource = oCierreN.obtenerTipoGasto();
            comboTipoGasto.DisplayMember = "tipoGasto";
            comboTipoGasto.ValueMember = "id";
            //comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void comboTipoGasto_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboTipoGasto.ValueMember.Equals(""))
            {
                int idTipoGasto = (int)comboTipoGasto.SelectedValue;
                oGastoE.IdTipoGasto = idTipoGasto;
                cargarGrilla();
            }
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);

            if (oUsuario == null) return;

            if (oUsuario.Admin)
            {
                formAddOrEditGasto frmAddOrEditGasto = new formAddOrEditGasto();
                frmAddOrEditGasto.oUsuario = oUsuario;
                frmAddOrEditGasto.asignarForm(this);
                frmAddOrEditGasto.Show();                 
            }
            else
            {
                MessageBox.Show("Debe agregar sus gastos desde la pantalla de Caja Venta.\n");
            }
            oUsuario = null;
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {                
                int idGasto = Convert.ToInt32(grillaGastos.CurrentRow.Cells["id"].Value.ToString());
                bool formAbierto = false;
                foreach (Form frm in Application.OpenForms)
                {
                    if (frm.GetType() == typeof(formAddOrEditGasto))
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("idGastoLabel") && ctrl.Text.Equals(idGasto.ToString())) //(oUsuario != null && ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oUsuario.User))
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
                    formAddOrEditGasto frmAddOrEditGasto = new formAddOrEditGasto();
                    frmAddOrEditGasto.idGasto = idGasto;
                    frmAddOrEditGasto.asignarForm(this);
                    frmAddOrEditGasto.Show();
                }
                oUsuario = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
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
