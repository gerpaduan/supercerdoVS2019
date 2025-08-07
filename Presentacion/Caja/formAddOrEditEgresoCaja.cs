using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using Utilidades;
using static Presentacion.Caja.formCerrarCaja;
using System.Text.RegularExpressions;

namespace Presentacion.Caja
{
    public partial class formAddOrEditEgresoCaja : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.Usuario oUsuario;
        public Negocio.Usuario oUsuarioN = new Negocio.Usuario();

        formEgresosCaja frmEgresosCaja;

        public int idEgresoCaja = 0;
        bool readOnly = false;
        bool huboModificacion = false;
        public bool egresoDesdeCajaVenta = false;

        public formAddOrEditEgresoCaja()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formAddOrEditEgresoCaja_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                checkTicket.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["ticketForms"].ToString());
                bool closeForm = false;
                if (idEgresoCaja == 0 && oUsuario == null) closeForm = true;

                if (!closeForm)
                {
                    cargarSucursal();
                    cargarTiposEgresoCaja();
                    txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
                    if (idEgresoCaja > 0)
                    {
                        oEgresoCajaE = oCierreN.getEgresoCajaById(idEgresoCaja);
                        cargarCampos();
                        readOnly = true;
                        setearPropiedadesForm();
                        idEgresoCajaLabel.Text = idEgresoCaja.ToString();//asigno id para identificar el formulario al llamar
                    }
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

        private void validarAperturaCaja()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0))
            {
                MessageBox.Show(oUsuario.Nombre + ":\nDebes Abrir Caja para poder registrar gastos.", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                oUsuario = null;
                if (idEgresoCaja == 0)this.Close();
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
            this.txtUsuario.Text = oUsuario.Nombre;
        }

        private void setearPropiedadesForm()
        {            
            this.Text = readOnly ? "Info EgresoCaja" : "Modificar";
            this.btnAceptar.Text = readOnly ? "&Modificar" : "&Guardar";
            txtFechaTexto.Visible = readOnly;
            txtFechaTexto.Text = Util_Form.fechaFormato24Horas(txtFechaEgresoCaja.Value);
            txtSucursal.Visible = readOnly || !((oUsuario != null && oUsuario.Admin) || FormPrincipal.logueado);
            comboSucursal.Visible = !txtSucursal.Visible;
            txtTipoEgresoCaja.Visible = readOnly;
            txtSucursal.Text = comboSucursal.Text;
            txtTipoEgresoCaja.Text = comboTipoEgresoCaja.Text;
            txtDescripcion.ReadOnly = readOnly;
            txtMonto.ReadOnly = readOnly;
            txtDetalle.ReadOnly = readOnly;
            checkTicket.Visible = !readOnly;
            btnImprimir.Visible = readOnly;
        }

        private void cargarCampos()
        {
            //cargar campos en pantalla
            comboSucursal.SelectedValue = oEgresoCajaE.Sucursal.idSucursal;
            txtIdEgresoCaja.Text = oEgresoCajaE.Id.ToString();
            txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
            txtFechaEgresoCaja.Value = oEgresoCajaE.Fecha;
            comboTipoEgresoCaja.SelectedValue = oEgresoCajaE.IdTipoEgresoCaja;
            txtDescripcion.Text = oEgresoCajaE.Descripcion;
            txtMonto.Text = oEgresoCajaE.Monto.ToString();
            txtDetalle.Text = oEgresoCajaE.Detalle;
            txtCreado.Text = Util_Form.fechaFormato24Horas(oEgresoCajaE.Creado);
            txtCreadoPor.Text = oEgresoCajaE.CreadoPorUser != null ? oEgresoCajaE.CreadoPorUser.Nombre : "";
            txtModificado.Text = Util_Form.fechaFormato24Horas(oEgresoCajaE.Actualizado);
            txtModifPor.Text = oEgresoCajaE.ActualizadoPorUser != null ? oEgresoCajaE.ActualizadoPorUser.Nombre : "";
        }

        public void asignarForm(formEgresosCaja form)
        {
            frmEgresosCaja = form;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oEgresoCajaE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;

            txtSucursal.Text = comboSucursal.Text;
        }

        private void cargarTiposEgresoCaja()
        {
            DataTable dtTipoEgresosCaja = oCierreN.obtenerTiposEgresoCaja("", 0);
            dtTipoEgresosCaja.Rows[0][1] = dtTipoEgresosCaja.Rows[0][0].Equals(0) ? "Seleccione un tipo" : dtTipoEgresosCaja.Rows[0][1].ToString();
            comboTipoEgresoCaja.DataSource = dtTipoEgresosCaja;
            comboTipoEgresoCaja.DisplayMember = "tipoEgresoCaja";
            comboTipoEgresoCaja.ValueMember = "id";
        }

        private bool validarCajaAbiertaVendedeor()
        {
            bool resp = true;
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0) || oCierreE.FechaHoraInicio > txtFechaEgresoCaja.Value)
            {
                resp = false;
                MessageBox.Show("La fecha y hora del egreso de caja ("+ Utilidades.Util_Form.fechaFormato24Horas(txtFechaEgresoCaja.Value) +") debe ser mayor a la fecha de apertura de caja ("+
                Utilidades.Util_Form.fechaFormato24Horas(oCierreE.FechaHoraInicio) + ")",
                    "Mensaje de Error", MessageBoxButtons.OK,MessageBoxIcon.Stop);

            }

            return resp;
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            addOrEdit();         
        }

        private void addOrEdit()
        {
            try
            {
                bool tienePermiso = true;
                if (oUsuario == null)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);
                    if (oUsuario == null) return;
                    tienePermiso = (oEgresoCajaE.CreadoPor == oUsuario.Id || oUsuario.Admin) ? true : false;
                    if (!tienePermiso)
                    {
                        MessageBox.Show("No tiene permisos para modificar gastos de otra persona");
                        oUsuario = null;
                        return;
                    }
                }

                //bool cajaAbierta = (!egresoDesdeCajaVenta && (Presentacion.FormPrincipal.logueado || oUsuario.Admin)) ? true : validarCajaAbiertaVendedeor();
                bool cajaAbierta = true;
                if (!egresoDesdeCajaVenta)
                {
                    if (!(oUsuarioN.tienePermiso(oUsuario, this.Name, txtFechaEgresoCaja.Value,
                    oEgresoCajaE.Id > 0 ? oEgresoCajaE.CreadoPorUser.Id : oUsuario.Id)))
                    {
                        Utilidades.Mensajes.ErrorPermisoEdicion();
                        return;
                    }
                }
                else
                    cajaAbierta = validarCajaAbiertaVendedeor();


                if (tienePermiso && cajaAbierta && Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado,
                        Convert.ToInt32(comboSucursal.SelectedValue.ToString())))
                {
                    if (oEgresoCajaE.Id > 0 && readOnly)
                    {
                        //se valida que no sea egreso por venta por Tarjeta
                        if (oEgresoCajaE.IdTipoEgresoCaja.Equals(Entidades.EgresoCaja.idPagoTarjeta))
                        {
                            MessageBox.Show("No puede modificar los egresos de caja que son por ventas con tarjeta.\n\n",
                            "Egreso caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            return;
                        }

                        //se valida que no sea egreso por Cta Cte
                        if (oEgresoCajaE.esEgresoCtaCte(oEgresoCajaE.IdTipoEgresoCaja))
                        {
                            MessageBox.Show("No puede modificar los egresos de caja que son por Cuenta Corriente.\n\n",
                            "Egreso caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            return;
                        }

                        //se valida que no sea un egreso por compra
                        if (oEgresoCajaE.IdCompra != null && oEgresoCajaE.IdCompra > 0)
                        {
                            DialogResult resp = MessageBox.Show("No puede modificar el egreso de caja porque está asociado por una compra.\n\n"+
                            "Modifique la compra con ID: "+oEgresoCajaE.IdCompra+" y se actualizará automáticamente el egreso de caja asociado a la misma."+
                            "\n\n¿Desea modificar ahora la compra?", 
                            "Egreso caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                            if (resp == DialogResult.Yes)
                            {
                                formNuevaCompra frmNuevaCompra = new formNuevaCompra();
                                frmNuevaCompra.esEgresoCaja = true;
                                frmNuevaCompra.oEgresoCajaE = oEgresoCajaE;
                                frmNuevaCompra.oUsuario = oUsuario;
                                frmNuevaCompra.idCompra = Convert.ToInt32(oEgresoCajaE.IdCompra);
                                frmNuevaCompra.ShowDialog();
                                formAddOrEditEgresoCaja_Load(null, null);
                            }
                            return;
                        }
                        readOnly = false;
                        setearPropiedadesForm();
                    }
                    else
                    {
                        //Cargando TextBox para validar
                        int nroFilas = 0;
                        int nombreTextBox = 1;
                        int valorTextBox = 0;
                        //string[valor_campo][nombre_textBox]
                        string[,] textBoxes = new string[3, 2];
                        textBoxes[nroFilas, valorTextBox] = (int)comboTipoEgresoCaja.SelectedValue == 0 ? "" : "tiene_valor";
                        textBoxes[nroFilas++, nombreTextBox] = lblTipo.Text;

                        textBoxes[nroFilas, valorTextBox] = txtDescripcion.Text;
                        textBoxes[nroFilas++, nombreTextBox] = lblDescripcion.Text;

                        textBoxes[nroFilas, valorTextBox] = txtMonto.Text;
                        textBoxes[nroFilas++, nombreTextBox] = lblMonto.Text;

                        if (Util_Form.validarArrayCamposVacios(textBoxes) && Util_Form.validarFecha(txtFechaEgresoCaja.Value, "Fecha")
                           && Util_Form.validarCampoNumerico(txtMonto.Text, "Monto"))
                        {
                            cargarEgresoCaja();//se cargan datos del egresoCaja
                            if (!huboModificacion)
                            {
                                MessageBox.Show("No se han realizado modificaciones.\n\nPresione Cancelar para salir sin realizar modificaciones", "Egreso caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            //si es modificacion cerrar form
                            bool cerrarForm = oEgresoCajaE.Id > 0;

                            oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);

                            imprimirTicket();
                            MessageBox.Show("El egreso de caja se guardó correctamente.");
                            
                            //si nuevoEgresoCaja es llamado desde FrmEgresos no se cierra
                            if (frmEgresosCaja != null)
                            {
                                frmEgresosCaja.oUsuario = oUsuario;
                                frmEgresosCaja.cargarGrilla();
                                txtFechaEgresoCaja.Focus();

                                //se limpian los campos y objeto
                                oEgresoCajaE = new Entidades.EgresoCaja();
                                comboTipoEgresoCaja.SelectedIndex = 0;
                                txtDescripcion.Text = "";
                                txtMonto.Text = "";
                                txtDetalle.Text = "";

                                if (cerrarForm) this.Close();
                            }
                            else
                            {
                                this.Close();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar egreso de caja.\n\n" + ex.Message,
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void imprimirTicket()
        {
            try
            {
                oEgresoCajaE = oCierreN.getEgresoCajaById(oEgresoCajaE.Id);
                //imprimir ticket
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = checkTicket.Checked;
                ticket.TextoCentro("Egreso Caja");
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                ticket.TextoIzquierda("Sucursal: " + oEgresoCajaE.Sucursal.sucursal);
                ticket.TextoIzquierda("Vendedor: " + oEgresoCajaE.CreadoPorUser.Nombre);
                ticket.TextoIzquierda("Id: " + oEgresoCajaE.Id.ToString());
                ticket.TextoIzquierda("Fecha: " + Utilidades.Util_Form.fechaFormato24Horas(oEgresoCajaE.Fecha));
                ticket.LineasGuion();
                ticket.TextoIzquierda("Tipo: " + oEgresoCajaE.TipoEgresoCaja);
                ticket.TextoMuchasLineas("Descripción: " + oEgresoCajaE.Descripcion);
                ticket.TextoIzquierda("Monto: " + oEgresoCajaE.Monto);
                ticket.TextoMuchasLineas("Detalle: " + oEgresoCajaE.Detalle);
                DateTime? creado = oEgresoCajaE.Id.Equals(0) ? DateTime.Now : oEgresoCajaE.Creado;
                ticket.TextoIzquierda("Creado: " + Utilidades.Util_Form.fechaFormato24Horas(creado));
                if(oEgresoCajaE.Actualizado!= null) ticket.TextoIzquierda("Modif.: " + Utilidades.Util_Form.fechaFormato24Horas(oEgresoCajaE.Actualizado));
                ticket.LineasEnBlanco(5);
                ticket.realizarImpresion();
            }
            catch (Exception)
            {
                MessageBox.Show("Error al imprimir el Ticket");
                return;
            }
        }

        private void cargarEgresoCaja()
        {
            huboModificacion = true;
            //Se valida que se hayan realizado modificaciones en el registro
            if (oEgresoCajaE.Id > 0)
            {
                huboModificacion = !(oEgresoCajaE.Fecha.Equals(txtFechaEgresoCaja.Value) &&
                    oEgresoCajaE.IdTipoEgresoCaja.Equals((int)comboTipoEgresoCaja.SelectedValue) &&
                    oEgresoCajaE.Descripcion.Equals(txtDescripcion.Text) &&
                    oEgresoCajaE.Monto.Equals(Utilidades.Util_Form.convertFloat(txtMonto.Text, false)) &&
                    oEgresoCajaE.Detalle.Equals(txtDetalle.Text) &&
                    oEgresoCajaE.Sucursal.idSucursal.Equals(oSucursalE.idSucursal)
                    );
                if (!huboModificacion) return;
            }

            oEgresoCajaE.Fecha = txtFechaEgresoCaja.Value;
            oEgresoCajaE.IdTipoEgresoCaja = (int)comboTipoEgresoCaja.SelectedValue;
            oEgresoCajaE.TipoEgresoCaja = comboTipoEgresoCaja.Text;
            oEgresoCajaE.Descripcion = comboTipoEgresoCaja.Text;
            oEgresoCajaE.Descripcion = txtDescripcion.Text;
            oEgresoCajaE.Monto = Utilidades.Util_Form.convertFloat(txtMonto.Text, true);
            oEgresoCajaE.Detalle = txtDetalle.Text;
            oEgresoCajaE.Sucursal = oSucursalE;
            oEgresoCajaE.CreadoPor = oEgresoCajaE.Id > 0 ? oEgresoCajaE.CreadoPor : oUsuario.Id;
            oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? oUsuario.Id : 0;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
            }
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

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            checkTicket.Checked = true;
            imprimirTicket();
        }

        private void btnIngresoBilletes_Click(object sender, EventArgs e)
        {
            formIngresoBilletes frmIngresoBilletes = new formIngresoBilletes();
            frmIngresoBilletes.txtBoxAcargar = this.txtMonto;
            frmIngresoBilletes.ShowDialog();
            if (!frmIngresoBilletes.txtBoxAcargar.Text.Equals("0") )
            {
                txtMonto.Text = frmIngresoBilletes.txtBoxAcargar.Text;
            }

            string textoOriginal = txtDetalle.Text;
            string delimitadorInicio = "[";
            string delimitadorFin = "]";
            string textoReemplazo = frmIngresoBilletes.detalleCantBilletes.ToString();

            // Expresión regular para encontrar texto entre delimitadores
            string patron = $@"{Regex.Escape(delimitadorInicio)}(.*?){Regex.Escape(delimitadorFin)}";

            // Reemplazar el texto entre delimitadores
            string resultado = textoOriginal.Contains("[") ?
                Regex.Replace(textoOriginal, patron, $"{delimitadorInicio}{textoReemplazo}{delimitadorFin}") :
                textoOriginal + textoReemplazo;
            resultado = string.IsNullOrEmpty(textoReemplazo) && resultado.Contains("///") ? resultado.Replace("///", "") : resultado;
            txtDetalle.Text =  resultado;
        }
    }
}
