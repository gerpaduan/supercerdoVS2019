using AFIP.WSPSA13;
using Entidades;
using System;
using System.IO;
using System.Net;
#if NET472
// SoapException solo existe en System.Web.Services (net472) -- en net10.0 el shim de
// ServiceReferenceCore\WsPsa13Compat.cs llama al servicio via WCF, los errores SOAP llegan como
// System.ServiceModel.FaultException/CommunicationException (ya cubiertos por TraducirMensajeError
// via el texto del mensaje, ver mas abajo).
using System.Web.Services.Protocols;
#endif

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
            // true si fallo la conexion o el certificado (no "el CUIT no existe"): sirve para cortar/derivar a otro certificado.
            public bool ErrorTecnico { get; set; }
        }

        private const string ServicioAfipPadron = "ws_sr_padron_a13";

        // CUIT dueno del certificado con el que se firma (el de la empresa, o el de la plataforma en el padron compartido).
        private readonly long _cuitRepresentada;
        private readonly string _urlLogin;
        private readonly string _urlPadron;
        private readonly LoginClass _login;

        // basePathOverride: opcional, solo lo usa WebCore (ver PersonasController.BuscarPadronAfipAjax),
        // mismo motivo que AFIP.GenerarFacturaService: AppDomain.CurrentDomain.BaseDirectory es el
        // root del sitio para el Web clasico (asi corre IIS las apps ASP.NET), pero para WebCore
        // (Kestrel self-hosted) es la carpeta de build (bin/Debug/net10.0), no el content root del
        // proyecto -- ahi no esta AFIP/<cuit>/. Sin override, comportamiento identico al original
        // (net472 no cambia).
        // config: opcional (URLs por entorno y clave del pfx); null = endpoints oficiales y pfx sin clave.
        // OJO: antes un Entorno_HOMO_PROD vacio se trataba como HOMOLOGACION; ahora es PRODUCCION, igual
        // que en facturacion (AfipEntorno). Las empresas con el campo vacio deben tenerlo seteado explicito.
        public ConsultarPadronService(Entidades.Empresa empresa, string basePathOverride = null, AfipConfig config = null)
        {
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            if (empresa.Cuit <= 0) throw new ArgumentException("Empresa sin CUIT válido.", nameof(empresa));

            _cuitRepresentada = empresa.Cuit;

            config = config ?? AfipConfig.PorDefecto();
            bool homologacion = config.EsHomologacion(empresa.Entorno_HOMO_PROD);
            var endpoints = config.Para(empresa.Entorno_HOMO_PROD);
            _urlLogin = endpoints.WsaaUrl;
            _urlPadron = endpoints.PadronUrl;

            // Regla de rutas compartida con facturacion (AfipRutas): AFIP/<cuit>/prod o /homo (o el pfx historico).
            var ubicacion = AfipRutas.Resolver(basePathOverride, _cuitRepresentada.ToString(), homologacion, empresa.NombreCertificado_pfx);
            AfipRutas.AsegurarPlantilla(ubicacion);
            _login = IniciarLogin(ubicacion.Carpeta, ubicacion.RutaPfx, config.ClaveCertificado ?? "");
        }

        // Con una credencial explicita: cuitRepresentada es el CUIT dueno del certificado (en el padron
        // compartido, el de la PLATAFORMA, no el de la empresa que consulta); carpeta es donde estan su pfx,
        // su LoginTemplate.xml y su ticket. Ver WebCore.Services.PadronAfipGateway.
        public ConsultarPadronService(long cuitRepresentada, string carpeta, string nombrePfx, string clave, AfipEndpoints endpoints, bool homologacion = false)
        {
            if (cuitRepresentada <= 0) throw new ArgumentException("CUIT representado inválido.", nameof(cuitRepresentada));
            if (string.IsNullOrWhiteSpace(carpeta)) throw new ArgumentException("Falta la carpeta del certificado.", nameof(carpeta));
            if (endpoints == null) throw new ArgumentNullException(nameof(endpoints));

            _cuitRepresentada = cuitRepresentada;
            _urlLogin = endpoints.WsaaUrl;
            _urlPadron = endpoints.PadronUrl;

            _login = IniciarLogin(carpeta, AfipRutas.Certificado(carpeta, nombrePfx), clave ?? "");
        }

        // Verifica los archivos, arma el LoginClass y hace el login (reusa el ticket vigente del archivo).
        private LoginClass IniciarLogin(string carpeta, string rutaCertificado, string clave)
        {
            string rutaTA = AfipRutas.Ticket(carpeta, esPadron: true);
            string rutaTemplate = AfipRutas.Template(carpeta);

            if (!File.Exists(rutaCertificado))
                throw new FileNotFoundException("No se encontró el certificado AFIP configurado para la empresa.", rutaCertificado);

            if (!File.Exists(rutaTemplate))
                throw new FileNotFoundException("No se encontró el template de login AFIP configurado para la empresa.", rutaTemplate);

            var login = new LoginClass(ServicioAfipPadron, _urlLogin, rutaCertificado, clave, rutaTA, carpeta);

            try
            {
                login.HacerLogin();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(TraducirMensajeError(ex), ex);
            }

            return login;
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
                    _cuitRepresentada,
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
                    ErrorTecnico = true,
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

#if NET472
            if (ex is SoapException || texto.Contains("soap"))
#else
            if (ex is System.ServiceModel.FaultException || texto.Contains("soap") || texto.Contains("fault"))
#endif
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
