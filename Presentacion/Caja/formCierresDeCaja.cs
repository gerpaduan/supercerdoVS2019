using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Utilidades;

namespace Presentacion.Caja
{
    public partial class formCierresDeCaja : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Usuario oUsuario;

        protected enum tipoCierre { AbrirCaja, CerrarCaja };
        protected tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;

        DataTable dtCierresDeCaja = null;

        public formCierresDeCaja()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formCierresDeCaja_Load(object sender, EventArgs e)
        {
            fechaDesde.Value = DateTime.Today.AddDays(-7);
            this.Text += Utilidades.Conexion.getSucursalConexion();
            cargarSucursal();
            cargarGrilla();
        }

        private void cargarGrilla()
        {
            dtCierresDeCaja = oCierreN.findCierreCaja(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindAll, txtBuscar.Text, fechaDesde.Value);
            grillaCierresDeCaja.DataSource = dtCierresDeCaja;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oCierreE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal-1;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oCierreE.Sucursal = oSucursalE;
                cargarGrilla();
            }
            
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            int? cierreCajaId = Convert.ToInt32(grillaCierresDeCaja.CurrentRow.Cells["id"].Value.ToString());
            cerrarCajaVendedor(cierreCajaId);
        }

        private void cerrarCajaVendedor(int? cierreCajaId)
        {
            // Request status through the Employee object if present.  
            if (cierreCajaId != null)
            {
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.soloActivos = true;
                frmLogin.ShowDialog(this);
                if (oUsuario == null) return;
                if (oUsuario.Admin)
                {
                    formCerrarCaja frmCerrarCaja = new formCerrarCaja();
                    frmCerrarCaja.oUserCierre = oUsuario;
                    frmCerrarCaja.tipoCierreActual = formCerrarCaja.tipoCierre.ModificarCaja;
                    frmCerrarCaja.oCierreE.Id = cierreCajaId.Value;
                    frmCerrarCaja.ShowDialog();
                    cargarGrilla();
                }
                else
                {
                    MessageBox.Show("No tienes permiso");
                }
                oUsuario = null;
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }
    }
}
