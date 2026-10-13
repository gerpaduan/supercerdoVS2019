// Configuracion > Certificado ARCA: alta y renovacion del certificado digital de AFIP/ARCA de la empresa
// sin tocar el servidor, con el vencimiento a la vista y el paso a paso para pedirlo en ARCA.
// Un certificado POR ENTORNO (produccion y pruebas/homologacion), cada uno en su carpeta (AFIP/<cuit>/prod y /homo):
// cargar el de un entorno nunca toca los archivos del otro. Titulares fiscales (2026-10-09): se opera sobre UN titular
// (el principal = el CUIT de la empresa, o uno de los demas); cada titular tiene su propia carpeta AFIP/<su cuit>/.
// El entorno en uso para facturar es de la EMPRESA (lo cambia el super-admin): esta pantalla no lo cambia.
// Flujo: (1) GenerarCsr -> baja el .csr y lo sube a ARCA; (2) SubirCertificado -> sube el .crt que devuelve
// ARCA y el sistema arma el .pfx. Solo admin y solo Postgres (la clave del pfx se guarda cifrada en la base).
// La empresa SIEMPRE sale de la sesion y el CUIT/razon social del titular SIEMPRE salen de su fila (nunca del request:
// el idTitular del request se valida contra la empresa de la sesion); el entorno del request se valida contra PROD/HOMO.
// Titular compartido entre empresas (2026-10-12): un mismo CUIT puede ser titular en varias empresas y todas usan el mismo
// certificado (AFIP/<cuit>/, clave en certificados_arca_cuit). Lo ADMINISTRA (pedir, instalar, descartar) solo la empresa donde
// ese CUIT es el principal (o la del titular mas antiguo) y el super-admin; las demas ven su estado y lo usan.
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
        // Titulares fiscales (null en SQL Server: la pantalla opera sobre la empresa como siempre).
        private readonly Negocio.TitularFiscal _titulares;

        public CertificadoArcaController(IUsuarioSesionService sesion, IWebHostEnvironment env, IAfipConfigProvider afip,
            ICertificadoArcaEstado estado, ILogger<CertificadoArcaController> log)
        {
            _sesion = sesion;
            _env = env;
            _afip = afip;
            _estado = estado;
            _log = log;
            _titulares = Infrastructure.NegocioFactory.CrearTitularFiscal(sesion.Empresa);
        }

        private bool EsAdmin => _sesion.UsuarioActual.Admin;

        // Super-admin de la plataforma (usuarios.superadmin): puede administrar el certificado de cualquier CUIT y ve los nombres
        // de las otras empresas que lo comparten (el admin de una empresa no: son datos de otra).
        private bool? _esSuperAdmin;
        private bool EsSuperAdmin => _esSuperAdmin ??= _sesion.UsuarioActual != null
            && Infrastructure.NegocioFactory.CrearSystemAdministrationRepository().EsSuperAdmin(_sesion.UsuarioActual.Id);

        // Titular sobre el que opera la pantalla: sus datos "efectivos" (CUIT, razon social, certificado, condicion IVA).
        private sealed class Objetivo
        {
            public Entidades.Empresa Empresa { get; set; } = null!;
            public int IdTitular { get; set; }          // 0 = el principal (la empresa)
            public bool EsPrincipal { get; set; }
            public long Cuit { get; set; }
            public string RazonSocial { get; set; } = "";
            public string NombreCertificadoPfx { get; set; } = "";
            public bool EsRRII { get; set; }
            public Entidades.TitularFiscal? Titular { get; set; }

            // Para pedirle la configuracion (clave del pfx) a AfipConfigProvider.
            public Entidades.EmisorFiscal ComoEmisor() => new Entidades.EmisorFiscal
            {
                IdTitular = EsPrincipal ? (int?)null : IdTitular,
                Cuit = Cuit,
                RazonSocial = RazonSocial,
                EsRRII = EsRRII,
                EsPredeterminado = EsPrincipal,
                NombreCertificadoPfx = NombreCertificadoPfx
            };
        }

        // Resuelve sobre que titular se opera. idTitular <= 0 (o SQL Server) = el principal. Un titular de otra empresa
        // o inexistente se rechaza (RLS ya limita la lectura a la empresa de la sesion).
        private Objetivo? ResolverObjetivo(Entidades.Empresa? empresa, int idTitular, out string motivo)
        {
            motivo = "";
            if (empresa == null) return null;

            Entidades.TitularFiscal? titular = null;
            if (_titulares != null && idTitular > 0)
            {
                titular = _titulares.Obtener(idTitular);
                if (titular == null || titular.IdEmpresa != empresa.IdEmpresa)
                {
                    motivo = "El titular elegido no existe.";
                    return null;
                }
                // Titularidad cancelada por el super-admin: la empresa ya no puede usar (ni siquiera probar) ese certificado.
                if (titular.Revocado)
                {
                    motivo = "La titularidad de ese titular fue cancelada por el administrador del sistema.";
                    return null;
                }
            }

            if (titular == null || titular.EsPredeterminado)
            {
                return new Objetivo
                {
                    Empresa = empresa,
                    IdTitular = titular?.IdTitular ?? 0,
                    EsPrincipal = true,
                    Cuit = empresa.Cuit,
                    RazonSocial = empresa.RazonSocialAfip ?? empresa.NombreFantasia ?? "",
                    NombreCertificadoPfx = empresa.NombreCertificado_pfx ?? "",
                    EsRRII = empresa.EsRRII,
                    Titular = titular
                };
            }

            return new Objetivo
            {
                Empresa = empresa,
                IdTitular = titular.IdTitular,
                EsPrincipal = false,
                Cuit = titular.Cuit,
                RazonSocial = titular.RazonSocial ?? "",
                NombreCertificadoPfx = titular.NombreCertificadoPfx ?? "",
                EsRRII = titular.EsRRII,
                Titular = titular
            };
        }

        // entorno: pestana que se abre (PROD/HOMO); vacio = la del entorno con el que factura la empresa.
        // idTitular: titular sobre el que se opera (0 = el principal).
        [HttpGet]
        public IActionResult Index(string? entorno = null, int idTitular = 0)
        {
            if (!EsAdmin) return AccesoDenegado();

            var empresa = _estado.EmpresaActual();
            var vm = new CertificadoArcaVm { Soportado = _afip.SoportaCertificados, DiasAviso = AfipSettings.DiasAviso };
            if (empresa != null)
            {
                var objetivo = ResolverObjetivo(empresa, idTitular, out string motivoTitular);
                if (objetivo == null)
                {
                    TempData["CertificadoError"] = motivoTitular;
                    objetivo = ResolverObjetivo(empresa, 0, out _)!;
                }

                vm.IdTitular = objetivo.IdTitular;
                vm.EsPrincipal = objetivo.EsPrincipal;
                vm.Cuit = objetivo.Cuit;
                vm.RazonSocial = objetivo.RazonSocial;
                vm.Entorno = AfipEntorno.Normalizar(empresa.Entorno_HOMO_PROD);
                vm.AliasSugerido = "carnisys" + DateTime.Now.ToString("yyyyMM", CultureInfo.InvariantCulture);

                // Titular compartido: quien administra el certificado de este CUIT y si otras empresas lo usan.
                if (_titulares != null && objetivo.Cuit > 0)
                {
                    var otras = _titulares.OtrasEmpresasConCuit(objetivo.Cuit, empresa.IdEmpresa);
                    int administradora = _titulares.EmpresaAdministradoraDeCuit(objetivo.Cuit);
                    vm.PuedeAdministrar = administradora == 0 || administradora == empresa.IdEmpresa || EsSuperAdmin;
                    vm.ComparteCertificado = otras.Count > 0;
                    vm.EmpresasQueLoComparten = EsSuperAdmin
                        ? string.Join(", ", otras.Select(o => o.Nombre))
                        : otras.Count + (otras.Count == 1 ? " empresa más" : " empresas más");
                    if (!vm.PuedeAdministrar && EsSuperAdmin)
                        vm.EmpresaAdministradora = otras.FirstOrDefault(o => o.IdEmpresa == administradora)?.Nombre ?? "";
                }

                if (_titulares != null)
                {
                    foreach (var t in _titulares.Listar(empresa.IdEmpresa, false))
                        vm.Titulares.Add(new CertificadoTitularOpcionVm
                        {
                            IdTitular = t.IdTitular,
                            Nombre = t.RazonSocial ?? "",
                            Cuit = t.Cuit,
                            EsPrincipal = t.EsPredeterminado,
                            Habilitado = t.Habilitado
                        });
                }

                if (vm.Soportado && objetivo.Cuit > 0)
                {
                    foreach (bool homologacion in new[] { false, true })
                    {
                        vm.Entornos.Add(new CertificadoEntornoVm
                        {
                            Entorno = homologacion ? AfipEntorno.Homologacion : AfipEntorno.Produccion,
                            EsActivo = homologacion == vm.EsHomologacion,
                            Info = LeerEstado(empresa, objetivo, homologacion),
                            Ruta = "AFIP/" + objetivo.Cuit + "/" + (homologacion ? AfipRutas.CarpetaHomo : AfipRutas.CarpetaProd)
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

        private InfoCertificado LeerEstado(Entidades.Empresa empresa, Objetivo objetivo, bool homologacion)
        {
            return objetivo.EsPrincipal
                ? _estado.Leer(empresa, homologacion)
                : _estado.LeerTitular(empresa, objetivo.Titular, homologacion);
        }

        // Paso 1: genera la clave + el pedido (.csr) del entorno y lo descarga. El .csr se sube a ARCA; la clave
        // privada se queda en el servidor (AFIP/<cuit>/<prod|homo>/pendiente).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GenerarCsr(string alias, string? entorno, int idTitular = 0)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, idTitular, entorno, out var objetivo, out bool homologacion, out string motivo, requiereAdministrar: true))
                return Volver(entorno, idTitular, error: motivo);

            try
            {
                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                string csr = servicio.GenerarCsr(objetivo!.Cuit, homologacion, objetivo.RazonSocial, alias);
                _log.LogInformation("Certificado ARCA: pedido (CSR) generado para la empresa {IdEmpresa}, titular {IdTitular} ({Entorno}).", empresa!.IdEmpresa, objetivo.IdTitular, NombreEntorno(homologacion));
                return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(objetivo, homologacion, alias));
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(entorno, idTitular, error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error generando el pedido (CSR) de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(entorno, idTitular, error: "No se pudo generar el pedido. Revisá los permisos de escritura de la carpeta AFIP del servidor.");
            }
        }

        // Vuelve a bajar el .csr del pedido pendiente del entorno (por si se perdio el archivo).
        [HttpGet]
        public IActionResult DescargarCsrPendiente(string? entorno, int idTitular = 0)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, idTitular, entorno, out var objetivo, out bool homologacion, out string motivo, requiereAdministrar: true))
                return Volver(entorno, idTitular, error: motivo);

            string? csr = new CertificadoArcaService(_env.ContentRootPath).LeerCsrPendiente(objetivo!.Cuit, homologacion);
            if (csr == null)
                return Volver(entorno, idTitular, error: "No hay un pedido pendiente.");
            return File(System.Text.Encoding.ASCII.GetBytes(csr), "application/x-pem-file", NombreCsr(objetivo, homologacion, "pendiente"));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DescartarPedido(string? entorno, int idTitular = 0)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, idTitular, entorno, out var objetivo, out bool homologacion, out string motivo, requiereAdministrar: true))
                return Volver(entorno, idTitular, error: motivo);

            new CertificadoArcaService(_env.ContentRootPath).DescartarPedidoPendiente(objetivo!.Cuit, homologacion);
            return Volver(entorno, idTitular, mensaje: "Se descartó el pedido pendiente.");
        }

        // Paso 2: sube el .crt que devolvio ARCA. Se valida (mismo pedido, mismo CUIT, vigente), se arma el
        // .pfx con una clave nueva (guardada cifrada, por entorno) y se instala en la carpeta del entorno,
        // reemplazando (con backup) solo el certificado de ESE entorno.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(MaxBytesCrt * 4)]
        public IActionResult SubirCertificado(IFormFile? certificado, string? entorno, int idTitular = 0)
        {
            if (!EsAdmin) return AccesoDenegado();
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, idTitular, entorno, out var objetivo, out bool homologacion, out string motivo, requiereAdministrar: true))
                return Volver(entorno, idTitular, error: motivo);

            if (certificado == null || certificado.Length == 0)
                return Volver(entorno, idTitular, error: "Elegí el archivo del certificado (.crt) que descargaste de ARCA.");
            if (certificado.Length > MaxBytesCrt)
                return Volver(entorno, idTitular, error: "El archivo es demasiado grande para ser un certificado.");
            string extension = Path.GetExtension(certificado.FileName ?? "").ToLowerInvariant();
            if (!ExtensionesCrt.Contains(extension))
                return Volver(entorno, idTitular, error: "El archivo debe ser un certificado (.crt, .cer o .pem).");

            try
            {
                byte[] contenido;
                using (var ms = new MemoryStream())
                {
                    certificado.CopyTo(ms);
                    contenido = ms.ToArray();
                }

                // El nombre del archivo es fijo (certificado.pfx) y la carpeta la decide el CUIT y el entorno: no depende de
                // empresas.nombrecertificado_pfx, asi un certificado de pruebas nunca pisa el de produccion.
                var servicio = new CertificadoArcaService(_env.ContentRootPath);
                var resultado = servicio.Instalar(objetivo!.Cuit, homologacion, AfipRutas.NombreCertificadoFijo, contenido,
                    clave =>
                    {
                        // El principal sigue guardando su clave como siempre (certificado_arca_clave: un build anterior puede leerla);
                        // todos, ademas, en el registro por CUIT que comparten las empresas con ese titular.
                        if (objetivo.EsPrincipal) _afip.GuardarClave(homologacion, clave);
                        _afip.GuardarClaveCuit(objetivo.Cuit, homologacion, clave);
                    });

                // La empresa queda marcada "con certificado" (habilita la facturacion electronica) solo si no lo estaba. Un titular
                // no principal queda "con certificado" por el registro por CUIT (certificados_arca_cuit) que se guarda arriba.
                if (objetivo.EsPrincipal)
                    Infrastructure.NegocioFactory.CrearCertificadoArcaClave(_sesion.Empresa).MarcarConCertificado(AfipRutas.NombreCertificadoFijo);

                _log.LogInformation("Certificado ARCA instalado: empresa {IdEmpresa}, titular {IdTitular} ({Entorno}), huella {Huella}, vence {Vence:yyyy-MM-dd}, usuario {IdUsuario}.",
                    empresa!.IdEmpresa, objetivo.IdTitular, NombreEntorno(homologacion), resultado.Huella, resultado.Vence, _sesion.UsuarioActual.Id);

                // Recalcula el aviso ya (cierra el del vencimiento anterior).
                _estado.EvaluarAviso(empresa, forzar: true);

                return Volver(entorno, idTitular, mensaje: "Certificado de " + (homologacion ? "pruebas (homologación)" : "producción") + " instalado. Vence el "
                    + resultado.Vence.ToString("dd/MM/yyyy", new CultureInfo("es-AR")) + ". La próxima factura de ese entorno usa el certificado nuevo.");
            }
            catch (CertificadoArcaException ex)
            {
                return Volver(entorno, idTitular, error: ex.Message);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Certificado ARCA: error instalando el certificado de la empresa {IdEmpresa}.", empresa!.IdEmpresa);
                return Volver(entorno, idTitular, error: "No se pudo instalar el certificado. Quedó el anterior sin cambios; revisá el log del servidor.");
            }
        }

        // Probar conexion del entorno: recorre el camino real de la facturacion (certificado -> login en ARCA -> consulta
        // de solo lectura al WSFE por cada punto de venta) SIN emitir nada, y devuelve cada paso con su explicacion.
        // Reusa el ticket vigente (ARCA no entrega otro mientras hay uno valido), asi que no molesta a la facturacion.
        // Sirve tambien para verificar el certificado de un titular nuevo ANTES de usarlo (cada tarjeta prueba su entorno
        // aunque no sea el que usa la empresa).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProbarConexion(string? entorno, int idTitular = 0)
        {
            if (!EsAdmin) return StatusCode(403, new { ok = false, mensaje = "Solo administradores." });
            var empresa = _estado.EmpresaActual();
            if (!PuedeOperar(empresa, idTitular, entorno, out var objetivo, out bool homologacion, out string motivo))
                return Json(new { ok = false, pasos = new[] { new { titulo = "Empresa", ok = false, detalle = motivo } }, homologacion = false });

            try
            {
                var puntosVenta = PuntosVentaAProbar(empresa!, objetivo!);
                var resultado = AfipDiagnostico.ProbarFacturacion(objetivo!.Cuit, objetivo.EsRRII, objetivo.NombreCertificadoPfx, empresa!.Entorno_HOMO_PROD,
                    puntosVenta, _env.ContentRootPath, _afip.Crear(objetivo.ComoEmisor(), homologacion), NombreEntorno(homologacion));
                _log.LogInformation("Certificado ARCA: prueba de conexion de la empresa {IdEmpresa}, titular {IdTitular} ({Entorno}): {Resultado}.", empresa.IdEmpresa, objetivo.IdTitular, NombreEntorno(homologacion), resultado.Ok ? "OK" : "con errores");

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

        // Puntos de venta que se prueban: de un titular no principal, los habilitados de su lista; del principal, los de las
        // sucursales donde el es el titular (y, si ninguna lo usa, todos los de la empresa como hasta ahora).
        private List<int> PuntosVentaAProbar(Entidades.Empresa empresa, Objetivo objetivo)
        {
            if (!objetivo.EsPrincipal && objetivo.Titular != null)
                return objetivo.Titular.PuntosVenta.Where(p => p.Habilitado).Select(p => p.CodPuntoVentaAfip).Distinct().ToList();

            // Sucursales de la empresa (RLS ya limita a la empresa de la sesion).
            var sucursales = Infrastructure.NegocioFactory.CrearSucursal(_sesion.Empresa).findAll();
            var todos = sucursales.Select(s => s.CodPuntoVentaAfip).Where(p => p > 0).Distinct().ToList();
            if (_titulares == null) return todos;

            var principal = _titulares.ObtenerPrincipal(empresa.IdEmpresa);
            if (principal == null) return todos;

            var propios = sucursales
                .Where(s => (_titulares.ObtenerIdTitularDeSucursal(s.IdSucursal) ?? principal.IdTitular) == principal.IdTitular)
                .Select(s => s.CodPuntoVentaAfip).Where(p => p > 0).Distinct().ToList();
            return propios.Count > 0 ? propios : todos;
        }

        // Vuelve a la pantalla, en la pestana del entorno y el titular con los que se estaba trabajando.
        private IActionResult Volver(string? entorno, int idTitular, string? mensaje = null, string? error = null)
        {
            if (mensaje != null) TempData["CertificadoMensaje"] = mensaje;
            if (error != null) TempData["CertificadoError"] = error;
            int? titular = idTitular > 0 ? idTitular : (int?)null;
            return TryEntorno(entorno, out bool homologacion)
                ? RedirectToAction(nameof(Index), new { entorno = NombreEntorno(homologacion), idTitular = titular })
                : RedirectToAction(nameof(Index), new { idTitular = titular });
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

        // La pantalla opera solo sobre la empresa de la sesion y un titular de ella, con CUIT valido, en un ambiente que
        // guarde la clave y con un entorno valido (PROD/HOMO).
        private bool PuedeOperar(Entidades.Empresa? empresa, int idTitular, string? entorno, out Objetivo? objetivo, out bool homologacion, out string motivo, bool requiereAdministrar = false)
        {
            motivo = "";
            homologacion = false;
            objetivo = null;
            if (!_afip.SoportaCertificados) { motivo = "La gestión de certificados requiere Postgres."; return false; }
            objetivo = ResolverObjetivo(empresa, idTitular, out string motivoTitular);
            if (objetivo == null) { motivo = string.IsNullOrEmpty(motivoTitular) ? "La empresa no tiene un CUIT válido cargado." : motivoTitular; return false; }
            if (objetivo.Cuit.ToString().Length != 11) { motivo = objetivo.EsPrincipal ? "La empresa no tiene un CUIT válido cargado." : "El titular no tiene un CUIT válido cargado."; return false; }
            if (!TryEntorno(entorno, out homologacion)) { motivo = "Elegí el entorno del certificado (producción o pruebas)."; return false; }

            // Pedir, instalar o descartar el certificado de un CUIT compartido: solo la empresa administradora (o el super-admin),
            // para que dos empresas no se pisen el pedido pendiente ni el .pfx.
            if (requiereAdministrar && _titulares != null && empresa != null
                && !_titulares.PuedeAdministrarCertificado(objetivo.Cuit, empresa.IdEmpresa) && !EsSuperAdmin)
            {
                motivo = "El certificado de este CUIT lo administra otra empresa: solo ella (o el administrador del sistema) puede pedirlo, instalarlo o descartarlo.";
                return false;
            }
            return true;
        }

        private static string NombreCsr(Objetivo objetivo, bool homologacion, string alias)
        {
            // Solo caracteres seguros en el nombre de archivo.
            string limpio = new string((alias ?? "").Where(c => char.IsLetterOrDigit(c) || c == '-' || c == '_').ToArray());
            return objetivo.Cuit + (homologacion ? "-homo" : "") + (limpio.Length > 0 ? "-" + limpio : "") + ".csr";
        }
    }
}
