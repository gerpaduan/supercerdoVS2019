using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;
using Utilidades;
using Presentacion.Caja;

namespace Presentacion.Pagos
{
    public partial class formAddOrEditPago : Form, InterfaceUsuario, InterfacePersona
    {
        public  formPagos frmPagos;
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        public Entidades.Persona oPersonaE = new Entidades.Persona();
        Entidades.Pago oPagoE = new Entidades.Pago();
        Entidades.Pago oPagoSinMod = new Entidades.Pago();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        public Entidades.Usuario oUsuario;
        
        public int idPago = 0;
        bool comboSucursalCargada = false;
        bool huboModif = true;
        bool modificar = false;
        bool readOnly = false;
        bool ultimaValidacion = true;//valida que los ingresos estén correctos antes de ingresar datos al DB

        public formAddOrEditPago()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formNuevoPago_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion(); 
            try
            {
                bool closeForm = false;
                if (idPago == 0 && oUsuario == null) closeForm = true;

                if (!closeForm)
                {
                    cargarSucursal();
                    if (idPago > 0)
                    {
                        oPagoE = oCtaCteN.getPagoById(idPago);
                        oPersonaE = oPagoE.Persona;
                        oPagoSinMod = oCtaCteN.getPagoById(idPago);
                        cargarCampos();
                        readOnly = true;
                        setearPropiedadesForm();
                        idPagoLabel.Text = idPago.ToString();//asigno id para identificar el formulario al llamar
                    }

                    txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
                    txtPersona.Text = oPersonaE != null ? oPersonaE.razonSocial : "";
                    //se valida que sea Admin para cambiar de sucursal
                    comboSucursal.Visible = ((oUsuario != null && oUsuario.Admin) || FormPrincipal.logueado);
                    txtSucursal.Visible = !comboSucursal.Visible;
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en evento Load()\n" + ex.Message);
            }
        }

        private void setearPropiedadesForm()
        {
            this.Text = readOnly ? "Info Pago" : "Modificar Pago";
            this.btnGuardar.Text = readOnly ? "&Modificar" : "&Guardar";
            txtSucursal.Visible = readOnly || !((oUsuario != null && oUsuario.Admin) || FormPrincipal.logueado);
            comboSucursal.Visible = !txtSucursal.Visible;
            txtSucursal.Text = comboSucursal.Text;
            btnBuscarProv.Visible = !readOnly;
            txtFechaPago.Enabled = !readOnly;
            comboTipoPago.Enabled = !readOnly;
            //txtTipoEgresoCaja.Visible = readOnly;
            //txtTipoEgresoCaja.Text = comboTipoEgresoCaja.Text;
            comboTipoPago.Enabled = !readOnly;
            txtNroRecibo.ReadOnly = readOnly;
            txtImporte.ReadOnly = readOnly;
            txtBanco.ReadOnly = readOnly;
            txtNroCheque.ReadOnly = readOnly;
            txtTitular.ReadOnly = readOnly;
            txtObservaciones.ReadOnly = readOnly;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oPagoE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;

            txtSucursal.Text = comboSucursal.Text;
            comboSucursalCargada = true;
        }

        private void btnBuscarProv_Click(object sender, EventArgs e)
        {
            formBuscarPersona frmBuscarProv = new formBuscarPersona();
            frmBuscarProv.Show(this);
        }

        //comunicación con interface
        public void EnviarPersona(Entidades.Persona persona)
        {
            oPersonaE = persona;
            this.txtPersona.Text = oPersonaE.razonSocial;
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
            this.txtUsuario.Text = oUsuario.Nombre;
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            addOrEditPago();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                this.Close();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        #region Métodos

        private void cargarCampos()
        {
            txtIdPago.Text = oPagoE.Id.ToString();
            txtPersona.Text = oPagoE.Persona != null ? oPagoE.Persona.razonSocial : "";
            txtNroRecibo.Text = oPagoE.NroRecibo;
            checkAProveedor.Checked = oPagoE.AProveedor;
            txtFechaPago.Value = oPagoE.Fecha.Year > 1000 ? oPagoE.Fecha : DateTime.Now;
            comboTipoPago.Text = oPagoE.FormaPago != null ? oPagoE.FormaPago : "";
            txtImporte.Text = oPagoE.Importe.ToString("F2");
            txtBanco.Text = oPagoE.Banco;
            txtNroCheque.Text = oPagoE.NroCheque;
            txtTitular.Text = oPagoE.TitularCheque;
            txtObservaciones.Text = oPagoE.Observaciones;

            txtCreado.Text = oPagoE.Creado != null ? oPagoE.Creado.ToString() : "";
            txtCreadoPor.Text = oPagoE.CreadoPor != null ? oPagoE.CreadoPor.Nombre : ""; 
            txtModificado.Text = oPagoE.Actualizado.ToString();
            txtModifPor.Text = oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.Nombre : ""; 
        }

        private void addOrEditPago()
        {
            if (validar())
            {                
                try
                {
                    //
                    if (oUsuario == null)
                    {
                        FormLoginVendedor frmLogin = new FormLoginVendedor();
                        frmLogin.ShowDialog(this);
                        if (oUsuario == null) return;

                        if (!oUsuario.Admin)
                        {
                            MessageBox.Show("No tiene permisos para modificar gastos de otra persona");
                            oUsuario = null;
                            return;
                        }

                        formNuevoPago_Load(null, null);
                        readOnly = false;
                        setearPropiedadesForm();
                        return;
                    }
                    //

                    cargarPago();

                    if (!huboModificaciones()) 
                        return;

                    bool esModificacion = oPagoE.Id > 0;

                    oPagoE = oCtaCteN.addOrEditPago(oPagoE);

                    //Cuenta Corriente
                    try
                    {
                        oCtaCteN.crearMovCtaCtePago(oPagoE);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al guardar Mov en Cta Cte. \n-El Pago se registró correctamente." + "\n\n" + ex.Source);
                    }

                    MessageBox.Show("El Pago de registró correctamente.");

                    if(frmPagos != null) 
                        frmPagos.cargarGrilla();

                    oPagoE = new Entidades.Pago();
                    oPagoSinMod = new Entidades.Pago();
                    cargarCampos();

                    if (esModificacion) 
                        this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }            
        }

        private void cargarPago()
        {
            try
            {
                ultimaValidacion = true;

                oPagoE.Sucursal = oSucursalE;
                oPagoE.NroRecibo = txtNroRecibo.Text.Trim();
                oPagoE.Persona = oPersonaE;
                oPagoE.FormaPago = comboTipoPago.Text.Trim();
                oPagoE.Fecha = txtFechaPago.Value;
                oPagoE.AProveedor = checkAProveedor.Checked;
                oPagoE.Importe = Utilidades.Util_Form.convertFloat(txtImporte.Text, false);

                if (comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Efectivo.ToString()) ||
                    comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Otro.ToString()))
                {
                    oPagoE.Banco = "";
                    oPagoE.NroCheque = "";
                    oPagoE.TitularCheque = "";
                }
                else
                {
                    oPagoE.Banco = txtBanco.Text;
                    oPagoE.NroCheque = txtNroCheque.Text;
                    oPagoE.TitularCheque = txtTitular.Text;
                }

                oPagoE.Observaciones = txtObservaciones.Text.Trim();

                oPagoE.CreadoPor = oPagoE.Id > 0 ? oPagoE.CreadoPor : oUsuario;
                oPagoE.ActualizadoPor = oPagoE.Id > 0 ? oUsuario : oPagoE.ActualizadoPor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private bool huboModificaciones()
        {
            ///Se comprueba si se realizaron modificaciones
            ///
            huboModif = (oPagoE.Fecha != oPagoSinMod.Fecha || 
                oPagoE.Importe != oPagoSinMod.Importe ||
                oPagoE.FormaPago != oPagoSinMod.FormaPago ||
                oPagoE.Banco != oPagoSinMod.Banco || 
                oPagoE.NroCheque != oPagoSinMod.NroCheque ||
                oPagoE.NroRecibo != oPagoSinMod.NroRecibo ||
                oPagoE.AProveedor != oPagoSinMod.AProveedor || 
                oPagoE.Observaciones != oPagoSinMod.Observaciones ||
                (oPagoE.Persona != null && oPagoSinMod.Persona != null && oPagoE.Persona.idPersona != oPagoSinMod.Persona.idPersona) ||
                (oPagoE.Sucursal != null && oPagoSinMod.Sucursal != null && oPagoE.Sucursal.idSucursal != oPagoSinMod.Sucursal.idSucursal) ||
                oPagoE.TitularCheque != oPagoSinMod.TitularCheque);

            if (!huboModif)
                MessageBox.Show("No se realizaron han realizado modificaciones en el Pago.\n\n" +
                    "Presione Cancelar para salir sin realizar modificaciones", "No hubo modificación", MessageBoxButtons.OK, MessageBoxIcon.Information);

            return huboModif;
        }

        private bool validar()
        {
            bool respuesta = true;

            if(!Util_Form.validarFecha(txtFechaPago.Value, "Fecha"))
                return false; ;

            if (txtNroRecibo.Text=="" || txtPersona.Text ==""  || comboTipoPago.Text==""
                || txtImporte.Text=="")
            {
                respuesta = false;

                string mensaje = "Complete los siguientes campos: ";

                if (txtNroRecibo.Text == "" )
                {
                    mensaje += "\n" + "-Número de Recibo";
                }

                if (txtPersona.Text == "")
                {
                    mensaje += "\n" + "-Persona";
                }
                if (comboTipoPago.Text == "")
                {
                    mensaje += "\n" + "-Forma Pago";
                }
                if (txtImporte.Text == "")
                {
                    mensaje += "\n" + "-Importe";
                }
                
                MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            return respuesta;        
        }

        #endregion

        private void checkAProveedor_CheckedChanged(object sender, EventArgs e)
        {
            if (readOnly) 
                return;

            if (checkAProveedor.Checked)
            {
                checkAProveedor.Text = "Pagar a ...";
                checkAProveedor.BackColor = Color.LimeGreen;
            }
            else
            {
                checkAProveedor.Text = "Recibí de ...";
                checkAProveedor.BackColor = Color.IndianRed;
            }
        }

        private void comboTipoPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboTipoPago.Text.Equals(""))
            {
                if (comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Efectivo.ToString()) || 
                    comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Otro.ToString()))
                {
                    panelCheque.Visible = false;
                }
                else
                {
                    panelCheque.Visible = true;
                }
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboSucursalCargada)
                {
                    oSucursalE = oSucursalN.findById(Convert.ToInt32(comboSucursal.SelectedValue));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la sucursal");
            }
        }

    }
}
