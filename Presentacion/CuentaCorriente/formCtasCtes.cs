using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.CuentaCorriente
{
    public partial class formCtasCtes : Form
    {
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        public bool desdePOS = false;
        public Entidades.CierreCaja oCierreCajaE;
        bool formCargado = false;
        public formCtasCtes()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formCtasCtes_Load(object sender, EventArgs e)
        {
            cargarGrilla();
            txtDescripcion.Focus();
            txtDescripcion.Select();
            formCargado = true;
        }

        private void cargarGrilla()
        {
            try
            {
                //si se llama desde POS se oculta el importe del Saldo
                if (!desdePOS && (FormPrincipal.oUserLogueado == null || 
                    !oUsuarioN.tienePermiso(FormPrincipal.oUserLogueado, this.Name, DateTime.Today, 
                    Utilidades.ValoresParametrosMetodos.IdCreadorNulo())))
                {
                    Utilidades.Mensajes.ErrorPermisoAcceso();
                    if (!formCargado)
                        this.Close();
                    return;
                }

                grillaCtasCtes.DataSource = oCtaCteN.obtenerCtasCtes(txtDescripcion.Text,null);
                grillaCtasCtes.AutoGenerateColumns = false;
                grillaCtasCtes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                //formato
                grillaCtasCtes.Columns["Saldo"].DefaultCellStyle.Format = "N2";
                grillaCtasCtes.Columns["Saldo"].Visible = !desdePOS;//si se llama desde POS se oculta el importe del Saldo
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void txtDescripcion_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            try
            {
                formCtaCtePersona frmCtaCtePersona = new formCtaCtePersona();
                frmCtaCtePersona.idPersona = Convert.ToInt32(grillaCtasCtes.CurrentRow.Cells["IdPersona"].Value.ToString());
                frmCtaCtePersona.desdePOS = desdePOS;
                frmCtaCtePersona.oCierreCajaE = oCierreCajaE;
                frmCtaCtePersona.Show();
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

        private void txtDescripcion_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                cargarGrilla();
            }
        }
    }
}
