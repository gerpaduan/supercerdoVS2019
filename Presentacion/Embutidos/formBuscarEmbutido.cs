using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Embutidos
{
    public partial class formBuscarEmbutido : Form
    {
        Negocio.Corte oCorteN;

        public formBuscarEmbutido()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            cargarGrilla();
        }

        private void cargarGrilla()
        {
            oCorteN = new Negocio.Corte(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            grillaCortes.AutoGenerateColumns = false;
            grillaCortes.DataSource = oCorteN.buscarCorteSinMaestro(txtBuscarCorte.Text.Trim());
        }

        public void enviarCorte()
        {
            try
            {
                Entidades.Corte oCorte = new Entidades.Corte();

                InterfaceEmbutido formInterface = this.Owner as InterfaceEmbutido;

                int idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells[0].Value.ToString());
                oCorte = oCorteN.findCorteById(idCorte, false);

                if (oCorte == null || oCorte.idCorte == 0)
                {
                    MessageBox.Show("No se seleccionó ningún Producto");
                    return;
                }
                if (formInterface != null)
                {
                    formInterface.EnviarEmbutido(oCorte);
                }
                this.Close();
            }
            catch (Exception)
            {
                this.Close();
            }
        }

        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            enviarCorte();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void grillaCortes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            enviarCorte();
        }

        private void formBuscarEmbutido_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            txtBuscarCorte.Select();
        }

        private void txtBuscarCorte_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue.Equals(13))
            {
                enviarCorte();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
