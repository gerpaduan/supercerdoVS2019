using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Cortes
{
    public partial class formBuscarCorte : formBaseColor
    {
        
        Negocio.Corte oCorteN;
        public formBuscarCorte()
        {
            InitializeComponent();
            cargarGrilla();
            txtBuscarCorte.Focus();
            
        }

        private void buscarCorte()
        {
            oCorteN = new Negocio.Corte();
            string txtBusqueda = txtBuscarCorte.Text.Trim();

            grillaCortes.AutoGenerateColumns = false;
            grillaCortes.DataSource = oCorteN.buscarCorteSinMaestro(txtBusqueda);                        
        }

        private void cargarGrilla()
        {
            buscarCorte();
            //oCorteN = new Negocio.Corte();
            //grillaCortes.AutoGenerateColumns = false;
            //grillaCortes.DataSource = oCorteN.obtenerCortes();

            
        }

        public void enviarCorte()
        {
            Entidades.Corte oCorte = new Entidades.Corte();

            InterfaceCorte formInterface = this.Owner as InterfaceCorte;
            try
            {
                cargarDatos(oCorte);

                if (formInterface != null)
                {
                    formInterface.EnviarCorte(oCorte);
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("La descripción ingresada no corresponde a ningún corte.","El corte no existe",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
        }

        private void cargarDatos(Entidades.Corte oCorte)
        {
            
            oCorte.idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells[0].Value.ToString());
            oCorte.codigo =  Convert.ToInt32(grillaCortes.CurrentRow.Cells["codigo"].Value.ToString());
            
            oCorte.corte = grillaCortes.CurrentRow.Cells[2].Value.ToString();
          //  oCorte.tipo = grillaCortes.CurrentRow.Cells["tipo"].Value.ToString();
        
        }
        private void btnBuscarCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            enviarCorte();
        }

        private void grillaCortes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            enviarCorte();
        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;

                enviarCorte();
            }

        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formBuscarCorte_Load(object sender, EventArgs e)
        {
            txtBuscarCorte.Select();
        }

        

    }

    
}
