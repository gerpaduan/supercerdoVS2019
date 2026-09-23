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
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Helpers;
using WebCore.Infrastructure;
using WebCore.Models;
using VerificacionDispositivoStore = Negocio.VerificacionDispositivoStore;

namespace WebCore.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Empresa _oEmpresaN;
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly ILogger<LoginController> _logger;

        // _sesion solo lo usa CambiarSucursal (2026-09-07, ver docs/DECISIONS.md): esa accion
        // corre YA logueado, con la empresa real del usuario -- distinto de _oUsuarioN/_oSucursalN
        // de arriba, que a proposito usan EmpresaContextNulo porque Index/ValidarLogin corren
        // ANTES de saber a que empresa pertenece el usuario.
        public LoginController(WebCore.Services.IUsuarioSesionService sesion, ILogger<LoginController> logger)
        {
            IEmpresaContext empresaNula = new EmpresaContextNulo();
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresaNula);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(empresaNula);
            _oEmpresaN = WebCore.Infrastructure.NegocioFactory.CrearEmpresa(empresaNula);
            _sesion = sesion;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index(string returnUrl = "")
        {
            // Deja emitida la cookie de identificacion del navegador (dispositivo seguro) antes del POST.
            DispositivoNavegador.AsegurarToken(HttpContext);

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

            // Un dispositivo es "seguro" por (a) el ID de hardware que informa el agente de impresion
            // (PC) o (b) la cookie del navegador ya autorizada (celular / PC sin agente). Un
            // dispositivo bloqueado por el admin no cuenta como seguro ni se puede re-autorizar.
            string tokenNavegador = DispositivoNavegador.AsegurarToken(HttpContext);
            string serieToken = Negocio.DispositivoSeguro.SerieDeToken(tokenNavegador);
            Entidades.DispositivoSeguro? dispositivoAutorizado = null;
            Entidades.DispositivoSeguro? dispositivoBloqueado = null;
            if (candidato != null)
                (dispositivoAutorizado, dispositivoBloqueado) = ResolverDispositivo(candidato.IdEmpresa, model.NumeroSerieDispositivo, serieToken);
            bool esDispositivoSeguro = dispositivoAutorizado != null;

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
                switch (EvaluarReglasPostCredencial(user, esDispositivoSeguro))
                {
                    case ReglaPostCredencial.FueraDeHorario:
                        model.Clave = "";
                        model.Error = MensajeFueraDeHorario;
                        return View(model);
                    case ReglaPostCredencial.DispositivoNoAutorizado:
                        return DispositivoNoAutorizadoTrasClaveValida(user, ip, model, serieToken, dispositivoBloqueado);
                }

                return await CompletarLoginAsync(
                    user, model.ReturnUrl, ip, model.Usuario, dispositivoAutorizado,
                    dispositivoAutorizado != null ? "Login desde dispositivo seguro" : "Login correcto");
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

        // ---------------------------------------------------------------------------------
        // Login por huella (passkeys WebAuthn, 2026-09-21, ver docs/DECISIONS.md "Login por huella
        // (passkeys)"). Credencial "descubrible": el usuario no escribe nada, el navegador le
        // muestra sus huellas registradas para este sitio y la firma identifica al usuario. La
        // huella reemplaza SOLO la contraseña: cuenta activa/bloqueada, rate limit, horario
        // laboral, dispositivo seguro y auditoria se siguen aplicando (EvaluarReglasPostCredencial,
        // CompletarLoginAsync). Las dos acciones responden JSON: el JS de Login/Index navega a la
        // URL que devuelven. El registro de una huella nueva vive en PasskeyController (con sesion).
        // ---------------------------------------------------------------------------------

        private const string SessionAssertionOptions = "fido2.assertionOptions";

        // Clave que se usa en LoginRateLimiter para los intentos por huella (no hay usuario tipeado);
        // el limite efectivo es por IP.
        private const string ClaveRateLimiterHuella = "(huella)";

        private const string MensajeHuellaInvalida = "No fue posible iniciar sesión con la huella. Probá de nuevo o ingresá con usuario y contraseña.";

        private JsonResult PasskeyError(string mensaje, int status = 400)
        {
            Response.StatusCode = status;
            return Json(new { ok = false, error = mensaje });
        }

        // Paso 1: desafio para que el navegador pida la huella. El desafio se guarda en la Session
        // y se consume una sola vez en PasskeyLogin.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PasskeyOptions()
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            if (LoginRateLimiter.IsBlocked(ObtenerDireccionIp(), ClaveRateLimiterHuella, out var retryAfter))
                return PasskeyError(MensajeRateLimit(retryAfter), 429);

            var fido2 = HttpContext.RequestServices.GetRequiredService<IFido2>();
            var options = fido2.GetAssertionOptions(new GetAssertionOptionsParams
            {
                // Lista vacia = credencial descubrible (sin pedir usuario antes).
                AllowedCredentials = new List<PublicKeyCredentialDescriptor>(),
                // Exige que el autenticador verifique al usuario (huella / PIN), no solo presencia.
                UserVerification = UserVerificationRequirement.Required
            });

            HttpContext.Session.SetString(SessionAssertionOptions, options.ToJson());
            return Json(options);
        }

        // Paso 2: verifica la firma que devolvio el autenticador y, si es valida, inicia sesion.
        // serieDispositivo = ID de hardware que informa el agente de impresion (igual que el login
        // por clave, para que el dispositivo seguro se reconozca tambien con la huella).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PasskeyLogin(
            [FromBody] AuthenticatorAssertionRawResponse clientResponse, string? returnUrl = "", string? serieDispositivo = "")
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            string ip = ObtenerDireccionIp();
            if (LoginRateLimiter.IsBlocked(ip, ClaveRateLimiterHuella, out var retryAfter))
                return PasskeyError(MensajeRateLimit(retryAfter), 429);

            // Desafio de un solo uso: se borra siempre, aunque la verificacion falle.
            string? optionsJson = HttpContext.Session.GetString(SessionAssertionOptions);
            HttpContext.Session.Remove(SessionAssertionOptions);
            if (string.IsNullOrEmpty(optionsJson))
                return PasskeyError("La solicitud venció. Volvé a intentar.");

            var options = AssertionOptions.FromJson(optionsJson);
            var oPasskeyN = WebCore.Infrastructure.NegocioFactory.CrearUsuarioPasskey(new EmpresaContextNulo());

            // El tenant sale de la propia credencial (login sin usuario tipeado, tenant desconocido).
            var guardada = oPasskeyN.ObtenerPorCredentialIdSinTenant(clientResponse.RawId);
            if (guardada == null)
            {
                LoginRateLimiter.RegisterFailure(ip, ClaveRateLimiterHuella);
                return PasskeyError(MensajeHuellaInvalida, 401);
            }

            VerifyAssertionResult resultado;
            try
            {
                var fido2 = HttpContext.RequestServices.GetRequiredService<IFido2>();
                resultado = await fido2.MakeAssertionAsync(new MakeAssertionParams
                {
                    AssertionResponse = clientResponse,
                    OriginalOptions = options,
                    StoredPublicKey = guardada.PublicKey,
                    StoredSignatureCounter = (uint)guardada.SignCount,
                    // La credencial debe pertenecer al userHandle que informa el autenticador.
                    IsUserHandleOwnerOfCredentialIdCallback = (args, cancellationToken) => Task.FromResult(
                        args.UserHandle != null
                        && args.UserHandle.AsSpan().SequenceEqual(guardada.UserHandle)
                        && args.CredentialId.AsSpan().SequenceEqual(guardada.CredentialId))
                });
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "Verificacion de huella fallida para la passkey {IdPasskey} (usuario {IdUsuario}).", guardada.Id, guardada.IdUsuario);
                LoginRateLimiter.RegisterFailure(ip, ClaveRateLimiterHuella);
                return PasskeyError(MensajeHuellaInvalida, 401);
            }

            var oUsuarioN = CrearUsuarioNegocio(guardada.IdEmpresa);
            var user = CargarUsuarioParaSesion(oUsuarioN, guardada.IdUsuario, guardada.IdEmpresa);
            if (user == null || !user.Activo)
            {
                LoginRateLimiter.RegisterFailure(ip, ClaveRateLimiterHuella);
                return PasskeyError(MensajeHuellaInvalida, 401);
            }

            if (user.Bloqueado)
            {
                LoginRateLimiter.RegisterFailure(ip, ClaveRateLimiterHuella);
                return PasskeyError("Tu cuenta está bloqueada por intentos fallidos. Pedile a un administrador que la desbloquee.", 403);
            }

            // Firma valida: se registra el uso (el contador detecta credenciales clonadas).
            oPasskeyN.RegistrarUsoSinTenant(guardada.Id, resultado.SignCount);
            oUsuarioN.RegistrarLoginExitoso(user);

            // Mismo criterio de dispositivo seguro que el login por clave.
            string serieToken = Negocio.DispositivoSeguro.SerieDeToken(DispositivoNavegador.AsegurarToken(HttpContext));
            var (dispositivoAutorizado, dispositivoBloqueado) = ResolverDispositivo(user.IdEmpresa, serieDispositivo, serieToken);

            switch (EvaluarReglasPostCredencial(user, dispositivoAutorizado != null))
            {
                case ReglaPostCredencial.FueraDeHorario:
                    return PasskeyError(MensajeFueraDeHorario, 403);
                case ReglaPostCredencial.DispositivoNoAutorizado:
                    if (dispositivoBloqueado != null)
                    {
                        RegistrarAcceso(user, false, "Dispositivo bloqueado por el administrador", ip, dispositivoBloqueado);
                        return PasskeyError("Este dispositivo fue bloqueado por el administrador. Pedile que lo habilite o ingresá desde otro dispositivo autorizado.", 403);
                    }

                    // Mismo flujo que el login por clave: codigo por mail para autorizar el dispositivo.
                    string nonce = VerificacionDispositivoStore.Instancia.CrearPedido(user.Id, user.IdEmpresa, serieToken, returnUrl ?? "");
                    DispositivoNavegador.GuardarNoncePedido(HttpContext, nonce);
                    RegistrarAcceso(user, false, "Dispositivo no autorizado", ip, null);
                    return Json(new { ok = true, redirectUrl = Url.Action(nameof(DispositivoNoAutorizado)) });
            }

            var destino = await CompletarLoginAsync(user, returnUrl ?? "", ip, ClaveRateLimiterHuella, dispositivoAutorizado, "Login con huella");
            return Json(new { ok = true, redirectUrl = (destino as RedirectResult)?.Url ?? Url.Content("~/Home/Index") });
        }

        private static string MensajeRateLimit(TimeSpan retryAfter)
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "Se superó el máximo de intentos. Esperá {0} minuto(s) y volvé a intentar.",
                Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes)));
        }

        public async Task<IActionResult> Logout()
        {
            RegistrarLogoutConVentaEnCurso();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        // Ventas en curso (ver docs/DECISIONS.md "Ventas en curso"): si al cerrar sesion la cuenta tiene
        // ventas del POS sin finalizar, queda constancia en cada una (evento LOGOUT). Nunca debe impedir el
        // logout: si falla solo se loguea.
        private void RegistrarLogoutConVentaEnCurso()
        {
            try
            {
                if (!WebCore.Helpers.PosBorradorSettings.Habilitado || !_sesion.EstaAutenticado) return;

                var usuario = _sesion.UsuarioActual;
                WebCore.Infrastructure.NegocioFactory.CrearVentaBorrador(_sesion.Empresa).RegistrarLogout(usuario.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar el logout con ventas en curso.");
            }
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

        // ---------------------------------------------------------------------------------
        // Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md "Dispositivo
        // seguro como condicion de login"). Flujo: clave valida + usuario que requiere dispositivo
        // seguro + dispositivo no autorizado -> pantalla DispositivoNoAutorizado con dos vias:
        // (1) PC con el agente de impresion: el usuario copia el ID de hardware y se lo pasa al
        // admin, que lo carga en /DispositivosSeguros; (2) celular / equipo sin agente: codigo de
        // 6 digitos por mail que se escribe en la misma pantalla y deja el navegador autorizado
        // sin intervencion del admin (el admin puede bloquearlo despues). Sin sesion emitida hasta
        // que el dispositivo queda autorizado.
        // ---------------------------------------------------------------------------------

        private const string TempDispError = "DispError";
        private const string TempDispSuccess = "DispSuccess";

        [HttpGet]
        public IActionResult DispositivoNoAutorizado()
        {
            var pedido = VerificacionDispositivoStore.Instancia.Obtener(DispositivoNavegador.LeerNoncePedido(HttpContext));
            if (pedido == null)
                return VolverAlLoginConError("La autorización venció. Ingresá de nuevo tu usuario y contraseña.");

            return View(ArmarDispositivoNoAutorizadoVm(pedido));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EnviarCodigoDispositivo()
        {
            string? nonce = DispositivoNavegador.LeerNoncePedido(HttpContext);
            var store = VerificacionDispositivoStore.Instancia;
            var pedido = store.Obtener(nonce);
            if (pedido == null)
                return VolverAlLoginConError("La autorización venció. Ingresá de nuevo tu usuario y contraseña.");

            var user = CrearUsuarioNegocio(pedido.IdEmpresa).getUsuarioById(pedido.IdUsuario, sinRestriccionDeTenant: true);
            if (user == null || !user.Activo)
                return VolverAlLoginConError("No fue posible iniciar sesión con los datos ingresados.");

            if (!SmtpMailHelper.IsValidEmail(user.Email))
            {
                TempData[TempDispError] = "Tu usuario no tiene un mail válido cargado. Pedile a un administrador que lo cargue o que autorice este dispositivo.";
                return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            if (!SmtpMailHelper.IsConfigured())
            {
                TempData[TempDispError] = "El envío de mails no está configurado en este servidor. Pedile a un administrador que autorice este dispositivo.";
                return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            if (!store.PuedeEnviarCodigo(nonce!, out var espera))
            {
                TempData[TempDispError] = string.Format(CultureInfo.CurrentCulture, "Ya te enviamos un código. Esperá {0} segundo(s) para pedir otro.", Math.Max(1, (int)Math.Ceiling(espera.TotalSeconds)));
                return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            string? codigo = store.GenerarCodigo(nonce!);
            if (codigo == null)
                return VolverAlLoginConError("La autorización venció. Ingresá de nuevo tu usuario y contraseña.");

            try
            {
                SmtpMailHelper.SendDeviceVerification(user.Email, user.Nombre, codigo, (int)VerificacionDispositivoStore.VidaCodigo.TotalMinutes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el codigo de autorizacion de dispositivo al usuario {IdUsuario}.", user.Id);
                TempData[TempDispError] = "No pudimos enviar el mail. Intentá de nuevo en unos minutos o pedile a un administrador que autorice este dispositivo.";
                return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            TempData[TempDispSuccess] = "Te enviamos un código a " + EnmascararEmail(user.Email) + ". Escribilo abajo (vence en " + (int)VerificacionDispositivoStore.VidaCodigo.TotalMinutes + " minutos).";
            return RedirectToAction(nameof(DispositivoNoAutorizado));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerificarDispositivo(string? codigo, string? nombreDispositivo)
        {
            string? nonce = DispositivoNavegador.LeerNoncePedido(HttpContext);
            var store = VerificacionDispositivoStore.Instancia;
            var pedido = store.Obtener(nonce);
            if (pedido == null)
                return VolverAlLoginConError("La autorización venció. Ingresá de nuevo tu usuario y contraseña.");

            // El nombre se valida ANTES de gastar un intento del codigo.
            string nombre = (nombreDispositivo ?? "").Trim();
            if (nombre.Length == 0 || nombre.Length > 100)
            {
                TempData[TempDispError] = "Ingresá un nombre para este dispositivo (ej. \"Celular de Juan\", máximo 100 caracteres).";
                return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            var oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(new EmpresaContextFijo(pedido.IdEmpresa));
            var existente = oDispositivoN.ObtenerPorSerie(pedido.SerieDispositivo, pedido.IdEmpresa);
            if (existente != null && existente.Bloqueado)
            {
                store.Quitar(nonce!);
                DispositivoNavegador.QuitarNoncePedido(HttpContext);
                return VolverAlLoginConError("Este dispositivo fue bloqueado por el administrador.");
            }

            switch (store.Verificar(nonce!, codigo))
            {
                case VerificacionDispositivoStore.ResultadoVerificacion.Ok:
                    break;
                case VerificacionDispositivoStore.ResultadoVerificacion.Incorrecto:
                    TempData[TempDispError] = "El código no es correcto. Revisalo e intentá de nuevo.";
                    return RedirectToAction(nameof(DispositivoNoAutorizado));
                case VerificacionDispositivoStore.ResultadoVerificacion.DemasiadosIntentos:
                    DispositivoNavegador.QuitarNoncePedido(HttpContext);
                    return VolverAlLoginConError("Demasiados intentos con un código incorrecto. Ingresá de nuevo tu usuario y contraseña.");
                default:
                    TempData[TempDispError] = "El código venció o todavía no pediste uno. Pedí un código nuevo.";
                    return RedirectToAction(nameof(DispositivoNoAutorizado));
            }

            DispositivoNavegador.QuitarNoncePedido(HttpContext);

            var oUsuarioN = CrearUsuarioNegocio(pedido.IdEmpresa);
            var user = CargarUsuarioParaSesion(oUsuarioN, pedido.IdUsuario, pedido.IdEmpresa);
            if (user == null || !user.Activo)
                return VolverAlLoginConError("No fue posible iniciar sesión con los datos ingresados.");

            if (existente == null)
            {
                oDispositivoN.Agregar(new Entidades.DispositivoSeguro
                {
                    IdEmpresa = pedido.IdEmpresa,
                    NumeroSerie = pedido.SerieDispositivo,
                    Descripcion = nombre,
                    IdUsuarioCreador = user.Id,
                    Origen = "Autoservicio",
                    EmailAlta = user.Email
                });
                existente = oDispositivoN.ObtenerPorSerie(pedido.SerieDispositivo, pedido.IdEmpresa);
            }

            oUsuarioN.RegistrarLoginExitoso(user);
            return await CompletarLoginAsync(
                user, pedido.ReturnUrl, ObtenerDireccionIp(), user.User ?? "", existente,
                "Dispositivo autorizado por mail: " + nombre);
        }

        private IActionResult DispositivoNoAutorizadoTrasClaveValida(
            Entidades.Usuario user, string ip, LoginIndexVm model, string serieToken, Entidades.DispositivoSeguro? dispositivoBloqueado)
        {
            if (dispositivoBloqueado != null)
            {
                RegistrarAcceso(user, false, "Dispositivo bloqueado por el administrador", ip, dispositivoBloqueado);
                model.Clave = "";
                model.Error = "Este dispositivo fue bloqueado por el administrador. Pedile que lo habilite o ingresá desde otro dispositivo autorizado.";
                return View("Index", model);
            }

            string nonce = VerificacionDispositivoStore.Instancia.CrearPedido(user.Id, user.IdEmpresa, serieToken, model.ReturnUrl);
            DispositivoNavegador.GuardarNoncePedido(HttpContext, nonce);
            RegistrarAcceso(user, false, "Dispositivo no autorizado", ip, null);
            return RedirectToAction(nameof(DispositivoNoAutorizado));
        }

        private DispositivoNoAutorizadoVm ArmarDispositivoNoAutorizadoVm(VerificacionDispositivoStore.Pedido pedido)
        {
            var user = CrearUsuarioNegocio(pedido.IdEmpresa).getUsuarioById(pedido.IdUsuario, sinRestriccionDeTenant: true);
            bool tieneEmail = user != null && SmtpMailHelper.IsValidEmail(user.Email);

            return new DispositivoNoAutorizadoVm
            {
                UsuarioNombre = user?.Nombre ?? "",
                TieneEmail = tieneEmail,
                EmailEnmascarado = tieneEmail ? EnmascararEmail(user!.Email) : "",
                SmtpConfigurado = SmtpMailHelper.IsConfigured(),
                CodigoEnviado = pedido.CodigoExpiraUtc.HasValue && pedido.CodigoExpiraUtc.Value > DateTime.UtcNow,
                Error = TempData[TempDispError] as string,
                Success = TempData[TempDispSuccess] as string
            };
        }

        // Autorizado = ID de hardware (agente) o token del navegador registrado y NO bloqueado.
        // Bloqueado = alguno de los dos registrado pero bloqueado por el admin (solo se informa si
        // ninguno esta autorizado).
        private (Entidades.DispositivoSeguro? autorizado, Entidades.DispositivoSeguro? bloqueado) ResolverDispositivo(
            int idEmpresa, string? serieHardware, string serieToken)
        {
            // Con el tenant real de la empresa (dispositivosseguros tiene RLS en Postgres: con
            // EmpresaContextNulo el filtro nunca encuentra filas).
            var oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(new EmpresaContextFijo(idEmpresa));

            var candidatos = new List<Entidades.DispositivoSeguro?>();
            if (!string.IsNullOrWhiteSpace(serieHardware))
                candidatos.Add(oDispositivoN.ObtenerPorSerie(serieHardware.Trim(), idEmpresa));
            if (!string.IsNullOrWhiteSpace(serieToken))
                candidatos.Add(oDispositivoN.ObtenerPorSerie(serieToken, idEmpresa));

            var autorizado = candidatos.FirstOrDefault(d => d != null && !d.Bloqueado);
            var bloqueado = autorizado == null ? candidatos.FirstOrDefault(d => d != null && d.Bloqueado) : null;
            return (autorizado, bloqueado);
        }

        private async Task<IActionResult> CompletarLoginAsync(
            Entidades.Usuario user, string returnUrl, string ip, string usuarioTipeado,
            Entidades.DispositivoSeguro? dispositivo, string motivo)
        {
            RegistrarAcceso(user, true, motivo, ip, dispositivo);
            await FirmarCookieAsync(user);
            LoginRateLimiter.Reset(ip, usuarioTipeado);

            // Modal de sucursal post-login (item 3 de la cuarta ronda de pedidos, 2026-09-10 --
            // ver docs/DECISIONS.md "Batch 2: modal de sucursal automatico al loguearse"). Port
            // de Web/Controllers/LoginController.cs:151-157: si la empresa tiene 2+ sucursales,
            // se avisa siempre en que sucursal va a operar -- sin condicion adicional (no
            // depende de admin ni de cuantas sucursales tenga ESTE usuario en particular).
            var oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(new EmpresaContextFijo(user.IdEmpresa));
            var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            if (sucursalesEmpresa.Count >= 2)
                TempData["MostrarModalSucursalPostLogin"] = true;

            return RedirigirPostLogin(returnUrl);
        }

        // Auditoria de accesos (LoginUbicacionLog, sin geolocalizacion): la auditoria nunca debe
        // impedir el login, pero un fallo se loguea (no se traga en silencio).
        private void RegistrarAcceso(Entidades.Usuario user, bool permitido, string motivo, string ip, Entidades.DispositivoSeguro? dispositivo)
        {
            try
            {
                _oUsuarioN.RegistrarLoginUbicacion(new Entidades.LoginUbicacionLog
                {
                    IdUsuario = user.Id,
                    IdSucursal = user.IdSucursal,
                    FechaHora = DateTime.Now,
                    Permitido = permitido,
                    Motivo = motivo.Length > 300 ? motivo.Substring(0, 300) : motivo,
                    Ip = ip,
                    IdDispositivoSeguro = dispositivo?.Id,
                    Dispositivo = dispositivo?.Descripcion
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo registrar el acceso del usuario {IdUsuario} en la auditoria.", user.Id);
            }
        }

        private static Negocio.Usuario CrearUsuarioNegocio(int idEmpresa)
        {
            return WebCore.Infrastructure.NegocioFactory.CrearUsuario(new EmpresaContextFijo(idEmpresa));
        }

        // Mismo armado de usuario que el login normal (sucursal + permisos) -- reusado al
        // completar el login tras autorizar el dispositivo por mail.
        private static Entidades.Usuario? CargarUsuarioParaSesion(Negocio.Usuario oUsuarioN, int idUsuario, int idEmpresa)
        {
            var user = oUsuarioN.getUsuarioById(idUsuario, sinRestriccionDeTenant: true);
            if (user == null)
                return null;

            var oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(new EmpresaContextFijo(idEmpresa));
            user.Sucursal = user.IdSucursal > 0 ? oSucursalN.findById(user.IdSucursal) : null;
            user.SucursalNombre = user.Sucursal?.SucursalNombre ?? "";
            user.Permisos = oUsuarioN.getPermisosUsuario(user.Id);
            return user;
        }

        private IActionResult VolverAlLoginConError(string mensaje)
        {
            TempData["Error"] = mensaje;
            return RedirectToAction(nameof(Index));
        }

        // "juan.perez@gmail.com" -> "j***@gmail.com"
        private static string EnmascararEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "";

            int arroba = email.IndexOf('@');
            if (arroba <= 0)
                return "***";

            return email.Substring(0, 1) + "***" + email.Substring(arroba);
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
            //
            // Bug real encontrado 2026-09-16 (cutover San Lorenzo, ver docs/DECISIONS.md): un
            // Redirect("/Home/Index") con ruta absoluta literal ignora el PathBase de la app --
            // funciona en carnisys.com (standalone, PathBase vacio) pero da 404 en cualquier
            // deploy hosteado como subaplicacion de IIS (San Lorenzo/Servidor SM, PathBase
            // "/CarniSysWeb"): el browser termina pidiendo "/Home/Index" sin el prefijo, y el
            // sitio IIS no tiene nada mapeado ahi. Url.Content("~/...") resuelve el "~" contra el
            // PathBase real de la request, evitando el 404 sin reintroducir el colapso a "/".
            return Redirect(Url.Content("~/Home/Index"));
        }

        // Reglas que se aplican DESPUES de validar la credencial (clave o huella), compartidas por
        // ambos caminos de login (2026-09-21, se extrajo del POST de Index para el login por huella,
        // ver docs/DECISIONS.md "Login por huella (passkeys)"): horario laboral y dispositivo seguro.
        // Solo a no-admin: los admin quedan exentos siempre (evita quedarse sin acceso para
        // autorizar dispositivos). Cada caller decide como mostrar el rechazo.
        private enum ReglaPostCredencial { Ok, FueraDeHorario, DispositivoNoAutorizado }

        private const string MensajeFueraDeHorario = "Fuera del horario laboral permitido para iniciar sesión.";

        private ReglaPostCredencial EvaluarReglasPostCredencial(Entidades.Usuario user, bool esDispositivoSeguro)
        {
            if (user.Admin)
                return ReglaPostCredencial.Ok;

            // Horario laboral (a nivel empresa): un empleado no-admin que intenta loguearse fuera de
            // las 2 jornadas configuradas queda bloqueado ANTES de crear sesion -- no se toca el rate
            // limiter (las credenciales eran correctas).
            var empresaActual = _oEmpresaN.findById(user.IdEmpresa);
            if (!EstaDentroDelHorarioPermitido(empresaActual, DateTime.Now))
                return ReglaPostCredencial.FueraDeHorario;

            // Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md): se exige a
            // los no-admin si la empresa lo activo o el usuario esta marcado.
            bool requiereDispositivo = user.RequiereDispositivoSeguro || (empresaActual?.ExigirDispositivoSeguro ?? false);
            if (requiereDispositivo && !esDispositivoSeguro)
                return ReglaPostCredencial.DispositivoNoAutorizado;

            return ReglaPostCredencial.Ok;
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
