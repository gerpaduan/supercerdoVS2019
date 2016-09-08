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
        DataTable dtMov;
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
                dtMov = oCtaCteN.getCtaCteByIdPersona(idPersona, fechaDesdePick.Value);

                if (!checkSinRegRepetidos.Checked)
                {
                    int[] aBorrar = new int[dtMov.Rows.Count];
                    for (int i = 0; i < aBorrar.Length; i++)
                    {
                        aBorrar[i] = -1;
                    }

                    for (int filaPrimer = 0; filaPrimer < dtMov.Rows.Count; filaPrimer++)
                    {
                        for (int fila = 0; fila < dtMov.Rows.Count; fila++)
                        {
                            if (aBorrar[filaPrimer] == 1)
                                break;

                            string tablaPrimer = dtMov.Rows[filaPrimer]["tabla"].ToString();
                            string idtablaPrimer = dtMov.Rows[filaPrimer]["idTabla"].ToString();
                            string sucursalPrimer = dtMov.Rows[filaPrimer]["sucursal"].ToString();
                            int idPrimer = Convert.ToInt32(dtMov.Rows[filaPrimer]["id"].ToString());

                            string tabla = dtMov.Rows[fila]["tabla"].ToString();
                            string idtabla = dtMov.Rows[fila]["idTabla"].ToString();
                            string sucursal = dtMov.Rows[fila]["sucursal"].ToString();
                            int id = Convert.ToInt32(dtMov.Rows[fila]["id"].ToString());

                            if (tabla.Equals(tablaPrimer) && idtabla.Equals(idtablaPrimer) &&
                                 sucursal.Equals(sucursalPrimer) && id < idPrimer)
                            {
                                aBorrar[fila] = 1;
                            }
                        }
                    }

                    for (int i = 0; i < aBorrar.Length; i++)
                    {
                        if (aBorrar[i] == 1)
                            dtMov.Rows[i].Delete();
                    }

                    dtMov.AcceptChanges();
                }

                grillaMovCtaCte.DataSource = dtMov;
                grillaMovCtaCte.AutoGenerateColumns = false;

                grillaMovCtaCte.Columns["idPersona"].Visible = false;
                grillaMovCtaCte.Columns["razonSocial"].Visible = false;
                grillaMovCtaCte.Columns["id"].Visible = true;// false;

                grillaMovCtaCte.Rows[0].Selected =false;

                lblActualizar.Visible = false;
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

        private void checkSinRegRepetidos_CheckedChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void fechaDesdePick_KeyDown(object sender, KeyEventArgs e)
        {
            lblActualizar.Visible = true;
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }

        private void fechaDesdePick_ValueChanged(object sender, EventArgs e)
        {
            lblActualizar.Visible = true;
        }
    }
}
