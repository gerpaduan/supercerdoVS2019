using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;

namespace Presentacion.Personas
{
    public partial class formPersonas : Form
    {
        Negocio.Persona oPersonaN;

        public formPersonas()
        {
            InitializeComponent();
            cargarGrilla();
        }


        #region Métodos

        public void cargarGrilla()
        {

            oPersonaN = new Negocio.Persona();
            string txtBusqueda = txtBuscar.Text.Trim();
            grillaPersonas.DataSource = null;
            grillaPersonas.AutoGenerateColumns = false;
            grillaPersonas.DataSource = oPersonaN.buscarPersona(txtBusqueda);
            oPersonaN = null;
        }


        public void agregarPersona()
        {
            formNuevaPersona frmNuevaPersona = new formNuevaPersona();
            frmNuevaPersona.obtenerParametros(this);
            frmNuevaPersona.ShowDialog();
            
        }

        public void infoPersona()
        {

            try
            {
                Entidades.Persona oPersonaE = new Entidades.Persona();
                cargarDatos(oPersonaE);

                formInfoPersona frmInfoPersona = new formInfoPersona();
                frmInfoPersona.cargarCampos(oPersonaE);
                frmInfoPersona.ShowDialog();
                cargarGrilla();
            }
            catch (Exception)
            {
                MessageBox.Show("Hubo un error al seleccionar la fila");
            }
        }

        private void cargarDatos(Entidades.Persona oPersonaE)
        {
            oPersonaE.idPersona = Convert.ToInt32(grillaPersonas.CurrentRow.Cells["idPersona"].Value.ToString());
            oPersonaE.razonSocial = grillaPersonas.CurrentRow.Cells["razonSocial"].Value.ToString();
            oPersonaE.otrosDatos = grillaPersonas.CurrentRow.Cells["otrosDatos"].Value.ToString();
            oPersonaE.tipo = grillaPersonas.CurrentRow.Cells["tipo"].Value.ToString();
        }


        #endregion


        #region Acciones

        private void nuevo_Click(object sender, EventArgs e)
        {
            agregarPersona();
        }


        private void btnBuscar_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void txtBuscar_TextChanged(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscar_Click_1(object sender, EventArgs e)
        {
            cargarGrilla();
        }
        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            infoPersona();
        }

        private void grillaPersonas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            infoPersona();
        }
        #endregion

        private void btnCancelar_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formPersonas_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();

        }
    }
}
