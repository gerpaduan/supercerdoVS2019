using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Presentacion
{
    public partial class formInfoProveedor : formModificarProveedor
    {
        Entidades.Persona oProvModificarE = new Entidades.Persona();
        public formInfoProveedor()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        #region Acciones

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
    
        private void modificar_Click(object sender, EventArgs e)
        {
            cambiarValoresForm();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion


        #region Métodos


        public void cargarCampos(Entidades.Persona oProveedorE)
        {
            txtRazonSocial.Text = oProveedorE.RazonSocial;
            txtOtrosDatos.Text = oProveedorE.OtrosDatos;
            oProvModificarE = oProveedorE;
        }

        //se modifican valores del form
        private void cambiarValoresForm()
        {
            
            //this.txtRazonSocial.ReadOnly = false;
            this.txtOtrosDatos.ReadOnly = false;
            this.btnAceptar.Text = "Guardar";
        
        }
        public void modificarProveedor()
        {
            if (validarCampos())
            {
                Negocio.Persona oProveedorN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);

                oProvModificarE.otrosDatos = this.txtOtrosDatos.Text.Trim();

                oProveedorN.modificarProveedor(oProvModificarE);                
                
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

       

      
    }
}
