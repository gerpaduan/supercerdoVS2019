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
using Presentacion.Ticket;

namespace Presentacion.Caja
{
    public partial class formPuntoExpendio : Form, InterfaceCorte, InterfaceUsuario, InterfaceSector
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
        public Entidades.Venta oVentaE = new Entidades.Venta();
        Entidades.LineaVenta oLineaVenta;
        Entidades.StockCorteSucursal oStockCorteSucursal;
        Entidades.Venta oUltimaVentaVendedor;
        Entidades.TemporalLineaVenta oTemporalLineaVenta = new Entidades.TemporalLineaVenta();

        public List<Entidades.LineaVenta> listaLineaVenta = new List<Entidades.LineaVenta>();
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

        //float balanza
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
        float totalCorte, precioKg, cantKg, cantKgTarjeta, kgsTotalCalculado;
        float totalVenta = 0, abona = 0, cambio = 0, ganPesosTotRedondeo = 0, ganKgsTotRedondeo = 0,
            ganPesosRedondeoLinea = 0, ganKgsRedondeoLinea = 0, acumRedondeoKgs = 0, acumRedondeImporte = 0;

        float porcAjEfectivo, porcAjDebito, porcAjCredito, porcAjCtaCte, porcAjQr, porcAjTranf, limiteKgParaAjuste;
        bool esAjustePorcTarj = false;
        int idConsumidorFinal;

        /// <summary>
        /// Variable Para manejar los codigos de barra internos
        /// </summary>

        bool esCodBarraInterno , esCodBarraEstandar = false;
        string codigoEnCodBarra = "", segundoModulo = "";
        public string sector = "";
        
        #endregion


        public formPuntoExpendio()
        {
            InitializeComponent(); this.Icon = Properties.Resources.CarniSys_ICONO;

            timer1.Interval = Convert.ToInt32(ConfigurationManager.AppSettings["timerForm"].ToString());
            this.KeyPreview = true;

            //Se obtienen los parametros
            Negocio.OtrasClases oOtrasClasesN = new Negocio.OtrasClases();
            oOtrasClasesN.obtenerParametros();

            //asigo sucursal a la venta  
            int idSucursal = Convert.ToInt32(Utilidades.Conexion.getIdSucursalConexion());
            oSucursalE = oSucursalN.findById(idSucursal);
            oVentaE.Sucursal = oSucursalE;
            this.txtSucursal.Text = oVentaE.Sucursal.sucursal;
            Negocio.Persona oPersonaN = new Negocio.Persona();
            idConsumidorFinal = Entidades.Parametros.idConsumidorFinal;
            oCliente = oPersonaN.findById(idConsumidorFinal);
            txtFecVenta.Text = DateTime.Now.ToString();
            if (!fecha.Equals(""))
            {
                txtFecVenta.Text = DateTime.Parse(fecha).ToString();
            }
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
                    oVentaE.IdVenta = oVentaN.agregarExpendio(oVentaE);

                    cargarLineasExpendio_Imprimir(true, checkTicket.Checked);

                    oVentaE.IdVenta = 0;
                    limpiarListas();
                    //si es ventada duplicada se cierra la misma
                    if (ventanaDuplicada)
                    {
                        this.Close();
                        return;
                    }
                    txtCodigo.Focus();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void cargarLineasExpendio_Imprimir(bool agregarLineasEnDB, bool imprimir)
        {
            Ticket.CreaTicket ticket = new Ticket.CreaTicket();
            //imprimir si está checked
            ticket.imprimir = imprimir;
            ticket.TextoCentro(oVentaE.Sector);
            ticket.LineasEnBlanco(1);
            //ticket.TextoIzquierda("123456789*123456789*123456789*123456789*123456789*");
            ticket.TextoIzquierda("Nro Expendio: " + oVentaE.IdExpendio);
            ticket.TextoIzquierda("Id.Cliente: " + oVentaE.IdentificacionExpendio);
            ticket.TextoExtremos("Fecha: " + oVentaE.FechaVenta.Date.ToString(), "Hora: " + oVentaE.FechaVenta.TimeOfDay.ToString());
            //ticket.LineasEnBlanco(0);
            ticket.LineasGuion();

            for (int index = 0; index < listaLineaVenta.Count; index++)
            {
                Entidades.LineaVenta linea = listaLineaVenta[index];

                if (agregarLineasEnDB)
                {
                    //setear por cada linea cantKg <- KgsTotalCalculado
                    linea.CantKg = linea.KgsTotalCalculado;

                    //si está anulada la linea se asigna el IdLineaVenta del corte anulado
                    linea.IndexAnulado = Entidades.LineaVenta.esAnulado(linea.Estado) ? listaLineaVenta[linea.IndexAnulado].IdLineaVenta :
                        Entidades.LineaVenta.getIdEstado(Entidades.LineaVenta.estados.NoAnulado);

                    listaLineaVenta[index] = oVentaN.agregarLineaExprendio(linea);
                }

                ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                    linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);

                totalVenta += linea.PrecioKg * linea.CantKg;
            }

            ticket.TextoDerecha("-------");
            ticket.AgregaTotales("Total", totalVenta);
            ticket.LineasEnBlanco(1);
            ticket.TextoIzquierda("Articulos: " + listaLineaVenta.Count.ToString());
            ticket.TextoIzquierda("Cajero: " + (oUsuario != null && oUsuario.Id > 0 ? oUsuario.Id.ToString() : oVentaE.Vendedor.Id.ToString()));
            ticket.GraciasPorSuCompra();
            ticket.LineasEnBlanco(2);
            ticket.realizarImpresion();
        }

        private void limpiarListas()
        {
            Negocio.Persona oPersonaN = new Negocio.Persona();
            txtFecVenta.Text = DateTime.Now.ToString();
            txtCliente.Text = "";
            lblCantItems.Text = "0";
            lblCantKgs.Text = "0,000";
            lblTotalS.Text = "000,00";

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
            oVentaE.FechaVenta = Convert.ToDateTime(txtFecVenta.Text).AddDays(0);
            oVentaE.Turno = "";
            oVentaE.DiaFestivo = "";
            oVentaE.TotalImporte = totalVenta;
            oVentaE.AcumRedondeoImporte = ganPesosTotRedondeo;
            oVentaE.AcumRedondeoKgs = ganKgsTotRedondeo;
            oVentaE.LineasVenta = listaLineaVenta;
            oVentaE.FormaPago = Entidades.Venta.formaPagoEnum.Nulo.ToString();
            oVentaE.TipoComprobante = Convert.ToChar(Entidades.Venta.tipoComprobanteEnum.X.ToString());

            oVentaE.IdentificacionExpendio = txtCliente.Text;
            oVentaE.Sector = lblPuntoExpendio.Text;
            oVentaE.CantItems = lblCantItems.Text;
            oVentaE.TotalImporte = totalVenta;
            oVentaE.Observaciones = "";
            oVentaE.NroRemito = "";
            oVentaE.SerialCPU = Utilidades.Util_Form.GetCPUId();
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

            lblCantItems.Text = grillaLineasVenta.Rows.Count.ToString();
            lblCantKgs.Text = totalKgs.ToString("N3");
            lblTotalS.Text = totalPesos.ToString("N2");
            totalVenta = float.Parse(lblTotalS.Text.Trim());
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

        private bool validarLinea()
        {
            //Se valida que no sea media res
            if (oCorteE != null && !oCorteE.Habilitado)
            {
                MessageBox.Show("- \'" + oCorteE.CorteDesc +"\' no está habilitado para la venta", "Corte No Habilitado",MessageBoxButtons.OK, MessageBoxIcon.Stop);
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
                bool esKgsMayorUnGramo = Utilidades.Util_Form.validarNumeroMayorUnGramo(txtCantKgs.Text, "Kgs.");
                bool esPrecioMayorACero =  Utilidades.Util_Form.validarNumeroMayorACero(txtPrecioKg.Text, "Precio");
                if (!esKgsMayorUnGramo && !checkLeerPeso.Checked) //(!esKgsMayorACero && !checkLeerPeso.Checked)
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

                return esKgsMayorUnGramo && esPrecioMayorACero && esKgsMenorAMil;
            }
        }

        private bool validacionFinal()
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
                txtCliente.Focus();
                txtCliente.SelectAll();
                return false;
            }

            return true;
        }

        private void quitarLinea()
        {
            try
            {
                if (grillaLineasVenta.SelectedRows.Count > 0 || grillaLineasVenta.CurrentRow != null)
                {
                    int nroFila = grillaLineasVenta.Rows.GetFirstRow(DataGridViewElementStates.Selected);//obtiene nro de fila de la grilla
                    listaLineaVenta.RemoveAt(nroFila);//elimina objetos de las listas
                    listaLineaGrilla.RemoveAt(nroFila);
                    cargarGrilla();
                }
                else
                {
                    MessageBox.Show("No hay ninguna fila seleccionada.", "Seleccione un fila", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

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

                        this.txtPrecioKg.Text = oCorteE.precioKg.ToString("N2");

                        cargarTotalCorte();
                    }
                    else
                    {
                        oCorteE = null;
                        this.txtTotalCorte.Text = "";
                        this.txtPrecioKg.Text = "";
                        this.txtCorte.Text = "";
                        totalesParciales(0, 0);
                    }
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Valor demasiado grande o demasiado pequeño para Int32."))
                        return;
                    
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
      
                        }
                        catch (Exception)
                        {
                        }
                    }
                    totalCorte = (kgsTotalCalculado * precioKg); //(cantKg * precioKg);//
                    //cargo el txt total corte
                    txtTotalCorte.Text = totalCorte.ToString("N2");


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

        private void totalesParciales(float kgsCorte, float totalCorte)
        {
            cargarTotales();

            if (totalCorte > 0)
            {
                try
                {
                    lblCantKgs.Text = (Utilidades.Util_Form.convertFloat(lblCantKgs.Text, false) + kgsCorte).ToString("N3");
                    lblTotalS.Text = "$" + (Utilidades.Util_Form.convertFloat(lblTotalS.Text, false) + totalCorte).ToString("N2");
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

                    txtTotalCorte.Text = totalCorte.ToString();                   

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

                if (oUsuario == null || string.IsNullOrEmpty(sector)) return false;

                DialogResult respuesta;
                respuesta = MessageBox.Show("¿Cerrar la ventana de Punto de Expendio de "+oUsuario.Nombre+"?.", "Cerrar ventana", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

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

        private void txtCodigo_TextChanged(object sender, EventArgs e)
        {
            if (grillaLineasVenta.Rows.Count.Equals(0))
            {
                //al iniciar la venta se actualizan los cortes cargándose el dtCortes
                if (txtCodigo.Text.Length == 1)
                    dtCortes = oCorteN.cargarDtCortes();
            }
            cargarCorte();

            if ((txtCodigo.Text.Length == 8 && esDigitoControlCorrectoEAN8(false)) ||
               txtCodigo.Text.Length == 13 && esDigitoControlCorrectoEAN13(false))
            {
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
                ///si el CodigoBarra es de un producto pesable, Se lee el codigo, y se lee balanza. El usuario deberá agregar manualmente
                ///
                    if (oCorteE.Pesable)
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
                lblErrorBalanza.Text = ex.Message;
                lblErrorBalanza.Visible = true;
                txtCodigo.Focus();
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

        private void formPuntoExpendio_Load(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            checkLeerPeso.Checked = FormPrincipal.leerBalanza;
            checkLeerPeso.Enabled = FormPrincipal.leerBalanza;
            checkTicket.Checked = Convert.ToBoolean(ConfigurationManager.AppSettings["ticketExpendio"].ToString());
            this.Text += Utilidades.Conexion.getSucursalConexion();
            lblTeclasRapidas.Text = "Inicio = Codigo  |  Fin = Abonar  |  ESC = Salir  |  F2 = Pant.Principal  |   " +
                "" +
                "F10 = Buscar Corte  |  AvPág = Cambiar Vendedor";
            if (oUsuario != null)
            {
                //se vuelve a validar que el usuario no sea nulo(sucede cuando no quiere abrir caja)
                if (oUsuario == null) return;

                //se pide que seleccione el Sector
                if (!ventanaDuplicada)
                {
                    Presentacion.Caja.FormSelectPuntoExpendio frmSelectPuntoExpendio = new Presentacion.Caja.FormSelectPuntoExpendio();
                    frmSelectPuntoExpendio.ShowDialog(this);

                    if (string.IsNullOrEmpty(sector))
                    {
                        this.Close();
                        return;
                    }

                }
                lblPuntoExpendio.Text = sector;
                oVentaE.Vendedor = oUsuario;
                oVentaE.ImprimirTipoCbte = Entidades.Venta.imprimirCbteEnum.Nulo.ToString();
                usuario.Text = oUsuario.User;
                txtVendedor.Text = oUsuario.Nombre;
                this.Text += " | "+oUsuario.Nombre;

                if (sector.Equals("PRESUPUESTO"))
                {
                    txtPrecioKg.Enabled = true;
                    txtPrecioKg.ReadOnly = false;
                    txtPrecioKg.TabStop = true;
                }

                //Color colorUser = string.IsNullOrEmpty(oUsuario.ColorForm) ?
                //    System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(77)))), ((int)(((byte)(129))))) : System.Drawing.Color.FromName(oUsuario.ColorForm);
                //this.pnlBuscar.BackColor = colorUser;
                //this.grupoCortes.BackColor = colorUser;
                //comboColors.Text = colorUser.ToString();
                //grillaLineasVenta.DefaultCellStyle.SelectionBackColor = colorUser;
                dtCortes = oCorteN.cargarDtCortes();

                timer1.Enabled = true;
            }
            else
            {
                this.Close();
            }
        }

        public void EnviarSector(string sector)
        {
            this.sector = sector;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Multiply:
                    dejarDeLeerPeso = checkLeerPeso.Checked;
                    checkLeerPeso.Checked = FormPrincipal.leerBalanza ? !checkLeerPeso.Checked : checkLeerPeso.Checked;
                    break;
                case Keys.Home:
                    txtCodigo.Focus();
                    break;
                case Keys.PageUp:
                    txtCodigo.Focus();
                    break;
                case Keys.End:
                    //si el campo cliente está vacio se hace foco
                    if (string.IsNullOrEmpty(txtCliente.Text))
                    {
                        txtCliente.Focus();
                        txtCliente.Select();
                    }
                    else
                    {
                        btnAceptar.Focus();
                    }
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
                        //sumarUltimasDosVentas();
                        break;
                case Keys.F10:
                    buscarCorte();
                    break;
                case Keys.F12:
                    //bloquear();
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

        private void cambiarPuntoDeVenta()
        {
            bool cambioForm = false;
            foreach (Form frm in Application.OpenForms)
            {
                if (frm.GetType() == typeof(formPuntoExpendio))
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

        private void txtCliente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)(Keys.Enter))
            {
                btnAceptar.Focus();
                btnAceptar.Select();
                ////20241013 - si hace foco y es vacío se manda un return para evitar el tab
                //if (txtCodigo.Focused && String.IsNullOrEmpty(txtCodigo.Text))
                //{
                //    return;
                //    ///Al ingrtesar el primer corte, luego de ingresar el codigo aparecera el cartel de forma pago
                //    ///la idea es q NO muestre el total del corte sin antes poner la forma pago
                //    //Solicitar forma de pago si balanza es distinta a nulo o cero                        

                //    //bool resp = !ingresarFormaPago() ? true: false;


                //}
                //e.Handled = true;
                //SendKeys.Send("{TAB}");
            }
        }

        private void txtCliente_Enter(object sender, EventArgs e)
        {
            txtCliente.BackColor = focusColor;
        }

        private void txtCliente_Leave(object sender, EventArgs e)
        {
            txtCliente.BackColor = enableColor;
        }

        private void txtPrecioKg_Enter(object sender, EventArgs e)
        {
            txtPrecioKg.BackColor = focusColor;
        }

        private void txtPrecioKg_Leave(object sender, EventArgs e)
        {
            txtPrecioKg.BackColor = enableColor;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Utilidades.GenerarCodigoBarra codBarra = new GenerarCodigoBarra();
            codBarra.Main();
        }

        private void formPuntoExpendio_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
        }

        private void btnBloquear_Click(object sender, EventArgs e)
        {
            bloquear();
        }

        private void bloquear()
        {
            btnAceptar.Enabled = false;
            grupoCortes.Enabled = false;
            pnlBuscar.Enabled = false;
            grillaLineasVenta.Enabled = false;
        }

        private void btnIngresar_Click(object sender, EventArgs e)
        {
        }


        private void btnAceptar_Enter(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromName("LimeGreen");
        }

        private void btnAceptar_Leave(object sender, EventArgs e)
        {
            btnAceptar.BackColor = Color.FromName("HotTrack");
        }


        private void txtCodigo_Enter(object sender, EventArgs e)
        {
            this.txtCodigo.BackColor = focusColor;
        }

        private void txtCodigo_Leave(object sender, EventArgs e)
        {
            try
            {
                //Se valida que precio lista sea distinto a cero para evitar error en bonificar al infinito
                if (oCorteE != null && oCorteE.precioKg == 0)
                {
                    MessageBox.Show("No se permiten ingresar productos con precio de lista $0.00 (cero)", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtCodigo.Focus();
                    return;
                }
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
                ///se borra codigo viejo
                //if ((oCorteE != null && oCorteE.idCorte > 0 && 
                //    oCorteE.tipo.Equals("Unidad") && checkLeerPeso.Checked) || !FormPrincipal.leerBalanza)

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
                        if (frm.GetType() == typeof(formPuntoExpendio))
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
            this.btnAgregar.UseVisualStyleBackColor = false;
            this.btnAgregar.BackColor = focusColor;
        }

        private void btnAgregar_Leave(object sender, EventArgs e)
        {
            this.btnAgregar.UseVisualStyleBackColor = true;
        }

        private void checkTicket_CheckedChanged(object sender, EventArgs e)
        {
            checkTicket.BackColor = Utilidades.Util_Form.getBackColorCheckBox(checkTicket.Checked);
            txtCodigo.Focus();
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

        private void checkLeerPeso_Enter(object sender, EventArgs e)
        {
            capturarPantalla();
            capturarPantallaFinal = true;
        }

        private void capturarPantalla()
        {
            Utilidades.Util_Form.capturarPantalla(txtVendedor.Text, DateTime.Now);
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
            if (Application.OpenForms["formGetAllLineaExpendio"] != null)
            {
                Application.OpenForms["formGetAllLineaExpendio"].Activate();
                Application.OpenForms["formGetAllLineaExpendio"].WindowState = FormWindowState.Normal;
            }
            else
            {
                formGetAllLineaExpendio frmGetAllLV = new formGetAllLineaExpendio();
                frmGetAllLV.oUsuarioE = oUsuario;
                frmGetAllLV.idPersona = oCliente.idPersona;
                frmGetAllLV.idSucursal = oSucursalE.idSucursal;
                frmGetAllLV.Show();
            }
        }


        private void duplicarVentana_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            formPuntoExpendio frmVentaCajaDuplicada = new formPuntoExpendio();
            frmVentaCajaDuplicada.oUsuario = oUsuario;
            frmVentaCajaDuplicada.sector = sector;
            frmVentaCajaDuplicada.ventanaDuplicada = true;
            frmVentaCajaDuplicada.duplicarVentana.Visible = false;
            frmVentaCajaDuplicada.Show();
        }
    }
}
