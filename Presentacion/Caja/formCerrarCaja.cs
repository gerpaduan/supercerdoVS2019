using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Windows.Forms;
using Utilidades;
using Presentacion.Ventas;

namespace Presentacion.Caja
{
    public partial class formCerrarCaja : Form
    {
        protected Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        protected Negocio.Sucursal oSucursalN = new Negocio.Sucursal();

        public Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
        protected Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        public Entidades.Usuario oUserIncio = new Entidades.Usuario();
        public Entidades.Usuario oUserCierre = new Entidades.Usuario();

        protected enum tipoCierre { AbrirCaja, CerrarCaja };
        protected tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;

        public formCerrarCaja()
        {
            InitializeComponent();
        }

        private void formCerrarCaja_Load(object sender, EventArgs e)
        {
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUserIncio;
            validarAperturaForm();
            txtSucursal.Text = oSucursalE.sucursal;
            txtFechaHoraCierre.Text = DateTime.Now.ToString();
        }

        private void btnCerrarCaja_Click(object sender, EventArgs e)
        {
            cargarCierreCaja();
        }

        protected void cargarCierreCaja()
        {
            try
            {
                if (validaciones())
                {
                    if (tipoCierreActual.Equals(tipoCierre.AbrirCaja))
                    {
                        oCierreE = new Entidades.CierreCaja();
                        oCierreE.Sucursal = oSucursalE;
                        oCierreE.UsuarioInicio = oUserIncio;
                        oCierreE.FechaHoraInicio = Convert.ToDateTime(txtFechaHoraInicio.Text);
                    }
                    oCierreE.UsuarioCierre = oUserCierre;
                    oCierreE.FechaHoraCierre = string.IsNullOrEmpty(txtFechaHoraCierre.Text) ? (DateTime?)null : Convert.ToDateTime(txtFechaHoraCierre.Text.ToString());
                    oCierreE.CajaInicio = Util_Form.convertFloat(txtCajaInicial.Text);
                    oCierreE.Ventas = string.IsNullOrEmpty(txtVentas.Text) ? (float?)null : Util_Form.convertFloat(txtVentas.Text);
                    oCierreE.Gastos = string.IsNullOrEmpty(txtGastos.Text) ? (float?)null : Util_Form.convertFloat(txtGastos.Text);
                    oCierreE.CajaCierre = string.IsNullOrEmpty(txtCajaCierre.Text) ? (float?)null : Util_Form.convertFloat(txtCajaCierre.Text);
                    oCierreE.Diferencia = string.IsNullOrEmpty(txtDiferencia.Text) ? (float?)null : Util_Form.convertFloat(txtDiferencia.Text);
                    oCierreE.CajaInicioSiguiente = string.IsNullOrEmpty(txtCajaInicioSiguiente.Text) ? (float?)null : Util_Form.convertFloat(txtCajaInicioSiguiente.Text);
                    oCierreE.ImporteRetirado = string.IsNullOrEmpty(txtImporteRetirado.Text) ? (float?)null : Util_Form.convertFloat(txtImporteRetirado.Text);
                    
                    DialogResult respuesta = DialogResult.No;
                    switch (tipoCierreActual)
                    {
                        case tipoCierre.AbrirCaja:
                            respuesta = MessageBox.Show("¿Está seguro que desea abrir caja?."
                                , "Abrir Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                                MessageBoxDefaultButton.Button2);    
                            break;
                        case tipoCierre.CerrarCaja:
                            respuesta = MessageBox.Show("¿Está seguro que desea cerrar caja?."
                                , "Cerrar Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            break;
                    }

                    if (respuesta == DialogResult.Yes)
                    {
                        oCierreN.addOrEditCierreCaja(oCierreE);
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception cargarCierreCaja");
            }
            
        }

        protected bool validaciones()
        {
            bool resp = true;

            if (oSucursalE == null || txtUserInicio.Text == "" || txtFechaHoraInicio.Text == "" ||
                txtCajaInicial.Text == "" )
            {
                resp = false;
                string mensaje = "Se deben completar los siguientes campos\n";
                if (oSucursalE == null)
                {                    
                    mensaje += "\n- Sucursal";
                }
                if (txtUserInicio.Text == "")
                {
                    mensaje += "\n- Usuario Inicio";
                }
                if (txtFechaHoraInicio.Text == "")
                {
                    mensaje += "\n- Fecha Hora Inicio"; ;
                }
                if (txtCajaInicial.Text == "")
                {
                    mensaje += "\n- Caja Inicial";
                }
                if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
                {
                    mensaje += "\n\nSi no puede completar los mensajes comuníquese con el adminitrador del sistema";
                }
                MessageBox.Show(mensaje);
            }
            if (resp && tipoCierreActual.Equals(tipoCierre.CerrarCaja))
            {
                int nroFilas = 0;
                int nombreTextBox = 1;
                int valorTextBox = 0;
                //string[valor_campo][nombre_textBox]
                string[,] textBoxes = new string[5, 2];
                textBoxes[nroFilas, valorTextBox] = txtUserCierre.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblUsuarioCierre.Text;

                textBoxes[nroFilas, valorTextBox] = txtCajaInicial.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblCajaInicial.Text;

                textBoxes[nroFilas, valorTextBox] = txtVentas.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblVentas.Text;

                textBoxes[nroFilas, valorTextBox] = txtGastos.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblGastos.Text;

                textBoxes[nroFilas, valorTextBox] = txtCajaCierre.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblCajaCierre.Text;

                if (!Utilidades.Util_Form.validarArrayCamposVacios(textBoxes))
                {
                    resp = false;
                }
            }
            return resp;
        }

        private void calcularCierreCaja()
        {
            if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
            {
                float cero = Util_Form.convertFloat("0"),
                cajaInicial = txtCajaInicial.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicial.Text),
                ventas = txtVentas.Text.Equals("") ? cero : Util_Form.convertFloat(txtVentas.Text),
                gastos = txtGastos.Text.Equals("") ? cero : Util_Form.convertFloat(txtGastos.Text),
                cajaCierre = txtCajaCierre.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaCierre.Text),
                cajaInicioSiguiente = txtCajaInicioSiguiente.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicioSiguiente.Text),
                importeRetirado = txtImporteRetirado.Text.Equals("") ? cero : Util_Form.convertFloat(txtImporteRetirado.Text),
                diferencia = 0;

                diferencia = (gastos + cajaCierre) - (cajaInicial + ventas);
                importeRetirado = cajaCierre - cajaInicioSiguiente;
                txtDiferencia.Text = diferencia.ToString("F2");
                txtImporteRetirado.Text = importeRetirado.ToString();
            }             
        }

        private void txtCajaInicial_TextChanged(object sender, EventArgs e)
        {
            if (!(txtCajaInicial.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaInicial.Text, "Caja Inicial")))
            {
                txtCajaInicial.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtVentas_TextChanged(object sender, EventArgs e)
        {
            if (!(txtVentas.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtVentas.Text, "Ventas")))
            {
                txtVentas.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtGastos_TextChanged(object sender, EventArgs e)
        {
            if (!(txtGastos.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtGastos.Text, "Gastos")))
            {
                txtGastos.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtCajaCierre_TextChanged(object sender, EventArgs e)
        {
            if (!(txtCajaCierre.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaCierre.Text, "Caja Cierre")))
            {
                txtCajaCierre.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtCajaInicioSiguiente_TextChanged(object sender, EventArgs e)
        {
            if (!(txtCajaInicioSiguiente.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaInicioSiguiente.Text, "Caja")))
            {
                txtCajaInicioSiguiente.Text = "";
            }
            calcularCierreCaja();
        }

        private void txtImporteRetirado_TextChanged(object sender, EventArgs e)
        {
            if (!(txtImporteRetirado.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtImporteRetirado.Text, "Importe a Retirar")))
            {
                txtImporteRetirado.Text = "";
            }
            calcularCierreCaja();
        }

        private void validarAperturaForm()
        {
            try
            {
                if (tipoCierreActual.Equals(tipoCierre.AbrirCaja))
                {
                    oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
                    if (oCierreE != null && oCierreE.FechaHoraCierre.Equals(null))
                    {
                        MessageBox.Show(oUserIncio.Nombre +" ya ha abierto la caja en la siguiente fecha\n" + "Fecha: " + oCierreE.FechaHoraInicio.ToString() +
                            "\n\nDebe Cerrar Caja para volver a abrir", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    txtUserInicio.Text = oUserIncio.Nombre;
                    txtCajaInicial.Text = oCierreE != null ? oCierreE.CajaInicioSiguiente.ToString() : "";
                }
                if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
                {
                    oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");
                    //if (!oUserCierre.Admin && !oCierreE.UsuarioInicio.Id.Equals(oUserCierre.Id))
                    if (!oUserCierre.Admin)
                    {
                        MessageBox.Show(oUserCierre.Nombre + "\nNo tienes permiso para los cierres de caja.","Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    if (!oCierreE.FechaHoraCierre.Equals(null))
                    {
                        MessageBox.Show("No puede Cerrar Caja porque no se ha iniciado caja anteriormente.\n" + "Fecha Ultimo Cierre: " + oCierreE.FechaHoraCierre.ToString(),
                            "Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }

                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(formVentaCaja))
                        {
                            foreach (Control ctrl in frm.Controls)
                            {
                                if (ctrl.Name.Equals("usuario") && ctrl.Text.Equals(oCierreE.UsuarioInicio.User))
                                {
                                    MessageBox.Show("No puedes cerrar la caja de "+ oCierreE.UsuarioInicio.Nombre +" porque tiene una venta en curso." +
                                        "\n\nCierre la pantalla de ventas correspondiente al vendedor e intente cerrar caja nuevamente",
                                        "Cerrar Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    this.Close();
                                    break;
                                }
                            }
                        }
                    }
                    txtUserInicio.Text = oCierreE.UsuarioInicio.Nombre;
                    txtUserCierre.Text = oUserCierre.Nombre;
                    txtFechaHoraInicio.Text = oCierreE.FechaHoraInicio.ToString();
                    txtFechaHoraCierre.Text = oCierreE.FechaHoraCierre.ToString();
                    txtCajaInicial.Text = oCierreE.CajaInicio.ToString();
                    txtVentas.Text = oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oSucursalE.idSucursal, oCierreE.FechaHoraInicio, DateTime.Now).ToString();
                    txtGastos.Text = oCierreE.Gastos.ToString();
                    txtCajaCierre.Text = oCierreE.CajaCierre.ToString();
                    txtDiferencia.Text = oCierreE.Diferencia.ToString();
                    txtCajaInicioSiguiente.Text = oCierreE.CajaInicioSiguiente.ToString();
                    txtImporteRetirado.Text = oCierreE.ImporteRetirado.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en validarAperturaForm() \n" + ex.Message);
            }
            
        }
    }
}
