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

namespace Presentacion.Caja
{
    public partial class formUltimaVenta : Form, InterfaceUsuario, InterfacePersona
    {
        public Entidades.Venta oUltimaVenta = new Entidades.Venta();
        List<Entidades.LineaVenta> lineaNuevosAnulados = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();
        Entidades.Usuario oVendedorNuevo;

        Negocio.Venta oVentaN = new Negocio.Venta();
        Entidades.LineaVenta oLineaVenta;
        wsAFIPvs2008.formFacturaElectronica formFactElec;

        bool huboModificaciones = false;//se establece true cuando se modificó algo
        bool formCargado = false;

        public formUltimaVenta()
        {
            InitializeComponent();
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
            //checkCtaCte.Visible = !oUltimaVenta.Persona.idPersona.Equals(Entidades.Parametros.idConsumidorFinal);

            //setFormaDePago();
            //checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            //checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            //checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
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

            huboModificaciones = false;
            formCargado = true;
            //if (oUltimaVenta.EnCtaCte )
            //{
            //    //btnBuscarCliente.Visible = false;
            //    panelInfoCtaCte.Visible = true;
            //    //MessageBox.Show("No se permiten modificar el cliente en ventas que son a Cuenta Corriente.", "Cta. Cte", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //}
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
                                grillaLineasVenta.Rows[nroFila].Cells["Corte"].Style.Font = new Font(grillaLineasVenta.Font.ToString(), 13);
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
            imprimirTicket();
        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            try
            {
                //valido que la fecha/hora de la venta sea menor al minimo de minutos parametrizado
                TimeSpan diffMinutosTime = DateTime.Now - oUltimaVenta.FechaVenta;
                int diffMinutos = diffMinutosTime.Minutes;

                //si no está loguedo, si fecha venta es distinta a hoy y pasaron minino de minutos para el acceso
                if (!oUltimaVenta.Vendedor.Admin && !Presentacion.FormPrincipal.logueado && !((DateTime.Now.Date == oUltimaVenta.FechaVenta.Date) &&  
                    (Entidades.Parametros.minAccesoUltimaVentaVendedor > diffMinutos)))
                {
                    MessageBox.Show("Ya pasó el mínimo de tiempo requerido para poder modificar la Venta.",
                        "Tiempo expirado", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //Solicita que ingrese Forma de Pago
                if (!ingresarFormaPago())
                {
                    MessageBox.Show("Ingrese una Forma de Pago.",
                        "Verifique la forma de pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //valida que un venta en CTA CTE sea solo en Cta Cte
                if (checkCtaCte.Checked && (!oUltimaVenta.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                    oUltimaVenta.Persona.idPersona.Equals(Entidades.Parametros.idConsumidorFinal)))
                {
                    MessageBox.Show("Las ventas en cuenta corriente (CTA.CTE.) no pueden ser a Consumidor Final y debe seleccionar Cta.Cte en forma de pago" +
                        "\n\nCorrija y vuelva a finalizar la venta.",
                        "Verifique la forma de pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (huboModificaciones)
                {
                    DialogResult respuesta = MessageBox.Show("¿Está seguro que desea modificar los datos de la venta?", "Modificar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta.Equals(DialogResult.Yes))
                    {
                        oVentaN.modificarVenta(oUltimaVenta, oUltimaVenta.Sucursal.IdSucursal, false);

                        foreach (Entidades.LineaVenta lineaNuevoAnulado in lineaNuevosAnulados)
                        {
                            oVentaN.agregarLineaVenta(lineaNuevoAnulado);
                        }

                        formVentaCaja fVtaCaja = new formVentaCaja();
                        //se genera el egreso de caja si no es Efectivo
                        fVtaCaja.egresoCajaPagoTarjeta(oUltimaVenta);

                        //Agregar en Cta Cte
                        try
                        {
                            oVentaN.crearMovCtaCteVenta(oUltimaVenta);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al crear el Movimiento en la Cuenta Corriente.\n\n"+
                                "**La Venta se registró correctamente**\n\n" + ex.Message + "\n" + ex.Source);
                        }

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
            huboModificaciones = true;            
            oUltimaVenta.Observaciones = txtObservaciones.Text;
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
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("A " + oUltimaVenta.Persona.razonSocial);
                ticket.TextoIzquierda("Nro. T. " + oUltimaVenta.IdVenta.ToString());
                ticket.TextoExtremos("Fecha: " + oUltimaVenta.FechaVenta.Date.ToString(), "Hora: " + oUltimaVenta.FechaVenta.TimeOfDay.ToString());
                ticket.LineasGuion();

                foreach (Entidades.LineaVenta linea in oUltimaVenta.LineasVenta)
                {
                    ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                        linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                }
                ticket.TextoDerecha("-------");
                ticket.AgregaTotales("Total", Convert.ToDouble(txtTotalS.Text));
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                ticket.TextoIzquierda("Cajero: " + oUltimaVenta.Vendedor.Id);
                ticket.GraciasPorSuCompra();
                ticket.LineasEnBlanco(2);
                ticket.realizarImpresion();
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
                    string mensaje = "¿Está seguro de anular el corte seleccionado?" + datosLinea;
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
                    MessageBox.Show("El corte seleccionado ya ha sido anulado.", "Anular corte", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
    }
}
