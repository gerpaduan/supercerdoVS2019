// Configuracion > Certificado ARCA: alta y renovacion del certificado digital de AFIP/ARCA de la empresa
// sin tocar el servidor, con el vencimiento a la vista y el paso a paso para pedirlo en ARCA.
// Flujo: (1) GenerarCsr -> baja el .csr y lo sube a ARCA; (2) SubirCertificado -> sube el .crt que devuelve
// ARCA y el sistema arma el .pfx. Solo admin y solo Postgres (la clave del pfx se guarda cifrada en la base).
// La empresa SIEMPRE sale de la sesion, nunca del request. Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
using AFIP;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebCore.Helpers;
using WebCore.Models;
using WebCore.Services;

namespace WebCore.Controllers
{
    public class CertificadoArcaController : Controller
    {
        private const long MaxBytesCrt = 64 * 1024;
        private static readonly string[] ExtensionesCrt = { ".crt", ".cer", ".pem" };

        private readonly IUsuarioSesionService _sesion;
        private readonly IWebHostEnvironment _env;
        private readonly IAfipConfigProvider _afip;
        private readonly ICertificadoArcaEstado _estado;
        private readonly ILogger<CertificadoArcaController> _log;

        public CertificadoArcaController(IUsuarioSesionService sesion, IWebHostEnvironment env, IAfipConfigProvider afip,
            ICertificadoArcaEstado estado, ILogger<CertificadoArcaController> log)
        {
            _sesion = sesion;
            _env = env;
            _afip = afip;
            _estado = estado;
            _log = log;
        }

        private bool EsAdmin => _sesion.UsuarioActual.Admin;

        [HttpGet]
        public IActionResult Index()
        {
            if (!EsAdmin) return AccesoDenegado();

            var empresa = _estado.EmpresaActual();
            var vm = new CertificadoArcaVm { Soportado = _afip.SoportaCertificados, DiasAviso = AfipSettings.DiasAviso };
            if (empresa != null)
            {
                vm.Cuit = empresa.Cuit;
                vm.RazonSocial = empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "";
                vm.Entorno = AfipEntorno.Normalizar(empresa.Entorno_HOMO_PROD);
                vm.AliasSugerido = "carnisys-" + DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
                if (vm.Soportado && empresa.Cuit > 0)
                {
                    vm.Info = _estado.Leer(empresa);
                    // Entrar a la pantalla refresca el aviso (sin esperar a la hora de la campana).
                    _estado.EvaluarAviso(empresa, forzar: true);
                }
                vm.AdvertenciaEntorno = AdvertenciaEntorno(vm);
            }

            ViewBag.Title = "Certificado ARCA";
            ViewBag.Seccion = "Certificado ARCA";
            return View(vm);
        }

        // Paso 1: genera la clave + el pedido (.csr) y lo descarga. El .csr se sube a ARCA; la clave privada
        // se queda en el servidor.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarCsr(string alias)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, out string motivo))
                return Volver(error: motivo);

            try
            {
                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                string csr = servicio.GenerarCsr(empresa!.Cuit, empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "", alias);
                _log.LogInformation("Certificado ARCA: pedido (CSR) generado para la empresa {IdEmpresa}.", empresa.IdEmpresa);
                return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(empresa, alias));
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error generando el pedido (CSR) de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(error: "No se pudo generar el pedido. Revisá los permisos de escritura de la carpeta AFIP del servidor.");
            }
        }

        // Vuelve a bajar el .csr del pedido pendiente (por si se perdio el archivo).
        [HttpGet]
        public IActionResult DescargarCsrPendiente()
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, out string motivo))
                return Volver(error: motivo);

            string? csr = new CertificadoArcaService(_env.ContentRootPath).LeerCsrPendiente(empresa!.Cuit);
            if (csr == null)
                return Volver(error: "No hay un pedido pendiente.");
            return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(empresa, "pendiente"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarPedido()
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, out string motivo))
                return Volver(error: motivo);

            new CertificadoArcaService(_env.ContentRootPath).DescartarPedidoPendiente(empresa!.Cuit);
            return Volver(mensaje: "Se descartó el pedido pendiente.");
        }

        // Paso 2: sube el .crt que devolvio ARCA. Se valida (mismo pedido, mismo CUIT, vigente), se arma el
        // .pfx con una clave nueva (guardada cifrada) y se reemplaza el anterior dejando backup.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxBytesCrt * 4)]
        public IActionResult SubirCertificado(IFormFile? certificado)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, out string motivo))
                return Volver(error: motivo);

            if (certificado == null || certificado.Length == 0)
                return Volver(error: "Elegí el archivo del certificado (.crt) que descargaste de ARCA.");
            if (certificado.Length > MaxBytesCrt)
                return Volver(error: "El archivo es demasiado grande para ser un certificado.");
            string extension = Path.GetExtension(certificado.FileName ?? "").ToLowerInvariant();
            if (!ExtensionesCrt.Contains(extension))
                return Volver(error: "El archivo debe ser un certificado (.crt, .cer o .pem).");

            try
            {
                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    certificado.CopyTo(ms);
                    contenido = ms.ToArray();
                }

                // Nombre del pfx: el que ya tiene la empresa; si no tiene, uno segun el entorno.
                bool teniaNombre = !string.IsNullOrWhiteSpace(empresa!.NombreCertificado_pfx);
                string nombrePfx = teniaNombre
                    ? AfipRutas.NombreCertificado(empresa.NombreCertificado_pfx)
                    : (AfipEntorno.EsHomologacion(empresa.Entorno_HOMO_PROD) ? "certif-homo.pfx" : AfipRutas.NombreCertificadoPorDefecto);

                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                var resultado = servicio.Instalar(empresa.Cuit, nombrePfx, contenido, _afip.GuardarClave);

                if (!teniaNombre)
                    Infrastructure.NegocioFactory.CrearCertificadoArcaClave(_sesion.Empresa).ActualizarNombreCertificado(resultado.NombreArchivo);

                _log.LogInformation("Certificado ARCA instalado: empresa {IdEmpresa}, huella {Huella}, vence {Vence:yyyy-MM-dd}, usuario {IdUsuario}.",
                    empresa.IdEmpresa, resultado.Huella, resultado.Vence, _sesion.UsuarioActual.Id);

                // Recalcula el aviso ya (cierra el del vencimiento anterior).
                empresa.NombreCertificado_pfx = resultado.NombreArchivo;
                _estado.EvaluarAviso(empresa, forzar: true);

                return Volver(mensaje: "Certificado instalado. Vence el " + resultado.Vence.ToString("dd/MM/yyyy", new CultureInfo("es-AR"))
                    + ". La próxima factura usa el certificado nuevo.");
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error instalando el certificado de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(error: "No se pudo instalar el certificado. Quedó el anterior sin cambios; revisá el log del servidor.");
            }
        }

        // Probar conexion: recorre el camino real de la facturacion (certificado -> login en ARCA -> consulta de
        // solo lectura al WSFE por cada punto de venta) SIN emitir nada, y devuelve cada paso con su explicacion.
        // Reusa el ticket vigente (ARCA no entrega otro mientras hay uno valido), asi que no molesta a la facturacion.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProbarConexion()
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo administradores." });
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, out string motivo))
                return Json(new { ok = false, pasos = new[] { new { titulo = "Empresa", ok = false, detalle = motivo } }, homologacion = false });

            try
            {
                // Puntos de venta AFIP de las sucursales de la empresa (RLS ya limita a la empresa de la sesion).
                var puntosVenta = Infrastructure.NegocioFactory.CrearSucursal(_sesion.Empresa).findAll()
                    .Select(s => s.CodPuntoVentaAfip).Where(p => p > 0).Distinct().ToList();

                var resultado = AfipDiagnostico.ProbarFacturacion(empresa!, puntosVenta, _env.ContentRootPath, _afip.Crear());
                _log.LogInformation("Certificado ARCA: prueba de conexion de la empresa {IdEmpresa}: {Resultado}.", empresa!.IdEmpresa, resultado.Ok ? "OK" : "con errores");

                return Json(new
                {
                    ok = resultado.Ok,
                    homologacion = resultado.EsHomologacion,
                    pasos = resultado.Pasos.Select(p => new { titulo = p.Titulo, ok = p.Ok, detalle = p.Detalle })
                });
            }
            catch (InvalidOperationException ex)
            {
                // Clave guardada que no se puede descifrar (AfipConfigProvider): mensaje ya mostrable.
                return Json(new { ok = false, homologacion = false, pasos = new[] { new { titulo = "Clave del certificado", ok = false, detalle = ex.Message } } });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error inesperado en la prueba de conexion de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Json(new { ok = false, homologacion = false, pasos = new[] { new { titulo = "Prueba de conexión", ok = false, detalle = "Error inesperado; revisá el log del servidor." } } });
            }
        }

        // ------------------------------------------------------------------ helpers

        private IActionResult Volver(string? mensaje = null, string? error = null)
        {
            if (mensaje != null) TempData["CertificadoMensaje"] = mensaje;
            if (error != null) TempData["CertificadoError"] = error;
            return RedirectToAction(nameof(Index));
        }

        private IActionResult AccesoDenegado()
        {
            ViewBag.Seccion = "Certificado ARCA";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        // La pantalla opera solo sobre la empresa de la sesion, con CUIT valido y en un ambiente que guarde la clave.
        private bool PuedeOperar(Entidades.Empresa? empresa, out string motivo)
        {
            motivo = "";
            if (!_afip.SoportaCertificados) { motivo = "La gestión de certificados requiere Postgres."; return false; }
            if (empresa == null || empresa.Cuit.ToString().Length != 11) { motivo = "La empresa no tiene un CUIT válido cargado."; return false; }
            return true;
        }

        private static string NombreCsr(Entidades.Empresa empresa, string alias)
        {
            // Solo caracteres seguros en el nombre de archivo.
            string limpio = new string((alias ?? "").Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            return empresa.Cuit + (limpio.Length > 0 ? "-" + limpio : "") + ".csr";
        }

        // El cert de homologacion y el de produccion son distintos: se avisa si el nombre del archivo sugiere el otro.
        private static string AdvertenciaEntorno(CertificadoArcaVm vm)
        {
            string nombre = (vm.Info.NombreArchivo ?? "").ToLowerInvariant();
            if (vm.Info.Estado == EstadoCertificado.SinCertificado) return "";
            if (vm.EsHomologacion && !nombre.Contains("homo"))
                return "La empresa está en modo PRUEBA (homologación) pero el archivo '" + vm.Info.NombreArchivo
                    + "' no parece un certificado de homologación. Los certificados de homologación se piden aparte en ARCA (WSASS).";
            if (!vm.EsHomologacion && nombre.Contains("homo"))
                return "La empresa está en PRODUCCIÓN pero el archivo '" + vm.Info.NombreArchivo + "' parece de homologación.";
            return "";
        }
    }
}
