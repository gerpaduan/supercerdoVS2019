// Administracion del sistema > Certificado de padron (plataforma): el certificado UNICO con el que se consulta el
// padron de ARCA (ws_sr_padron_a13) para todas las empresas al dar de alta un cliente. SOLO super-admin
// (mismo gate que SystemAdministrationController). Alias dedicado al padron: solo ese servicio autorizado, asi que
// si el archivo se filtra no permite facturar. Mismo flujo que el certificado de empresa: pedido (.csr) -> ARCA ->
// .crt -> el sistema arma el .pfx. Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
using AFIP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Globalization;
using WebCore.Helpers;
using WebCore.Services;

namespace WebCore.Controllers
{
    public class PadronPlataformaVm
    {
        public bool Soportado { get; set; }
        public long? Cuit { get; set; }
        public InfoCertificado Info { get; set; } = new InfoCertificado();
        public int[] DiasAviso { get; set; } = Array.Empty<int>();
        public bool UsarPlataforma { get; set; }
        public bool Cortada { get; set; }
        public DateTime? CortadaHasta { get; set; }
        public int FallosSeguidos { get; set; }
        public int MaxPorHoraEmpresa { get; set; }
        public IReadOnlyDictionary<string, long> Contadores { get; set; } = new Dictionary<string, long>();
        // CUIT del pedido pendiente (si hay), para recordarlo al subir el .crt.
        public long? CuitPedidoPendiente { get; set; }
    }

    public class PadronPlataformaController : Controller
    {
        private const long MaxBytesCrt = 64 * 1024;
        private static readonly string[] ExtensionesCrt = { ".crt", ".cer", ".pem" };

        private readonly IUsuarioSesionService _sesion;
        private readonly IWebHostEnvironment _env;
        private readonly IPadronPlataforma _plataforma;
        private readonly ILogger<PadronPlataformaController> _log;

        public PadronPlataformaController(IUsuarioSesionService sesion, IWebHostEnvironment env, IPadronPlataforma plataforma, ILogger<PadronPlataformaController> log)
        {
            _sesion = sesion;
            _env = env;
            _plataforma = plataforma;
            _log = log;
        }

        // Gate de super-admin de plataforma (columna Usuarios.superadmin, NO el sistema de Permisos).
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var repo = Infrastructure.NegocioFactory.CrearSystemAdministrationRepository();
            if (!repo.EsSuperAdmin(_sesion.UsuarioActual.Id))
            {
                ViewBag.Title = "Administracion del sistema";
                ViewBag.Seccion = "Administracion del sistema";
                filterContext.Result = View("~/Views/Shared/AccesoDenegado.cshtml");
                return;
            }
            base.OnActionExecuting(filterContext);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var servicio = CertificadoArcaService.ParaPlataforma(_env.ContentRootPath);
            var vm = new PadronPlataformaVm
            {
                Soportado = _plataforma.Soportado,
                DiasAviso = AfipSettings.DiasAviso,
                UsarPlataforma = AfipSettings.PadronUsarPlataforma,
                Cortada = PadronAfipGateway.PlataformaCortada,
                CortadaHasta = PadronAfipGateway.PlataformaCortadaHasta,
                FallosSeguidos = PadronAfipGateway.PlataformaFallosSeguidos,
                MaxPorHoraEmpresa = AfipSettings.PadronMaxPorHoraEmpresa,
                Contadores = PadronAfipEstadisticas.Todas()
            };

            if (vm.Soportado)
            {
                vm.Cuit = _plataforma.CuitConfigurado();
                vm.Info = _plataforma.LeerEstado();
                vm.CuitPedidoPendiente = servicio.LeerCuitPendiente();
                vm.Info.HayPedidoPendiente = vm.CuitPedidoPendiente.HasValue;
            }

            ViewBag.Title = "Certificado de padrón";
            ViewBag.Seccion = "Administracion del sistema";
            return View(vm);
        }

        // Paso 1: genera clave + pedido (.csr) para el CUIT del alias de la plataforma.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarCsr(string cuit, string razonSocial, string alias)
        {
            if (!_plataforma.Soportado) return Volver(error: "Requiere Postgres.");
            if (!long.TryParse((cuit ?? "").Trim(), out long cuitNumero) || cuitNumero.ToString().Length != 11)
                return Volver(error: "El CUIT debe tener 11 dígitos.");

            try
            {
                string csr = CertificadoArcaService.ParaPlataforma(_env.ContentRootPath).GenerarCsr(cuitNumero, razonSocial, alias);
                _log.LogInformation("Certificado de padrón de la plataforma: pedido (CSR) generado por el usuario {IdUsuario}.", _sesion.UsuarioActual.Id);
                return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", cuitNumero + "-padron.csr");
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado de padrón de la plataforma: error generando el pedido.");
                return Volver(error: "No se pudo generar el pedido. Revisá los permisos de escritura de la carpeta AFIP del servidor.");
            }
        }

        [HttpGet]
        public IActionResult DescargarCsrPendiente()
        {
            var servicio = CertificadoArcaService.ParaPlataforma(_env.ContentRootPath);
            long? cuit = servicio.LeerCuitPendiente();
            string? csr = cuit.HasValue ? servicio.LeerCsrPendiente(cuit.Value) : null;
            if (csr == null) return Volver(error: "No hay un pedido pendiente.");
            return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", cuit + "-padron.csr");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarPedido()
        {
            var servicio = CertificadoArcaService.ParaPlataforma(_env.ContentRootPath);
            long? cuit = servicio.LeerCuitPendiente();
            servicio.DescartarPedidoPendiente(cuit ?? 0);
            return Volver(mensaje: "Se descartó el pedido pendiente.");
        }

        // Paso 2: sube el .crt que devolvio ARCA para el alias del padron.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxBytesCrt * 4)]
        public IActionResult SubirCertificado(IFormFile? certificado)
        {
            if (!_plataforma.Soportado) return Volver(error: "Requiere Postgres.");
            if (certificado == null || certificado.Length == 0)
                return Volver(error: "Elegí el archivo del certificado (.crt) que descargaste de ARCA.");
            if (certificado.Length > MaxBytesCrt)
                return Volver(error: "El archivo es demasiado grande para ser un certificado.");
            if (!ExtensionesCrt.Contains(Path.GetExtension(certificado.FileName ?? "").ToLowerInvariant()))
                return Volver(error: "El archivo debe ser un certificado (.crt, .cer o .pem).");

            var servicio = CertificadoArcaService.ParaPlataforma(_env.ContentRootPath);
            long? cuit = servicio.LeerCuitPendiente();
            if (!cuit.HasValue)
                return Volver(error: "No hay un pedido pendiente: generá primero el pedido (CSR) y subilo a ARCA.");

            try
            {
                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    certificado.CopyTo(ms);
                    contenido = ms.ToArray();
                }

                int idUsuario = _sesion.UsuarioActual.Id;
                var resultado = servicio.Instalar(cuit.Value, AfipRutas.NombreCertificadoPlataforma, contenido,
                    clave => _plataforma.Guardar(cuit.Value, AfipRutas.NombreCertificadoPlataforma, clave, idUsuario));

                _log.LogInformation("Certificado de padrón de la plataforma instalado: huella {Huella}, vence {Vence:yyyy-MM-dd}, usuario {IdUsuario}.",
                    resultado.Huella, resultado.Vence, idUsuario);
                return Volver(mensaje: "Certificado de padrón instalado. Vence el " + resultado.Vence.ToString("dd/MM/yyyy", new CultureInfo("es-AR")) + ".");
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado de padrón de la plataforma: error instalando el certificado.");
                return Volver(error: "No se pudo instalar el certificado. Quedó el anterior sin cambios; revisá el log del servidor.");
            }
        }

        // Prueba real y de solo lectura: login con el certificado de la plataforma y consulta del padron de su propio CUIT.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Probar()
        {
            var config = _plataforma.Obtener();
            if (config == null)
                return Json(new { ok = false, mensaje = "La plataforma no tiene un certificado de padrón utilizable (falta cargarlo o no se puede descifrar su clave)." });

            try
            {
                var servicio = new ConsultarPadronService(config.Cuit, config.Carpeta, config.NombreArchivo, config.Clave, AfipSettings.ConfigBase().Produccion);
                var resultado = servicio.ConsultarDatosContribuyente(config.Cuit.ToString());
                return Json(new
                {
                    ok = resultado.Ok,
                    mensaje = resultado.Ok
                        ? "Todo bien: ARCA respondió a la consulta de padrón (" + (resultado.RazonSocialAfip ?? "") + ")."
                        : (resultado.Mensaje ?? "ARCA no devolvió datos.") + (resultado.ErrorTecnico ? " Revisá que el alias tenga autorizado ws_sr_padron_a13." : "")
                });
            }
            catch (Exception ex)
            {
                // El login trae su mensaje ya traducido (InvalidOperationException de ConsultarPadronService).
                string mensaje = ex is InvalidOperationException ? ex.Message : ConsultarPadronService.TraducirMensajeError(ex);
                return Json(new { ok = false, mensaje });
            }
        }

        private IActionResult Volver(string? mensaje = null, string? error = null)
        {
            if (mensaje != null) TempData["PadronMensaje"] = mensaje;
            if (error != null) TempData["PadronError"] = error;
            return RedirectToAction(nameof(Index));
        }
    }
}
