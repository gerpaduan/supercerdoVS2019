using Entidades;
using Presentacion.Caja;
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
    public partial class formNuevaPersona : Form, InterfaceUsuario
    {
        public  formPersonas frmPersonas;
        Entidades.Persona oPersonaE = new Entidades.Persona();
        Entidades.Persona oPersonaSinMod = new Entidades.Persona();
        Negocio.Persona oPersonaN = new Negocio.Persona();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        public Entidades.Usuario oUsuario;
        public int idPersona = 0;
        bool huboModif = true;
        bool modificar = false;
        bool readOnly = false;

        DataTable dtIva;
        public bool modifPersonaCajaVenta = false;//se setea en TRUE para poder modificar la persona desde Caja Venta

        public formNuevaPersona()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }
        
        private void formNuevaPersona_Load(object sender, EventArgs e)
        {
            try
            {
                cargarIva();
                txtIdentificacion.Focus();
                txtIdentificacion.Select();
                if (idPersona > 0)
                {
                    oPersonaE = oPersonaN.findById(idPersona);
                    oPersonaSinMod = oPersonaN.findById(idPersona);

                    cargarCampos();
                    readOnly = !modifPersonaCajaVenta; //ver descripcion en la declaracion de la var.
                    setearPropiedadesForm();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en la carga del formulario.\n\n" + ex.Message);
            }
        }

        private void cargarIva()
        {
            dtIva = new DataTable();
            oPersonaN = new Negocio.Persona();
            dtIva = oPersonaN.getIva();
            comboIva.DataSource = dtIva;
            comboIva.DisplayMember = "iva";
            comboIva.ValueMember = "id";
            //comboIva.SelectedValue = 1;
        }

        private void setearPropiedadesForm()
        {
            this.Text = readOnly ? "Info Persona" : "Modificar Persona";
            this.btnGuardar.Text = readOnly ? "&Modificar" : "&Guardar";

            ///validar que la persona a modificar no tenga ventas ni compras a su nombre
            ///si tiene pedir permiso a Admin para modificar datos importantes
            ///
            //valido que es un nueva persona
            bool permitirModificarCampos = oPersonaE == null || oPersonaE.idPersona == 0;
            bool personaTieneCompras_Ventas = true;

            if (!permitirModificarCampos && oUsuario != null)
            {
                personaTieneCompras_Ventas = oPersonaN.personaTieneCompras_Ventas(oPersonaE.idPersona);

                if (personaTieneCompras_Ventas)
                {
                    if (oUsuario.Admin)
                    {
                        personaTieneCompras_Ventas = false;//establezco false para permitir la edicion en los campos
                        MessageBox.Show(
                           "“Al cambiar la Identificación, Razón Social o CUIT puede afectar los registros históricos. (la persona tiene ventas o compras realizadas)”",
                           "Info",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                         );
                    }
                    else
                    {
                        MessageBox.Show(
                           "“Para cambiar la Identificación, Razón Social o CUIT debe comunicarse con un administrador," +
                           " ya que puede afectar los registros históricos. (la persona tiene ventas o compras realizadas)”",
                           "Info",
                           MessageBoxButtons.OK,
                           MessageBoxIcon.Information
                         );
                    }
                }
            }

            //comboTipoPersona.Enabled = !readOnly && FormPrincipal.logueado;
            txtIdentificacion.ReadOnly = personaTieneCompras_Ventas; //!(this.btnGuardar.Text.Equals("&Guardar") && oUsuario.Admin);
            btnCopiarRS.Visible = !readOnly && !personaTieneCompras_Ventas;
            txtRazonSocial.ReadOnly = personaTieneCompras_Ventas; //readOnly;
            checkCtaCte.Enabled = oUsuario!= null && oUsuario.Admin; //!readOnly;
            comboIva.Enabled = !personaTieneCompras_Ventas; //!readOnly;
            txtCuit.ReadOnly = personaTieneCompras_Ventas; //readOnly;
            btnBuscarAfip.Visible = !readOnly && !personaTieneCompras_Ventas;
            txtTelefono.ReadOnly = readOnly;
            txtDomicilio.ReadOnly = readOnly;
            txtCiudad.ReadOnly = readOnly;
            txtOtrosDatos.ReadOnly = readOnly;
        }

        private void cargarCampos()
        {
            //comboTipoPersona.Text = oPersonaE.tipo;
            txtIdentificacion.Text = oPersonaE.Identificacion;
            txtRazonSocial.Text = oPersonaE.razonSocial;
            comboIva.SelectedValue = oPersonaE.IdIva;
            txtCuit.Text = oPersonaE.Cuit;
            txtTelefono.Text = oPersonaE.Telefono;
            txtDomicilio.Text = oPersonaE.Domicilio;
            txtCiudad.Text = oPersonaE.Ciudad;
            txtBonificacion.Text = oPersonaE.Bonificacion.ToString("F2");
            checkCtaCte.Checked = oPersonaE.CtaCte;
            txtOtrosDatos.Text = oPersonaE.otrosDatos;
        }

        public void obtenerParametros(formPersonas formPersonaParam)
        {
            frmPersonas = formPersonaParam;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
        }

        private void addOrEditPersona()
        {
            if (oPersonaE.idPersona > 0 && readOnly)
            {
                ///Se valida que la persona no sea un ID reservado por el sistema
                ///
                if (oPersonaE.idPersona.Equals(Entidades.Parametros.idConsumidorFinal) || oPersonaE.idPersona.Equals(Entidades.Parametros.idIndefinido))
                {
                    MessageBox.Show("La persona seleccionada es reservada por el sistema y no se puede modificar");
                    return;
                }

                if (oUsuario == null)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.soloActivos = true;
                    frmLogin.ShowDialog(this);
                }

                if (oUsuario == null) return;

                //if (!oUsuarioN.tienePermiso(oUsuario, this.Name, oPersonaE.Creado, oUsuario.Id))
                //{
                //    Utilidades.Mensajes.ErrorPermisoEdicion();
                //    oUsuario = null;
                //    return;
                //}

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
            oPersonaE.Identificacion = txtIdentificacion.Text.Trim();
            oPersonaE.razonSocial = txtRazonSocial.Text.Trim();
            oPersonaE.IdIva = comboIva.SelectedValue == null ? 0 : Convert.ToInt32(comboIva.SelectedValue.ToString());
            oPersonaE.Cuit = txtCuit.Text;// Convert.ToInt64(txtCuit.Text.Replace("-", ""));
            oPersonaE.Cuit = txtCuit.Text.Replace("-", "").TrimStart();
            oPersonaE.Telefono = txtTelefono.Text;
            oPersonaE.Domicilio = txtDomicilio.Text;
            oPersonaE.Ciudad = txtCiudad.Text;
            oPersonaE.CtaCte = checkCtaCte.Checked;
            oPersonaE.Bonificacion = Utilidades.Util_Form.convertFloat(txtBonificacion.Text, false);
            oPersonaE.otrosDatos = txtOtrosDatos.Text.Trim();
            //oPersonaE.tipo = "";// comboTipoPersona.Text;
        }

        private bool huboModificaciones()
        { 
            bool huboModif = true;
            if ((oPersonaE.Identificacion.Equals(oPersonaSinMod.Identificacion)) &&
                (oPersonaE.razonSocial.Equals(oPersonaSinMod.razonSocial)) &&
                (oPersonaE.IdIva.Equals(oPersonaSinMod.IdIva)) &&
                (oPersonaE.Cuit.Equals(oPersonaSinMod.Cuit) ||
                oPersonaSinMod.Cuit.Equals(oPersonaE.Cuit.Replace("-", " ").Replace(" ",""))) &&
                (oPersonaE.Telefono.Equals(oPersonaSinMod.Telefono)) &&
                (oPersonaE.Domicilio.Equals(oPersonaSinMod.Domicilio)) &&
                (oPersonaE.Ciudad.Equals(oPersonaSinMod.Ciudad)) &&
                (oPersonaE.CtaCte.Equals(oPersonaSinMod.CtaCte)) &&
                (oPersonaE.Bonificacion.Equals(oPersonaSinMod.Bonificacion)) &&
                (oPersonaE.otrosDatos.Equals(oPersonaSinMod.otrosDatos)))
                huboModif = false;

            return huboModif;
        }

        public Boolean validar()
        {
            //Obtiene los Numeros del String
            string cuitSinGuion = System.Text.RegularExpressions.Regex.Match(txtCuit.Text, @"\d+").Value;

            int idPersonaCuitEncontrado = oPersonaN.existeCuit(txtCuit.Text);
            bool existeCuit = (idPersonaCuitEncontrado > 0 && oPersonaE.idPersona != idPersonaCuitEncontrado);

            if (!string.IsNullOrEmpty(cuitSinGuion) && existeCuit)
            {
                MessageBox.Show("El CUIT ingresado ya existe para un cliente.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (comboIva.SelectedIndex < 0)
            {
                MessageBox.Show("Seleccione un valor en IVA. (consulte al cliente su condición frente al IVA)", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            bool retornar = false;
            //Cargando TextBox para validar
            int nroFilas = 0;
            int nombreTextBox = 1;
            int valorTextBox = 0;
            //string[valor_campo][nombre_textBox]
            string[,] textBoxes = new string[3, 3];
            //textBoxes[nroFilas, valorTextBox] = comboTipoPersona.Text == "" ? "" : "tiene_valor";
            //textBoxes[nroFilas++, nombreTextBox] = lblTipo.Text;

            textBoxes[nroFilas, valorTextBox] = txtIdentificacion.Text;
            textBoxes[nroFilas++, nombreTextBox] = lblNombreIdentif.Text;

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
            //VALIDAR QUE ES ADD OR EDIT
            DialogResult resp = DialogResult.Yes;

            if ((oPersonaE == null || oPersonaE.idPersona == 0) || (oPersonaE.idPersona > 0 && !readOnly))
            {
                cargarPersona();
                if (huboModificaciones())
                    resp = MessageBox.Show("¿Está seguro de salir sin guardar las modificaciones?", "Se perderán las modificaciones",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
            }

            if (resp.Equals(DialogResult.Yes))
                this.Close();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            addOrEditPersona();
        }

        private void btnCopiarRS_Click(object sender, EventArgs e)
        {
            txtRazonSocial.Text = txtIdentificacion.Text;
        }

        private void btnBuscarAfip_Click(object sender, EventArgs e)
        {
            try
            {
                string cuitSinGuiones = txtCuit.Text.Replace("-", "");
                wsAFIPvs2008.formFacturaElectronica formFactElec = new wsAFIPvs2008.formFacturaElectronica();
                formFactElec.loadForm();
                formFactElec.ConsultarDatosContribuyente(cuitSinGuiones);
                txtRazonSocial.Text = txtIdentificacion.Text = formFactElec.razonSocialAfip;
                txtDomicilio.Text = formFactElec.domicilioFiscalAfip;
                txtCiudad.Text = formFactElec.localidadAfip + ", " + formFactElec.provinciaAfip;
                comboIva.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo obtener los datos desde Afip. " + ex.Message);
            }
        }

        private void btnInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                   "⚠️ Aviso:\n\n" +
                   "• Cualquier usuario puede crear nuevas personas.\n" +
                   "• Solo los administradores pueden modificar la Razón Social, CUIT o Identificación" +
                   " de personas que ya tengan ventas o compras registradas.",
                   "Restricción de permisos",
                   MessageBoxButtons.OK,
                   MessageBoxIcon.Information
               );
        }

        private void btnWhatsApp_Click(object sender, EventArgs e)
        {
            Util_Form.enviarWhatsApp(txtTelefono.Text);
        }
    }
}
