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

namespace Presentacion.Caja
{
    public partial class formVentaCaja : Form, InterfaceCorte, InterfacePersona, InterfaceUsuario, InterfaceFormaPago
    {
        bool pesoBalanza = false;
        bool capturarPantallaFinal = false;
        Utilidades.SingletonLeerPeso Leer_Peso;
        Utilidades.Util_Form Util_Form = new Utilidades.Util_Form();
        #region variables
        public string vendedor = "-";
        public string precioBonificado = "";
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

        bool dejarDeLeerPeso = false;
        int sucAnterior;
        int tiempoInactivo = 0;
        int tiempoBloqueo = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoBloqueo"].ToString());
        int sumaTitilar = 0;
        int titilarHasta = 2000;
        int tiempoRegistrarTemporal = Convert.ToInt32(ConfigurationManager.AppSettings["tiempoRegistrarTemporal"].ToString());
        string ultimoTextoEnTxtCodigo = "";
        Random randomClass = new Random();

        public int SucAnterior
        {
            get { return sucAnterior; }
            set { sucAnterior = value; }
        }

        bool modificar = false;
        bool fijarPeso = Convert.ToBoolean(ConfigurationManager.AppSettings["fijarPeso"].ToString());
        bool cartelPrimerCorteVendedor = Convert.ToBoolean(ConfigurationManager.AppSettings["cartelPrimerCorteVendedor"].ToString());
        bool ultimaVenta = Convert.ToBoolean(ConfigurationManager.AppSettings["ultimaVenta"].ToString());
        string fecha = "", estadoVenta = "", detalleRedondeo;
        float totalCorte, precioKg, cantKg, cantKgTarjeta, kgsTotalCalculado;
        float totalVenta = 0, abona = 0, cambio = 0, ganPesosTotRedondeo = 0, ganKgsTotRedondeo = 0,
            ganPesosRedondeoLinea = 0, ganKgsRedondeoLinea = 0, acumRedondeoKgs = 0, acumRedondeImporte = 0;
        /// TODO: agregar los campos en tablas de BD y obtener de allí los valores
        /// porcAjEfectivo, porcAjDebito, porcAjCredito, limiteKgParaAjuste;
        /// **Crear tabla General con los campos IdConsumidorFinal
        /// **Obtener los pesos de condimentos en embutidos desde BD y NO desde app.config!!
        float porcAjEfectivo, porcAjDebito, porcAjCredito, limiteKgParaAjuste;
        bool esAjustePorcTarj = false;
        int idConsumidorFinal;

        #endregion


        public formVentaCaja()
        {
            InitializeComponent();

            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            this.KeyPreview = true;

            //asigo sucursal a la venta  
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oVentaE.Sucursal = oSucursalE;
            this.txtSucursal.Text = oVentaE.Sucursal.sucursal;
            Negocio.Persona oPersonaN = new Negocio.Persona();
            idConsumidorFinal = Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString());
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            txtFecVenta.Text = DateTime.Now.ToString();
            if (!fecha.Equals(""))
            {
                txtFecVenta.Text = DateTime.Parse(fecha).ToString();
            }
            checkCtaCte_CheckedChanged(null,null);
            checkLeerPeso.Visible = (FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["leerPesoCaja"].ToString()));
            checkTicket.Visible = FormPrincipal.logueado || Convert.ToBoolean(ConfigurationManager.AppSettings["ticket"].ToString());

            //se cargar los porcentajes de ajuste por tarjeta
            porcAjEfectivo = float.Parse("1,010");//obtener de Base de Datos
            porcAjDebito = float.Parse("1,010");
            porcAjCredito = float.Parse("1,010");
            limiteKgParaAjuste = float.Parse("6");
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
                            if (grillaLineasVenta.Rows[nroFila].Cells["Corte"].Value.ToString().Length > 22)
                            {
                                grillaLineasVenta.Rows[nroFila].Cells["Corte"].Style.Font = new Font(grillaLineasVenta.Font.ToString(), 13);
                            }
                            if (Convert.ToInt32(grillaLineasVenta.Rows[nroFila].Cells["Codigo"].Value) == linea.Corte.codigo &&
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
                    oVentaE.IdVenta = oVentaN.agregarVenta(oVentaE);
                    Ticket.CreaTicket ticket = new Ticket.CreaTicket();

                    bool esEfectivo = oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString());
                   
                    //se genera el egreso de caja si paga con tarjeta
                    if (!esEfectivo)
                        egresoCajaPagoTarjeta(oVentaE);

                    //imprimir si está checked o no es efectivo
                    ticket.imprimir = checkTicket.Checked || !esEfectivo;
                    ticket.TextoCentro("x");
                    ticket.NoValidoComoFactura();
                    ticket.LineasEnBlanco(1);
                    if (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                        ticket.TextoCentro("A Cta. Cte.");
                    //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
                    ticket.TextoIzquierda("A " + oVentaE.Persona.razonSocial);
                    ticket.TextoIzquierda("Forma Pago: " + oVentaE.FormaPago.ToString());
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
                    //foreach (Entidades.LineaVenta linea in listaLineaVenta)
                    //{
                    //    oVentaN.agregarLineaVenta(linea);
                    //    ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                    //        linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                    //}
                    //ticket.LineasEnBlanco(1);
                    ticket.TextoDerecha("-------");
                    ticket.AgregaTotales("Total", totalVenta);
                    //si se ingresa la cantidad del pago se imprime
                    if (abona > 0)
                    {
                        ticket.AgregaTotales("Pago", abona);
                        ticket.AgregaTotales("Vuelto", cambio);
                    }
                    ticket.LineasEnBlanco(1);
                    ticket.TextoIzquierda("Articulos: " + txtCantItems.Text);// + "   Cajero: " + txtVendedor.Text);
                    //ticket.TextoIzquierda("Cajero: " + txtVendedor.Text);
                    ticket.TextoIzquierda("Cajero: " + oUsuario.Id);
                    ticket.GraciasPorSuCompra();
                    ticket.LineasEnBlanco(2);

                    //Agregar en Cta Cte
                    try
                    {
                        oVentaN.crearMovCtaCteVenta(oVentaE);

                        //se genera el egreso de caja por Cta. Cte
                        if(oVentaE.EnCtaCte) 
                            egresoCajaPorCtaCte(oVentaE);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al crear el Movimiento en la Cuenta Corriente.\n\n**La Venta se registró correctamente**\n\n" + ex.Message);
                    }

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

                //Solo imprimo Egreso Caja si venta es en Efectivo
                if (oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                    imprimirTicket(oEgresoCajaE);
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
                Entidades.EgresoCaja oEgresoCajaE = new Entidades.EgresoCaja();

                oEgresoCajaE.Fecha = oVentaConEgresoCaja.FechaVenta;
                oEgresoCajaE.IdTipoEgresoCaja = Entidades.EgresoCaja.idPagoTarjeta;
                oEgresoCajaE.Descripcion = "Venta " + oVentaConEgresoCaja.FormaPago.ToString() + " - ID:" + oVentaConEgresoCaja.IdVenta.ToString();
                oEgresoCajaE.Monto = float.Parse(txtTotalS.Text);// oVentaN.getTotalVenta(oVentaConEgresoCaja.IdVenta);
                oEgresoCajaE.Detalle = " | Kgs: " + txtTotalKgs.Text + 
                    " | Precio: " + (float.Parse(txtTotalS.Text) / float.Parse(txtTotalKgs.Text)).ToString("N3") + 
                    " | TOT: " + txtTotalS.Text;
                oEgresoCajaE.Sucursal = oVentaConEgresoCaja.Sucursal;
                oEgresoCajaE.IdCompra = 0;
                oEgresoCajaE.Tabla = Entidades.EgresoCaja.tablas.Ventas.ToString();
                oEgresoCajaE.IdTabla = oVentaConEgresoCaja.IdVenta;
                oEgresoCajaE.CreadoPor = oVentaConEgresoCaja.Vendedor.Id;
                oEgresoCajaE.ActualizadoPor = oEgresoCajaE.Id > 0 ? (oUsuario != null ? oUsuario.Id : -1) : -1;

                Negocio.CierreCaja oCierreN = new Negocio.CierreCaja();
                oEgresoCajaE = oCierreN.addOrEditEgresoCaja(oEgresoCajaE);
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
            int idConsumidorFinal = Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString());
            oCliente = oPersonaN.findById(idConsumidorFinal);
            this.txtCliente.Text = oCliente.razonSocial;
            txtCuit.Text = "";
            txtEmail.Text = "";
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
            linkUltimasVentasCliente.Visible = false;
            restablecerFormaDePago();
            comboTipoComprobante.SelectedIndex = 0; //Remito

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
            oVentaE.FechaVenta = Convert.ToDateTime(txtFecVenta.Text);
            oVentaE.NroRemito = txtNroRemito.Text.Trim();
            oVentaE.Turno = acumRedondeoDetalle;
            oVentaE.DiaFestivo = "";
            oVentaE.Observaciones = txtObservaciones.Text.Trim();
            oVentaE.Estado = estadoVenta;
            oVentaE.EnCtaCte = checkCtaCte.Checked; 
            //Si FormaPago <> 'Efectivo y TipoCombrobante es 'X' entonces establecer tipoComprobante 'B'
            oVentaE.TipoComprobante = (!(oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))&& 
                comboTipoComprobante.SelectedItem.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString())) ?
                Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.B.ToString()) : Convert.ToChar(comboTipoComprobante.SelectedItem.ToString());
            oVentaE.Cuit = txtCuit.Text;
            oVentaE.Email = txtEmail.Text;
            oVentaE.AcumRedondeoImporte = ganPesosTotRedondeo;
            oVentaE.AcumRedondeoKgs = ganKgsTotRedondeo;
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
            lineaVentaP.corte = lineaE.Bonificacion == 0 ? lineaE.Corte.CorteDesc :
                    (lineaE.Corte.CorteDesc.Length < 9 ? lineaE.Corte.CorteDesc :
                    lineaE.Corte.CorteDesc.Substring(0, 9)) + 
                    " (Bonif. " + lineaE.Bonificacion.ToString("F2") + "%)";
            lineaVentaP.cantKgs = lineaE.CantKg;
            lineaVentaP.kgsTotalCalculado = lineaE.KgsTotalCalculado;
            lineaVentaP.kgsAjusteTarj = lineaE.KgsAjusteTarj;
            //lineaVentaP.precioKg = oVentaE.bonificar(oCliente, lineaE.PrecioKg, lineaE.Corte.Mayorista);
            lineaVentaP.precioKg = oVentaE.bonificar(oCliente, lineaE.Corte.precioKg, lineaE.Corte.Mayorista);
            lineaVentaP.totalS = lineaE.PrecioKg * lineaE.KgsTotalCalculado;
            lineaVentaP.Random = lineaE.Random;

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
        }

        private bool ingresarFormaPago()
        {
            bool resp = true;
            if (string.IsNullOrEmpty(oVentaE.FormaPago))
            {
                checkEfectivo.Checked = checkDebito.Checked = checkCredito.Checked = false;//Asegura q se inicien todos false
                                
                formFormaPago frmFormaPago = new formFormaPago();
                //frmFormaPago.esCorteUnidad = 
                frmFormaPago.ShowDialog(this);

                //si ninguna forma de pago está seleccionada no se valida
                if (checkEfectivo.Checked == false && checkDebito.Checked == false && checkCredito.Checked == false)
                    return false;
            }
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
                MessageBox.Show("- \'" + oCorteE.CorteDesc +"\' no está habilitado para la venta", "Corte No Habilitado",MessageBoxButtons.OK, MessageBoxIcon.Stop);
                txtCodigo.Focus();
                return false;
            }

            //si es consumidor final no se permite precios mayorista excepto que esté logueado como admin
            int inicioCodigoMayorista = (ConfigurationManager.AppSettings["codigoPrecioMayorista"]) != null ?
                Convert.ToInt32(ConfigurationManager.AppSettings["codigoPrecioMayorista"].ToString()) : 0;
            if (oCorteE != null && oCorteE.Mayorista && !FormPrincipal.logueado && !oUsuario.Admin && 
                oCliente.idPersona.Equals(Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString())))
            {
                MessageBox.Show("No tienes permiso para realizar ventas con precio mayorista a un consumidor final.\n\n"+
                    "Busque el cliente o agréguelo para poder realizar la venta con precios mayoristas", "Precio mayorista");
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
            //valida que un venta en CTA CTE sea solo en Efectivo
            if (checkCtaCte.Checked && !oVentaE.FormaPago.ToString().Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
            {
                MessageBox.Show("Las ventas en cuenta corriente (CTA.CTE.) sólo pueden ser en EFECTIVO."+
                    "\n\nSi la venta es en CTA.CTE. seleccione la forma de pago en Efectivo."+
                    "(La forma de pago que tiene seleccionada es: "+oVentaE.FormaPago.ToString()+")", 
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
                if (!oCierreN.validarCajaAbiertaVendedor(Convert.ToDateTime(txtFecVenta.Text), oVentaE.Sucursal, oUsuario))
                {
                    MessageBox.Show(oUsuario.Nombre + " debes abrir caja para poder registrar la venta.\n\n"+
                    "Si abrió caja inténtelo nuevamente.", "No abrió caja", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    txtFecVenta.Text = DateTime.Now.ToString();//se actualiza la hora

                    return false;
                }

                string mensaje = "Complete los siguientes campos: ";
                
                //se valida que no finalice venta con bonificacion para consumidor final
                for (int index = 0; index < listaLineaVenta.Count; index++)
                {
                    if ((listaLineaVenta[index].Bonificacion != 0 || listaLineaVenta[index].Corte.Mayorista) && !FormPrincipal.logueado && !oUsuario.Admin && 
                        oCliente.idPersona.Equals(Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString())))
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
                            mensaje = "No tienes permiso para poder bonificar y/o vender productos mayoristas a un cliente Consumidor Final";
                            MessageBox.Show(mensaje, "No se puede bonificar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }  
                    }                  
                }

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
                        "    |   Cantidad:  " + oLineaVentaSelect.CantKg + 
                        "    |    Total:  $ " + oLineaVentaSelect.CantKg * oLineaVentaSelect.PrecioKg;
                    string mensaje = "¿Está seguro de anular el corte seleccionado?" + datosLinea;
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
                        oLineaVenta.IndexAnulado = nroFila;

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
            if (txtCodigo.Text != "")
            {
                try
                {
                    lblNoHabilitado.Visible = false;
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
                                oCorteE.Mayorista = Convert.ToBoolean(fila["mayorista"]);
                                oCorteE.EnCierreStock = Convert.ToBoolean(fila["enCierreStock"]);
                                oCorteE.Habilitado = Convert.ToBoolean(fila["habilitado"]);
                        }
                        //cargo los campos
                        
                        this.txtCodigo.Text = Convert.ToString(oCorteE.codigo);
                        this.txtCorte.Text = oCorteE.corte;

                        //si no está habilitado no muestra el importe
                        if (!oCorteE.Habilitado)
                        {
                            lblNoHabilitado.Visible = true;
                            return;
                        }

                        this.txtPrecioKg.Text = oVentaE.bonificar(oCliente, oCorteE.precioKg, oCorteE.Mayorista).ToString("N2");//oCorteE.precioKg.ToString("N");
                        
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
                totalesParciales(0, 0);
                //test redondeo
                txtRedondeo.Text = "";
                txtKgsRedondeo.Text = "";
            }
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

                        //"001.305"            

                        //Si la centena del decimal cambia de valor, Variar peso hasta ###.#99
                        //camparar las centenar de Decimales y/o Unidad de peso para verificar que no hay un cambio brusco.
                        bool kgNroRedondo = txtCantKgs.Text.Length > 6 && txtCantKgs.Text.Substring(4, 3).Equals("000");

                        ///REDONDAR importe SI:
                        ///*Cliente es consumidor final
                        ///*la cantidad de ganancia NO excede a $5
                        ///*el Kg no es un número redondo
                        ///
                        bool redondear = oCliente.idPersona.Equals(idConsumidorFinal) && ganPesosTotRedondeo < 5.1 && !kgNroRedondo ? true : false;

                        //cargo el Temporal de LineaVenta
                        try
                        {
                            switch (oVentaE.FormaPago)
                            {
                                case "Efectivo":
                                    kgsTotalCalculado = (cantKg * porcAjEfectivo);
                                    break;
                                case "Debito":
                                    kgsTotalCalculado = (cantKg * porcAjDebito);
                                    break;
                                case "Credito":
                                    kgsTotalCalculado = (cantKg * porcAjCredito);
                                    break;
                                default:
                                    kgsTotalCalculado = (cantKg * porcAjEfectivo);
                                    break;
                            } 
                            
                            //se crea esta variable temporal para recalular kgs en caso q el kg ajusta cambie la unidad entera
                            float tempKgsTotalCalculado = kgsTotalCalculado;

                            string[] dosPartesKgsTarj = kgsTotalCalculado.ToString().Split(',');
                            string[] dosPartesKgsBalanza = cantKg.ToString().Split(',');
                            bool esKgsRedondo = dosPartesKgsBalanza.Count().Equals(1) || dosPartesKgsBalanza[1].Equals("000")
                                || dosPartesKgsBalanza[1].Equals("00") || dosPartesKgsBalanza[1].Equals("0");

                            ///Si cambia parte entera Kilaje al ajustar, establecer decimales en ###.995
                            if ((!(dosPartesKgsTarj[0] == dosPartesKgsBalanza[0])))
                                kgsTotalCalculado = Util_Form.convertFloat(dosPartesKgsBalanza[0] + ".995", false);
                            
                            ///NO ajustar kgs por Tarjeta cuando:
                            ///*cantKg de balanza es mayor al limite estipulado
                            ///*ó Cliente contiene "Empleado" en su nombre
                            ///*ó Kg Real Balanza es un entero
                            if (cantKg > limiteKgParaAjuste || oCliente.razonSocial.Contains("mpleado") || esKgsRedondo)
                                //se setear el valor real balanza
                                kgsTotalCalculado = cantKg;
                                                                                    
                            //Setear el temporal de la linea venta
                            oTemporalLineaVenta = new Entidades.TemporalLineaVenta();
                            oTemporalLineaVenta.FechaInicioPesada = DateTime.Now;
                            oTemporalLineaVenta.Corte = oCorteE;
                            oTemporalLineaVenta.Vendedor = oUsuario;
                            oTemporalLineaVenta.Sucursal = oSucursalE;
                            oTemporalLineaVenta.CantKg = cantKg;
                            oTemporalLineaVenta.KgsTotalCalculado = kgsTotalCalculado;
                            oTemporalLineaVenta.TotalCorte = (kgsTotalCalculado * precioKg);

                            //Seteo a CERO las variables
                            ganPesosRedondeoLinea = ganKgsRedondeoLinea = 0;

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
                    totalCorte = (kgsTotalCalculado * precioKg);// (cantKg * precioKg);
                    //cargo el txt total corte
                    txtRedondeo.Text = (cantKg * precioKg).ToString("N");
                    txtTotalCorte.Text = totalCorte.ToString("N2");

                    //test redondeo
                    //totalCorteRed = (cantKgTarjeta * precioKg);
                    txtRedondeo.Text = (cantKg * precioKg).ToString("N2");
                    txtKgsRedondeo.Text = kgsTotalCalculado.ToString("N3");

                    totalesParciales(cantKg, totalCorte);
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
                int cantDigitos = precioSinDecimal.ToString().Length;
                ///obtengo el numero la unidad del importe (CDU,dd)
                ///C=Centena / D=Decena / U=Unidad / dd=decimales
                int unidadPrecio = Convert.ToInt32(char.GetNumericValue(precioSinDecimal.ToString(),
                    precioSinDecimal.ToString().Length - 1));

                //si la unidad del importe es mayor o igual a 5 pesos
                if (unidadPrecio >= 5 && unidadPrecio < 9)
                {
                    //Calculo unoa decimales Random para variar el importe
                    Random rndRedondeo = new Random();
                    float centavosRedondeo = (rndRedondeo.Next(2, 50)) ;
                    centavosRedondeo = centavosRedondeo / 100;
                    importe = (precioSinDecimal + (10 - unidadPrecio)) - centavosRedondeo;
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
            checkCtaCte.Visible = !oCliente.idPersona.Equals(Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString()));
            checkCtaCte.Checked = oCliente.CtaCte;
            linkUltimasVentasCliente.Visible = !oCliente.idPersona.Equals(Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString()));
            //Ocultar Ultimas Ventas Para Cocinas y Furlana
            if (!oUsuario.Admin && (oCliente.razonSocial.ToLower().Contains("furlana") || oCliente.razonSocial.ToLower().Contains("cocina")))
                linkUltimasVentasCliente.Visible = false;

            this.txtCliente.Text = oCliente.razonSocial;
            lblClienteConBonif.Visible = oCliente.Bonificacion.Equals(0) ? false : true;
            lblClienteConBonif.Text = lblClienteConBonif.Visible ?
                "Cliente con Bonificación (" + oCliente.Bonificacion.ToString("N2") + " %)" : "";
            this.txtCodigo.Focus();

            ////Actualizo el corte cargado 
            //cargarCorte(); ----postergado---
        }

        public void EnviarFormaPago(Entidades.Venta.formaPagoEnum formaPago)
        {
            switch (formaPago)
            {
                case Entidades.Venta.formaPagoEnum.Efectivo:
                    checkEfectivo.Checked = true;
                    break;
                case Entidades.Venta.formaPagoEnum.Debito:
                    checkDebito.Checked = true;
                    break;
                case Entidades.Venta.formaPagoEnum.Credito:
                    checkCredito.Checked = true;
                    break;
            }                    
        }

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            if (grillaLineasVenta.Rows.Count.Equals(0))
            {
                //titilarTextBoxVendedor();
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
                    if (txtCodigo.Focused)
                    {
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
                            txtCantKgs.Text = Utilidades.Util_Form.leerPesoBalanza();
                            lblErrorBalanza.Visible = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtCantKgs.Text = "Error balanza";
                lblErrorBalanza.Text = ex.Message;
                lblErrorBalanza.Visible = true;
                //timer1.Enabled = false;

                //txtCantKgs.Text = "Error balanza";
                //timer1.Enabled = false;
                //if (FormPrincipal.logueado && Utilidades.Util_Form.errorBalanza(ex.Message) == DialogResult.Yes)
                //{
                //    dejarDeLeerPeso = true;
                //    checkLeerPeso.Checked = false;
                //}
                //else
                //{
                //    lblErrorBalanza.Text = ex.Message;
                //    lblErrorBalanza.Visible = true;
                //    timer1.Enabled = true;
                //}
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void formVentaCaja_Load(object sender, EventArgs e)
        {
            this.Text += Utilidades.Conexion.getSucursalConexion();
            lblTeclasRapidas.Text = "Inicio = Codigo  |  Fin = Abonar  |  ESC = Salir  |  F2 = Pant.Principal  |   "+
                "F4 = Bonificación  |  F5 = Nueva Compra  |  F6 = Mis Egresos Caja  |  F7 = Egresos Caja  |\n  F9 = Buscar Cliente  |  " +
                "F10 = Buscar Corte  |  F11 = Observaciones  |  F12 = Bloquear  |";
            if (oUsuario != null)
            {
                validarAperturaCaja();
                //se vuelve a validar que el usuario no sea nulo(sucede cuando no quiere abrir caja)
                if (oUsuario == null) return;

                oVentaE.Vendedor = oUsuario;
                usuario.Text = oUsuario.User;
                txtVendedor.Text = oUsuario.Nombre;
                lblVendedorNombre.Text = oUsuario.Nombre;
                this.Text = oUsuario.Nombre;
                Color colorUser = System.Drawing.Color.FromName(oUsuario.ColorForm);
                this.pnlBuscar.BackColor = colorUser;
                this.grupoCortes.BackColor = colorUser;
                comboColors.Text = colorUser.ToString();
                grillaLineasVenta.DefaultCellStyle.SelectionBackColor = colorUser;
                timerBloquearCaja.Start();
                ultimaVentaVendedor();
                restablecerFormaDePago();
                comboTipoComprobante.SelectedIndex = 0;
                
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
                    //si estable nulo el Usuario porque decidió no abrir caja
                    oUsuario = null;
                    formVentaCaja_Load(null, null);
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
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
                case Keys.End:
                    if (!estaBloqueado())
                    mostrarPago();
                    break;
                case Keys.PageDown:
                    cambiarPuntoDeVenta();
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
                        sumarUltimasDosVentas();
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
                        MessageBox.Show("No se pueden bonificar cortes anulados");
                        return;
                    }
                }

                //si es consumidor final no se permite bonificacion excepto que esté logueado como admin
                if (!FormPrincipal.logueado && !oUsuario.Admin && oCliente.idPersona.Equals(Convert.ToInt32(ConfigurationManager.AppSettings["idConsumidorFinal"].ToString())))
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
                frmBonificar.frmVentaCaja = this;
                frmBonificar.ShowDialog();

                listaLineaVenta[nroFila].PrecioKg = Utilidades.Util_Form.convertFloat(precioBonificado, false);
                listaLineaVenta[nroFila].Bonificacion = (1 - (listaLineaVenta[nroFila].PrecioKg / listaLineaVenta[nroFila].PrecioReal)) * 100;
                listaLineaGrilla[nroFila].corte = listaLineaVenta[nroFila].Bonificacion == 0 ? oLineaVentaSelect.Corte.CorteDesc :
                    (oLineaVentaSelect.Corte.CorteDesc.Length < 9 ? oLineaVentaSelect.Corte.CorteDesc : oLineaVentaSelect.Corte.CorteDesc.Substring(0,9)) + " (Bonif. " + listaLineaVenta[nroFila].Bonificacion.ToString("F2") + "%)";
                listaLineaGrilla[nroFila].precioKg = Utilidades.Util_Form.convertFloat(precioBonificado, false);
                listaLineaGrilla[nroFila].totalS = listaLineaGrilla[nroFila].precioKg * listaLineaGrilla[nroFila].KgsTotalCalculado;

                cargarGrilla();

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
            grillaLineasVenta.Enabled = false;

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
                if ((oCorteE != null && oCorteE.idCorte > 0 && 
                    oCorteE.tipo.Equals("Unidad") && checkLeerPeso.Checked) || !FormPrincipal.leerBalanza)
                {
                    checkLeerPeso.Checked = false;
                    txtCantKgs.Focus();
                }
                else
                {
                    if (!dejarDeLeerPeso && oCorteE != null && oCorteE.idCorte > 0 && !oCorteE.tipo.Equals("Unidad") && !checkLeerPeso.Checked)
                    {
                        //checkLeerPeso.Checked = FormPrincipal.logueado ? 
                        //    checkLeerPeso.Checked : true;
                        //if (checkLeerPeso.Checked) btnAgregar.Focus();

                        checkLeerPeso.Checked = true;
                        btnAgregar.Focus();
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
                //se valida que no hay pasado el limite de tiempo para editar la venta
                if (!FormPrincipal.logueado && oUltimaVentaVendedor.Creado.AddMinutes(10) < DateTime.Now)
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
                if (comboColors.Items.Count < 13)
                {
                    comboColors.Items.Add(Color.SteelBlue);
                    comboColors.Items.Add(Color.DarkCyan);
                    comboColors.Items.Add(Color.DarkOrchid);
                    comboColors.Items.Add(Color.SeaGreen);
                    comboColors.Items.Add(Color.DarkCyan);
                    comboColors.Items.Add(Color.Black);
                    comboColors.Items.Add(Color.Red);
                    comboColors.Items.Add(Color.Green);
                    comboColors.Items.Add(Color.Firebrick);
                    comboColors.Items.Add(Color.Teal);
                    comboColors.Items.Add(Color.DarkSlateBlue);
                    comboColors.Items.Add(Color.DimGray);
                }

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

        private void comboColors_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                this.pnlBuscar.BackColor = (Color)comboColors.SelectedItem;
                this.grupoCortes.BackColor = (Color)comboColors.SelectedItem;
            }
            catch (Exception)
            {
            }
        }

        private void linkUltimasVentasCliente_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            formGetAllLineaVenta frmGetAllLV = new formGetAllLineaVenta();
            frmGetAllLV.verUltimasVentasClientes = true;
            frmGetAllLV.idPersona = oCliente.idPersona;
            frmGetAllLV.idSucursal = oSucursalE.idSucursal;
            frmGetAllLV.ShowDialog();
        }

        private void txtCantKgs_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void checkBoxRedondeo_CheckedChanged(object sender, EventArgs e)
        {
            checkBoxRedondeo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkBoxRedondeo.Checked);
        }

        private void restablecerFormaDePago()
        {
            oVentaE.FormaPago = null;

            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(false);
        }

        private void setFormaDePago()
        {
            restablecerFormaDePago();
            checkEfectivo.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkEfectivo.Checked);
            checkDebito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkDebito.Checked);
            checkCredito.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkCredito.Checked);
        }

        private void checkEfectivo_CheckedChanged(object sender, EventArgs e)
        {            
            setFormaDePago();
            if (checkEfectivo.Checked)
            {
                checkDebito.Checked = checkCredito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Efectivo.ToString();
            }
        }

        private void checkDebito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkDebito.Checked)
            {
                checkEfectivo.Checked = checkCredito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Debito.ToString();
            }
        }

        private void checkCredito_CheckedChanged(object sender, EventArgs e)
        {
            setFormaDePago();

            if (checkCredito.Checked)
            {
                checkEfectivo.Checked = checkDebito.Checked = false;
                oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Credito.ToString();
            }
        }

        private void comboTipoComprobante_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTipoComprobante.SelectedItem.ToString().Equals(Entidades.Venta.tipoComprobanteEnum.X.ToString()))
            {
                txtCuit.ReadOnly = txtEmail.ReadOnly = true;
                txtCodigo.Focus();
            }
            else
            {
                txtCuit.ReadOnly = txtEmail.ReadOnly = false;
                txtCuit.Focus();
            }
        }

    }
}
