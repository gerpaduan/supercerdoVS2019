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
using System.Collections;
using System.Reflection;
using Utilidades;
using Presentacion.CuentaCorriente;
using static Presentacion.Caja.formCerrarCaja;
using System.Drawing.Text;
using System.Windows;
using System.Globalization;

namespace Presentacion.Caja
{
    public partial class formPOS : Form, InterfaceCorte, InterfacePersona, InterfaceUsuario, InterfaceFormaPago, InterfaceImprimirCbte
    {
        bool pesoBalanza = false;
        bool capturarPantallaFinal = false;
        Utilidades.SingletonLeerPeso Leer_Peso;
        Utilidades.Util_Form Util_Form = new Utilidades.Util_Form();
        wsAFIPvs2008.formFacturaElectronica formFactElec;
        #region variables
        public string vendedor = "-";
        public string precioBonificado = "";
        public bool bonificarTodos = false;
        public string porcentajeBonif_String = "";
        formVentas frmVentas;
        Negocio.Corte oCorteN = new Negocio.Corte();
        DataTable dtCortes = new DataTable();
        DataTable dtExpendios = new DataTable(); 
        DataTable tablaFiltrada;
        Negocio.Sucursal oSucursalN = new Negocio.Sucursal();
        Negocio.Venta oVentaN = new Negocio.Venta();
        Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
        Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
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

        bool dejarDeLeerPeso = false;
        bool ventanaDuplicada = false; //cuando una venta esta en curso y se necesita registrar otra para rapidez en la atencion
        int sucAnterior;
        int tiempoInactivo = 0;
        int tiempoBloqueo = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoBloqueo"].ToString());
        int sumaTitilar = 0;
        int titilarHasta = 2000;
        int tiempoRegistrarTemporal = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoRegistrarTemporal"].ToString());
        string ultimoTextoEnTxtCodigo = "";
        bool factura = Convert.ToBoolean(ConfigurationManager.AppSettings["factura"].ToString());
        Random randomClass = new Random();

        //Calculo decimales Random para redondear el importe del corte
        Random rndRedondeo = new Random();
        float centavosRedondeo = 0.93f;

        int nroErrorBalanza = 0;
        bool expendioDesdeCodigoBarra = false;
        /// <summary>
        /// Para Evitar el Leave cuando se carga un Codigo Barra EAN13 desde Expendios
        /// porque generaba un error 
        /// </summary>
        bool cargandoExpendios = false;
        public int SucAnterior
        {
            get { return sucAnterior; }
            set { sucAnterior = value; }
        }

        bool modificar = false;
        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());
        bool redondeo = Convert.ToBoolean(ConfigurationManager.AppSettings["redondeo"].ToString());
        int importeMaxRedondeo = Convert.ToInt32(ConfigurationManager.AppSettings["importeMaxRedondeo"].ToString());        
        bool cartelPrimerCorteVendedor = Convert.ToBoolean(ConfigurationManager.AppSettings["cartelPrimerCorteVendedor"].ToString());
        bool ultimaVenta = Convert.ToBoolean(ConfigurationManager.AppSettings["ultimaVenta"].ToString());
        string fecha = "", estadoVenta = "", detalleRedondeo;
        float totalCorte, precioKg, precioKgCorteExpendio, cantKg, cantKgTarjeta, kgsTotalCalculado;
        float totalVenta = 0, abona = 0, cambio = 0, ganPesosTotRedondeo = 0, ganKgsTotRedondeo = 0,
            ganPesosRedondeoLinea = 0, ganKgsRedondeoLinea = 0, acumRedondeoKgs = 0, acumRedondeImporte = 0;

        float porcAjEfectivo, porcAjDebito, porcAjCredito, porcAjCtaCte, porcAjQr, porcAjTranf, limiteKgParaAjuste;
        bool esAjustePorcTarj = false;
        int idConsumidorFinal;
        private bool isExpanded = false;
        private bool changeComboExpendio = false;
        private int idExpendioVenta;
        /// <summary>
        /// Variable Para manejar los codigos de barra internos
        /// </summary>

        bool esCodBarraInterno , esCodBarraEstandar = false;
        string codigoEnCodBarra = "", segundoModulo = "";
        public float pagoMixtoEfectivo = 0f;
        long codigoBuscado = 0;//si se llama al form buscar codigo se set codigoBuscado con el codigo del producto
        /// <summary>
        /// Cuenta la cantidad de veces que se activa auto el asterisco para desctivar balanza
        /// </summary>
        private bool asteriscoPressKey = false;
        bool valorAnteriorBalanza = false;
        #endregion


        public formPOS()
        {
            InitializeComponent();
            this.Icon = Properties.Resources.CarniSys_ICONO;

            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            this.KeyPreview = true;

            //Se obtienen los parametros
            Negocio.OtrasClases oOtrasClasesN = new Negocio.OtrasClases();
            oOtrasClasesN.obtenerParametros();

            //asigo sucursal a la venta  
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oVentaE.Sucursal = oSucursalE;
            //this.txtSucursal.Text = oVentaE.Sucursal.sucursal;
            Negocio.Persona oPersonaN = new Negocio.Persona();
            idConsumidorFinal = Entidades.Parametros.idConsumidorFinal;
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            txtFecVenta.Text = DateTime.Now.ToString();
            txtFecVenta.Text = DateTime.Now.ToString("ddd dd MMM yyyy · HH:mm", new CultureInfo("es-AR"));
            if (!fecha.Equals(""))
            {
                txtFecVenta.Text = DateTime.Parse(fecha).ToString();
                txtFecVenta.Text = DateTime.Now.ToString("dddd-dd MMMM, yyyy HH:mm", new System.Globalization.CultureInfo("es-ES"));
            }
            checkCtaCte_CheckedChanged(null,null);
            checkLeerPeso.Visible = (FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPesoCaja"].ToString()));
            //checkTicket.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["ticket"].ToString());

            //se cargar los porcentajes de ajuste por tarjeta
            porcAjEfectivo = Entidades.Parametros.porcAjEfectivo;
            porcAjDebito = Entidades.Parametros.porcAjDebito;
            porcAjCredito = Entidades.Parametros.porcAjCredito;
            porcAjCtaCte = 1;//no se obtiene el valor desde parametros
            porcAjQr = Entidades.Parametros.porcAjQr;
            porcAjTranf = Entidades.Parametros.porcAjTranf;
                
            limiteKgParaAjuste = Entidades.Parametros.limiteKgParaAjuste;
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
            //txtSucursal.Text = oVentaE.Sucursal.sucursal;
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

                    oVentaN.modificarVenta(oVentaE, SucAnterior, true);

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
                    MessageBox.Show("No se ha cargado ningún Producto en la venta. ", "No hay Productos cargados", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                            if (grillaLineasVenta.Rows[nroFila].Cells["Corte"].Value.ToString().Length > 22)
                            {
                                grillaLineasVenta.Rows[nroFila].Cells["Corte"].Style.Font = new Font(grillaLineasVenta.Font.ToString(), 13);
                            }
                            if (Convert.ToInt64(grillaLineasVenta.Rows[nroFila].Cells["Codigo"].Value) == linea.Corte.codigo &&
                                linea.IndexAnulado == nroFila && Entidades.LineaVenta.esAnulado(linea.Estado))
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

        private void agregarVenta()
        {
            if (validacionFinal())
            {
                cargarVenta();

                try
                {
                    //oVentaN.cargaExhaustiva(oVentaE, listaLineaVenta);
                    //return;


                    oVentaE.IdVenta = oVentaN.agregarVenta(oVentaE);
                    Ticket.CreaTicket ticket = new Ticket.CreaTicket();                
                                     
                    //imprimir si está checked
                    ticket.imprimir = checkTicket.Checked;
                    ticket.TextoCentro("x");
                    ticket.NoValidoComoFactura();
                    ticket.LineasEnBlanco(1);
                    if (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                        ticket.TextoCentro("A Cta. Cte.");
                    //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                    ticket.TextoIzquierda("A " + oVentaE.Persona.razonSocial);
                    string formaPagoImprimir = oVentaE.PagoMixtoEfectivo > 0 ? oVentaE.FormaPago.ToString() + "|Efvo" : oVentaE.FormaPago.ToString();
                    ticket.TextoIzquierda("Forma Pago: " + formaPagoImprimir);
                    ticket.TextoIzquierda("Nro. T. " + oVentaE.IdVenta.ToString());
                    ticket.TextoExtremos("Fecha: " + oVentaE.FechaVenta.Date.ToString(), "Hora: " + oVentaE.FechaVenta.TimeOfDay.ToString());
                    //ticket.LineasEnBlanco(0);
                    ticket.LineasGuion();
 
                    for (int index = 0; index < listaLineaVenta.Count; index++)
                    {
                        Entidades.LineaVenta linea = listaLineaVenta[index];
                        //setear por cada linea cantKg <- KgsTotalCalculado
                        linea.CantKg = linea.KgsTotalCalculado;

                        //si está anulada la linea se asigna el IdLineaVenta del corte anulado
                        linea.IndexAnulado = Entidades.LineaVenta.esAnulado(linea.Estado) ? listaLineaVenta[linea.IndexAnulado].IdLineaVenta : 
                            Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado);

                        listaLineaVenta[index] = oVentaN.agregarLineaVenta(linea);
                        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                            linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                    }
 
                    ticket.TextoDerecha("-------");
                    ticket.AgregaTotales("Total", totalVenta);
                    //si se ingresa la cantidad del pago se imprime
                    if (abona > 0)
                    {
                        ticket.AgregaTotales("Pago", abona);
                        ticket.AgregaTotales("Vuelto", cambio);
                    }
                    ticket.LineasEnBlanco(1);
                    ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);
                    ticket.TextoIzquierda("Cajero: " + oUsuario.Id);
                    ticket.GraciasPorSuCompra();
                    ticket.LineasEnBlanco(2);
                    ticket.realizarImpresion();


                    //se genera el egreso de caja si no es Efectivo
                    egresoCajaPagoTarjeta(oVentaE);

                    //Agregar en Cta Cte
                    try
                    {
                        oVentaN.crearMovCtaCteVenta(oVentaE);

                        ////se genera el egreso de caja por Cta. Cte
                        //if(oVentaE.EnCtaCte) 
                        //    egresoCajaPorCtaCte(oVentaE);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al crear el Movimiento en la Cuenta Corriente.\n\n**La Venta se registró correctamente**\n\n" + ex.Message);
                    }

                    try
                    {
                        if (oVentaE.ImprimirTipoCbte.Equals(Entidades.Venta.imprimirCbteEnum.Factura.ToString()))
                        {
                            facturaElectronica();                        
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al intentar generar Factura Electonica.\n\n**La Venta se registró correctamente**\n\n" + ex.Message);
                    }
                    oVentaE.IdVenta = 0; 
                    limpiarListas();
                    //si es ventada duplicada se cierra la misma
                    if (ventanaDuplicada)
                    {
                        this.Close();
                        return;
                    }
                    ultimaVentaVendedor();

                    //Si es Factura no se llama al formFormaPago para que el ShowDialog no dificulte la gestion del usuario
                    if (!oVentaE.ImprimirTipoCbte.Equals(Entidades.Venta.imprimirCbteEnum.Factura.ToString()))
                        ingresarFormaPago();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
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
                        MessageBox.Show("Hay una factura pendiende de registrar. Se abrirá otra ventana de facturacion");
                        frm.BringToFront();
                        break;
                    }
                    
                    if (oVentaE.IdVenta > 0)//Solo se pasa el obj Venta si es nuevo
                    {
                        formFactElec.idVenta = oVentaE.IdVenta;
                        formFactElec.cargarDatosAfip = false;
                        formFactElec.cargarVenta();
                    }
                    frm.BringToFront();
                    formFactuElec_Abierto = true;
                    formFactElec.logueado = FormPrincipal.logueado;
                    break;
                }
            }

            if (!formFactuElec_Abierto)
            {
                formFactElec = new wsAFIPvs2008.formFacturaElectronica(() =>
                {
                    // Esto se ejecuta al cerrarse FormC
                    this.WindowState = FormWindowState.Normal;
                    this.Show();
                    this.Activate(); // o BringToFront()
                });
                formFactElec.idVenta = oVentaE.IdVenta;
                formFactElec.logueado = FormPrincipal.logueado;
                formFactElec.Show();
            }
        }

        public  void egresoCajaPorCtaCte(Entidades.Venta oVentaConEgresoCaja)
        {
            try
            {
                Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

                oEgresoCajaE.Fecha = oVentaConEgresoCaja.FechaVenta;
                oEgresoCajaE.IdTipoEgresoCaja = 100;
                oEgresoCajaE.Descripcion = "Venta a " + oVentaConEgresoCaja.Persona.razonSocial + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                oEgresoCajaE.Monto = oVentaN.getTotalVenta(oVentaConEgresoCaja.IdVenta);
                oEgresoCajaE.Detalle = oVentaConEgresoCaja.Observaciones;
                oEgresoCajaE.Sucursal = oVentaConEgresoCaja.Sucursal;
                oEgresoCajaE.IdCompra = 0;
                oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Ventas.ToString();
                oEgresoCajaE.IdTabla = oVentaConEgresoCaja.IdVenta;
                oEgresoCajaE.CreadoPor = oVentaConEgresoCaja.Vendedor.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? (oUsuario != null ? oUsuario.Id : -1) : -1;

                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);

                ////Solo imprimo Egreso Caja si venta es en Efectivo
                //if (oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                //    imprimirTicket(oEgresoCajaE);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el Egreso.\n\nLa Venta y el movimiento en la Cta. Cte se registró correctamente." + "\n\n" + ex.Source);
            }
        }

        public void egresoCajaPagoTarjeta(Entidades.Venta oVentaConEgresoCaja)
        {
            try
            {
                oVentaN.egresoCajaPagoTarjeta(oVentaConEgresoCaja.IdVenta, oUsuario, oVentaConEgresoCaja.PagoMixtoEfectivo);

                #region Codigo anterior: SE PASO A CAPA NEGOCIO
                ///Codigo anterior: SE PASO A CAPA NEGOCIO
                ///
                //Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

                //oEgresoCajaE.Fecha = oVentaConEgresoCaja.FechaVenta;
                //oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idPagoTarjeta;
                //oEgresoCajaE.Descripcion = "Venta " + oVentaConEgresoCaja.FormaPago.ToString() + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                //oEgresoCajaE.Monto = float.Parse(txtTotalS.Text);// oVentaN.getTotalVenta(oVentaConEgresoCaja.IdVenta);
                //oEgresoCajaE.Detalle = " | Kgs: " + txtTotalKgs.Text + 
                //    " | Precio: " + (float.Parse(txtTotalS.Text) / float.Parse(txtTotalKgs.Text)).ToString("N3") + 
                //    " | TOT: " + txtTotalS.Text;
                //oEgresoCajaE.Sucursal = oVentaConEgresoCaja.Sucursal;
                //oEgresoCajaE.IdCompra = 0;
                //oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Ventas.ToString();
                //oEgresoCajaE.IdTabla = oVentaConEgresoCaja.IdVenta;
                //oEgresoCajaE.CreadoPor = oVentaConEgresoCaja.Vendedor.Id;
                //oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? (oUsuario != null ? oUsuario.Id : -1) : -1;

                //Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                //oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar el Egreso por pago de Tarjeta.\n\nLa Venta se registró correctamente." + "\n\n" + ex.Source);
            }
        }

        private void imprimirTicket(Entidades.EgresoCaja oEgresoCajaE)
        {
            try
            {
                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
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
                if (oEgresoCajaE.Actualizado != null) ticket.TextoIzquierda("Modif.: " + Utilidades.Util_Form.fechaFormato24Horas(oEgresoCajaE.Actualizado));
                ticket.LineasEnBlanco(5);
                ticket.realizarImpresion();
            }
            catch (Exception)
            {
                MessageBox.Show("Error al imprimir el Ticket");
                return;
            }
        }

        private void limpiarListas()
        {
            Negocio.Persona oPersonaN = new Negocio.Persona();
            int idConsumidorFinal = Entidades.Parametros.idConsumidorFinal;
            oCliente = oPersonaN.findById(idConsumidorFinal);
            EnviarPersona(oCliente);
            txtCuit.Text = "";
            txtDomicilio.Text = "";
            txtFecVenta.Text = DateTime.Now.ToString();
            txtNroRemito.Text = "";
            txtObservaciones.Text = "";
            txtCantItems.Text = "0";
            txtTotalKgs.Text = "0,000";
            txtTotalS.Text = "000,00";
            txtAbona.Text = "";
            txtCambio.Text = "";
            panelPago.Visible = false;
            panelAbonar.Visible = true;
            checkCtaCte.Visible = false;
            checkCtaCte.Checked = false;
            lblClienteConBonif.Visible = false;          
            restablecerFormaDePago();
            checkPagoMixto.Checked = false;
            pagoMixtoEfectivo = 0;
            comboTipoComprobante.SelectedIndex = 0; //Remito
            comboFormaPago.SelectedIndex = 0;

            totalVenta = 0;
            abona = 0;
            cambio = 0;

            //Variables de redondeo de importes
            ganKgsRedondeoLinea = 0; 
            ganKgsTotRedondeo = 0; 
            ganPesosRedondeoLinea = 0; 
            ganPesosTotRedondeo = 0;
            detalleRedondeo = "";
            
            listaLineaGrilla = new List<LineaVenta>();
            listaLineaVenta = new List<Entidades.LineaVenta>();
            grillaLineasVenta.DataSource = null;
            oVentaE.ListaExpendios.Clear();
            txtBuscarExpendio.Text = "";
        }

        private void cargarVenta()
        {
            string totalRedondeo = "\n-----------\n" + "Tot.Kgs: " + ganKgsTotRedondeo.ToString("F3") + 
                " | Tot.$: " + ganPesosTotRedondeo.ToString("F2") + "\n-----------\n\n";
            totalRedondeo += detalleRedondeo;

            bool reinicio = (acumRedondeImporte == 0 && acumRedondeoKgs == 0);
            acumRedondeImporte += ganPesosTotRedondeo;
            acumRedondeoKgs += ganKgsTotRedondeo;
            string acumRedondeoDetalle = reinicio ? "****Reinicio****" :
                "Kgs: " + acumRedondeoKgs.ToString("F3") + " | $: " + acumRedondeImporte.ToString("F2");

            oVentaE.Persona = oCliente;
            oVentaE.Sucursal = oSucursalE;
            oVentaE.TipoVenta = "Caja";
            oVentaE.FechaVenta = Convert.ToDateTime(txtFecVenta.Text).AddDays(0);
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
            oVentaE.Turno = "";
            oVentaE.DiaFestivo = "";
            oVentaE.Observaciones = txtObservaciones.Text.Trim();
            oVentaE.Estado = estadoVenta;
            oVentaE.EnCtaCte = checkCtaCte.Checked;
            //Si no es factura y FormaPago <> (Efectivo && CTACTE) y TipoCombrobante es 'X' entonces establecer tipoComprobante 'B'            
            oVentaE.TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString());
                //!factura ? Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString()) : (!(oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()) ||
                //oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString())) && 
                ////comboTipoComprobante.SelectedItem.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString())) ?
                //Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.B.ToString()) : Convert.ToChar(comboTipoComprobante.SelectedItem.ToString());
            oVentaE.PagoMixtoEfectivo = pagoMixtoEfectivo;
            oVentaE.Cuit = txtCuit.Text;
            oVentaE.Email = txtDomicilio.Text;
            oVentaE.TotalImporte = totalVenta;
            oVentaE.AcumRedondeoImporte = ganPesosTotRedondeo;
            oVentaE.AcumRedondeoKgs = ganKgsTotRedondeo;
            oVentaE.LineasVenta = listaLineaVenta;
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
            txtTotalKgs.Text = totalKgs.ToString("N3");
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
                    ////restablezco el redondeo
                    //rndRedondeo = new Random();
                    //centavosRedondeo = (rndRedondeo.Next(75, 99));
                    //centavosRedondeo = centavosRedondeo / 100;

                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
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

            //test redondeo
            txtRedondeo.Text = "";
            txtKgsRedondeo.Text = "";

            txtCodigo.Focus();
        }

        private void cargarListaGrilla(Entidades.LineaVenta lineaE)
        {
            LineaVenta lineaVentaP = new LineaVenta();

            lineaVentaP.idCorte = lineaE.Corte.idCorte;
            lineaVentaP.codigo = lineaE.Corte.codigo;
            lineaVentaP.corte = lineaE.Corte.corte;
            //Si hay redondeo se agrega un punto
            lineaVentaP.corte = lineaE.Bonificacion == 0 ?
                (lineaE.KgsAjusteTarj > 0 ? lineaE.Corte.CorteDesc + " ." : lineaE.Corte.CorteDesc) :
                    (lineaE.Corte.CorteDesc.Length < 9 ? lineaE.Corte.CorteDesc :
                    lineaE.Corte.CorteDesc.Substring(0, 9)) + 
                    " (Bonif. " + lineaE.Bonificacion.ToString("F2") + "%)";
            lineaVentaP.cantKgs = lineaE.CantKg;
            lineaVentaP.kgsTotalCalculado = lineaE.KgsTotalCalculado;
            lineaVentaP.kgsAjusteTarj = lineaE.KgsAjusteTarj;
            lineaVentaP.precioKg = lineaE.PrecioKg;
            lineaVentaP.totalS = lineaE.PrecioKg * lineaE.KgsTotalCalculado;
            lineaVentaP.Random = lineaE.Random;
            lineaVentaP.IdExpendio = lineaE.IdExpendio;
            if (lineaE.Estado == 1)
            {
                lineaVentaP.estado = "Anulado";
                lineaVentaP.corte += " (Anulado)";
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
            oLineaVenta.KgsTotalCalculado = kgsTotalCalculado;
            oLineaVenta.KgsAjusteTarj = oLineaVenta.KgsTotalCalculado - oLineaVenta.CantKg;
            precioKg = idExpendioVenta > 0 ? precioKgCorteExpendio : precioKg;
            oLineaVenta.PrecioKg = precioKg;
            oLineaVenta.PesoBalanza = pesoBalanza;
            oLineaVenta.Bonificacion = (1 - (precioKg / oCorteE.precioKg)) * 100;
            oLineaVenta.PrecioReal = oCorteE.precioKg;
            oLineaVenta.Random = randomClass.Next(0, 1000000);

            if (oLineaVenta.CantKg < 0)
            {
                oLineaVenta.Estado = 1;//Anulado
            }
            else
            {
                oLineaVenta.Estado = 0;//Activo
            }

            //Cargo valores de Redondeo de Caja y detalle en Observaciones de Venta
            detalleRedondeo += "\n"+"** Linea: "+listaLineaGrilla.Count.ToString()+" | "+ganKgsRedondeoLinea.ToString("F3")+" | "+
                ganPesosRedondeoLinea.ToString("F2")+" **";
            ganKgsTotRedondeo += ganKgsRedondeoLinea;
            ganPesosTotRedondeo += ganPesosRedondeoLinea;
            ganPesosRedondeoLinea = ganKgsRedondeoLinea = 0;//Seteo a CERO las variables

            oLineaVenta.IdExpendio = idExpendioVenta;
        }

        private bool ingresarFormaPago()
        {
            bool resp = true;
            if (string.IsNullOrEmpty(oVentaE.FormaPago))
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked =
                    checkCtaCtePago.Checked = checkQr.Checked = checkTransf.Checked = checkCtaCte.Checked = false;//Asegura q se inicien todos false
                                
                formFormaPago frmFormaPago = new formFormaPago();
                frmFormaPago.ShowDialog(this);

                //si ninguna forma de pago está seleccionada no se valida
                if (checkEfectivo.Checked == false && checkDebito.Checked == false && checkCredito.Checked == false
                    && checkCtaCtePago.Checked == false && checkQr.Checked == false && checkTransf.Checked == false)
                    return false;
                txtCodigo.Focus();
            }
            if (oCorteE != null)
                cargarCorte();
            return resp;
        }

        private bool validarLinea()
        {
            //Solicita que ingrese Forma de Pago
            if (!ingresarFormaPago())
                return false;

            //Se valida que no sea media res
            if (oCorteE != null && !oCorteE.Habilitado)
            {
                MessageBox.Show("- \'" + oCorteE.CorteDesc +"\' no está habilitado para la venta", "Producto No Habilitado",MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtCodigo.Focus();
                return false;
            }


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
                        MessageBox.Show("El código ingresado no pertenece a ningún Producto.", "El Producto no existe", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        txtCodigo.Focus();
                        txtCodigo.SelectAll();
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
                bool esKgsMayorACero = Utilidades.Util_Form.validarNumeroMayorACero(txtCantKgs.Text, "Kgs.");
                //Si es Corte desde Expendio se permite precio cero
                bool esPrecioMayorACero = idExpendioVenta > 0 || Utilidades.Util_Form.validarNumeroMayorACero(txtPrecioKg.Text, "Precio");
                if (!esKgsMayorACero && !checkLeerPeso.Checked)
	            {
                    txtCantKgs.Focus();
                    txtCantKgs.SelectAll();
	            }

                //validar que cantidad kilos sea menor a mil
                float numeroKgs;
                bool esKgsMenorAMil = true;
                if (float.TryParse(txtCantKgs.Text.Replace('.',','), out numeroKgs) && (numeroKgs > 1000))
                {
                    esKgsMenorAMil = false;
                    MessageBox.Show("La cantidad de kgs ingresada debe ser menor a 1.000 (mil).", "Completar campos", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtCantKgs.Text = "";
                    txtCantKgs.Focus();
                    txtCantKgs.SelectAll();
                }

                return esKgsMayorACero && esPrecioMayorACero && esKgsMenorAMil;
            }
        }

        private bool validacionFinal()
        {
            //Solicita que ingrese Forma de Pago
            if (!ingresarFormaPago())
                return false;

            //se valida pago mixto, si deshabilita check significa que no cumple con las restricciones
            if (checkPagoMixto.Checked)
            {
                validarPagoMixto();
                if (!checkPagoMixto.Checked)
                    return false;

                ///si está tildado Pago Mixto
                ///mostrar form y calcular los diferentes montos y los egresos segun la forma de pago
                ///
                formPagoMixto formPagoMixto = new formPagoMixto();
                formPagoMixto.totalPesos = totalVenta;
                formPagoMixto.formaPago = oVentaE.FormaPago;
                formPagoMixto.formPOS = this;
                formPagoMixto.ShowDialog();
                //si le dio al boton ingresar en form pago mixto continuar sino return false
                if (!(pagoMixtoEfectivo > 0))
                    return false;
            }
            

            //valida que un venta en CTA CTE sea solo en Cta Cte
            if (checkCtaCte.Checked && (!oVentaE.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.CtaCte.ToString()) ||
                oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal)))
            {
                MessageBox.Show("Las ventas en Cuenta Corriente (CTA.CTE.) no pueden ser a Consumidor Final" +
                    "\n\nPor favor, revisa los datos ingresados y vuelva a intentarlo.",
                    "Verifique la forma de pago", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

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
                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                bool cajaAbierta = oCierreN.validarCajaAbiertaVendedor(Convert.ToDateTime(txtFecVenta.Text), oVentaE.Sucursal, oUsuario);
                if (totalVenta > 0 && !cajaAbierta)
                {
                    MessageBox.Show(oUsuario.Nombre + " la caja ha sido cerrada.\n\n"+
                    "Pasos:\n1- Anule todos los ítems y finalice la Venta.\n2- Abra caja y vuelva a registrar la venta.", "No abrió caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtFecVenta.Text = DateTime.Now.ToString();//se actualiza la hora

                    return false;
                }

                string mensaje = "Complete los siguientes campos: ";
                
                //se valida que no finalice venta con bonificacion para consumidor final
                for (int index = 0; index < listaLineaVenta.Count; index++)
                {
                    if (listaLineaVenta[index].Bonificacion != 0 && !FormPrincipal.logueado && !oUsuario.Admin && 
                        oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal))
                    {
                        bool esAnulado = false;
                        //se valida que el corte no hay sido anulado
                        for (int nroFila = 0; nroFila < listaLineaVenta.Count; nroFila++)
                        {
                            if (listaLineaVenta[index].IndexAnulado >= 0 || (listaLineaVenta[index].Corte.codigo == listaLineaVenta[nroFila].Corte.codigo &&
                                listaLineaVenta[nroFila].IndexAnulado == index))
                            {                                
                                esAnulado = true;
                                break;
                            }
                        }

                        if (!esAnulado)
                        {
                            mensaje = "No tienes permiso para hacer bonificaciones a un cliente Consumidor Final";
                            MessageBox.Show(mensaje, "No se puede bonificar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }  
                    }                  
                }

                //valido que no haya ningún Producto seleccionado al finalizar venta
                if (txtCodigo.Text.Length > 0)
                {
                    mensaje = "No se puede finalizar la venta si existe un producto seleccionado.\n" +
                        "Borre el código e inténtelo nuevamente";
                    MessageBox.Show(mensaje, "Existe un producto seleccionado", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        if ((abona > 0 && cambio < 0))
                        {
                            if (cambio < 0)
                            {
                                mensaje = "El pago del cliente es menor al total de la venta";
                                MessageBox.Show(mensaje, "Error en el pago", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                txtAbona.Select();
                                txtAbona.Focus();
                            }
                            return false;
                        }
                        else
                        {
                            formFinalizarVenta formFinVenta = new formFinalizarVenta();
                            formFinVenta.oVentaE = oVentaE;
                            formFinVenta.ShowDialog(this);

                            if (oVentaE.ImprimirTipoCbte != null && !oVentaE.ImprimirTipoCbte.Equals(Entidades.Venta.imprimirCbteEnum.Nulo.ToString()))
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

        ///Se obtiene el tipo de comprobante a imprimir.
        public void EnviarImprimirCbte(Entidades.Venta.imprimirCbteEnum imprimirTipoCbte)
        {
            try
            {
                oVentaE.ImprimirTipoCbte = imprimirTipoCbte.ToString();
                switch (imprimirTipoCbte)
                {
                    case Entidades.Venta.imprimirCbteEnum.SinTicket:
                        checkTicket.Checked = false;
                        break;
                    case Entidades.Venta.imprimirCbteEnum.Ticket:
                        checkTicket.Checked = true;
                        break;
                    case Entidades.Venta.imprimirCbteEnum.Factura:
                        checkTicket.Checked = false;
                        break;
                }
            }
            catch (Exception)
            {
                oVentaE.ImprimirTipoCbte = Entidades.Venta.imprimirCbteEnum.Nulo.ToString();
            }
        }

        private void quitarLinea()
        {
            if (!grillaLineasVenta.ContainsFocus)
                return;

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
                    string datosLinea = "\n\n Datos del Producto \n-----------------------------------------\n " +
                        oLineaVentaSelect.Corte.corte +
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + 
                        "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.PrecioKg;
                    string mensaje = "¿Está seguro de anular el Producto seleccionado?" + datosLinea;
                    DialogResult respuesta = MessageBox.Show(mensaje, "Anular Corte", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (respuesta == System.Windows.Forms.DialogResult.Yes)
                    {
                        oLineaVenta = new Entidades.LineaVenta();
                        oLineaVenta.Corte = oLineaVentaSelect.Corte;
                        oLineaVenta.Venta = oLineaVentaSelect.Venta;
                        oLineaVenta.CantKg = oLineaVentaSelect.CantKg * -1;
                        oLineaVenta.KgsTotalCalculado = oLineaVentaSelect.KgsTotalCalculado * -1;
                        oLineaVenta.KgsAjusteTarj = oLineaVentaSelect.KgsAjusteTarj * -1;
                        oLineaVenta.PrecioKg = oLineaVentaSelect.PrecioKg;
                        oLineaVenta.Estado = 1;//anulado
                        oLineaVenta.Bonificacion = oLineaVentaSelect.Bonificacion;
                        oLineaVenta.IndexAnulado = nroFila;
                        oLineaVenta.IdExpendio = oLineaVentaSelect.IdExpendio;

                        //se agrega el index del anulado al corte seleccionado para anular
                        //--el index equivale a la cantidad en listaLineaVenta antes de cargarLista--
                        listaLineaVenta[nroFila].IndexAnulado = listaLineaVenta.Count;

                        cargarListas();
                        cargarGrilla();

                        txtCodigo.Focus();
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

        private void cargarCorte()
        {
            if (txtCodigo.Text != "")
            {
                try
                {
                    lblNoHabilitado.Visible = false;
                    oStockCorteSucursal = null;
                    oStockCorteSucursal = new Entidades.StockCorteSucursal();

                    oCorteE = null;
                    oCorteE = new Entidades.Corte();

                    //dtCortes = oCorteN.buscarCodigoCorte(Convert.ToInt64(txtCodigo.Text.Trim()));

                    DataRow[] filas = dtCortes.Select("codigo = " + Convert.ToInt64(txtCodigo.Text.Trim()));
                    //if (dtCortes.Rows.Count > 0)
                    if (filas.Length > 0)
                    {
                        //foreach (DataRow fila in dtCortes.Rows)
                        foreach (DataRow fila in filas)
                        {
                                oCorteE.idCorte = Convert.ToInt32(fila["idCorte"].ToString());
                                oCorteE.codigo = Convert.ToInt64(fila["codigo"].ToString());
                                oCorteE.corte = fila["corte"].ToString();
                                oCorteE.IdAlicuotaIva = Convert.ToInt32(fila["idAlicuotaIva"].ToString());
                                oCorteE.AlicuotaIva = float.Parse(fila["alicuotaIva"].ToString());
                                oCorteE.precioKg = float.Parse(fila["precioKg"].ToString());
                                oCorteE.precioKgReferencia = float.Parse(fila["precioKg"].ToString());
                                oCorteE.tipo = fila["tipo"].ToString();
                                oCorteE.IngresoRapidoEmbutido = Convert.ToBoolean(fila["ingresoRapidoEmbutido"]);
                                oCorteE.EnCierreStock = Convert.ToBoolean(fila["enCierreStock"]);
                                oCorteE.Habilitado = Convert.ToBoolean(fila["habilitado"]);
                                oCorteE.Pesable = Convert.ToBoolean(fila["pesable"]);
                        }
                        //cargo los campos                        
                        //this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                        this.txtCorte.Text = oCorteE.corte;

                        //si no está habilitado no muestra el importe
                        if (!oCorteE.Habilitado)
                        {
                            lblNoHabilitado.Visible = true;
                            return;
                        }

                        //Se establece el precio segun la forma de pago
                        establecerPrecioCorteSegunFormaPago();

                        this.txtPrecioKg.Text = oVentaE.bonificar(oCliente, oCorteE.precioKg, false).ToString("N2");//oCorteE.precioKg.ToString("N");
                        
                        cargarTotalCorte();
                    }
                    else
                    {
                        oCorteE = null;
                        this.txtTotalCorte.Text = "";
                        this.txtPrecioKg.Text = "";
                        this.txtCorte.Text = "";
                        totalesParciales(0, 0);
                        //test redondeo
                        txtRedondeo.Text = "";
                        txtKgsRedondeo.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Valor demasiado grande o demasiado pequeño para Int32."))
                        return;
                    
                    MessageBox.Show("Error al cargar Producto\n\n" + ex.Message);
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
                totalesParciales(0, 0);
                //test redondeo
                txtRedondeo.Text = "";
                txtKgsRedondeo.Text = "";
            }
        }

        /// <summary>
        /// Se establece el precio del corte según  la forma de pago seleccionada
        /// </summary>
        private void establecerPrecioCorteSegunFormaPago()
        {
            switch (oVentaE.FormaPago)
            {
                case "Efectivo":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjEfectivo);
                    break;
                case "Debito":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjDebito);
                    break;
                case "Credito":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjCredito);
                    break;
                case "CtaCte":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjCtaCte);
                    break;
                case "Qr":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjQr);
                    break;
                case "Tranf":
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjTranf);
                    break;
                default:
                    oCorteE.precioKg = (oCorteE.precioKgReferencia * porcAjCtaCte);
                    break;
            }
            oCorteE.precioKg = Util_Form.convertFloat(Math.Round(oCorteE.precioKg, 2).ToString(), false);
        }

        private void cargarTotalCorte()
        {     
            //calcular total corte para peso inestable
            if (txtCantKgs.Text.Contains("i"))
            {
                try 
	            {
                    ///lectura de peso inestable "001.300 i" (1.300 kgs en balanza)
                    float kgPesoInestable = Utilidades.Util_Form.convertFloat(txtCantKgs.Text.Substring(0, 7), false);
                    float precioKgCorte = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);

                    float totalCorteInestable = (kgPesoInestable * precioKgCorte);// redondear ? redondearMultipo10(kgPesoInestable * precioKgCorte) : (kgPesoInestable * precioKgCorte);
                    //cargo el txt total corte
                    txtTotalCorte.Text = totalCorteInestable.ToString("F2");

                    totalesParciales(kgPesoInestable, totalCorteInestable);

	            }
	            catch (Exception)
	            {
            		txtTotalCorte.Text = "";
	            }
                return;
            }

            if (!txtCantKgs.Text.Equals("") && (checkLeerPeso.Checked  || 
                ( !checkLeerPeso.Checked && Utilidades.Util_Form.validarCampoNumerico(txtCantKgs.Text, "Kgs"))))
            {
                try
                {
                    //Lectura de peso estable en balanza "001.305"
                    try
                    {
                        cantKg = Utilidades.Util_Form.convertFloat(txtCantKgs.Text, false);
                    }
                    catch (Exception)
                    {

                        cantKg = float.Parse(txtCantKgs.Text.Trim());
                    }

                    if (oCorteE != null)
                    {
                        try
                        {
                            precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);
                        }
                        catch (Exception)
                        {

                            if (checkLeerPeso.Checked)
                            {
                                precioKg = 0;
                            }
                        }                       

                        //cargo el Temporal de LineaVenta
                        try
                        {
                            kgsTotalCalculado = cantKg;

                            string[] dosPartesKgsTarj = kgsTotalCalculado.ToString().Split(',');
                            string[] dosPartesKgsBalanza = cantKg.ToString().Split(',');
                            bool esKgsRedondo = dosPartesKgsBalanza.Count().Equals(1) || dosPartesKgsBalanza[1].Equals("000")
                                || dosPartesKgsBalanza[1].Equals("00") || dosPartesKgsBalanza[1].Equals("0");

                            /////////Si cambia parte entera Kilaje al ajustar, establecer decimales en ###.995
                            //////if ((!(dosPartesKgsTarj[0] == dosPartesKgsBalanza[0])))
                            //////    kgsTotalCalculado = Util_Form.convertFloat(dosPartesKgsBalanza[0] + ".995", false);
                            
                            /////////NO ajustar kgs por Tarjeta cuando:
                            /////////*CheckBoxRedondeo R no está checked
                            /////////*cantKg de balanza es mayor al limite estipulado
                            /////////*ó Cliente contiene "Empleado" en su nombre
                            /////////*ó Kg Real Balanza es un entero
                            //////if (!checkBoxRedondeo.Checked || cantKg > limiteKgParaAjuste || oCliente.razonSocial.Contains("mpleado") || esKgsRedondo)
                            //////    //se setear el valor real balanza
                            //////    kgsTotalCalculado = cantKg;
                                                                                    
                            //Setear el temporal de la linea venta
                            oTemporalLineaVenta = new Entidades.TemporalLineaVenta();
                            oTemporalLineaVenta.FechaInicioPesada = DateTime.Now;
                            oTemporalLineaVenta.Corte = oCorteE;
                            oTemporalLineaVenta.Vendedor = oUsuario;
                            oTemporalLineaVenta.Sucursal = oSucursalE;
                            oTemporalLineaVenta.CantKg = cantKg;
                            oTemporalLineaVenta.KgsTotalCalculado = kgsTotalCalculado;
                            oTemporalLineaVenta.TotalCorte = (cantKg * precioKg);

                            //Seteo a CERO las variables
                            ganPesosRedondeoLinea = ganKgsRedondeoLinea = 0;

                            //"001.305"            

                            //Si la centena del decimal cambia de valor, Variar peso hasta ###.#99
                            //camparar las centenar de Decimales y/o Unidad de peso para verificar que no hay un cambio brusco.

                            ///REDONDAR importe SI:
                            ///*Codigo es menor a 100
                            ///*Cliente es consumidor final
                            ///*la cantidad de ganancia NO excede a $5
                            ///*el Kg no es un número redondo
                            ///
                            bool redondear = oCorteE.codigo < 100 && oCliente.idPersona.Equals(idConsumidorFinal) && ganPesosTotRedondeo < importeMaxRedondeo && !esKgsRedondo ? true : false;

                            if (redondear)
                            {
                                //float importeRedondeo = redondearMultipo10(cantKg * precioKg);
                                float importeRedondeo = redondearMultipo10(kgsTotalCalculado * precioKg);
                                float kgsRedondeo = (importeRedondeo / precioKg);

                                //Si al redondear los Kgs cambia la parte entera del Kilaje, NO se redondea
                                string[] parteEnteraRedondeo = kgsRedondeo.ToString().Split(',');
                                string[] parteEnteraCantKgs = kgsTotalCalculado.ToString().Split(',');
                                if (parteEnteraRedondeo[0] == parteEnteraCantKgs[0])
                                {
                                    //Guardo las diferencia en el redondeo
                                    ganKgsRedondeoLinea = kgsRedondeo - kgsTotalCalculado;//Guarda Dif KGS
                                    ganPesosRedondeoLinea = importeRedondeo - (kgsTotalCalculado * precioKg);//Guarda Dif Dinero

                                    kgsTotalCalculado = kgsRedondeo;
                                    oTemporalLineaVenta.KgsTotalCalculado = kgsTotalCalculado;
                                    oTemporalLineaVenta.TotalCorte = (kgsTotalCalculado * precioKg);
                                }
                            }                  
                        }
                        catch (Exception)
                        {
                        }
                    }
                    totalCorte = (kgsTotalCalculado * precioKg); //(cantKg * precioKg);//
                    //cargo el txt total corte
                    txtRedondeo.Text = (cantKg * precioKg).ToString("N");
                    txtTotalCorte.Text = totalCorte.ToString("N2");

                    //test redondeo
                    //totalCorteRed = (cantKgTarjeta * precioKg);
                    txtRedondeo.Text = (cantKg * precioKg).ToString("N2");
                    txtKgsRedondeo.Text = kgsTotalCalculado.ToString("N3");

                    totalesParciales(kgsTotalCalculado, totalCorte);
                    //totalesParciales(cantKg, totalCorte);

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

        /// <summary>
        /// Metodo para Redondear a multiplo de 10 el Monto de la Venta. Para aplicar cuando faltan billetes de $5
        /// </summary>
        /// 
        private float redondearMultipo10(float importe)
        {
            if (checkBoxRedondeo.Checked)
            {
                int precioSinDecimal = Convert.ToInt32(Math.Truncate(importe));
                float centavos = importe - precioSinDecimal;
                int cantDigitos = precioSinDecimal.ToString().Length;
                /////obtengo el numero la unidad del importe (CDU,dd)
                /////C=Centena / D=Decena / U=Unidad / dd=decimales
                //int unidadPrecio = Convert.ToInt32(char.GetNumericValue(precioSinDecimal.ToString(),
                //    precioSinDecimal.ToString().Length - 1));

                ////si la unidad del importe es mayor o igual a 5 pesos
                //if (unidadPrecio >= 5 && unidadPrecio <= 9)
                //{
                //    //Calculo unos decimales Random para variar el importe
                //    //Random rndRedondeo = new Random();
                //    //float centavosRedondeo = (rndRedondeo.Next(2, 50)) ;
                //    //importe = (precioSinDecimal + (10 - unidadPrecio)) - centavosRedondeo;
                //    importe = (precioSinDecimal + (9 - unidadPrecio)) + centavos;
                //}             

                ///obtengo el numero la decena del importe (CDU,dd)
                ///C=Centena / D=Decena / U=Unidad / dd=decimales
                int decenaPrecio; //= Convert.ToInt32(char.GetNumericValue(precioSinDecimal.ToString(),

                //si el numero es mayor a mil se aplicar redondeo
                if (cantDigitos > 3)
                {
                     decenaPrecio = Convert.ToInt32(precioSinDecimal.ToString().Substring(cantDigitos - 2));

                    //si la decena está entre 50 y 90 pesos
                    if (decenaPrecio >= 50 && decenaPrecio <= 90)
                        importe = (precioSinDecimal + (90 - decenaPrecio)) + centavos;
                }
            }

            return importe;
        }


        private void totalesParciales(float kgsCorte, float totalCorte)
        {
            cargarTotales();

            if (totalCorte > 0)
            {
                try
                {
                    txtTotalKgs.Text = (Utilidades.Util_Form.convertFloat(txtTotalKgs.Text, false) + kgsCorte).ToString("N3");
                    txtTotalS.Text = (Utilidades.Util_Form.convertFloat(txtTotalS.Text, false) + totalCorte).ToString("N2");
                }
                catch (Exception)
                {
                    cargarTotales();
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
                        totalCorte = Utilidades.Util_Form.convertFloat(txtTotalCorte.Text, false);
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
                        precioKg = Utilidades.Util_Form.convertFloat(txtPrecioKg.Text, false);
                    }
                    catch (Exception)
                    {
                        precioKg = float.Parse(txtPrecioKg.Text.Trim());
                    }
                    //totalCorte = precioKg * cantKg;
                    totalCorte = precioKg * kgsTotalCalculado;

                    if (Presentacion.FormPrincipal.logueado || idExpendioVenta > 0)
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
                //si es ventana duplicada se cierra automaticamente
                if (ventanaDuplicada)
                    return false;

                if (oUsuario == null) return false;

                DialogResult respuesta;
                respuesta = MessageBox.Show("¿Cerrar la ventana de Venta de "+oUsuario.Nombre+"?.", "Cerrar ventana", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

                if (respuesta == DialogResult.Yes)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        private void btnBuscaCorte_Click(object sender, EventArgs e)
        {
            buscarCorte();
        }

        private void buscarCorte()
        {
            dtCortes = oCorteN.cargarDtCortes();
            formBuscarCorte frmBuscarCorte = new formBuscarCorte();
            frmBuscarCorte.Show(this);
        }

        public void EnviarCorte(Entidades.Corte corte)
        {
            oCorteE = null;

            oCorteE = corte;

            codigoBuscado = oCorteE.codigo;
            this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
            this.txtCorte.Text = oCorteE.corte;
            this.txtCodigo.Focus();

        }

        private void btnAceptar_Click(object sender, EventArgs e)
        {
            tiempoInactivo = 0;
            esModificacion();
            if (capturarPantallaFinal) capturarPantalla();
            capturarPantallaFinal = false;
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

            linkVerCtaCte.Visible = oUsuario.Admin;// !oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal);
            linkUltimasVentasCliente.Text = oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal) ?
                "mis ventas" : "Ver ultimas 5 ventas";

            ////Ocultar Ultimas Ventas Para Cocinas y Furlana
            if ((FormPrincipal.soyYo && !oUsuario.Admin) &&
                (oCliente.razonSocial.ToLower().Contains("furlana") || oCliente.razonSocial.ToLower().Contains("cocina")))
            {
                linkUltimasVentasCliente.Visible = false;
                linkVerCtaCte.Visible = false;
            }

            this.txtCliente.Text = oCliente.razonSocial;
            this.txtCuit.Text = oCliente.Cuit;
            this.txtDomicilio.Text = oCliente.Domicilio + " - " + oCliente.Ciudad;
            //Si es RRII (IdIva = 2) se selecciona Comprobante A
            //comboTipoComprobante.SelectedItem = oCliente.IdIva == 2 ? Entidades.Venta.tipoComprobanteEnum.A.ToString() : Entidades.Venta.tipoComprobanteEnum.X.ToString();
            lblClienteConBonif.Visible = oCliente.Bonificacion.Equals(0) ? false : true;
            lblClienteConBonif.Text = lblClienteConBonif.Visible ?
                "Cliente con Bonificación (" + oCliente.Bonificacion.ToString("N2") + " %)" : "";

            restablecerFormaDePago();
            //unchecked todos las formas de pago para que las vuelva a ingresar y evitar algun error por descuido
            //con clientes en cta cte.
            checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked =  false;
        }

        public void EnviarFormaPago(Entidades.Venta.formaPagoEnum formaPago)
        {
            switch (formaPago)
            {
                case Entidades.Venta.formaPagoEnum.Efectivo:
                    checkEfectivo.Checked = true;
                    comboFormaPago.SelectedIndex = 1;
                    break;
                case Entidades.Venta.formaPagoEnum.Debito:
                    checkDebito.Checked = true;
                    comboFormaPago.SelectedIndex = 2;
                    break;
                case Entidades.Venta.formaPagoEnum.Credito:
                    checkCredito.Checked = true;
                    comboFormaPago.SelectedIndex = 3;
                    break;
                case Entidades.Venta.formaPagoEnum.CtaCte:
                    checkCtaCtePago.Checked = true;
                    comboFormaPago.SelectedIndex = 6;
                    break;
                case Entidades.Venta.formaPagoEnum.Qr:
                    checkQr.Checked = true;
                    comboFormaPago.SelectedIndex = 4;
                    break;
                case Entidades.Venta.formaPagoEnum.Transferencia:
                    checkTransf.Checked = true;
                    comboFormaPago.SelectedIndex = 5;
                    break;
            }                    
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            ///Si las dos primeras letras son igual a "PE" significa q es un numero de expendio
            ///tomar el numero y cargar el expendio
            ///
            int longCodigo = txtCodigo.Text.Length;
            string text = txtCodigo.Text;
            bool validarNroExpendio = (longCodigo == 1 && !char.IsDigit(text[0]) && text[0] == 'P') || (longCodigo == 2 && !char.IsDigit(text[1]) && text == "PE") ||
                (longCodigo >= 3 && char.IsDigit(text[2]) && text[0] == 'P' && text[1] == 'E');
            if (validarNroExpendio)
            {
                ///buscarExpendio
                ///el Nro Expendio estará entre PE y F que indica el fin del numero
                ///
                if (text[longCodigo-1] == 'F')
                {
                    expendioDesdeCodigoBarra = true;
                    expendirExpendios();
                    txtBuscarExpendio.Text = text.Replace("PE", "").Replace("F", "");
                    txtBuscarExpendio_KeyPress(txtBuscarExpendio, new KeyPressEventArgs((char)Keys.Enter));
                    //expendirExpendios();
                    //txtBuscarExpendio.Text = "";

                }
                return;
            }


            if (grillaLineasVenta.Rows.Count.Equals(0))
            {
                //al iniciar la venta se actualizan los cortes cargándose el dtCortes
                if (txtCodigo.Text.Length == 1)
                    dtCortes = oCorteN.cargarDtCortes();
            }
            cargarCorte();

            int primerosDos = txtCodigo.Text.Length == 8 ?
                int.Parse(txtCodigo.Text.Substring(0, 2)) : 0;
            bool esEAN8 = primerosDos < 20 && primerosDos > 29;

            if ((txtCodigo.Text.Length == 8 && esEAN8 && esDigitoControlCorrectoEAN8(false)) ||
               txtCodigo.Text.Length == 13 && esDigitoControlCorrectoEAN13(false))
            {
                //si se buscó el codigo en el formCortes y es EAN se forza a ingresar la cantidad
                //evitando así la carga por error
                //if (codigoBuscado != 0)
                //    return;

                if (cargandoExpendios)
                    return;
                // Desplazar el foco a otro control para disparar el evento Leave.
                this.ActiveControl = null;  // Esto simula que el usuario sale del TextBox.
                txtCodigo.Focus();
                txtCodigo.SelectAll();
            }
            ///Codigo Viejo //si se borra el corte actual se llama a metodo registrarTemporalLinea
            ///if (oCorteE != null && !ultimoTextoEnTxtCodigo.Equals(txtCodigo.Text))

        }

        private void codigoDeBarraMetodo()
        {
            ///si codigo es EAN-8
            /// 
            if (txtCodigo.Text.Length == 8)
            {
                if (!esDigitoControlCorrectoEAN8(true))
                {
                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
                    return;
                }
                esCodBarraEstandar = true;
            }

            ///Si codigo es longitud 13 y comienza con 20 o 21 entonces es de barra codigo interno
            ///                        
            if (txtCodigo.Text.Length == 13)
            {
                if (!esDigitoControlCorrectoEAN13(true))
                {
                    txtCodigo.Focus();
                    txtCodigo.SelectAll();
                    return;
                }

                int prefijo = Convert.ToInt32(txtCodigo.Text.Substring(0, 2));
                //si  está entre 20 y 29 es codigo interno
                esCodBarraEstandar = !(prefijo > 19 && prefijo < 30);
                if (!esCodBarraEstandar)
                {
                    esCodBarraInterno = true;

                    codigoEnCodBarra = txtCodigo.Text.Substring(2, FormPrincipal.cantDigitosProdEnCodBarra);
                    segundoModulo = txtCodigo.Text.Substring((2 + FormPrincipal.cantDigitosProdEnCodBarra), (13 - ((2 + FormPrincipal.cantDigitosProdEnCodBarra + 1))));

                    txtCodigo.Text = codigoEnCodBarra;
                    // registrarTemporalLineaVenta();
                }
            }
                        

            if (esCodBarraEstandar)
            {
                //Si se llamó formCortes evitar el ingreso automatico de la cantidad
                if (codigoBuscado != 0 && oCorteE != null && codigoBuscado == oCorteE.codigo)
                {
                    return;
                }
                codigoBuscado = 0;

                ///si el CodigoBarra es de un producto pesable, Se lee el codigo, y se lee balanza. El usuario deberá agregar manualmente
                ///
                if (oCorteE != null && oCorteE.Pesable)
                {
                    checkLeerPeso.Checked = FormPrincipal.leerBalanza;
                    btnAgregar.Focus();
                    return;
                }

                ///si balanza no está activada y tiene una cantidad, se deja esa cantidad sino 1
                txtCantKgs.Text = (!checkLeerPeso.Checked && !string.IsNullOrEmpty(txtCantKgs.Text)) ? txtCantKgs.Text : "1";
                esCodBarraEstandar = false;
                codigoEnCodBarra = segundoModulo = "";
                agregarLinea();
                txtCodigo.Focus();
                txtCodigo.SelectAll();
            }
            if (esCodBarraInterno && oCorteE != null)
            {
                //checkLeerPeso.Checked = false;
                if (FormPrincipal.esCodBarraPorCantidad)
                {
                    txtCantKgs.Text = segundoModulo.Insert(segundoModulo.Length - 3, ".");
                }
                else
                {
                    txtTotalCorte.Text = segundoModulo.Insert(segundoModulo.Length - 2, ".");
                    float totalCorte = Util_Form.convertFloat(txtTotalCorte.Text, false);
                    txtCantKgs.Text = (totalCorte / oCorteE.precioKg).ToString("F3");
                }

                codigoEnCodBarra = segundoModulo = "";
                agregarLinea();
                txtCodigo.Focus();
                txtCodigo.SelectAll();
            }
        }

        private bool esDigitoControlCorrectoEAN13(bool mostrarMensaje)
        {
            #region DigitoControl_CodigoBarra
            string codigoBarra = txtCodigo.Text.Substring(0, txtCodigo.Text.Length - 1);// "400638133393"; // Primeros 12 dígitos del EAN-13
            int digitoControlDeCodBarra = Convert.ToInt32(txtCodigo.Text.Substring(12, 1));// Convert.ToChar(txtCodigo.Text[12]));
            int sumaImpares = 0;
            int sumaPares = 0;

            // Recorre cada dígito del código
            for (int i = 0; i < codigoBarra.Length; i++)
            {
                int digito = int.Parse(codigoBarra[i].ToString());

                // Si la posición es impar, suma a sumaImpares
                // Si la posición es par, suma a sumaPares
                if ((i + 1) % 2 == 0)  // Posiciones pares (1-indexed)
                {
                    sumaPares += digito;
                }
                else // Posiciones impares (1-indexed)
                {
                    sumaImpares += digito;
                }
            }

            // Multiplica la suma de los pares por 3
            sumaPares *= 3;

            // Suma total
            int sumaTotal = sumaImpares + sumaPares;

            // Cálculo del dígito de control
            int digitoControl = (10 - (sumaTotal % 10)) % 10;
            bool esCorrectoCodBarra = (digitoControl == digitoControlDeCodBarra);
            if(!esCorrectoCodBarra && mostrarMensaje)
                MessageBox.Show("Error al leer codigo de barra", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            
            return esCorrectoCodBarra;

            #endregion
        }

        public bool esDigitoControlCorrectoEAN8(bool mostrarMensaje)
        {
            string ean8 = txtCodigo.Text.Substring(0, txtCodigo.Text.Length - 1);// "400638133393"; // Primeros 12 dígitos del EAN-13
            int digitoControlDeCodBarra = Convert.ToInt32(txtCodigo.Text.Substring(7, 1));// Convert.ToChar(txtCodigo.Text[12]));
            int suma = 0;

            // Recorremos el código y aplicamos las multiplicaciones correspondientes
            for (int i = 0; i < 7; i++)
                {
                int digito = int.Parse(ean8[i].ToString());

                // Si el índice es impar (0, 2, 4, 6), multiplicamos por 3.
                // Si es par (1, 3, 5), multiplicamos por 1.
                suma += (i % 2 == 0) ? digito * 3 : digito;
            }

            // Calculamos el dígito de control
            int digitoControl = (10 - (suma % 10)) % 10;

            bool esCorrectoCodBarra = (digitoControl == digitoControlDeCodBarra);
            if (!esCorrectoCodBarra && mostrarMensaje)
                MessageBox.Show("Error al leer codigo de barra EAN8", "", MessageBoxButtons.OK, MessageBoxIcon.Error);

            return esCorrectoCodBarra;
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
            if (e.KeyChar == '*')// (char)(Keys.Multiply))
            {
                e.Handled = true;
                return;
            }

            if (e.KeyChar == (char)(Keys.Enter))
            {
                if (txtAbona.Focused) //si esta en abona finaliza la venta
                {
                    esModificacion();
                }
                else
                {

                    //20241013 - si hace foco y es vacío se manda un return para evitar el tab
                    if (txtCodigo.Focused && String.IsNullOrEmpty(txtCodigo.Text))
                    {
                        return;
                        ///Al ingrtesar el primer corte, luego de ingresar el codigo aparecera el cartel de forma pago
                        ///la idea es q NO muestre el total del corte sin antes poner la forma pago
                        //Solicitar forma de pago si balanza es distinta a nulo o cero                        
                        
                        //bool resp = !ingresarFormaPago() ? true: false;

                        
                    }
                    e.Handled = true;
                    SendKeys.Send("{TAB}");
                }                
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!FormPrincipal.leerBalanza) return;

                if (checkLeerPeso.Checked)
                {
                    if (fijarPeso)
                    {
                        txtCantKgs.Text = "0.200";
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
                            txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza();
                            lblErrorBalanza.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtCantKgs.Text = "Error balanza";
                lblErrorBalanza.Text = "Presione * (asterisco) para desctivar la balanza+\n" + ex.Message;
                lblErrorBalanza.Visible = true;
                txtCodigo.Focus();

                nroErrorBalanza++;
                //si tira error mas de 5 veces se desactiva balanza automaticamente y se pone contador en serio
                if (nroErrorBalanza > 10)
                {
                    timer1.Stop();
                    nroErrorBalanza = 0;
                    MessageBox.Show("Balanza desactivada automaticamente");
                }
            }
        }

        private void checkLeerPeso_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                checkLeerPeso.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkLeerPeso.Checked);

                if (checkLeerPeso.Checked && FormPrincipal.leerBalanza)
                {
                    dejarDeLeerPeso = false;
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
                    lblErrorBalanza.Visible = false;
                }

                //if (asteriscoPressKey && valorAnteriorBalanza == checkLeerPeso.Checked)
                //{
                //    asteriscoPressKey = false;
                //    txtCodigo.Text += "*";
                //    //checkLeerPeso.Checked = !checkLeerPeso.Checked;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formPOS_Load(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            this.Text = FormPrincipal.textForm;
            lblTeclasRapidas.Text = "Inicio = Codigo  |  Fin = Abonar  |  ESC = Salir  |  Insert = Forma Pago  |  Supr = Quitar Línea  | F2 = Pant.Principal  |   " +
                "F3 = Cálculo Billetes  | F4 = Bonificación  |  F5 = Nueva Compra  |  \n F6 = Mis Egresos Caja  |  F7 = Egresos Caja  | F8 = Facturacion | F9 = Buscar Cliente  |  " +
                "F10 = Buscar Producto  |  F12 = Bloquear | RePág = Cambiar Vendedor |  AvPág = Expendios";
            comboExpendioEstado.SelectedIndex = 0;
            comboFormaPago.SelectedIndex = 0;
            AplicarPlaceholder();

            if (oUsuario != null)
            {
                validarAperturaCaja();
                //se vuelve a validar que el usuario no sea nulo(sucede cuando no quiere abrir caja)
                if (oUsuario == null) return;

                oVentaE.ListaExpendios = new List<int> { 0 };
                oVentaE.ListaExpendios.Clear();
                oVentaE.Vendedor = oUsuario;
                oVentaE.ImprimirTipoCbte = Entidades.Venta.imprimirCbteEnum.Nulo.ToString();
                usuario.Text = oUsuario.User;
                txtVendedor.Text = oUsuario.Nombre;
                //this.Text = oUsuario.Nombre;

                ///TODO:cambiar color
                Color colorUser = string.IsNullOrEmpty(oUsuario.ColorForm) ?
                    System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129))))) : System.Drawing.Color.FromName(oUsuario.ColorForm);
               
                colorUser = System.Drawing.SystemColors.InactiveBorder;
                this.pnlBuscar.BackColor = colorUser;
                this.grupoCortes.BackColor = colorUser;
                //comboColors.Text = colorUser.ToString();
                //grillaLineasVenta.DefaultCellStyle.SelectionBackColor = colorUser;
                timerBloquearCaja.Start();
                ultimaVentaVendedor();
                restablecerFormaDePago();
                comboTipoComprobante.SelectedIndex = 0;
                checkBoxRedondeo.Checked = checkBoxRedondeo.Visible = redondeo;
                dtCortes = oCorteN.cargarDtCortes();

                timer1.Enabled = true;

                panelExpendios();
            }
            else
            {
                this.Close();
            }
        }

        private void AplicarPlaceholder()
        {
            txtClave.ForeColor = Color.Gray;
            txtClave.Text = "Contraseña";
            txtClave.PasswordChar = '\0'; // Quitar ocultamiento mientras se ve el placeholder

            txtClave.GotFocus += (s, e) =>
            {
                if (txtClave.Text == "Contraseña")
                {
                    txtClave.Text = "";
                    txtClave.ForeColor = Color.Black;
                    txtClave.PasswordChar = '*'; // Restaurar ocultamiento
                }
            };

            txtClave.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtClave.Text))
                {
                    txtClave.ForeColor = Color.Gray;
                    txtClave.Text = "Contraseña";
                    txtClave.PasswordChar = '\0'; // Mostrar texto plano
                }
            };

            //observaciones
            txtObservaciones.ForeColor = Color.Gray;
            txtObservaciones.Text = "Observaciones";

            txtObservaciones.GotFocus += (s, e) =>
            {
                if (txtObservaciones.Text == "Observaciones")
                {
                    txtObservaciones.Text = "";
                    txtObservaciones.ForeColor = Color.Black;
                }
            };

            txtObservaciones.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtClave.Text))
                {
                    txtObservaciones.ForeColor = Color.Gray;
                    txtClave.Text = "Observaciones";
                }
            };
        }

        private void validarAperturaCaja()
        {
            ///Cuando el usuario abre caja por primera vez oCierreE es null y lanza error
            ///Entonces valido y si es null, creo la instancia
            if (oCierreE == null)
                oCierreE = new Entidades.CierreCaja();

            oCierreE.Sucursal = oSucursalE;
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, "");
            if (oCierreE == null || !(oCierreE.UsuarioCierre == null || oCierreE.UsuarioCierre.Id.Equals(0)))
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
                    //si estable nulo el Usuario porque decidió no abrir caja
                    oUsuario = null;
                    formPOS_Load(null, null);
                }                
            }
        }

        private bool estaBloqueado()
        {
            return panelBloquear.Visible;
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Multiply:
                    if (btnAgregar.Focused)
                    {
                        KeyPressEventArgs e = new KeyPressEventArgs('*'); // Simula que se presiona '*'
                        TxtPruebaENTER_KeyPress(txtCodigo, e);
                        checkLeerPeso.Checked = FormPrincipal.leerBalanza ? !checkLeerPeso.Checked : checkLeerPeso.Checked;
                        break;
                    }
                    asteriscoPressKey = true;
                    valorAnteriorBalanza = dejarDeLeerPeso = checkLeerPeso.Checked;
                    checkLeerPeso.Checked = FormPrincipal.leerBalanza ? !checkLeerPeso.Checked : checkLeerPeso.Checked;
                    txtCodigo.Focus();
                    break;
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    cambiarPuntoDeVenta();
                    break;
                case Keys.End:
                    if (!estaBloqueado())
                    mostrarPago();
                    break;
                case Keys.PageDown:
                    expendirExpendios();
                    break;
                case Keys.Insert:
                    oVentaE.FormaPago = null;
                    ingresarFormaPago();
                    break;
                case Keys.Delete:
                    quitarLinea();
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
                case Keys.F3:
                        calculoBilletes();
                        break;
                case Keys.F4:
                        if (!estaBloqueado())
                            bonificarCorte();
                        break;
                case Keys.F5:
                    if (!estaBloqueado())
                        agregarCompra();
                    break;
                case Keys.F6:
                    if (!estaBloqueado())
                    misEgresoCaja();
                    break;
                case Keys.F7:
                    if (!estaBloqueado())
                    agregarEgresoCaja();
                    break;
                case Keys.F8:
                    if (!estaBloqueado())
                        facturaElectronica();
                    break;
                case Keys.F9:
                    if (!estaBloqueado())
                    buscarCliente();
                    break;
                case Keys.F10:
                    if (!estaBloqueado())
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

        private void agregarCompra()
        {
            bool formAbierto = false;

            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formNuevaCompra))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (ctrl.Name.Equals("pnlBuscar"))
                         {
                             foreach (Control child in ctrl.Controls)
                             {
                                 if (oUsuario != null && child.Name.Equals("txtUsuario") && child.Text.Equals(oUsuario.Nombre))
                                 {
                                     frm.BringToFront();
                                     formAbierto = true;
                                     break;
                                 }
                             }
                         }
                    }
                }
            }
            if (!formAbierto)
            {
                formNuevaCompra frmNuevaCompra = new formNuevaCompra();
                frmNuevaCompra.oUsuario = oUsuario;
                frmNuevaCompra.esEgresoCaja = true;
                frmNuevaCompra.Show();
            }
        }

        private void calculoBilletes()
        {
            formIngresoBilletes frmIngresoBilletes = new formIngresoBilletes();
            frmIngresoBilletes.ShowDialog();
        }
        private void sumarUltimasDosVentas()
        {
            //try
            //{
            //    oUltimaVentaVendedor = oVentaN.getUltimaVentaVendedor(oUsuario.Id);
            //    double totalUltimaVenta = 0;
            //    foreach (Entidades.LineaVenta linea in oUltimaVentaVendedor.LineasVenta)
            //    {
            //        totalUltimaVenta += linea.PrecioKg * linea.CantKg;
            //    }
            //    MessageBox.Show(totalUltimaVenta.ToString());
            //}
            //catch (Exception)
            //{
                
            //}
        }

        private void bonificarCorte()
        {
            if (grillaLineasVenta.SelectedRows.Count > 0)
            {
                int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla

                Entidades.LineaVenta oLineaVentaSelect = new Entidades.LineaVenta();
                oLineaVentaSelect = listaLineaVenta[nroFila];

                //se valida que el corte no haya sido anulado para poder bonificar
                foreach (Entidades.LineaVenta linea in listaLineaVenta)
                {
                    if (oLineaVentaSelect.IndexAnulado >= 0 || (oLineaVentaSelect.Corte.codigo == linea.Corte.codigo &&
                        linea.IndexAnulado == nroFila))
                    {
                        MessageBox.Show("No se pueden bonificar Productos anulados");
                        return;
                    }
                }

                //si es consumidor final no se permite bonificacion excepto que esté logueado como admin
                if (!FormPrincipal.logueado && !oUsuario.Admin && oCliente.idPersona.Equals(Entidades.Parametros.idConsumidorFinal))
                {
                    MessageBox.Show("No tienes permiso para realizar bonificaciones a un consumidor final.\n\nBusque el cliente o agréguelo para poder realizar la bonificación");
                    return;
                }

                precioBonificado = oLineaVentaSelect.PrecioKg.ToString("F2");
                //se busca si el producto seleccionado ya tiene bonificacion
                foreach (Entidades.LineaVenta lineaCargada in listaLineaVenta)
                {
                    if (!Entidades.LineaVenta.esAnulado(lineaCargada.Estado) && lineaCargada.Bonificacion != 0 && 
                        lineaCargada.Corte.codigo.Equals(oLineaVentaSelect.Corte.codigo) &&
                        lineaCargada.Random.Equals(oLineaVentaSelect.Random))
                    {
                        oLineaVentaSelect.PrecioKg = lineaCargada.PrecioKg;
                        break;
                    }
                }

                formBonificar frmBonificar = new formBonificar();
                frmBonificar.oLineaVenta = oLineaVentaSelect;
                frmBonificar.frmVentaCajaConExp = this;
                frmBonificar.ShowDialog();

                //si es Bonificar todos se recorre toda la lista y se la actualiza
                if (bonificarTodos)
                {
                    nroFila = 0;
                    float porcentajeBonif_Float = (100 - Utilidades.Util_Form.convertFloat(porcentajeBonif_String, false)) / 100;

                    ///TODO Permitir quitar bonificacion a todos los cortes si check todos está activado
                    ///en la descripcion de la bonificacion muestra decimal
                    ///si bonificacion todos es cero, no mostrar descripcion
                    for (nroFila = 0; nroFila < listaLineaVenta.Count; nroFila++)
                    {
                        listaLineaVenta[nroFila].PrecioKg = listaLineaVenta[nroFila].Corte.precioKg * porcentajeBonif_Float;
                        listaLineaVenta[nroFila].Bonificacion = Utilidades.Util_Form.convertFloat(porcentajeBonif_String, false);
                        listaLineaGrilla[nroFila].corte = listaLineaVenta[nroFila].Bonificacion == 0 ? listaLineaVenta[nroFila].Corte.CorteDesc :
                            (listaLineaVenta[nroFila].Corte.CorteDesc.Length < 9 ? listaLineaVenta[nroFila].Corte.CorteDesc : listaLineaVenta[nroFila].Corte.CorteDesc.Substring(0, 9)) + " (Bonif. " + listaLineaVenta[nroFila].Bonificacion.ToString("F2") + "%)";
                        listaLineaGrilla[nroFila].precioKg = listaLineaVenta[nroFila].PrecioKg;
                        listaLineaGrilla[nroFila].totalS = listaLineaGrilla[nroFila].precioKg * listaLineaGrilla[nroFila].KgsTotalCalculado;
                    }
                }
                else
                {
                    listaLineaVenta[nroFila].PrecioKg = Utilidades.Util_Form.convertFloat(precioBonificado, false);
                    //listaLineaVenta[nroFila].Bonificacion = (1 - (listaLineaVenta[nroFila].PrecioKg / listaLineaVenta[nroFila].PrecioReal)) * 100;
                    listaLineaVenta[nroFila].Bonificacion = (1 - (listaLineaVenta[nroFila].PrecioKg / listaLineaVenta[nroFila].Corte.precioKg)) * 100;
                    listaLineaGrilla[nroFila].corte = listaLineaVenta[nroFila].Bonificacion == 0 ? oLineaVentaSelect.Corte.CorteDesc :
                        (oLineaVentaSelect.Corte.CorteDesc.Length < 9 ? oLineaVentaSelect.Corte.CorteDesc : oLineaVentaSelect.Corte.CorteDesc.Substring(0, 9)) + " (Bonif. " + listaLineaVenta[nroFila].Bonificacion.ToString("F2") + "%)";
                    listaLineaGrilla[nroFila].precioKg = Utilidades.Util_Form.convertFloat(precioBonificado, false);
                    listaLineaGrilla[nroFila].totalS = listaLineaGrilla[nroFila].precioKg * listaLineaGrilla[nroFila].KgsTotalCalculado;
                }

                actualizarPrecios();
                cargarGrilla();

                //si bonificar todos no hubo error se informa el exito
                if (bonificarTodos)
                {
                    bonificarTodos = false;
                    MessageBox.Show("La bonificación se realizó correctamente.");
                }
                txtCodigo.Focus();
            }
            else
            {
                MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            oLineaVenta = null;
        }

        private void misEgresoCaja()
        {
            Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
            Entidades.CierreCaja oCierreE = new Entidades.CierreCaja();
            oCierreE.UsuarioInicio = oUsuario;
            oCierreE.Sucursal = oSucursalE;

            formEgresosCajaVendedor frmEgresosCajaVendedor = new formEgresosCajaVendedor();
            frmEgresosCajaVendedor.oCierreE = oCierreN.findByIdOrLast(oCierreE, Entidades.CierreCaja.tipoBusqueda.FindLast, null);
            frmEgresosCajaVendedor.ShowDialog();
        }

        private void agregarEgresoCaja()
        {
            if (panelBloquear.Visible) return;
            formAddOrEditEgresoCaja frmAddOrEditEgresoCaja = new formAddOrEditEgresoCaja();
            frmAddOrEditEgresoCaja.oUsuario = oUsuario;
            frmAddOrEditEgresoCaja.egresoDesdeCajaVenta = true;
            frmAddOrEditEgresoCaja.ShowDialog();
        }

        private void cambiarPuntoDeVenta()
        {
            bool cambioForm = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formPOS))
                {
                    foreach (Control ctrl in frm.Controls)
                    {
                        if (oUsuario != null && ctrl.Name.Equals("usuario") && !ctrl.Text.Equals(oUsuario.User))
                        {
                            Utilidades.BarraProgreso barraProgreso = new Utilidades.BarraProgreso(null ,ctrl.Text.ToUpper());
                            barraProgreso.ShowDialog();
                            cambioForm = true;
                            frm.TopMost = true;  // Asegúrate de que esté por encima de otras ventanas
                            frm.BringToFront();
                            frm.Activate();
                            frm.TopMost = false; // Restablece su estado normal
                            break;
                        }
                    }
                }
                if (cambioForm)
                { break; }
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

        private void formPOS_FormClosing(object sender, FormClosingEventArgs e)
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
            panelBloquear.BringToFront();
            btnBloquear.Visible = false;
            btnAceptar.Enabled = false;
            btnAbonar.Enabled = false;
            grupoCortes.Enabled = false;
            pnlBuscar.Enabled = false;
            panelPago.Enabled = false;
            grillaLineasVenta.Enabled = false;
            panelDespliegue.Enabled = false;
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
                grillaLineasVenta.Enabled = true;
                panelDespliegue.Enabled = true;

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
            //btnAceptar.BackColor = Color.FromName("LimeGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            //btnAceptar.BackColor = Color.FromName("HotTrack");
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
                panelAbonar.Visible = false;//!checkEfectivo.Checked;// 
                panelPago.Visible = true;// checkEfectivo.Checked;
                txtAbona.ReadOnly = !checkEfectivo.Checked;
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

            ingresarFormaPago();
        }

        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            try
            {
                ///Al leer con la pistola se aplica el tab
                ///entonces aca se valida si es codigo de barra cuando todo el campo codigo está cargado 
                ///y no surge el problema de cortar un ean13 en 8 digitos
                ///
                codigoDeBarraMetodo();

                ///Si esCodBarraInterno es TRUE - salgo del método para evitar error
                ///
                if (esCodBarraInterno)
                {
                    esCodBarraInterno = false;
                    return;
                }

                this.txtCodigo.BackColor = enableColor;

                if ((!string.IsNullOrEmpty(txtCodigo.Text) && oCorteE != null && oCorteE.idCorte > 0 &&
                    !oCorteE.Pesable && !esCodBarraEstandar && !esCodBarraInterno && checkLeerPeso.Checked) ||
                    (!esCodBarraEstandar && !esCodBarraInterno && !FormPrincipal.leerBalanza))
                {
                    checkLeerPeso.Checked = false;
                    txtCantKgs.Focus();
                }
                else
                {
                    ///todo probar !string.IsNullOrEmpty(txtCodigo.Text)
                    if (!string.IsNullOrEmpty(txtCodigo.Text) && !dejarDeLeerPeso && oCorteE != null && oCorteE.idCorte > 0 && 
                        oCorteE.Pesable && !esCodBarraEstandar && !esCodBarraInterno && !checkLeerPeso.Checked)
                    {
                        checkLeerPeso.Checked = true;
                        btnAgregar.Focus();
                    }
                }

                if (cartelPrimerCorteVendedor && !this.txtCodigo.Text.Equals("") && grillaLineasVenta.Rows.Count.Equals(0))
                {
                    int cantCajaVenta = 0;
                    foreach (Form frm in Application.OpenForms)
                    {
                        if (frm.GetType() == typeof(formPOS))
                        {
                            cantCajaVenta++;
                            if (cantCajaVenta > 1)
                            {
                                titilarTextBoxVendedor();
                                break; 
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hubo un error al cargar el Producto.\nMetodo: txtCodigo_Leave().\n\n" + ex.Message);
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
            ingresarFormaPago();
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
            checkTicket.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkTicket.Checked);
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
            sumaTitilar += timerTitilar.Interval;
            if (sumaTitilar > titilarHasta)
            {
                txtVendedor.BackColor = readOnlyColor;
                timerTitilar.Stop();
            }
        }

        private void titilarTextBoxVendedor()
        {
            sumaTitilar = 0;
            timerTitilar.Start();
        }

        private void panelExpendios()
        {
            ///panel expendio
            panelDespliegue.Width = 30;// Inicialmente colapsado
            panelDespliegue.Dock = DockStyle.Right;
            panelExpendioLateral.Parent = this;
        }

        private void btnExpandir_Click(object sender, EventArgs e)
        {
            expendirExpendios();
        }

        private void expendirExpendios()
        {
            if (isExpanded)
            {
                // Colapsar el panel
                panelDespliegue.Width = 0;
                panelDespliegue.Visible = false;
                // btnTogglePanel.Text = "Mostrar Panel";
                txtCodigo.Focus();  
            }
            else
            {
                // Expandir el panel
                panelDespliegue.Visible = true;
                panelDespliegue.Width = 450; // Ajusta la altura según el contenido
                panelDespliegue.BringToFront();
                dtExpendios = oVentaN.obtenerUltimosExpendios(Convert.ToInt32(txtMinutosDesde.Text), oSucursalE.IdSucursal);
                filtrarExpendio();
                txtBuscarExpendio.Focus();
            }

            isExpanded = !isExpanded;
            panelExpendioLateral.Visible = !isExpanded;
            panelExpendioLateral.Parent = this;
        }

        private void txtBuscarExpendio_TextChanged(object sender, EventArgs e)
        {
            //para que actualice la grilla cuando se vacia el textBox xq sino cargaba la lista de la BD
            if (string.IsNullOrWhiteSpace(txtBuscarExpendio.Text))
                changeComboExpendio = true;

            filtrarExpendio();
        }
        private void filtrarExpendio()
        {

            // Crear un nuevo DataTable con la misma estructura que el original
            tablaFiltrada = dtExpendios.Clone();

            if (dtExpendios.Rows.Count > 0)
            {
                try
                {
                    string filtroExpendioEnVenta = "";
                    if (oVentaE.ListaExpendios != null && oVentaE.ListaExpendios.Count > 0)
                    {
                        foreach (int item in oVentaE.ListaExpendios)
                        {
                            filtroExpendioEnVenta += comboExpendioEstado.Text == "PENDIENTES" ? " AND idExpendio <> " + item : " OR idExpendio = " + item;
                        }
                    }

                    //para poder filtar identif.Cliente q es string se asigna '0' a filtro de idExpendio
                    string filtroPorId = int.TryParse(txtBuscarExpendio.Text, out int numero) ? txtBuscarExpendio.Text : "0";
                    string filtroPorIdentif = "identificacionExpendio LIKE " + (int.TryParse(txtBuscarExpendio.Text, out int numero1) ? "'" + txtBuscarExpendio.Text + "'" : "'%"+ txtBuscarExpendio.Text + "%'");
                    string filtroExpendio = !string.IsNullOrWhiteSpace(txtBuscarExpendio.Text) ? "(idExpendio = " + filtroPorId + " OR "+ filtroPorIdentif + " ) AND " : string.Empty;
                    string filtroCombo = comboExpendioEstado.Text == "PENDIENTES" ? "(idVenta IS NULL OR idVenta = 0)"+filtroExpendioEnVenta: "(idVenta IS NOT NULL AND idVenta > 0)"+filtroExpendioEnVenta;
                    string filtroCompleto = !string.IsNullOrEmpty(filtroExpendio) ? filtroExpendio+filtroCombo : filtroCombo;
                    DataRow[] filas = dtExpendios.Select(filtroCompleto);
                    
                    // Crear un nuevo DataTable con la misma estructura que el original
                    tablaFiltrada = dtExpendios.Clone();

                    // Importar cada DataRow del arreglo al nuevo DataTable
                    foreach (DataRow fila in filas)
                    {
                        tablaFiltrada.ImportRow(fila);
                    }

                    // Asignar el DataTable filtrado al DataGridView
                    grillaExpendios.DataSource = tablaFiltrada;
                    
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Valor demasiado grande o demasiado pequeño para Int32."))
                        return;

                    MessageBox.Show("Error al cargar Producto\n\n" + ex.Message);
                }
            }
            else
            {
                tablaFiltrada = dtExpendios;
            }
            grillaExpendios.DataSource = tablaFiltrada;
        }

        private void txtBuscarExpendio_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)(Keys.Enter))
                {
                    ///Si combo es "ASIGNADOS" 
                    if (!comboExpendioEstado.Text.Equals("PENDIENTES"))
                    {

                        MessageBox.Show($"Los Expendios seleccionados ya han sido asignados a ésta u otra venta.",
                                        "Error de Validación",
                                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }                    

                    //se verifica que no hay valores diferentes en identificacion expendio
                    // Verificar si hay filas suficientes en el DataGridView
                    if (grillaExpendios.Rows.Count > 1)
                    {
                        // Recorrer las filas del DataGridView
                        for (int i = 0; i < grillaExpendios.Rows.Count; i++)
                        {
                            // Ignorar filas vacías o nuevas
                            if (grillaExpendios.Rows[i].IsNewRow) continue;

                            // Obtener el valor de referencia
                            var valorReferencia = grillaExpendios.Rows[i].Cells["identificacionExpendio"].Value?.ToString();

                            // Comparar el valor de referencia con los demás valores
                            for (int j = 0; j < grillaExpendios.Rows.Count; j++)
                            {
                                // Evitar comparar la misma fila
                                if (i == j || grillaExpendios.Rows[j].IsNewRow) continue;

                                var valorComparado = grillaExpendios.Rows[j].Cells["identificacionExpendio"].Value?.ToString();

                                // Verificar si el valor comparado es diferente
                                if (valorComparado != valorReferencia && !expendioDesdeCodigoBarra)
                                {
                                    MessageBox.Show($"Los valores en las filas {i + 1} y {j + 1} de 'Identif. Cliente' no coinciden.\n"+
                                        "Para evitar error clique el botón sobre la fila de Nro.Expendio.",
                                                    "Error de Validación",
                                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }
                    }

                    for (int i = 0; i < grillaExpendios.Rows.Count; i++)
                    {
                        DataGridViewRow fila = grillaExpendios.Rows[i];

                        // Verificar que la fila no sea una fila nueva
                        if (!fila.IsNewRow)
                        {
                            agregarExpendio(fila);
                        }
                    }

                    if (grillaExpendios.Rows.Count == 0)
                    {
                        MessageBox.Show($"No se encontró el Nro Expendio: {txtBuscarExpendio.Text}\nPuede que no exista o ya haya sido asignado a una venta\n"+
                            "Realice una busqueda manual incrementando la cantidad de minutos",
                                                "Error en Expendio",
                                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    //si se agregó desde Codigo se verifica que exista el expendio
                    if (expendioDesdeCodigoBarra)
                    {
                        expendioDesdeCodigoBarra = false;
                        txtCodigo.Text = "";
                        expendirExpendios();
                        txtBuscarExpendio.Text = "";
                        expendioDesdeCodigoBarra = false;
                        return;
                    }

                    filtrarExpendio();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error al cargar las lineas de expendios\n" + ex.Message,"",MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void agregarExpendio(DataGridViewRow fila)
        {
            // Obtener el valor de una celda específica
            txtCodigo.Text = fila.Cells["cod"].Value.ToString();
            precioKgCorteExpendio = Utilidades.Util_Form.convertFloat(fila.Cells["precio"].Value.ToString(), false); 
            txtCantKgs.Text = fila.Cells["Cant"].Value.ToString();
            //se agrega el expendio a la venta
            idExpendioVenta = Convert.ToInt32(fila.Cells["idExpendio"].Value);
            if (!oVentaE.ListaExpendios.Contains(idExpendioVenta))
                oVentaE.ListaExpendios.Add(idExpendioVenta);

            agregarLinea();

            fila.Cells["idVenta"].Value = "1";
            //se reinicia idExpendio
            idExpendioVenta = 0;
        }

        private void comboExpendioEstado_SelectedIndexChanged(object sender, EventArgs e)
        {
            changeComboExpendio = true;
            filtrarExpendio();
            changeComboExpendio = false;
        }

        private void btnQuitarAsignados_Click(object sender, EventArgs e)
        {
            if (oVentaE.ListaExpendios == null || oVentaE.ListaExpendios.Count == 0)
            {
                MessageBox.Show($"La Venta aún no tiene asignado Expendios.",
                                "Informe",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult respuesta;
            respuesta = MessageBox.Show("¿Está seguro que desea quitar expendios de la venta?.", "Eliminar expendios", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == DialogResult.No)
                return;

            listaLineaVenta.RemoveAll(item => item.IdExpendio > 0);
            listaLineaGrilla.RemoveAll(item => item.IdExpendio > 0);
            oVentaE.ListaExpendios.Clear();
            idExpendioVenta = 0;
            cargarGrilla();
            filtrarExpendio();
        }

        private void grillaExpendios_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {

                // Ignore clicks that are not on button cells.  
                if (e.RowIndex < 0 || e.ColumnIndex !=
                    grillaExpendios.Columns["btnAgregarExpendio"].Index) return;
                ///Si combo es "ASIGNADOS" 
                if (!comboExpendioEstado.Text.Equals("PENDIENTES"))
                {
                    MessageBox.Show($"Los Expendios seleccionados ya han sido asignados a ésta u otra venta.",
                                    "Error de Validación",
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Obtener el valor de la columna "identificacionExpendio" de la fila seleccionada
                var valorSeleccionado = grillaExpendios.CurrentRow.Cells["idExpendio"].Value?.ToString();
                // Recorrer todas las filas del DataGridView
                foreach (DataGridViewRow fila in grillaExpendios.Rows)
                {
                    // Ignorar filas vacías o nuevas
                    if (fila.IsNewRow) continue;

                    // Obtener el valor actual de la columna "identificacionExpendio"
                    var valorActual = fila.Cells["idExpendio"].Value?.ToString();

                    cargandoExpendios = true;
                    // Comparar si coincide con el valor seleccionado
                    if (valorActual == valorSeleccionado)
                    {
                        agregarExpendio(fila);
                    }
                }
                cargandoExpendios = false;
                filtrarExpendio();
            }
            catch (Exception)
            {
                cargandoExpendios = false;
                throw;
            }
        }

        private void txtMinutosDesde_SelectedItemChanged(object sender, EventArgs e)
        {
            dtExpendios = oVentaN.obtenerUltimosExpendios(Convert.ToInt32(txtMinutosDesde.Text), oSucursalE.IdSucursal);
            filtrarExpendio();
        }

        private void btnDespligueLateral_Click(object sender, EventArgs e)
        {
            expendirExpendios();
        }

        private void comboFormaPago_SelectedIndexChanged(object sender, EventArgs e)
        {
            //FORMA PAGO
            //EFECTIVO
            //DEBITO
            //CREDITO
            //QR
            //TRANSFERENCIA
            //CTA.CTE

            string formaPago = comboFormaPago.SelectedItem?.ToString().ToUpper();            

            foreach (Control ctrl in groupFormaPago.Controls)
            {
                if (ctrl is CheckBox chk)
                {
                    if (string.IsNullOrEmpty(formaPago) || formaPago.Equals("FORMA PAGO"))
                        chk.Checked = false;
                    else
                    {
                        string textReplace = chk.Text.Replace(" ", "").ToUpper();
                        if (formaPago.Contains(textReplace))
                        {
                            chk.Checked = true;
                            break; // si solo querés marcar uno
                        }
                    }
                }
            }
        }

        private void comboFormaPago_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.End)
            {
                e.Handled = true;
                e.SuppressKeyPress = true; // evita que el control reciba la tecla
            }
        }

        private void grillaLineasVenta_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void checkPagoMixto_CheckedChanged(object sender, EventArgs e)
        {
            validarPagoMixto();
        }

        private void validarPagoMixto()
        {
            if (checkPagoMixto.Checked && (checkEfectivo.Checked || checkCtaCte.Checked || oVentaE.FormaPago == null))
            {
                MessageBox.Show("Para 'Pago Mixto' debe seleccionar una forma pago y ser diferente a Efectivo y Cta.Cte", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                checkPagoMixto.Checked = false;
                checkEfectivo.Checked = false;
                checkCtaCte.Checked = false;
            }
        }

        private void lblUltimaVenta_Click(object sender, EventArgs e)
        {

            if (ultimaVenta)
            {
                ultimaVentaVendedor();
                //se valida que no hay pasado el limite de tiempo para editar la venta
                if (!FormPrincipal.logueado && oUltimaVentaVendedor.Creado.AddMinutes(15) < DateTime.Now)
                {
                    MessageBox.Show("Caducó el tiempo para modificar la venta.\n\n(Inicie sesión como admin para poder modificar)",
                        "Tiempo Caducado");
                    return;
                }

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

        private void linkVerCtaCte_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                formCtaCtePersona frmCtaCtePersona = new formCtaCtePersona();
                frmCtaCtePersona.idPersona = oCliente.idPersona;
                frmCtaCtePersona.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
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
                lblHoraUltimaVenta.Text = oUltimaVentaVendedor.FechaVenta.ToShortDateString() +
                    " " + oUltimaVentaVendedor.FechaVenta.ToShortTimeString() +
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

        private void checkLeerPeso_Enter(object sender, EventArgs e)
        {
            capturarPantalla();
            capturarPantallaFinal = true;
        }

        private void capturarPantalla()
        {
            Utilidades.Util_Form.capturarPantalla(txtVendedor.Text, DateTime.Now);
        }

        private void checkCtaCte_CheckedChanged(object sender, EventArgs e)
        {
            checkCtaCte.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCtaCte.Checked);
        }

        private void comboColors_DrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {                

                ComboBox cmb = sender as ComboBox;
                if (cmb == null) return;
                if (e.Index < 0) return;
                if (!(cmb.Items[e.Index] is Color)) return;
                Color color = (Color)cmb.Items[e.Index];
                // Dibujamos el fondo
                e.DrawBackground();
                // Creamos los objetos GDI+
                Brush brush = new SolidBrush(color);
                Pen forePen = new Pen(e.ForeColor);
                Brush foreBrush = new SolidBrush(e.ForeColor);
                // Dibujamos el borde del rectángulo
                e.Graphics.DrawRectangle(
                    forePen,
                    new Rectangle(e.Bounds.Left + 2, e.Bounds.Top + 2, 19,
                        e.Bounds.Size.Height - 4));
                // Rellenamos el rectángulo con el Color seleccionado
                // en la combo
                e.Graphics.FillRectangle(brush,
                    new Rectangle(e.Bounds.Left + 3, e.Bounds.Top + 3, 18,
                        e.Bounds.Size.Height - 5));
                // Dibujamos el nombre del color
                e.Graphics.DrawString(color.Name, cmb.Font,
                    foreBrush, e.Bounds.Left + 25, e.Bounds.Top + 2);
                // Eliminamos objetos GDI+
                brush.Dispose();
                forePen.Dispose();
                foreBrush.Dispose();
            }
            catch (Exception)
            {
                
            }
        }

        //private void comboColors_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        this.pnlBuscar.BackColor = (Color)comboColors.SelectedItem;
        //        this.grupoCortes.BackColor = (Color)comboColors.SelectedItem;
        //    }
        //    catch (Exception)
        //    {
        //    }
        //}

        private void linkUltimasVentasCliente_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (oCliente.idPersona == Entidades.Parametros.idConsumidorFinal)
            {
                formVentasVendedor frmVentasVendedor = new formVentasVendedor();
                frmVentasVendedor.desdeCajaVenta = true;
                frmVentasVendedor.oCierreE = oCierreE;
                frmVentasVendedor.ShowDialog();
            
            }
            else
            {
                formGetAllLineaVenta frmGetAllLV = new formGetAllLineaVenta();
                frmGetAllLV.verUltimasVentasClientes = true;
                frmGetAllLV.desdeCajaVenta = true;
                frmGetAllLV.idPersona = oCliente.idPersona;
                frmGetAllLV.idSucursal = oSucursalE.idSucursal;
                frmGetAllLV.ShowDialog();
            }
        }

        private void checkBoxRedondeo_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRedondeo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkBoxRedondeo.Checked);
        }

        #region FormaPago
        private void restablecerFormaDePago()
        {
            oVentaE.FormaPago = null;

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
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString();
                actualizarPrecios();
            }
        }

        private void checkDebito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkDebito.Checked)
            {
                checkEfectivo.Checked = checkCredito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Debito.ToString();
                actualizarPrecios();
            }
        }

        private void checkCredito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCredito.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCtaCtePago.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Credito.ToString();
                actualizarPrecios();
            }
        }

        private void checkCtaCtePago_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCtaCtePago.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkQr.Checked =
                    checkTransf.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.CtaCte.ToString();      
                actualizarPrecios();
            }

            checkCtaCte.Checked = checkCtaCtePago.Checked;
        }

        private void checkQr_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkQr.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = checkCtaCtePago.Checked =
                    checkTransf.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Qr.ToString();
                actualizarPrecios();
            }

        }

        private void checkTransf_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkTransf.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = 
                    checkCtaCtePago.Checked = checkQr.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Transferencia.ToString();
                actualizarPrecios();
            }

        }

        private void actualizarPrecios()
        {
            string codigoCorteIngresado = txtCodigo.Text;
            txtCodigo.Text = "";
            if (grillaLineasVenta.Rows.Count > 0)
            {
                for (int index = 0; index < listaLineaVenta.Count; index++)
                {
                    Entidades.LineaVenta linea = listaLineaVenta[index];
                    oCorteE = linea.Corte;
                    establecerPrecioCorteSegunFormaPago();
                    listaLineaVenta[index].Corte = oCorteE;

                    if (linea.Bonificacion == 0)
                    {
                        //oCorteE = linea.Corte;
                        //establecerPrecioCorteSegunFormaPago();

                        //listaLineaVenta[index].Corte = oCorteE;
                        listaLineaVenta[index].PrecioKg = oCorteE.precioKg;

                        listaLineaGrilla[index].precioKg = oVentaE.bonificar(oCliente, oCorteE.precioKg, false);
                        listaLineaGrilla[index].totalS = listaLineaGrilla[index].precioKg * listaLineaGrilla[index].KgsTotalCalculado;
                    }
                }
                cargarGrilla();
            }
            //se vuelve a cargar el codigo ingresado para actualizar el precio
            txtCodigo.Text = codigoCorteIngresado;
        }

        #endregion

        private void comboTipoComprobante_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTipoComprobante.SelectedItem.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString()))
            {
                txtCuit.ReadOnly = txtDomicilio.ReadOnly = true;
                txtCodigo.Focus();
            }
            else
            {
                txtCuit.ReadOnly = txtDomicilio.ReadOnly = false;
                txtCuit.Focus();
            }
        }

        private void duplicarVentana_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            formPOS frmVentaCajaDuplicada = new formPOS();
            frmVentaCajaDuplicada.oUsuario = oUsuario;
            frmVentaCajaDuplicada.ventanaDuplicada = true;
            frmVentaCajaDuplicada.duplicarVentana.Visible = false;
            frmVentaCajaDuplicada.Show();
        }
    }
}
