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
    public partial class formNuevaPersona : Form
    {
        formPersonas frmPersonas = new formPersonas();


        public formNuevaPersona()
        {
            InitializeComponent();
        }

        public void obtenerParametros(formPersonas formPersonaParam)
        {
            frmPersonas = formPersonaParam;
        }

        private void agregarPersona()
        {
            if (validar())
            {
                try
                {
                    Entidades.Persona oPersonaE = new Entidades.Persona();

                    oPersonaE.razonSocial = txtRazonSocial.Text.Trim();
                    oPersonaE.otrosDatos = txtOtrosDatos.Text.Trim();
                    oPersonaE.tipo = comboTipoPersona.Text;

                    Negocio.Persona oPersonaN = new Negocio.Persona();
                    oPersonaN.agregarPersona(oPersonaE);

                    frmPersonas.cargarGrilla();
                    this.Close();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
                    
            }
        }

        public Boolean validar()
        {
            if (txtRazonSocial.Text.Trim() != "" || comboTipoPersona.Text!="")
            {
                return true;
            }
            else
            {
                MessageBox.Show("Complete el campo Tipo y/o Razon Social. No puede estar vacío.", "Completar los campos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            agregarPersona();
        }


    }
}
