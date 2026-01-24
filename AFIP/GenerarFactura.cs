using AFIP.WSFEHOMO;
using Entidades;
using System;
using System.IO;
using System.Linq;

namespace AFIP
{
    internal class GenerarFactura
    {
        public class AfipResult
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
            public FacturaElectronica Factura { get; set; }
            public FECAEResponse RawResponse { get; set; }
        }

        public class GenerarFacturaService
        {
            private readonly string servicioAfip = "wsfe";
            private readonly string clave = "";
            private readonly string urlLogin;
            private readonly string urlWSFE;
            private readonly string certificadoPath;
            private readonly int ptoVtaAfip;
            private readonly string servidorTipo;

            public GenerarFacturaService()
            {

                //servidorTipo = ConfigurationManager.AppSettings["tipoServidor"] ?? "0";
                //certificadoPath = AppDomain.CurrentDomain.BaseDirectory + (ConfigurationManager.AppSettings["rutaCertificado"] ?? "");
                //// Determinar URLs segun tipo servidor (consistente con formFacturaElectronica)
                //if (servidorTipo == "1")
                //{
                //    urlLogin = "https://wsaa.afip.gov.ar/ws/services/LoginCms?wsdl";
                //    urlWSFE = "https://servicios1.afip.gov.ar/wsfev1/service.asmx?WSDL";
                //}
                //else
                //{
                //    urlLogin = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
                //    urlWSFE = "https://wswhomo.afip.gov.ar/wsfev1/service.asmx?WSDL";
                //}

                //if (!int.TryParse(ConfigurationManager.AppSettings["ptoVtaAfip"], out ptoVtaAfip))
                //    ptoVtaAfip = Convert.ToInt32(ConfigurationManager.AppSettings["ptoVtaAfip"] ?? "1");

                ///OBTENER LOS DATOS DE LA EMPRESA LOGUEADA
                ///SE DEBERIAN RECUPERAR DE LA SESSION

                string basePath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "AFIP",
                    "20306210786"//empresa.Cuit
                );

                string rutaCertificado = Path.Combine(
                    basePath,
                    "certif-prod.pfx"// empresa.AfipCertFileName //-- ej: AFIP\20123456789\certificado.pfx
                );

                string rutaTA = Path.Combine(
                    basePath,
                    "TicketAcceso.txt"
                );


                var login = new LoginClass(
                    "wsfe",
                    "https://wsaa.afip.gov.ar/ws/services/LoginCms",
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

                    // 1) Login (usa LoginClass del proyecto wsAFIPvs2008)
                    var login = new LoginClass(servicioAfip, urlLogin, certificadoPath, clave);
                    login.hacerLogin(); // establece Token/Sign y certificado interno

                    // 2) Auth request
                    var auth = new FEAuthRequest
                    {
                        Cuit = long.Parse(ConfigurationManager.AppSettings["cuit"] ?? "0"),
                        Token = login.Token,
                        Sign = login.Sign
                    };

                    // 3) Instanciar servicio
                    var service = new Service();
                    service.Url = urlWSFE;
                    service.ClientCertificates.Add(login.certificado);

                    // 4) Elegir tipo de comprobante (heurística simple)
                    int codTipoCbte;
                    var clienteIva = venta.Persona?.Iva ?? "";
                    if (!string.IsNullOrEmpty(clienteIva) && clienteIva.ToLower().Contains("responsable"))
                        codTipoCbte = esNotaCredito ? FacturaElectronica.codNotaCreditoA_Afip : FacturaElectronica.codFacturaA_Afip;
                    else
                        codTipoCbte = esNotaCredito ? FacturaElectronica.codNotaCreditoC_Afip : FacturaElectronica.codFacturaC_Afip;

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
                    int tipoDoc = 99;
                    long nroDoc = 0;
                    if (!string.IsNullOrWhiteSpace(venta.Persona?.Cuit))
                    {
                        var s = venta.Persona.Cuit.Replace("-", "");
                        long.TryParse(s, out nroDoc);
                        // heurística tipo doc
                        tipoDoc = (s.Length == 11) ? 80 : 99;
                    }

                    det.DocTipo = tipoDoc;
                    det.DocNro = nroDoc;

                    // CBTE nro -> recuperar ultimo
                    var ultimo = service.FECompUltimoAutorizado(auth, ptoVtaAfip, codTipoCbte);
                    var next = ultimo.CbteNro + 1;
                    det.CbteDesde = next;
                    det.CbteHasta = next;

                    // Fecha del comprobante en formato AAAAMMDD
                    det.CbteFch = (venta.FechaVenta != DateTime.MinValue ? venta.FechaVenta : DateTime.Now).ToString("yyyyMMdd");

                    // Importes: implementación inicial:
                    // - para Factura C (consumidor final) enviamos todo en ImpTotal
                    // - para Factura A intentamos separar neto/iva si las alícuotas están disponibles en las líneas (básico)
                    double impTotal = Math.Round(Convert.ToDouble(venta.TotalImporte), 2);
                    det.ImpTotal = impTotal;

                    // Intento cálculo simple de neto/iva por alícuota si hay datos en líneas
                    double impNeto = 0;
                    double impIva = 0;
                    if (venta.LineasVenta != null && venta.LineasVenta.Any(l => l.AlicuotaIva > 0))
                    {
                        foreach (var l in venta.LineasVenta)
                        {
                            var totalLinea = l.CantKg * l.PrecioKg;
                            double divisor = 1.0 + (l.AlicuotaIva / 100.0);
                            var baseImp = totalLinea / divisor;
                            var ivaLinea = totalLinea - baseImp;
                            impNeto += Math.Round(baseImp, 2);
                            impIva += Math.Round(ivaLinea, 2);
                        }
                    }
                    else
                    {
                        // fallback: todo a neto (Factura C) o neto igual total (A/B may need proper calc)
                        impNeto = impTotal;
                        impIva = 0;
                    }

                    // Si tipo comprobante es A (cod 1) y hay IVA, enviar ImpNeto/ImpIVA
                    if (codTipoCbte == FacturaElectronica.codFacturaA_Afip)
                    {
                        det.ImpNeto = Math.Round(impNeto, 2);
                        det.ImpIVA = Math.Round(impIva, 2);
                    }
                    else
                    {
                        det.ImpNeto = impNeto; // puede ser igual a total
                        det.ImpIVA = impIva;
                    }

                    det.ImpTotConc = 0;
                    det.ImpOpEx = 0;
                    det.ImpTrib = 0;

                    // Moneda (pesos)
                    det.MonId = "PES";
                    det.MonCotiz = 1;

                    // Opcional: alícuotas (solo si hay lista de alícuotas calculadas)
                    if (venta.LineasVenta != null && venta.LineasVenta.Any(l => l.AlicuotaIva > 0))
                    {
                        var alicuotas = venta.LineasVenta
                            .GroupBy(x => x.IdAlicuotaIva)
                            .Select(g => new { Id = g.Key, Base = g.Sum(x => x.CantKg * x.PrecioKg) / (1 + (g.First().AlicuotaIva / 100.0)), Importe = g.Sum(x => x.CantKg * x.PrecioKg) - g.Sum(x => x.CantKg * x.PrecioKg) / (1 + (g.First().AlicuotaIva / 100.0)), Iva = g.First().AlicuotaIva })
                            .ToArray();

                        if (alicuotas.Length > 0)
                        {
                            var arr = alicuotas.Select((a, i) => new AlicIva
                            {
                                Id = a.Id,
                                BaseImp = Math.Round(a.Base, 2),
                                Importe = Math.Round(a.Importe, 2)
                            }).ToArray();
                            det.Iva = arr;
                        }
                    }

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
}
