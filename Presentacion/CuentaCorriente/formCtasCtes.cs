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

        public formCtasCtes()
        {
            InitializeComponent();
        }

        private void formCtasCtes_Load(object sender, EventArgs e)
        {
            cargarGrilla();
            txtDescripcion.Focus();
            txtDescripcion.Select();
        }

        private void cargarGrilla()
        {
            try
            {
                grillaCtasCtes.DataSource = oCtaCteN.obtenerCtasCtes(txtDescripcion.Text);
                grillaCtasCtes.AutoGenerateColumns = false;
                grillaCtasCtes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                //formato
                grillaCtasCtes.Columns["Saldo"].DefaultCellStyle.Format = "F2";
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
