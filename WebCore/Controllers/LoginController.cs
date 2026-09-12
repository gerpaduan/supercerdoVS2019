// Port de Web/Controllers/LoginController.cs -- login real con Cookie Authentication de ASP.NET
// Core en vez de Session["Usuario"]+Forms Auth (ver docs/DECISIONS.md "Login/Sesion real para
// WebCore"). Alcance v1 (decidido con el usuario): login + rate limiting por IP + bloqueo de
// cuenta por intentos fallidos + dispositivo seguro + horario laboral. Fuera de alcance v1
// (documentado como gap para v2): geo-validacion de ubicacion de login (ValidarUbicacion,
// LoginUbicacionLog), mail automatico de desbloqueo de cuenta (EnviarMailDesbloqueo) -- el
// desbloqueo en v1 lo hace un admin a mano via UsuariosController.DesbloquearUsuario, que ya
// existe. Recuperacion de contraseña por email (ForgotPassword/ResetPassword) se agrego 2026-09-09
// (item 1 de los 12 pendientes, Batch E, ver docs/DECISIONS.md "Batch E: recuperacion de
// contraseña"), con mejoras de seguridad deliberadas sobre el clasico (confirmadas con el
// usuario): rate limiting propio por IP (PasswordResetRateLimiter, nuevo) y clave minima de 6
// caracteres (el clasico aceptaba 1).
using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Helpers;
using WebCore.Infrastructure;
using WebCore.Models;

namespace WebCore.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Empresa _oEmpresaN;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;

        // _sesion solo lo usa CambiarSucursal (2026-09-07, ver docs/DECISIONS.md): esa accion
        // corre YA logueado, con la empresa real del usuario -- distinto de _oUsuarioN/_oSucursalN
        // de arriba, que a proposito usan EmpresaContextNulo porque Index/ValidarLogin corren
        // ANTES de saber a que empresa pertenece el usuario.
        public LoginController(WebCore.Services.IUsuarioSesionService sesion)
        {
            IEmpresaContext empresaNula = new EmpresaContextNulo();
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresaNula);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(empresaNula);
            _oEmpresaN = WebCore.Infrastructure.NegocioFactory.CrearEmpresa(empresaNula);
            _sesion = sesion;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = "")
        {
            var model = new LoginIndexVm
            {
                ReturnUrl = returnUrl ?? "",
                Error = TempData["Error"] as string,
                Success = TempData["Success"] as string
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginIndexVm model)
        {
            model.Usuario = (model.Usuario ?? "").Trim();
            model.Clave = model.Clave ?? "";
            model.ReturnUrl = model.ReturnUrl ?? "";

            if (!ModelState.IsValid)
                return View(model);

            string ip = ObtenerDireccionIp();

            // Resuelto ANTES del rate limiter por IP: (a) decide si este login viene de un
            // dispositivo seguro y saltea el bloqueo por IP -- NO el bloqueo por cuenta, que sigue
            // igual mas abajo sin excepcion; (b) lo reusa el chequeo de cuenta bloqueada.
            var candidato = _oUsuarioN.ObtenerUsuarioPorIdentificador(model.Usuario);

            var oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(new EmpresaContextNulo());
            bool esDispositivoSeguro = candidato != null
                && !string.IsNullOrWhiteSpace(model.NumeroSerieDispositivo)
                && oDispositivoN.ExisteSerieSegura(model.NumeroSerieDispositivo, candidato.IdEmpresa);

            if (!esDispositivoSeguro && LoginRateLimiter.IsBlocked(ip, model.Usuario, out var retryAfter))
            {
                Response.StatusCode = 429;
                model.Clave = "";
                model.Error = string.Format(
                    CultureInfo.CurrentCulture,
                    "Se supero el maximo de intentos. Espera {0} minuto(s) y volve a intentar.",
                    Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes)));
                return View(model);
            }

            // Cuenta bloqueada por intentos fallidos (distinto del rate limiter por IP de arriba):
            // se chequea antes de validar la contraseña, para no seguir contando intentos contra
            // una cuenta ya bloqueada. Sin mail automatico de desbloqueo en v1 (ver cabecera).
            if (candidato != null && candidato.Activo && candidato.Bloqueado)
            {
                LoginRateLimiter.RegisterFailure(ip, model.Usuario);
                model.Clave = "";
                model.Error = "Tu cuenta está bloqueada por intentos fallidos. Pedile a un administrador que la desbloquee.";
                return View(model);
            }

            var user = _oUsuarioN.ValidarUsuarioWeb(model.Usuario, model.Clave);

            if (user != null && user.Activo)
            {
                IEmpresaContext empresa = new EmpresaContextFijo(user.IdEmpresa);
                var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresa);
                var oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(empresa);

                user = oUsuarioN.getUsuarioById(user.Id, sinRestriccionDeTenant: true);
                if (user == null)
                {
                    model.Clave = "";
                    model.Error = "No fue posible iniciar sesión con los datos ingresados.";
                    return View(model);
                }

                user.Sucursal = user.IdSucursal > 0 ? oSucursalN.findById(user.IdSucursal) : null;
                user.SucursalNombre = user.Sucursal?.SucursalNombre ?? "";
                user.Permisos = oUsuarioN.getPermisosUsuario(user.Id);

                oUsuarioN.RegistrarLoginExitoso(user);

                // Horario laboral (a nivel empresa): un empleado no-admin que intenta loguearse
                // fuera de las 2 jornadas configuradas queda bloqueado aca, ANTES de crear sesion
                // -- no se toca el rate limiter (las credenciales eran correctas).
                if (!user.Admin)
                {
                    var empresaActual = _oEmpresaN.findById(user.IdEmpresa);
                    if (!EstaDentroDelHorarioPermitido(empresaActual, DateTime.Now))
                    {
                        model.Clave = "";
                        model.Error = "Fuera del horario laboral permitido para iniciar sesión.";
                        return View(model);
                    }
                }

                await FirmarCookieAsync(user);
                LoginRateLimiter.Reset(ip, model.Usuario);

                // Modal de sucursal post-login (item 3 de la cuarta ronda de pedidos, 2026-09-10 --
                // ver docs/DECISIONS.md "Batch 2: modal de sucursal automatico al loguearse"). Port
                // de Web/Controllers/LoginController.cs:151-157: si la empresa tiene 2+ sucursales,
                // se avisa siempre en que sucursal va a operar -- sin condicion adicional (no
                // depende de admin ni de cuantas sucursales tenga ESTE usuario en particular).
                var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
                if (sucursalesEmpresa.Count >= 2)
                    TempData["MostrarModalSucursalPostLogin"] = true;

                return RedirigirPostLogin(model.ReturnUrl);
            }

            // Solo cuenta contra el bloqueo persistente si el usuario/email existe y esta activo --
            // no tiene sentido bloquear por intentos contra una cuenta que no existe o inactiva.
            if (candidato != null && candidato.Activo && !candidato.Bloqueado)
            {
                _oUsuarioN.RegistrarIntentoFallido(candidato, GetAccountLockoutMaxAttempts(), sinRestriccionDeTenant: true);
            }

            LoginRateLimiter.RegisterFailure(ip, model.Usuario);
            model.Clave = "";
            model.Error = "No fue posible iniciar sesión con los datos ingresados.";
            return View(model);
        }

        // Mensaje generico deliberado (2026-09-09, port de Web/Controllers/LoginController.cs:22):
        // nunca revela si el usuario/email ingresado existe -- evita que este endpoint sirva para
        // enumerar cuentas reales.
        private const string GenericRecoveryMessage = "Si los datos ingresados corresponden a un usuario registrado, recibirás instrucciones para recuperar tu contraseña. Si no te llega el mail, comunicate con el administrador de tu empresa.";

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new PasswordRecoveryRequestVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(PasswordRecoveryRequestVm model)
        {
            model ??= new PasswordRecoveryRequestVm();
            model.UsuarioOEmail = (model.UsuarioOEmail ?? "").Trim();

            if (!ModelState.IsValid)
                return View(model);

            string ip = ObtenerDireccionIp();

            // Rate limiting por IP (mejora de seguridad nueva, ver cabecera del archivo) -- se
            // registra el intento ANTES de chequear el bloqueo para que el propio intento que
            // dispara el bloqueo ya cuente, igual que LoginRateLimiter.
            if (PasswordResetRateLimiter.IsBlocked(ip, out var retryAfter))
            {
                Response.StatusCode = 429;
                model.Mensaje = string.Format(
                    CultureInfo.CurrentCulture,
                    "Se superó el máximo de solicitudes. Esperá {0} minuto(s) y volvé a intentar.",
                    Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes)));
                return View(model);
            }

            PasswordResetRateLimiter.RegisterRequest(ip);

            try
            {
                var usuarios = _oUsuarioN.BuscarUsuariosPorIdentificador(model.UsuarioOEmail, true)
                    .Where(u => u != null && u.Activo)
                    .ToList();

                foreach (var usuario in usuarios)
                {
                    if (string.IsNullOrWhiteSpace(usuario.Email))
                        continue;

                    string rawToken = PasswordSecurity.GenerateToken();
                    string tokenHash = PasswordSecurity.ComputeSha256Base64(rawToken);
                    int expirationMinutes = GetPasswordResetExpirationMinutes();
                    DateTime nowUtc = DateTime.UtcNow;

                    _oUsuarioN.CrearTokenRecuperacion(new Entidades.UsuarioPasswordResetToken
                    {
                        IdUsuario = usuario.Id,
                        IdEmpresa = usuario.IdEmpresa,
                        TokenHash = tokenHash,
                        FechaCreacionUtc = nowUtc,
                        FechaExpiracionUtc = nowUtc.AddMinutes(expirationMinutes),
                        Usado = false,
                        IdentificadorSolicitado = model.UsuarioOEmail,
                        EmailDestino = usuario.Email ?? ""
                    });

                    if (SmtpMailHelper.IsConfigured())
                    {
                        string resetUrl = Url.Action("ResetPassword", "Login", new { token = rawToken }, Request.Scheme) ?? "";
                        SmtpMailHelper.SendPasswordReset(usuario.Email, usuario.Nombre, resetUrl, expirationMinutes);
                    }
                }
            }
            catch
            {
                // Se responde igual para no filtrar informacion sensible (mismo criterio que
                // clasico) -- un fallo de SMTP/DB no debe revelar si la cuenta existe.
            }

            TempData["Success"] = GenericRecoveryMessage;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = "")
        {
            var model = new PasswordResetVm
            {
                Token = token ?? "",
                TokenValido = TokenEsValido(token, "reset")
            };

            if (!model.TokenValido)
                model.Mensaje = "El enlace de recuperación no es válido o ya venció.";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(PasswordResetVm model)
        {
            model ??= new PasswordResetVm();
            model.Token = model.Token ?? "";
            model.TokenValido = TokenEsValido(model.Token, "reset");

            if (!string.IsNullOrEmpty(model.NuevaClave) && model.NuevaClave.Contains(' '))
                ModelState.AddModelError(nameof(model.NuevaClave), "La contraseña no puede contener espacios en blanco.");

            if (!model.TokenValido)
                ModelState.AddModelError("", "El enlace de recuperación no es válido o ya venció.");

            if (!ModelState.IsValid)
                return View(model);

            string tokenHash = PasswordSecurity.ComputeSha256Base64(model.Token);
            var token = _oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
            if (token == null || token.Usado || token.Proposito != "reset" || token.FechaExpiracionUtc < DateTime.UtcNow)
            {
                model.TokenValido = false;
                model.Mensaje = "El enlace de recuperación no es válido o ya venció.";
                ModelState.AddModelError("", model.Mensaje);
                return View(model);
            }

            var usuario = _oUsuarioN.getUsuarioById(token.IdUsuario, sinRestriccionDeTenant: true);
            if (usuario == null || !usuario.Activo)
            {
                model.TokenValido = false;
                ModelState.AddModelError("", "No fue posible actualizar la contraseña.");
                return View(model);
            }

            _oUsuarioN.ActualizarPasswordWebSeguro(usuario.Id, model.NuevaClave, sinRestriccionDeTenant: true);
            _oUsuarioN.MarcarTokenRecuperacionComoUsado(token.Id);
            _oUsuarioN.InvalidarTokensPendientesUsuario(usuario.Id, "reset");

            TempData["Success"] = "Tu contraseña fue actualizada correctamente. Ya podés iniciar sesión.";
            return RedirectToAction("Index");
        }

        private bool TokenEsValido(string? tokenRaw, string proposito)
        {
            if (string.IsNullOrWhiteSpace(tokenRaw))
                return false;

            try
            {
                string tokenHash = PasswordSecurity.ComputeSha256Base64(tokenRaw);
                var token = _oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
                return token != null && !token.Usado && token.Proposito == proposito && token.FechaExpiracionUtc >= DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private static int GetPasswordResetExpirationMinutes()
        {
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Security:PasswordResetTokenMinutes"], out var minutes) && minutes > 0)
                return minutes;

            return 60;
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // Cambiar clave desde el menu de usuario del POS (2026-09-10, item 6 de la segunda ronda
        // de pedidos, ver docs/DECISIONS.md). Port de Web/Controllers/LoginController.cs:291-337,
        // adaptado a IUsuarioSesionService en vez de Session["Usuario"] (mismo patron que
        // CambiarSucursal, arriba) -- esta accion corre con sesion YA iniciada (a diferencia de
        // ForgotPassword/ResetPassword, pre-login), asi que usa el _Layout.cshtml normal con
        // sidebar, no el layout standalone de login. Mismo criterio de seguridad ya aplicado a
        // PasswordResetVm (Batch E): minimo 6 caracteres, no 1 como el clasico.
        [Authorize]
        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (_sesion.UsuarioActual == null)
                return RedirectToAction("Index");

            var model = new ChangePasswordVm
            {
                Error = TempData["Error"] as string,
                Success = TempData["Success"] as string
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordVm model)
        {
            if (_sesion.UsuarioActual == null)
                return RedirectToAction("Index");

            model ??= new ChangePasswordVm();

            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_sesion.Empresa, _sesion.Parametros);
            var usuarioActual = oUsuarioN.getUsuarioById(_sesion.UsuarioActual.Id);

            if (usuarioActual == null || !usuarioActual.Activo)
                ModelState.AddModelError("", "No fue posible actualizar la contraseña.");
            else if (oUsuarioN.ValidarUsuarioWeb(usuarioActual.User, model.ClaveActual) == null)
                ModelState.AddModelError(nameof(model.ClaveActual), "La contraseña actual es incorrecta.");

            if (!ModelState.IsValid)
                return View(model);

            oUsuarioN.ActualizarPasswordWebSeguro(usuarioActual.Id, model.NuevaClave);
            oUsuarioN.InvalidarTokensPendientesUsuario(usuarioActual.Id, "reset");

            TempData["Success"] = "Tu contraseña fue actualizada correctamente.";
            return RedirectToAction("ChangePassword");
        }

        // Cambiar sucursal desde el menu de usuario (2026-09-07, pedido explicito del usuario --
        // ver docs/DECISIONS.md). Port de Web/Controllers/LoginController.cs:562-600 -- misma
        // logica (persistir en Usuarios.idSucursal + devolver el nombre nuevo), adaptado a
        // IUsuarioSesionService en vez de Session["Usuario"]: el claim IdSucursal de la cookie NO
        // se reemite (nada lo lee para resolver sucursal -- UsuarioSesionService.UsuarioActual
        // relee Usuario/Sucursal frescos de la BD en cada request, asi que el proximo request ya
        // ve el cambio solo con la persistencia).
        [Authorize]
        [HttpPost]
        public IActionResult CambiarSucursal(int idSucursal)
        {
            try
            {
                var usuario = _sesion.UsuarioActual;
                var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_sesion.Empresa, _sesion.Parametros);
                var oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_sesion.Empresa, _sesion.Parametros);

                var sucursal = oSucursalN.findById(idSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida" });

                usuario.IdSucursal = sucursal.IdSucursal;
                usuario.Sucursal = sucursal;
                usuario.SucursalNombre = sucursal.SucursalNombre;

                oUsuarioN.setSucursalUsuario(usuario);

                return Json(new { ok = true, sucursalNombre = usuario.SucursalNombre, idSucursal = usuario.IdSucursal });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, msg = ex.Message });
            }
        }

        private async Task FirmarCookieAsync(Entidades.Usuario user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Name, user.User ?? ""),
                new Claim(SesionClaims.NombreCompleto, user.Nombre ?? ""),
                new Claim(SesionClaims.IdEmpresa, user.IdEmpresa.ToString(CultureInfo.InvariantCulture)),
                new Claim(SesionClaims.IdSucursal, user.IdSucursal.ToString(CultureInfo.InvariantCulture)),
                new Claim(SesionClaims.Admin, user.Admin ? "true" : "false"),
                new Claim(SesionClaims.EsUsuarioProduccion, user.EsUsuarioProduccion ? "true" : "false"),
                new Claim(SesionClaims.PermitirLoginFueraSucursal, user.PermitirLoginFueraSucursal ? "true" : "false"),
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = true
            });
        }

        private IActionResult RedirigirPostLogin(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Redirect LITERAL, no RedirectToAction("Index","Home") (2026-09-07, bug real
            // encontrado por el usuario -- ver docs/DECISIONS.md): controller=Home/action=Index
            // son los DEFAULTS de la ruta "default" (Program.cs), asi que el generador de links de
            // ASP.NET Core colapsa esa URL a "/" -- que ahora sirve la landing publica
            // (LandingController, ruta "PublicHome"), no el dashboard. El login terminaba
            // mostrando el landing en vez de mandar al usuario ya logueado al Home real.
            return Redirect("/Home/Index");
        }

        // Compara la hora actual del servidor contra las 2 jornadas configuradas en la empresa.
        // Admin ya viene exceptuado por el caller.
        private static bool EstaDentroDelHorarioPermitido(Entidades.Empresa? empresa, DateTime ahora)
        {
            if (empresa == null)
                return true; // fail-open: no se pudo resolver la empresa, no bloquear por un dato faltante

            var horaActual = ahora.TimeOfDay;
            return EstaEnJornada(horaActual, empresa.HorarioDiurnoDesde, empresa.HorarioDiurnoHasta)
                || EstaEnJornada(horaActual, empresa.HorarioTardeDesde, empresa.HorarioTardeHasta);
        }

        private static bool EstaEnJornada(TimeSpan horaActual, TimeSpan desde, TimeSpan hasta)
        {
            if (desde <= hasta)
                return horaActual >= desde && horaActual <= hasta;

            // Jornada que cruza medianoche (ej. 22:00-02:00) -- no es el caso por defecto.
            return horaActual >= desde || horaActual <= hasta;
        }

        private int GetAccountLockoutMaxAttempts()
        {
            if (int.TryParse(System.Configuration.ConfigurationManager.AppSettings["Security:AccountLockoutMaxAttempts"], out var intentos) && intentos > 0)
                return intentos;

            return 5;
        }

        private string ObtenerDireccionIp()
        {
            var forwardedFor = Request.Headers["X-Forwarded-For"].ToString();
            if (!string.IsNullOrWhiteSpace(forwardedFor))
            {
                var primero = forwardedFor.Split(',').Select(x => x.Trim()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
                if (!string.IsNullOrWhiteSpace(primero))
                    return primero;
            }

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }

        // IEmpresaContext fijo (no depende de HttpContext) -- usado durante el POST de login, antes
        // de que la cookie/claims existan, para resolver el usuario ya con su tenant real conocido.
        private sealed class EmpresaContextFijo : IEmpresaContext
        {
            public EmpresaContextFijo(int idEmpresa) => IdEmpresa = idEmpresa;
            public int IdEmpresa { get; }
        }
    }
}
