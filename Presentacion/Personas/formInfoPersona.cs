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
    public partial class formInfoPersona : Form
    {
        Entidades.Persona oPersonaModificarE = new Entidades.Persona();

        Negocio.Persona oPersonaN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

        public formInfoPersona()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
            eliminar.Enabled = FormPrincipal.logueado;//solo permiter borrar si es admin
            txtRazonSocial.Enabled = FormPrincipal.logueado;//solo permite modif nombre si es admin
        }

        #region Métodos


        public void cargarCampos(Entidades.Persona oPersonaE)
        {
            comboTipoPersona.Text = oPersonaE.tipo;
            txtRazonSocial.Text = oPersonaE.RazonSocial;
            txtOtrosDatos.Text = oPersonaE.OtrosDatos;
            oPersonaModificarE = oPersonaE;
            
        }

        //se modifican valores del form
        private void cambiarValoresForm()
        {

            //this.txtRazonSocial.ReadOnly = false;
            this.comboTipoPersona.Enabled = true;
            this.txtRazonSocial.ReadOnly = false;
            this.txtOtrosDatos.ReadOnly = false;
            this.btnAceptar.Text = "Guardar";

        }
        public void modificarProveedor()
        {
            if (validarCampos())
            {
                oPersonaModificarE.otrosDatos = this.txtOtrosDatos.Text.Trim();
                oPersonaModificarE.razonSocial = this.txtRazonSocial.Text.Trim();
                oPersonaModificarE.tipo = this.comboTipoPersona.Text;

                oPersonaN.modificarProveedor(oPersonaModificarE);

            }
        }

        public void EliminarPersona()
        {
            DialogResult resp = MessageBox.Show("Está seguro que desea eliminar la persona?.", "Eliminar persona", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            if (resp==DialogResult.Yes)
            {
                oPersonaN.eliminarPersona(oPersonaModificarE);

                this.Close();
            }
        
        }


        public Boolean validarCampos()
        {
            if (txtRazonSocial.Text.Trim() != "")
            {
                return true;
            }
            else
            {
                MessageBox.Show("Complete el campo Razon Social. No puede estar vacío.", "Completar Razon Social", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region Acciones

        private void modificar_Click(object sender, EventArgs e)
        {
            cambiarValoresForm();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            if (this.txtOtrosDatos.ReadOnly == false)
            {
                modificarProveedor();
                this.Close();
            }
            else
            {
                this.Close();
            }
        }

        private void eliminar_Click(object sender, EventArgs e)
        {
            EliminarPersona();
        }

        #endregion

        

      

    }
}
