// Trae de ARCA los puntos de venta habilitados para facturar por web services de UN CUIT (WSFE FEParamGetPtosVenta,
// solo lectura): titulares fiscales (2026-10-09, ver docs/DECISIONS.md). Usa el certificado de ese CUIT (carpeta
// AFIP/<cuit>/<entorno>/), asi que solo funciona con el certificado ya cargado. Nunca lanza: los errores vuelven en
// Resultado.Mensaje. Solo net10 (comparte el shim WCF de ServiceReferenceCore; ver AFIP.csproj).
using AFIP.WSFEHOMO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AFIP
{
    public static class ConsultarPuntosVentaService
    {
        private const string SufijoWsaa = "?wsdl";
        private const string SufijoWsfe = "?WSDL";

        public class PuntoVentaArca
        {
            public int Codigo { get; set; }
            public string TipoEmision { get; set; }
        }

        public class Resultado
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
            public List<PuntoVentaArca> PuntosVenta { get; } = new List<PuntoVentaArca>();
        }

        // Un punto de venta esta activo si no esta bloqueado y no tiene fecha de baja. ARCA informa "N"/"S" en
        // Bloqueado y, sin baja, FchBaja vacio, nulo o el texto "NULL" (PENDIENTE verificar con ARCA real).
        public static bool EsActivo(string bloqueado, string fchBaja)
        {
            bool estaBloqueado = string.Equals((bloqueado ?? "").Trim(), "S", StringComparison.OrdinalIgnoreCase)
                || string.Equals((bloqueado ?? "").Trim(), "SI", StringComparison.OrdinalIgnoreCase)
                || string.Equals((bloqueado ?? "").Trim(), "TRUE", StringComparison.OrdinalIgnoreCase);
            if (estaBloqueado) return false;

            string baja = (fchBaja ?? "").Trim();
            bool tieneBaja = baja.Length > 0 && !string.Equals(baja, "NULL", StringComparison.OrdinalIgnoreCase);
            return !tieneBaja;
        }

        public static List<PuntoVentaArca> FiltrarActivos(IEnumerable<AFIP.WSFECore.PtoVenta> puntos)
        {
            return (puntos ?? Enumerable.Empty<AFIP.WSFECore.PtoVenta>())
                .Where(p => p != null && p.Nro > 0 && EsActivo(p.Bloqueado, p.FchBaja))
                .OrderBy(p => p.Nro)
                .Select(p => new PuntoVentaArca { Codigo = p.Nro, TipoEmision = p.EmisionTipo })
                .ToList();
        }

        public static Resultado Consultar(long cuit, bool homologacion, string basePathOverride, AfipConfig config, string nombreCertificadoPfx)
        {
            var resultado = new Resultado();
            try
            {
                config = config ?? AfipConfig.PorDefecto();
                var endpoints = config.Para(homologacion ? AfipEntorno.Homologacion : AfipEntorno.Produccion);

                var ubicacion = AfipRutas.Resolver(basePathOverride, cuit.ToString(), homologacion, nombreCertificadoPfx);
                if (!File.Exists(ubicacion.RutaPfx))
                {
                    resultado.Mensaje = "Todavía no hay certificado de " + (homologacion ? "pruebas (homologación)" : "producción")
                        + " cargado para este CUIT. Cargalo en Certificado ARCA y volvé a intentar.";
                    return resultado;
                }
                AfipRutas.AsegurarPlantilla(ubicacion);

                var login = new LoginClass("wsfe", endpoints.WsaaUrl + SufijoWsaa, ubicacion.RutaPfx, config.ClaveCertificado ?? "",
                    ubicacion.RutaTicket(false), ubicacion.Carpeta);
                login.HacerLogin();

                var auth = new AFIP.WSFECore.FEAuthRequest { Cuit = cuit, Token = login.Token, Sign = login.Sign };
                var servicio = new Service();
                servicio.Url = endpoints.WsfeUrl + SufijoWsfe;
                servicio.ClientCertificates.Add(login.Certificado);

                var respuesta = servicio.FEParamGetPtosVenta(auth);
                if (respuesta != null && respuesta.Errors != null && respuesta.Errors.Any())
                {
                    resultado.Mensaje = "ARCA respondió con error: " + string.Join(" | ", respuesta.Errors.Select(e => e.Code + ": " + e.Msg));
                    return resultado;
                }

                resultado.PuntosVenta.AddRange(FiltrarActivos(respuesta == null ? null : respuesta.ResultGet));
                resultado.Ok = true;
                resultado.Mensaje = resultado.PuntosVenta.Count == 0
                    ? "ARCA no informa puntos de venta habilitados para facturar por web services para este CUIT."
                    : "Puntos de venta recuperados desde ARCA.";
                return resultado;
            }
            catch (Exception ex)
            {
                resultado.Ok = false;
                resultado.Mensaje = ConsultarPadronService.TraducirMensajeError(ex);
                return resultado;
            }
        }
    }
}
