// Configuracion > Certificado ARCA: alta y renovacion del certificado digital de AFIP/ARCA de la empresa
// sin tocar el servidor, con el vencimiento a la vista y el paso a paso para pedirlo en ARCA.
// La empresa tiene UN certificado POR ENTORNO (produccion y pruebas/homologacion), cada uno en su carpeta
// (AFIP/<cuit>/prod y /homo): cargar el de un entorno nunca toca los archivos del otro.
// Flujo: (1) GenerarCsr -> baja el .csr y lo sube a ARCA; (2) SubirCertificado -> sube el .crt que devuelve
// ARCA y el sistema arma el .pfx. Solo admin y solo Postgres (la clave del pfx se guarda cifrada en la base).
// La empresa SIEMPRE sale de la sesion, nunca del request; el entorno del request se valida contra PROD/HOMO.
// Detalle: docs/06-datos-e-integraciones/afip-y-facturacion.md.
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

        // entorno: pestana que se abre (PROD/HOMO); vacio = la del entorno con el que factura la empresa.
        [HttpGet]
        public IActionResult Index(string? entorno = null)
        {
            if (!EsAdmin) return AccesoDenegado();

            var empresa = _estado.EmpresaActual();
            var vm = new CertificadoArcaVm { Soportado = _afip.SoportaCertificados, DiasAviso = AfipSettings.DiasAviso };
            if (empresa != null)
            {
                vm.Cuit = empresa.Cuit;
                vm.RazonSocial = empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "";
                vm.Entorno = AfipEntorno.Normalizar(empresa.Entorno_HOMO_PROD);
                vm.AliasSugerido = "carnisys" + DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);
                if (vm.Soportado && empresa.Cuit > 0)
                {
                    foreach (bool homologacion in new[] { false, true })
                    {
                        vm.Entornos.Add(new CertificadoEntornoVm
                        {
                            Entorno = homologacion ? AfipEntorno.Homologacion : AfipEntorno.Produccion,
                            EsActivo = homologacion == vm.EsHomologacion,
                            Info = _estado.Leer(empresa, homologacion),
                            Ruta = "AFIP/" + empresa.Cuit + "/" + (homologacion ? AfipRutas.CarpetaHomo : AfipRutas.CarpetaProd)
                        });
                    }
                    // Entrar a la pantalla refresca el aviso (sin esperar a la hora de la campana).
                    _estado.EvaluarAviso(empresa, forzar: true);
                }
            }

            ViewBag.EntornoAbierto = TryEntorno(entorno, out bool abrirHomo) ? (abrirHomo ? AfipEntorno.Homologacion : AfipEntorno.Produccion) : vm.Entorno;
            ViewBag.Title = "Certificado ARCA";
            ViewBag.Seccion = "Certificado ARCA";
            return View(vm);
        }

        // Paso 1: genera la clave + el pedido (.csr) del entorno y lo descarga. El .csr se sube a ARCA; la clave
        // privada se queda en el servidor (AFIP/<cuit>/<prod|homo>/pendiente).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarCsr(string alias, string? entorno)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, entorno, out bool homologacion, out string motivo))
                return Volver(entorno, error: motivo);

            try
            {
                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                string csr = servicio.GenerarCsr(empresa!.Cuit, homologacion, empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "", alias);
                _log.LogInformation("Certificado ARCA: pedido (CSR) generado para la empresa {IdEmpresa} ({Entorno}).", empresa.IdEmpresa, NombreEntorno(homologacion));
                return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(empresa, homologacion, alias));
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(entorno, error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error generando el pedido (CSR) de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(entorno, error: "No se pudo generar el pedido. Revisá los permisos de escritura de la carpeta AFIP del servidor.");
            }
        }

        // Vuelve a bajar el .csr del pedido pendiente del entorno (por si se perdio el archivo).
        [HttpGet]
        public IActionResult DescargarCsrPendiente(string? entorno)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, entorno, out bool homologacion, out string motivo))
                return Volver(entorno, error: motivo);

            string? csr = new CertificadoArcaService(_env.ContentRootPath).LeerCsrPendiente(empresa!.Cuit, homologacion);
            if (csr == null)
                return Volver(entorno, error: "No hay un pedido pendiente.");
            return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(empresa, homologacion, "pendiente"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarPedido(string? entorno)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, entorno, out bool homologacion, out string motivo))
                return Volver(entorno, error: motivo);

            new CertificadoArcaService(_env.ContentRootPath).DescartarPedidoPendiente(empresa!.Cuit, homologacion);
            return Volver(entorno, mensaje: "Se descartó el pedido pendiente.");
        }

        // Paso 2: sube el .crt que devolvio ARCA. Se valida (mismo pedido, mismo CUIT, vigente), se arma el
        // .pfx con una clave nueva (guardada cifrada, por entorno) y se instala en la carpeta del entorno,
        // reemplazando (con backup) solo el certificado de ESE entorno.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxBytesCrt * 4)]
        public IActionResult SubirCertificado(IFormFile? certificado, string? entorno)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, entorno, out bool homologacion, out string motivo))
                return Volver(entorno, error: motivo);

            if (certificado == null || certificado.Length == 0)
                return Volver(entorno, error: "Elegí el archivo del certificado (.crt) que descargaste de ARCA.");
            if (certificado.Length > MaxBytesCrt)
                return Volver(entorno, error: "El archivo es demasiado grande para ser un certificado.");
            string extension = Path.GetExtension(certificado.FileName ?? "").ToLowerInvariant();
            if (!ExtensionesCrt.Contains(extension))
                return Volver(entorno, error: "El archivo debe ser un certificado (.crt, .cer o .pem).");

            try
            {
                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    certificado.CopyTo(ms);
                    contenido = ms.ToArray();
                }

                // El nombre del archivo es fijo (certificado.pfx) y la carpeta la decide el entorno: no depende de
                // empresas.nombrecertificado_pfx, asi un certificado de pruebas nunca pisa el de produccion.
                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                var resultado = servicio.Instalar(empresa!.Cuit, homologacion, AfipRutas.NombreCertificadoFijo, contenido,
                    clave => _afip.GuardarClave(homologacion, clave));

                // La empresa queda marcada "con certificado" (habilita la facturacion electronica) solo si no lo estaba.
                Infrastructure.NegocioFactory.CrearCertificadoArcaClave(_sesion.Empresa).MarcarConCertificado(AfipRutas.NombreCertificadoFijo);

                _log.LogInformation("Certificado ARCA instalado: empresa {IdEmpresa} ({Entorno}), huella {Huella}, vence {Vence:yyyy-MM-dd}, usuario {IdUsuario}.",
                    empresa.IdEmpresa, NombreEntorno(homologacion), resultado.Huella, resultado.Vence, _sesion.UsuarioActual.Id);

                // Recalcula el aviso ya (cierra el del vencimiento anterior).
                _estado.EvaluarAviso(empresa, forzar: true);

                return Volver(entorno, mensaje: "Certificado de " + (homologacion ? "pruebas (homologación)" : "producción") + " instalado. Vence el "
                    + resultado.Vence.ToString("dd/MM/yyyy", new CultureInfo("es-AR")) + ". La próxima factura de ese entorno usa el certificado nuevo.");
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(entorno, error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error instalando el certificado de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(entorno, error: "No se pudo instalar el certificado. Quedó el anterior sin cambios; revisá el log del servidor.");
            }
        }

        // Probar conexion del entorno: recorre el camino real de la facturacion (certificado -> login en ARCA -> consulta
        // de solo lectura al WSFE por cada punto de venta) SIN emitir nada, y devuelve cada paso con su explicacion.
        // Reusa el ticket vigente (ARCA no entrega otro mientras hay uno valido), asi que no molesta a la facturacion.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProbarConexion(string? entorno)
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo administradores." });
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, entorno, out bool homologacion, out string motivo))
                return Json(new { ok = false, pasos = new[] { new { titulo = "Empresa", ok = false, detalle = motivo } }, homologacion = false });

            try
            {
                // Puntos de venta AFIP de las sucursales de la empresa (RLS ya limita a la empresa de la sesion).
                var puntosVenta = Infrastructure.NegocioFactory.CrearSucursal(_sesion.Empresa).findAll()
                    .Select(s => s.CodPuntoVentaAfip).Where(p => p > 0).Distinct().ToList();

                var resultado = AfipDiagnostico.ProbarFacturacion(empresa!, puntosVenta, _env.ContentRootPath, _afip.Crear(homologacion), NombreEntorno(homologacion));
                _log.LogInformation("Certificado ARCA: prueba de conexion de la empresa {IdEmpresa} ({Entorno}): {Resultado}.", empresa!.IdEmpresa, NombreEntorno(homologacion), resultado.Ok ? "OK" : "con errores");

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

        // Vuelve a la pantalla, en la pestana del entorno con el que se estaba trabajando.
        private IActionResult Volver(string? entorno, string? mensaje = null, string? error = null)
        {
            if (mensaje != null) TempData["CertificadoMensaje"] = mensaje;
            if (error != null) TempData["CertificadoError"] = error;
            return TryEntorno(entorno, out bool homologacion)
                ? RedirectToAction(nameof(Index), new { entorno = NombreEntorno(homologacion) })
                : RedirectToAction(nameof(Index));
        }

        private IActionResult AccesoDenegado()
        {
            ViewBag.Seccion = "Certificado ARCA";
            return View("~/Views/Shared/AccesoDenegado.cshtml");
        }

        // "PROD" / "HOMO" del request (cualquier otra cosa se rechaza: el entorno decide la CARPETA en disco).
        private static bool TryEntorno(string? entorno, out bool homologacion)
        {
            homologacion = false;
            if (string.Equals(entorno?.Trim(), AfipEntorno.Produccion, StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(entorno?.Trim(), AfipEntorno.Homologacion, StringComparison.OrdinalIgnoreCase)) { homologacion = true; return true; }
            return false;
        }

        private static string NombreEntorno(bool homologacion) => homologacion ? AfipEntorno.Homologacion : AfipEntorno.Produccion;

        // La pantalla opera solo sobre la empresa de la sesion, con CUIT valido, en un ambiente que guarde la clave y
        // con un entorno valido (PROD/HOMO).
        private bool PuedeOperar(Entidades.Empresa? empresa, string? entorno, out bool homologacion, out string motivo)
        {
            motivo = "";
            homologacion = false;
            if (!_afip.SoportaCertificados) { motivo = "La gestión de certificados requiere Postgres."; return false; }
            if (empresa == null || empresa.Cuit.ToString().Length != 11) { motivo = "La empresa no tiene un CUIT válido cargado."; return false; }
            if (!TryEntorno(entorno, out homologacion)) { motivo = "Elegí el entorno del certificado (producción o pruebas)."; return false; }
            return true;
        }

        private static string NombreCsr(Entidades.Empresa empresa, bool homologacion, string alias)
        {
            // Solo caracteres seguros en el nombre de archivo.
            string limpio = new string((alias ?? "").Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            return empresa.Cuit + (homologacion ? "-homo" : "") + (limpio.Length > 0 ? "-" + limpio : "") + ".csr";
        }
    }
}
