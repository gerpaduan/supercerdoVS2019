// Port de Web/Controllers/DispositivosSegurosController.cs (ver docs/DECISIONS.md, migracion
// ASP.NET Core, Modulo 6 -- Reportes y administracion). Registro de PCs conocidas (numero de
// serie via agente local) que saltan el bloqueo por IP del login. Usuario/empresa reales via
// IUsuarioSesionService (login real, ver docs/DECISIONS.md 2026-09-06) -- ya no hay stub
// hardcodeado.
//
// Acceso (2026-10-22, ver docs/DECISIONS.md): el menu Configuracion -> Dispositivos seguros lo ve todo
// usuario, pero la pantalla solo opera con un ADMINISTRADOR: si la sesion es admin entra directo; si no
// (usuario de produccion o comun) se pide usuario y clave de un admin ("operador", mismo login que Ventas/
// Compras) y queda activo en Session hasta salir de la pantalla (PermisosHelper.LimpiarOperadorModuloSiSalioDelModulo).
// Lo que se hace en la pantalla queda a nombre del operador, no de la cuenta de sesion.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Helpers;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class DispositivosSegurosController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly Negocio.DispositivoSeguro _oDispositivoN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public DispositivosSegurosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(_empresa);
        }

        // Clave del operador en Session ("OperadorModulo_DispositivosSeguros"): el nombre del modulo debe ser
        // igual al del controller para que el layout lo limpie al salir de la pantalla.
        private const string ModuloOperador = "DispositivosSeguros";

        // Quien opera la pantalla: el usuario de sesion si es admin; si no, el admin que se logueo como
        // operador (null si todavia no lo hizo).
        private Entidades.Usuario OperadorActivo
        {
            get
            {
                if (PuedeAdministrar(_usuarioActual)) return _usuarioActual;
                return PermisosHelper.ObtenerOperadorModulo(HttpContext.Session, ModuloOperador);
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Sin admin (de sesion u operador) no se muestra nada: se pide el login de un administrador.
            if (!PuedeAdministrar(OperadorActivo))
                return RedirectToAction("AutorizarModulo", new { returnUrl = Url.Action("Index", "DispositivosSeguros") });

            var model = new DispositivosSegurosIndexVm
            {
                PuedeAdministrar = true,
                Items = _oDispositivoN.Listar(_empresa.IdEmpresa)
            };

            // Solicitudes pendientes (Fase 1c). Solo las ve quien puede administrar. Si la tabla todavia no
            // existe en este servidor (script pendiente), la pantalla sigue funcionando sin la seccion.
            if (model.PuedeAdministrar)
            {
                try
                {
                    model.Solicitudes = _oDispositivoN.ListarSolicitudesPendientes(_empresa.IdEmpresa);
                }
                catch (Exception ex)
                {
                    HttpContext.RequestServices.GetRequiredService<ILogger<DispositivosSegurosController>>()
                        .LogWarning(ex, "No se pudieron listar las solicitudes de dispositivo pendientes.");
                }
            }

            ViewBag.Title = "Dispositivos seguros";
            ViewBag.Seccion = "Dispositivos seguros";
            return View("~/Views/DispositivosSeguros/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(string numeroSerie, string descripcion)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para agregar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            numeroSerie = (numeroSerie ?? "").Trim();
            if (string.IsNullOrWhiteSpace(numeroSerie))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El número de serie es obligatorio.";
                return RedirectToAction("Index");
            }

            try
            {
                _oDispositivoN.Agregar(new Entidades.DispositivoSeguro
                {
                    IdEmpresa = _empresa.IdEmpresa,
                    NumeroSerie = numeroSerie,
                    Descripcion = (descripcion ?? "").Trim(),
                    IdUsuarioCreador = OperadorActivo.Id
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El dispositivo se agregó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "No se pudo agregar el dispositivo: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para eliminar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            _oDispositivoN.Eliminar(id, _empresa.IdEmpresa);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = "El dispositivo se eliminó correctamente.";
            return RedirectToAction("Index");
        }

        // Bloquear / desbloquear un dispositivo (login solo desde dispositivos seguros, 2026-09-19).
        // Un dispositivo bloqueado no cuenta como seguro y no se puede re-autorizar por mail; para
        // que el usuario vuelva a usarlo hay que desbloquearlo aca.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarBloqueo(int id, bool bloquear)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para bloquear dispositivos seguros.";
                return RedirectToAction("Index");
            }

            _oDispositivoN.SetBloqueado(id, _empresa.IdEmpresa, bloquear);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = bloquear ? "El dispositivo se bloqueó: ya no puede usarse para ingresar." : "El dispositivo se desbloqueó.";
            return RedirectToAction("Index");
        }

        // Solicitudes de autorizacion de dispositivo (2026-10-02, Fase 1c, ver docs/DECISIONS.md "Login
        // por CUIT, clave rapida (PIN) y politica de clave"). Aprobar da de alta el dispositivo
        // (Origen="Solicitud"); la descripcion se puede editar antes de aprobar. Ambas acciones dejan
        // constancia en la auditoria de accesos.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AprobarSolicitud(int id, string? descripcion)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para autorizar dispositivos.";
                return RedirectToAction("Index");
            }

            var solicitud = _oDispositivoN.ObtenerSolicitud(id, _empresa.IdEmpresa);
            var operador = OperadorActivo;
            bool aprobada = _oDispositivoN.AprobarSolicitud(id, _empresa.IdEmpresa, operador.Id, descripcion ?? "");
            if (aprobada && solicitud != null)
                RegistrarEventoSolicitud(solicitud, "Dispositivo autorizado por administrador (" + (operador.User ?? "") + "): " + solicitud.Nombre);

            TempData["AlertType"] = aprobada ? "success" : "warning";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = aprobada
                ? "Dispositivo autorizado. El usuario ya puede ingresar desde ahí."
                : "No se pudo aprobar: la solicitud ya fue resuelta o el dispositivo está bloqueado (desbloquealo primero desde el listado).";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RechazarSolicitud(int id)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para rechazar solicitudes.";
                return RedirectToAction("Index");
            }

            var solicitud = _oDispositivoN.ObtenerSolicitud(id, _empresa.IdEmpresa);
            var operador = OperadorActivo;
            bool rechazada = _oDispositivoN.RechazarSolicitud(id, _empresa.IdEmpresa, operador.Id);
            if (rechazada && solicitud != null)
                RegistrarEventoSolicitud(solicitud, "Solicitud de dispositivo rechazada por administrador (" + (operador.User ?? "") + "): " + solicitud.Nombre);

            TempData["AlertType"] = rechazada ? "success" : "warning";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = rechazada ? "Solicitud rechazada." : "La solicitud ya fue resuelta.";
            return RedirectToAction("Index");
        }

        // Auditoria de accesos (loginubicacionlog), a nombre del usuario que hizo la solicitud: asi el
        // listado de "Auditoria de accesos" muestra quien pidio y quien resolvio. Nunca debe impedir la
        // accion del admin: si falla, solo se deja en el log.
        private void RegistrarEventoSolicitud(Entidades.DispositivoSolicitud solicitud, string motivo)
        {
            try
            {
                WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa).RegistrarLoginUbicacion(new Entidades.LoginUbicacionLog
                {
                    IdUsuario = solicitud.IdUsuario,
                    FechaHora = DateTime.Now,
                    Permitido = true,
                    Motivo = motivo.Length > 300 ? motivo.Substring(0, 300) : motivo,
                    Ip = WebCore.Helpers.ClientIp.Obtener(HttpContext)
                });
            }
            catch (Exception ex)
            {
                HttpContext.RequestServices.GetRequiredService<ILogger<DispositivosSegurosController>>()
                    .LogWarning(ex, "No se pudo registrar el evento de auditoria de la solicitud {IdSolicitud}.", solicitud.Id);
            }
        }

        // Fichaje de jornada (2026-09-29, ver docs/DECISIONS.md): independiente de CambiarBloqueo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarHabilitadoFichaje(int id, bool habilitar)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para habilitar dispositivos para fichaje.";
                return RedirectToAction("Index");
            }

            _oDispositivoN.SetHabilitadoFichaje(id, _empresa.IdEmpresa, habilitar);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = habilitar
                ? "El dispositivo quedó habilitado para fichar entrada/salida."
                : "El dispositivo ya no está habilitado para fichar.";
            return RedirectToAction("Index");
        }

        // Bloqueo por inactividad (2026-10-15, ver docs/DECISIONS.md): "No bloquear sesion" para la PC
        // propia de un admin. Rige desde el proximo login desde ese dispositivo (viaja como claim).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarNoBloquearSesion(int id, bool noBloquear)
        {
            if (!PuedeAdministrar(OperadorActivo))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para cambiar el bloqueo de sesión de dispositivos.";
                return RedirectToAction("Index");
            }

            _oDispositivoN.SetNoBloquearSesion(id, _empresa.IdEmpresa, noBloquear);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = noBloquear
                ? "La sesión ya no se bloqueará por inactividad en este dispositivo (rige desde el próximo inicio de sesión)."
                : "La sesión volverá a bloquearse por inactividad en este dispositivo (rige desde el próximo inicio de sesión).";
            return RedirectToAction("Index");
        }

        // ===== Login del administrador (operador) para usuarios no admin =====
        // Mismo patron que ComprasController/VentasController.AutorizarModulo (pantalla dedicada + modal de
        // seleccion de usuario con clave + limite de intentos por sesion), con una regla extra: el operador
        // DEBE ser administrador de esta empresa.
        [HttpGet]
        public IActionResult AutorizarModulo(string? returnUrl)
        {
            // Ya hay admin (de sesion u operador): no hay nada que autorizar.
            string destino = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? returnUrl
                : Url.Action("Index", "DispositivosSeguros")!;
            if (PuedeAdministrar(OperadorActivo)) return LocalRedirect(destino);

            ViewBag.UsuariosAdminEmpresa = ObtenerAdministradoresParaCombo();
            ViewBag.ReturnUrlModulo = destino;
            ViewBag.Title = "Autorizar administrador";
            ViewBag.Seccion = "Dispositivos seguros";
            return View("~/Views/DispositivosSeguros/AutorizarModulo.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult AutorizarOperador(int idUsuario, string? clave)
        {
            string sessionId = ObtenerSessionIdEstable();
            if (PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return Json(new { ok = false, bloqueado = true, segundosRestantes = (int)Math.Ceiling(retryAfter.TotalSeconds) });

            // Mensaje unico para usuario inexistente, clave mala o usuario que no es admin: no dar pistas.
            const string mensajeGenerico = "Usuario o contraseña incorrectos.";
            if (idUsuario <= 0 || string.IsNullOrWhiteSpace(clave))
                return Json(PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            var usuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa);
            var candidato = usuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo)
                return Json(PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            var validado = usuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo || !PuedeAdministrar(validado))
                return Json(PosOperadorStepUpRateLimiter.RegisterFailure(sessionId).ComoRespuestaJson(mensajeGenerico));

            PosOperadorStepUpRateLimiter.Reset(sessionId);
            PermisosHelper.RegistrarOperadorModulo(HttpContext.Session, ModuloOperador, validado);
            return Json(new { ok = true, nombre = validado.Nombre });
        }

        // ASP.NET Core Session no manda el Set-Cookie hasta el primer write: sin esto el rate-limit por
        // sesion nunca acumula (ver el mismo helper en VentasController/ComprasController).
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // Solo administradores activos de la empresa (columna "admin"): son los unicos que pueden autorizar.
        // OJO: el parametro soloAdmin de obtenerUsuarios NO sirve aca, trae unicamente la cuenta global
        // "admin". Si la consulta no trae las columnas esperadas, el combo queda vacio y no se puede entrar
        // (falla cerrado).
        private List<object> ObtenerAdministradoresParaCombo()
        {
            var dt = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa).obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre") || !dt.Columns.Contains("admin"))
                return new List<object>();

            return dt.AsEnumerable()
                .Where(row => row["id"] != DBNull.Value && row["nombre"] != DBNull.Value
                    && row["admin"] != DBNull.Value && Convert.ToBoolean(row["admin"]))
                .Select(row => new { id = Convert.ToInt32(row["id"]), nombre = Convert.ToString(row["nombre"]) ?? "" })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }
    }
}
