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
    public partial class formCajasAbiertas : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Usuario oUsuario;

        protected enum tipoCierre { AbrirCaja, CerrarCaja };
        protected tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;

        DataTable dtCajasAbiertas = null;

        public formCajasAbiertas()
        {
            InitializeComponent();
        }

        private void formCajasAbiertas_Load(object sender, EventArgs e)
        {
            cargarSucursal();
            cargarGrilla();
        }

        private void cargarGrilla()
        {
            dtCajasAbiertas = oCierreN.findCierreCaja(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindOpen, txtBuscar.Text);
            grillaCajasAbiertas.AutoGenerateColumns = false;
            grillaCajasAbiertas.DataSource = dtCajasAbiertas;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
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

        private void grillaCajasAbiertas_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks that are not on button cells.  
            if (e.RowIndex < 0 || e.ColumnIndex !=
                grillaCajasAbiertas.Columns["CerrarCaja"].Index) return;

            // Retrieve the Employee object from the "Assigned To" cell.
            int? cierreCajaId = Convert.ToInt32(grillaCajasAbiertas.Rows[e.RowIndex].Cells["id"].Value.ToString());

            cerrarCajaVendedor(cierreCajaId);
        }

        private void cerrarCajaVendedor(int? cierreCajaId)
        {
            // Request status through the Employee object if present.  
            if (cierreCajaId != null)
            {
                FormLoginVendedor frmLogin = new FormLoginVendedor();
                frmLogin.ShowDialog(this);

                if (oUsuario != null)
                {
                    formCerrarCaja frmCerrarCaja = new formCerrarCaja();
                    frmCerrarCaja.oUserCierre = oUsuario;
                    frmCerrarCaja.oCierreE.Id = cierreCajaId.Value;
                    frmCerrarCaja.ShowDialog();
                    cargarGrilla();
                }
                oUsuario = null;
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void grillaCajasAbiertas_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                e.SuppressKeyPress = true;
                int? cierreCajaId = Convert.ToInt32(grillaCajasAbiertas.SelectedRows[0].Cells["id"].Value.ToString());
                cerrarCajaVendedor(cierreCajaId);
            }
        }
    }
}
