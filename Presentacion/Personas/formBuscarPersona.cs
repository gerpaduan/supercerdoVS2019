using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion.Personas
{
    public partial class formBuscarPersona : Form
    {
        Negocio.Persona oPersonaN = new Negocio.Persona();
        bool tabStop = false;
        public formBuscarPersona()
        {
            InitializeComponent();
            cargarGrilla();
            txtBuscar.Focus();
        }

        public void cargarGrilla()
        {
            oPersonaN = new Negocio.Persona();
            grillaPersonas.AutoGenerateColumns = false;

            grillaPersonas.DataSource = oPersonaN.buscarPersona(txtBuscar.Text.Trim());

            oPersonaN = null;
        }

        public void buscarPersona()
        {            
            btnSeleccionar.TabStop = true;
            oPersonaN = new Negocio.Persona();
            string txtBusqueda = txtBuscar.Text.Trim();
            grillaPersonas.AutoGenerateColumns = false;
            grillaPersonas.DataSource = oPersonaN.buscarPersona(txtBusqueda);
            oPersonaN = null;
        }

        public void enviarPersona()
        {
            Entidades.Persona oPersonaE = new Entidades.Persona();

            cargarDatos(oPersonaE);

            InterfacePersona formInterface = this.Owner as InterfacePersona;
            if (formInterface != null)
            {
                formInterface.EnviarPersona(oPersonaE);
            }
            this.Close();
        }

        private void cargarDatos(Entidades.Persona oPersonaE)
        {
            try
            {                
                oPersonaE.idPersona = Convert.ToInt32(grillaPersonas.CurrentRow.Cells[0].Value.ToString());
                oPersonaE.razonSocial = grillaPersonas.CurrentRow.Cells[1].Value.ToString();
                oPersonaE.otrosDatos = grillaPersonas.CurrentRow.Cells[2].Value.ToString();
            }
            catch (Exception)
            {
                MessageBox.Show("No se seleccionó ningún cliente");
            }
        }
        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!tabStop)
            {
                tabIndex();
            }
            if (e.KeyChar == (char)(Keys.Enter))
            {
                e.Handled = true;
                enviarPersona();
            }
        }

        private void tabIndex()
        {
            tabStop = true;
            grillaPersonas.TabStop = true;
            btnSeleccionar.TabStop = true;
            btnCancelar.TabStop = true;
            grillaPersonas.TabIndex = 2;
            btnSeleccionar.TabIndex = 3;
            btnCancelar.TabIndex = 4;
        }

        private void btnSeleccionar_Click(object sender, EventArgs e)
        {
            enviarPersona();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            cargarGrilla();
        }

        private void grillaPersonas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            enviarPersona();
        }

        private void formBuscarPersona_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Tab:
                    tabIndex();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

    }
}
