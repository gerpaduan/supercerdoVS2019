using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Ventas;
using Presentacion.Caja;

namespace Presentacion.CuentaCorriente
{
    public partial class formCtaCtePersona : Form, InterfaceUsuario
    {
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Entidades.Usuario oUsuario;

        public int idPersona;
        Entidades.Persona oPersonaE;
        DateTime fechaDesde = DateTime.Now.AddDays(-30);

        public formCtaCtePersona()
        {
            InitializeComponent();
        }

        private void formCtaCtePersona_Load(object sender, EventArgs e)
        {
            try
            {
                Negocio.Persona oPersonaN = new Negocio.Persona();
                oPersonaE = oPersonaN.findById(idPersona);
                txtPersona.Text = oPersonaE.razonSocial;
                fechaDesdePick.Value = fechaDesde;
                cargarGrilla();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarGrilla()
        {
            try
            {
                grillaMovCtaCte.DataSource = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesdePick.Value);
                grillaMovCtaCte.AutoGenerateColumns = false;

                grillaMovCtaCte.Columns["idPersona"].Visible = false;
                grillaMovCtaCte.Columns["razonSocial"].Visible = false;
                grillaMovCtaCte.Columns["id"].Visible = false;

                grillaMovCtaCte.Rows[0].Selected =false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {
                if (grillaMovCtaCte.CurrentRow == null)
                {
                    MessageBox.Show("Seleccione un registro");
                    return;
                }

                string tabla = grillaMovCtaCte.CurrentRow.Cells["tabla"].Value.ToString();
                int idTabla = Convert.ToInt32(grillaMovCtaCte.CurrentRow.Cells["idTabla"].Value.ToString());

                Entidades.MovCtaCte oMovCtaCteE = new Entidades.MovCtaCte();
                Entidades.MovCtaCte.tablas tablaEnum = oMovCtaCteE.getTablaEnum(tabla);
                switch (tablaEnum)
                {
                    case Entidades.MovCtaCte.tablas.Ventas:
                        infoVenta(idTabla);
                        break;
                    case Entidades.MovCtaCte.tablas.Compras: 
                        Compras.formModificarCompra frmModificarCompra = new Compras.formModificarCompra();
                        frmModificarCompra.cargarParametros(null, idTabla);
                        frmModificarCompra.Show();
                        break;
                    case Entidades.MovCtaCte.tablas.Pagos:
                        Pagos.formAddOrEditPago frmAddOrEditPago = new Presentacion.Pagos.formAddOrEditPago();
                        frmAddOrEditPago.idPago = idTabla;
                        frmAddOrEditPago.Show();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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

        private void infoVenta(int idVenta)
        {

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

        private void menuNuevoPago_Click(object sender, EventArgs e)
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);

            if (oUsuario == null) return;

            if (oUsuario.Admin)
            {
                Pagos.formAddOrEditPago frmAddOrEditPago = new Presentacion.Pagos.formAddOrEditPago();
                frmAddOrEditPago.oPersonaE = oPersonaE;
                frmAddOrEditPago.oUsuario = oUsuario;
                frmAddOrEditPago.Show();
            }
            else
            {
                MessageBox.Show("Debe agregar sus gastos desde la pantalla de Caja Venta.\n");
            }
            oUsuario = null;
        }
        
        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }
    }
}
