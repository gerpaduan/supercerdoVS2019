using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;
using System.Configuration;
using iTextSharp.text;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System.Diagnostics;
using System.Threading;
using System.Globalization;

namespace Presentacion.Caja
{
    public partial class formUltimaVenta : Form, InterfaceUsuario, InterfacePersona
    {
        public Entidades.Venta oUltimaVenta = new Entidades.Venta();
        List<Entidades.LineaVenta> lineaNuevosAnulados = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();
        Entidades.Usuario oVendedorNuevo;
        public Entidades.CierreCaja oCierreE;

        Negocio.Venta oVentaN = new Negocio.Venta();
        Negocio.Usuario oUsuarioN = new Negocio.Usuario();
        Entidades.LineaVenta oLineaVenta;
        wsAFIPvs2008.formFacturaElectronica formFactElec;

        bool huboModificaciones = false;//se establece true cuando se modificó algo
        bool formCargado = false;
        public float pagoMixtoEfectivo = 0f;

        public formUltimaVenta()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;
        }

        private void formUltimaVenta_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();

            cargarGrilla();
            changeCheckTicket();
            comboTipoComprobante.SelectedItem = oUltimaVenta.TipoComprobante.ToString();
            this.txtCliente.Text = oUltimaVenta.Persona.razonSocial;
            txtFecVenta.Text = oUltimaVenta.FechaVenta.ToString();
            txtVendedor.Text = oUltimaVenta.Vendedor.Nombre;
            txtSucursal.Text = oUltimaVenta.Sucursal.sucursal;
            txtNroTicket.Text = oUltimaVenta.IdVenta.ToString();
            txtCuit.Text = oUltimaVenta.Cuit.ToString();
            txtEmail.Text = oUltimaVenta.Email.ToString();
            txtObservaciones.Text = oUltimaVenta.Observaciones;
            checkCtaCte.Checked = oUltimaVenta.EnCtaCte;
            checkPagoMixto.Checked = oUltimaVenta.PagoMixtoEfectivo > 0;
            switch (oUltimaVenta.FormaPago)
            {
                case "Efectivo":
                    checkEfectivo.Checked = true;
                    //checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Debito":
                    checkDebito.Checked = true;//.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Credito":
                    checkCredito.Checked = true;//.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "CtaCte":
                    checkCtaCtePago.Checked = true;//.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Qr":
                    checkQr.Checked = true;//.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                case "Transferencia":
                    checkTransf.Checked = true;//.BackColor = Utilidades.Util_Form.getBackColorCheckBox(true);
                    break;
                default:
                    break;
            }
            notaCredito.Enabled = oVentaN.existeFactuElectParaVenta(oUltimaVenta.IdVenta) > 0;
            huboModificaciones = false;
            formCargado = true;
        }

        public void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = null;
                cargarListaGrilla();
                grillaLineasVenta.DataSource = listaLineaGrilla;
                if (listaLineaGrilla.Count > 0)
                {
                    grillaLineasVenta.Rows[listaLineaGrilla.Count - 1].Selected = true;
                    grillaLineasVenta.FirstDisplayedScrollingRowIndex = listaLineaGrilla.Count - 1;

                    for (int nroFila = 0; nroFila < grillaLineasVenta.Rows.Count; nroFila++)
                    {
                        foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                        {
                            if (grillaLineasVenta.Rows[nroFila].Cells["Corte"].Value.ToString().Length > 22)
                            {
                                grillaLineasVenta.Rows[nroFila].Cells["Corte"].Style.Font = new System.Drawing.Font(grillaLineasVenta.Font.ToString(), 13);
                            }

                            if (Convert.ToInt64(grillaLineasVenta.Rows[nroFila].Cells["Codigo"].Value) == linea.Corte.codigo && 
                                Convert.ToInt32(grillaLineasVenta.Rows[nroFila].Cells["idLineaVenta"].Value) == linea.IndexAnulado)
                            {
                                grillaLineasVenta.Rows[nroFila].DefaultCellStyle.ForeColor = Color.Red;
                            }
                        }
                    }
                }
                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarListaGrilla()
        {
            LineaVenta lineaVentaP;
            listaLineaGrilla.Clear();

            foreach (Entidades.LineaVenta  lineaE in oUltimaVenta.LineasVenta)
            {
                lineaVentaP = new LineaVenta();
                lineaVentaP.IdLineaVenta = lineaE.IdLineaVenta;
                lineaVentaP.idCorte = lineaE.Corte.idCorte;
                lineaVentaP.codigo = lineaE.Corte.codigo;
                lineaVentaP.corte = lineaE.Corte.corte;
                lineaVentaP.cantKgs = lineaE.CantKg;
                lineaVentaP.precioKg = lineaE.PrecioKg;
                lineaVentaP.totalS = lineaE.PrecioKg * lineaE.CantKg;
                lineaVentaP.IndexAnulado = lineaE.IndexAnulado;

                if (lineaE.Estado == 1)
                {
                    lineaVentaP.estado = "Anulado";
                    lineaVentaP.corte += "(Anulado)";
                }
                else
                {
                    lineaVentaP.estado = "";
                }

                listaLineaGrilla.Add(lineaVentaP);
                lineaVentaP = null;
            }
        }

        private void cargarTotales()
        {
            float totalKgs = 0;
            float totalPesos = 0;

            foreach (LineaVenta linea in listaLineaGrilla)
            {
                totalKgs += linea.cantKgs;
                totalPesos += linea.totalS;
            }

            txtCantItems.Text = grillaLineasVenta.Rows.Count.ToString();
            txtTotalKgs.Text = totalKgs.ToString("F3");
            txtTotalS.Text = totalPesos.ToString("N2");
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oVendedorNuevo = usuario;
        }

        private void cambiarVendedor_Click(object sender, EventArgs e)
        {
            FormLoginVendedor frmLogin = new FormLoginVendedor();
            frmLogin.ShowDialog(this);
            validarAperturaCaja();
            if ( oVendedorNuevo != null && oVendedorNuevo.Id > 0)
            {
                huboModificaciones = true;
                oUltimaVenta.Vendedor = oVendedorNuevo;
                txtVendedor.Text = oVendedorNuevo.Nombre;
            }
            oVendedorNuevo = null;
        }

        private void validarAperturaCaja()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.Sucursal = oUltimaVenta.Sucursal;
            oCierreE.UsuarioInicio = oVendedorNuevo;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !oCierreE.UsuarioCierre.Id.Equals(0))
            {
                MessageBox.Show(oVendedorNuevo.Nombre + ":\nDebes Abrir Caja para poder registrar ventas.", "Abrir Caja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                oVendedorNuevo = null;
            }
        }

        private void ImprimirTicket_Click(object sender, EventArgs e)
        {
            imprimirTicket_menu();
        }

        public void imprimirTicket_menu()
        {
            ///si es factura llamar a generador pdf de factura electronica
            ///            
            if (!oUltimaVenta.TipoComprobante.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString()))
            {
                wsAFIPvs2008.formFacturaElectronica formFactElectronica = new wsAFIPvs2008.formFacturaElectronica();
                formFactElectronica.oFactuElec = oVentaN.getFactuElecById(oVentaN.esVentaSinFacturar(oUltimaVenta.IdVenta, false));
                //formFactElectronica.pdf_FacturaMetodo();
                formFactElectronica.imprimirTicket(formFactElectronica.oFactuElec.esFacturaA(formFactElectronica.oFactuElec.CodTipoCbteAfip.ToString()),
                    DialogResult.Yes);
                formFactElectronica.Close();
                return;
            }
            imprimirTicket();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                //Validación que es llamado desde POS
                if (oCierreE != null)
                {
                    Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                    bool cajaAbierta = oCierreN.validarCajaAbiertaVendedor(oUltimaVenta.FechaVenta, oUltimaVenta.Sucursal, oCierreE.UsuarioInicio);
                    if (!cajaAbierta)
                    {
                        MessageBox.Show(oCierreE.UsuarioInicio.Nombre + " la caja ha sido cerrada.\n\n" , "Caja Cerrada", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }

                if (oCierreE == null || !(oUsuarioN.tienePermiso(oCierreE.UsuarioInicio, this.Name, oUltimaVenta.FechaVenta, oCierreE.UsuarioInicio.Id)))
                {
                    Utilidades.Mensajes.ErrorPermisoEdicion();
                    return;
                }

                //Solicita que ingrese Forma de Pago
                if (!ingresarFormaPago())
                {
                    MessageBox.Show("Ingrese una Forma de Pago.",
                        "Verifique la forma de pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //se valida pago mixto, si deshabilita check significa que no cumple con las restricciones
                if (checkPagoMixto.Checked && huboModificaciones)
                {
                    validarPagoMixto();
                    if (!checkPagoMixto.Checked)
                        return;

                    ///si está tildado Pago Mixto
                    ///mostrar form y calcular los diferentes montos y los egresos segun la forma de pago
                    ///
                    formPagoMixto formPagoMixto = new formPagoMixto();
                    formPagoMixto.totalPesos = oUltimaVenta.TotalImporte;
                    formPagoMixto.formaPago = oUltimaVenta.FormaPago;
                    formPagoMixto.pagoMixtoEfectivo = oUltimaVenta.PagoMixtoEfectivo;
                    formPagoMixto.formUltimaVenta = this;
                    formPagoMixto.ShowDialog();

                    //si le dio al boton ingresar en form pago mixto continuar sino return false
                    if (!(pagoMixtoEfectivo > 0))
                        return;
                }

                oUltimaVenta.PagoMixtoEfectivo = checkPagoMixto.Checked ? pagoMixtoEfectivo : 0;
                

                //valida que un venta en CTA CTE sea solo en Cta Cte
                if (checkCtaCte.Checked && (!oUltimaVenta.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                    oUltimaVenta.Persona.idPersona.Equals(Entidades.Parametros.idConsumidorFinal)))
                {
                    MessageBox.Show("Las ventas en cuenta corriente (CTA.CTE.) no pueden ser a Consumidor Final y debe seleccionar Cta.Cte en forma de pago" +
                        "\n\nCorrija y vuelva a finalizar la venta.",
                        "Verifique la forma de pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (huboModificaciones || (oUltimaVenta.Observaciones != txtObservaciones.Text))
                {
                    oUltimaVenta.Observaciones = txtObservaciones.Text;

                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea modificar los datos de la venta?", "Modificar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta.Equals(DialogResult.Yes))
                    {
                        oVentaN.modificarVenta(oUltimaVenta, oUltimaVenta.Sucursal.IdSucursal, false, lineaNuevosAnulados);
                            
                        if(checkTicket.Checked)
                            imprimirTicket();

                        huboModificaciones = false;//se establece FALSE para evitar mensaje de salida del form
                        this.Close();
                    }
                }
                else
                {
                    MessageBox.Show("No se realizaron cambios en la venta para guardar.\nPara cerrar la ventana presione el botón Salir");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar la modificación en la venta.\n\n"+ex.Message);
            }
        }

        private bool ingresarFormaPago()
        {
            bool resp = true;
            if (string.IsNullOrEmpty(oUltimaVenta.FormaPago))
            {
                //si ninguna forma de pago está seleccionada no se valida
                if (checkEfectivo.Checked == false && checkDebito.Checked == false && checkCredito.Checked == false
                    && checkCtaCtePago.Checked == false && checkQr.Checked == false && checkTransf.Checked == false)
                    return false;
            }
            return resp;
        }

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            buscarCliente();
        }

        private void buscarCliente()
        {
            formBuscarPersona frmBuscarPersona = new formBuscarPersona();
            frmBuscarPersona.Show(this);
        }

        public void EnviarPersona(Entidades.Persona persona)
        {
            huboModificaciones = true;
            oUltimaVenta.Persona = persona;
            restablecerFormaDePago();
            //unchecked todos las formas de pago para que las vuelva a ingresar y evitar algun error por descuido
            //con clientes en cta cte.
            checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
            this.txtCliente.Text = oUltimaVenta.Persona.razonSocial;
        }

        private void txtObservaciones_TextChanged(object sender, EventArgs e)
        {
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void formUltimaVenta_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private bool salir()
        {
            bool resp = false;
            if (huboModificaciones)
            {
                string mensaje = "Si sale no se guardarán los datos modificados.\n\n¿Está seguro que desea salir?.";
                if (MessageBox.Show(mensaje, "Salir", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2).Equals(DialogResult.No))
                {
                    resp = true;
                }
            }
            return resp;
        }
        
        private void imprimirTicket()
        {
            try
            {
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                ticket.imprimir = true;
                ticket.TextoCentro("x");
                ticket.NoValidoComoFactura();
                ticket.TextoCentro(ConfigurationManager.AppSettings["Negocio"].ToString());
                string NegocioAgregado1 = ConfigurationManager.AppSettings["NegocioAgregado1"].ToString();
                string NegocioAgregado2 = ConfigurationManager.AppSettings["NegocioAgregado2"].ToString();
                string NegocioAgregado3 = ConfigurationManager.AppSettings["NegocioAgregado3"].ToString();
                string NegocioAgregado4 = ConfigurationManager.AppSettings["NegocioAgregado4"].ToString();

                if (!(NegocioAgregado1.Equals("-") || string.IsNullOrEmpty(NegocioAgregado1)))
                    ticket.TextoCentro(NegocioAgregado1);
                if (!(NegocioAgregado2.Equals("-") || string.IsNullOrEmpty(NegocioAgregado2)))
                    ticket.TextoCentro(NegocioAgregado2);
                if (!(NegocioAgregado3.Equals("-") || string.IsNullOrEmpty(NegocioAgregado3)))
                    ticket.TextoIzquierda(NegocioAgregado3);
                if (!(NegocioAgregado4.Equals("-") || string.IsNullOrEmpty(NegocioAgregado4)))
                    ticket.TextoIzquierda(NegocioAgregado4);

                ticket.LineasEnBlanco(1);
                if (oUltimaVenta.EnCtaCte && oUltimaVenta.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                    ticket.TextoCentro("A Cta. Cte.");
                ticket.TextoIzquierda("A " + oUltimaVenta.Persona.razonSocial);
                string formaPagoImprimir = oUltimaVenta.PagoMixtoEfectivo > 0 ? oUltimaVenta.FormaPago.ToString() + "|Efvo" : oUltimaVenta.FormaPago.ToString();
                ticket.TextoIzquierda("Forma Pago: " + formaPagoImprimir);
                ticket.TextoIzquierda("Nro. T. " + oUltimaVenta.IdVenta.ToString());
                ticket.TextoExtremos("Fecha: " + oUltimaVenta.FechaVenta.Date.ToString(), "Hora: " + oUltimaVenta.FechaVenta.TimeOfDay.ToString());
                ticket.LineasGuion();

                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                {
                    //ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                    //    linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);

                    ticket.AgregaArticulo(linea.Corte.corte.ToString(),
                        linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                }
                ticket.TextoDerecha("-------");
                ticket.AgregaTotales("Total", (double)oUltimaVenta.getImporteVenta(oUltimaVenta));//Convert.ToDouble(txtTotalS.Text));
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Articulos: " + oUltimaVenta.getCantItems(oUltimaVenta).ToString());//txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                ticket.TextoIzquierda("Cajero: " + oUltimaVenta.Vendedor.Id);
                ticket.GraciasPorSuCompra();
                ticket.LineasEnBlanco(2);
                ticket.realizarImpresion();
                ticket.CortaTicket();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error a imprimir el ticket.\n\n" + ex.Message, "Error ticket");
            }
        }

        private void anularCorte()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla

                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
                oLineaVentaSelect = oUltimaVenta.LineasVenta[nroFila];

                bool existeAnulado = false;
                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                {
                    if (Entidades.LineaVenta.esAnulado(oLineaVentaSelect.Estado) || oLineaVentaSelect.IdLineaVenta == linea.IndexAnulado)
                    {
                        existeAnulado = true;
                        break;
                    }
                }

                if (!existeAnulado)
                {
                    string datosLinea = "\n\n Datos del Corte \n-----------------------------------------\n " +
                        oLineaVentaSelect.Corte.corte +
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.PrecioKg;
                    string mensaje = "¿Está seguro de anular el Producto seleccionado?" + datosLinea;
                    DialogResult respuesta = MessageBox.Show(mensaje, "Anular Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = oLineaVentaSelect.Corte;
                        oLineaVenta.Venta = oLineaVentaSelect.Venta;
                        oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
                        oLineaVenta.KgsAjusteTarj = oLineaVenta.KgsAjusteTarj * -1;
                        oLineaVenta.KgsRedondeo = oLineaVenta.KgsRedondeo * -1;
                        oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.IndexAnulado = oLineaVentaSelect.IdLineaVenta;

                        lineaNuevosAnulados.Add(oLineaVenta);
                        oUltimaVenta.LineasVenta.Add(oLineaVenta);
                        cargarListaGrilla();
                        cargarGrilla();
                        cargarTotales();
                        huboModificaciones = true;
                    }
                }
                else
                {
                    MessageBox.Show("El Producto seleccionado ya ha sido anulado.", "Anular Producto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            oLineaVenta = null;
        }

        private void grillaLineasVenta_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // Ignore clicks that are not on button cells.  
            if (e.RowIndex < 0 || e.ColumnIndex !=
                grillaLineasVenta.Columns["btnAnular"].Index) return;

            anularCorte();
        }


        #region FormaPago
        private void restablecerFormaDePago()
        {
            if (!formCargado)
                return;
            huboModificaciones = true;
            oUltimaVenta.FormaPago = null;

            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkCtaCtePago.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkQr.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkTransf.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
        }

        private void setFormaDePago()
        {
            restablecerFormaDePago();
            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkEfectivo.Checked);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkDebito.Checked);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCredito.Checked);
            checkCtaCtePago.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCtePago.Checked);
            checkQr.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkQr.Checked);
            checkTransf.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkTransf.Checked);
        }

        private void checkEfectivo_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();
            if (checkEfectivo.Checked)
            {
                checkDebito.Checked = checkCredito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString();
                
            }
        }

        private void checkDebito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkDebito.Checked)
            {
                checkEfectivo.Checked = checkCredito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.Debito.ToString();
                
            }
        }

        private void checkCredito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCredito.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.Credito.ToString();
                
            }
        }


        private void checkCtaCtePago_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCtaCtePago.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.CtaCte.ToString();
                //panelCtaCte.Visible = true;                
            }
            checkCtaCte.Checked = checkCtaCtePago.Checked;
            oUltimaVenta.EnCtaCte = checkCtaCtePago.Checked;
        }

        private void checkQr_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkQr.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkCtaCtePago.Checked =
                    checkTransf.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.Qr.ToString();
                
            }
        }

        private void checkTransf_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkTransf.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked =
                    checkCtaCtePago.Checked = checkQr.Checked = false;
                oUltimaVenta.FormaPago = Entidades.Venta.formaPagoEnum.Transferencia.ToString();
                
            }
        }

        #endregion

        private void checkTicket_CheckedChanged(object sender, EventArgs e)
        {
            changeCheckTicket();
        }

        private void changeCheckTicket()
        {

            checkTicket.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkTicket.Checked);
        }

        private void checkCtaCte_CheckedChanged(object sender, EventArgs e)
        {
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);

            if(!huboModificaciones)
                huboModificaciones = checkCtaCte.Checked != oUltimaVenta.EnCtaCte;
            oUltimaVenta.EnCtaCte = checkCtaCte.Checked;
        }

        private void comboTipoComprobante_SelectedIndexChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
            if (comboTipoComprobante.SelectedItem.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString()))
            {
                txtCuit.ReadOnly = txtEmail.ReadOnly = true;
            }
            else
            {
                txtCuit.ReadOnly = txtEmail.ReadOnly = false;
                txtCuit.Focus();
            }
            oUltimaVenta.TipoComprobante = Convert.ToChar(comboTipoComprobante.SelectedItem);
        }

        private void txtCuit_TextChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
            oUltimaVenta.Cuit = txtCuit.Text;
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {
            huboModificaciones = true;
            oUltimaVenta.Email = txtEmail.Text;
        }

        private void facturaElectronica_Click(object sender, EventArgs e)
        {
            facturaElectronica();
        }

        private void facturaElectronica()
        {
            bool formFactuElec_Abierto = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(wsAFIPvs2008.formFacturaElectronica))
                {
                    formFactElec = (wsAFIPvs2008.formFacturaElectronica)frm;
                    if (formFactElec.idVenta > 0 && formFactElec.facturaPendiente)
                    {
                        foreach (Control ctrl in frm.Controls)
                        {
                            if (ctrl.Name.Equals("txtIdVenta") && ctrl.Text.Equals(oUltimaVenta.IdVenta.ToString()))
                            {
                                formFactuElec_Abierto = true;
                                frm.BringToFront();
                                frm.Visible = false;
                                frm.ShowDialog();
                                break;
                            }
                        }

                        ///Si Form de Factura no está abierto para el idVenta se informa y se abre otro form
                        if (!formFactuElec_Abierto)
                        {
                            MessageBox.Show("Hay una factura pendiende de registrar. Se abrirá otra ventana de facturacion");
                        }
                        break;
                    }

                    if (oUltimaVenta.IdVenta > 0)//Solo se pasa el idVenta si es nuevo
                    {
                        formFactElec.idVenta = oUltimaVenta.IdVenta;
                        formFactElec.cargarDatosAfip = false;
                        formFactElec.cargarVenta();
                    }
                    frm.BringToFront();
                    formFactuElec_Abierto = true;
                    formFactElec.logueado = FormPrincipal.logueado;
                    this.Visible = false;
                    break;
                }
            }

            if (!formFactuElec_Abierto)
            {
                formFactElec = new wsAFIPvs2008.formFacturaElectronica();
                formFactElec.idVenta = oUltimaVenta.IdVenta;
                formFactElec.logueado = FormPrincipal.logueado;
                formFactElec.esShowDialog = true;
                formFactElec.ShowDialog();
            }
        }

        private void checkPagoMixto_CheckedChanged(object sender, EventArgs e)
        {
            if (!formCargado)
                return;

            huboModificaciones = true;
            validarPagoMixto();
        }

        private void validarPagoMixto()
        {
            if (checkPagoMixto.Checked && (checkEfectivo.Checked || checkCtaCte.Checked || oUltimaVenta.FormaPago == null))
            {
                MessageBox.Show("Para 'Pago Mixto' debe seleccionar una forma pago y ser diferente a Efectivo y Cta.Cte", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkPagoMixto.Checked = false;
                checkEfectivo.Checked = false;
                checkCtaCte.Checked = false;
            }
        }

        private void notaCredito_Click(object sender, EventArgs e)
        {
            wsAFIPvs2008.formFacturaElectronica formFactElecNotaCredito = new wsAFIPvs2008.formFacturaElectronica();
            formFactElecNotaCredito.idVenta = oUltimaVenta.IdVenta;
            formFactElecNotaCredito.notaCredito = true;
            formFactElecNotaCredito.logueado = FormPrincipal.logueado;
            formFactElecNotaCredito.esShowDialog = true;
            formFactElecNotaCredito.ShowDialog();
        }

        private void pdf_Click(object sender, EventArgs e)
        {
            PDF_menu();
        }

        public void PDF_menu()
        {
            string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
            ruta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), ruta);// "ReciboCheques.pdf");
            //GenerarPDFRecibo(oUltimaVenta, ruta);
            GenerarPDF(ruta);
            System.Diagnostics.Process.Start(ruta);
        }

        #region imprimirRecibo
        public void GenerarPDF(string rutaDestino)
        {
            ///si es factura llamar a generador pdf de factura electronica
            ///            
            if (!oUltimaVenta.TipoComprobante.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString()))
            {
                wsAFIPvs2008.formFacturaElectronica  formFactElectronica = new wsAFIPvs2008.formFacturaElectronica();
                formFactElectronica.oFactuElec = oVentaN.getFactuElecById(oVentaN.esVentaSinFacturar(oUltimaVenta.IdVenta, false));
                formFactElectronica.pdf_FacturaMetodo();
                formFactElectronica.Close();
                return;
            }

            string ruta = rutaDestino;
            string rutaPDF = rutaDestino + "\\" + oUltimaVenta.FechaVenta.ToString("yyyyMMdd") + " - Comprobante X - ID " + 
                oUltimaVenta.IdVenta.ToString() + ".pdf";

            Document doc = new Document(PageSize.A4, 30, 30, 20, 20);
            PdfWriter.GetInstance(doc, new FileStream(rutaPDF, FileMode.Create));


            doc.Open();

            // Fuentes y estilos
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fontComments = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var fontNormalBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

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
            //izquierda.AddElement(new Paragraph("Razón Social: " + ConfigurationManager.AppSettings["Dueno"].ToString() + "\n", fuenteRazonSocial));
            //izquierda.AddElement(new Paragraph(ConfigurationManager.AppSettings["Direccion"].ToString() + " - " + ConfigurationManager.AppSettings["Localidad"].ToString() + "\n", fuenteRazonSocial));
            //izquierda.AddElement(new Paragraph("Cond.IVA: " + ConfigurationManager.AppSettings["CondicionIVA"].ToString() + "\n", fuenteRazonSocial));
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

            derecha.AddElement(new Paragraph("N°Comprobante: " + oUltimaVenta.Sucursal.idSucursal.ToString() + " - " + oUltimaVenta.IdVenta.ToString() + "\n", fuenteNegrita));
            derecha.AddElement(new Paragraph("Fecha: " + oUltimaVenta.FechaVenta.Date.ToString("dd/MM/yyyy") + "\n\n", fuenteNormal));
            //derecha.AddElement(new Paragraph(ConfigurationManager.AppSettings["IIBB"] + "\n", fuenteNormal));
            //derecha.AddElement(new Paragraph("CUIT: " + ConfigurationManager.AppSettings["cuit"] + "\n", fuenteNormal));
            //derecha.AddElement(new Paragraph("Inicio Act.: " + ConfigurationManager.AppSettings["InicioActividades"] + "\n", fuenteNormal));

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
            cliente.AddCell(CeldaSimple(oUltimaVenta.Persona.razonSocial.ToUpper(), fuenteNormal));

            cliente.AddCell(CeldaSimple("Cond. IVA:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oUltimaVenta.Persona.Iva, fuenteNormal));

            cliente.AddCell(CeldaSimple("Domicilio:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oUltimaVenta.Persona.Domicilio.ToUpper(), fuenteNormal));

            cliente.AddCell(CeldaSimple("CUIT:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oUltimaVenta.Persona.Cuit, fuenteNormal));

            cliente.AddCell(CeldaSimple("Forma pago:", fuenteNegrita));
            cliente.AddCell(CeldaSimple(oUltimaVenta.FormaPago, fuenteNormal));

            cliente.AddCell(CeldaSimple("", fuenteNegrita));
            cliente.AddCell(CeldaSimple("", fuenteNormal));

            doc.Add(cliente);

            doc.Add(linea);
            doc.Add(new Paragraph(" ")); // Espacio


            Entidades.FacturaElectronica oDocumentoImprimir = new Entidades.FacturaElectronica(); //notaCredito ? oNotaCredito : oFactuElec;
            oDocumentoImprimir.Venta = oUltimaVenta;

            #region tabla de productos
            char letraFactura = oUltimaVenta.TipoComprobante;
            int cantCol = letraFactura == 'A' ? 5 : 4;
            PdfPTable productosTable = new PdfPTable(cantCol);
            productosTable.WidthPercentage = 100;


            if (letraFactura == 'A')
                productosTable.SetWidths(new float[] { 6f, 2f, 2f, 2f, 2f });
            else
                productosTable.SetWidths(new float[] { 6f, 2f, 2f, 2f });


            string[] headers = { "Descripción", "Cantidad", "Precio Un", "Importe" };
            foreach (var h in headers)
            {
                var celda = new PdfPCell(new Phrase(h, fuenteNegrita));
                celda.BackgroundColor = new BaseColor(255, 200, 200);
                celda.HorizontalAlignment = Element.ALIGN_CENTER;
                productosTable.AddCell(celda);
            }

            foreach (Entidades.LineaVenta item in oDocumentoImprimir.Venta.LineasVenta)
            {
                productosTable.AddCell(new PdfPCell(new Phrase(item.Corte.codigo.ToString() + " - " + item.Corte.corte, fontNormal)) { Border = 0 });
                productosTable.AddCell(new PdfPCell(new Phrase(item.CantKg.ToString("F3"), fontNormal)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                productosTable.AddCell(new PdfPCell(new Phrase(item.PrecioKg.ToString("#,##0.00", new CultureInfo("es-AR")), fontNormal)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                if (letraFactura == 'A')
                    productosTable.AddCell(new PdfPCell(new Phrase(item.AlicuotaIva.ToString("#,##0.00", new CultureInfo("es-AR")), fontNormal)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                productosTable.AddCell(new PdfPCell(new Phrase((item.PrecioKg * item.CantKg).ToString("#,##0.00", new CultureInfo("es-AR")), fontNormal)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

            }

            doc.Add(productosTable);

            #endregion

            // Agregar la línea al doc
            doc.Add(linea);
            doc.Add(new iTextSharp.text.Paragraph("\n"));

            // Totales
            PdfPTable totalTable = new PdfPTable(3);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 5f, 1f, 1f });

            totalTable.AddCell(new PdfPCell(new Phrase("", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            totalTable.AddCell(new PdfPCell(new Phrase("Total: $", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            totalTable.AddCell(new PdfPCell(new Phrase(oDocumentoImprimir.Venta.TotalImporte.ToString("#,##0.00", new CultureInfo("es-AR")), fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

            doc.Add(totalTable);

            // Agregar la línea al doc
            doc.Add(linea);

            doc.Add(new iTextSharp.text.Paragraph(" "));

            // Obs
            PdfPTable obs = new PdfPTable(3);
            obs.WidthPercentage = 100;
            obs.SetWidths(new float[] { 5f, 1f, 1f });
            string observaciones = string.IsNullOrEmpty(txtObservaciones.Text) ? "" : "obs:  " + txtObservaciones.Text;
            obs.AddCell(new PdfPCell(new Phrase(observaciones, fontComments)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
            obs.AddCell(new PdfPCell(new Phrase("", fontComments)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
            obs.AddCell(new PdfPCell(new Phrase("", fontComments)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });

            doc.Add(obs);

            doc.Close();

            //// Usar Process.Start para abrir el PDF
            Process.Start(new ProcessStartInfo(rutaPDF) { UseShellExecute = true });
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
    }
}
