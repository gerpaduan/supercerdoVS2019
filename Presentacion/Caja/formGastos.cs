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
            dtGastos = oCierreN.obtenerGastos(oGastoE.Sucursal.idSucursal, oGastoE.IdTipoGasto, txtDescripcion.Text, fechaDesde.Value.Date, fechaHasta.Value.Date);
            grillaGastos.DataSource = dtGastos;
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

            formAddOrEditGasto frmAddOrEditGasto = new formAddOrEditGasto();
            frmAddOrEditGasto.oUsuario = oUsuario;
            frmAddOrEditGasto.asignarForm(this);
            frmAddOrEditGasto.Show();

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
    }
}
