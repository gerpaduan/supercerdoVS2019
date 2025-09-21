using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Configuration;

namespace Presentacion.Pagos
{
    public partial class formPagos : Form, InterfaceUsuario
    {
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.Pago oPagoE = new Entidades.Pago();
        Entidades.Persona oPersonaE;
        Entidades.Usuario oUsuario;

        DataTable dtPagos = new DataTable();

        DataGridViewRow fila;
        string tramite;
        bool cargar = false;
        bool cerrarForm = false;

        public formPagos()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void nuevo_Click(object sender, EventArgs e)
        {
            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formAddOrEditPago))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("idPagoLabel") && ctrl.Text.Equals("0"))
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
                this.BringToFront();
                Presentacion.Caja.FormLoginVendedor frmLogin = new Presentacion.Caja.FormLoginVendedor();
                frmLogin.ShowDialog(this);

                if (oUsuario == null) return;

                if (oUsuario.Admin)
                {
                    Pagos.formAddOrEditPago frmAddOrEditPago = new Presentacion.Pagos.formAddOrEditPago();
                    frmAddOrEditPago.oPersonaE = oPersonaE;
                    frmAddOrEditPago.desdePOS = false;
                    frmAddOrEditPago.oUsuario = oUsuario;
                    frmAddOrEditPago.Show();
                }
                else
                {
                    MessageBox.Show("Debe agregar sus gastos desde la pantalla de Caja Venta.\n");
                }
            }

            oUsuario = null;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
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
            //CerrarForm = true para evitar que se muestre dos veces el cartel del mensaje
            if (!cerrarForm && (FormPrincipal.oUserLogueado == null ||
                       !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, txtFechaDesde.Value,
                       Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
            {
                Utilidades.Mensajes.ErrorPermisoAcceso();
                if (!cargar)
                    cerrarForm = true;
                return;
            }

            if (cargar)
            {
                string descripcion = txtDescripcion.Text.Trim();

                dtPagos = oCtaCteN.obtenerPagos(descripcion, txtFechaDesde.Value.Date, txtFechaHasta.Value.Date);
                grillaPagos.DataSource = null;
                grillaPagos.DataSource = dtPagos;
                grillaPagos.Columns["importe"].DefaultCellStyle.Format = "N2";
                grillaPagos.Columns["efectivo"].DefaultCellStyle.Format = "N2";
                grillaPagos.Columns["aProveedor"].Visible = false;
            }
        }

        private void modificarPago()
        {
            try
            {
                
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
                    oPagoE.Id = Convert.ToInt32(fila.Cells["Id"].Value.ToString());
                    oPagoE = oCtaCteN.getPagoById(oPagoE.Id);

                    oCtaCteN.eliminarPago(oPagoE);
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
            }
            else
            {
                MessageBox.Show("Asegurese de seleccionar una fila de la grilla.");
            }            
        }

        private void formatearGrilla()
        {
            if (dtPagos.Rows.Count > 0)
            {
                
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


        private void formPagos_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            //leo de App.config fecha Desde
            txtFechaDesde.Value = DateTime.Now;
            cargar = true;
            if (cerrarForm)
            {
                this.Close();
                return;
            }
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

        private void btnSeleccionar_Click_1(object sender, EventArgs e)
        {
            int idPago = Convert.ToInt32(grillaPagos.CurrentRow.Cells["id"].Value.ToString());

            bool formAbierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formAddOrEditPago))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("idPagoLabel") && ctrl.Text.Equals(idPago.ToString()))
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
                formAddOrEditPago frmAddOrEditPago = new formAddOrEditPago();
                frmAddOrEditPago.idPago = idPago;
                frmAddOrEditPago.desdePOS = false;
                frmAddOrEditPago.frmPagos = this;
                frmAddOrEditPago.Show();
            }
        }

        private void menuDuplicar_Click(object sender, EventArgs e)
        {
            formPagos frmPago = new formPagos();
            frmPago.Show();
        }
    }
}
