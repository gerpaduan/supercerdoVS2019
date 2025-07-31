using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using wsAFIPvs2008.WSFEHOMO;
using wsAFIPvs2008.WSPSA13;
using System.Configuration;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Utilidades;
using Font = System.Drawing.Font;
using iTextSharp.text.pdf.draw;
using QRCoder;
using Newtonsoft.Json;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Diagnostics;
using System.Windows.Documents;
using System.Threading;

namespace wsAFIPvs2008
{
    public partial class formFacturaElectronica : Form
    {
        public int idVenta = 0;
        public int idFactuElec = 0;
        public int idNotaCredito = 0;
        public bool esShowDialog = false;
        public bool notaCredito = false;
        Entidades.Venta oVentaE; 
        Negocio.Venta oVentaN = new Negocio.Venta();
        Negocio.Persona oPersonaN = new Negocio.Persona();
        Entidades.FacturaElectronica oFactuElec;
        Entidades.FacturaElectronica oNotaCredito;
        Entidades.Sucursal oSucursalEntidad = new Entidades.Sucursal();
        Entidades.Persona personaPadron = new Entidades.Persona();
        List<Entidades.AlicuotaIva> listaAlicuotasFactura = new List<Entidades.AlicuotaIva>();
        List<Entidades.LineaVenta> lineaNuevosAnulados = new List<Entidades.LineaVenta>();
        List<Entidades.lineaVentaUnificada> listaLineaGrilla = new List<Entidades.lineaVentaUnificada>();
        DataTable dtIva;
        public bool logueado;
        public bool facturaPendiente = true;
        public bool cargarDatosAfip;
        public string razonSocialAfip;
        public string domicilioFiscalAfip;
        public string localidadAfip;
        public string provinciaAfip;
        bool mostrarSeleccionados;
        bool generarPDF = false;
        //variable porque que formPOS tome el foco al cerrar el form
        private readonly Action onCloseCallback;

        private LoginClass oLoginClass;
        private string urlLogin;
        private string urlWSFE;
        bool esRRII = ConfigurationManager.AppSettings["ivaCliente"].ToString().Equals("RRII");
        string cuit = ConfigurationManager.AppSettings["cuit"].ToString();
        string certificado = Directory.GetCurrentDirectory() + ConfigurationManager.AppSettings["rutaCertificado"].ToString();
        string servidor_0test_1prod = ConfigurationManager.AppSettings["tipoServidor"].ToString();
        string qrTicketFactura = ConfigurationManager.AppSettings["qrTicketFactura"].ToString();
        string servicioAfip = "wsfe";
        string clave = "";
        string idIvaAliAfip = ConfigurationManager.AppSettings["idIvaAliAfip"].ToString();
        int ptoVtaAfip = Convert.ToInt32(ConfigurationManager.AppSettings["ptoVtaAfip"].ToString());
        float iva_a_multiplicar;
        float importeTotal, importeNeto, importeIva = 0;
        List<int> listaIdAlicuotaConIva = new List<int>();

        //Login para Datos Persona
        private LoginClass_Person oLoginClassPerson;
        private string urlLoginPerson;
        private string urlWSPN;
        string servicioAfipPerson = "ws_sr_padron_a13";
        string errores = "";

        LoginClass Login_;
        public LoginClass Login
        {
          get { return Login_; }
            set { Login_ = value; }
        }

        LoginClass_Person LoginPerson_;
        public LoginClass_Person LoginPerson
        {
            get { return LoginPerson_; }
            set { LoginPerson_ = value; }
        }

        CbteTipoResponse TiposComprobantes_;

        public CbteTipoResponse TiposComprobantes
        {
            get { return TiposComprobantes_; }
            set { TiposComprobantes_ = value; }
        }
        ConceptoTipoResponse TipoConceptos_;

        public ConceptoTipoResponse TipoConceptos
        {
            get { return TipoConceptos_; }
            set { TipoConceptos_ = value; }
        }
        DocTipoResponse TipoDoc_;

        public DocTipoResponse TipoDoc
        {
            get { return TipoDoc_; }
            set { TipoDoc_ = value; }
        }
        MonedaResponse Monedas_;

        public MonedaResponse Monedas
        {
            get { return Monedas_; }
            set { Monedas_ = value; }
        }
        FEPtoVentaResponse puntosventa_;

        public FEPtoVentaResponse puntosventa
        {
            get { return puntosventa_; }
            set { puntosventa_ = value; }
        }
        IvaTipoResponse TiposIVA_;

        public IvaTipoResponse TiposIVA
        {
            get { return TiposIVA_; }
            set { TiposIVA_ = value; }
        }
        OpcionalTipoResponse opcionales_;

        public OpcionalTipoResponse opcionales
        {
            get { return opcionales_; }
            set { opcionales_ = value; }
        }
        FEAuthRequest authRequest_;

        public FEAuthRequest authRequest
        {
            get { return authRequest_; }
            set { authRequest_ = value; }
        } 

        /// <summary>
        /// *** Pasar idVenta para obtener el objeto ***
        /// </summary>
        public formFacturaElectronica(Action onClose = null)
        {
            InitializeComponent();
            onCloseCallback = onClose;
        }

        private void formFacturaElectronica_Load(object sender, EventArgs e)
        {
            loadForm();    
        }

        public void loadForm()
        {
            cargarDatosAfip = true;

            if (servidor_0test_1prod == "1")
            {
                lblServidor.Text = "Produccion";
                urlLogin = "https://wsaa.afip.gov.ar/ws/services/LoginCms?wsdl";
                urlWSFE = "https://servicios1.afip.gov.ar/wsfev1/service.asmx?WSDL";
                urlWSPN = "https://aws.afip.gov.ar/sr-padron/webservices/personaServiceA13?WSDL";
                //urlWSPN = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13?WSDL";

            }
            else
            {
                lblServidor.Text = "Testing";
                urlLogin = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
                urlWSFE = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?WSDL";
                urlWSPN = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13?WSDL";
            }

            login();

            MyCuitTX.Text = cuit;
            txtPuntoVentaConfig.Text = ptoVtaAfip.ToString();
            ptos_venta_cm.DisplayMember = "Nro";
            TiposComprobantesCMB.DisplayMember = "Desc";
            TiposComprobantesCMB.ValueMember = "Id";
            TipoConcepto.DisplayMember = "Desc";
            TipoConcepto.ValueMember = "Id";
            TipoDocCMB.DisplayMember = "Desc";
            TipoDocCMB.ValueMember = "Id";
            MonedaCMB.DisplayMember = "Desc";
            MonedaCMB.ValueMember = "Id";
            TipoIVACmb.DisplayMember = "Desc";
            TipoIVACmb.ValueMember = "Id"; 

            cargarIva();
            cargarVenta();
            RegistrarBtn.Focus();
            RegistrarBtn.Select();
        }

        public void cargarVenta()
        {
            txtIdVenta.Text = idVenta.ToString();
            oVentaE = !string.IsNullOrEmpty(txtIdVenta.Text) ? oVentaN.getVentaById(Convert.ToInt32(txtIdVenta.Text)) : null;

            label_Sucursal.Text = oSucursalEntidad.getNomSucPorPtoVtaAfip(ptoVtaAfip);
            facturaPendiente = true;
            FechaDTP.Enabled = logueado;
            checkTodosDatos.Enabled = logueado;
            CargaBtn.Enabled = logueado;
            mostrarSeleccionados = !checkTodosDatos.Checked;
            txtFormaPago.Text = oVentaE.FormaPago + (oVentaE.PagoMixtoEfectivo > 0 ? " | Efectivo" : "");
            TotalTx.ReadOnly = mostrarSeleccionados;
            TotalTx.Text = "";
            //Si el Obj Venta es nulo se cargaDatosAfip y sale del metodo
            if (oVentaE == null || oVentaE.IdVenta == 0)
            {
                cargaDatosAfipRecibidos();
                return;
            }

            idFactuElec = oVentaN.esVentaSinFacturar(oVentaE.IdVenta, false);
            oFactuElec = idFactuElec > 0 ? oVentaN.getFactuElecById(idFactuElec) : new Entidades.FacturaElectronica();
            ///Si la Venta ya fue facturada, se bloquean y habilitan los campos y componentes que no pueden ser modificados
            TiposComprobantesCMB.Enabled = idFactuElec == 0;
            TipoDocCMB.Enabled = idFactuElec == 0;
            DocTX.ReadOnly = idFactuElec != 0;
            imprimir.Enabled = idFactuElec != 0;
            pdf_Factura.Enabled = idFactuElec != 0;

            label_Sucursal.Text = oVentaE.Sucursal.sucursal;
            if (cargarDatosAfip)
                cargaDatosAfipRecibidos();
            else
                inicializaciones(mostrarSeleccionados);

            cambiarIngresoImporte();
            cargarGrilla(); //cambio de lugar - antes estaba arriba de if(cargarDatosAfip)

            FechaDTP.Value = oVentaE.FechaVenta;
            txtRazonSocial.Text = oVentaE.Persona.razonSocial;
            DocTX.Text = oVentaE.Persona.Cuit.Replace("-", "");
            txtDomicilio.Text = oVentaE.Persona.Domicilio + " - " + oVentaE.Persona.Ciudad;
            comboIva.SelectedValue = oVentaE.Persona.IdIva;
            TotalTx.Text = oVentaE.TotalImporte.ToString("N2");
            TotalTx.ForeColor = Color.DarkRed;
            NroCbteTX.Text = oFactuElec.NroCbteAfip != null ? oFactuElec.NroCbteAfip : NroCbteTX.Text;
            txtCAE.Text = oFactuElec.CAE1;
            txtVTO.Text = oFactuElec.FecVtoCAE;
            txtObservaciones.Text = oVentaE.Observaciones;
            calcularImportes();

            //Si nota credito
            if (notaCredito)
                notaCreditoSeteo();
            RegistrarBtn.Focus();
        }

        private void notaCreditoSeteo()
        {
            panelNotaCredito.Visible = true;
            txtNroFacturaNotaCredito.Text = oFactuElec.DescTipoCbteAfip.ToString() + " " + oFactuElec.PtoVtaAfip.ToString() + "-" + oFactuElec.NroCbteAfip.ToString();
            idNotaCredito = oVentaN.esVentaSinFacturar(oVentaE.IdVenta, true);
            oNotaCredito = idNotaCredito > 0 ? oVentaN.getFactuElecById(idNotaCredito) : new Entidades.FacturaElectronica();
            NroCbteTX.Text = oNotaCredito.NroCbteAfip;
            txtFormaPago.Text = "Otro";
            comboIva.Enabled = false;
            btnBuscarCuit.Visible = false;
            //3: Nota de Crédito A
            //8: Nota de Crédito B
            ////13: Nota de Crédito C
            //switch (switch_on)
            //{
            //    default:
            //}
            //TiposComprobantesCMB.SelectedIndex = oFactuElec.esFacturaA(oFactuElec.CodTipoCbteAfip.ToString()) ?;
        }
        private void login()
        {
            try
            {
                oLoginClass = new LoginClass(servicioAfip, urlLogin, certificado, clave);
                oLoginClassPerson = new LoginClass_Person(servicioAfipPerson, urlLogin, certificado, clave);
                string responseTA = oLoginClass.hacerLogin();
                string respontaTA1 = oLoginClassPerson.hacerLogin();
            }
            catch (Exception ex)
            {
                Resultado.Text = ex.Message;
                throw;
            }
        }

        private void cargarIva()
        {
            dtIva = new DataTable();
            oPersonaN = new Negocio.Persona();
            dtIva = oPersonaN.getIva();
            comboIva.DataSource = dtIva;
            comboIva.DisplayMember = "iva";
            comboIva.ValueMember = "id";
            //comboIva.SelectedValue = 1;
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
                        foreach (Entidades.LineaVenta linea in oVentaE.LineasVenta)
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
               // cargarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void cargarListaGrilla()
        {
            Entidades.lineaVentaUnificada lineaVentaP;
            listaLineaGrilla.Clear();
            listaIdAlicuotaConIva.Clear();
            importeTotal = importeNeto = importeIva = 0;

            foreach (Entidades.LineaVenta lineaE in oVentaE.LineasVenta)
            {
                lineaVentaP = new Entidades.lineaVentaUnificada();
                lineaVentaP.IdLineaVenta = lineaE.IdLineaVenta;
                lineaVentaP.idCorte = lineaE.Corte.idCorte;
                lineaVentaP.codigo = lineaE.Corte.codigo;
                lineaVentaP.corte = lineaE.Corte.corte;
                lineaVentaP.cantKgs = lineaE.CantKg;
                lineaVentaP.precioKg = lineaE.PrecioKg;
                lineaVentaP.IdAlicuotaIva = lineaE.IdAlicuotaIva;
                lineaVentaP.AlicuotaIva = lineaE.AlicuotaIva;
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

                //calculo de las Base Imponible de Alicuotas
                for (int i = 0; i < listaAlicuotasFactura.Count; i++) 
                {
                    if (listaAlicuotasFactura[i].IdIva == lineaE.IdAlicuotaIva)
                    {
                        float divisorIva = 1 + (listaAlicuotasFactura[i].Iva / 100);
                        float baseImponibleLinea = lineaVentaP.totalS / divisorIva;
                        importeTotal += lineaVentaP.totalS;
                        importeNeto += baseImponibleLinea;
                        importeIva += lineaVentaP.totalS - baseImponibleLinea;
                        listaAlicuotasFactura[i].BaseImponible += (float)Math.Round(baseImponibleLinea, 2);
                        listaAlicuotasFactura[i].Importe += (float)Math.Round((lineaVentaP.totalS - baseImponibleLinea),2);

                    }
                }

                listaLineaGrilla.Add(lineaVentaP);
                lineaVentaP = null;
            }

            //se cargan los textBox de Alicuotas
            foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
            {
                ///< !--Alicuotas IVA->ID 3 = 0 % | ID 4 = 10.5 % | ID 5 = 21 % | ID 6 = 27 % | ID 8 = 5 % | ID 9 = 2.5 % -->

                //se carga la alicuota si es mayor a cero
                if (item.Importe > 0)
                    listaIdAlicuotaConIva.Add(item.IdIva);

                switch (item.IdIva)
                {
                    case 3:
                        txtIva0.Text = item.Importe.ToString("N2");
                        break;
                    case 4:
                        txtIva10_5.Text = item.Importe.ToString("N2");
                        break;
                    case 5:
                        txtIva21.Text = item.Importe.ToString("N2");
                        break;
                    case 6:
                        txtIva27.Text = item.Importe.ToString("N2");
                        break;
                    case 8:
                        txtIva5.Text = item.Importe.ToString("N2");
                        break;
                    case 9:
                        txtIva2_5.Text = item.Importe.ToString("N2");
                        break;
                }
            }
        }

        private void CargaBtn_Click(object sender, EventArgs e)
        {
            reCargarDatosAfip();
        }

        private void reCargarDatosAfip()
        {
            cargarDatosAfip = true;
            cargarVenta();
        }

        private void cargaDatosAfipRecibidos()
        {
            try
            {
                mostrarSeleccionados = !checkTodosDatos.Checked;
              
                authRequest = new FEAuthRequest();
                authRequest.Cuit = long.Parse(cuit);
                authRequest.Sign = oLoginClass.Sign;
                authRequest.Token = oLoginClass.Token;

                WSFEHOMO.Service service = new WSFEHOMO.Service();
                service.Url = urlWSFE;
                service.ClientCertificates.Add(oLoginClass.certificado);

                puntosventa = service.FEParamGetPtosVenta(authRequest);
                ptos_venta_cm.DataSource = puntosventa.ResultGet;
                iniciarPtosVenta(mostrarSeleccionados);
                errores += puntosventa.Errors != null ? "\nAl cargar combo en puntosventa: " + puntosventa.Errors[0].Msg.ToString() : "";
                #region codigo para mostrar puntos de venta recuperados desde afip
                //Resultado.Text = "Puntos de Ventas: ";
                //if (puntosventa.ResultGet != null)
                //{
                //    foreach (PtoVenta ptoVenta in puntosventa.ResultGet)
                //    {
                //        Resultado.Text += "Puntos de Ventas: Nro: " + ptoVenta.Nro.ToString() + " EmisionTipo:" + ptoVenta.EmisionTipo
                //            + " " + ptoVenta.EmisionTipo + " " + ptoVenta.FchBaja + " " + ptoVenta.Bloqueado + "\n\n";

                //    }
                //}
                //if (puntosventa.Errors != null)
                //{
                //    foreach (Err error in puntosventa.Errors)
                //    {
                //        Resultado.Text += "\n\nErrores: " + error.Code + " - " + error.Msg;
                //    }
                //}
                #endregion

                #region Tipos Comprobantes Carga Combo
                TiposComprobantes = service.FEParamGetTiposCbte(authRequest);
                TiposComprobantesCMB.DataSource = TiposComprobantes.ResultGet;
                iniciarTipoCbtes(mostrarSeleccionados);

                errores += TiposComprobantes.Errors != null ? "\nAl cargar combo en TiposComprobantes: " + TiposComprobantes.Errors[0].Msg.ToString() : "";
                #endregion

                //Obtiene Concepto y se inicializa en 1-Producto
                TipoConceptos = service.FEParamGetTiposConcepto(authRequest);
                TipoConcepto.DataSource = TipoConceptos.ResultGet;
                iniciarTipoConcepto(mostrarSeleccionados);

                errores += TipoConceptos.Errors != null ? "\nAl cargar combo en tipo Concepto: " + TipoConceptos.Errors[0].Msg.ToString() : "";

                #region Tipos Documentos Carga Combo
                //Obtiene tipo Doc e inicializa combo en Id = 99 Desc: Doc.(otro)
                TipoDoc = service.FEParamGetTiposDoc(authRequest);
                TipoDocCMB.DataSource = TipoDoc.ResultGet;
                iniciarTipoDoc(mostrarSeleccionados);
                #endregion

                Monedas = service.FEParamGetTiposMonedas(authRequest);
                MonedaCMB.DataSource = Monedas.ResultGet;
                MonedaCMB.Enabled = !mostrarSeleccionados;

                errores += Monedas.Errors != null ? "\nAl cargar combo en Monedas: " + Monedas.Errors[0].Msg.ToString() : "";

                TiposIVA = service.FEParamGetTiposIva(authRequest);
                TipoIVACmb.DataSource = TiposIVA.ResultGet;
                iniciarTipoIva(mostrarSeleccionados);

                errores += TiposIVA.Errors != null ? "\nAl cargar combo en TiposIVA: " + TiposIVA.Errors[0].Msg.ToString() : "";

                var lastCbteObj = service.FECompUltimoAutorizado(authRequest, ptoVtaAfip, (int)TiposComprobantesCMB.SelectedValue);// TiposComprobantes.ResultGet[0].Id); 
                NroCbteTX.Text = (lastCbteObj.CbteNro + 1).ToString();
                //opcionales = service.FEParamGetTiposOpcional(authRequest);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n" + errores);
            }
        }

        private void inicializaciones(bool mostrarSeleccionados)
        {
            iniciarPtosVenta(mostrarSeleccionados);
            iniciarTipoConcepto(mostrarSeleccionados);
            iniciarTipoDoc(mostrarSeleccionados);
            iniciarTipoCbtes(mostrarSeleccionados);
            iniciarTipoIva(mostrarSeleccionados);
        }

        private void iniciarPtosVenta(bool mostrarSeleccionados)
        {
            PtoVenta[] ptosVentaSelect = new PtoVenta[1];
            ptos_venta_cm.Enabled = !mostrarSeleccionados;

            if (oVentaE == null || oVentaE.IdVenta == 0)
                return;

            for (int index = 0; index < ptos_venta_cm.Items.Count; index++)
            {
                PtoVenta item = (PtoVenta)ptos_venta_cm.Items[index];
                if (item.Nro == oVentaE.Sucursal.getPtoVtaAfip(oVentaE.Sucursal.idSucursal))
                {
                    ptos_venta_cm.SelectedIndex = index;
                    break;
                }
            }
        }

        private void iniciarTipoIva(bool mostrarSeleccionados)
        {
            listaAlicuotasFactura.Clear();  //limpia lista para que no sume importes de ventas anteriores

            //Obtiene las Alícuotas y establece el 10.5%
            for (int index = 0; index < TipoIVACmb.Items.Count; index++)
            {
                IvaTipo item = (IvaTipo)TipoIVACmb.Items[index];

                //cargo las alicuotas de iva para luego aplicar el importe
                Entidades.AlicuotaIva oAli = new Entidades.AlicuotaIva();
                oAli.IdIva = Convert.ToInt32(item.Id);
                oAli.Iva = Util_Form.convertFloat(item.Desc.Replace("%", ""), true);
                listaAlicuotasFactura.Add(oAli);

                if (item.Id == idIvaAliAfip)
                    TipoIVACmb.SelectedIndex = index;
            }
            TipoIVACmb.Enabled = !mostrarSeleccionados;
        }

        private void iniciarTipoDoc(bool mostrarSeleccionados)
        {
            DocTipo[] tiposDocSelect = new DocTipo[3];
            if (mostrarSeleccionados)
            {
                int idxSelect = 0;
                for (int index = 0; index < TipoDocCMB.Items.Count; index++)
                {
                    DocTipo item = (DocTipo)TipoDocCMB.Items[index];
                    if (item.Id == 99 || item.Id == 80 || item.Id == 96)
                    {
                        tiposDocSelect[idxSelect] = item;
                        idxSelect++;
                    }
                }
                TipoDocCMB.DataSource = tiposDocSelect;
            }
            for (int index = 0; index < TipoDocCMB.Items.Count; index++)
            {
                DocTipo item = (DocTipo)TipoDocCMB.Items[index];
                if (item.Id == 99)
                {
                    TipoDocCMB.SelectedIndex = index;
                    break;
                }
            }
        }

        private void iniciarTipoConcepto(bool mostrarSeleccionados)
        {
            ConceptoTipo[] tiposConceptoSelect = new ConceptoTipo[1];
            //if (mostrarSeleccionados)
            //{
            //    int idxSelect = 0;
            //    for (int index = 0; index < TipoConcepto.Items.Count; index++)
            //    {
            //        ConceptoTipo item = (ConceptoTipo)TipoConcepto.Items[index];
            //        //ID para Producto
            //        if (item.Id == 1)
            //        {
            //            tiposConceptoSelect[idxSelect] = item;
            //            idxSelect++;
            //        }
            //    }
            //    TipoConcepto.DataSource = tiposConceptoSelect;
            //}
            TipoConcepto.Enabled = !mostrarSeleccionados;
            for (int index = 0; index < TipoConcepto.Items.Count; index++)
            {
                ConceptoTipo item = (ConceptoTipo)TipoConcepto.Items[index];
                if (item.Id == Entidades.FacturaElectronica.codConceptoProductos_Afip)
                {
                    TipoConcepto.SelectedIndex = index;
                    break;
                }
            }
        }

        private void iniciarTipoCbtes(bool mostrarSeleccionados)
        {
            CbteTipo[] tiposComprobantesSelect = esRRII ? new CbteTipo[6] : new CbteTipo[3];
            if (mostrarSeleccionados)
            {
                //1: Factura A
                //2: Nota de Débito A
                //3: Nota de Crédito A
                //4: Recibo A
                //6: Factura B
                //7: Nota de Débito B
                //8: Nota de Crédito B
                //9: Recibo B
                //11: Factura C
                //12: Nota de Débito C
                //13: Nota de Crédito C
                //15: Recibo C
                int idxSelect = 0;
                for (int index = 0; index < TiposComprobantesCMB.Items.Count; index++)
                {
                    CbteTipo item = (CbteTipo)TiposComprobantesCMB.Items[index];
                    //si es RRII se muestran comprobantes que puede emitir 
                    if (esRRII)
                    {
                        if (item.Id == 1 || item.Id == 2 || item.Id == 3 || item.Id == 6 || item.Id == 7 || item.Id == 8)
                        {
                            tiposComprobantesSelect[idxSelect] = item;
                            idxSelect++;
                        }
                    }
                    else
                    {
                        if (item.Id == 11 || item.Id == 12 || item.Id == 13)
                        {
                            tiposComprobantesSelect[idxSelect] = item;
                            idxSelect++;
                        }
                    }
                }
                TiposComprobantesCMB.DataSource = tiposComprobantesSelect;
            }

            if (oVentaE == null || oVentaE.IdVenta == 0)
                return;

            ///obtiene el codigo del tipo de comprobante
            ///si luego es nota de credito, se setea codigo de nota segun comprobante
            int codTipoCbteAFip = oFactuElec.getCodTipoCbteAFIP_segunLetraFactura(oVentaE.TipoComprobante.ToString(), esRRII, notaCredito);


            for (int index = 0; index < TiposComprobantesCMB.Items.Count; index++)
            {
                CbteTipo item = (CbteTipo)TiposComprobantesCMB.Items[index];
                if (item.Id == codTipoCbteAFip)
                    TiposComprobantesCMB.SelectedIndex = index;
            }
        }

        #region codigoBarra
        //private void mostrar(FECompConsultaResponse asdf) {
        //    object r = asdf.ResultGet;
        //    string m = "";
        //    if (r) {
        //        IsNot;
        //        null;
        //        m = ("Estado: " 
        //                    + (r.Resultado + "\r\n"));
        //        ("CAE: " + r.CodAutorizacion);
        //        "\r\n";
        //        ("Vto: " + r.FchVto);
        //        "\r\n";
        //        ("Desde-Hasta: " 
        //                    + (r.CbteDesde + ("-" + r.CbteHasta)));
        //        "\r\n";
        //        ("Para: " + r.DocNro);
        //        "\r\n";
        //        ("Tipo Emision: " + r.EmisionTipo);
        //        "\r\n";
        //        ("Total: " + r.ImpTotal);
        //        "\r\n";
        //        if (r.Observaciones) {
        //            IsNot;
        //            null;
        //            foreach (o in r.Observaciones) {
        //                (string.Format("Obs: {0} ({1})", o.Msg, o.Code) + "\r\n");
        //            }
                    
        //        }
                
        //        // With...
        //        5.BottomMargin = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        5.TopMargin = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        5.RightMargin = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        80.LeftMargin = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        2.BarHeight = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        BarcodeLib.Barcode.UnitOfMeasure.PIXEL.BarWidth = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        true.UOM = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        string.Concat(authRequest.Cuit, r.CbteTipo.ToString("00"), r.PtoVta.ToString("0000"), r.CodAutorizacion, r.FchVto).AddCheckSum = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        BarcodeLib.Barcode.BarcodeType.INTERLEAVED25.Data = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //        LinearWinForm2.Type = Imaging.ImageFormat.Bmp.drawBarcode(LinearWinForm2.CreateGraphics);
        //    }
        //    else {
        //        m = "No hay ninguno anterior";
        //    }
            
        //    if (asdf.Errors) {
        //        IsNot;
        //        null;
        //        foreach (er in asdf.Errors) {
        //            ("\r\n" + string.Format("Er: {0}: {1}", er.Code, er.Msg));
        //        }
                
        //    }
            
        //    if (asdf.Events) {
        //        IsNot;
        //        null;
        //        foreach (ev in asdf.Events) {
        //            ("\r\n" + string.Format("Ev: {0}: {1}", ev.Code, ev.Msg));
        //        }
                
        //    }
            
        //    Resultado.Text = m;
        //}
        #endregion

        private void CalcBtn_Click(object sender, EventArgs e)
        {
            calcularImportes();
        }

        private void calcularImportes()
        {
            try
            {
                if (string.IsNullOrEmpty(TotalTx.Text) || !Utilidades.Util_Form.validarNumeroMayorACero(TotalTx.Text, "Total"))
                    return;

                IvaTipo iva = (IvaTipo)TipoIVACmb.SelectedItem;

                string desc = iva.Desc;
                desc = desc.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                desc = desc.Substring(0, (desc.Count() - 1));
                float ivaval = float.Parse(desc);


                float total = Utilidades.Util_Form.convertFloat(TotalTx.Text, false);
                float totalImporte = float.Parse(TotalTx.Text);
                float neto = importeNeto;// (total / iva_a_multiplicar);
                float imp_iva = importeIva;
                ImpIvaTx.Text = Math.Round(imp_iva, 2).ToString("N2");
                NetoTX.Text = Math.Round(neto, 2).ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //RegistrarBtn
        private void Button2_Click(object sender, EventArgs e)
        {
            try {

                //DialogResult imprimir1 = MostrarMensaje();

                //List<Entidades.AlicuotaIva> listaali = new List<Entidades.AlicuotaIva> ();
                //foreach (Entidades.LineaVenta item in oVentaE.LineasVenta)
                //{
                //    Entidades.AlicuotaIva ali = new Entidades.AlicuotaIva();
                //    ali.IdIva = item.Corte.IdAlicuotaIva;
                //    ali.BaseImponible = item.AlicuotaIva;
                //    ali.Importe = (item.AlicuotaIva * (item.CantKg * item.PrecioKg)) / 100;
                //    listaali.Add( ali );    
                //}
                //oFactuElec.ListaAlicuota = listaali;
                //oVentaN.addOrEditFactuElec(oFactuElec);

                ///si es nota de credita se llama el metodo
                ///
                if (notaCredito)
                {
                    notaCredito_Generar();
                    return;
                }
                bool nuevaPersona = false;

                if (!string.IsNullOrEmpty(DocTX.Text) && !Utilidades.Util_Form.validarCampoNumeroEntero(DocTX.Text, "Doc"))
                    return;

                if (TiposComprobantesCMB.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un valor en Comprobante");
                    return;
                }

                if (comboIva.SelectedValue == null)
                {
                    MessageBox.Show("Seleccione un valor en Cond.Iva");
                    return;
                }

                //Si cuit no está en Personas, lo agrega automáticamente
                if (!string.IsNullOrEmpty(DocTX.Text) && !(oPersonaN.existeCuit(DocTX.Text) > 0))
                {
                    nuevaPersona = true;
                    personaPadron.IdIva = Convert.ToInt16(comboIva.SelectedValue);
                }

                WSFEHOMO.Service service = getServicio();
                //**Certificado para loguearse con AFIP**
                service.ClientCertificates.Add(oLoginClass.certificado);

                DialogResult respuesta;
                if (oFactuElec != null && oFactuElec.Id > 0 && !string.IsNullOrEmpty(oFactuElec.CAE1))
                {
                   if (oFactuElec.RazonSocialAFIP.Equals(txtRazonSocial.Text) && oFactuElec.DomicilioAFIP.Equals(txtDomicilio.Text)
                        && oFactuElec.CondicionIvaAFIP.Equals(comboIva.Text))
                        MessageBox.Show("La Venta ya ha sido facturada", "Venta Facturada", MessageBoxButtons.OK, MessageBoxIcon.Information);                      
                   else
                   {
                       respuesta = MessageBox.Show("Venta ya ha sido facturada.\n\n¿Actualizar datos?",
                        "Factura Electronica", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                       if (DialogResult.Yes == respuesta)
                       {
                           oFactuElec.RazonSocialAFIP = txtRazonSocial.Text;
                           oFactuElec.DomicilioAFIP = txtDomicilio.Text;
                           //oFactuElec.CondicionIvaAFIP = comboIva.Text;
                           oVentaN.addOrEditFactuElec(oFactuElec);
                           //imprimirTicket(oFactuElec.esFacturaA(TiposComprobantesCMB.SelectedValue.ToString()), respuesta);
                           limpiarCampos(service);
                       }
                   }
                   return;
                }
                else
                    oFactuElec = new Entidades.FacturaElectronica();

                if (!Utilidades.Util_Form.validarCampoNumerico(TotalTx.Text, "Total"))
                    return;

                respuesta = MessageBox.Show("¿Registrar Factura Electrónica?.\n\nTotal: $ "+TotalTx.Text,
                    "Factura Electronica", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (!(DialogResult.Yes == respuesta))
                    return;

                if (oVentaE == null)
                {
                    if (logueado)
                    {
                        oVentaE = new Entidades.Venta();
                        oVentaE.LineasVenta = new List<Entidades.LineaVenta>();
                    }
                    else
                    {
                        MessageBox.Show("La venta que quiere facturar no existe.\n\nCierre y vuelva a abrir el formulario");
                        return;
                    }
                }

                ////Cambiando . por , para convertir a double
                //ImpIvaTx.Text = ImpIvaTx.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                //NetoTX.Text = NetoTX.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                //TotalTx.Text = TotalTx.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                //return;
                CbteTipo cm = (CbteTipo) TiposComprobantesCMB.SelectedItem;
                FECAERequest req = new FECAERequest(); ///Request para obtener CAE
                FECAECabRequest cab = new FECAECabRequest(); ///CABECERA DEL REQUEST
                FECAEDetRequest det = new FECAEDetRequest(); ///DETALLE(cuerpo) DEL REQUEST
                                                             ///
                //Cabecera del Request
                cab.CantReg = 1;
                cab.PtoVta = ptoVtaAfip;
                cab.CbteTipo = cm.Id;

                req.FeCabReq = cab; ///Asignamos la cabecera al Request 
                
                ///****Detalle del Request***
                //Concepto = 1-Producto
                ConceptoTipo concepto = (ConceptoTipo) TipoConcepto.SelectedItem;
                det.Concepto = concepto.Id;
                //Tipo y Nro Doc del Cliente
                // Para consumidortes finales ID=99 Desc= Doc. (Otro)
                DocTipo doctipo = (DocTipo) TipoDocCMB.SelectedItem;
                det.DocTipo = doctipo.Id;
                det.DocNro = !string.IsNullOrEmpty(DocTX.Text) ? long.Parse(DocTX.Text) : 0;
                //Ultimo CAE autorizado y se le suma 1
                FERecuperaLastCbteResponse lastRes = service.FECompUltimoAutorizado(authRequest, ptoVtaAfip, cm.Id);
                int last = lastRes.CbteNro;
                det.CbteDesde = last + 1;
                det.CbteHasta = last + 1;

                //Fecha del Comprobante
                det.CbteFch = FechaDTP.Value.ToString("yyyyMMdd");
                //Se calculan los importes a enviar
                det.ImpNeto = esRRII ? Convert.ToDouble(NetoTX.Text) : Convert.ToDouble(TotalTx.Text);
                det.ImpIVA = esRRII ? Convert.ToDouble(ImpIvaTx.Text) : 0;
                det.ImpTotal = Convert.ToDouble(TotalTx.Text);
                det.ImpTotConc = 0;
                det.ImpOpEx = 0;
                det.ImpTrib = 0;


                //Tipo Moneda 
                Moneda mon = (Moneda)MonedaCMB.SelectedItem;
                det.MonId = mon.Id;
                det.MonCotiz = 1;

                //Alicuota IVA
                //AlicIva alicuota = new AlicIva();
                //IvaTipo ivat = (IvaTipo)TipoIVACmb.SelectedItem;
                //alicuota.Id = Convert.ToInt32(ivat.Id); 
                //alicuota.BaseImp = Convert.ToDouble(NetoTX.Text);
                //alicuota.Importe = Convert.ToDouble(ImpIvaTx.Text);  

                if (esRRII)
                {
                    AlicIva[] alicuotaArr = new AlicIva[listaIdAlicuotaConIva.Count];
                    //recorro listaIdAlicuotaConIva y agrego al array
                    oFactuElec.ListaAlicuota = new List<Entidades.AlicuotaIva>();
                    for (int i = 0; i < listaIdAlicuotaConIva.Count; i++)
                    {
                        foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
                        {
                            if (item.IdIva == listaIdAlicuotaConIva[i])
                            {
                                AlicIva alicuota = new AlicIva();
                                alicuota.Id = item.IdIva;
                                //Redondeo para que queden 2 decimales y no tira error ws afip
                                alicuota.BaseImp = (double)Math.Round(item.BaseImponible, 2);
                                alicuota.Importe = (double)Math.Round(item.Importe, 2);

                                alicuotaArr[i] = alicuota;
                                oFactuElec.ListaAlicuota.Add(item);
                            }
                        }
                    }

                    det.Iva = alicuotaArr;
                }
                
                FECAEDetRequest[] reqArr = new FECAEDetRequest[1];
                reqArr[0] = det;
                req.FeDetReq = reqArr;

                //Solicita el CAE
                FECAEResponse r = service.FECAESolicitar(authRequest, req);

                string m = ("Estado: " 
                            + (r.FeCabResp.Resultado + "\r\n"));
                m += ("Estado Esp: " + r.FeDetResp[0].Resultado);
                m += "\r\n";
                m += ("CAE: " + r.FeDetResp[0].CAE);
                m += "\r\n";
                m += ("Vto: " + r.FeDetResp[0].CAEFchVto);
                m += "\r\n";
                m += ("Desde-Hasta: " + (r.FeDetResp[0].CbteDesde + ("-" + r.FeDetResp[0].CbteHasta)));
                m += "\r\n";

                string mensajeError = "";
                if (r.FeDetResp[0].Observaciones != null)
                    foreach (Obs o in r.FeDetResp[0].Observaciones) {
                        m += String.Format("Obs: {0} ({1})", o.Msg, o.Code) + "\r\n";

                        mensajeError += String.Format("Obs: {0} ({1})", o.Msg, o.Code) + "\r\n";
                }

                if(r.Errors != null ){
                    foreach(Err er in r.Errors){
                        m += String.Format("Er: {0}: {1}", er.Code, er.Msg) + "\r\n";

                        mensajeError += String.Format("Er: {0}: {1}", er.Code, er.Msg) + "\r\n";
                    }
                }

                if( r.Events != null ){
                    foreach(Evt ev in r.Events){
                        m += String.Format("Ev: {0}: {1}", ev.Code, ev.Msg) + "\r\n";

                        mensajeError += String.Format("Ev: {0}: {1}", ev.Code, ev.Msg) + "\r\n";
                    }
                }

                Resultado.Text = "";
                if (r.FeCabResp.Resultado.Equals("A"))
                {
                    facturaPendiente = false;
                    txtCAE.Text = r.FeDetResp[0].CAE;
                    txtVTO.Text = r.FeDetResp[0].CAEFchVto;

                    string ptoVtaFormatoAfip = (ptoVtaAfip + 100000).ToString().Substring(1);
                    string nroCbteFormatoAfip = (det.CbteDesde + 100000000).ToString().Substring(1);
                    string formaPago = oVentaE.FormaPago == null ? Entidades.Venta.formaPagoEnum.Debito.ToString() :
                        (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString())) ?
                        "Cuenta Corriente" : oVentaE.FormaPago.ToString();
                    //Si Factura A (id = 1)
                    bool esFacturaA = oFactuElec.esFacturaA(cm.Id.ToString());

                    //Cargar Objeto FactuElec
                    oFactuElec.PtoVtaAfip = ptoVtaFormatoAfip;
                    oFactuElec.FechaEmisionAfip = FechaDTP.Value;
                    oFactuElec.DescTipoCbteAfip = cm.Desc;
                    oFactuElec.CodTipoCbteAfip = cm.Id;
                    oFactuElec.NroCbteAfip = nroCbteFormatoAfip;
                    oFactuElec.TipoDocAfip = doctipo.Desc;
                    oFactuElec.NroDocAfip = DocTX.Text;
                    oFactuElec.RazonSocialAFIP = txtRazonSocial.Text;
                    oFactuElec.CondicionIvaAFIP = esFacturaA ? "Responsable Inscripto" : comboIva.SelectedText;
                    oFactuElec.DomicilioAFIP = txtDomicilio.Text;
                    oFactuElec.CondicionVenta = oVentaE.EnCtaCte ? "Cuenta Corriente" : "Contado";
                    oFactuElec.FormaPago = formaPago;
                    oFactuElec.CAE1 = txtCAE.Text;
                    oFactuElec.FecVtoCAE = txtVTO.Text;
                    oFactuElec.ImporteNetoGravado = Utilidades.Util_Form.convertFloat(det.ImpNeto.ToString("F2"), false);
                    oFactuElec.Iva = Utilidades.Util_Form.convertFloat(det.ImpIVA.ToString("F2"), false);
                    oFactuElec.ImporteTotal = Util_Form.convertFloat(det.ImpTotal.ToString("F2"), false);
                    oFactuElec.IdVenta = oVentaE.IdVenta;

                    oVentaN.addOrEditFactuElec(oFactuElec);

                    DialogResult imprimir = MostrarMensaje();
                        //MessageBox.Show("La Factura Electrónica se generó correctamente!.\n\n¿Imprimir ticket?.",
                        //"Imprimir Ticket", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
                     
                    Resultado.Text = "Hora Mensaje: " + DateTime.Now.ToLocalTime() +
                         "\n\n || La Factura se genero correctamente." + "\n || Importe: $ " + TotalTx.Text;
                    #region mostrar datos en AreaText
                    string salto = "\r\n";
                    #endregion

                    imprimirTicket(esFacturaA, imprimir);

                    if (generarPDF)
                        pdf_FacturaMetodo();

                    #region imprimir desde campos
                    ////Imprimir
                    //Ticket.CreaTicket ticket = new Ticket.CreaTicket();

                    ////imprimir si está checked
                    //ticket.imprimir = (imprimir == DialogResult.Yes) ? true : false;

                    ////Si Factura A (id = 1)
                    //bool esFacturaA = (cm.Id == 1);

                    //ticket.TextoCentro(ConfigurationManager.AppSettings["Negocio"].ToString());
                    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NegocioAgregado1"].ToString()))
                    //    ticket.TextoCentro(ConfigurationManager.AppSettings["NegocioAgregado1"].ToString());
                    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NegocioAgregado2"].ToString()))
                    //    ticket.TextoCentro(ConfigurationManager.AppSettings["NegocioAgregado2"].ToString());
                    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["NegocioAgregado3"].ToString()))
                    //    ticket.TextoCentro(ConfigurationManager.AppSettings["NegocioAgregado3"].ToString());

                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["IIBB"].ToString());
                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["Dueno"].ToString());
                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["Direccion"].ToString());
                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["Localidad"].ToString());
                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["InicioActividades"].ToString());
                    //ticket.TextoIzquierda(ConfigurationManager.AppSettings["CondicionIVA"].ToString());
                    //ticket.LineasGuion();

                    //if (esFacturaA)
                    //    ticket.TextoCentro("Original");
                    //ticket.TextoIzquierda(cm.Desc + " Electronica");
                    //string ptoVtaFormatoAfip = (ptoVtaAfip + 100000).ToString().Substring(1);
                    //string nroCbteFormatoAfip = (det.CbteDesde + 100000000).ToString().Substring(1);
                    //ticket.TextoIzquierda("Nro." + ptoVtaFormatoAfip + "-" + nroCbteFormatoAfip);
                    //ticket.TextoIzquierda("Fecha:" + FechaDTP.Value);// r.FeDetResp[0].CbteFch);
                    //string formaPago = oVentaE.FormaPago == null ? Entidades.Venta.formaPagoEnum.Debito.ToString() : 
                    //    (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString())) ?
                    //    "Cuenta Corriente" : oVentaE.FormaPago.ToString();
                    //ticket.TextoIzquierda("Pago: " + formaPago);
                    //ticket.LineasGuion();

                    //ticket.TextoIzquierda(txtRazonSocial.Text);
                    //if (det.DocNro != 0)
                    //{
                    //    ticket.TextoIzquierda(doctipo.Desc + ": " + det.DocNro.ToString()); 
                    //    ticket.TextoIzquierda(comboIva.SelectedText);
                    //    ticket.TextoIzquierda(txtDomicilio.Text);
                    //}

                    //ticket.LineasGuion();

                    //foreach (Entidades.LineaVenta linea in oVentaE.LineasVenta)
                    //{
                    //    if (esFacturaA)
                    //    {
                    //        float precioNeto = (linea.PrecioKg / iva_a_multiplicar);
                    //        float importeNeto = linea.CantKg * precioNeto;
                    //        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                    //            linea.CantKg, precioNeto, importeNeto);
                    //    }
                    //    else
                    //    {
                    //        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                    //            linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                    //    }
                    //}

                    //ticket.TextoDerecha("-------");
                    ////Si Factura A (id = 1)
                    //if (cm.Id == 1)
                    //{
                    //    ticket.TextoExtremos("Neto s/iva: ", det.ImpNeto.ToString("F2"));
                    //    ticket.TextoExtremos("Iva: ", det.ImpIVA.ToString("F2"));
                    //}
                    //ticket.TextoExtremos("TOTAL: ", det.ImpTotal.ToString("F2"));
                    //ticket.LineasEnBlanco(1);
                    //ticket.TextoIzquierda("CAE: " + r.FeDetResp[0].CAE);
                    //ticket.TextoIzquierda("Vto: " + r.FeDetResp[0].CAEFchVto);
                    //ticket.LineasEnBlanco(3);
                    #endregion

                    //si es nueva persona se la agregar y se actualiza la venta
                    if (nuevaPersona)
                    {
                        try
                        {
                            oPersonaN.addOrEditPersona(personaPadron);
                            personaPadron.idPersona = oPersonaN.existeCuit(personaPadron.Cuit);
                            oVentaN.actualizarCliente(idVenta, personaPadron.idPersona);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Ocurrió un error al agregar el Cliente y/o actualizar datos de venta.\n\n" + ex.Message);
                        }
                    }

                    if (esShowDialog)
                        this.Close();

                    this.SendToBack();
                    focoEnFormPOS();
                }                
                else
                {
                    ///Si factura electronica es rechazada guarda en la base de datos con el mensaje de error
                    ///
                    MessageBox.Show("Hubo un error al generar la factura\n\n"+mensajeError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Resultado.Text = "Hora Mensaje: " + DateTime.Now.ToLocalTime() + "\n *************** \n Mensaje Error: " + mensajeError;  
                    oFactuElec.Error = true;
                    oFactuElec.MensajeError = mensajeError;
                    oFactuElec.FechaError = DateTime.Now;
                    oVentaN.addOrEditFactuElec(oFactuElec);
                }
                limpiarCampos(service);
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void imprimirTicket(bool esFacturaA, DialogResult imprimir)
        {
            #region imprimir desde objeta FacturaElectronica
            try
            {
                //Imprimir
                Ticket.CreaTicket ticket = new Ticket.CreaTicket();

                //imprimir si está checked
                ticket.imprimir = (imprimir == DialogResult.Yes) ? true : false;

                Entidades.FacturaElectronica oDocumentoImprimir = notaCredito ? oNotaCredito : oFactuElec;

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

                ticket.TextoIzquierda(ConfigurationManager.AppSettings["IIBB"].ToString());
                ticket.TextoIzquierda(ConfigurationManager.AppSettings["Dueno"].ToString());
                ticket.TextoIzquierda(ConfigurationManager.AppSettings["Direccion"].ToString());
                ticket.TextoIzquierda(ConfigurationManager.AppSettings["Localidad"].ToString());
                ticket.TextoIzquierda(ConfigurationManager.AppSettings["InicioActividades"].ToString());
                ticket.TextoIzquierda(ConfigurationManager.AppSettings["CondicionIVA"].ToString());
                ticket.LineasGuion();

                if (esFacturaA)
                    ticket.TextoCentro("Original");

                ticket.TextoIzquierda(oDocumentoImprimir.DescTipoCbteAfip + " Electronica");
                ticket.TextoIzquierda("Nro." + oDocumentoImprimir.PtoVtaAfip + "-" + oDocumentoImprimir.NroCbteAfip);
                ticket.TextoIzquierda("Fecha:" + oDocumentoImprimir.FechaEmisionAfip);// r.FeDetResp[0].CbteFch);
                if (notaCredito)
                {
                    ticket.TextoIzquierda("Cbte Asoc: " + txtNroFacturaNotaCredito.Text.Replace("Factura ","F"));
                }
                ticket.TextoIzquierda("Pago: " + oDocumentoImprimir.FormaPago);
                ticket.LineasGuion();

                ticket.TextoIzquierda(oDocumentoImprimir.RazonSocialAFIP);
                if (!string.IsNullOrEmpty(oDocumentoImprimir.NroDocAfip))
                {
                    ticket.TextoIzquierda(oDocumentoImprimir.TipoDocAfip + ": " + oDocumentoImprimir.NroDocAfip);
                    ticket.TextoIzquierda(oDocumentoImprimir.CondicionIvaAFIP);
                    ticket.TextoIzquierda(oDocumentoImprimir.DomicilioAFIP);
                }

                ticket.LineasGuion();

                //Si linea es vacia se crea un ítem para facturacion
                Entidades.LineaVenta oLineaUnica = new Entidades.LineaVenta();
                Entidades.Corte oCorteUnico = new Entidades.Corte();

                if (oVentaE.LineasVenta == null || oVentaE.LineasVenta.Count == 0)
                {
                    oVentaE.LineasVenta = new List<Entidades.LineaVenta>();

                    oCorteUnico.codigo = 0;
                    oCorteUnico.corte = "Item Unitario";
                    oCorteUnico.precioKg = oDocumentoImprimir.ImporteTotal;

                    oLineaUnica.Corte = oCorteUnico;
                    oLineaUnica.CantKg = 1;
                    oLineaUnica.PrecioKg = oDocumentoImprimir.ImporteTotal;

                    oVentaE.LineasVenta.Add(oLineaUnica);
                }

                foreach (Entidades.LineaVenta linea in oVentaE.LineasVenta)
                {
                    if (esFacturaA)
                    {
                        //float precioNeto = (linea.PrecioKg / iva_a_multiplicar);
                        float divisorIva = 1 + (linea.AlicuotaIva / 100);
                        float precioNeto = (linea.PrecioKg / divisorIva);
                        float importeNeto = linea.CantKg * precioNeto;
                        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                            linea.CantKg, precioNeto, importeNeto);
                    }
                    else
                    {
                        ticket.AgregaArticulo(linea.Corte.codigo.ToString() + " " + linea.Corte.corte.ToString(),
                            linea.CantKg, linea.PrecioKg, linea.PrecioKg * linea.CantKg);
                    }
                }

                ticket.TextoDerecha("-------");
                //Si Factura A (id = 1)
                if (esFacturaA)
                {
                    ticket.TextoExtremos("Neto s/iva: ", oDocumentoImprimir.ImporteNetoGravado.ToString("F2"));

                    foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
                        if (item.Importe != 0)
                            ticket.TextoExtremos("Iva " + item.Iva + "%:", item.Importe.ToString("F2"));
                }

                ticket.TextoExtremos("TOTAL: ", oDocumentoImprimir.ImporteTotal.ToString("F2"));
                ticket.LineasEnBlanco(1);
                ticket.TextoIzquierda("CAE: " + oDocumentoImprimir.CAE1);
                ticket.TextoIzquierda("Vto: " + oDocumentoImprimir.FecVtoCAE);
                ticket.LineasEnBlanco(1);
                //si es transferencia se pide Nombre, DNI y Telefono
                bool esTransferencia = oDocumentoImprimir.FormaPago == Entidades.Pago.formasPago.Transferencia.ToString();
                if (esTransferencia)
                {
                    ticket.TextoIzquierda("Nombre:");
                    ticket.TextoIzquierda("DNI:");
                    ticket.TextoIzquierda("Telefono:");
                    ticket.LineasEnBlanco(4);
                }
                else
                {
                    ticket.LineasEnBlanco(3);
                }
                ticket.realizarImpresion();

                ///Imprimir QR
                ///
                if (ticket.imprimir && qrTicketFactura != null && !qrTicketFactura.ToUpper().Equals("NO"))
                {
                    string urlBase = "https://www.afip.gob.ar/fe/qr/";
                    string data = urlBase + "?p=" + (qrTicketFactura.Equals("INFO_FACTURA") ? GenerarJSON() : GenerarJSON_Contribuyente());
                    ticket.realizarImpresionQR(data);
                }
                //ticket = null;
                //ticket.LineasEnBlanco(3);
                //ticket.realizarImpresion();

                //si es transferencia preguntar si imprimir copia para el cliente
                if (esTransferencia)
                {
                    DialogResult imprimirCopia = MessageBox.Show("¿Imprimir copia?.",
                        "Imprimir Ticket", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
                    if (DialogResult.Yes == imprimirCopia)
                    {
                        ticket.realizarImpresion();
                        ///Imprimir QR
                        ///
                        if (ticket.imprimir && qrTicketFactura != null && !qrTicketFactura.ToUpper().Equals("NO"))
                        {
                            string urlBase = "https://www.afip.gob.ar/fe/qr/";
                            string data = urlBase + "?p=" + (qrTicketFactura.Equals("INFO_FACTURA") ? GenerarJSON() : GenerarJSON_Contribuyente());
                            ticket.realizarImpresionQR(data);
                        }
                    }                        
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Error al imprimir el ticket.");
            }
            #endregion
        }

        private void limpiarCampos(WSFEHOMO.Service service)
        {
            if (oFactuElec.Error)
                return;

            facturaPendiente = false;
            //Se limpian los campos
            oFactuElec = null;
            oVentaE = null;
            grillaLineasVenta.DataSource = null;

            txtIdVenta.Text = "";
            DocTX.Text = "";
            txtRazonSocial.Text = "";
            txtDomicilio.Text = "";
            ImpIvaTx.Text = "";
            TotalTx.Text = "";
            NetoTX.Text = "";
            txtCAE.Text = "";
            txtVTO.Text = "";
            txtIva0.Text = "";
            txtIva10_5.Text = "";
            txtIva21.Text = "";
            txtIva27.Text = "";
            txtIva5.Text = "";
            txtIva2_5.Text = "";

                //Obtiene Ultimo Nro Comprobante segun parametros
                var lastCbteObj = service.FECompUltimoAutorizado(authRequest, ptoVtaAfip, (int)TiposComprobantesCMB.SelectedValue);// TiposComprobantes.ResultGet[0].Id); 
            NroCbteTX.Text = (lastCbteObj.CbteNro + 1).ToString();
        }

        //ultimoBtn
        private void Button3_Click(object sender, EventArgs e)
        {
            try
            {
                WSFEHOMO.Service service = getServicio();
                service.ClientCertificates.Add(oLoginClass.certificado);

                CbteTipo cm = new CbteTipo();
                cm.Id = Convert.ToInt32(TiposComprobantesCMB.SelectedValue);

                //Recuperar puntos de ventas
                var ptoVtas = service.FEParamGetPtosVenta(authRequest);// FEParamGetPtosVenta

                FERecuperaLastCbteResponse last = service.FECompUltimoAutorizado(authRequest, ptoVtaAfip, cm.Id);
                FECompConsultaReq consulta = new FECompConsultaReq();
                consulta.CbteNro = last.CbteNro;
                consulta.CbteTipo = last.CbteTipo;
                consulta.PtoVta = last.PtoVta;
                FECompConsultaResponse asdf = service.FECompConsultar(authRequest, consulta);
                //mostrar(asdf); Muestra codigo de barras
                MessageBox.Show(("El Ultimo fue: " + last.CbteNro.ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //guardarBtn
        private void Button5_Click(object sender, EventArgs e)
        {

        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            //VtoDTP.Enabled = VtoCB.Checked;
        }

        private void NetoRB_CheckedChanged(object sender, EventArgs e)
        {
            cambiarIngresoImporte();
        }

        private void cambiarIngresoImporte()
        {
            NetoTX.ReadOnly = TotalRB.Checked;
            NetoTX.Text = "";
            TotalTx.Text = "";
            ImpIvaTx.Text = "";
            TotalTx.ReadOnly = NetoRB.Checked;
        }

        private void TiposComprobantesCMB_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (TiposComprobantesCMB.SelectedValue == null)
                {
                    NroCbteTX.Text = "";
                    return;
                }
                CbteTipo cm = new CbteTipo();
                cm.Id = Convert.ToInt32(TiposComprobantesCMB.SelectedValue.ToString());
                var last = getServicio().FECompUltimoAutorizado(authRequest, ptoVtaAfip, cm.Id);
                int ultimo_nro = last.CbteNro;
                NroCbteTX.Text = (ultimo_nro + 1).ToString();

                //if (cm.Id == 1)
                //{
                //    TipoDocCMB.SelectedValue = 80;
                //    comboIva.SelectedValue = 2;
                //}
                //else
                //{
                //    TipoDocCMB.SelectedValue = 99;
                //    comboIva.SelectedValue = 0;
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Service getServicio()
        {
            var s = new Service();
            s.Url = urlWSFE;
            return s;
        }
        private Service getServicioPerson()
        {
            var s = new Service();
            s.Url = urlWSPN;
            return s;
        }

        private void TipoIVACmb_SelectedIndexChanged(object sender, EventArgs e)
        {
            //se deberia agregar un metodo CalcularTotal al cambiar el combo
           // label_iva.Text = "IVA " + TipoIVACmb.Text; se comento el 17/02/2025 xq borraba el 27% de iva
        }

        private void TotalRB_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ptos_venta_cm_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtPuntoVentaConfig.Text = ptos_venta_cm.Text.ToString();
            ptoVtaAfip = Convert.ToInt32(ptos_venta_cm.Text.ToString());
            label_Sucursal.Text = oSucursalEntidad.getNomSucPorPtoVtaAfip(ptoVtaAfip);
        }

        private void TotalTx_TextChanged(object sender, EventArgs e)
        {
            calcularImportes();
        }

        private bool salir()
        {
            //Cuando el form es showDialog y ha sido facturada, se cierra automaticamente
            if (string.IsNullOrEmpty(txtIdVenta.Text) || txtIdVenta.Text.Equals("0") || (esShowDialog && !facturaPendiente) || (oFactuElec != null && !string.IsNullOrEmpty(oFactuElec.CAE1)))
            {
                esShowDialog = false;
                return false;
            }

            DialogResult respuesta;
            respuesta = MessageBox.Show("¿Cerrar la ventana sin realizar Factura Electrónica?.", 
                "Cerrar ventana", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);

            if (respuesta == DialogResult.Yes)
            {
                try
                {
                    if (oVentaE != null)// && !(oVentaE.FormaPago == Entidades.Venta.formaPagoEnum.Efectivo.ToString()))
                    {
                        oVentaN = new Negocio.Venta();
                        oFactuElec = new Entidades.FacturaElectronica();

                        oFactuElec.Id = oVentaN.esVentaSinFacturar(oVentaE.IdVenta, false);
                        //se valida que NO exista FacturaElectronica para el idVenta
                        if (oVentaE.IdVenta > 0 && (oFactuElec == null || oFactuElec.Id == 0))
                        {
                            oFactuElec.IdVenta = oVentaE.IdVenta;
                            oFactuElec.Error = true;
                            oFactuElec.MensajeError = "La venta NO ha sido facturada. Se Cerro la ventana sin facturar!";
                            oFactuElec.FechaError = DateTime.Now;
                            oVentaN.addOrEditFactuElec(oFactuElec);
                            facturaPendiente = false;
                        }
                    }
                }
                catch (Exception)
                {                    
                    throw;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void formFacturaElectronica_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = salir();
                
        }

        private void comboIva_SelectedIndexChanged(object sender, EventArgs e)
        {
            ///Cond.Iva:  1 - Consumidor Final / 2 - RRII / 3 - Monotributo / 4 - Exento
            ///Tipos Doc.: 80 - CUIT / 96 - DNI / 99 - Doc.(otro)
            //1: Factura A
            //2: Nota de Débito A
            //3: Nota de Crédito A
            //4: Recibo A
            //6: Factura B
            //7: Nota de Débito B
            //8: Nota de Crédito B
            //9: Recibo B
            //11: Factura C
            //12: Nota de Débito C
            //13: Nota de Crédito C
            //15: Recibo C

            TiposComprobantesCMB.SelectedValue  = esRRII ? 6 : 11;

            switch (comboIva.SelectedValue)
            {
                case 1:
                    TipoDocCMB.SelectedValue = 99;
                    break;
                case 2:
                    TipoDocCMB.SelectedValue = 80;
                    if (esRRII)
                        TiposComprobantesCMB.SelectedValue = notaCredito ? 3 : 1; //1: Factura A
                    break;
                case 3:
                    TipoDocCMB.SelectedValue = 80;
                    break;
                case 4:
                    TipoDocCMB.SelectedValue = 80;
                    break;
                default:
                    break;
            }
        }

        private void checkTodosDatos_CheckedChanged(object sender, EventArgs e)
        {
            mostrarSeleccionados = !checkTodosDatos.Checked;
            reCargarDatosAfip();
        }

        private void imprimir_Click(object sender, EventArgs e)
        {
            
            if (oFactuElec == null || (oFactuElec != null && string.IsNullOrEmpty(oFactuElec.CAE1)))
            {
                MessageBox.Show("No se puede imprimir ticket porque la factura no ha sido generada");
                return;
            }

            imprimirTicket(oFactuElec.esFacturaA(TiposComprobantesCMB.SelectedValue.ToString()), DialogResult.Yes);
        }

        private void pdf_Factura_Click(object sender, EventArgs e)
        {
            pdf_FacturaMetodo();
        }

        private void pdf_FacturaMetodo()
        {
            Entidades.FacturaElectronica oDocumentoImprimir = notaCredito ? oNotaCredito : oFactuElec;
            string tipoDocumentoDesc = notaCredito ? "Nota Crédito" : "Factura";

            if (oDocumentoImprimir == null || (oDocumentoImprimir != null && string.IsNullOrEmpty(oDocumentoImprimir.CAE1)))
            {
                MessageBox.Show("No se puede generar PDF porque la " + tipoDocumentoDesc + " no ha sido generada");
                return;
            }

            string ruta = ConfigurationManager.AppSettings["rutaPDF"].ToString();
            string rutaPDF = @ruta + "\\" + oDocumentoImprimir.FechaEmisionAfip?.ToString("yyyyMMdd") + " " +
                oDocumentoImprimir.DescTipoCbteAfip + " " + oDocumentoImprimir.PtoVtaAfip + "-" + oDocumentoImprimir.NroCbteAfip + ".pdf";

            // Verificar si la carpeta existe, si no, crearla
            if (!Directory.Exists(@ruta))
                Directory.CreateDirectory(@ruta);

            #region Factura PDF
            // Crear el documento PDF
            Document documento = new Document(PageSize.A4, 36, 36, 36, 36);
            PdfWriter.GetInstance(documento, new FileStream(rutaPDF, FileMode.Create));
            documento.Open();

            // Fuentes y estilos
            var fontTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var fontSubTitle = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var fontNormal = FontFactory.GetFont(FontFactory.HELVETICA, 10);
            var fontComments = FontFactory.GetFont(FontFactory.HELVETICA, 8);
            var fontNormalBold = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);

            // Crear una tabla para el membrete
            PdfPTable tablaMembrete = new PdfPTable(3);
            tablaMembrete.WidthPercentage = 100;

            // Definir tamaños de las columnas (30% para logo, 40% para datos empresa, 30% para datos cliente)
            float[] widths = new float[] { 47f, 6f, 47f };
            tablaMembrete.SetWidths(widths);

            //// Celda para el logo
            //string logoRuta = @"C:\Dropbox\BackUp\CarniSys_Logo.png"; // Reemplaza con la ruta de tu logo
            //iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoRuta);
            //logo.ScaleToFit(80f, 80f);

            //PdfPCell celdaLogo = new PdfPCell(logo);
            //celdaLogo.Border = iTextSharp.text.Rectangle.RECTANGLE;
            //celdaLogo.HorizontalAlignment = Element.ALIGN_LEFT;
            //tablaMembrete.AddCell(celdaLogo);

            PdfPCell celdamembreteIzquierda = new PdfPCell();
            //celdamembreteIzquierda.Border = iTextSharp.text.Rectangle.RECTANGLE;
            celdamembreteIzquierda.Border = iTextSharp.text.Rectangle.NO_BORDER;
            celdamembreteIzquierda.HorizontalAlignment = Element.ALIGN_CENTER;
            celdamembreteIzquierda.VerticalAlignment = Element.ALIGN_CENTER;

            Phrase membreteIzquierda = new Phrase();
            membreteIzquierda.Add(new Chunk("\n" + ConfigurationManager.AppSettings["Negocio"].ToString() + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20)));
            membreteIzquierda.Add(new Chunk("Razón Social: " + ConfigurationManager.AppSettings["Dueno"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteIzquierda.Add(new Chunk(ConfigurationManager.AppSettings["Direccion"].ToString() + " - " + ConfigurationManager.AppSettings["Localidad"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteIzquierda.Add(new Chunk("Condición frente al IVA: " + ConfigurationManager.AppSettings["CondicionIVA"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            celdamembreteIzquierda.AddElement(membreteIzquierda);
            tablaMembrete.AddCell(celdamembreteIzquierda);

            // Celda tipo Factura
            PdfPCell celdaTipoFactura = new PdfPCell();
            //celdaTipoFactura.Border = iTextSharp.text.Rectangle.RECTANGLE;
            celdaTipoFactura.Border = iTextSharp.text.Rectangle.NO_BORDER;
            celdaTipoFactura.HorizontalAlignment = Element.ALIGN_CENTER;
            celdaTipoFactura.VerticalAlignment = Element.ALIGN_TOP;

            Phrase tipoFactura = new Phrase();
            char letraFactura = oDocumentoImprimir.DescTipoCbteAfip[oDocumentoImprimir.DescTipoCbteAfip.Length - 1];
            string letraFacturaEncabezado = "  " + letraFactura + "  ";
            String codFactura = "COD." + (oDocumentoImprimir.CodTipoCbteAfip < 10 ? ("0" + oDocumentoImprimir.CodTipoCbteAfip.ToString()) : oDocumentoImprimir.CodTipoCbteAfip.ToString());
            string descComprobante = oDocumentoImprimir.DescTipoCbteAfip.Substring(0, oDocumentoImprimir.DescTipoCbteAfip.Length - 1);
            tipoFactura.Add(new Chunk(letraFacturaEncabezado, FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 15)));
            tipoFactura.Add(new Chunk(codFactura, FontFactory.GetFont(FontFactory.HELVETICA, 7)));
            celdaTipoFactura.AddElement(tipoFactura);
            tablaMembrete.AddCell(celdaTipoFactura);

            // Celda Membrete derecha
            PdfPCell celdamembreteDerecha = new PdfPCell();
            //celdamembreteDerecha.Border = iTextSharp.text.Rectangle.RECTANGLE;
            celdamembreteDerecha.Border = iTextSharp.text.Rectangle.NO_BORDER;
            celdamembreteDerecha.HorizontalAlignment = Element.ALIGN_CENTER;

            Phrase membreteDerecha = new Phrase();
            membreteDerecha.Add(new Chunk(descComprobante.ToUpper() + "\n", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12)));
            membreteDerecha.Add(new Chunk("Punto de Venta: " + oDocumentoImprimir.PtoVtaAfip + "   Comp.Nro: " + oDocumentoImprimir.NroCbteAfip + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteDerecha.Add(new Chunk("Fecha de Emisión: " + oDocumentoImprimir.FechaEmisionAfip.Value.Date.ToString("dd/MM/yyyy") + "\n\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteDerecha.Add(new Chunk("CUIT: " + ConfigurationManager.AppSettings["cuit"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["IIBB"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            membreteDerecha.Add(new Chunk(ConfigurationManager.AppSettings["InicioActividades"].ToString() + "\n", FontFactory.GetFont(FontFactory.HELVETICA, 9)));
            celdamembreteDerecha.AddElement(membreteDerecha);
            tablaMembrete.AddCell(celdamembreteDerecha);

            // Agregar la tabla al documento
            documento.Add(tablaMembrete);
            //documento.Add(new Paragraph("\n")); // Añadir un espacio después del membrete

            // Crear un LineSeparator para la línea horizontal
            LineSeparator line = new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, 0);
            // Agregar la línea al documento
            documento.Add(new Chunk(line));
            documento.Add(new Chunk(line));

            //// Información del Cliente
            PdfPTable clienteTable = new PdfPTable(1);
            clienteTable.WidthPercentage = 100;
            clienteTable.SetWidths(new float[] { 1f });

            string datosCliente = "CUIT:   " + oDocumentoImprimir.NroDocAfip +
                "              Apellido y Nombre/Razón Social:   " + oDocumentoImprimir.RazonSocialAFIP.ToUpper() +
                "\n\nCondición frente al IVA:   " + comboIva.Text + //oDocumentoImprimir.CondicionIvaAFIP.ToUpper() + 
                "\n\nDomicilio :   " + oDocumentoImprimir.DomicilioAFIP.ToUpper() +
                "\n\nCondición de venta:   " + txtFormaPago.Text.ToUpper() +
                "                                    " + txtNroFacturaNotaCredito.Text; //oDocumentoImprimir.CondicionVenta.ToUpper();

            clienteTable.AddCell(new PdfPCell(new Phrase(datosCliente, fontNormal)) { Border = 0 });
            documento.Add(clienteTable);

            #region tabla de productos

            int cantCol = letraFactura == 'A' ? 5 : 4;
            PdfPTable productosTable = new PdfPTable(cantCol);
            productosTable.WidthPercentage = 100;

            if (letraFactura == 'A')
                productosTable.SetWidths(new float[] { 6f, 2f, 2f, 2f, 2f });
            else
                productosTable.SetWidths(new float[] { 6f, 2f, 2f, 2f });

            productosTable.AddCell(new PdfPCell(new Phrase("Descripción", fontNormalBold)) { BorderWidthTop = 1, BorderWidthBottom = 1 });
            productosTable.AddCell(new PdfPCell(new Phrase("Cantidad", fontNormalBold)) { BorderWidthTop = 1, BorderWidthBottom = 1 });
            productosTable.AddCell(new PdfPCell(new Phrase("Precio Un.", fontNormalBold)) { BorderWidthTop = 1, BorderWidthBottom = 1 });
            if (letraFactura == 'A')
                productosTable.AddCell(new PdfPCell(new Phrase("Alicuota Iva", fontNormalBold)) { BorderWidthTop = 1, BorderWidthBottom = 1 });

            productosTable.AddCell(new PdfPCell(new Phrase("Importe", fontNormalBold)) { BorderWidthTop = 1, BorderWidthBottom = 1 });

            oDocumentoImprimir.Venta = oDocumentoImprimir.Venta == null ? oVentaE : oDocumentoImprimir.Venta;

            foreach (Entidades.LineaVenta item in oDocumentoImprimir.Venta.LineasVenta)
            {
                productosTable.AddCell(new PdfPCell(new Phrase(item.Corte.codigo.ToString() + " - " + item.Corte.corte, fontNormal)) { Border = 0 });
                productosTable.AddCell(new PdfPCell(new Phrase(item.CantKg.ToString("F3"), fontNormal)) { Border = 0 });
                productosTable.AddCell(new PdfPCell(new Phrase(item.PrecioKg.ToString("F2"), fontNormal)) { Border = 0 });
                if (letraFactura == 'A')
                    productosTable.AddCell(new PdfPCell(new Phrase(item.AlicuotaIva.ToString("F2"), fontNormal)) { Border = 0 });

                productosTable.AddCell(new PdfPCell(new Phrase((item.PrecioKg * item.CantKg).ToString("F2"), fontNormal)) { Border = 0 });

            }

            documento.Add(productosTable);

            int cantLineasVacias = Convert.ToInt32(ConfigurationManager.AppSettings["cantLineasVacias"].ToString());
            //se le resta la cantidad e alicuota - 1 para evitar que pise el QR
            cantLineasVacias -= (oDocumentoImprimir.Venta.LineasVenta.Count - 1);
            for (int i = 0; i < cantLineasVacias; i++)
                documento.Add(new iTextSharp.text.Paragraph("\n"));

            #endregion

            // Totales
            PdfPTable importeTextoTable = new PdfPTable(1);
            importeTextoTable.WidthPercentage = 100;
            importeTextoTable.SetWidths(new float[] { 1f });
            importeTextoTable.AddCell(new PdfPCell(new Phrase(ConvertirMontoEnTexto(Convert.ToDecimal(oVentaE.TotalImporte)), fontComments)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });
            documento.Add(importeTextoTable);

            // Agregar la línea al documento
            documento.Add(new Chunk(line));

            // Totales
            PdfPTable totalTable = new PdfPTable(3);
            totalTable.WidthPercentage = 100;
            totalTable.SetWidths(new float[] { 5f, 1f, 1f });

            totalTable.AddCell(new PdfPCell(new Phrase(txtObservaciones.Text, fontComments)) { Border = 0, HorizontalAlignment = Element.ALIGN_LEFT });

            if (letraFactura == 'A')
            {
                //totalTable.AddCell(new PdfPCell(new Phrase("", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                totalTable.AddCell(new PdfPCell(new Phrase("Neto s/iva: $", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                totalTable.AddCell(new PdfPCell(new Phrase(oDocumentoImprimir.ImporteNetoGravado.ToString("F2"), fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

                foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
                {
                    if (item.Importe > 0)
                    {
                        totalTable.AddCell(new PdfPCell(new Phrase("", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                        totalTable.AddCell(new PdfPCell(new Phrase("Iva " + item.Iva + "%: $", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                        totalTable.AddCell(new PdfPCell(new Phrase(item.Importe.ToString("F2"), fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                    }
                }
            }
            else
            {
                //totalTable.AddCell(new PdfPCell(new Phrase("", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                totalTable.AddCell(new PdfPCell(new Phrase("Subtotal: $", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
                totalTable.AddCell(new PdfPCell(new Phrase(oDocumentoImprimir.ImporteTotal.ToString("F2"), fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            }
            totalTable.AddCell(new PdfPCell(new Phrase("", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            totalTable.AddCell(new PdfPCell(new Phrase("Total: $", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            totalTable.AddCell(new PdfPCell(new Phrase(oDocumentoImprimir.ImporteTotal.ToString("F2"), fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

            documento.Add(totalTable);

            // Agregar la línea al documento
            documento.Add(new Chunk(line));

            documento.Add(new iTextSharp.text.Paragraph(" "));

            // Insertar el código QR en el documento PDF
            iTextSharp.text.Image qrImage = iTextSharp.text.Image.GetInstance(GenerateQRCode());
            // Configurar la posición del QR en la esquina inferior izquierda
            float xPosition = documento.LeftMargin; // Considera el margen izquierdo
            float yPosition = documento.BottomMargin + (100 / 2); // Considera el margen inferior
            qrImage.SetAbsolutePosition(xPosition, yPosition); // Esquina inferior izquierda
            qrImage.ScaleAbsolute(100, 100); // Ajustar el tamaño del QR

            documento.Add(qrImage);
            // Información del CAE
            // Totales
            PdfPTable infoCAE = new PdfPTable(1);
            infoCAE.WidthPercentage = 100;
            infoCAE.SetWidths(new float[] { 1f });
            infoCAE.AddCell(new PdfPCell(new Phrase($"CAE: {oDocumentoImprimir.CAE1}", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });
            infoCAE.AddCell(new PdfPCell(new Phrase($"Fecha de Vencimiento del CAE: {oDocumentoImprimir.FecVtoCAE}", fontNormalBold)) { Border = 0, HorizontalAlignment = Element.ALIGN_RIGHT });

            documento.Add(infoCAE);
            // Cerrar el documento
            documento.Close();

            if (MessageBox.Show("¿Abrir ubicación del archivo?",
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1).Equals(DialogResult.Yes))
            {
                //Abre ubicacion
                Process.Start("explorer.exe", @ruta);
                Thread.Sleep(1500); // Pausa por 3000 milisegundos (3 segundos)
            }

            // Usar Process.Start para abrir el PDF
            Process.Start(new ProcessStartInfo(rutaPDF) { UseShellExecute = true });
            #endregion
        }

        public byte[] GenerateQRCode()
        {
            //string data = GenerarJSON();
            string urlBase = "https://www.afip.gob.ar/fe/qr/";
            string data = urlBase + "?p=" + GenerarJSON();

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }
        private string GenerarJSON()
        {
            Entidades.FacturaElectronica oDocumentoImprimir = notaCredito ? oNotaCredito : oFactuElec;
            string fechaEmision = oDocumentoImprimir.FechaEmisionAfip?.ToString("yyyy-MM-dd");
            // Crear la estructura del JSON utilizando la variable
            long _cuitEmisor = long.Parse(cuit);
            long _nroCmp = long.Parse(oDocumentoImprimir.NroCbteAfip);
            long _nroDocRec = string.IsNullOrEmpty(oDocumentoImprimir.NroDocAfip) ? 0 : long.Parse(oDocumentoImprimir.NroDocAfip);
            long _codAut = long.Parse(oDocumentoImprimir.CAE1);
            int _ptoVta = Convert.ToInt32(oDocumentoImprimir.PtoVtaAfip);
            int _tipoDocRec = Convert.ToInt32(TipoDocCMB.SelectedValue.ToString());
            decimal _importe = Convert.ToDecimal(oDocumentoImprimir.ImporteTotal);
            var qrData = new
            {
                ver = 1,
                fecha = fechaEmision,
                cuit = _cuitEmisor,
                ptoVta = _ptoVta,
                tipoCmp = oDocumentoImprimir.CodTipoCbteAfip,
                nroCmp = _nroCmp,
                importe = _importe,
                moneda = "PES",
                ctz = 1,
                tipoDocRec = _tipoDocRec,
                nroDocRec = _nroDocRec,
                tipoCodAut = "E",
                codAut = _codAut
            };

            // Serializar el objeto a JSON
            string jsonData = JsonConvert.SerializeObject(qrData, Formatting.Indented);
            //Codificar el JSON en Base64
            string jsonDataBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));

            return jsonDataBase64;
        }

        private string GenerarJSON_Contribuyente()
        {
            Entidades.FacturaElectronica oDocumentoImprimir = notaCredito ? oNotaCredito : oFactuElec;
            string fechaEmision = oDocumentoImprimir.FechaEmisionAfip?.ToString("yyyy-MM-dd");
            // Crear la estructura del JSON utilizando la variable
            long _cuitEmisor = long.Parse(cuit);
            long _nroCmp = long.Parse(oDocumentoImprimir.NroCbteAfip);
            long _nroDocRec = string.IsNullOrEmpty(oDocumentoImprimir.NroDocAfip) ? 0 : long.Parse(oDocumentoImprimir.NroDocAfip);
            long _codAut = long.Parse(oDocumentoImprimir.CAE1);
            int _ptoVta = Convert.ToInt32(oDocumentoImprimir.PtoVtaAfip);
            int _tipoDocRec = Convert.ToInt32(TipoDocCMB.SelectedValue.ToString());
            decimal _importe = Convert.ToDecimal(oDocumentoImprimir.ImporteTotal);
            var qrData = new
            {
                ver = 1,
                fecha = fechaEmision,
                cuit = _cuitEmisor//,
                //ptoVta = _ptoVta,
                //tipoCmp = oDocumentoImprimir.CodTipoCbteAfip,
                //nroCmp = _nroCmp,
                //importe = _importe,
                //moneda = "PES",
                //ctz = 1,
                //tipoDocRec = _tipoDocRec,
                //nroDocRec = _nroDocRec,
                //tipoCodAut = "E",
                //codAut = _codAut
            };

            // Serializar el objeto a JSON
            string jsonData = JsonConvert.SerializeObject(qrData, Formatting.Indented);
            //Codificar el JSON en Base64
            string jsonDataBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(jsonData));

            return jsonDataBase64;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ( Util_Form.validarCampoNumeroEntero(DocTX.Text, "Doc"))
            {
                ConsultarDatosContribuyente(DocTX.Text);
            }
        }
        public void ConsultarDatosContribuyente(string cuitPerson)
        {
            try
            {
                FEAuthRequest authRequestPerson = new FEAuthRequest();
                authRequestPerson.Cuit = long.Parse(cuit);
                authRequestPerson.Sign = oLoginClassPerson.Sign;
                authRequestPerson.Token = oLoginClassPerson.Token;


                WSPSA13.PersonaServiceA13 servicePerson = new WSPSA13.PersonaServiceA13();
                servicePerson.Url = urlWSPN;
                servicePerson.ClientCertificates.Add(oLoginClassPerson.certificado);

                var response = servicePerson.getPersona(authRequestPerson.Token, authRequestPerson.Sign, authRequestPerson.Cuit, long.Parse(cuitPerson));

                if (response.persona != null)
                {
                    //establece el combo tipo Doc en CUIT - 80 es el ID de AFIP
                    TipoDocCMB.SelectedValue = 80;

                    razonSocialAfip = response.persona.tipoPersona.Equals("FISICA") ?
                        Convert.ToString(response.persona.apellido + " " + response.persona.nombre) : Convert.ToString(response.persona.razonSocial);
                    domicilioFiscalAfip = response.persona.domicilio[0].direccion;
                    localidadAfip = response.persona.domicilio[0].localidad;
                    provinciaAfip = response.persona.domicilio[0].descripcionProvincia;

                    txtRazonSocial.Text = razonSocialAfip;
                    txtDomicilio.Text = Convert.ToString(response.persona.domicilio[0].direccion + " - " +
                        response.persona.domicilio[0].localidad + ", " + response.persona.domicilio[0].descripcionProvincia);

                    //cargo el objete Persona para agregarlo a clientes si no existe tal cuit al Facturar
                    personaPadron.Cuit = cuitPerson;
                    personaPadron.razonSocial = personaPadron.Identificacion = razonSocialAfip;
                    personaPadron.Domicilio = domicilioFiscalAfip;
                    personaPadron.Ciudad = response.persona.domicilio[0].localidad + ", " + response.persona.domicilio[0].descripcionProvincia;

                    //Establece combos Comprobante y Cond.Iva en nulo para que el usuario seleccione el correspondiente
                    comboIva.SelectedIndex = -1;
                    TiposComprobantesCMB.SelectedIndex = -1;
                }
                else
                {
                    MessageBox.Show("No se encontraron datos para el CUIT especificado.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void NotaCredito_Click(object sender, EventArgs e)
        {
            notaCredito_Generar();
        }

        private void notaCredito_Generar()
        {
            #region NotaCredito Pendiente
            try
            {

                WSFEHOMO.Service service = getServicio();
                //**Certificado para loguearse con AFIP**
                service.ClientCertificates.Add(oLoginClass.certificado);

                DialogResult respuesta;
                if (oNotaCredito != null && oNotaCredito.Id > 0 && !string.IsNullOrEmpty(oNotaCredito.CAE1))
                {
                    MessageBox.Show("La Nota de Crédito ya ha sido generada anteriormente", "Nota Crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    return;
                }

                if (oFactuElec == null || oFactuElec.Id == 0)
                {
                    MessageBox.Show("La Factura para cual quiere generar Nota Crédito no existe.\n\nCierre y vuelva a abrir el formulario");
                    return;                    
                }


                if (!Utilidades.Util_Form.validarCampoNumerico(TotalTx.Text, "Total"))
                    return;

                string anularVentaMensaje = "\n\nNota: Al realizar la nota de crédito, se generará automáticamente una venta " +
                    " con cantidades en negativo e igual fecha para que quede anulada.";
                respuesta = MessageBox.Show("¿Generar Nota Crédito Electrónica?.\n\nTotal: $ " + TotalTx.Text + anularVentaMensaje,
                    "Nota Crédito", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (!(DialogResult.Yes == respuesta))
                    return;               
                

                oNotaCredito = new Entidades.FacturaElectronica();

                //Cambiando . por , para convertir a double
                ImpIvaTx.Text = ImpIvaTx.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                NetoTX.Text = NetoTX.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);
                TotalTx.Text = TotalTx.Text.Replace(".", System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                CbteTipo cm = (CbteTipo)TiposComprobantesCMB.SelectedItem;
                FECAERequest req = new FECAERequest(); ///Request para obtener CAE
                FECAECabRequest cab = new FECAECabRequest(); ///CABECERA DEL REQUEST
                FECAEDetRequest det = new FECAEDetRequest(); ///DETALLE(cuerpo) DEL REQUEST
                                                             ///
                //Cabecera del Request
                cab.CantReg = 1;
                cab.PtoVta = ptoVtaAfip;
                cab.CbteTipo = cm.Id;

                req.FeCabReq = cab; ///Asignamos la cabecera al Request 

                                    ///****Detalle del Request***
                //Concepto = 1-Producto
                ConceptoTipo concepto = (ConceptoTipo)TipoConcepto.SelectedItem;
                det.Concepto = concepto.Id;
                //Tipo y Nro Doc del Cliente
                // Para consumidortes finales ID=99 Desc= Doc. (Otro)
                DocTipo doctipo = (DocTipo)TipoDocCMB.SelectedItem;
                det.DocTipo = doctipo.Id;
                det.DocNro = !string.IsNullOrEmpty(DocTX.Text) ? long.Parse(DocTX.Text) : 0;
                //Ultimo CAE autorizado y se le suma 1
                FERecuperaLastCbteResponse lastRes = service.FECompUltimoAutorizado(authRequest, ptoVtaAfip, cm.Id);
                int last = lastRes.CbteNro;
                det.CbteDesde = last + 1;
                det.CbteHasta = last + 1;

                //Fecha del Comprobante
                det.CbteFch = FechaDTP.Value.ToString("yyyyMMdd");
                //Se calculan los importes a enviar
                det.ImpNeto = esRRII ? Convert.ToDouble(NetoTX.Text) : Convert.ToDouble(TotalTx.Text);
                det.ImpIVA = esRRII ? Convert.ToDouble(ImpIvaTx.Text) : 0;
                det.ImpTotal = Convert.ToDouble(TotalTx.Text);
                det.ImpTotConc = 0;
                det.ImpOpEx = 0;
                det.ImpTrib = 0;


                //Tipo Moneda 
                Moneda mon = (Moneda)MonedaCMB.SelectedItem;
                det.MonId = mon.Id;
                det.MonCotiz = 1;

                // COMPROBANTE ASOCIADO

                CbteAsoc[] cbtesAsociadosArr = new CbteAsoc[1];

                CbteAsoc cbteAsoc = new CbteAsoc();
                cbteAsoc.Tipo = oFactuElec.CodTipoCbteAfip;// Factura A (001), B (006), C (011)
                cbteAsoc.PtoVta = int.Parse(oFactuElec.PtoVtaAfip);
                cbteAsoc.Nro = long.Parse(oFactuElec.NroCbteAfip);// Número de factura asociada
                cbteAsoc.Cuit = oFactuElec.NroDocAfip;// CUIT del receptor
                cbtesAsociadosArr[0] = cbteAsoc;

                det.CbtesAsoc = cbtesAsociadosArr;

                if (esRRII)
                {
                    AlicIva[] alicuotaArr = new AlicIva[listaIdAlicuotaConIva.Count];
                    //recorro listaIdAlicuotaConIva y agrego al array
                    oNotaCredito.ListaAlicuota = new List<Entidades.AlicuotaIva>();
                    for (int i = 0; i < listaIdAlicuotaConIva.Count; i++)
                    {
                        foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
                        {
                            if (item.IdIva == listaIdAlicuotaConIva[i])
                            {
                                AlicIva alicuota = new AlicIva();
                                alicuota.Id = item.IdIva;
                                //Redondeo para que queden 2 decimales y no tira error ws afip
                                alicuota.BaseImp = (double)Math.Round(item.BaseImponible, 2);
                                alicuota.Importe = (double)Math.Round(item.Importe, 2);

                                alicuotaArr[i] = alicuota;
                                oNotaCredito.ListaAlicuota.Add(item);
                            }
                        }
                    }

                    det.Iva = alicuotaArr;
                }

                FECAEDetRequest[] reqArr = new FECAEDetRequest[1];
                reqArr[0] = det;
                req.FeDetReq = reqArr;

                //Solicita el CAE
                FECAEResponse r = service.FECAESolicitar(authRequest, req);

                string m = ("Estado: "
                            + (r.FeCabResp.Resultado + "\r\n"));
                m += ("Estado Esp: " + r.FeDetResp[0].Resultado);
                m += "\r\n";
                m += ("CAE: " + r.FeDetResp[0].CAE);
                m += "\r\n";
                m += ("Vto: " + r.FeDetResp[0].CAEFchVto);
                m += "\r\n";
                m += ("Desde-Hasta: " + (r.FeDetResp[0].CbteDesde + ("-" + r.FeDetResp[0].CbteHasta)));
                m += "\r\n";

                string mensajeError = "";
                if (r.FeDetResp[0].Observaciones != null)
                    foreach (Obs o in r.FeDetResp[0].Observaciones)
                    {
                        m += String.Format("Obs: {0} ({1})", o.Msg, o.Code) + "\r\n";

                        mensajeError += String.Format("Obs: {0} ({1})", o.Msg, o.Code) + "\r\n";
                    }

                if (r.Errors != null)
                {
                    foreach (Err er in r.Errors)
                    {
                        m += String.Format("Er: {0}: {1}", er.Code, er.Msg) + "\r\n";

                        mensajeError += String.Format("Er: {0}: {1}", er.Code, er.Msg) + "\r\n";
                    }
                }

                if (r.Events != null)
                {
                    foreach (Evt ev in r.Events)
                    {
                        m += String.Format("Ev: {0}: {1}", ev.Code, ev.Msg) + "\r\n";

                        mensajeError += String.Format("Ev: {0}: {1}", ev.Code, ev.Msg) + "\r\n";
                    }
                }

                Resultado.Text = "";
                if (r.FeCabResp.Resultado.Equals("A"))
                {
                    facturaPendiente = false;
                    txtCAE.Text = r.FeDetResp[0].CAE;
                    txtVTO.Text = r.FeDetResp[0].CAEFchVto;

                    string ptoVtaFormatoAfip = (ptoVtaAfip + 100000).ToString().Substring(1);
                    string nroCbteFormatoAfip = (det.CbteDesde + 100000000).ToString().Substring(1);
                    string formaPago = oVentaE.FormaPago == null ? Entidades.Venta.formaPagoEnum.Debito.ToString() :
                        (oVentaE.EnCtaCte && oVentaE.FormaPago.Equals(Entidades.Venta.formaPagoEnum.Efectivo.ToString())) ?
                        "Cuenta Corriente" : oVentaE.FormaPago.ToString();
                    //Si Factura A (id = 1)
                    bool esFacturaA = oNotaCredito.esFacturaA(cm.Id.ToString());

                    //Cargar Objeto FactuElec
                    oNotaCredito.PtoVtaAfip = ptoVtaFormatoAfip;
                    oNotaCredito.FechaEmisionAfip = FechaDTP.Value;
                    oNotaCredito.DescTipoCbteAfip = cm.Desc;
                    oNotaCredito.CodTipoCbteAfip = cm.Id;
                    oNotaCredito.NroCbteAfip = nroCbteFormatoAfip;
                    oNotaCredito.TipoDocAfip = doctipo.Desc;
                    oNotaCredito.NroDocAfip = DocTX.Text;
                    oNotaCredito.RazonSocialAFIP = txtRazonSocial.Text;
                    oNotaCredito.CondicionIvaAFIP = esFacturaA ? "Responsable Inscripto" : comboIva.SelectedText;
                    oNotaCredito.DomicilioAFIP = txtDomicilio.Text;
                    oNotaCredito.CondicionVenta = oVentaE.EnCtaCte ? "Cuenta Corriente" : "Contado";
                    oNotaCredito.FormaPago = txtFormaPago.Text;
                    oNotaCredito.CAE1 = txtCAE.Text;
                    oNotaCredito.FecVtoCAE = txtVTO.Text;
                    oNotaCredito.ImporteNetoGravado = Utilidades.Util_Form.convertFloat(det.ImpNeto.ToString("F2"), false);
                    oNotaCredito.Iva = Utilidades.Util_Form.convertFloat(det.ImpIVA.ToString("F2"), false);
                    oNotaCredito.ImporteTotal = Utilidades.Util_Form.convertFloat(det.ImpTotal.ToString("F2"), false);
                    oNotaCredito.IdVenta = oVentaE.IdVenta;

                    ///TODO: pendiente -> agregar campo de Alicuota en tabla FacturaElectronica para registra los importes de cada Alicuota
                    oVentaN.addOrEditFactuElec(oNotaCredito);

                    DialogResult imprimir = MessageBox.Show("La Nota de Credito se generó correctamente!.\n\n¿Imprimir ticket?.",
                       "Imprimir Ticket", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1);
                    Resultado.Text = "Hora Mensaje: " + DateTime.Now.ToLocalTime() +
                        "\n\n || La Nota de Credito se genero correctamente." + "\n || Importe: $ " + TotalTx.Text;
                    #region mostrar datos en AreaText
                    string salto = "\r\n";
                    #endregion

                    imprimirTicket(esFacturaA, imprimir);


                    ///crear la Venta para Anular la original por la Nota de Credito
                    ///
                    string errorVenta = "\n-Error al registrar la Venta", errorLinea = "\n-Error al registrar las Lineas de Venta",
                    errorEgresoCaja = "\n-Error al generar el Egreso de caja", errorCtaCte = "\n-Error al generar registro de Cta Cte.";
                    try
                    {
                        Entidades.Venta oVentaAnularPorNotaCredito = oVentaE;
                        oVentaAnularPorNotaCredito.IdVenta = 0;
                        oVentaAnularPorNotaCredito.NroRemito += "Nota Credito";
                        oVentaAnularPorNotaCredito.Observaciones += "**Venta anulada por Nota de Credito**";
                        oVentaAnularPorNotaCredito.IdVenta = oVentaN.agregarVenta(oVentaAnularPorNotaCredito);
                        errorVenta = "-La Venta se registró ok.";
                        for (int i = 0; i < oVentaAnularPorNotaCredito.LineasVenta.Count; i++)
                        {
                            oVentaAnularPorNotaCredito.LineasVenta[i].Venta = oVentaAnularPorNotaCredito;
                            oVentaAnularPorNotaCredito.LineasVenta[i].CantKg *= -1;
                            oVentaAnularPorNotaCredito.LineasVenta[i].KgsTotalCalculado *= -1;
                            oVentaN.agregarLineaVenta(oVentaAnularPorNotaCredito.LineasVenta[i]);
                        }
                        errorLinea = "-Las lineas de venta se registraron ok.";
                        oVentaN.egresoCajaPagoTarjeta(oVentaAnularPorNotaCredito.IdVenta, oVentaAnularPorNotaCredito.Vendedor, oVentaAnularPorNotaCredito.PagoMixtoEfectivo);
                        errorEgresoCaja = "-El egreso de caja se registró ok.";
                        oVentaN.crearMovCtaCteVenta(oVentaAnularPorNotaCredito);
                        errorCtaCte = "-El registro de Cta Cte ok.";
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message + "\nMensajes\n" + errorVenta + errorLinea + errorEgresoCaja + errorCtaCte);
                        return;
                    }

                    if (esShowDialog)
                        this.Close();

                    this.SendToBack();
                }
                else
                {
                    ///Si factura electronica es rechazada guarda en la base de datos con el mensaje de error
                    ///
                    MessageBox.Show("Hubo un error al generar la factura\n\n" + mensajeError, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Resultado.Text = "Hora Mensaje: " + DateTime.Now.ToLocalTime() + "\n *************** \n Mensaje Error: " + mensajeError;
                    oNotaCredito.Error = true;
                    oNotaCredito.MensajeError = mensajeError;
                    oNotaCredito.FechaError = DateTime.Now;
                    oVentaN.addOrEditFactuElec(oNotaCredito);
                }
                limpiarCampos(service);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            #endregion
        }

        private string ConvertirMontoEnTexto(decimal monto)
        {
            if (monto == 0)
                return "Cero";

            string[] unidades = { "", "uno", "dos", "tres", "cuatro", "cinco", "seis", "siete", "ocho", "nueve" };
            string[] decenas = { "", "diez", "veinte", "treinta", "cuarenta", "cincuenta", "sesenta", "setenta", "ochenta", "noventa" };
            string[] especiales = { "diez", "once", "doce", "trece", "catorce", "quince", "dieciséis", "diecisiete", "dieciocho", "diecinueve" };
            string[] centenas = { "", "cien", "doscientos", "trescientos", "cuatrocientos", "quinientos", "seiscientos", "setecientos", "ochocientos", "novecientos" };

            int millones = (int)(monto / 1000000);
            monto %= 1000000;
            int miles = (int)(monto / 1000);
            monto %= 1000;
            int cientos = (int)monto;
            monto -= cientos;

            int centavos = (int)((monto - Math.Truncate(monto)) * 100);

            string resultado = "Son pesos ";

            if (millones > 0)
                resultado += (millones > 1 ? ConvertirCentena(millones, unidades, decenas, especiales, centenas) + " millones " : "un millón ");

            if (miles > 0)
                resultado += (miles > 1 ? ConvertirCentena(miles, unidades, decenas, especiales, centenas) + " mil " : "mil ");

            if (cientos > 0)
                resultado += ConvertirCentena(cientos, unidades, decenas, especiales, centenas);

            if (centavos > 0)
                resultado += " con " + ConvertirCentena(centavos, unidades, decenas, especiales, centenas) + " centavos";

            return resultado.Trim();
        }

        private void formFacturaElectronica_FormClosed(object sender, FormClosedEventArgs e)
        {
            focoEnFormPOS();
        }

        private void focoEnFormPOS()
        {
            if (onCloseCallback != null)
                onCloseCallback();
        }

        private string ConvertirCentena(int numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
        {
            if (numero == 0) return "";

            string texto = "";

            if (numero > 99)
            {
                if (numero == 100)
                {
                    texto = "cien";
                }
                else
                {
                    texto = centenas[numero / 100] + " ";
                    numero %= 100;
                }
            }

            if (numero > 19)
            {
                texto += decenas[numero / 10] + (numero % 10 > 0 ? " y " + unidades[numero % 10] : "");
            }
            else if (numero >= 10)
            {
                texto += especiales[numero - 10];
            }
            else if (numero > 0)
            {
                texto += unidades[numero];
            }

            return texto.Trim();
        }

        ///
        /// Crea formulario llamador
        ///
        private DialogResult MostrarMensaje()
        {
            // Crear Formulario Dinámico
            System.Windows.Forms.Form msgBox = new System.Windows.Forms.Form()
            {
                Text = "Confirmación",
                Size = new Size(350, 200), // Asegurar un tamaño adecuado
                StartPosition = FormStartPosition.CenterScreen,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                KeyPreview = true, // Permite capturar teclas antes de que lleguen a los controles
               // Icon = SystemIcons.Question, // Icono de pregunta
                AutoSize = false, // Desactivar AutoSize
                Padding = new Padding(9) // Agregar un poco de espacio interior
            };

            // Panel para alinear el icono y el texto
            System.Windows.Forms.FlowLayoutPanel panelTexto = new System.Windows.Forms.FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Dock = DockStyle.Top, // Ubicación en la parte superior
                Padding = new Padding(10),
                Height = 60 // Ajustamos un tamaño fijo para el panel de texto
            };

            // Icono de pregunta
            System.Windows.Forms.PictureBox pictureBox = new System.Windows.Forms.PictureBox()
            {
                Image = SystemIcons.Question.ToBitmap(),
                SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize,
                Margin = new Padding(0, 0, 5, 0) // Separación entre el icono y el texto
            };

            // Texto
            System.Windows.Forms.Label label = new System.Windows.Forms.Label()
            {
                Text = "La Factura Electrónica se generó correctamente!.\n\n¿Imprimir ..?",
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 9, FontStyle.Regular) // Aumentar el tamaño del text
            };
            // Agregar el icono y el texto al contenedor
            //panelTexto.Controls.Add(pictureBox);
            panelTexto.Controls.Add(label);

            // Contenedor para alinear los botones
            System.Windows.Forms.FlowLayoutPanel panelBotones = new System.Windows.Forms.FlowLayoutPanel()
            {
                FlowDirection = FlowDirection.LeftToRight,
                Dock = DockStyle.Bottom, // Ubicación en la parte inferior
                Padding = new Padding(10),
                Height = 60, // Ajustamos un tamaño fijo para los botones
                AutoSize = true,
                WrapContents = false, // Evitar que los botones se envuelvan
            };

            // Botones con separación de 3 mm (3 píxeles)
            System.Windows.Forms.Button btnNo = new System.Windows.Forms.Button() { Text = "No", Width = 80, Margin = new Padding(3) };
            System.Windows.Forms.Button btnTicket = new System.Windows.Forms.Button() { Text = "Ticket", Width = 80, Margin = new Padding(3) };
            System.Windows.Forms.Button btnPDF = new System.Windows.Forms.Button() { Text = "PDF", Width = 80, Margin = new Padding(3) };

            // Seleccionar por defecto el botón "No"
            msgBox.AcceptButton = btnNo;
            btnNo.Select();

            string resultado = "No"; // Valor predeterminado

            btnNo.Click += (s, e) => { resultado = "No"; msgBox.DialogResult = DialogResult.Cancel; msgBox.Close(); };
            btnTicket.Click += (s, e) => { resultado = "Ticket"; msgBox.DialogResult = DialogResult.OK; msgBox.Close(); };
            btnPDF.Click += (s, e) => { resultado = "PDF"; msgBox.DialogResult = DialogResult.OK; msgBox.Close(); };

            // Manejar la tecla Enter globalmente
            msgBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    btnNo.PerformClick();
                    e.SuppressKeyPress = true;
                }
            };

            // Alinear los botones en el centro
            panelBotones.Controls.Add(btnNo);
            panelBotones.Controls.Add(btnTicket);
            panelBotones.Controls.Add(btnPDF);

            // Agregar controles al formulario
            msgBox.Controls.Add(panelTexto);
            msgBox.Controls.Add(panelBotones);

            // Mostrar el formulario como modal
            msgBox.ShowDialog();


            DialogResult resp = DialogResult.No;
            // Acciones según la selección
            switch (resultado)
            {
                case "Ticket":
                    resp = DialogResult.Yes;
                    break;
                case "PDF":
                    generarPDF = true;
                    break;
                case "No":
                    // No hacer nada
                    break;
            }
            return resp;
        }
    }
}
