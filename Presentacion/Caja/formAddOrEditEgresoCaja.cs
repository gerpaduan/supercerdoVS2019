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

namespace Presentacion.Caja
{
    public partial class formAddOrEditEgresoCaja : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.Usuario oUsuario;

        formEgresosCaja frmEgresosCaja;

        public int idEgresoCaja = 0;
        bool readOnly = false;
        bool huboModificacion = false;

        public formAddOrEditEgresoCaja()
        {
            InitializeComponent();
        }

        private void formAddOrEditEgresoCaja_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
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
            txtSucursal.Visible = readOnly;
            txtTipoEgresoCaja.Visible = readOnly;
            txtSucursal.Text = comboSucursal.Text;
            txtTipoEgresoCaja.Text = comboTipoEgresoCaja.Text;
            txtDescripcion.ReadOnly = readOnly;
            txtMonto.ReadOnly = readOnly;
            txtDetalle.ReadOnly = readOnly;
        }

        private void cargarCampos()
        {
            //cargar campos en pantalla
            comboSucursal.SelectedValue = oEgresoCajaE.Sucursal.idSucursal;
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
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oEgresoCajaE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void cargarTiposEgresoCaja()
        {
            DataTable dtTipoEgresosCaja = oCierreN.obtenerTiposEgresoCaja();
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
            try
            {
                bool tienePermiso = true;
                if (oUsuario == null)
                {
                    FormLoginVendedor frmLogin = new FormLoginVendedor();
                    frmLogin.ShowDialog(this);
                    tienePermiso = oUsuario != null && (oEgresoCajaE.CreadoPor == oUsuario.Id || oUsuario.Admin) ? true : false;
                    if (!tienePermiso)
                    {
                        MessageBox.Show("No tiene permisos para modificar gastos de otra persona");
                        oUsuario = null;
                    }
                }

                bool cajaAbierta = (Presentacion.FormPrincipal.logueado || oUsuario.Admin) ? true : validarCajaAbiertaVendedeor();

                if (tienePermiso && cajaAbierta && Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, 
                        Convert.ToInt32(comboSucursal.SelectedValue.ToString())))
                {
                    if (oEgresoCajaE.Id > 0 && readOnly)
                    {
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

                        //TODO: hacer validacion a los campos y guardar los datos
                        if (Util_Form.validarArrayCamposVacios(textBoxes) && Util_Form.validarFecha(txtFechaEgresoCaja.Value, "Fecha")
                           && Util_Form.validarCampoNumerico(txtMonto.Text, "Monto"))
                        {
                            cargarEgresoCaja();//se cargan datos del egresoCaja
                            if (!huboModificacion)
                            {
                                MessageBox.Show("No se han realizado modificaciones.\n\nPresione Cancelar para salir sin realizar modificaciones", "Egreso caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                return;
                            }
                            oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
                            if (frmEgresosCaja != null)
                            {
                                frmEgresosCaja.cargarGrilla();
                            }
                            MessageBox.Show("El egreso de caja se guardó correctamente.");
                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar egreso de caja.\n\n"+ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
    }
}
