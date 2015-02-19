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
            InitializeComponent();
            cargarGrilla();
        }

        //private void buscarCorte()
        //{
        //    oCorteN = new Negocio.Corte();
        //    string txtBusqueda = txtBuscarCorte.Text.Trim();

        //    grillaCortes.AutoGenerateColumns = false;
        //    grillaCortes.DataSource = oCorteN.buscarCorte(txtBusqueda);

        //}

        private void cargarGrilla()
        {
            oCorteN = new Negocio.Corte();
            grillaCortes.AutoGenerateColumns = false;
           // grillaCortes.DataSource = oCorteN.obtenerEmbutidos(txtBuscarCorte.Text.Trim());
            grillaCortes.DataSource = oCorteN.buscarCorte(txtBuscarCorte.Text.Trim());
        }

        public void enviarCorte()
        {
            Entidades.Corte oCorte = new Entidades.Corte();

            InterfaceEmbutido formInterface = this.Owner as InterfaceEmbutido;
            cargarDatos(oCorte);

            if (formInterface != null)
            {
                formInterface.EnviarEmbutido(oCorte);
            }
            this.Close();
        }

        private void cargarDatos(Entidades.Corte oCorte)
        {
            oCorte.idCorte = Convert.ToInt32(grillaCortes.CurrentRow.Cells[0].Value.ToString());
            oCorte.codigo = Convert.ToInt32(grillaCortes.CurrentRow.Cells["codigo"].Value.ToString());

            oCorte.corte = grillaCortes.CurrentRow.Cells[2].Value.ToString();
            oCorte.tipo = grillaCortes.CurrentRow.Cells["tipo"].Value.ToString();


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

        }
      

       

    }
}
