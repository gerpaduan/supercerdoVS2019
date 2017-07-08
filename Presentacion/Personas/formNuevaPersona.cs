using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Utilidades;

namespace Presentacion.Personas
{
    public partial class formNuevaPersona : Form
    {
        public  formPersonas frmPersonas;
        Entidades.Persona oPersonaE = new Entidades.Persona();
        Entidades.Persona oPersonaSinMod = new Entidades.Persona();
        Negocio.Persona oPersonaN = new Negocio.Persona();
        public int idPersona = 0;
        bool huboModif = true;
        bool modificar = false;
        bool readOnly = false;

        public formNuevaPersona()
        {
            InitializeComponent();
        }
        
        private void formNuevaPersona_Load(object sender, EventArgs e)
        {
            try
            {
                if (idPersona > 0)
                {
                    oPersonaE = oPersonaN.findById(idPersona);
                    oPersonaSinMod = oPersonaN.findById(idPersona);

                    cargarCampos();
                    readOnly = true;
                    setearPropiedadesForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la carga del formulario.\n\n" + ex.Message);
            }
        }

        private void setearPropiedadesForm()
        {
            this.Text = readOnly ? "Info Persona" : "Modificar Persona";
            this.btnGuardar.Text = readOnly ? "&Modificar" : "&Guardar";

            comboTipoPersona.Enabled = !readOnly && FormPrincipal.logueado;
            txtRazonSocial.ReadOnly = !(this.btnGuardar.Text.Equals("&Guardar") && FormPrincipal.logueado);
            txtBonificacion.ReadOnly = !(this.btnGuardar.Text.Equals("&Guardar") && FormPrincipal.logueado);
            checkCtaCte.Enabled = !readOnly;
            txtOtrosDatos.ReadOnly = readOnly;
        }

        private void cargarCampos()
        {
            comboTipoPersona.Text = oPersonaE.tipo;
            txtRazonSocial.Text = oPersonaE.razonSocial;
            txtBonificacion.Text = oPersonaE.Bonificacion.ToString("F2");
            checkCtaCte.Checked = oPersonaE.CtaCte;
            txtOtrosDatos.Text = oPersonaE.otrosDatos;
        }

        public void obtenerParametros(formPersonas formPersonaParam)
        {
            frmPersonas = formPersonaParam;
        }

        private void addOrEditPersona()
        {
            if (oPersonaE.idPersona > 0 && readOnly)
            {
                readOnly = false;
                setearPropiedadesForm();
                return;
            }

            if (validar())
            {
                try
                {
                    cargarPersona();
                    
                    //comprobar que se modificaron los datos
                    if (!huboModificaciones())
                    {
                        MessageBox.Show("No se realizaron modificaciones.\n\n"+
                            "Presione Cancelar si desea salir sin realizar modificaciones", "No hubo modificaciones",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    oPersonaN.addOrEditPersona(oPersonaE);

                    MessageBox.Show("La persona se ha guardado correctamente.");

                    if (frmPersonas != null)
                        frmPersonas.cargarGrilla();

                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }                    
            }
        }

        private void cargarPersona()
        {
            oPersonaE.razonSocial = txtRazonSocial.Text.Trim();
            oPersonaE.CtaCte = checkCtaCte.Checked;
            oPersonaE.Bonificacion = Utilidades.Util_Form.convertFloat(txtBonificacion.Text, false);
            oPersonaE.otrosDatos = txtOtrosDatos.Text.Trim();
            oPersonaE.tipo = comboTipoPersona.Text;
        }

        private bool huboModificaciones()
        { 
            bool huboModif = true;
            if ((oPersonaE.tipo.Equals(oPersonaSinMod.tipo)) &&
                            (oPersonaE.razonSocial.Equals(oPersonaSinMod.razonSocial)) &&
                            (oPersonaE.CtaCte.Equals(oPersonaSinMod.CtaCte)) &&
                            (oPersonaE.Bonificacion.Equals(oPersonaSinMod.Bonificacion)) &&
                            (oPersonaE.otrosDatos.Equals(oPersonaSinMod.otrosDatos)))
                huboModif = false;

            return huboModif;
        }

        public Boolean validar()
        {
            bool retornar = false;
            //Cargando TextBox para validar
            int nroFilas = 0;
            int nombreTextBox = 1;
            int valorTextBox = 0;
            //string[valor_campo][nombre_textBox]
            string[,] textBoxes = new string[3, 2];
            textBoxes[nroFilas, valorTextBox] = comboTipoPersona.Text == "" ? "" : "tiene_valor";
            textBoxes[nroFilas++, nombreTextBox] = lblTipo.Text;

            textBoxes[nroFilas, valorTextBox] = txtRazonSocial.Text;
            textBoxes[nroFilas++, nombreTextBox] = lblRazonSocial.Text;

            textBoxes[nroFilas, valorTextBox] = txtBonificacion.Text;
            textBoxes[nroFilas++, nombreTextBox] = lblBonificacion.Text;

            if (Util_Form.validarArrayCamposVacios(textBoxes) &&
                Util_Form.validarCampoNumerico(txtBonificacion.Text, lblBonificacion.Text))
                retornar = true;

            return retornar;            
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            DialogResult resp = DialogResult.Yes;
            cargarPersona();
            if (huboModificaciones())
                resp = MessageBox.Show("¿Está seguro de salir sin guardar las modificaciones?", "Se perderán las modificaciones",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (resp.Equals(DialogResult.Yes))
                this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            addOrEditPersona();
        }

    }
}
