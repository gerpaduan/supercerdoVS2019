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
    public partial class formAddOrEditGasto : Form, InterfaceUsuario
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        protected Entidades.Gasto oGastoE = new Entidades.Gasto();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();

        public Entidades.Usuario oUsuario;

        formGastos frmGastos;

        public int idGasto = 0;
        bool readOnly = false;

        public formAddOrEditGasto()
        {
            InitializeComponent();
        }

        private void formAddOrEditGasto_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            try
            {
                bool closeForm = false;
                if (idGasto == 0 && oUsuario == null) closeForm = true;

                if (!closeForm)
                {
                    cargarSucursal();
                    cargarTipoGasto();
                    txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
                    if (idGasto > 0)
                    {
                        oGastoE = oCierreN.getGastoById(idGasto);
                        cargarCampos();
                        readOnly = true;
                        setearPropiedadesForm();
                        idGastoLabel.Text = idGasto.ToString();//asigno id para identificar el formulario al llamar
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
                if (idGasto == 0)this.Close();
            }
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
            this.txtUsuario.Text = oUsuario.Nombre;
        }

        private void setearPropiedadesForm()
        {            
            this.Text = readOnly ? "Info Gasto" : "Modificar";
            this.btnAceptar.Text = readOnly ? "&Modificar" : "&Guardar";
            txtFechaTexto.Visible = readOnly;
            txtFechaTexto.Text = Util_Form.fechaFormato24Horas(txtFechaGasto.Value);
            txtSucursal.Visible = readOnly;
            txtTipoGasto.Visible = readOnly;
            txtSucursal.Text = comboSucursal.Text;
            txtTipoGasto.Text = comboTipoGasto.Text;
            txtDescripcion.ReadOnly = readOnly;
            txtMonto.ReadOnly = readOnly;
            txtDetalle.ReadOnly = readOnly;
        }

        private void cargarCampos()
        {
            //cargar campos en pantalla
            comboSucursal.SelectedValue = oGastoE.Sucursal.idSucursal;
            txtUsuario.Text = oUsuario != null ? oUsuario.Nombre : "-";
            txtFechaGasto.Value = oGastoE.Fecha;
            comboTipoGasto.SelectedValue = oGastoE.IdTipoGasto;
            txtDescripcion.Text = oGastoE.Descripcion;
            txtMonto.Text = oGastoE.Monto.ToString();
            txtDetalle.Text = oGastoE.Detalle;
            txtCreado.Text = Util_Form.fechaFormato24Horas(oGastoE.Creado);
            txtCreadoPor.Text = oGastoE.CreadoPorUser != null ? oGastoE.CreadoPorUser.Nombre : "";
            txtModificado.Text = Util_Form.fechaFormato24Horas(oGastoE.Actualizado);
            txtModifPor.Text = oGastoE.ActualizadoPorUser != null ? oGastoE.ActualizadoPorUser.Nombre : "";
        }

        public void asignarForm(formGastos form)
        {
            frmGastos = form;
        }

        private void cargarSucursal()
        {
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oGastoE.Sucursal = oSucursalE;

            comboSucursal.DataSource = oSucursalN.obtenerSucursales();
            comboSucursal.DisplayMember = "sucursal";
            comboSucursal.ValueMember = "idSucursal";
            comboSucursal.SelectedIndex = idSucursal - 1;
        }

        private void cargarTipoGasto()
        {
            DataTable dtTipoGastos = oCierreN.obtenerTipoGasto();
            dtTipoGastos.Rows[0][1] = dtTipoGastos.Rows[0][0].Equals(0) ? "Seleccione un tipo" : dtTipoGastos.Rows[0][1].ToString();
            comboTipoGasto.DataSource = dtTipoGastos;
            comboTipoGasto.DisplayMember = "tipoGasto";
            comboTipoGasto.ValueMember = "id";
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
                    tienePermiso = oUsuario != null && (oGastoE.CreadoPor == oUsuario.Id || oUsuario.Admin) ? true : false;
                    if (!tienePermiso)
                    {
                        MessageBox.Show("No tiene permisos para modificar gastos de otra persona");
                        oUsuario = null;
                    }
                }

                if (tienePermiso && Util_Form.validarFechaConAdmin((Presentacion.FormPrincipal.logueado || oUsuario.Admin), txtFechaGasto.Value, "Fecha") 
                    && Util_Form.validarSucursal(Presentacion.FormPrincipal.logueado, 
                        Convert.ToInt32(comboSucursal.SelectedValue.ToString())))
                {
                    if (oGastoE.Id > 0 && readOnly)
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
                        textBoxes[nroFilas, valorTextBox] = (int)comboTipoGasto.SelectedValue == 0 ? "" : "tiene_valor";
                        textBoxes[nroFilas++, nombreTextBox] = lblTipo.Text;

                        textBoxes[nroFilas, valorTextBox] = txtDescripcion.Text;
                        textBoxes[nroFilas++, nombreTextBox] = lblDescripcion.Text;

                        textBoxes[nroFilas, valorTextBox] = txtMonto.Text;
                        textBoxes[nroFilas++, nombreTextBox] = lblMonto.Text;

                        //TODO: hacer validacion a los campos y guardar los datos
                        if (Util_Form.validarArrayCamposVacios(textBoxes) && Util_Form.validarFecha(txtFechaGasto.Value, "Fecha")
                           && Util_Form.validarCampoNumerico(txtMonto.Text, "Monto"))
                        {
                            cargarGasto();//se cargan datos del gasto
                            oCierreN.addOrEditGasto(oGastoE);
                            if (frmGastos != null)
                            {
                                frmGastos.cargarGrilla();
                            }
                            //huboModificaciones = false;
                            MessageBox.Show("El gasto se guardó correctamente.");
                            this.Close();
                            //limpiarListas();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar gasto.\n\n"+ex.Message, 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }          
        }

        private void cargarGasto()
        {
            oGastoE.Fecha = txtFechaGasto.Value;
            oGastoE.IdTipoGasto = (int)comboTipoGasto.SelectedValue;
            oGastoE.Descripcion = txtDescripcion.Text;
            oGastoE.Monto = Utilidades.Util_Form.convertFloat(txtMonto.Text, true);
            oGastoE.Detalle = txtDetalle.Text;
            oGastoE.Sucursal = oSucursalE;
            oGastoE.CreadoPor = oGastoE.Id > 0 ? oGastoE.CreadoPor : oUsuario.Id;
            oGastoE.ActualizadoPor = oGastoE.Id > 0 ? oUsuario.Id : 0;
        }

        private void comboSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!comboSucursal.ValueMember.Equals(""))
            {
                int idSucursal = (int)comboSucursal.SelectedValue;
                oSucursalE = oSucursalN.findById(idSucursal);
                oGastoE.Sucursal = oSucursalE;
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
