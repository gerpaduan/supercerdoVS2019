// Port de Web/Controllers/DispositivosSegurosController.cs (ver docs/DECISIONS.md, migracion
// ASP.NET Core, Modulo 6 -- Reportes y administracion). Registro de PCs conocidas (numero de
// serie via agente local) que saltan el bloqueo por IP del login. Usuario/empresa reales via
// IUsuarioSesionService (login real, ver docs/DECISIONS.md 2026-09-06) -- ya no hay stub
// hardcodeado.
using System;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
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

        [HttpGet]
        public IActionResult Index()
        {
            var model = new DispositivosSegurosIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(_usuarioActual),
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
            if (!PuedeAdministrar(_usuarioActual))
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
                    IdUsuarioCreador = _usuarioActual.Id
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
            if (!PuedeAdministrar(_usuarioActual))
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
            if (!PuedeAdministrar(_usuarioActual))
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
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para autorizar dispositivos.";
                return RedirectToAction("Index");
            }

            var solicitud = _oDispositivoN.ObtenerSolicitud(id, _empresa.IdEmpresa);
            bool aprobada = _oDispositivoN.AprobarSolicitud(id, _empresa.IdEmpresa, _usuarioActual.Id, descripcion ?? "");
            if (aprobada && solicitud != null)
                RegistrarEventoSolicitud(solicitud, "Dispositivo autorizado por administrador (" + (_usuarioActual.User ?? "") + "): " + solicitud.Nombre);

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
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para rechazar solicitudes.";
                return RedirectToAction("Index");
            }

            var solicitud = _oDispositivoN.ObtenerSolicitud(id, _empresa.IdEmpresa);
            bool rechazada = _oDispositivoN.RechazarSolicitud(id, _empresa.IdEmpresa, _usuarioActual.Id);
            if (rechazada && solicitud != null)
                RegistrarEventoSolicitud(solicitud, "Solicitud de dispositivo rechazada por administrador (" + (_usuarioActual.User ?? "") + "): " + solicitud.Nombre);

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
            if (!PuedeAdministrar(_usuarioActual))
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

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == _empresa.IdEmpresa && usuario.Admin;
        }
    }
}
