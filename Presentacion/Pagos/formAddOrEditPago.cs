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
                        cargarCampos();
                        ultimaFormaPagoSelected = oPagoE.FormaPago;
                        readOnly = true;
                        setearPropiedadesForm();
                        idPagoLabel.Text = idPago.ToString();//asigno id para identificar el formulario al llamar
                    }
                    else
                    {
                        setearNroRecibo();
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

        /// <summary>
        /// Si es un Pago, se obtiene el Ultimo IdPago y se le suma 1 para generar el nro de Recibo
        /// 
        /// </summary>
        private void setearNroRecibo()
        {
            try
            {
                txtNroRecibo.Text = oPagoE.Id > 0 ? txtNroRecibo.Text : (oCtaCteN.getUltimoIdPago() + 1).ToString();
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
            txtNroRecibo.ReadOnly = readOnly;
            txtImporte.ReadOnly = readOnly;
            //txtBanco.ReadOnly = readOnly;
            txtNroCheque.ReadOnly = readOnly;
            //txtTitular.ReadOnly = readOnly;
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
                    panelCheque.Visible = true;
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
                    panelCheque.Visible = false;
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
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener la sucursal");
            }
        }

        private void txtEfectivo_TextChanged(object sender, EventArgs e)
        {

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
                CargarCheque();
            }
        }

        private void CargarCheque()
        {
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
                    MessageBox.Show("El Cheque ya ha sido asigando al siguiente Pago \nID: " + oCheque.PagoA.Id + "\n" + oCheque.PagoA.Persona.Identificacion);
                    return;
                }

                if (!checkAProveedor.Checked && oCheque.PagoDe != null && oCheque.PagoDe.Id > 0 && oCheque.PagoDe.Id != oPagoE.Id)
                {
                    MessageBox.Show("El Cheque ya ha sido asigando al siguiente Pago \nID: " + oCheque.PagoDe.Id + "\n" + oCheque.PagoDe.Persona.Identificacion);
                    return;
                }

                oPagoE.Cheques.Add(oCheque);
                CargarGrillaCheques();
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

                CargarCheque();
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

            ///subTotalCheques
            ///double totalImporte = 0;

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
            frmCheques.ShowDialog();
        }
    }
}
