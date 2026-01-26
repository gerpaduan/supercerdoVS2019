using AFIP.WSFEHOMO;
using Entidades;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
        private readonly string urlLogin = "https://wsaa.afip.gov.ar/ws/services/logincms";
        private readonly string urlWSFE = "https://servicios1.afip.gov.ar/wsfev1/service.asmx?WSDL";

        ///"Testing"
        //private readonly string urlLogin = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        //private readonly string urlWSFE = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?WSDL";
        //private readonly string urlWSPN = "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13?WSDL";
        private readonly string certificadoPath;

        //TODO: obtener de Session
        bool esRRII = true;// ConfigurationManager.AppSettings["ivaCliente"].ToString().Equals("RRII");
        string cuit = "20306210786";// ConfigurationManager.AppSettings["cuit"].ToString();
        private readonly int ptoVtaAfip = 6;  

        private readonly string servidorTipo;

        public GenerarFacturaService()
        {

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
                Path.Combine(rutaTA)
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
                for (int i = 0; i < listaAlicuotasFactura.Count; i++)
                {
                    listaAlicuotasFactura[i].BaseImponible = 0;
                    listaAlicuotasFactura[i].Importe = 0;
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

                //se cargan los textBox de Alicuotas
                foreach (Entidades.AlicuotaIva item in listaAlicuotasFactura)
                {
                    ///< !--Alicuotas IVA->ID 3 = 0 % | ID 4 = 10.5 % | ID 5 = 21 % | ID 6 = 27 % | ID 8 = 5 % | ID 9 = 2.5 % -->

                    //se carga la alicuota si es mayor a cero
                    if (item.Importe > 0)
                        listaIdAlicuotaConIva.Add(item.IdIva);
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
                int tipoDoc = Persona.esConsumidorFinal(venta.Persona) ? 99 : 80;
                long nroDoc = string.IsNullOrEmpty(venta.Persona.Cuit) ? 0 : long.Parse(venta.Persona.Cuit);

                det.DocTipo = tipoDoc;
                det.DocNro = nroDoc;

                // CBTE nro -> recuperar ultimo
                var ultimo = service.FECompUltimoAutorizado(auth, ptoVtaAfip, codTipoCbte);
                var next = ultimo.CbteNro + 1;
                det.CbteDesde = next;
                det.CbteHasta = next;

                // Fecha del comprobante en formato AAAAMMDD
                det.CbteFch = (venta.FechaVenta != DateTime.MinValue ? venta.FechaVenta : DateTime.Now).ToString("yyyyMMdd");

                //// Importes: implementación inicial:
                //// - para Factura C (consumidor final) enviamos todo en ImpTotal
                //// - para Factura A intentamos separar neto/iva si las alícuotas están disponibles en las líneas (básico)
                //double impTotal = Math.Round(Convert.ToDouble(venta.TotalImporte), 2);
                //det.ImpTotal = impTotal;

                //// Intento cálculo simple de neto/iva por alícuota si hay datos en líneas
                //double impNeto = 0;
                //double impIva = 0;
                //if (venta.LineasVenta != null && venta.LineasVenta.Any(l => l.AlicuotaIva > 0))
                //{
                //    foreach (var l in venta.LineasVenta)
                //    {
                //        var totalLinea = l.CantKg * l.PrecioKg;
                //        double divisor = 1.0 + (l.AlicuotaIva / 100.0);
                //        var baseImp = totalLinea / divisor;
                //        var ivaLinea = totalLinea - baseImp;
                //        impNeto += Math.Round(baseImp, 2);
                //        impIva += Math.Round(ivaLinea, 2);
                //    }
                //}
                //else
                //{
                //    // fallback: todo a neto (Factura C) o neto igual total (A/B may need proper calc)
                //    impNeto = impTotal;
                //    impIva = 0;
                //}

                //// Si tipo comprobante es A (cod 1) y hay IVA, enviar ImpNeto/ImpIVA
                //if (codTipoCbte == FacturaElectronica.codFacturaA_Afip)
                //{
                //    det.ImpNeto = Math.Round(impNeto, 2);
                //    det.ImpIVA = Math.Round(impIva, 2);
                //}
                //else
                //{
                //    det.ImpNeto = impNeto; // puede ser igual a total
                //    det.ImpIVA = impIva;
                //}

                //det.ImpTotConc = 0;
                //det.ImpOpEx = 0;
                //det.ImpTrib = 0;

                //// Moneda (pesos)
                //det.MonId = "PES";
                //det.MonCotiz = 1;

                //// Opcional: alícuotas (solo si hay lista de alícuotas calculadas)
                //if (venta.LineasVenta != null && venta.LineasVenta.Any(l => l.AlicuotaIva > 0))
                //{
                //    var alicuotas = venta.LineasVenta
                //        .GroupBy(x => x.IdAlicuotaIva)
                //        .Select(g => new { Id = g.Key, Base = g.Sum(x => x.CantKg * x.PrecioKg) / (1 + (g.First().AlicuotaIva / 100.0)), Importe = g.Sum(x => x.CantKg * x.PrecioKg) - g.Sum(x => x.CantKg * x.PrecioKg) / (1 + (g.First().AlicuotaIva / 100.0)), Iva = g.First().AlicuotaIva })
                //        .ToArray();

                //    if (alicuotas.Length > 0)
                //    {
                //        var arr = alicuotas.Select((a, i) => new AlicIva
                //        {
                //            Id = (int)a.Id,// a.Id,
                //            BaseImp = Math.Round(a.Base, 2),
                //            Importe = Math.Round(a.Importe, 2)
                //        }).ToArray();
                //        det.Iva = arr;
                //    }
                //}


                // ===============================
                // CALCULO FISCAL AFIP (CORRECTO)
                // ===============================

                bool informaIva =
                    codTipoCbte == FacturaElectronica.codFacturaA_Afip ||
                    codTipoCbte == FacturaElectronica.codNotaCreditoA_Afip;

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
                            int iddd = 4;
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
