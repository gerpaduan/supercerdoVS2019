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
        string ultimaFormaPagoSelected = ""; //guarda la ultima forma de pago seleccionada
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
                    oPagoE.Cheques = new List<Entidades.Cheque>();
                    if (idPago > 0)
                    {
                        oPagoE = oCtaCteN.getPagoById(idPago);
                        oPersonaE = oPagoE.Persona;
                        oPagoSinMod = oCtaCteN.getPagoById(idPago);
                        readOnly = true;
                        checkNroRecibo.Text = "Editar N°Recibo"; //solo cuando es un nuevo recibo se formatea su numero
                        checkNroRecibo.Checked = false;
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
            CargarGrillaCheques();
            ///los datos de los campo que ya no se usan se agregan a observaciones
            ///
            string info = string.IsNullOrEmpty(oPagoE.Banco) ? "" : "Banco: " + oPagoE.Banco;
            info +=  string.IsNullOrEmpty(oPagoE.NroCheque) ? "" : "\nN°Cheque: " + oPagoE.NroCheque;
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

                        MessageBox.Show("Para cambiar la forma de pago primero debe quitar los Cheque asignados","");
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

            if (!Utilidades.Util_Form.validarCampoNumerico(txtEfectivo.Text, "Importe Efectivo"))
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
                return;


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
        public void GenerarPDFRecibo(Pago datos, string rutaArchivo)
        {
            string rutaPDF = rutaArchivo + "\\" + DateTime.Today.ToString("yyyyMMdd") + " - Recibo de Pago - ID "+oPagoE.Id.ToString()+".pdf";

            // Verificar si la carpeta existe, si no, crearla
            if (!Directory.Exists(rutaArchivo))
                Directory.CreateDirectory(rutaArchivo);

            Document doc = new Document(PageSize.A4, 40, 40, 40, 40);
            PdfWriter.GetInstance(doc, new FileStream(rutaPDF, FileMode.Create));
            doc.Open();

            var negrita = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
            var normal = FontFactory.GetFont(FontFactory.HELVETICA, 10);

            // Fuentes y estilos
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fontComments = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var fontInfoUser = FontFactory.GetFont(FontFactory.HELVETICA, 6);
            var fontSaltoLineaMinimo = FontFactory.GetFont(FontFactory.HELVETICA, 3);
            var fontNormalBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            // Título
            var titulo = new Paragraph("RECIBO DE PAGO", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16));
            titulo.Alignment = Element.ALIGN_CENTER;
            doc.Add(titulo); 
            var noValidoComoFactura = new Paragraph("-No válido como factura-", fontComments);
            noValidoComoFactura.Alignment = Element.ALIGN_CENTER;
            doc.Add(noValidoComoFactura);
            doc.Add(new Paragraph(" "));

            // Tabla para fecha y número de recibo alineados a la derecha
            PdfPTable tablaCabecera = new PdfPTable(2);
            tablaCabecera.WidthPercentage = 100;
            tablaCabecera.SetWidths(new float[] { 3f, 1.5f });

            // Columna izquierda vacía
            string datosPagador = oPagoE.AProveedor ? ConfigurationManager.AppSettings["Dueno"].ToString() : datos.Persona.razonSocial;
            PdfPCell celdaIzquierda = new PdfPCell(new Phrase($"RECIBO DE: {datosPagador.ToUpper()}", fontNormalBold));
            celdaIzquierda.Border = iTextSharp.text.Rectangle.NO_BORDER;
            tablaCabecera.AddCell(celdaIzquierda);

            // Columna derecha con fecha y número de recibo
            string textoDerecha = $"Fecha: {datos.Fecha:dd/MM/yyyy}";
            //textoDerecha += $"\nID Pago {datos.Id}";
            textoDerecha += $"\nN° Recibo: {datos.NroRecibo}";

            PdfPCell celdaDerecha = new PdfPCell(new Phrase(textoDerecha, normal));
            celdaDerecha.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaDerecha.Border = iTextSharp.text.Rectangle.NO_BORDER;
            tablaCabecera.AddCell(celdaDerecha);

            doc.Add(tablaCabecera);

            // forma pago
            var formaPago = new Paragraph($"Forma Pago: {oPagoE.FormaPago}", fontNormalBold);
            formaPago.Alignment = Element.ALIGN_LEFT;
            doc.Add(formaPago);
            doc.Add(new Paragraph(" "));

            if (oPagoE.FormaPago.Contains(Entidades.Pago.formasPago.Cheque.ToString()))
            {
                // forma pago
                var detalle = new Paragraph($"Detalle", fontNormalBold);
                detalle.Alignment = Element.ALIGN_LEFT;
                doc.Add(detalle);

                var saltoLinea = new Paragraph($" ", fontSaltoLineaMinimo);
                detalle.Alignment = Element.ALIGN_LEFT;
                doc.Add(saltoLinea);
                //doc.Add(new Paragraph(" "));

                // Tabla de cheques
                PdfPTable tabla = new PdfPTable(4);
                tabla.WidthPercentage = 100;
                tabla.SetWidths(new float[] { 2f, 2f, 2f, 1.5f });

                tabla.AddCell(new Phrase("Nro Cheque", fontNormalBold));
                tabla.AddCell(new Phrase("Banco", fontNormalBold));
                tabla.AddCell(new Phrase("Fecha de Pago", fontNormalBold));
                tabla.AddCell(new Phrase("Importe", fontNormalBold));

                foreach (var cheque in datos.Cheques)
                {
                    tabla.AddCell(new Phrase(cheque.NroCheque, fontNormal));
                    tabla.AddCell(new Phrase(cheque.Banco, fontNormal));
                    tabla.AddCell(new Phrase(cheque.FechaPago.ToShortDateString(), fontNormal));
                    tabla.AddCell(new Phrase(cheque.Importe.ToString("F2"), fontNormal));
                }

                if (oPagoE.FormaPago.Contains("Eftvo"))
                {
                    //var importeEfectivo = new Paragraph($"Efectivo $: {oPagoE.Efectivo:F2}", fontNormal);
                    //importeEfectivo.Alignment = Element.ALIGN_RIGHT;
                    //doc.Add(importeEfectivo);

                    tabla.AddCell(new Phrase("//////", fontNormal));
                    tabla.AddCell(new Phrase("//////", fontNormal));
                    tabla.AddCell(new Phrase("//////", fontNormal));
                    tabla.AddCell(new Phrase("//////", fontNormal));

                    tabla.AddCell(new Phrase(" ", fontNormal));
                    tabla.AddCell(new Phrase(" ", fontNormal));
                    tabla.AddCell(new Phrase("EFECTIVO", fontNormal));
                    tabla.AddCell(new Phrase(oPagoE.Efectivo.ToString("F2"), fontNormal));
                }
                doc.Add(tabla);
                //doc.Add(new Paragraph(" "));
            }

            // Total
            var importe = float.TryParse(txtImporte.Text, out float val) ? val : 0f;
            var formatoImporte = val.ToString("N2", new CultureInfo("es-AR"));
            var totalParrafo = new Paragraph($"TOTAL RECIBIDO: $ {formatoImporte}", negrita);

            totalParrafo.Alignment = Element.ALIGN_RIGHT;
            doc.Add(totalParrafo);
            doc.Add(new Paragraph(" "));

            //// Observaciones
            string obs = string.IsNullOrEmpty(oPagoE.Observaciones) ? "-" : oPagoE.Observaciones;         
            //obs += $"\n\n\nID Pago {datos.Id}" + " | creado por:" + oPagoE.CreadoPor.User + " | modif.por: " + (oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.User : "-");

            // Tabla para fecha y número de recibo alineados a la derecha
            PdfPTable tablaPiePagina = new PdfPTable(2);
            tablaPiePagina.WidthPercentage = 100;
            tablaPiePagina.SetWidths(new float[] { 2.5f, 2f });

            // Columna izquierda vacía
            celdaIzquierda = new PdfPCell(new Phrase($"Observaciones: {obs}", fontComments));
            celdaIzquierda.Border = iTextSharp.text.Rectangle.NO_BORDER;
            tablaPiePagina.AddCell(celdaIzquierda);

            // Columna derecha con fecha y número de recibo
            textoDerecha = $"\n";
            textoDerecha += $"Firma: ____________________________";
            textoDerecha += $"\n\n";
            textoDerecha += $"\nAclaración:____________________________";

             celdaDerecha = new PdfPCell(new Phrase(textoDerecha, normal));
            celdaDerecha.HorizontalAlignment = Element.ALIGN_RIGHT;
            celdaDerecha.Border = iTextSharp.text.Rectangle.NO_BORDER;
            tablaPiePagina.AddCell(celdaDerecha);

            doc.Add(tablaPiePagina);

            // forma pago
            string info = $"ID Pago: {datos.Id}" + " | suc:" + oPagoE.Sucursal.sucursal +" | creado por:" + oPagoE.CreadoPor.User + " | modif.por: " + (oPagoE.ActualizadoPor != null ? oPagoE.ActualizadoPor.User : "-");

            var infoUser = new Paragraph(info, fontInfoUser);
            infoUser.Alignment = Element.ALIGN_LEFT;
            doc.Add(infoUser);
            doc.Add(new Paragraph(" "));


            doc.Close();
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
            GenerarPDFRecibo(oPagoE, ruta);
            System.Diagnostics.Process.Start(ruta);
        }

        private void checkNroRecibo_CheckedChanged(object sender, EventArgs e)
        {
            //txtNroRecibo.ReadOnly = checkNroRecibo.Checked && oPagoE.Id > 0;
            setearNroRecibo();
        }

        private void formAddOrEditPago_Activated(object sender, EventArgs e)
        {
            //setearNroRecibo();
        }
    }
}
