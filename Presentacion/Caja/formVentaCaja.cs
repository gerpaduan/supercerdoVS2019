using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Presentacion.Personas;
using Presentacion.Cortes;
using Presentacion.Caja;
using System.Configuration;

namespace Presentacion.Ventas
{
    public partial class formVentaCaja : Form, InterfaceCorte, InterfacePersona, InterfaceUsuario
    {
        bool pesoBalanza = false;
        Utilidades.SingletonLeerPeso Leer_Peso;
        Utilidades.Util_Form Util_Form = new Utilidades.Util_Form();
        #region variables
        public string vendedor = "-";
        formVentas frmVentas;
        Negocio.Corte oCorteN = new Negocio.Corte();
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        Negocio.Venta oVentaN = new Negocio.Venta();

        Entidades.Compra oCompraE = new Entidades.Compra();
        Entidades.Persona oCliente;
        public Entidades.Usuario oUsuario;
        Entidades.Corte oCorteE;
        Entidades.Sucursal oSucursalE = new Entidades.Sucursal();
        Entidades.Sucursal oSucAnterior = new Entidades.Sucursal();
        Entidades.Venta oVentaE = new Entidades.Venta();
        Entidades.LineaVenta oLineaVenta;
        Entidades.StockCorteSucursal oStockCorteSucursal;
        Entidades.Venta oUltimaVentaVendedor;
        Entidades.TemporalLineaVenta oTemporalLineaVenta = new Entidades.TemporalLineaVenta();

        List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
        List<LineaVenta> listaLineaGrilla = new List<LineaVenta>();

        Color enableColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["enableColor"].ToString()); //SystemColors.Window;
        Color readOnlyColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["readOnlyColor"].ToString());//SystemColors.ScrollBar;
        Color focusColor = ColorTranslator.FromHtml(ConfigurationManager.AppSettings["focusColor"].ToString());//Color.Orange;//Color.NavajoWhite;//Color.MediumAquamarine;

        Color ultimoColor = Color.Green;

        int sucAnterior;
        int tiempoInactivo = 0;
        int tiempoBloqueo = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoBloqueo"].ToString());
        int sumaTitilar = 0;
        int titilarHasta = 2000;
        int tiempoRegistrarTemporal = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoRegistrarTemporal"].ToString());
        string ultimoTextoEnTxtCodigo = "";

        public int SucAnterior
        {
            get { return sucAnterior; }
            set { sucAnterior = value; }
        }

        bool modificar = false;
        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());
        bool cartelPrimerCorteVendedor = Convert.ToBoolean(ConfigurationManager.AppSettings["cartelPrimerCorteVendedor"].ToString());
        bool ultimaVenta = Convert.ToBoolean(ConfigurationManager.AppSettings["ultimaVenta"].ToString());
        string fecha = "", estadoVenta = "";
        float totalCorte, precioKg, cantKg;
        float totalVenta = 0, abona = 0, cambio = 0;
        #endregion


        public formVentaCaja()
        {
            InitializeComponent();

            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            this.KeyPreview = true;

            //asigo sucursal a la venta  
            int idSucursal = Convert.ToInt32(ConfigurationManager.AppSettings["idSucursal"].ToString());
            oSucursalE = oSucursalN.findById(idSucursal);
            oVentaE.Sucursal = oSucursalE;
            this.txtSucursal.Text = oVentaE.Sucursal.sucursal;
            Negocio.Persona oPersonaN = new Negocio.Persona();
            int idConsumidorFinal = Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString());
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            txtFecVenta.Text = DateTime.Now.ToString();
            if (!fecha.Equals(""))
            {
                txtFecVenta.Text = DateTime.Parse(fecha).ToString();
            }

            checkLeerPeso.Visible = (FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPesoCaja"].ToString()));
            checkTicket.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["ticket"].ToString());
        }

        public void EnviarUsuario(Entidades.Usuario usuario)
        {
            oUsuario = usuario;
            this.txtVendedor.Text = oUsuario.Nombre;
        }
        #region Modificar_Venta


        public void parametrosModificacion(formVentas frmVentasParam, Entidades.Venta oVentaParam, List<Entidades.LineaVenta> listaLineaVentaParam, List<LineaVenta> listaLineaGrillaParam)
        {
            modificar = true;
            this.Text = "Modificar Venta";

            frmVentas = frmVentasParam;
            oVentaE = oVentaParam;
            oCliente = oVentaE.Persona;
            oSucursalE = oVentaE.Sucursal;
            oSucAnterior = oVentaParam.Sucursal;

            listaLineaVenta = listaLineaVentaParam;
            listaLineaGrilla = listaLineaGrillaParam;

            cargarCamposVenta();
            cargarGrilla();
        }

        private void cargarCamposVenta()
        {
            txtCliente.Text = oVentaE.Persona.razonSocial;
            txtSucursal.Text = oVentaE.Sucursal.sucursal;
            txtFechaVenta.Value = oVentaE.FechaVenta;
            txtNroRemito.Text = oVentaE.NroRemito;
            txtObservaciones.Text = oVentaE.Observaciones;

            estadoVenta = oVentaE.Estado;
        }

        private void modificarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();
                try
                {

                    oVentaN.modificarVenta(oVentaE, SucAnterior);

                    foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    {
                        oVentaN.agregarLineaVenta(linea);
                    }

                    frmVentas.cargarGrilla();

                    this.Close();

                }
                catch (Exception ex)
                {
                    string g = ex.Message;

                    MessageBox.Show(ex.Message);
                }
            }

        }

        #endregion

        private void esModificacion()
        {
            //si es modificacion o agregacion
            if (modificar)
            {
                modificarVenta();
            }
            else
            {
                if (grillaLineasVenta.SelectedRows.Count > 0)
                {
                    agregarVenta();
                }
                else
                {
                    MessageBox.Show("No se ha cargado ningún corte en la venta. ", "No hay cortes cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                }
            }

        }

        public void cargarGrilla()
        {
            try
            {
                grillaLineasVenta.AutoGenerateColumns = false;
                grillaLineasVenta.DataSource = null;

                grillaLineasVenta.DataSource = listaLineaGrilla;

                if (listaLineaGrilla.Count > 0)
                {
                    grillaLineasVenta.Rows[listaLineaGrilla.Count - 1].Selected = true;
                    grillaLineasVenta.FirstDisplayedScrollingRowIndex = listaLineaGrilla.Count - 1;

                    for (int nroFila = 0; nroFila < grillaLineasVenta.Rows.Count; nroFila++)
                    {
                        
                        foreach (Entidades.LineaVenta linea in listaLineaVenta)
                        {
                            if (Convert.ToInt32(grillaLineasVenta.Rows[nroFila].Cells["Codigo"].Value) == linea.Corte.codigo &&
                                linea.IndexAnulado == nroFila)
                            {
                                grillaLineasVenta.Rows[nroFila].DefaultCellStyle.ForeColor = Color.Red;
                            }
                        }
                        //if (grillaLineasVenta.Rows[nroFila].Cells["ind == nroFila)
                        //{
                        //    grillaLineasVenta.Rows[nroFila].DefaultCellStyle.ForeColor = Color.Red;
                        //}
                        
                    }
                }

                cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void agregarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();

                try
                {
                    oVentaE.IdVenta = oVentaN.agregarVenta(oVentaE);
                    Ticket.CreaTicket ticket = new Ticket.CreaTicket();
                    ticket.imprimir = checkTicket.Checked;
                    ticket.TextoCentro("x");
                    ticket.NoValidoComoFactura();
                    ticket.LineasEnBlanco(1);
                    //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                    ticket.TextoIzquierda("A " + oVentaE.Persona.razonSocial);
                    ticket.TextoIzquierda("Nro. T. " + oVentaE.IdVenta.ToString());
                    ticket.TextoExtremos("Fecha: " + oVentaE.FechaVenta.Date.ToString(), "Hora: " + oVentaE.FechaVenta.TimeOfDay.ToString());
                    //ticket.LineasEnBlanco(0);
                    ticket.LineasGuion();

                    foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    {
                        oVentaN.agregarLineaVenta(linea);
                        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                            linea.PrecioKg, linea.CantKg, linea.PrecioKg * linea.CantKg);
                    }
                    //ticket.LineasEnBlanco(1);
                    ticket.TextoDerecha("-------");
                    ticket.AgregaTotales("Total", totalVenta);
                    abona = abona > 0 ? abona : totalVenta;
                    ticket.AgregaTotales("Pago", abona);
                    ticket.AgregaTotales("Vuelto", cambio);
                    ticket.LineasEnBlanco(1);
                    ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                    //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                    ticket.TextoIzquierda("Cajero: " + oUsuario.Id);
                    ticket.GraciasPorSuCompra();
                    ticket.LineasEnBlanco(2);

                    //lblHoraUltimaVenta.Text = DateTime.Now.ToShortTimeString() + "\n$ " +txtTotalS.Text;
                    oVentaE.IdVenta = 0;
                    limpiarListas();
                    ultimaVentaVendedor();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }
        private void limpiarListas()
        {
            Negocio.Persona oPersonaN = new Negocio.Persona();
            int idConsumidorFinal = Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString());
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            txtFecVenta.Text = DateTime.Now.ToString();
            txtNroRemito.Text = "";
            txtObservaciones.Text = "";
            txtCantItems.Text = "0";
            txtTotalS.Text = "000,00";
            txtAbona.Text = "";
            txtCambio.Text = "";
            panelPago.Visible = false;
            panelAbonar.Visible = true;

            totalVenta = 0;
            abona = 0;
            cambio = 0;

            listaLineaGrilla = new List<LineaVenta>();
            listaLineaVenta = new List<Entidades.LineaVenta>();
            grillaLineasVenta.DataSource = null;
        }

        private void cargarVenta()
        {
            oVentaE.Persona = oCliente;
            oVentaE.Sucursal = oSucursalE;
            oVentaE.TipoVenta = "Caja";
            oVentaE.FechaVenta = Convert.ToDateTime(txtFecVenta.Text);
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
            oVentaE.Turno = "";
            oVentaE.DiaFestivo = "";
            oVentaE.Observaciones = txtObservaciones.Text.Trim();
            oVentaE.Estado = estadoVenta;
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
            txtTotalS.Text = totalPesos.ToString("N2");
            totalVenta = float.Parse(txtTotalS.Text.Trim());
            abonar();
        }

        private void agregarLinea()
        {
            //se asigna la lectura de balanza porque se destilda al desactivarse el form
            pesoBalanza = checkLeerPeso.Checked;
            if (validarLinea())
            {
                try
                {
                    if (grillaLineasVenta.Rows.Count == 0)
                    {
                        txtFecVenta.Text = DateTime.Now.ToString();
                    }
                    cargarLinea();

                    cargarListas();
                    cargarGrilla();
                    oTemporalLineaVenta = null;
                    limpiarCamposCorte();
                    oLineaVenta = null;

                    txtCodigo.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void cargarListas()
        {
            listaLineaVenta.Add(oLineaVenta);
            cargarListaGrilla(oLineaVenta);
        }



        private void sumarCorte(int nroLinea)
        {
            listaLineaVenta[nroLinea].CantKg = listaLineaVenta[nroLinea].CantKg + oLineaVenta.CantKg;
            listaLineaGrilla[nroLinea].cantKgs = listaLineaGrilla[nroLinea].cantKgs + oLineaVenta.CantKg;

            listaLineaGrilla[nroLinea].totalS = listaLineaGrilla[nroLinea].totalS + (oLineaVenta.CantKg * oLineaVenta.PrecioKg);
        }

        private void limpiarCamposCorte()
        {

            txtCodigo.Text = "";
            txtCorte.Text = "";
            txtCantKgs.Text = "";
            txtPrecioKg.Text = "";
            txtTotalCorte.Text = "";

            txtCodigo.Focus();
        }

        private void cargarListaGrilla(Entidades.LineaVenta lineaE)
        {
            LineaVenta lineaVentaP = new LineaVenta();

            lineaVentaP.idCorte = lineaE.Corte.idCorte;
            lineaVentaP.codigo = lineaE.Corte.codigo;
            lineaVentaP.corte = lineaE.Corte.corte;
            lineaVentaP.cantKgs = lineaE.CantKg;
            lineaVentaP.precioKg = lineaE.PrecioKg;
            lineaVentaP.totalS = lineaE.PrecioKg * lineaE.CantKg;

            if (lineaE.Estado == 1)
            {
                lineaVentaP.estado = "Anulado";
            }
            else
            {
                lineaVentaP.estado = "";
            }

            listaLineaGrilla.Add(lineaVentaP);
            lineaVentaP = null;

        }

        private void cargarLinea()
        {
            oLineaVenta = new Entidades.LineaVenta();

            oLineaVenta.Corte = oCorteE;
            oLineaVenta.Venta = oVentaE;

            oLineaVenta.CantKg = cantKg;
            oLineaVenta.PrecioKg = precioKg;
            oLineaVenta.PesoBalanza = pesoBalanza;

            if (oLineaVenta.CantKg < 0)
            {
                oLineaVenta.Estado = 1;//Anulado
            }
            else
            {
                oLineaVenta.Estado = 0;//Activo
            }
        }


        private bool validarLinea()
        {
            string mensaje = "Complete los siguientes campos: ";
            if (txtCodigo.Text.Trim() == "" || txtCantKgs.Text.Trim() == "" || txtPrecioKg.Text.Trim() == "")
            {
                if (txtCodigo.Text.Trim() == "")
                {
                    mensaje += "\n" + "-Código Corte";

                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                }

                else
                {
                    if (oCorteE == null)
                    {
                        MessageBox.Show("El código ingresado no pertenece a ningún corte.", "El Corte no existe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCodigo.Focus();
                    }
                    else
                    {
                        if (txtCantKgs.Text.Trim() == "")
                        {
                            mensaje += "\n" + "-Cant. Kgs";

                        }
                        if (txtPrecioKg.Text.Trim() == "")
                        {
                            mensaje += "\n" + "-Precio Kg";
                        }

                        MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        if(!checkLeerPeso.Checked) txtCantKgs.Focus();
                    }

                }
                return false;
            }
            if (!Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs."))
            {
                if (checkLeerPeso.Checked)
                {
                    btnAgregar.Focus();
                }
                else
                {
                    txtCantKgs.Focus();
                }
                return false;
            } 
            else
            {
                bool esMayorACero = Utilidades.Util_Form.validarNumeroMayorACero(txtCantKgs.Text, "Kgs.");
                if (!esMayorACero && !checkLeerPeso.Checked)
	            {
                    txtCantKgs.Focus();
                    txtCantKgs.SelectAll();
	            }
                return esMayorACero;
            }
        }

        private bool validacionFinal()
        {
            //si es una modificacion y no hay datos en la grilla no valida porque se eliminar la venta
            if (modificar && grillaLineasVenta.Rows.Count == 0)
            {
                DialogResult respuesta;
                respuesta = MessageBox.Show("¿Está seguro que desea eliminar todos los datos de la venta?.", "Eliminar venta", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == DialogResult.Yes)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                string mensaje = "Complete los siguientes campos: ";

                //valido que no haya ningún corte seleccionado al finalizar venta
                if (txtCodigo.Text.Length > 0)
                {
                    mensaje = "No se puede finalizar la venta si existe un corte seleccionado.\n" +
                        "Borre el código e inténtelo nuevamente";
                    MessageBox.Show(mensaje, "Existe un corte seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
                    return false;
                }

                if (txtCliente.Text.Trim() == "")
                {
                    if (txtCliente.Text.Trim() == "")
                    {
                        mensaje += "\n" + "-Cliente";
                    }

                    MessageBox.Show(mensaje, "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;
                }
                else
                {
                    if (totalVenta >= 0)
                    {
                        if ((abona > 0 && cambio < 0) || cambio >= 100)
                        {
                            if (cambio < 0)
                            {
                                mensaje = "El pago del cliente es menor al total de la venta";
                                MessageBox.Show(mensaje, "Error en el pago", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                txtAbona.Select();
                                txtAbona.Focus();
                            }
                            if (cambio >= 100)
                            {
                                mensaje = "El cambio debe ser menor a $100.\nVerifique el pago ingresado e intente finalizar la venta nuevamente.";
                                MessageBox.Show(mensaje, "Error en el pago", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                txtAbona.Select();
                                txtAbona.Focus();
                            }
                            return false;
                        }
                        else
                        {
                            DialogResult respuesta = MessageBox.Show(txtVendedor.Text+"\n¿Finalizar la venta?. ", txtVendedor.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);

                            if (respuesta == System.Windows.Forms.DialogResult.Yes)
                            {
                                txtCodigo.Focus();
                                return true;
                            }
                            else
                            {
                                txtCodigo.Focus();
                                return false;
                            }
                        }
                    }
                    else
                    {
                        mensaje = "No se puede finalizar la venta porque el total a pagar es negativo (menor a cero).\n\nTotal a pagar:  $ " + totalVenta;
                        MessageBox.Show(mensaje, "Error en la venta", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return false;
                    }
                }
            }
        }

        private void quitarLinea()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla

                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
                oLineaVentaSelect = listaLineaVenta[nroFila];

                bool existeAnulado = false;
                foreach (Entidades.LineaVenta linea in listaLineaVenta)
                {
                    if (oLineaVentaSelect.Corte.codigo == linea.Corte.codigo &&
                        linea.IndexAnulado == nroFila)
                    {
                        existeAnulado = true;
                    }
                }

                if (oLineaVentaSelect.Estado == 0 && !existeAnulado)
                {
                    string datosLinea = "\n\n Datos del Corte \n-----------------------------------------\n " +
                        oLineaVentaSelect.Corte.corte +
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.Corte.precioKg;
                    string mensaje = "¿Está seguro de anular el corte seleccionado?" + datosLinea;
                    DialogResult respuesta = MessageBox.Show(mensaje, "Anular Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        oLineaVenta = new Entidades.LineaVenta();

                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = oLineaVentaSelect.Corte;
                        oLineaVenta.Venta = oLineaVentaSelect.Venta;
                        oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
                        oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.IndexAnulado = nroFila;
                        cargarListas();
                        cargarGrilla();

                        txtCodigo.Focus();
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

        private void cargarCorte()
        {
            if (txtCodigo.Text.Trim() != "")
            {
                try
                {
                    oStockCorteSucursal = null;
                    oStockCorteSucursal = new Entidades.StockCorteSucursal();

                    oCorteE = null;
                    oCorteE = new Entidades.Corte();

                    DataTable dtCortes = new DataTable();
                        dtCortes = oCorteN.buscarCodigoCorte(Convert.ToInt32(txtCodigo.Text.Trim()));

                    if (dtCortes.Rows.Count > 0)
                    {
                        foreach (DataRow fila in dtCortes.Rows)
                        {
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt32(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();
                                oCorteE.precioKg = float.Parse(fila["precioKg"].ToString());
                                oCorteE.tipo = fila["tipo"].ToString();
                        }
                        //cargo los campos
                        this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                        this.txtCorte.Text = oCorteE.corte;
                        this.txtPrecioKg.Text = oCorteE.precioKg.ToString("N");
                        cargarTotalCorte();
                    }
                    else
                    {
                        oCorteE = null;
                        this.txtTotalCorte.Text = "";
                        this.txtPrecioKg.Text = "";
                        this.txtCorte.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar corte\n\n" + ex.Message);
                    limpiarCamposCorte();
                }
            }
            else
            {
                precioKg = 0;
                totalCorte = 0;
                txtCorte.Text = null;
                txtTotalCorte.Text = null;
                txtPrecioKg.Text = null;
            }
        }

        private void cargarTotalCorte()
        {
            if (!txtCantKgs.Text.Equals("") && (checkLeerPeso.Checked  || 
                ( !checkLeerPeso.Checked && Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs"))))
            {
                try
                {
                    //bool d = Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs");
                    try
                    {
                        cantKg = float.Parse(txtCantKgs.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        cantKg = float.Parse(txtCantKgs.Text.Trim());
                    }

                    if (oCorteE != null)
                    {
                        try
                        {
                            precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                        }
                        catch (Exception)
                        {
                            try
                            {
                                precioKg = float.Parse(txtPrecioKg.Text.Trim());
                            }
                            catch (Exception)
                            {

                                if (checkLeerPeso.Checked)
                                {
                                    precioKg = 0;
                                }
                            }
                        }

                        //cargo el Temporal de LineaVenta
                        try
                        {
                            oTemporalLineaVenta = new Entidades.TemporalLineaVenta();
                            oTemporalLineaVenta.FechaInicioPesada = DateTime.Now;
                            oTemporalLineaVenta.Corte = oCorteE;
                            oTemporalLineaVenta.Vendedor = oUsuario;
                            oTemporalLineaVenta.Sucursal = oSucursalE;
                            oTemporalLineaVenta.CantKg = cantKg;
                            oTemporalLineaVenta.TotalCorte = cantKg * precioKg;
                        }
                        catch (Exception)
                        {
                        }
                    }
                    totalCorte = cantKg * precioKg;
                    //cargo el txt total corte
                    txtTotalCorte.Text = totalCorte.ToString("N");

                }
                catch (Exception ex)
                {
                    if (txtCantKgs.Text.Trim() != "-" && !checkLeerPeso.Checked)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }

        }

        private void establecerPrecioKg()
        {
            if (!txtTotalCorte.Text.Equals(""))
            {
                try
                {
                    try
                    {
                        totalCorte = float.Parse(txtTotalCorte.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        totalCorte = float.Parse(txtTotalCorte.Text.Trim());
                    }

                    if (cantKg > 0)
                    {
                        precioKg = totalCorte / cantKg;
                        txtPrecioKg.Text = precioKg.ToString();
                    }
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void establecerTotalCorte()
        {
            if (!txtPrecioKg.Text.Equals(""))
            {
                try
                {
                    try
                    {
                        precioKg = float.Parse(txtPrecioKg.Text.Trim(), System.Globalization.NumberStyles.Float, new System.Globalization.CultureInfo("en-US"));
                    }
                    catch (Exception)
                    {

                        precioKg = float.Parse(txtPrecioKg.Text.Trim());
                    }
                    totalCorte = precioKg * cantKg;

                    if (Presentacion.FormPrincipal.logueado)
                    {
                        txtTotalCorte.Text = totalCorte.ToString();
                    }

                }
                catch (Exception)
                {
                    MessageBox.Show("Para fijar el Precio/Kg debe ingresar un precio válido.");
                    txtPrecioKg.Text = "";
                }
            }
        }

        private void txtCantKgs_TextChanged(object sender, EventArgs e)
        {
            cargarTotalCorte();
        }

        private void txtPrecioKg_TextChanged(object sender, EventArgs e)
        {
            establecerTotalCorte();
        }

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            tiempoInactivo = 0;
            agregarLinea();
        }

        private void btnQuitar_Click(object sender, EventArgs e)
        {
            tiempoInactivo = 0;
            quitarLinea();
        }

        private bool salir()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                string mensaje = "No se puede salir porque hay un venta en curso.\n\nFinalice la venta e inténtelo nuevamente.";
                MessageBox.Show(mensaje, "Salir", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = null;

            oCorteE = corte;

            this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            this.txtCorte.Text = oCorteE.corte;

            this.txtCodigo.Focus();

        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            tiempoInactivo = 0;
            esModificacion();
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            this.Close();

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
            oCliente = persona;
            this.txtCliente.Text = oCliente.razonSocial;

        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            if (grillaLineasVenta.Rows.Count.Equals(0))
            {
                titilarTextBoxVendedor();
            }

            //si se borra el corte actual se llama a metodo registrarTemporalLinea
            if (oCorteE != null && !ultimoTextoEnTxtCodigo.Equals(txtCodigo.Text))
            {
                registrarTemporalLineaVenta();
            }
            cargarCorte();
        }

        private void txtCodigo_KeyDown(object sender, KeyEventArgs e)
        {
            ultimoTextoEnTxtCodigo = txtCodigo.Text;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void TxtPruebaENTER_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                if (txtAbona.Focused) //si esta en abona finaliza la venta
                {
                    esModificacion();
                }
                else
                {
                    e.Handled = true;
                    SendKeys.Send("{TAB}");
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    if (fijarPeso)
                    {
                        txtCantKgs.Text = "1.500";
                    }
                    else
                    {
                        if (Convert.ToBoolean(ConfigurationManager.AppSettings["singleton"].ToString()))
                        {
                            Leer_Peso = Utilidades.SingletonLeerPeso.CrearLeerPeso();
                            txtCantKgs.Text = Leer_Peso.ObtenerPeso();
                        }
                        else
                        {
                            txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza(lblErrorBalanza.Visible);
                            lblErrorBalanza.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtCantKgs.Text = "Error balanza";
                timer1.Enabled = false;
                if (FormPrincipal.logueado && Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                {
                    checkLeerPeso.Checked = false;
                }
                else
                {
                    lblErrorBalanza.Text = ex.Message;
                    lblErrorBalanza.Visible = true;
                    timer1.Enabled = true;
                }
            }
        }

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (checkLeerPeso.Checked)
                {
                    txtCantKgs.BackColor = SystemColors.ScrollBar;
                    txtCantKgs.ReadOnly = true;
                    txtCantKgs.TabStop = false;
                    btnAgregar.Focus();
                    timer1.Enabled = true;
                }
                else
                {
                    txtCantKgs.BackColor = SystemColors.Window;
                    txtCantKgs.Text = "";
                    txtCantKgs.ReadOnly = false;
                    txtCantKgs.TabStop = true;
                    txtCantKgs.Focus();
                    timer1.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formVentaCaja_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            if (oUsuario != null)
            {
                validarAperturaCaja();
                oVentaE.Vendedor = oUsuario;
                usuario.Text = oUsuario.User;
                txtVendedor.Text = oUsuario.Nombre;
                lblVendedorNombre.Text = oUsuario.Nombre;
                this.Text = oUsuario.Nombre;
                Color colorUser = System.Drawing.Color.FromName(oUsuario.ColorForm);
                this.pnlBuscar.BackColor = colorUser;
                this.grupoCortes.BackColor = colorUser;
                grillaLineasVenta.DefaultCellStyle.SelectionBackColor = colorUser;
                timerBloquearCaja.Start();
                ultimaVentaVendedor();
            }
            else
            {
                this.Close();
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
                DialogResult resp = MessageBox.Show(oUsuario.Nombre + ":\nDebes Abrir Caja para poder registrar ventas.\n\n¿Desea abrir caja ahora?",
                    "Abrir Caja", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);//, MessageBoxButtons.YesNo, MessageBoxDefaultButton.Button2);
                if (resp.Equals(DialogResult.Yes))
                {
                    formAbrirCaja frmAbrirCaja = new formAbrirCaja();
                    frmAbrirCaja.oUserIncio = oUsuario;
                    frmAbrirCaja.ShowDialog();
                    validarAperturaCaja();
                }
                else
                {
                    this.Close();
                }                
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
                case Keys.End:
                    mostrarPago();
                    break;
                case Keys.PageDown:
                    cambiarPuntoDeVenta();
                    break;
                case Keys.F6:
                    misGasto();
                    break;
                case Keys.F7:
                    agregarGasto();
                    break;
                case Keys.F9:
                    buscarCliente();
                    break;
                case Keys.F2:
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(FormPrincipal))
                        {
                            frm.BringToFront();
                            break;
                        }
                    }
                    break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.F11:
                    txtObservaciones.Focus();
                    break;
                case Keys.F12:
                    bloquear();
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void misGasto()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE.Sucursal = oSucursalE;

            formGastosVendedor frmGastosVendedor = new formGastosVendedor();
            frmGastosVendedor.oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, null);
            frmGastosVendedor.ShowDialog();
        }

        private void agregarGasto()
        {
            if (panelBloquear.Visible) return;
            formAddOrEditGasto frmAddOrEditGasto = new formAddOrEditGasto();
            frmAddOrEditGasto.oUsuario = oUsuario;
            frmAddOrEditGasto.ShowDialog();
        }

        private void cambiarPuntoDeVenta()
        {
            bool cambioForm = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formVentaCaja))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (oUsuario != null && ctrl.Name.Equals("usuario") && !ctrl.Text.Equals(oUsuario.User))
                        {
                            Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso(null ,ctrl.Text.ToUpper());
                            barraProgreso.ShowDialog();
                            cambioForm = true;
                            frm.BringToFront();
                            break;
                        }
                    }
                }
                if (cambioForm) { break; }
            }
        }

        private void txtAbona_TextChanged(object sender, EventArgs e)
        {
            abonar();
        }

        private void abonar()
        {
            if (txtAbona.Text != "" && Utilidades.Util_Form.validarCampoNumerico(txtAbona.Text, "Abona") && 
                (txtAbona.Text.Contains("-") ? Utilidades.Util_Form.validarNumeroMayorACero(txtAbona.Text, "Abona") : true))
            {
                abona = float.Parse(txtAbona.Text.Replace('.', ','));
                cambio = abona - totalVenta;
            }
            else
            {
                txtAbona.Text = "";
                txtCambio.Text = "";
                abona = 0;
                cambio = 0;
            }
            txtCambio.Text = cambio.ToString("N2");
        }

        private void formVentaCaja_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private void btnBloquear_Click(object sender, EventArgs e)
        {
            bloquear();
        }

        private void bloquear()
        {
            panelBloquear.Visible = true;
            btnBloquear.Visible = false;
            btnAceptar.Enabled = false;
            btnAbonar.Enabled = false;
            grupoCortes.Enabled = false;
            pnlBuscar.Enabled = false;
            panelPago.Enabled = false;
            txtClave.Focus();
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
            desbloquear();
        }

        private void desbloquear()
        {
            if (txtClave.Text.Equals(oUsuario.Clave))
            {
                panelBloquear.Visible = false;
                btnBloquear.Visible = true;
                lblErrorClave.Visible = false;
                txtClave.Text = "";

                btnAbonar.Enabled = true;
                btnAceptar.Enabled = true;
                grupoCortes.Enabled = true;
                pnlBuscar.Enabled = true;
                panelPago.Enabled = true;

                timerBloquearCaja.Start();
                tiempoInactivo = 0;
                txtCodigo.Focus();
            }
            else
            {
                lblErrorClave.Visible = true;
            }
        }

        private void btnAceptar_Enter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromName("LimeGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromName("HotTrack");
        }

        private void btnAbonar_Click(object sender, EventArgs e)
        {
            mostrarPago();
        }

        private void mostrarPago()
        {
            if (txtAbona.Focused)
            {
                esModificacion();
            }
            else
            {
                panelAbonar.Visible = false;
                panelPago.Visible = true;
                txtAbona.ReadOnly = false;
                txtAbona.Focus();
            }
        }

        private void txtClave_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                desbloquear();
            }
        }

        private void txtCodigo_Enter(object sender, EventArgs e)
        {
            this.txtCodigo.BackColor = focusColor;
        }

        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            try
            {
                this.txtCodigo.BackColor = enableColor;
                if (oCorteE != null && oCorteE.idCorte > 0 && oCorteE.tipo.Equals("Unidad") && checkLeerPeso.Checked)
                {
                    checkLeerPeso.Checked = false;
                    txtCantKgs.Focus();
                }
                else
                {
                    if (oCorteE != null && oCorteE.idCorte > 0 && !oCorteE.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                    {
                        checkLeerPeso.Checked = FormPrincipal.logueado ? 
                            checkLeerPeso.Checked : true;
                        if (checkLeerPeso.Checked) btnAgregar.Focus();
                    }
                }

                if (cartelPrimerCorteVendedor && !this.txtCodigo.Text.Equals("") && grillaLineasVenta.Rows.Count.Equals(0))
                {
                    int cantCajaVenta = 0;
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(formVentaCaja))
                        {
                            cantCajaVenta++;
                            if (cantCajaVenta > 1)
                            {
                                titilarTextBoxVendedor();
                                //Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso("Caja de..." ,oUsuario.User.ToUpper());
                                //barraProgreso.ShowDialog();
                                break; 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al cargar el corte.\nMetodo: txtCodigo_Leave().\n\n" + ex.Message);
            }
        }

        private void txtCantKgs_Enter(object sender, EventArgs e)
        {
            this.txtCantKgs.BackColor = focusColor;
        }

        private void txtCantKgs_Leave(object sender, EventArgs e)
        {
            this.txtCantKgs.BackColor = enableColor;
        }

        private void btnAgregar_Enter(object sender, EventArgs e)
        {
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.BackColor = focusColor;
        }

        private void btnAgregar_Leave(object sender, EventArgs e)
        {
            this.btnAgregar.UseVisualStyleBackColor = true;
        }

        private void txtAbona_Enter(object sender, EventArgs e)
        {
            this.txtAbona.BackColor = focusColor;
        }

        private void txtAbona_Leave(object sender, EventArgs e)
        {
            this.txtAbona.BackColor = enableColor;
        }

        private void checkTicket_CheckedChanged(object sender, EventArgs e)
        {
            txtCodigo.Focus();
        }

        private void timerBloquearCaja_Tick(object sender, EventArgs e)
        {
            tiempoInactivo += timerBloquearCaja.Interval;
            if (tiempoInactivo >= tiempoBloqueo)
            {
                bloquear();
                timerBloquearCaja.Stop();
            }
        }

        private void timerTitilar_Tick(object sender, EventArgs e)
        {
            txtVendedor.BackColor = txtVendedor.BackColor.Equals(readOnlyColor) ? focusColor : readOnlyColor;
            lblVendedorNombre.BackColor = lblVendedorNombre.BackColor.Equals(readOnlyColor) ? focusColor : readOnlyColor;
            sumaTitilar += timerTitilar.Interval;
            if (sumaTitilar > titilarHasta)
            {
                lblVendedorNombre.Visible = false;
                txtVendedor.BackColor = readOnlyColor;
                timerTitilar.Stop();
            }
        }

        private void titilarTextBoxVendedor()
        {
            sumaTitilar = 0;
            lblVendedorNombre.Visible = true;
            timerTitilar.Start();
        }

        private void lblUltimaVenta_Click(object sender, EventArgs e)
        {
            if (ultimaVenta)
            {
                ultimaVentaVendedor();
                if (oUltimaVentaVendedor != null)
                {
                    formUltimaVenta frmUltimaVenta = new formUltimaVenta();
                    frmUltimaVenta.oUltimaVenta = oUltimaVentaVendedor;
                    frmUltimaVenta.ShowDialog();
                    ultimaVentaVendedor();
                }
                else
                {
                    MessageBox.Show("No se pudo cargar los datos de la última venta.", "Última venta no cargada");
                }
            }
        }

        private void ultimaVentaVendedor()
        {
            try
            {
                oUltimaVentaVendedor = oVentaN.getUltimaVentaVendedor(oUsuario.Id);
                double totalUltimaVenta = 0;
                foreach (Entidades.LineaVenta  linea in oUltimaVentaVendedor.LineasVenta)
                {
                    totalUltimaVenta += linea.PrecioKg * linea.CantKg;
                }
                lblHoraUltimaVenta.Text = oUltimaVentaVendedor.IdVenta.ToString() +
                    "\n " + oUltimaVentaVendedor.FechaVenta.ToShortDateString() +
                    "\n " + oUltimaVentaVendedor.FechaVenta.ToShortTimeString() +
                    "\n$ " + totalUltimaVenta.ToString("F2");
            }
            catch (Exception)
            {
                lblHoraUltimaVenta.Text = "No se\nobtuvieron\nlos datos";
            }
        }

        private void registrarTemporalLineaVenta()
        {
            try
            {
                if (oTemporalLineaVenta != null && oTemporalLineaVenta.CantKg > 0 && oTemporalLineaVenta.Corte != null &&                    
                    oTemporalLineaVenta.FechaInicioPesada.AddMilliseconds(tiempoRegistrarTemporal) <= DateTime.Now)
                {
                    oTemporalLineaVenta.VentaEnCurso = (grillaLineasVenta.Rows.Count > 0);
                    oVentaN.agregarTemporalLineaVenta(oTemporalLineaVenta);
                    oTemporalLineaVenta = null;
                }
            }
            catch (Exception)
            {
                oTemporalLineaVenta = null;
            }
        }
    }
}
