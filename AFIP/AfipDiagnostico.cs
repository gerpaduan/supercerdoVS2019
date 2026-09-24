// "Probar conexion" de la facturacion electronica: verifica de punta a punta, SIN emitir nada, que el
// certificado de la empresa sirve para facturar. Recorre el mismo camino que GenerarFacturaService:
//   1) archivos (pfx y plantilla de login) presentes,
//   2) login en WSAA con el certificado (abre el pfx con su clave, reusa el ticket vigente o pide uno),
//   3) consulta de SOLO LECTURA al WSFE (ultimo comprobante autorizado de cada punto de venta): valida la
//      relacion "wsfe" del alias y que el punto de venta este habilitado.
// Cada paso vuelve con un resultado y una explicacion en castellano. Nunca lanza: los errores son un paso fallido.
using AFIP.WSFEHOMO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace AFIP
{
    public class PasoDiagnostico
    {
        public string Titulo { get; set; }
        public bool Ok { get; set; }
        public string Detalle { get; set; }
    }

    public class ResultadoDiagnostico
    {
        public List<PasoDiagnostico> Pasos { get; } = new List<PasoDiagnostico>();
        public bool Ok
        {
            get { return Pasos.Count > 0 && Pasos.All(p => p.Ok); }
        }
        // true si la empresa esta en homologacion (la prueba fue contra el ambiente de pruebas de AFIP).
        public bool EsHomologacion { get; set; }
    }

    public static class AfipDiagnostico
    {
        // Compatibilidad con GenerarFacturaService: mismo sufijo de URL que usa al facturar.
        private const string SufijoWsaa = "?wsdl";
        private const string SufijoWsfe = "?WSDL";

        // puntosVenta: los codigos de punto de venta AFIP de las sucursales de la empresa (distintos, > 0).
        public static ResultadoDiagnostico ProbarFacturacion(Entidades.Empresa empresa, IEnumerable<int> puntosVenta, string basePathOverride, AfipConfig config)
        {
            var resultado = new ResultadoDiagnostico();
            config = config ?? AfipConfig.PorDefecto();
            resultado.EsHomologacion = config.EsHomologacion(empresa.Entorno_HOMO_PROD);
            var endpoints = config.Para(empresa.Entorno_HOMO_PROD);

            // ---- 1) archivos
            string cuit = empresa.Cuit.ToString();
            string carpeta;
            try { carpeta = AfipRutas.Carpeta(basePathOverride, cuit); }
            catch (ArgumentException)
            {
                resultado.Pasos.Add(Paso("Datos de la empresa", false, "La empresa no tiene un CUIT válido cargado."));
                return resultado;
            }

            string rutaPfx = AfipRutas.Certificado(carpeta, empresa.NombreCertificado_pfx);
            string rutaTicket = AfipRutas.Ticket(carpeta, esPadron: false, homologacion: resultado.EsHomologacion);
            if (!File.Exists(rutaPfx))
            {
                resultado.Pasos.Add(Paso("Certificado", false, "No se encontró el archivo del certificado (" + Path.GetFileName(rutaPfx) + "). Cargalo con los pasos de esta pantalla."));
                return resultado;
            }
            if (!File.Exists(AfipRutas.Template(carpeta)))
            {
                resultado.Pasos.Add(Paso("Plantilla de login", false, "Falta el archivo LoginTemplate.xml en la carpeta del CUIT. Al instalar un certificado desde esta pantalla se crea solo."));
                return resultado;
            }
            resultado.Pasos.Add(Paso("Archivos", true, "Certificado " + Path.GetFileName(rutaPfx) + " y plantilla de login presentes."));

            // ---- 2) login WSAA (abre el pfx, reusa o pide ticket)
            LoginClass login;
            try
            {
                login = new LoginClass("wsfe", endpoints.WsaaUrl + SufijoWsaa, rutaPfx, config.ClaveCertificado ?? "", rutaTicket, carpeta);
                login.HacerLogin();
                resultado.Pasos.Add(Paso("Login en ARCA (WSAA)", true,
                    "Ticket de acceso " + (resultado.EsHomologacion ? "de homologación " : "") + "vigente hasta " + login.ExpirationTime.ToString("dd/MM/yyyy HH:mm") + "."));
            }
            catch (Exception ex)
            {
                resultado.Pasos.Add(Paso("Login en ARCA (WSAA)", false, ExplicarLogin(ex, resultado.EsHomologacion)));
                return resultado;
            }

            // ---- 3) WSFE, solo lectura, un chequeo por punto de venta
            var puntos = (puntosVenta ?? Enumerable.Empty<int>()).Where(p => p > 0).Distinct().ToList();
            if (puntos.Count == 0)
            {
                resultado.Pasos.Add(Paso("Punto de venta", false, "Ninguna sucursal tiene punto de venta de AFIP cargado."));
                return resultado;
            }

            int tipoComprobante = empresa.EsRRII ? Entidades.FacturaElectronica.codFacturaB_Afip : Entidades.FacturaElectronica.codFacturaC_Afip;
            foreach (int puntoVenta in puntos)
            {
                string titulo = "Facturación electrónica (WSFE) - punto de venta " + puntoVenta;
                try
                {
                    var auth = new FEAuthRequest { Cuit = empresa.Cuit, Token = login.Token, Sign = login.Sign };
                    var servicio = new Service();
                    servicio.Url = endpoints.WsfeUrl + SufijoWsfe;
                    servicio.ClientCertificates.Add(login.Certificado);

                    var ultimo = servicio.FECompUltimoAutorizado(auth, puntoVenta, tipoComprobante);
                    if (ultimo != null && ultimo.Errors != null && ultimo.Errors.Any())
                    {
                        string errores = string.Join(" | ", ultimo.Errors.Select(e => e.Code + ": " + e.Msg));
                        resultado.Pasos.Add(Paso(titulo, false, "ARCA respondió con error: " + errores + ". " + PistaWsfe(errores, puntoVenta)));
                    }
                    else
                    {
                        resultado.Pasos.Add(Paso(titulo, true, "ARCA respondió bien (último comprobante autorizado: " + (ultimo != null ? ultimo.CbteNro : 0) + "). No se emitió nada."));
                    }
                }
                catch (Exception ex)
                {
                    resultado.Pasos.Add(Paso(titulo, false, ConsultarPadronService.TraducirMensajeError(ex)));
                }
            }

            return resultado;
        }

        private static PasoDiagnostico Paso(string titulo, bool ok, string detalle)
        {
            return new PasoDiagnostico { Titulo = titulo, Ok = ok, Detalle = detalle };
        }

        // Causa probable de un fallo en el login segun el error, en lenguaje de usuario.
        private static string ExplicarLogin(Exception ex, bool homologacion)
        {
            var cadena = new List<Exception>();
            for (var e = ex; e != null; e = e.InnerException) cadena.Add(e);
            string texto = string.Join(" ", cadena.Select(e => e.Message)).ToLowerInvariant();

            if (cadena.Any(e => e is CryptographicException))
                return "No se pudo abrir el certificado: la clave no corresponde o el archivo está dañado. Cargá el certificado de nuevo.";
            if (texto.Contains("alreadyauthenticated") || texto.Contains("ya posee un ta") || texto.Contains("ta valido"))
                return "ARCA indica que ya hay un ticket de acceso vigente y no entrega otro. Esperá unos minutos y reintentá.";
            if (texto.Contains("certificado digital no es v") || texto.Contains("cert.path") || texto.Contains("certificate") && texto.Contains("expired") || texto.Contains("vencid"))
                return "ARCA rechazó el certificado (vencido o inválido)." + (homologacion ? " La empresa está en modo PRUEBA: necesita el certificado de homologación." : " Si la empresa está en producción, verificá que no sea un certificado de homologación.");
            if (texto.Contains("computador fiscal") || texto.Contains("no autorizado") || texto.Contains("no existe"))
                return "ARCA no reconoce el alias o el servicio no está autorizado. Revisá la relación 'Facturación Electrónica (wsfe)' del alias en ARCA. " + ConsultarPadronService.TraducirMensajeError(ex);
            return ConsultarPadronService.TraducirMensajeError(ex);
        }

        private static string PistaWsfe(string errores, int puntoVenta)
        {
            string texto = errores.ToLowerInvariant();
            if (texto.Contains("relacion") || texto.Contains("relación") || texto.Contains("autoriz"))
                return "Probablemente falta la relación 'Facturación Electrónica (wsfe)' del alias en ARCA (Administrador de Relaciones de Clave Fiscal).";
            if (texto.Contains("punto") || texto.Contains("pto"))
                return "Verificá que el punto de venta " + puntoVenta + " esté habilitado como 'RECE para aplicativo y web services'.";
            return "Verificá la relación 'Facturación Electrónica (wsfe)' del alias y que el punto de venta " + puntoVenta + " esté habilitado para web services.";
        }
    }
}
