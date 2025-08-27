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
using Presentacion.Cheques;
using System.Web.Services.Description;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Entidades;
using Microsoft.Win32;
using System.Configuration;
using System.Globalization;
using Presentacion.Embutidos;
using iTextSharp.text.pdf.draw;
using System.Xml.Linq;


namespace Presentacion.Pagos
{
    public partial class formAddOrEditPago : Form, InterfaceUsuario, InterfacePersona
    {
        public formPagos frmPagos;
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        Negocio.CuentaCorriente oCtaCteN = new Negocio.CuentaCorriente();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
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
        bool cargandoForm = true;
        bool ultimaValidacion = true;//valida que los ingresos estén correctos antes de ingresar datos al DB
        string ultimaFormaPagoSelected = ""; //guarda la ultima forma de pago seleccionada
        public bool desdePOS = true; //para indicar que es llamado desde el form POS
        public formAddOrEditPago()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formNuevoPago_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                //si es desdePOS se oculta el boton para buscar persona y se establace que es un cobro 'Recibi de..'
                btnBuscarProv.Visible = !desdePOS;
                checkAProveedor.Checked = !desdePOS;
                

                if (oUsuario == null)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);
                }

                if (oUsuario == null)
                {
                    this.Close();
                    return;
                }

                if (!oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaPago.Value,
                            oPagoE != null && oPagoE.Id > 0 ? oPagoE.CreadoPor.Id : oUsuario.Id))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    this.Close();
                    return;
                }

                bool closeForm = false;
                if (idPago == 0 && oUsuario == null) closeForm = true;

                //inicio form con id 0 y se setea si es edicion
                idPagoLabel.Text = "0";
                txtEfectivo.ReadOnly = false;
                if (!closeForm)
                {
                    cargarSucursal();
                    oPagoE.Cheques = new List<Entidades.Cheque>();
                    if (idPago > 0)
                    {
                        oPagoE = oCtaCteN.getPagoById(idPago);
                        oPersonaE = oPagoE.Persona;
                        oPagoSinMod = oCtaCteN.getPagoById(idPago);
                        readOnly = true;
                        checkNroRecibo.Text = "Editar N°Recibo"; //solo cuando es un nuevo recibo se formatea su numero
                        checkNroRecibo.Checked = false;
                        btnIngresoBilletes.Enabled = !readOnly;
                        cargarCampos();
                        ultimaFormaPagoSelected = oPagoE.FormaPago;
                        setearPropiedadesForm();
                        idPagoLabel.Text = idPago.ToString();//asigno id para identificar el formulario al llamar
                    }
                    else
                    {
                        setearNroRecibo();
                    }

                    btnImprimir.Visible = oPagoE.Id > 0;
                    txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
                    txtPersona.Text = oPersonaE != null ? oPersonaE.razonSocial : "";
                    //se valida que sea Admin para cambiar de sucursal
                    comboSucursal.Visible = ((oUsuario != null && oUsuario.Admin) || FormPrincipal.logueado);
                    txtSucursal.Visible = !comboSucursal.Visible;
                    btnBuscarProv.Focus();
                    cargandoForm = false;
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

        /// <summary>
        /// Si es un Pago, se obtiene el Ultimo IdPago y se le suma 1 para generar el nro de Recibo
        /// 
        /// </summary>
        private void setearNroRecibo()
        {
            try
            {
                ///Si Edicion Pago
                if (oPagoE.Id > 0)
                {
                    txtNroRecibo.ReadOnly = !checkNroRecibo.Checked;
                    txtNroRecibo.Text = !checkNroRecibo.Checked ? oPagoE.NroRecibo : txtNroRecibo.Text;
                    txtNroRecibo.Focus();
                    return;
                }

                ///Si Nuevo Pago
                if (!checkNroRecibo.Checked)
                {
                    txtNroRecibo.ReadOnly = false;
                    txtNroRecibo.Focus();
                    return;
                }
                string nroRemitoFormateado = oSucursalE.idSucursal.ToString("D3") + "-" + (oCtaCteN.getUltimoIdPago() + 1).ToString("D8");

                txtNroRecibo.Text = oPagoE.Id > 0 ? txtNroRecibo.Text : nroRemitoFormateado;// (oCtaCteN.getUltimoIdPago() + 1).ToString();
            }
            catch (Exception)
            {

                throw;
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
            btnBuscarCheque.Visible = !readOnly;
            txtFechaPago.Enabled = !readOnly;
            comboTipoPago.Enabled = !readOnly;
            //txtTipoEgresoCaja.Visible = readOnly;
            //txtTipoEgresoCaja.Text = comboTipoEgresoCaja.Text;
            comboTipoPago.Enabled = !readOnly;
            checkNroRecibo.Enabled = !readOnly;
            txtNroRecibo.ReadOnly = true;// readOnly && checkNroRecibo.Enabled && !checkNroRecibo.Checked;
            txtImporte.ReadOnly = readOnly;
            //txtBanco.ReadOnly = readOnly;
            txtNroCheque.ReadOnly = readOnly;
            txtEfectivo.ReadOnly = readOnly;
            btnIngresoBilletes.Enabled = !readOnly;
            //txtTitular.ReadOnly = readOnly;
            txtObservaciones.ReadOnly = readOnly;
            btnImprimir.Visible = readOnly;
            if (grilla.Rows.Count > 0)
                grilla.Columns["btnQuitar"].Visible = !readOnly;
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
            CargarSaldo();
        }

        private void CargarSaldo()
        {
            DataTable dt = oCtaCteN.obtenerCtasCtes("", oPersonaE.idPersona);
            if (dt.Rows.Count == 0)
            {
                txtSaldo.Text = "S/D"; // O dejarlo vacío si preferís
                return;
            }
            var saldoStr = oCtaCteN.obtenerCtasCtes("", oPersonaE.idPersona).Rows[0]["Saldo"].ToString();

            if (decimal.TryParse(saldoStr, out decimal saldo))
            {
                // Formatear con puntos de miles y coma decimal (cultura Argentina)
                txtSaldo.Text = saldo.ToString("#,##0.00", new System.Globalization.CultureInfo("es-AR"));
            }
            else
            {
                txtSaldo.Text = "0,00"; // O dejarlo vacío si preferís
            }
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

        protected override bool ProcessCmdKey(ref System.Windows.Forms.Message msg, Keys keyData)
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
            txtEfectivo.Text = oPagoE.Efectivo.ToString("F2");
            CargarSaldo();
            CargarGrillaCheques();
            ///los datos de los campo que ya no se usan se agregan a observaciones
            ///
            string info = string.IsNullOrEmpty(oPagoE.Banco) ? "" : "Banco: " + oPagoE.Banco;
            info += string.IsNullOrEmpty(oPagoE.NroCheque) ? "" : "\nN°Cheque: " + oPagoE.NroCheque;
            info += string.IsNullOrEmpty(oPagoE.TitularCheque) ? "" : "\nTitular Cheque: " + oPagoE.TitularCheque;
            txtObservaciones.Text = info + oPagoE.Observaciones;

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
                    if (oUsuario == null)
                    {
                        FormLoginVendedor frmLogin = new FormLoginVendedor();
                        frmLogin.ShowDialog(this);
                    }

                    if (oUsuario == null) return;

                    if (!oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaPago.Value, 
                        oPagoE != null && oPagoE.Id > 0 ? oPagoE.CreadoPor.Id : oUsuario.Id))
                    {
                        Utilidades.Mensajes.ErrorPermisoEdicion();
                        return;
                    }

                    if (readOnly)
                    {
                        formNuevoPago_Load(null, null);
                        readOnly = false;
                        setearPropiedadesForm();
                        return;
                    }

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

                    //se actualiza el saldo
                    CargarSaldo();

                    //MessageBox.Show("El Pago de registró correctamente.");
                    DialogResult resp = MessageBox.Show("El Pago de registró correctamente.\n\n¿Generar Recibo en PDF?", "",
                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (resp.Equals(DialogResult.Yes))
                        imprimirRecibo();


                    if (frmPagos != null)
                        frmPagos.cargarGrilla();

                    oPagoE = new Entidades.Pago();
                    oPagoSinMod = new Entidades.Pago();
                    cargarCampos();

                    //if (esModificacion) 
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
                setearNroRecibo();//se vuelve a setear el nro recibo para que no se dupliquen
                oPagoE.NroRecibo = txtNroRecibo.Text.Trim();
                oPagoE.Persona = oPersonaE;
                oPagoE.FormaPago = comboTipoPago.Text.Trim();
                oPagoE.Fecha = txtFechaPago.Value;
                oPagoE.AProveedor = checkAProveedor.Checked;
                oPagoE.Importe = Utilidades.Util_Form.convertFloat(txtImporte.Text, false);
                oPagoE.Efectivo = !string.IsNullOrEmpty(txtEfectivo.Text) ? Utilidades.Util_Form.convertFloat(txtEfectivo.Text, false) : 0;

                if (comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Efectivo.ToString()) ||
                    comboTipoPago.Text.Equals(Entidades.Pago.formasPago.Otro.ToString()))
                {
                    oPagoE.Banco = "";
                    oPagoE.NroCheque = "";
                    oPagoE.TitularCheque = "";
                }
                else
                {
                    oPagoE.Banco = "";// txtBanco.Text;
                    oPagoE.NroCheque = "";// txtNroCheque.Text;
                    oPagoE.TitularCheque = "";// txtTitular.Text;
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

            if (oPersonaE.idPersona.Equals(Entidades.Parametros.idConsumidorFinal))
            {
                MessageBox.Show("No se pueden registrar Pagos a Consumidor Final", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }            

            if (!Util_Form.validarFecha(txtFechaPago.Value, "Fecha"))
                return false; 

            if (txtNroRecibo.Text == "" || txtPersona.Text == "" || comboTipoPago.Text == ""
                || txtImporte.Text == "")
            {
                respuesta = false;

                string mensaje = "Complete los siguientes campos: ";

                if (txtNroRecibo.Text == "")
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
            if (!cargandoForm)//para evitar que no cambie de color el checkbox
            {
                if (readOnly)
                    return;

                setearNroRecibo();

                if (oPagoE.Cheques != null && oPagoE.Cheques.Count > 0)
                {
                    //Validar Cheques de Pago y Cobro
                    bool errorIconsistencia = false;
                    string cobro_pago = checkAProveedor.Checked ? "Pago (Entregado)" : "Cobro (Recibido)";
                    string mensajeDeInconsistencia = "Los siguientes cheques ya fueron registrados como " + cobro_pago + "\n";
                    foreach (Entidades.Cheque oCheque in oPagoE.Cheques)
                    {
                        if (checkAProveedor.Checked && oCheque.PagoA != null && oCheque.PagoA.Id > 0 && oCheque.PagoA.Id != oPagoE.Id)
                        {
                            mensajeDeInconsistencia += "\nID Pago: " + oCheque.PagoA.Id + " - " + oCheque.PagoA.Persona.Identificacion;
                            errorIconsistencia = true;
                        }
                        if (!checkAProveedor.Checked && oCheque.PagoDe != null && oCheque.PagoDe.Id > 0 && oCheque.PagoDe.Id != oPagoE.Id)
                        {
                            mensajeDeInconsistencia += "\nID Pago: " + oCheque.PagoDe.Id + " - " + oCheque.PagoDe.Persona.Identificacion;
                            errorIconsistencia = true;
                        }
                    }
                    mensajeDeInconsistencia += "\n\nSi continua con la modificación se perderán los datos originales del " + cobro_pago;
                    if (errorIconsistencia)
                        MessageBox.Show(mensajeDeInconsistencia, "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }

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
                if (comboTipoPago.Text.Contains(Entidades.Pago.formasPago.Cheque.ToString()))
                {
                    if (ultimaFormaPagoSelected != comboTipoPago.Text && ultimaFormaPagoSelected.Contains("Eftvo")  && !(string.IsNullOrEmpty(txtEfectivo.Text) || txtEfectivo.Text.Equals("0")))
                    {
                        MessageBox.Show("El campo Efectivo tiene asignado un importe.\nBorre el valor o póngalo en 0 (cero) para cambiar la forma de pago.", "");
                        comboTipoPago.Text = ultimaFormaPagoSelected;
                        return;
                    }

                    txtImporte.ReadOnly = true;
                    panelCheque.Enabled = true;
                    if (comboTipoPago.Text.Contains("Eftvo"))
                    {

                        panelEfectivo.Visible = true;
                    }
                    else
                    {
                        panelEfectivo.Visible = false;
                        txtEfectivo.Text = "";
                    }
                }
                else
                {
                    if (ultimaFormaPagoSelected.Contains(Entidades.Pago.formasPago.Cheque.ToString()) && oPagoE.Cheques.Count > 0)
                    {

                        MessageBox.Show("Para cambiar la forma de pago primero debe quitar los Cheque asignados", "");
                        comboTipoPago.Text = ultimaFormaPagoSelected;
                        return;
                    }
                    panelCheque.Enabled = false;
                    txtImporte.ReadOnly = false;
                    txtEfectivo.Text = "";
                }

                ultimaFormaPagoSelected = comboTipoPago.Text;
            }
        }

        private void comboSucursal_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (comboSucursalCargada)
                {
                    oSucursalE = oSucursalN.findById(Convert.ToInt32(comboSucursal.SelectedValue));
                    setearNroRecibo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la sucursal");
            }
        }

        private void txtEfectivo_TextChanged(object sender, EventArgs e)
        {
            if (readOnly)
                return;

            if (!string.IsNullOrEmpty(txtEfectivo.Text) && !Utilidades.Util_Form.validarCampoNumerico(txtEfectivo.Text, "Importe Efectivo"))
            {
                txtImporte.Text = "";
                return;
            }

            CalcularImporte();
        }

        private void CalcularImporte()
        {
            try
            {
                // Intentamos convertir ambos campos a float
                float efectivo = 0;
                float totalCheques = 0;

                efectivo = !string.IsNullOrEmpty(txtEfectivo.Text) ? Util_Form.convertFloat(txtEfectivo.Text, false) : 0;
                float.TryParse(txtTotalCheques.Text, out totalCheques);

                float total = efectivo + totalCheques;

                txtImporte.Text = total.ToString("0.00"); // Con dos decimales
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void txtNroCheque_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == Convert.ToChar(Keys.Enter))
            {
                CargarCheque(null);
            }
        }

        public void CargarCheque(string nroCheque)
        {
            if (!string.IsNullOrEmpty(nroCheque))
                txtNroCheque.Text = nroCheque;

            Entidades.Cheque oCheque = oCtaCteN.getChequePorIDorNro(0, txtNroCheque.Text);
            if (oCheque != null)
            {
                if (oPagoE.Cheques.Any(c => c.NroCheque == oCheque.NroCheque))
                {
                    MessageBox.Show("El Cheque ya ha sido asignado al Pago actual");
                    return;
                }
                //se verifica que el mismo cheque no se asignado a dos pagos diferentes
                if (checkAProveedor.Checked && oCheque.PagoA != null && oCheque.PagoA.Id > 0 && oCheque.PagoA.Id != oPagoE.Id)
                {
                    MessageBox.Show("El Cheque ya ha sido asigando al siguiente Pago \n\nID Pago: " + oCheque.PagoA.Id + " - " + oCheque.PagoA.Persona.Identificacion);
                    return;
                }

                if (!checkAProveedor.Checked && oCheque.PagoDe != null && oCheque.PagoDe.Id > 0 && oCheque.PagoDe.Id != oPagoE.Id)
                {
                    MessageBox.Show("El Cheque ya ha sido asigando al siguiente Pago \n\nID Pago: " + oCheque.PagoDe.Id + " - " + oCheque.PagoDe.Persona.Identificacion);
                    return;
                }

                if (oCheque.FechaPago.AddDays(30) < DateTime.Today)
                {
                    DialogResult resp = MessageBox.Show("El Cheque N°: " + txtNroCheque.Text + " está vencido (Fecha Pago: " + oCheque.FechaPago.ToShortDateString() + " ).\n¿Desea agregarlo igualmente?", "",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                    if (resp.Equals(DialogResult.No))
                        return;
                }

                oPagoE.Cheques.Add(oCheque);
                CargarGrillaCheques();
                lblCantCheques.Text = oPagoE.Cheques.Count.ToString();
                txtNroCheque.Text = "";
            }
            else
            {
                DialogResult resp = MessageBox.Show("El Cheque N°: " + txtNroCheque.Text + " no existe.\n¿Desea agregarlo?", "",
                                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (resp.Equals(DialogResult.No))
                    return;

                formCheques frmCheques = new formCheques();
                frmCheques.llamadoDesdePago = true;
                frmCheques.oUsuario = oUsuario;
                frmCheques.nroChequeDesdePago = txtNroCheque.Text;
                frmCheques.NuevoCheque();
                frmCheques.ShowDialog();

                CargarCheque(null);
            }
        }

        private void CargarGrillaCheques()
        {
            if (oPagoE.Cheques == null || oPagoE.Cheques.Count == 0)
            {
                grilla.DataSource = null;
                return;
            }


            grilla.DataSource = null;
            grilla.Columns.Clear(); // Eliminar columnas anteriores

            //cargar lista de cheques a pago y cargar grilla
            var chequesReducidos = oPagoE.Cheques
                                .Select(c => new
                                {
                                    Id = c.Id,
                                    NroCheque = c.NroCheque,
                                    Banco = c.Banco,
                                    FechaPago = c.FechaPago.ToShortDateString(), // o sin ToShortDateString si querés el DateTime completo
                                    Importe = c.Importe.ToString("F2"),
                                })
                                .ToList();

            grilla.DataSource = chequesReducidos;

            // Ocultar columna Id si no querés mostrarla
            if (grilla.Columns.Contains("Id"))
                grilla.Columns["Id"].Visible = false;

            // Agregar columna de botón "Quitar"
            DataGridViewButtonColumn btnVer = new DataGridViewButtonColumn();
            btnVer.Name = "btnVer";
            btnVer.HeaderText = "";
            btnVer.Text = "Ver";
            btnVer.UseColumnTextForButtonValue = true;
            // Ajustar ancho automáticamente para que entre "Ver" y nada más
            btnVer.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            grilla.Columns.Add(btnVer);

            // Agregar columna de botón "Quitar"
            DataGridViewButtonColumn btnQuitar = new DataGridViewButtonColumn();
            btnQuitar.Name = "btnQuitar";
            btnQuitar.HeaderText = "";
            btnQuitar.Text = "Quitar";
            btnQuitar.UseColumnTextForButtonValue = true;
            btnQuitar.Width = 60;
            grilla.Columns.Add(btnQuitar);

            grilla.DefaultCellStyle.ForeColor = Color.Black;

            float totalImporte = 0.00f;
            foreach (DataGridViewRow row in grilla.Rows)
            {
                if (row.Cells["Importe"].Value != null)
                {
                    if (float.TryParse(row.Cells["Importe"].Value.ToString(), out float importe))
                    {
                        totalImporte += importe;
                    }
                }
            }
            txtTotalCheques.DataBindings.Clear();
            txtTotalCheques.Text = totalImporte.ToString("F2");
            lblCantCheques.Text = oPagoE.Cheques.Count.ToString();
        }

        private void txtTotalCheques_TextChanged(object sender, EventArgs e)
        {
            CalcularImporte();
        }

        private void grilla_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && grilla.Columns[e.ColumnIndex].Name == "btnQuitar")
            {
                int idCheque = Convert.ToInt32(grilla.Rows[e.RowIndex].Cells["Id"].Value);

                var chequeAEliminar = oPagoE.Cheques.FirstOrDefault(c => c.Id == idCheque);
                if (chequeAEliminar != null)
                {
                    oPagoE.Cheques.Remove(chequeAEliminar);
                    CargarGrillaCheques(); // Volver a cargar grilla con columnas actualizadas
                }
            }

            // Verifica que el clic fue en la columna del botón y en una fila válida
            if (grilla.Rows.Count > 0 && e.RowIndex >= 0 && grilla.Columns[e.ColumnIndex].Name == "btnVer")
            {
                int idCheque = Convert.ToInt32(grilla.Rows[e.RowIndex].Cells["Id"].Value);

                var chequeInfo = oPagoE.Cheques.FirstOrDefault(c => c.Id == idCheque);
                if (chequeInfo != null)
                {
                    var propiedades = chequeInfo.GetType().GetProperties();
                    StringBuilder sb = new StringBuilder();

                    foreach (var prop in propiedades)
                    {
                        var nombre = prop.Name;
                        var valor = prop.GetValue(chequeInfo, null) ?? "null";
                        string valorFormateado;
                        switch (nombre)
                        {
                            case "Propio":
                                valorFormateado = (bool)valor?.Equals(true) ? "Propio" : "Tercero";
                                break;

                            case "RecibidoDe":
                                valorFormateado = chequeInfo.PagoDe != null ? chequeInfo.PagoDe.Persona.Identificacion : "-";
                                break;

                            case "EntregadoA":
                                valorFormateado = chequeInfo.PagoA != null ? chequeInfo.PagoA.Persona.Identificacion : "-";
                                break;

                            case "CreadoPor":
                                valorFormateado = chequeInfo.CreadoPor != null ? chequeInfo.CreadoPor.Nombre : "-";
                                break;

                            case "ActualizadoPor":
                                valorFormateado = chequeInfo.ActualizadoPor != null ? chequeInfo.ActualizadoPor.Nombre : "-";
                                break;

                            default:
                                valorFormateado = valor?.ToString() ?? "null";
                                break;
                        }

                        if (!nombre.Equals("PagoDe") && !nombre.Equals("PagoA"))
                            sb.AppendLine($"{nombre}: {valorFormateado}");
                    }

                    string resultado = sb.ToString();
                    MessageBox.Show("Info Cheque: \n\n" + resultado);
                }
                // Aquí podés abrir un formulario, mostrar detalles, etc.
            }
        }

        private void btnBuscarCheque_Click(object sender, EventArgs e)
        {
            formCheques frmCheques = new formCheques();
            frmCheques.llamadoDesdePago = true;
            frmCheques.oUsuario = oUsuario;
            frmCheques.OnChequeDobleClick = CargarCheque;
            frmCheques.ShowDialog();
        }

        #region imprimirRecibo
        public void GenerarReciboPDF(string rutaDestino)
        {
            rutaDestino = rutaDestino + "\\" + oPagoE.Fecha.ToString("yyyyMMdd") + " - Recibo de Pago - ID " + oPagoE.Id.ToString() + ".pdf";

            Document doc = new Document(PageSize.A4, 30, 30, 20, 20);
            PdfWriter.GetInstance(doc, new FileStream(rutaDestino, FileMode.Create));
            doc.Open();

            var colorRojo = new BaseColor(174, 0, 0);
            var fuenteTitulo = FontFactory.GetFont(FontFactory.HELVETICA, 25, colorRojo);
            var fuenteRazonSocial = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var fuenteNormal = FontFactory.GetFont(FontFactory.HELVETICA, 9);
            var fuenteNegrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var fuenteX = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35);
            var fuenteFooter = FontFactory.GetFont(FontFactory.HELVETICA, 7);

            // ---------- CABECERA: tabla 3 columnas ----------
            PdfPTable cabecera = new PdfPTable(3);
            cabecera.WidthPercentage = 100;
            cabecera.SetWidths(new float[] { 33f, 34f, 33f });

            // Columna izquierda: nombre empresa y dirección
            PdfPCell izquierda = new PdfPCell();
            izquierda.Border = iTextSharp.text.Rectangle.NO_BORDER;
            izquierda.AddElement(new Paragraph(ConfigurationManager.AppSettings["Negocio"].ToString() + "\n", fuenteTitulo));
            izquierda.AddElement(new Paragraph(" ", fuenteRazonSocial));
            izquierda.AddElement(new Paragraph("Razón Social: " + ConfigurationManager.AppSettings["Dueno"].ToString() + "\n", fuenteRazonSocial));
            izquierda.AddElement(new Paragraph(ConfigurationManager.AppSettings["Direccion"].ToString() + " - " + ConfigurationManager.AppSettings["Localidad"].ToString() + "\n", fuenteRazonSocial));
            izquierda.AddElement(new Paragraph("Cond.IVA: " + ConfigurationManager.AppSettings["CondicionIVA"].ToString() + "\n", fuenteRazonSocial));
            cabecera.AddCell(izquierda);

            PdfPCell centro = new PdfPCell();
            centro.Border = iTextSharp.text.Rectangle.NO_BORDER;
            centro.VerticalAlignment = Element.ALIGN_MIDDLE; // esto sí funciona

            // Usá un Paragraph con alineación centrada
            Paragraph parrafoCentro = new Paragraph();
            parrafoCentro.Alignment = Element.ALIGN_CENTER;
            parrafoCentro.Add(new Chunk("X\n", fuenteX));
            parrafoCentro.Add(new Chunk("- Documento no válido como factura -", fuenteFooter));

            centro.AddElement(parrafoCentro);
            cabecera.AddCell(centro);

            // Columna derecha: número, fecha, cuit, etc.
            PdfPCell derecha = new PdfPCell();
            derecha.Border = iTextSharp.text.Rectangle.NO_BORDER;
            derecha.HorizontalAlignment = Element.ALIGN_RIGHT;

            derecha.AddElement(new Paragraph("N°Recibo: " + oPagoE.NroRecibo + "\n", fuenteNegrita));
            derecha.AddElement(new Paragraph("Fecha: " + oPagoE.Fecha.Date.ToString("dd/MM/yyyy") + "\n\n", fuenteNormal));
            derecha.AddElement(new Paragraph(ConfigurationManager.AppSettings["IIBB"] + "\n", fuenteNormal));
            derecha.AddElement(new Paragraph("CUIT: " + ConfigurationManager.AppSettings["cuit"] + "\n", fuenteNormal));
            derecha.AddElement(new Paragraph("Inicio Act.: " + ConfigurationManager.AppSettings["InicioActividades"] + "\n", fuenteNormal));

            cabecera.AddCell(derecha);

            doc.Add(cabecera);
            LineSeparator linea = new LineSeparator(1.5f, 100f, BaseColor.GRAY, Element.ALIGN_CENTER, -6);
            doc.Add(new Chunk(linea));

            doc.Add(new Paragraph(" ")); // Espacio
            // ---------- CLIENTE ----------
            PdfPTable cliente = new PdfPTable(4);
            cliente.WidthPercentage = 100;
            cliente.SetWidths(new float[] { 15, 45, 10, 20 });


            cliente.AddCell(CeldaSimple("Sr. (es):", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oPagoE.Persona.razonSocial.ToUpper(), fuenteNormal));

            cliente.AddCell(CeldaSimple("Cond. IVA:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oPagoE.Persona.Iva, fuenteNormal));

            cliente.AddCell(CeldaSimple("Domicilio:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oPagoE.Persona.Domicilio.ToUpper(), fuenteNormal));

            cliente.AddCell(CeldaSimple("CUIT:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oPagoE.Persona.Cuit, fuenteNormal));

            doc.Add(cliente);

            doc.Add(linea);
            doc.Add(new Paragraph(" ")); // Espacio

            // ---------- TABLA DE VALORES RECIBIDOS ----------
            string detallePago = "";
            string importesDetalle = "";
            int espaciosBlanco = 15;
            double importesCheque = 0.00f;
            if (oPagoE.FormaPago.Contains(Entidades.Pago.formasPago.Cheque.ToString()))
            {

                if (oPagoE.FormaPago.Contains("Eftvo"))
                {
                    detallePago += AjustarString("Efectivo", espaciosBlanco, true);
                    detallePago += AjustarString(" ", espaciosBlanco, true);
                    detallePago += AjustarString(" ", espaciosBlanco, true);
                    detallePago += AjustarString(" ", espaciosBlanco, true);

                    detallePago += "\n\n";

                    importesDetalle += oPagoE.Efectivo.ToString("F2");
                    importesDetalle += "\n";    
                }

                detallePago += AjustarString("Nro Cheque", espaciosBlanco, true);
                detallePago += AjustarString("Banco", espaciosBlanco, true);
                detallePago += AjustarString("Fecha Pago", espaciosBlanco, true);
                detallePago += AjustarString("Importe", espaciosBlanco, true);

                detallePago += AjustarString("-------------", espaciosBlanco, true);
                detallePago += AjustarString("-------------", espaciosBlanco, true);
                detallePago += AjustarString("-------------", espaciosBlanco, true);
                detallePago += AjustarString("-------------", espaciosBlanco, true);

                importesDetalle += "\n\n\n";

                foreach (var cheque in oPagoE.Cheques)
                {
                    detallePago += "\n";
                    detallePago += AjustarString(cheque.NroCheque, espaciosBlanco, true);
                    detallePago += AjustarString(cheque.Banco, espaciosBlanco, true);
                    detallePago += AjustarString(cheque.FechaPago.ToShortDateString(), espaciosBlanco, true);
                    detallePago += AjustarString(cheque.Importe.ToString("F2"), 12, false);

                    importesDetalle += "\n";
                    importesCheque += cheque.Importe;
                }

                detallePago += AjustarString("             ", espaciosBlanco, true);
                detallePago += AjustarString("             ", espaciosBlanco, true);
                detallePago += AjustarString("             ", espaciosBlanco, true);
                detallePago += AjustarString("Total Cheques", espaciosBlanco, true);

                importesDetalle += importesCheque.ToString("F2");

                string lineas = "___________________";
                detallePago += string.IsNullOrEmpty(oPagoE.Observaciones) ? "" : "\n\n"+lineas+"\nObs.: " + oPagoE.Observaciones;
            }
            else
            {
                detallePago = oPagoE.Observaciones;
                importesDetalle = oPagoE.Importe.ToString("F2");
            }

            PdfPTable tablaValores = new PdfPTable(3);
            tablaValores.WidthPercentage = 100;
            tablaValores.SetWidths(new float[] { 20, 60, 20 });

            string[] headers = { "Forma Pago", "Detalle", "Importe" };
            foreach (var h in headers)
            {
                var celda = new PdfPCell(new Phrase(h, fuenteNegrita));
                celda.BackgroundColor = new BaseColor(255, 200, 200);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                tablaValores.AddCell(celda);
            }

            // Fila ejemplo
            tablaValores.AddCell(CeldaCentrada(oPagoE.FormaPago, fuenteNormal));

            var fuenteMono = oPagoE.FormaPago.Contains(Entidades.Pago.formasPago.Cheque.ToString()) ?
                FontFactory.GetFont(FontFactory.COURIER, 9) : fuenteNormal;
            var celdaDetalle = new PdfPCell(new Phrase(detallePago, fuenteMono));
            celdaDetalle.HorizontalAlignment = Element.ALIGN_LEFT;
            tablaValores.AddCell(celdaDetalle);

            tablaValores.AddCell(CeldaDerecha(importesDetalle, fuenteNormal));

            doc.Add(tablaValores);

            // ---------- TOTAL ----------
            PdfPTable tablaTotal = new PdfPTable(6);
            tablaTotal.WidthPercentage = 100;
            tablaTotal.SetWidths(new float[] { 12, 16, 10, 10, 26, 26 });
            for (int i = 0; i < 5; i++)
                tablaTotal.AddCell(new PdfPCell() { Border = iTextSharp.text.Rectangle.NO_BORDER });

            PdfPCell celdaTotal = new PdfPCell(new Phrase("Total: $ " + oPagoE.Importe.ToString("#,##0.00", new CultureInfo("es-AR")), fuenteNegrita));
            celdaTotal.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaTotal.Border = iTextSharp.text.Rectangle.TOP_BORDER;
            tablaTotal.AddCell(celdaTotal);

            doc.Add(tablaTotal);

            PdfPTable tablaSaldo = new PdfPTable(6);
            tablaSaldo.WidthPercentage = 100;
            tablaSaldo.SetWidths(new float[] { 12, 16, 10, 10, 26, 26 });
            for (int i = 0; i < 5; i++)
                tablaSaldo.AddCell(new PdfPCell() { Border = iTextSharp.text.Rectangle.NO_BORDER });


            PdfPCell celdaSaldo = new PdfPCell(new Phrase("[ Saldo: $ " + txtSaldo.Text + " ]", fuenteNormal));
            celdaSaldo.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaSaldo.Border = iTextSharp.text.Rectangle.NO_BORDER;
            tablaSaldo.AddCell(celdaSaldo);

            doc.Add(tablaSaldo);

            doc.Close();
        }

        // Funciones auxiliares
        private PdfPCell CeldaSimple(string texto, iTextSharp.text.Font fuente, int alineacion = Element.ALIGN_LEFT)
        {
            PdfPCell celda = new PdfPCell(new Phrase(texto, fuente));
            celda.Border = iTextSharp.text.Rectangle.NO_BORDER;
            celda.HorizontalAlignment = alineacion;
            celda.Padding = 4f;
            return celda;
        }

        private PdfPCell CeldaCentrada(string texto, iTextSharp.text.Font fuente)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 5
            };
        }

        private PdfPCell CeldaDerecha(string texto, iTextSharp.text.Font fuente)
        {
            return new PdfPCell(new Phrase(texto, fuente))
            {
                HorizontalAlignment = Element.ALIGN_RIGHT,
                Padding = 5
            };
        }
        //public void GenerarPDFRecibo(Pago datos, string rutaArchivo)
        //{
        //    string rutaPDF = rutaArchivo + "\\" + DateTime.Today.ToString("yyyyMMdd") + " - Recibo de Pago - ID "+oPagoE.Id.ToString()+".pdf";

        //    // Verificar si la carpeta existe, si no, crearla
        //    if (!Directory.Exists(rutaArchivo))
        //        Directory.CreateDirectory(rutaArchivo);

        //    Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
        //    PdfWriter.GetInstance(doc, new FileStream(rutaPDF, FileMode.Create));
        //    doc.Open();

        //    var negrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        //    var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

        //    // Fuentes y estilos
        //    var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        //    var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        //    var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        //    var fontComments = FontFactory.GetFont(FontFactory.HELVETICA, 8);
        //    var fontInfoUser = FontFactory.GetFont(FontFactory.HELVETICA, 6);
        //    var fontSaltoLineaMinimo = FontFactory.GetFont(FontFactory.HELVETICA, 3);
        //    var fontNormalBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

        //    ////nuevo memebrete
        //    ///

        //    // Crear una tabla para el membrete
        //    PdfPTable tablaMembrete = new PdfPTable(3);
        //    tablaMembrete.WidthPercentage = 100;

        //    // Definir tamaños de las columnas (30% para logo, 40% para datos empresa, 30% para datos cliente)
        //    float[] widths = new float[] { 42f, 20f, 38f };
        //    tablaMembrete.SetWidths(widths);

        //    PdfPCell celdamembreteIzquierda = new PdfPCell();
        //    //celdamembreteIzquierda.Border = iTextSharp.text.iTextSharp.text.Rectangle.RECTANGLE;
        //    celdamembreteIzquierda.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    celdamembreteIzquierda.HorizontalAlignment = Element.ALIGN_CENTER;
        //    celdamembreteIzquierda.VerticalAlignment = Element.ALIGN_CENTER;

        //    Phrase membreteIzquierda = new Phrase();
        //    membreteIzquierda.Add(new Chunk("\n" + ConfigurationManager.AppSettings["Negocio"].ToString() + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20)));
        //    membreteIzquierda.Add(new Chunk("Razón Social: " + ConfigurationManager.AppSettings["Dueno"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteIzquierda.Add(new Chunk(ConfigurationManager.AppSettings["Direccion"].ToString() + " - " + ConfigurationManager.AppSettings["Localidad"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteIzquierda.Add(new Chunk("Condición frente al IVA: " + ConfigurationManager.AppSettings["CondicionIVA"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    celdamembreteIzquierda.AddElement(membreteIzquierda);
        //    tablaMembrete.AddCell(celdamembreteIzquierda);


        //    // Celda tipo Factura
        //    PdfPCell celdaTipoFactura = new PdfPCell();
        //    //celdaTipoFactura.Border = iTextSharp.text.iTextSharp.text.Rectangle.RECTANGLE;
        //    celdaTipoFactura.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    // Alineación correcta
        //    celdaTipoFactura.HorizontalAlignment = Element.ALIGN_CENTER; // alineación horizontal centrada
        //    celdaTipoFactura.VerticalAlignment = Element.ALIGN_TOP;   // alineación vertical centrada (o usa TOP si lo preferís arriba)


        //    Phrase tipoFactura = new Phrase();
        //    char letraFactura = 'X';
        //    string letraFacturaEncabezado = "  " + letraFactura + "  ";
        //    string descComprobante = "RECIBO";
        //    tipoFactura.Add(new Chunk(letraFacturaEncabezado, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 35)));
        //    tipoFactura.Add(new Chunk("\n", FontFactory.GetFont(FontFactory.HELVETICA, 6)));
        //    tipoFactura.Add(new Chunk("- No válido como factura -", FontFactory.GetFont(FontFactory.HELVETICA, 7)));
        //    celdaTipoFactura.AddElement(tipoFactura);
        //    tablaMembrete.AddCell(celdaTipoFactura);

        //    // Celda Membrete derecha
        //    PdfPCell celdamembreteDerecha = new PdfPCell();
        //    celdamembreteDerecha.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    celdamembreteDerecha.VerticalAlignment = Element.ALIGN_TOP; // opcional
        //    celdamembreteDerecha.HorizontalAlignment = Element.ALIGN_LEFT; // opcional

        //    // Usar Paragraph en lugar de Phrase
        //    Paragraph membreteDerecha = new Paragraph();
        //    membreteDerecha.Alignment = Element.ALIGN_LEFT; // Alineación real del texto

        //    membreteDerecha.Add(new Chunk(descComprobante.ToUpper() + "\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
        //    membreteDerecha.Add(new Chunk("N°Recibo: " + oPagoE.NroRecibo + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteDerecha.Add(new Chunk("Fecha de Emisión: " + oPagoE.Fecha.Date.ToString("dd/MM/yyyy") + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteDerecha.Add(new Chunk("CUIT: " + ConfigurationManager.AppSettings["cuit"] + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["IIBB"] + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["InicioActividades"] + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));

        //    celdamembreteDerecha.AddElement(membreteDerecha);
        //    tablaMembrete.AddCell(celdamembreteDerecha);

        //    //// Celda Membrete derecha
        //    //PdfPCell celdamembreteDerecha = new PdfPCell();
        //    ////celdamembreteDerecha.Border = iTextSharp.text.iTextSharp.text.Rectangle.RECTANGLE;
        //    //celdamembreteDerecha.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    //celdamembreteDerecha.HorizontalAlignment = Element.ALIGN_RIGHT;

        //    //Phrase membreteDerecha = new Phrase();
        //    //membreteDerecha.Add(new Chunk(descComprobante.ToUpper() + "\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
        //    //membreteDerecha.Add(new Chunk("N°Recibo: " + oPagoE.NroRecibo +"\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    //membreteDerecha.Add(new Chunk("Fecha de Emisión: " + oPagoE.Fecha.Date.ToString("dd/MM/yyyy") + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    //membreteDerecha.Add(new Chunk("CUIT: " + ConfigurationManager.AppSettings["cuit"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    //membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["IIBB"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    //membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["InicioActividades"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
        //    //celdamembreteDerecha.AddElement(membreteDerecha);
        //    //tablaMembrete.AddCell(celdamembreteDerecha);

        //    // Agregar la tabla al doc
        //    doc.Add(tablaMembrete);
        //    //doc.Add(new Paragraph("\n")); // Añadir un espacio después del membrete

        //    // Crear un LineSeparator para la línea horizontal
        //    LineSeparator line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0);
        //    // Agregar la línea al doc
        //    doc.Add(new Chunk(line));
        //    doc.Add(new Chunk(line));

        //    //// Información del Cliente
        //    PdfPTable clienteTable = new PdfPTable(1);
        //    clienteTable.WidthPercentage = 100;
        //    clienteTable.SetWidths(new float[] { 1f });

        //    string datosCliente = "CUIT:   " + oPagoE.Persona.Cuit +
        //        "              Apellido y Nombre/Razón Social:   " + oPagoE.Persona.razonSocial.ToUpper() +
        //        "\n\nCondición frente al IVA:   " + oPagoE.Persona.Iva + //oDocumentoImprimir.CondicionIvaAFIP.ToUpper() + 
        //        "\n\nDomicilio :   " + oPagoE.Persona.Domicilio.ToUpper(); //oDocumentoImprimir.CondicionVenta.ToUpper();

        //    clienteTable.AddCell(new PdfPCell(new Phrase(datosCliente, fontNormal)) { Border = 0 });
        //    doc.Add(clienteTable);

        //    ////


        //    //if (false)
        //    //{

        //    //    // Título
        //    //    var titulo = new Paragraph("RECIBO DE PAGO", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16));
        //    //    titulo.Alignment = Element.ALIGN_CENTER;
        //    //    doc.Add(titulo);
        //    //    var noValidoComoFactura = new Paragraph("-No válido como factura-", fontComments);
        //    //    noValidoComoFactura.Alignment = Element.ALIGN_CENTER;
        //    //    doc.Add(noValidoComoFactura);
        //    //    doc.Add(new Paragraph(" "));

        //    //    // Tabla para fecha y número de recibo alineados a la derecha
        //    //    PdfPTable tablaCabecera = new PdfPTable(2);
        //    //    tablaCabecera.WidthPercentage = 100;
        //    //    tablaCabecera.SetWidths(new float[] { 3f, 1.5f });

        //    //    // Columna izquierda vacía
        //    //    string datosPagador = oPagoE.AProveedor ? ConfigurationManager.AppSettings["Dueno"].ToString() : datos.Persona.razonSocial;
        //    //    PdfPCell celdaIzquierda = new PdfPCell(new Phrase($"RECIBO DE: {datosPagador.ToUpper()}", fontNormalBold));
        //    //    celdaIzquierda.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    //    tablaCabecera.AddCell(celdaIzquierda);

        //    //    // Columna derecha con fecha y número de recibo
        //    //    string textoDerecha = $"Fecha: {datos.Fecha:dd/MM/yyyy}";
        //    //    //textoDerecha += $"\nID Pago {datos.Id}";
        //    //    textoDerecha += $"\nN° Recibo: {datos.NroRecibo}";

        //    //    PdfPCell celdaDerecha = new PdfPCell(new Phrase(textoDerecha, normal));
        //    //    celdaDerecha.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    //    celdaDerecha.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    //    tablaCabecera.AddCell(celdaDerecha);

        //    //    doc.Add(tablaCabecera);

        //    //    // forma pago
        //    //    var formaPago = new Paragraph($"Forma Pago: {oPagoE.FormaPago}", fontNormalBold);
        //    //    formaPago.Alignment = Element.ALIGN_LEFT;
        //    //    doc.Add(formaPago);
        //    //    doc.Add(new Paragraph(" "));
        //    //}

        //    if (oPagoE.FormaPago.Contains(Entidades.Pago.formasPago.Cheque.ToString()))
        //    {
        //        // forma pago
        //        var detalle = new Paragraph($"Detalle", fontNormalBold);
        //        detalle.Alignment = Element.ALIGN_LEFT;
        //        doc.Add(detalle);

        //        var saltoLinea = new Paragraph($" ", fontSaltoLineaMinimo);
        //        detalle.Alignment = Element.ALIGN_LEFT;
        //        doc.Add(saltoLinea);
        //        //doc.Add(new Paragraph(" "));

        //        // Tabla de cheques
        //        PdfPTable tabla = new PdfPTable(4);
        //        tabla.WidthPercentage = 100;
        //        tabla.SetWidths(new float[] { 2f, 2f, 2f, 1.5f });

        //        tabla.AddCell(new Phrase("Nro Cheque", fontNormalBold));
        //        tabla.AddCell(new Phrase("Banco", fontNormalBold));
        //        tabla.AddCell(new Phrase("Fecha de Pago", fontNormalBold));
        //        tabla.AddCell(new Phrase("Importe", fontNormalBold));

        //        foreach (var cheque in datos.Cheques)
        //        {
        //            tabla.AddCell(new Phrase(cheque.NroCheque, fontNormal));
        //            tabla.AddCell(new Phrase(cheque.Banco, fontNormal));
        //            tabla.AddCell(new Phrase(cheque.FechaPago.ToShortDateString(), fontNormal));
        //            tabla.AddCell(new Phrase(cheque.Importe.ToString("F2"), fontNormal));
        //        }

        //        if (oPagoE.FormaPago.Contains("Eftvo"))
        //        {
        //            //var importeEfectivo = new Paragraph($"Efectivo $: {oPagoE.Efectivo:F2}", fontNormal);
        //            //importeEfectivo.Alignment = Element.ALIGN_RIGHT;
        //            //doc.Add(importeEfectivo);

        //            tabla.AddCell(new Phrase("//////", fontNormal));
        //            tabla.AddCell(new Phrase("//////", fontNormal));
        //            tabla.AddCell(new Phrase("//////", fontNormal));
        //            tabla.AddCell(new Phrase("//////", fontNormal));

        //            tabla.AddCell(new Phrase(" ", fontNormal));
        //            tabla.AddCell(new Phrase(" ", fontNormal));
        //            tabla.AddCell(new Phrase("EFECTIVO", fontNormal));
        //            tabla.AddCell(new Phrase(oPagoE.Efectivo.ToString("F2"), fontNormal));
        //        }
        //        doc.Add(tabla);
        //        //doc.Add(new Paragraph(" "));
        //    }

        //    // Total
        //    var importe = float.TryParse(txtImporte.Text, out float val) ? val : 0f;
        //    var formatoImporte = val.ToString("N2", new CultureInfo("es-AR"));
        //    var totalParrafo = new Paragraph($"TOTAL RECIBIDO: $ {formatoImporte}", negrita);

        //    totalParrafo.Alignment = Element.ALIGN_RIGHT;
        //    doc.Add(totalParrafo);
        //    doc.Add(new Paragraph(" "));

        //    //// Observaciones
        //    string obs = string.IsNullOrEmpty(oPagoE.Observaciones) ? "-" : oPagoE.Observaciones;         
        //    //obs += $"\n\n\nID Pago {datos.Id}" + " | creado por:" + oPagoE.CreadoPor.User + " | modif.por: " + (oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.User : "-");

        //    // Tabla para fecha y número de recibo alineados a la derecha
        //    PdfPTable tablaPiePagina = new PdfPTable(2);
        //    tablaPiePagina.WidthPercentage = 100;
        //    tablaPiePagina.SetWidths(new float[] { 2.5f, 2f });

        //    // Columna izquierda vacía
        //    PdfPCell celdaIzquierda = new PdfPCell(new Phrase($"Observaciones: {obs}", fontComments));
        //    celdaIzquierda.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    tablaPiePagina.AddCell(celdaIzquierda);

        //    // Columna derecha con fecha y número de recibo
        //    string textoDerecha = $"\n";
        //    textoDerecha += $"Firma: ____________________________";
        //    textoDerecha += $"\n\n";
        //    textoDerecha += $"\nAclaración:____________________________";

        //    PdfPCell celdaDerecha = new PdfPCell(new Phrase(textoDerecha, normal));
        //    celdaDerecha.HorizontalAlignment = Element.ALIGN_RIGHT;
        //    celdaDerecha.Border = iTextSharp.text.iTextSharp.text.Rectangle.NO_BORDER;
        //    tablaPiePagina.AddCell(celdaDerecha);

        //    doc.Add(tablaPiePagina);

        //    // forma pago
        //    string info = $"ID Pago: {datos.Id}" + " | suc:" + oPagoE.Sucursal.sucursal +" | creado por:" + oPagoE.CreadoPor.User + " | modif.por: " + (oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.User : "-");

        //    var infoUser = new Paragraph(info, fontInfoUser);
        //    infoUser.Alignment = Element.ALIGN_LEFT;
        //    doc.Add(infoUser);
        //    doc.Add(new Paragraph(" "));


        //    doc.Close();
        //}
    

            string AjustarString(string input, int espacios, bool espaciosDerecha)
            {
                // Si la cadena es más larga que 20 caracteres, se trunca a 20
                if (input.Length > espacios)
                    return input.Substring(0, espacios);
                // Si es menor a 20, se completa con espacios a la derecha hasta llegar a 20 caracteres
                else
                {
                    input = espaciosDerecha ? input.PadRight(espacios) : input.PadLeft(espacios);
                    return input;
                }
            }
        
        #endregion

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            imprimirRecibo();
        }

        private void imprimirRecibo()
        {
            string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
            ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ruta);// "ReciboCheques.pdf");
            //GenerarPDFRecibo(oPagoE, ruta);
            GenerarReciboPDF(ruta);
            System.Diagnostics.Process.Start(ruta);
        }

        private void checkNroRecibo_CheckedChanged(object sender, EventArgs e)
        {
            //txtNroRecibo.ReadOnly = checkNroRecibo.Checked && oPagoE.Id > 0;
            setearNroRecibo();
        }

        private void btnObservaciones_Click(object sender, EventArgs e)
        {

            formReceta frmReceta = new formReceta(txtObservaciones.Text); // Pasar el texto actual
            frmReceta.editar = !readOnly;
            frmReceta.observaciones = true;
            frmReceta.OnObservaciones = CargarObservaciones;
            frmReceta.ShowDialog();
        }

        public void CargarObservaciones(string obs)
        {
            txtObservaciones.Text = obs;
        }

        private void btnIngresoBilletes_Click(object sender, EventArgs e)
        {
            CalculoBilletes();
        }

        private void CalculoBilletes()
        {
            formIngresoBilletes frmIngresoBilletes = new formIngresoBilletes();
            frmIngresoBilletes.txtBoxAcargar = this.txtEfectivo;
            frmIngresoBilletes.ShowDialog();
            if (!frmIngresoBilletes.txtBoxAcargar.Text.Equals("0"))
            {
                txtEfectivo.Text = frmIngresoBilletes.txtBoxAcargar.Text;
            }
        }
    }
}
