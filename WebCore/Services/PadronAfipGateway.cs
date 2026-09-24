// Unico punto de entrada para consultar el padron de ARCA (ws_sr_padron_a13) al dar de alta un cliente.
// Orden de credenciales: (1) certificado de la PLATAFORMA (uno para todas las empresas, alias dedicado al padron),
// (2) certificado de la propia empresa, si lo tiene, (3) nada: el usuario carga los datos a mano.
// Salvaguardas (para no arriesgar bloqueos ni condiciones de carrera):
//   - cache por empresa y CUIT consultado (24 h): un reintento no vuelve a ARCA;
//   - tope de consultas por empresa por hora;
//   - corte automatico del certificado de plataforma tras N fallos tecnicos seguidos (se usa el de la empresa);
//   - ticket WSAA compartido y renovado una sola vez (AfipTicketCache, dentro de LoginClass);
//   - se registra cuantas consultas hace cada empresa y con que credencial, sin guardar el CUIT consultado.
// Interruptor general: Afip:Padron:UsarPlataforma=false. Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
using System.Collections.Concurrent;
using AFIP;
using WebCore.Helpers;

namespace WebCore.Services
{
    public interface IPadronAfipGateway
    {
        // Nunca lanza: los problemas vuelven como resultado con Ok=false y un mensaje para el usuario.
        ConsultarPadronService.PadronAfipResult Consultar(Entidades.Empresa empresa, string cuitConsulta);
    }

    // Estado y contadores del padron compartido (para la pantalla del super-admin).
    public static class PadronAfipEstadisticas
    {
        private static readonly ConcurrentDictionary<string, long> Contadores = new ConcurrentDictionary<string, long>();

        public static void Sumar(string clave) { Contadores.AddOrUpdate(clave, 1, (_, n) => n + 1); }

        public static long Obtener(string clave) { return Contadores.TryGetValue(clave, out var n) ? n : 0; }

        public static IReadOnlyDictionary<string, long> Todas() { return new Dictionary<string, long>(Contadores); }
    }

    public class PadronAfipGateway : IPadronAfipGateway
    {
        public const string OrigenCache = "cache";
        public const string OrigenPlataforma = "plataforma";
        public const string OrigenEmpresa = "empresa";
        public const string OrigenNinguno = "ninguno";
        public const string OrigenLimitada = "limitada";

        // Estado compartido entre requests, armado con la configuracion (App.config) al primer uso.
        private static readonly AfipLimitador Limitador = new AfipLimitador(AfipSettings.PadronMaxPorHoraEmpresa, TimeSpan.FromHours(1));
        private static readonly AfipCorteAutomatico CortePlataforma = new AfipCorteAutomatico(AfipSettings.PadronFallosParaCortar, TimeSpan.FromMinutes(AfipSettings.PadronMinutosCorte));
        private static readonly AfipCacheTemporal<ConsultarPadronService.PadronAfipResult> Cache = new AfipCacheTemporal<ConsultarPadronService.PadronAfipResult>(TimeSpan.FromHours(AfipSettings.PadronCacheHoras));

        // Para la pantalla del super-admin.
        public static bool PlataformaCortada => CortePlataforma.Cortado;
        public static DateTime? PlataformaCortadaHasta => CortePlataforma.CortadoHasta;
        public static int PlataformaFallosSeguidos => CortePlataforma.FallosSeguidos;
        public static int UsosUltimaHora(int idEmpresa) => Limitador.UsosEnVentana(idEmpresa.ToString());

        private readonly IWebHostEnvironment _env;
        private readonly IAfipConfigProvider _afip;
        private readonly IPadronPlataforma _plataforma;
        private readonly ILogger<PadronAfipGateway> _log;

        public PadronAfipGateway(IWebHostEnvironment env, IAfipConfigProvider afip, IPadronPlataforma plataforma, ILogger<PadronAfipGateway> log)
        {
            _env = env;
            _afip = afip;
            _plataforma = plataforma;
            _log = log;
        }

        public ConsultarPadronService.PadronAfipResult Consultar(Entidades.Empresa empresa, string cuitConsulta)
        {
            string cuit = NormalizarCuit(cuitConsulta);
            if (cuit.Length != 11 || !cuit.All(char.IsDigit))
                return new ConsultarPadronService.PadronAfipResult { Ok = false, Mensaje = "El CUIT consultado no es válido." };
            if (empresa == null)
                return new ConsultarPadronService.PadronAfipResult { Ok = false, Mensaje = "No se encontró la configuración AFIP de la empresa actual." };

            string claveCache = empresa.IdEmpresa + "|" + cuit;

            // (1) Cache: un resultado exitoso reciente de esta misma empresa no vuelve a ARCA.
            if (Cache.TryGet(claveCache, out var enCache))
            {
                Registrar(empresa, OrigenCache, "ok");
                return enCache;
            }

            // (2) Tope por hora de esta empresa (protege contra bloqueos y abuso de un tenant).
            if (!Limitador.IntentarUsar(empresa.IdEmpresa.ToString()))
            {
                Registrar(empresa, OrigenLimitada, "limitada");
                return new ConsultarPadronService.PadronAfipResult
                {
                    Ok = false,
                    Mensaje = "Se alcanzó el límite de consultas de padrón por hora para esta empresa. Cargá los datos del cliente a mano o probá más tarde."
                };
            }

            // (3) Certificado de la plataforma (si esta configurado y no esta cortado).
            var resultado = TryConsultarConPlataforma(empresa, cuit);

            // (4) Certificado de la propia empresa, si lo tiene.
            if (resultado == null)
                resultado = TryConsultarConEmpresa(empresa, cuit);

            // (5) Nada disponible.
            if (resultado == null)
            {
                Registrar(empresa, OrigenNinguno, "sin_credencial");
                return new ConsultarPadronService.PadronAfipResult
                {
                    Ok = false,
                    Mensaje = "El padrón de ARCA no está disponible en este momento. Cargá los datos del cliente a mano."
                };
            }

            if (resultado.Ok) Cache.Set(claveCache, resultado);
            return resultado;
        }

        // null = no se pudo usar (no configurado, cortado o fallo tecnico): se sigue con la siguiente credencial.
        private ConsultarPadronService.PadronAfipResult? TryConsultarConPlataforma(Entidades.Empresa empresa, string cuit)
        {
            if (!AfipSettings.PadronUsarPlataforma) return null;
            if (CortePlataforma.Cortado) return null;

            PadronPlataformaConfig? config;
            try { config = _plataforma.Obtener(); }
            catch (Exception ex)
            {
                _log.LogError(ex, "Padrón: no se pudo leer la configuración del certificado de la plataforma.");
                return null;
            }
            if (config == null) return null;

            var resultado = Ejecutar(() => new ConsultarPadronService(
                config.Cuit, config.Carpeta, config.NombreArchivo, config.Clave,
                AfipSettings.ConfigBase().Produccion, homologacion: false), cuit);

            if (resultado.ErrorTecnico)
            {
                CortePlataforma.RegistrarFallo();
                _log.LogWarning("Padrón: falló el certificado de la plataforma ({Fallos} seguidos). Se usa el de la empresa {IdEmpresa}, si lo tiene. {Mensaje}",
                    CortePlataforma.FallosSeguidos, empresa.IdEmpresa, resultado.Mensaje);
                Registrar(empresa, OrigenPlataforma, "error");
                return null;
            }

            // "El CUIT no existe / sin datos" es un resultado valido de ARCA, no una falla del certificado.
            CortePlataforma.RegistrarExito();
            Registrar(empresa, OrigenPlataforma, resultado.Ok ? "ok" : "sin_datos");
            return resultado;
        }

        private ConsultarPadronService.PadronAfipResult? TryConsultarConEmpresa(Entidades.Empresa empresa, string cuit)
        {
            if (empresa.Cuit <= 0) return null;

            string carpeta;
            try { carpeta = AfipRutas.Carpeta(_env.ContentRootPath, empresa.Cuit.ToString()); }
            catch (ArgumentException) { return null; }
            if (!File.Exists(AfipRutas.Certificado(carpeta, empresa.NombreCertificado_pfx))) return null;

            AfipConfig config;
            try { config = _afip.Crear(); }
            catch (InvalidOperationException ex)
            {
                return new ConsultarPadronService.PadronAfipResult { Ok = false, ErrorTecnico = true, Mensaje = ex.Message };
            }

            var resultado = Ejecutar(() => new ConsultarPadronService(empresa, _env.ContentRootPath, config), cuit);
            Registrar(empresa, OrigenEmpresa, resultado.Ok ? "ok" : (resultado.ErrorTecnico ? "error" : "sin_datos"));
            return resultado;
        }

        // Crea el servicio (hace el login) y consulta. Los errores de login o de la consulta vuelven como resultado.
        private static ConsultarPadronService.PadronAfipResult Ejecutar(Func<ConsultarPadronService> crearServicio, string cuit)
        {
            try
            {
                return crearServicio().ConsultarDatosContribuyente(cuit);
            }
            catch (Exception ex)
            {
                return new ConsultarPadronService.PadronAfipResult
                {
                    Ok = false,
                    ErrorTecnico = true,
                    // El login ya trae su mensaje traducido (InvalidOperationException de ConsultarPadronService).
                    Mensaje = ex is InvalidOperationException ? ex.Message : ConsultarPadronService.TraducirMensajeError(ex)
                };
            }
        }

        // Auditoria de uso: quien consulto y con que credencial. NUNCA se registra el CUIT consultado (dato de un tercero).
        private void Registrar(Entidades.Empresa empresa, string origen, string resultado)
        {
            PadronAfipEstadisticas.Sumar(origen);
            _log.LogInformation("Padrón ARCA: empresa {IdEmpresa}, credencial {Origen}, resultado {Resultado}.", empresa.IdEmpresa, origen, resultado);
        }

        private static string NormalizarCuit(string cuit)
        {
            return string.IsNullOrWhiteSpace(cuit) ? "" : cuit.Trim().Replace("-", "").Replace(" ", "");
        }
    }
}
