using AFIP.WSFEHOMO;
using Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AFIP
{
    public class GenerarFacturaService
    {
        public class AfipResult
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
            public FacturaElectronica Factura { get; set; }
            public FECAEResponse RawResponse { get; set; }
        }

        private LoginClass login;
        private readonly string servicioAfip = "wsfe";
        private readonly string clave = "";

        //"Producción"
        private readonly string urlLogin = "https://wsaa.afip.gov.ar/ws/services/LoginCms?wsdl";
        private readonly string urlWSFE = "https://servicios1.afip.gov.ar/wsfev1/service.asmx?WSDL";

        ///"Testing"
        //private readonly string urlLogin = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        //private readonly string urlWSFE = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?WSDL";
        //private readonly string urlWSPN = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13?WSDL";
        private readonly string certificadoPath;

        //TODO: obtener de Session
        bool esRRII = false;// ConfigurationManager.AppSettings["ivaCliente"].ToString().Equals("RRII");
        string cuit = "";// "20306210786";// ConfigurationManager.AppSettings["cuit"].ToString();
        private readonly int ptoVtaAfip = 0;  

        private readonly string servidorTipo;
               

        public GenerarFacturaService(Entidades.Venta venta)
        {
            //OBTENER LOS DATOS DE LA EMPRESA LOGUEADA
            esRRII = venta.Sucursal.Empresa.EsRRII == 1;// == FacturaElectronica.codRRII_IvaAfip;
            cuit = venta.Sucursal.Empresa.Cuit.ToString();
            ptoVtaAfip = venta.Sucursal.CodPuntoVentaAfip;

            ///OBTENER LOS DATOS DE LA EMPRESA LOGUEADA
            ///SE DEBERIAN RECUPERAR DE LA SESSION

            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AFIP",
                cuit//empresa.Cuit
            );

            string rutaCertificado = Path.Combine(
                basePath,
                "certif-prod.pfx"// empresa.AfipCertFileName //-- ej: AFIP\20123456789\certificado.pfx
            );

            string rutaTA = Path.Combine(
                basePath,
                "TicketAcceso.txt"
            );

            login = new LoginClass(
                servicioAfip,//"wsfe",
                urlLogin,//"https://wsaa.afip.gov.ar/ws/services/LoginCms",
                Path.Combine(rutaCertificado),
                "",//ConfigurationManager.AppSettings["ClaveCertificadoAFIP"], //la clave la mando vacia en winform
                Path.Combine(rutaTA),
                basePath
            );

            login.HacerLogin();
        }

        /// <summary>
        /// Genera factura en AFIP a partir de la venta suministrada.
        /// DEV: implementación inicial. Recomendado mejorar cálculo de neto/iva y selección tipo comprobante.
        /// </summary>
        public AfipResult GenerarFactura(Entidades.Venta venta, bool esNotaCredito = false)
        {
            var result = new AfipResult() { Ok = false };

            try
             {
                if (venta == null) throw new ArgumentNullException(nameof(venta));


                List<int> listaIdAlicuotaConIva = new List<int>();
               
                List<Entidades.AlicuotaIva> listaAlicuotasFactura = new List<Entidades.AlicuotaIva>();

                //Inicializo valores de las Base Imponible de Alicuotas
                // solo para 10.5% y 21 %
                ///< !--Alicuotas IVA->ID 3 = 0 % | ID 4 = 10.5 % | ID 5 = 21 % | ID 6 = 27 % | ID 8 = 5 % | ID 9 = 2.5 % -->
                int cantAlicuotas = 2;
                for (int i = 0; i < cantAlicuotas; i++)
                {
                    Entidades.AlicuotaIva oAli = new Entidades.AlicuotaIva();
                    oAli.IdIva = i + 4; //4 y 5
                    oAli.Iva = i == 0 ? 10.5f : 21f;
                    oAli.BaseImponible = 0;
                    oAli.Importe = 0;

                    listaAlicuotasFactura.Add(oAli);
                }

                foreach (Entidades.LineaVenta lineaE in venta.LineasVenta)
                {
                    //calculo de las Base Imponible de Alicuotas
                    for (int i = 0; i < listaAlicuotasFactura.Count; i++)
                    {
                        if (listaAlicuotasFactura[i].IdIva == lineaE.IdAlicuotaIva)
                        {
                            float totalLinea = lineaE.PrecioKg * lineaE.CantKg;
                            float divisorIva = 1 + (listaAlicuotasFactura[i].Iva / 100);
                            float baseImponibleLinea = totalLinea / divisorIva;
                            listaAlicuotasFactura[i].BaseImponible += (float)Math.Round(baseImponibleLinea, 2);
                            listaAlicuotasFactura[i].Importe += (float)Math.Round((totalLinea - baseImponibleLinea), 2);
                        }
                    }
                }                

                // 2) Auth request
                var auth = new FEAuthRequest
                {
                    Cuit = Convert.ToInt64(cuit),//TODO: obtener de Session.Empresa.cuit long.Parse(ConfigurationManager.AppSettings["cuit"] ?? "0"),
                    Token = login.Token,
                    Sign = login.Sign
                };

                // 3) Instanciar servicio
                var service = new Service();
                service.Url = urlWSFE;
                service.ClientCertificates.Add(login.Certificado);

                // 4) Elegir tipo de comprobante 

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

                ///Tabla de tipos de IVA en AFIP y BD
                //id  iva                     abrev
                //1   Consumidor Final        Cons.Final
                //2   Responsable Inscripto   Resp.Incr.
                //3   Monotributista          Monotr.
                //4   Exento                  Exento

                int codTipoCbte;
                bool clienteEsRRII = venta.Persona.IdIva == FacturaElectronica.codRRII_IvaAfip;
                codTipoCbte = esRRII ? 
                    (clienteEsRRII ? FacturaElectronica.codFacturaA_Afip : FacturaElectronica.codFacturaB_Afip ) :
                    FacturaElectronica.codFacturaC_Afip;

                if (esNotaCredito)
                { 
                    codTipoCbte = esRRII ?
                        (clienteEsRRII ? FacturaElectronica.codNotaCreditoA_Afip : FacturaElectronica.codNotaCreditoB_Afip) :
                        FacturaElectronica.codNotaCreditoC_Afip;
                }

                // 5) Cabecera FECAECabRequest
                var cab = new FECAECabRequest
                {
                    CantReg = 1,
                    PtoVta = ptoVtaAfip,                        
                    CbteTipo = codTipoCbte
                };

                // 6) Detalle FECAEDetRequest
                var det = new FECAEDetRequest();

                // Concepto 1 = productos
                det.Concepto = 1;

                // Tipo y nro doc cliente (si no existe -> 99 y 0)
                int tipoDoc = venta.Persona.ConsumidorFinal ? 99 : 80; //TODO: reemplazar el llamado a entidad por Negocio
                long nroDoc = string.IsNullOrEmpty(venta.Persona.Cuit) ? 0 : long.Parse(venta.Persona.Cuit);

                det.DocTipo = tipoDoc;
                det.DocNro = nroDoc; // o de tu cliente

                var factTemp = new FacturaElectronica();

                #region CONDICION IVA RECEPTOR - CODIGOS AFIP
                //1 = IVA Responsable Inscripto

                //4 = IVA Sujeto Exento

                //5 = Consumidor Final

                //6 = Responsable Monotributo

                //7 = Sujeto No Categorizado

                //8 = Proveedor del Exterior

                //9 = Cliente del Exterior

                //10 = IVA Liberado – Ley 19.640

                //13 = Monotributista Social

                //15 = IVA No Alcanzado

                //16 = Monotributo Trabajador Independiente Promovido
                #endregion

                det.CondicionIVAReceptorId = factTemp.MapearCondicionIVAReceptorIdAfip(venta.Persona.IdIva);

                // CBTE nro -> recuperar ultimo
                var ultimo = service.FECompUltimoAutorizado(auth, ptoVtaAfip, codTipoCbte);
                var next = ultimo.CbteNro + 1;
                det.CbteDesde = next;
                det.CbteHasta = next;

                // Fecha del comprobante en formato AAAAMMDD
                det.CbteFch = (venta.FechaVenta != DateTime.MinValue ? venta.FechaVenta : DateTime.Now).ToString("yyyyMMdd");

                // ===============================
                // CALCULO FISCAL AFIP (CORRECTO)
                // ===============================

                bool informaIva = esRRII;

                det.ImpTotConc = 0;
                det.ImpOpEx = 0;
                det.ImpTrib = 0;

                // Moneda
                det.MonId = "PES";
                det.MonCotiz = 1;
                det.MonCotizSpecified = true;

                // Inicializar
                det.ImpNeto = 0;
                det.ImpIVA = 0;
                det.Iva = null;

                // ===============================
                // FACTURA A / NOTA CREDITO A
                // ===============================
                if (informaIva &&
                    venta.LineasVenta != null &&
                    venta.LineasVenta.Any(l => l.AlicuotaIva > 0))
                {
                    var ivaArr = venta.LineasVenta
                        .GroupBy(x => new { x.IdAlicuotaIva, x.AlicuotaIva })
                        .Select(g =>
                        {
                            double total = g.Sum(x => x.CantKg * x.PrecioKg);
                            double baseImp = Math.Round(
                                total / (1 + g.Key.AlicuotaIva / 100.0),
                                2, MidpointRounding.AwayFromZero);

                            double iva = Math.Round(
                                total - baseImp,
                                2, MidpointRounding.AwayFromZero);

                            det.ImpNeto += baseImp;
                            det.ImpIVA += iva;
                            return new AlicIva
                            {
                                Id = (int)Math.Round(g.Key.IdAlicuotaIva, 0, MidpointRounding.AwayFromZero),
                                BaseImp = baseImp,
                                Importe = iva
                            };
                        })
                        .ToArray();

                    det.Iva = ivaArr;

                    det.ImpNeto = Math.Round(
                        Convert.ToDouble(det.ImpNeto),
                        2, MidpointRounding.AwayFromZero);
                    det.ImpIVA = Math.Round(
                        Convert.ToDouble(det.ImpIVA),
                        2, MidpointRounding.AwayFromZero);
                }

                // ===============================
                // FACTURA B / C (IVA INCLUIDO)
                // ===============================
                if (!informaIva)
                {
                    det.ImpNeto = Math.Round(
                        Convert.ToDouble(venta.TotalImporte),
                        2, MidpointRounding.AwayFromZero);

                    det.ImpIVA = 0;
                }

                // ===============================
                // TOTAL (AFIP EXIGE COHERENCIA)
                // ===============================
                det.ImpTotal = Math.Round(
                    det.ImpNeto +
                    det.ImpIVA +
                    det.ImpTrib +
                    det.ImpOpEx +
                    det.ImpTotConc,
                    2, MidpointRounding.AwayFromZero);

                // Armar FECAERequest
                var req = new FECAERequest
                {
                    FeCabReq = cab,
                    FeDetReq = new FECAEDetRequest[] { det }
                };

                // Llamar AFIP
                var r = service.FECAESolicitar(auth, req);

                result.RawResponse = r;

                // Mensajes / errores
                string mensajeError = "";
                if (r.Errors != null && r.Errors.Any())
                    mensajeError = string.Join(" | ", r.Errors.Select(er => $"{er.Code}: {er.Msg}"));

                if (r.FeDetResp != null && r.FeDetResp.Length > 0 && r.FeDetResp[0].Observaciones != null)
                    mensajeError += (string.IsNullOrEmpty(mensajeError) ? "" : " | ") + string.Join(" | ", r.FeDetResp[0].Observaciones.Select(o => $"{o.Code}: {o.Msg}"));

                // Resultado
                if (r.FeCabResp != null && r.FeCabResp.Resultado == "A")
                {
                    var detResp = r.FeDetResp[0];

                    // Formatear ptoVta y nroCbte como en WinForms
                    string ptoVtaFormato = (ptoVtaAfip + 100000).ToString().Substring(1);
                    string nroCbteFormato = (detResp.CbteDesde + 100000000).ToString().Substring(1);

                    var fact = new FacturaElectronica
                    {
                        PtoVtaAfip = ptoVtaFormato,
                        FechaEmisionAfip = (venta.FechaVenta != DateTime.MinValue ? venta.FechaVenta : DateTime.Now),
                        DescTipoCbteAfip = r.FeCabResp != null ? r.FeCabResp.Resultado : "",
                        CodTipoCbteAfip = codTipoCbte,
                        NroCbteAfip = nroCbteFormato,
                        TipoDocAfip = tipoDoc == 80 ? "CUIT" : "OTRO",
                        NroDocAfip = nroDoc > 0 ? nroDoc.ToString() : "",
                        RazonSocialAFIP = venta.Persona?.razonSocial ?? "",
                        CondicionIvaAFIP = venta.Persona?.Iva ?? "",
                        DomicilioAFIP = venta.Persona != null ? $"{venta.Persona.Domicilio} - {venta.Persona.Ciudad}" : "",
                        CondicionVenta = venta.EnCtaCte ? "Cuenta Corriente" : "Contado",
                        FormaPago = venta.FormaPago ?? "",
                        CAE1 = detResp.CAE,
                        FecVtoCAE = detResp.CAEFchVto,
                        ImporteNetoGravado = (float)det.ImpNeto,
                        Iva = (float)det.ImpIVA,
                        ImporteTotal = (float)det.ImpTotal,
                        IdVenta = venta.IdVenta,
                        PorcentajeFacturacion = 100
                    };
                    fact.ListaAlicuota.AddRange(listaAlicuotasFactura.Where(a => a.Importe > 0));


                    // Persistir: se delega al Negocio (tu capa) desde el controlador.
                    result.Ok = true;
                    result.Factura = fact;
                    result.Mensaje = "Factura generada correctamente";
                }
                else
                {
                    result.Ok = false;
                    result.Mensaje = "AFIP rechazó la solicitud: " + mensajeError;
                }

                return result;
            }
            catch (Exception ex)
            {
                return new AfipResult
                {
                    Ok = false,
                    Mensaje = ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "")
                };
            }
        }
        
    }
}
