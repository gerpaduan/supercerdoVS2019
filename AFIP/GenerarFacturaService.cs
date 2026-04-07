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

        private readonly LoginClass login;
        private readonly string servicioAfip = "wsfe";
        private readonly string clave = "";

        //"Producción"
        private readonly string urlLogin = "https://wsaa.afip.gov.ar/ws/services/LoginCms?wsdl";
        private readonly string urlWSFE = "https://servicios1.afip.gov.ar/wsfev1/service.asmx?WSDL";

        // privados / contexto
        private readonly Venta _ventaCtx;
        private readonly bool esRRII;
        private readonly string cuit;
        private readonly int ptoVtaAfip;

        public GenerarFacturaService(Entidades.Venta venta)
        {
            if (venta == null) throw new ArgumentNullException(nameof(venta));
            if (venta.Sucursal == null) throw new ArgumentException("Venta sin Sucursal", nameof(venta));
            if (venta.Sucursal.Empresa == null) throw new ArgumentException("Venta sin Empresa", nameof(venta));

            _ventaCtx = venta;

            // Compatibilidad: EsRRII puede venir bool o int (0/1)
            esRRII = Convert.ToBoolean(venta.Sucursal.Empresa.EsRRII);

            cuit = venta.Sucursal.Empresa.Cuit.ToString();
            ptoVtaAfip = venta.Sucursal.CodPuntoVentaAfip;

            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AFIP",
                cuit
            );

            string rutaCertificado = Path.Combine(
                basePath,
                venta.Sucursal.Empresa.NombreCertificado_pfx
            );

            string rutaTA = Path.Combine(
                basePath,
                "TicketAcceso.txt"
            );

            login = new LoginClass(
                servicioAfip,
                urlLogin,
                Path.Combine(rutaCertificado),
                "",
                Path.Combine(rutaTA),
                basePath
            );

            login.HacerLogin();
        }

        // =========================
        // Helpers
        // =========================

        private static (double pct, double factor) GetPorcentajeFactor(Entidades.FacturaElectronica factura)
        {
            double pct = 100;

            try
            {
                pct = Convert.ToDouble(factura?.PorcentajeFacturacion ?? 100f);
            }
            catch
            {
                pct = 100;
            }

            // Política tolerante: si viene 0 o negativo, asumimos 100
            if (pct <= 0) pct = 100;

            // Clamp
            if (pct > 100) pct = 100;
            if (pct < 0.01) pct = 0.01;

            return (pct, pct / 100.0);
        }

        private static string OnlyDigits(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "";
            var chars = s.Where(char.IsDigit).ToArray();
            return new string(chars);
        }

        private static long ToLongOr0(string s)
        {
            var d = OnlyDigits(s);
            long n;
            return long.TryParse(d, out n) ? n : 0;
        }

        private static int ToIntOr(int fallback, string s)
        {
            int n;
            return int.TryParse(OnlyDigits(s), out n) ? n : fallback;
        }

        private static void CalcularFiscalAfipConPorcentaje(
            FECAEDetRequest det,
            Entidades.FacturaElectronica factura,
            bool informaIva,
            double factor,
            out double impNeto,
            out double impIva,
            out double impTotal,
            out List<Entidades.AlicuotaIva> listaAlicuotas
        )
        {
            // Variables locales (NO out dentro de lambdas)
            var alicuotasLocal = new List<Entidades.AlicuotaIva>();

            det.ImpTotConc = 0;
            det.ImpOpEx = 0;
            det.ImpTrib = 0;

            det.MonId = "PES";
            det.MonCotiz = 1;
            det.MonCotizSpecified = true;

            det.ImpNeto = 0;
            det.ImpIVA = 0;
            det.Iva = null;

            impNeto = 0;
            impIva = 0;
            impTotal = 0;

            var lineas = factura?.Venta?.LineasVenta ?? new List<Entidades.LineaVenta>();
            if (lineas.Count == 0)
            {
                det.ImpTotal = 0;
                listaAlicuotas = alicuotasLocal;
                return;
            }

            // Total “IVA incluido” de la venta, escalado por porcentaje
            double totalVenta = lineas.Sum(l => l.CantKg * l.PrecioKg) * factor;

            // ===============================
            // IVA DISCRIMINADO (RRII)
            // ===============================
            if (informaIva && lineas.Any(l => l.AlicuotaIva > 0))
            {
                var ivaWsList = new List<AFIP.WSFEHOMO.AlicIva>();

                var grupos = lineas
                    .Where(l => l.AlicuotaIva > 0)
                    .GroupBy(l => new { l.IdAlicuotaIva, l.AlicuotaIva })
                    .ToList();

                double netoAcum = 0;
                double ivaAcum = 0;

                foreach (var g in grupos)
                {
                    double totalGrupo = g.Sum(x => x.CantKg * x.PrecioKg) * factor;

                    double baseImp = Math.Round(
                        totalGrupo / (1 + g.Key.AlicuotaIva / 100.0),
                        2, MidpointRounding.AwayFromZero);

                    double iva = Math.Round(
                        totalGrupo - baseImp,
                        2, MidpointRounding.AwayFromZero);

                    netoAcum += baseImp;
                    ivaAcum += iva;

                    // WSFE
                    ivaWsList.Add(new AFIP.WSFEHOMO.AlicIva
                    {
                        Id = (int)Math.Round(g.Key.IdAlicuotaIva, 0, MidpointRounding.AwayFromZero),
                        BaseImp = baseImp,
                        Importe = iva
                    });

                    // Persistencia
                    alicuotasLocal.Add(new Entidades.AlicuotaIva
                    {
                        IdIva = (int)Math.Round(g.Key.IdAlicuotaIva, 0, MidpointRounding.AwayFromZero),
                        Iva = (float)g.Key.AlicuotaIva,
                        BaseImponible = (float)Math.Round(baseImp, 2, MidpointRounding.AwayFromZero),
                        Importe = (float)Math.Round(iva, 2, MidpointRounding.AwayFromZero)
                    });
                }

                det.Iva = ivaWsList.ToArray();

                det.ImpNeto = Math.Round(netoAcum, 2, MidpointRounding.AwayFromZero);
                det.ImpIVA = Math.Round(ivaAcum, 2, MidpointRounding.AwayFromZero);

                impNeto = det.ImpNeto;
                impIva = det.ImpIVA;
            }
            else
            {
                // RRII pero sin IVA > 0: tratamos todo como neto
                det.ImpNeto = Math.Round(totalVenta, 2, MidpointRounding.AwayFromZero);
                det.ImpIVA = 0;

                impNeto = det.ImpNeto;
                impIva = 0;
            }

            // ===============================
            // IVA INCLUIDO (NO RRII) - B/C
            // ===============================
            if (!informaIva)
            {
                det.ImpNeto = Math.Round(totalVenta, 2, MidpointRounding.AwayFromZero);
                det.ImpIVA = 0;

                impNeto = det.ImpNeto;
                impIva = 0;
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

            impTotal = det.ImpTotal;

            // Asignar OUT al final
            listaAlicuotas = alicuotasLocal;
            factura.ListaAlicuota = alicuotasLocal;
        }

        /// <summary>
        /// Genera factura en AFIP a partir de la factura suministrada (debe tener Venta cargada o se usa la Venta del ctor).
        /// Aplica factura.PorcentajeFacturacion SIN MODIFICAR la Venta: solo escala los importes calculados.
        /// </summary>
        public AfipResult GenerarFactura(Entidades.FacturaElectronica factura, bool esNotaCredito = false)
        {
            var result = new AfipResult() { Ok = false };

            try
            {
                if (factura == null) throw new ArgumentNullException(nameof(factura));

                // Si no viene la Venta cargada en la factura, usamos la del constructor
                if (factura.Venta == null)
                    factura.Venta = _ventaCtx;

                if (factura.Venta?.Persona == null)
                    throw new InvalidOperationException("La factura debe tener Venta y Persona cargadas.");

                if (factura.Venta.LineasVenta == null || factura.Venta.LineasVenta.Count == 0)
                    throw new InvalidOperationException("La venta no tiene líneas.");

                // Porcentaje/factor (NO TOCAR VENTA)
                var (pct, factor) = GetPorcentajeFactor(factura);

                // 1) Auth request
                var auth = new FEAuthRequest
                {
                    Cuit = Convert.ToInt64(cuit),
                    Token = login.Token,
                    Sign = login.Sign
                };

                // 2) Instanciar servicio
                var service = new Service();
                service.Url = urlWSFE;
                service.ClientCertificates.Add(login.Certificado);

                // 3) Cabecera FECAECabRequest
                var cab = new FECAECabRequest
                {
                    CantReg = 1,
                    PtoVta = ptoVtaAfip,
                    CbteTipo = factura.CodTipoCbteAfip
                };

                // 4) Detalle FECAEDetRequest
                var det = new FECAEDetRequest();

                // Concepto 1 = productos
                det.Concepto = 1;

                // Doc tipo/nro: prioriza lo que venga en FacturaElectronica, si no usa persona
                int tipoDocFallback = factura.Venta.Persona.ConsumidorFinal ? 
                    Entidades.FacturaElectronica.codTipoDoc_SinIdentif
                    : Entidades.FacturaElectronica.codTipoDoc_CUIT;

                int tipoDoc = !string.IsNullOrWhiteSpace(factura.TipoDocAfip)
                    ? ToIntOr(tipoDocFallback, factura.TipoDocAfip)
                    : tipoDocFallback;

                long nroDoc = !string.IsNullOrWhiteSpace(factura.NroDocAfip)
                    ? ToLongOr0(factura.NroDocAfip)
                    : ToLongOr0(factura.Venta.Persona.Cuit);

                det.DocTipo = tipoDoc;
                det.DocNro = nroDoc;

                // Condición IVA receptor (AFIP)
                var factTemp = new FacturaElectronica();
                det.CondicionIVAReceptorId = factTemp.MapearCondicionIVAReceptorIdAfip(factura.Venta.Persona.IdIva);

                // CBTE nro -> recuperar ultimo
                var ultimo = service.FECompUltimoAutorizado(auth, ptoVtaAfip, factura.CodTipoCbteAfip);
                var next = ultimo.CbteNro + 1;
                det.CbteDesde = next;
                det.CbteHasta = next;

                // Fecha comprobante AAAAMMDD
                var fch = factura.FechaEmisionAfip.HasValue ? factura.FechaEmisionAfip.Value : DateTime.Now;
                det.CbteFch = fch.ToString("yyyyMMdd");

                // ===============================
                // CALCULO FISCAL AFIP + PORCENTAJE
                // ===============================
                bool informaIva = esRRII;

                double impNeto, impIva, impTotal;
                List<Entidades.AlicuotaIva> listaAlicuotasFactura;

                CalcularFiscalAfipConPorcentaje(
                    det,
                    factura,
                    informaIva,
                    factor,
                    out impNeto,
                    out impIva,
                    out impTotal,
                    out listaAlicuotasFactura
                );

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
                    mensajeError += (string.IsNullOrEmpty(mensajeError) ? "" : " | ") +
                                   string.Join(" | ", r.FeDetResp[0].Observaciones.Select(o => $"{o.Code}: {o.Msg}"));

                // Resultado
                if (r.FeCabResp != null && r.FeCabResp.Resultado == "A")
                {
                    var detResp = r.FeDetResp[0];

                    // Formatear ptoVta y nroCbte
                    string ptoVtaFormato = (ptoVtaAfip + 100000).ToString().Substring(1);
                    string nroCbteFormato = (detResp.CbteDesde + 100000000).ToString().Substring(1);

                    var fact = new FacturaElectronica
                    {
                        PtoVtaAfip = ptoVtaFormato,
                        FechaEmisionAfip = fch,
                        DescTipoCbteAfip = (esNotaCredito ? "Nota Credito " : "Factura ") + factura.getLetraId_TipoCbte(factura.CodTipoCbteAfip), //factura.DescTipoCbteAfip ?? "",
                        CodTipoCbteAfip = factura.CodTipoCbteAfip,
                        NroCbteAfip = nroCbteFormato,

                        TipoDocAfip = det.DocTipo.ToString(),
                        NroDocAfip = det.DocNro > 0 ? det.DocNro.ToString() : "",
                        RazonSocialAFIP = factura.Venta.Persona?.razonSocial ?? "",
                        CondicionIvaAFIP = factura.CondicionIvaAFIP,//factura.Venta.Persona?.Iva ?? "",
                        DomicilioAFIP = factura.DomicilioAFIP,// factura.Venta.Persona != null ? $"{factura.Venta.Persona.Domicilio} - {factura.Venta.Persona.Ciudad}" : "",

                        CondicionVenta = factura.Venta.EnCtaCte ? "Cuenta Corriente" : "Contado",
                        FormaPago = factura.FormaPago, //factura.Venta.FormaPago ?? "",

                        CAE1 = detResp.CAE,
                        FecVtoCAE = detResp.CAEFchVto,

                        ImporteNetoGravado = (float)Math.Round(impNeto, 2, MidpointRounding.AwayFromZero),
                        Iva = (float)Math.Round(impIva, 2, MidpointRounding.AwayFromZero),
                        ImporteTotal = (float)Math.Round(impTotal, 2, MidpointRounding.AwayFromZero),

                        IdVenta = factura.Venta.IdVenta,
                        PorcentajeFacturacion = (float)pct
                    };

                    // IVA discriminado para persistencia (si hay)
                    if (listaAlicuotasFactura != null && listaAlicuotasFactura.Count > 0)
                        fact.ListaAlicuota.AddRange(listaAlicuotasFactura.Where(a => a.Importe > 0));

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