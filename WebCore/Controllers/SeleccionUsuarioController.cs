using System;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Utilidades;

namespace WebCore.Controllers
{
    // Port de Web/Controllers/SeleccionUsuarioController.cs (2026-09-09, ver docs/DECISIONS.md
    // "Batch B: permisos reales + operador de produccion"). Pantalla compartida de "quien esta
    // haciendo esto" (SIN contrasena) para Movimientos/Stock/Elaborados cuando la sesion es la
    // cuenta compartida de produccion -- se pide ANTES de entrar a la vista de edicion. Distinto
    // del step-up CON contrasena de Ventas/Compras/PuntosExpendio (ResolverOperadorModulo/POS,
    // ya portado): aca no se valida nada del lado del servidor, el id elegido se revalida igual
    // que siempre al momento de guardar (ResolverUsuarioCreador, ya portado en cada controller).
    // Reusa el mismo _ModalSeleccionUsuario.cshtml/seleccion-usuario.js que Ventas/PuntosExpendio.
    public class SeleccionUsuarioController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        public SeleccionUsuarioController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();
        }

        public IActionResult Index(string returnUrl, string cancelUrl)
        {
            var user = _sesion.UsuarioActual;

            ViewBag.UsuariosActivosEmpresa = ObtenerUsuariosActivosEmpresaParaCombo(user);
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            ViewBag.CancelUrl = Url.IsLocalUrl(cancelUrl) ? cancelUrl : Url.Action("Index", "Home");
            ViewBag.Title = "Seleccionar usuario";

            return View("~/Views/SeleccionUsuario/Index.cshtml");
        }

        // ===== Clave para modificar un registro anterior a hoy (docs/DECISIONS.md, 2026-10-15) =====
        // La cuenta de produccion eligio su nombre sin clave; si ella sola no puede tocar un registro
        // anterior a hoy, los controllers de Movimientos/Stock/Elaborados mandan aca para que esa persona
        // ponga SU clave (ver ElevacionRegistroHelper). Con la clave validada vuelve a returnUrl y el
        // controller evalua el permiso por fecha de esa persona.
        private const string MensajeClaveIncorrecta = "La clave es incorrecta.";

        [HttpGet]
        public IActionResult ClaveParaModificar(string modulo, int idUsuario, string returnUrl, string cancelUrl)
        {
            var candidato = ObtenerCandidatoClave(modulo, idUsuario);
            if (candidato == null)
                return RedirectDeVuelta(cancelUrl, "No se pudo identificar al usuario. Elegilo de nuevo.");

            return VistaClave(modulo, candidato, returnUrl, cancelUrl, null, 0);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarClaveParaModificar(string modulo, int idUsuario, string clave, string returnUrl, string cancelUrl)
        {
            var candidato = ObtenerCandidatoClave(modulo, idUsuario);
            if (candidato == null)
                return RedirectDeVuelta(cancelUrl, "No se pudo identificar al usuario. Elegilo de nuevo.");

            // Limite de intentos por sesion, el mismo del step-up de Ventas/Compras/POS.
            string sessionId = ObtenerSessionIdEstable();
            if (WebCore.Helpers.PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return VistaClave(modulo, candidato, returnUrl, cancelUrl,
                    "Demasiados intentos. Esperá un momento antes de volver a probar.", (int)Math.Ceiling(retryAfter.TotalSeconds));

            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var validado = string.IsNullOrWhiteSpace(clave) ? null : oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo || validado.Id != candidato.Id)
            {
                WebCore.Helpers.PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return VistaClave(modulo, candidato, returnUrl, cancelUrl, MensajeClaveIncorrecta, 0);
            }

            WebCore.Helpers.PosOperadorStepUpRateLimiter.Reset(sessionId);
            WebCore.Helpers.ElevacionRegistroHelper.Registrar(HttpContext.Session, modulo, validado);
            return Redirect(Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home"));
        }

        // Persona elegida en pantalla, solo si la sesion es la cuenta de produccion, el modulo es uno de los
        // tres y la persona esta activa y es de la misma empresa (mismo criterio que ResolverUsuarioCreador).
        private Entidades.Usuario ObtenerCandidatoClave(string modulo, int idUsuario)
        {
            var sesion = _sesion.UsuarioActual;
            if (sesion == null || !sesion.EsUsuarioProduccion || idUsuario <= 0
                || !WebCore.Helpers.ElevacionRegistroHelper.EsModuloValido(modulo))
                return null;

            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var candidato = oUsuarioN.getUsuarioById(idUsuario);
            return candidato != null && candidato.Activo && candidato.IdEmpresa == sesion.IdEmpresa ? candidato : null;
        }

        private IActionResult VistaClave(string modulo, Entidades.Usuario candidato, string returnUrl, string cancelUrl, string error, int segundosBloqueo)
        {
            ViewBag.Title = "Ingresá tu clave";
            ViewBag.Modulo = modulo;
            ViewBag.IdUsuario = candidato.Id;
            ViewBag.NombreUsuario = candidato.Nombre;
            ViewBag.ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Action("Index", "Home");
            ViewBag.CancelUrl = Url.IsLocalUrl(cancelUrl) ? cancelUrl : Url.Action("Index", "Home");
            ViewBag.Error = error;
            ViewBag.SegundosBloqueo = segundosBloqueo;
            return View("~/Views/SeleccionUsuario/ClaveParaModificar.cshtml");
        }

        private IActionResult RedirectDeVuelta(string cancelUrl, string mensaje)
        {
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Usuario";
            TempData["AlertMsg"] = mensaje;
            return Redirect(Url.IsLocalUrl(cancelUrl) ? cancelUrl : Url.Action("Index", "Home"));
        }

        // Ver ComprasController.ObtenerSessionIdEstable: Session no manda la cookie hasta el primer write, y sin
        // eso el limite de intentos por sesion nunca acumula.
        private string ObtenerSessionIdEstable()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("_estable")))
                HttpContext.Session.SetString("_estable", "1");
            return HttpContext.Session.Id;
        }

        // Mismo criterio que el resto de los controllers de esta migracion (Movimientos/Stock/
        // Elaborados/Compras/Cajas ya lo duplican cada uno por su cuenta -- no hay BaseController
        // compartido en WebCore, ver comentario de cabecera de cualquiera de esos archivos).
        private System.Collections.Generic.List<object> ObtenerUsuariosActivosEmpresaParaCombo(Entidades.Usuario usuarioSesion)
        {
            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            var dt = oUsuarioN.obtenerUsuarios(true);
            if (dt == null || !dt.Columns.Contains("id") || !dt.Columns.Contains("nombre"))
                return new System.Collections.Generic.List<object>();

            return dt.AsEnumerable()
                .Select(row => new
                {
                    id = dt.Columns.Contains("id") && int.TryParse(Convert.ToString(row["id"]), out int id) ? id : 0,
                    nombre = dt.Columns.Contains("nombre") ? Convert.ToString(row["nombre"]) ?? "" : ""
                })
                .Where(u => u.id > 0 && !string.IsNullOrWhiteSpace(u.nombre))
                .OrderBy(u => u.nombre, StringComparer.OrdinalIgnoreCase)
                .Cast<object>()
                .ToList();
        }
    }
}
