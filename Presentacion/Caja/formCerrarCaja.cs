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
        Entidades.CierreCaja oCierreAnterior;


        public enum tipoCierre { AbrirCaja, CerrarCaja, ReAbrirCaja, ModificarCaja };
        public tipoCierre tipoCierreActual = tipoCierre.CerrarCaja;
        bool esModificarCaja = false;

        public formCerrarCaja()
        {
            InitializeComponent();
        }

        private void formCerrarCaja_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text += Utilidades.Conexion.getSucursalConexion();
                int idSucursal = Utilidades.Conexion.getIdSucursalConexion();
                btnImprimir.Visible = tipoCierreActual.Equals(tipoCierre.ModificarCaja);
                if (oCierreE == null || oCierreE.Id == 0)
                {
                    oSucursalE = oSucursalN.findById(idSucursal);
                    oCierreE.Sucursal = oSucursalE;
                    oCierreE.UsuarioInicio = oUserIncio;
                }
                validarAperturaForm();
                txtSucursal.Text = oCierreE.Sucursal.sucursal;
                txtCajaCierre.Select();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                    oCierreE.CajaInicio = Util_Form.convertFloat(txtCajaInicial.Text, true);
                    oCierreE.Ventas = string.IsNullOrEmpty(txtVentas.Text) ? (float?)null : Util_Form.convertFloat(txtVentas.Text, true);
                    oCierreE.EgresosCaja = string.IsNullOrEmpty(txtEgresosCaja.Text) ? (float?)null : Util_Form.convertFloat(txtEgresosCaja.Text, true);
                    oCierreE.CajaCierre = string.IsNullOrEmpty(txtCajaCierre.Text) ? (float?)null : Util_Form.convertFloat(txtCajaCierre.Text, true);
                    oCierreE.Diferencia = string.IsNullOrEmpty(txtDiferencia.Text) ? (float?)null : Util_Form.convertFloat(txtDiferencia.Text, true);
                    oCierreE.CajaInicioSiguiente = string.IsNullOrEmpty(txtCajaInicioSiguiente.Text) ? (float?)null : Util_Form.convertFloat(txtCajaInicioSiguiente.Text, true);
                    oCierreE.ImporteRetirado = string.IsNullOrEmpty(txtImporteRetirado.Text) ? (float?)null : Util_Form.convertFloat(txtImporteRetirado.Text, true);
                    
                    DialogResult respuesta = DialogResult.No;
                    switch (tipoCierreActual)
                    {
                        case tipoCierre.AbrirCaja:
                            respuesta = MessageBox.Show("¿Está seguro que desea abrir caja?."
                                , "Abrir Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, 
                                MessageBoxDefaultButton.Button2);    
                            break;
                        case tipoCierre.CerrarCaja:
                            string pregunta = esModificarCaja ? "--La modificación del cierre de caja puede acarrear "+
                                "errores irreversibles si ingresa datos incorrectos--"+"\n\n¿Está seguro que desea modificar el cierre de caja?." : "¿Está seguro que desea cerrar caja?.";
                            respuesta = MessageBox.Show(pregunta, "Cerrar Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            break;
                        case tipoCierre.ModificarCaja:
                            respuesta = MessageBox.Show("¿Está seguro que desea realizar modificaciones a la caja?." +
                                "\n\nNota: se actualizarán aquellos campos donde hubo modificaciones."
                                , "Modificar Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            break;
                        case tipoCierre.ReAbrirCaja:
                            respuesta = MessageBox.Show("--Si modifica la Fecha Hora Inicio de caja asegúrese que no se interponga con otro " +
                                "cierre de caja para evitar errores--" + "\n\n¿Está seguro que desea re-abrir caja?." +
                                "\n"
                                , "Re-Abrir Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            break;
                    }

                    if (respuesta == DialogResult.Yes)
                    {
                        if (tipoCierreActual.Equals(tipoCierre.ReAbrirCaja))
                        {
                            oCierreE.FechaHoraCierre = null;
                            oCierreE.UsuarioCierre.Id = 0;
                        }
                        if (tipoCierreActual.Equals(tipoCierre.ModificarCaja))
                        {
                            tipoCierreActual = tipoCierre.CerrarCaja;
                            btnCerrarCaja.Text = "&Cerrar Caja";
                            pickerFechaHoraInicio.Visible = true;
                            pickerFechaHoraCierre.Visible = true;
                            pickerFechaHoraCierre.Visible = true;
                            validarAperturaForm();
                            return;
                        }

                        oCierreN.addOrEditCierreCaja(oCierreE);

                        if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
                        {
                            imprimirTicket();
                        }

                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception cargarCierreCaja");
            }
            
        }

        private void imprimirTicket()
        {
            //imprimir ticket
            Ticket.CreaTicket ticket = new Ticket.CreaTicket();
            ticket.imprimir = checkTicket.Checked;

            ///Copia Cajero
            ticket.TextoCentro("Cierre Caja");
            ticket.TextoCentro("--Copia Cajero--");
            ticket.LineasEnBlanco(1);
            //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
            ticket.TextoIzquierda("Vendedor: " + oCierreE.UsuarioInicio.Nombre);
            ticket.TextoIzquierda("Desde: " + oCierreE.FechaHoraInicio.Value.ToString());
            ticket.TextoIzquierda("Hasta: " + oCierreE.FechaHoraCierre.Value.ToString());
            ticket.LineasGuion();
            ticket.AgregaTotales("Caja Inicial", Convert.ToDouble(oCierreE.CajaInicio));
            ticket.LineasEnBlanco(1);
            ticket.AgregaTotales("Diferencia", Convert.ToDouble(oCierreE.Diferencia));
            ticket.AgregaTotales("Queda en Caja:", Convert.ToDouble(oCierreE.CajaInicioSiguiente));
            ticket.LineasEnBlanco(3);
            ticket.realizarImpresion();

            if (MessageBox.Show("¿Imprimir copia para administrador?", "",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1).Equals(DialogResult.Yes))
            {
                ///Copia Encargado 
                ticket.TextoCentro("Cierre Caja");
                ticket.TextoCentro("--Copia Admin--");
                ticket.LineasEnBlanco(1);
                //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                ticket.TextoIzquierda("Vendedor: " + oCierreE.UsuarioInicio.Nombre);
                ticket.TextoIzquierda("Desde: " + oCierreE.FechaHoraInicio.Value.ToString());
                ticket.TextoIzquierda("Hasta: " + oCierreE.FechaHoraCierre.Value.ToString());
                ticket.LineasGuion();
                ticket.AgregaTotales("Caja Inicial", Convert.ToDouble(oCierreE.CajaInicio));
                ticket.AgregaTotales("Ventas", Convert.ToDouble(oCierreE.Ventas));
                ticket.AgregaTotales("EgresosCaja", Convert.ToDouble(oCierreE.EgresosCaja));
                ticket.AgregaTotales("Caja Cierre", Convert.ToDouble(oCierreE.CajaCierre));
                ticket.AgregaTotales("Diferencia", Convert.ToDouble(oCierreE.Diferencia));
                ticket.AgregaTotales("Prox. Caja", Convert.ToDouble(oCierreE.CajaInicioSiguiente));
                ticket.AgregaTotales("Retira", Convert.ToDouble(oCierreE.ImporteRetirado));
                ticket.LineasEnBlanco(3);
                ticket.realizarImpresion();
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

                textBoxes[nroFilas, valorTextBox] = txtEgresosCaja.Text;
                textBoxes[nroFilas++, nombreTextBox] = lblEgresosCaja.Text;

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
                float cero = Util_Form.convertFloat("0", true),
                cajaInicial = txtCajaInicial.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicial.Text, true),
                ventas = txtVentas.Text.Equals("") ? cero : Util_Form.convertFloat(txtVentas.Text, true),
                gastos = txtEgresosCaja.Text.Equals("") ? cero : Util_Form.convertFloat(txtEgresosCaja.Text, true),
                cajaCierre = txtCajaCierre.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaCierre.Text, true),
                cajaInicioSiguiente = txtCajaInicioSiguiente.Text.Equals("") ? cero : Util_Form.convertFloat(txtCajaInicioSiguiente.Text, true),
                importeRetirado = txtImporteRetirado.Text.Equals("") ? cero : Util_Form.convertFloat(txtImporteRetirado.Text, true),
                diferencia = 0;

                diferencia = (gastos + cajaCierre) - (cajaInicial + ventas);
                oCierreE.Diferencia = diferencia;
                txtDiferencia.Text = txtCajaCierre.Text.Length > 0 ? diferencia.ToString("F2") : "";
                if (txtCajaInicioSiguiente.ReadOnly)
                {
                    txtCajaInicioSiguiente.Text = (cajaCierre - importeRetirado).ToString();
                }
                else
                {
                    importeRetirado = cajaCierre - cajaInicioSiguiente;
                    txtImporteRetirado.Text = importeRetirado.ToString();
                }
                //if (txtCajaInicioSiguiente.ReadOnly)
                //{
                //    txtCajaInicioSiguiente.Text = (cajaCierre - importeRetirado).ToString();
                //}
                //else
                //{
                //    importeRetirado = cajaCierre - cajaInicioSiguiente;
                //    txtImporteRetirado.Text = importeRetirado.ToString();
                //}
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

        private void txtEgresosCaja_TextChanged(object sender, EventArgs e)
        {
            if (!(txtEgresosCaja.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtEgresosCaja.Text, "EgresosCaja")))
            {
                txtEgresosCaja.Text = "";
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
            if (!txtCajaInicioSiguiente.ReadOnly)
            {
                if (!(txtCajaInicioSiguiente.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtCajaInicioSiguiente.Text, "Caja")))
                {
                    txtCajaInicioSiguiente.Text = "";
                }
                calcularCierreCaja();                
            }
        }

        private void txtImporteRetirado_TextChanged(object sender, EventArgs e)
        {
            if (!txtImporteRetirado.ReadOnly)
            {
                if (!(txtImporteRetirado.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtImporteRetirado.Text, "Importe a Retirar")))
                {
                    txtImporteRetirado.Text = "";
                }
                calcularCierreCaja();                
            }
        }

        private void validarAperturaForm()
        {
            try
            {
                if (tipoCierreActual.Equals(tipoCierre.AbrirCaja) || tipoCierreActual.Equals(tipoCierre.ReAbrirCaja))
                {
                    panelTaparCamposCierre.BringToFront();
                    btnVerEgresosCaja.TabStop = false;
                    btnCajaAnterior.Visible = false;
                    //Ingreso billetes se modifica la ubicacion del boton
                    btnIngresoBilletes.Location = btnCajaAnterior.Location;

                    //Si Usuario abre caja por primera vez, findByIdOrLast será nulo
                    Entidades.CierreCaja oUltimoCierreUsuario = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
                    if (oUltimoCierreUsuario != null)
                        oCierreE = oUltimoCierreUsuario;

                    if (!tipoCierreActual.Equals(tipoCierre.ReAbrirCaja) && oCierreE.Id != 0 && oCierreE.FechaHoraCierre.Equals(null))
                    {
                        MessageBox.Show(oUserIncio.Nombre +" ya ha abierto la caja en la siguiente fecha\n" + "Fecha: " + oCierreE.FechaHoraInicio.ToString() +
                            "\n\nDebe Cerrar Caja para volver a abrir", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    txtUserInicio.Text = tipoCierreActual.Equals(tipoCierre.ReAbrirCaja) ? oCierreE.UsuarioInicio.Nombre : oUserIncio.Nombre;
                    txtCajaInicial.Text = tipoCierreActual.Equals(tipoCierre.ReAbrirCaja) ? oCierreE.CajaInicio.ToString() : 
                        (oCierreE != null ? oCierreE.CajaInicioSiguiente.ToString() : "");
                    if (tipoCierreActual.Equals(tipoCierre.ReAbrirCaja))
                    {
                        pickerFechaHoraInicio.Visible = true;
                        pickerFechaHoraInicio.Value = oCierreE.FechaHoraInicio.Value;
                        txtFechaHoraInicio.Text = oCierreE.FechaHoraInicio.Value.ToString();
                    }
                }

                if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
                {
                    readOnlyCampos();
                    //si se está modificando no se obtiene el cierre
                    if(!esModificarCaja) 
                        oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");
                    oCierreAnterior = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLastOpen, "");
                    lblDiferenciaEntreCaja.Visible = oCierreAnterior != null && !oCierreAnterior.CajaInicioSiguiente.Equals(oCierreE.CajaInicio);

                    Negocio.Venta oVentaN = new Negocio.Venta();
                    oCierreE.FechaHoraCierre = oCierreE.FechaHoraCierre != null ? oCierreE.FechaHoraCierre : DateTime.Now;
                    lblCortesAnulados.Visible = oVentaN.getVentasVendedorCierreCaja(oCierreE, true).Rows.Count > 0;

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
                    txtVentas.Text = oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oCierreE.Sucursal.idSucursal, 
                        oCierreE.FechaHoraInicio, esModificarCaja ? oCierreE.FechaHoraCierre : DateTime.Now).ToString();
                    oCierreE.EgresosCaja = oCierreN.getMontoEgresosCajaVendedor(oCierreE);
                    txtEgresosCaja.Text = oCierreE.EgresosCaja.ToString();
                    txtCajaCierre.Text = oCierreE.CajaCierre.ToString();
                    txtDiferencia.Text = oCierreE.Diferencia.ToString();
                    txtCajaInicioSiguiente.Text = oCierreE.CajaInicioSiguiente.ToString();
                    txtImporteRetirado.Text = oCierreE.ImporteRetirado.ToString();
                }

                if (tipoCierreActual.Equals(tipoCierre.ModificarCaja))
                {
                    oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindById, "");

                    //si fecha cierre es vacia abrir cada
                    if (oCierreE.FechaHoraCierre == null)
                    {
                        btnCerrarCaja.Text = "&Re-Abrir Caja";
                        tipoCierreActual = tipoCierre.ReAbrirCaja;
                        validarAperturaForm();
                        return;
                    }

                    txtUserInicio.Text = oCierreE.UsuarioInicio.Nombre;
                    txtUserCierre.Text = oUserCierre.Nombre;
                    txtFechaHoraInicio.Text = oCierreE.FechaHoraInicio.ToString();
                    txtFechaHoraCierre.Text = oCierreE.FechaHoraCierre.ToString();
                    pickerFechaHoraInicio.Value = oCierreE.FechaHoraInicio.Value;
                    pickerFechaHoraCierre.Value = oCierreE.FechaHoraCierre.Value;
                    txtCajaInicial.Text = oCierreE.CajaInicio.ToString();
                    txtVentas.Text = oCierreE.Ventas.ToString();
                    txtEgresosCaja.Text = oCierreE.EgresosCaja.ToString();
                    txtCajaCierre.Text = oCierreE.CajaCierre.ToString();
                    txtDiferencia.Text = oCierreE.Diferencia.ToString();
                    txtCajaInicioSiguiente.Text = oCierreE.CajaInicioSiguiente.ToString();
                    txtImporteRetirado.Text = oCierreE.ImporteRetirado.ToString();
                    btnCerrarCaja.Text = "&Modificar Caja";
                    esModificarCaja = true;
                    readOnlyCampos();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error en validarAperturaForm() \n" + ex.Message);
            }            
        }

        private void readOnlyCampos()
        {
            bool esCerrarCaja = tipoCierreActual.Equals(tipoCierre.CerrarCaja);
            txtCajaInicial.ReadOnly = (esModificarCaja && !esCerrarCaja) || (!esModificarCaja && esCerrarCaja);
            txtCajaInicial.TabStop = !txtCajaInicial.ReadOnly;
            txtCajaInicial.BackColor = Util_Form.getBackColorTextBox(txtCajaInicial.ReadOnly);
            panelTaparCamposCierre.Visible = true;
            checkTicket.Visible = true;
            checkTicket.Checked = true;
            controlEleccionImporte.Visible = esCerrarCaja;
            txtCajaCierre.ReadOnly = !esCerrarCaja;
            txtCajaCierre.BackColor = Util_Form.getBackColorTextBox(txtCajaCierre.ReadOnly);
            //txtCajaInicioSiguiente.ReadOnly = !esCerrarCaja;
            //txtCajaInicioSiguiente.BackColor = Util_Form.getBackColorTextBox(txtCajaInicioSiguiente.ReadOnly);
            txtImporteRetirado.ReadOnly = !esCerrarCaja;
            txtImporteRetirado.BackColor = Util_Form.getBackColorTextBox(txtImporteRetirado.ReadOnly);
        }

        private void btnVerEgresosCaja_Click(object sender, EventArgs e)
        {
            formEgresosCajaVendedor frmEgresosCajaVendedor = new formEgresosCajaVendedor();
            frmEgresosCajaVendedor.oCierreE = oCierreE;
            frmEgresosCajaVendedor.ShowDialog();
            //se actualiza el egreso
            actualizarVentas_Egresos();
        }

        private void controlEleccionImporte_ValueChanged(object sender, EventArgs e)
        {
            if (controlEleccionImporte.Value.Equals(0))
            {
                txtCajaInicioSiguiente.ReadOnly = true;
                txtCajaInicioSiguiente.BackColor = SystemColors.ScrollBar;
                txtImporteRetirado.ReadOnly = false;
                txtImporteRetirado.BackColor = SystemColors.Window;
            }
            else
            {
                txtCajaInicioSiguiente.ReadOnly = false;
                txtCajaInicioSiguiente.BackColor = SystemColors.Window;
                txtImporteRetirado.ReadOnly = true;
                txtImporteRetirado.BackColor = SystemColors.ScrollBar;
            }

        }

        private void btnVentas_Click(object sender, EventArgs e)
        {
            formVentasVendedor frmVentasVendedor = new formVentasVendedor();
            frmVentasVendedor.desdeCajaVenta = true;
            frmVentasVendedor.oCierreE = oCierreE;
            frmVentasVendedor.ShowDialog();

            actualizarVentas_Egresos();
        }

        private void actualizarVentas_Egresos()
        {
            if (tipoCierreActual.Equals(tipoCierre.CerrarCaja))
            {
                txtFechaHoraCierre.Text = Util_Form.fechaFormato24Horas(esModificarCaja ? oCierreE.FechaHoraCierre : DateTime.Now);
                oCierreE.FechaHoraCierre = esModificarCaja ? oCierreE.FechaHoraCierre : null;
                txtVentas.Text = oCierreN.obtenerTotalVentas(oCierreE.UsuarioInicio.Id, oCierreE.Sucursal.idSucursal,
                    oCierreE.FechaHoraInicio, esModificarCaja ? oCierreE.FechaHoraCierre : DateTime.Now).ToString();
                txtEgresosCaja.Text  = oCierreN.getMontoEgresosCajaVendedor(oCierreE).ToString();
            }
        }

        private void btnCajaAnterior_Click(object sender, EventArgs e)
        {
            try
            {
                if (oCierreAnterior != null)
                {
                    string mensaje = "Ultimo cierre de " + oCierreAnterior.UsuarioInicio.Nombre + "\n\n" +
                        "Apertura: " + oCierreAnterior.FechaHoraInicio.ToString() + "\nCierre: " + oCierreAnterior.FechaHoraCierre.ToString() +
                        "\n-------------\nQuedó en Caja anterior: " + oCierreAnterior.CajaInicioSiguiente.ToString() + "\nCaja inicio actual: "+oCierreE.CajaInicio+
                        "\n-------------\nDiferencia: " + (oCierreAnterior.CajaInicioSiguiente-oCierreE.CajaInicio).ToString();
                    MessageBox.Show(mensaje, "Caja Anterior");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el cierre de caja anterior.\n\n" + ex.Message);
            }
        }

        private void btnIngresoBilletes_Click(object sender, EventArgs e)
        {
            formIngresoBilletes frmIngresoBilletes = new formIngresoBilletes();
            frmIngresoBilletes.txtBoxAcargar = this.txtCajaCierre;
            frmIngresoBilletes.ShowDialog();
            if (!frmIngresoBilletes.txtBoxAcargar.Text.Equals("0") && 
                (tipoCierreActual.Equals(tipoCierre.AbrirCaja) || tipoCierreActual.Equals(tipoCierre.ReAbrirCaja)))
            {
                txtCajaInicial.Text = frmIngresoBilletes.txtBoxAcargar.Text;
            }
        }

        private void btnReAbrir_Click(object sender, EventArgs e)
        {
            try
            {
                txtCajaInicial.ReadOnly = false;
                txtCajaInicial.BackColor = Util_Form.getBackColorTextBox(txtCajaInicial.ReadOnly);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pickerDate_ValueChanged(object sender, EventArgs e)
        {
            if (esModificarCaja)
            {
                oCierreE.FechaHoraInicio = pickerFechaHoraInicio.Value;
                oCierreE.FechaHoraCierre = pickerFechaHoraCierre.Value;
                validarAperturaForm();
                return;
            }
            if (tipoCierreActual.Equals(tipoCierre.ReAbrirCaja))
            {
                oCierreE.FechaHoraInicio = pickerFechaHoraInicio.Value;
                return;
            }
        }

        private void btnImprimir_Click(object sender, EventArgs e)
        {
            imprimirTicket();
        }
    }
}
