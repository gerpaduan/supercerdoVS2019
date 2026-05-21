using AFIP.WSPSA13;
using Entidades;
using System;
using System.IO;
using System.Net;
using System.Web.Services.Protocols;

namespace AFIP
{
    public class ConsultarPadronService
    {
        public class PadronAfipResult
        {
            public bool Ok { get; set; }
            public string Mensaje { get; set; }
            public Entidades.Persona Persona { get; set; }
            public string RazonSocialAfip { get; set; }
            public string DomicilioFiscalAfip { get; set; }
            public string LocalidadAfip { get; set; }
            public string ProvinciaAfip { get; set; }
            public string EstadoClaveAfip { get; set; }
            public string ActividadPrincipalAfip { get; set; }
            public string CondicionIvaAfip { get; set; }
            public int IdIvaSugerido { get; set; }
            public personaReturn RawResponse { get; set; }
        }

        private const string ServicioAfipPadron = "ws_sr_padron_a13";

        private readonly Entidades.Empresa _empresa;
        private readonly string _cuitEmpresa;
        private readonly string _urlLogin;
        private readonly string _urlPadron;
        private readonly LoginClass _login;

        public ConsultarPadronService(Entidades.Empresa empresa)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            if (empresa.Cuit <= 0) throw new ArgumentException("Empresa sin CUIT válido.", nameof(empresa));

            _empresa = empresa;
            _cuitEmpresa = empresa.Cuit.ToString();

            bool entornoProduccion = !string.IsNullOrWhiteSpace(empresa.Entorno_HOMO_PROD)
                && empresa.Entorno_HOMO_PROD.Trim().ToUpperInvariant().Contains("PROD");

            _urlLogin = entornoProduccion
                ? "https://wsaa.afip.gov.ar/ws/services/LoginCms"
                : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

            _urlPadron = entornoProduccion
                ? "https://aws.afip.gov.ar/sr-padron/webservices/personaServiceA13"
                : "https://awshomo.afip.gov.ar/sr-padron/webservices/personaServiceA13";

            string basePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "AFIP",
                _cuitEmpresa
            );

            string nombreCertificado = string.IsNullOrWhiteSpace(empresa.NombreCertificado_pfx)
                ? "certif-prod.pfx"
                : empresa.NombreCertificado_pfx;

            string rutaCertificado = Path.Combine(basePath, nombreCertificado);
            string rutaTA = Path.Combine(basePath, "TicketAccesoPerson.txt");
            string rutaTemplate = Path.Combine(basePath, "LoginTemplate.xml");

            if (!File.Exists(rutaCertificado))
                throw new FileNotFoundException("No se encontró el certificado AFIP configurado para la empresa.", rutaCertificado);

            if (!File.Exists(rutaTemplate))
                throw new FileNotFoundException("No se encontró el template de login AFIP configurado para la empresa.", rutaTemplate);

            _login = new LoginClass(
                ServicioAfipPadron,
                _urlLogin,
                rutaCertificado,
                "",
                rutaTA,
                basePath
            );

            try
            {
                _login.HacerLogin();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(TraducirMensajeError(ex), ex);
            }
        }

        public PadronAfipResult ConsultarDatosContribuyente(string cuitPersona)
        {
            try
            {
                string cuitNormalizado = NormalizarCuit(cuitPersona);
                long cuitConsulta;
                if (string.IsNullOrWhiteSpace(cuitNormalizado) || cuitNormalizado.Length != 11 || !long.TryParse(cuitNormalizado, out cuitConsulta))
                {
                    return new PadronAfipResult
                    {
                        Ok = false,
                        Mensaje = "El CUIT consultado no es válido."
                    };
                }

                var servicePerson = new PersonaServiceA13
                {
                    Url = _urlPadron
                };
                servicePerson.ClientCertificates.Add(_login.Certificado);

                var response = servicePerson.getPersona(
                    _login.Token,
                    _login.Sign,
                    _empresa.Cuit,
                    cuitConsulta
                );

                if (response == null || response.persona == null)
                {
                    return new PadronAfipResult
                    {
                        Ok = false,
                        Mensaje = "No se encontraron datos para el CUIT ingresado.",
                        RawResponse = response
                    };
                }

                var domicilio = response.persona.domicilio != null && response.persona.domicilio.Length > 0
                    ? response.persona.domicilio[0]
                    : null;

                string razonSocial = string.Equals(response.persona.tipoPersona, "FISICA", StringComparison.OrdinalIgnoreCase)
                    ? (Convert.ToString(response.persona.apellido) + " " + Convert.ToString(response.persona.nombre)).Trim()
                    : Convert.ToString(response.persona.razonSocial);

                string domicilioFiscal = domicilio != null ? Convert.ToString(domicilio.direccion) : "";
                string localidad = domicilio != null ? Convert.ToString(domicilio.localidad) : "";
                string provincia = domicilio != null ? Convert.ToString(domicilio.descripcionProvincia) : "";
                string ciudad = string.IsNullOrWhiteSpace(localidad) && string.IsNullOrWhiteSpace(provincia)
                    ? ""
                    : (localidad + ", " + provincia).Trim().Trim(',');
                string estadoClave = Convert.ToString(response.persona.estadoClave);
                string actividadPrincipal = Convert.ToString(response.persona.descripcionActividadPrincipal);

                if (string.IsNullOrWhiteSpace(razonSocial)
                    && string.IsNullOrWhiteSpace(domicilioFiscal)
                    && string.IsNullOrWhiteSpace(ciudad)
                    && string.IsNullOrWhiteSpace(estadoClave)
                    && string.IsNullOrWhiteSpace(actividadPrincipal))
                {
                    return new PadronAfipResult
                    {
                        Ok = false,
                        Mensaje = "No se encontraron datos para el CUIT ingresado.",
                        RawResponse = response
                    };
                }

                var persona = new Entidades.Persona
                {
                    Cuit = cuitNormalizado,
                    RazonSocial = razonSocial,
                    Identificacion = razonSocial,
                    Domicilio = domicilioFiscal,
                    Ciudad = ciudad
                };

                return new PadronAfipResult
                {
                    Ok = true,
                    Mensaje = "Consulta AFIP realizada correctamente.",
                    Persona = persona,
                    RazonSocialAfip = razonSocial,
                    DomicilioFiscalAfip = domicilioFiscal,
                    LocalidadAfip = localidad,
                    ProvinciaAfip = provincia,
                    EstadoClaveAfip = estadoClave,
                    ActividadPrincipalAfip = actividadPrincipal,
                    CondicionIvaAfip = "",
                    IdIvaSugerido = 0,
                    RawResponse = response
                };
            }
            catch (Exception ex)
            {
                return new PadronAfipResult
                {
                    Ok = false,
                    Mensaje = TraducirMensajeError(ex)
                };
            }
        }

        public static string TraducirMensajeError(Exception ex)
        {
            string detalle = ObtenerDetalleTecnico(ex);
            string texto = ((ex != null ? ex.Message : "") + " " + (ex != null && ex.InnerException != null ? ex.InnerException.Message : "")).ToLowerInvariant();

            if (ex is TimeoutException || texto.Contains("timeout") || texto.Contains("timed out"))
                return "Timeout al consultar AFIP/ARCA. Revise la conexión e intente nuevamente. " + detalle;

            if (ex is WebException || texto.Contains("unable to connect") || texto.Contains("forcibly closed") || texto.Contains("connection") || texto.Contains("remote name could not be resolved"))
                return "No se pudo conectar con AFIP/ARCA. Revise la conexión o el servicio. " + detalle;

            if (ex is SoapException || texto.Contains("soap"))
                return "El servicio de AFIP/ARCA devolvió un error. " + detalle;

            if (texto.Contains("token") || texto.Contains("sign") || texto.Contains("cms") || texto.Contains("certificado") || texto.Contains("certificate") || texto.Contains("wsaa"))
                return "Token o certificado AFIP inválido o vencido. " + detalle;

            if (texto.Contains("no se encontró el certificado") || texto.Contains("no se encontro el certificado") || texto.Contains("template de login"))
                return "La configuración AFIP de la empresa está incompleta. " + detalle;

            return "Error inesperado al consultar AFIP/ARCA. " + detalle;
        }

        private static string NormalizarCuit(string cuit)
        {
            return string.IsNullOrWhiteSpace(cuit)
                ? ""
                : cuit.Trim().Replace("-", "").Replace(" ", "");
        }

        private static string ObtenerDetalleTecnico(Exception ex)
        {
            string detalle = ex != null ? ex.Message : "";
            if (ex != null && ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
                detalle = string.IsNullOrWhiteSpace(detalle) ? ex.InnerException.Message : detalle + " | " + ex.InnerException.Message;

            detalle = (detalle ?? "").Trim();
            return string.IsNullOrWhiteSpace(detalle) ? "" : "Detalle: " + detalle;
        }
    }
}
