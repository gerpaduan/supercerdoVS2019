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
    public partial class formNuevoProveedor : formBaseColor
    {
        public formNuevoProveedor()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

         
        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (validar())
            { 
                agregarProveedor();
                
                this.Close();
            }
        }

        #region Métodos

        public void agregarProveedor()
        {
            Entidades.Persona oProveedorE = new Entidades.Persona();

            oProveedorE.razonSocial = txtRazonSocial.Text.Trim();
            oProveedorE.otrosDatos = txtOtrosDatos.Text.Trim();
            oProveedorE.tipo = "Proveedor";

            Negocio.Persona oProveedorN = new Negocio.Persona(FormPrincipal.EmpresaSTATIC, FormPrincipal.ParametrosCTX);
            oProveedorN.agregarPersona(oProveedorE);
        }

        public Boolean validar()
        {
            if (txtRazonSocial.Text.Trim() != "")
            {
                return true;
            }
            else
            {
                MessageBox.Show("Complete el campo Razon Social. No puede estar vacío.","Completar Razon Social", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        
        #endregion

    }
}
