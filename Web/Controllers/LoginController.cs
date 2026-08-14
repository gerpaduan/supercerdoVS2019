using Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Security;
using System.Web.Mvc;
using Utilidades;
using Web.Filters;
using Web.Helpers;
using Web.Models;
using wsAFIPvs2008;

namespace Web.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private const string GenericRecoveryMessage = "Si los datos ingresados corresponden a un usuario registrado, recibirás instrucciones para recuperar tu contraseña.";

        private const string SessionKeyUbicacionLoginValidada = "UbicacionLoginValidada";
        private const decimal PrecisionMaximaLoginMetros = 150m;
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;
        private Negocio.Empresa oEmpresaN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            IEmpresaContext empresa = new EmpresaContextNulo();
            oUsuarioN = new Negocio.Usuario(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);
            oEmpresaN = new Negocio.Empresa(empresa);
        }

        [HttpGet]
        public ActionResult Index(string returnUrl = "")
        {
            var model = new LoginIndexVm
            {
                ReturnUrl = returnUrl ?? "",
                Error = ViewBag.Error as string,
                Success = TempData["Success"] as string
            };

            if (TempData["Error"] != null && string.IsNullOrWhiteSpace(model.Error))
                model.Error = Convert.ToString(TempData["Error"]);

            return View("~/Views/Login/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginIndexVm model)
        {
            model = model ?? new LoginIndexVm();
            model.Usuario = (model.Usuario ?? "").Trim();
            model.Clave = model.Clave ?? "";
            model.ReturnUrl = model.ReturnUrl ?? "";

            if (!ModelState.IsValid)
                return View("~/Views/Login/Index.cshtml", model);

            // Resuelto ANTES del rate limiter por IP, para dos cosas: (a) decidir si este login
            // viene de un dispositivo seguro y saltear el bloqueo por IP -- NO el bloqueo por
            // cuenta, que sigue igual mas abajo, sin excepcion (decision explicita); (b) el
            // chequeo de cuenta bloqueada, que ya usaba este mismo candidato.
            var candidato = oUsuarioN.ObtenerUsuarioPorIdentificador(model.Usuario);

            // El numero de serie es un campo de formulario comun, sin atadura criptografica al
            // dispositivo real -- riesgo aceptado explicitamente (ver docs/DECISIONS.md). Es una
            // conveniencia para reducir friccion en maquinas de oficina conocidas, no una barrera
            // dura: por eso solo se saltea el bloqueo por IP, nunca el bloqueo por cuenta.
            var oDispositivoN = new Negocio.DispositivoSeguro(new EmpresaContextNulo());
            bool esDispositivoSeguro = candidato != null
                && !string.IsNullOrWhiteSpace(model.NumeroSerieDispositivo)
                && oDispositivoN.ExisteSerieSegura(model.NumeroSerieDispositivo, candidato.IdEmpresa);

            TimeSpan retryAfter;
            if (!esDispositivoSeguro && LoginRateLimiter.IsBlocked(Request, model.Usuario, out retryAfter))
            {
                Response.StatusCode = 429;
                model.Clave = "";
                model.Error = string.Format(
                    CultureInfo.CurrentCulture,
                    "Se supero el maximo de intentos. Espera {0} minuto(s) y volve a intentar.",
                    Math.Max(1, (int)Math.Ceiling(retryAfter.TotalMinutes)));
                return View("~/Views/Login/Index.cshtml", model);
            }

            // Cuenta bloqueada por intentos fallidos (distinto del rate limiter por IP de arriba,
            // y NO afectado por dispositivo seguro): se chequea antes de validar la contraseña,
            // para no seguir contando intentos contra una cuenta ya bloqueada ni reenviar el mail
            // de desbloqueo en cada reintento.
            if (candidato != null && candidato.Activo && candidato.Bloqueado)
            {
                LoginRateLimiter.RegisterFailure(Request, model.Usuario);
                model.Clave = "";
                model.Error = "Tu cuenta está bloqueada por intentos fallidos. Revisá tu email para el link de desbloqueo, o pedile a un administrador que la desbloquee.";
                return View("~/Views/Login/Index.cshtml", model);
            }

            var user = oUsuarioN.ValidarUsuarioWeb(model.Usuario, model.Clave);

            if (user != null && user.Activo)
            {
                IEmpresaContext empresa = new EmpresaContextWin(user.IdEmpresa);

                oUsuarioN = new Negocio.Usuario(empresa);
                oSucursalN = new Negocio.Sucursal(empresa);
                oEmpresaN = new Negocio.Empresa(empresa);

                user = oUsuarioN.getUsuarioById(user.Id);
                if (user != null)
                    user.Permisos = oUsuarioN.getPermisosUsuario(user.Id);

                if (user != null)
                    oUsuarioN.RegistrarLoginExitoso(user);

                string sucNombre = user != null && user.Sucursal == null
                    ? "Seleccione Sucursal"
                    : (user != null && user.Sucursal != null ? user.Sucursal.SucursalNombre : "");

                if (user != null)
                    user.SucursalNombre = sucNombre ?? "";

                // Horario laboral (a nivel empresa, ver Empresa.HorarioDiurno*/HorarioTarde*): un
                // empleado no-admin que intenta loguearse fuera de las 2 jornadas configuradas
                // queda bloqueado aca, ANTES de crear sesion -- no se toca el rate limiter (las
                // credenciales eran correctas, no es un intento fallido de contrasena).
                if (user != null && !user.Admin)
                {
                    Entidades.Empresa empresaActual = oEmpresaN.findById(user.IdEmpresa);
                    if (!EstaDentroDelHorarioPermitido(empresaActual, DateTime.Now))
                    {
                        model.Clave = "";
                        model.Error = "Fuera del horario laboral permitido para iniciar sesión.";
                        return View("~/Views/Login/Index.cshtml", model);
                    }
                }

                SessionSecurityHelper.RenewAuthenticatedSession(HttpContext, model.Usuario);
                Session["Usuario"] = user;
                Session["IdEmpresa"] = user != null ? user.IdEmpresa : 0;
                Session.Remove("PARAM_CTX");

                IParametrosContext paramCtx = new Negocio.Parametros(new EmpresaContextWeb());
                paramCtx.Reload();
                Session["PARAM_CTX"] = paramCtx;
                LoginRateLimiter.Reset(Request, model.Usuario);

                bool requiereValidacionUbicacion = RequiereValidacionUbicacionPostLogin(user);
                Session[SessionKeyUbicacionLoginValidada] = !requiereValidacionUbicacion;

                if (requiereValidacionUbicacion)
                    return RedirectToAction("ValidarUbicacion", "Login", new { returnUrl = model.ReturnUrl });

                // Login exitoso sin geo-validacion requerida: igual queda registrado en
                // LoginUbicacionLog (auditoria general de accesos, no solo del flujo de geo-validacion).
                // Coordenadas: las que ya haya capturado en silencio el JS de Login/Index.cshtml -- pueden
                // venir vacias si el navegador no dio permiso, se registra igual.
                RegistrarLoginUbicacion(
                    user,
                    user.Sucursal,
                    ParseNullableDecimal(model.Latitud),
                    ParseNullableDecimal(model.Longitud),
                    ParseNullableDecimal(model.PrecisionMetros),
                    null,
                    true,
                    "Login exitoso (sin geo-validación requerida).",
                    ObtenerDireccionIp());

                return RedirigirPostLogin(model.ReturnUrl);
            }

            // Solo se cuenta contra el bloqueo persistente si el usuario/email existe y esta
            // activo -- pedido explicito: no tiene sentido bloquear (ni mandar mail) por intentos
            // contra una cuenta que no existe o esta inactiva.
            if (candidato != null && candidato.Activo && !candidato.Bloqueado)
            {
                bool seAcabaDeBloquear = oUsuarioN.RegistrarIntentoFallido(candidato, GetAccountLockoutMaxAttempts());
                if (seAcabaDeBloquear)
                    EnviarMailDesbloqueo(candidato);
            }

            LoginRateLimiter.RegisterFailure(Request, model.Usuario);
            model.Clave = "";
            model.Error = "No fue posible iniciar sesión con los datos ingresados.";
            return View("~/Views/Login/Index.cshtml", model);

#pragma warning disable 162
            model.Clave = "";
            model.Error = user != null && !user.Activo
                ? "No fue posible iniciar sesión. Verificá tus datos o consultá a un administrador si tu usuario está inactivo."
                : "Usuario o contraseña incorrectos.";

            return View("~/Views/Login/Index.cshtml", model);
#pragma warning restore 162
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("~/Views/Login/ForgotPassword.cshtml", new PasswordRecoveryRequestVm());
        }

        [HttpGet]
        public ActionResult ValidarUbicacion(string returnUrl = "")
        {
            var usuarioSesion = Session["Usuario"] as Entidades.Usuario;
            if (usuarioSesion == null)
                return RedirectToAction("Index", "Login", new { returnUrl = returnUrl ?? "" });

            if (UbicacionLoginYaValidadaEnSesion())
                return RedirigirPostLogin(returnUrl);

            var model = new LoginUbicacionVm
            {
                ReturnUrl = returnUrl ?? "",
                Error = ViewBag.Error as string,
                Success = TempData["Success"] as string
            };

            if (TempData["Error"] != null && string.IsNullOrWhiteSpace(model.Error))
                model.Error = Convert.ToString(TempData["Error"]);

            return View("~/Views/Login/ValidarUbicacion.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ValidarUbicacion(LoginUbicacionVm model)
        {
            var usuarioSesion = Session["Usuario"] as Entidades.Usuario;
            model = model ?? new LoginUbicacionVm();
            model.ReturnUrl = model.ReturnUrl ?? "";

            if (usuarioSesion == null)
                return RedirectToAction("Index", "Login", new { returnUrl = model.ReturnUrl });

            IEmpresaContext empresa = new EmpresaContextWin(usuarioSesion.IdEmpresa);
            oUsuarioN = new Negocio.Usuario(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);

            var user = oUsuarioN.getUsuarioById(usuarioSesion.Id);
            if (user != null)
            {
                user.Permisos = oUsuarioN.getPermisosUsuario(user.Id);
                string sucNombre = user.Sucursal == null
                    ? "Seleccione Sucursal"
                    : (user.Sucursal != null ? user.Sucursal.SucursalNombre : "");
                user.SucursalNombre = sucNombre ?? "";
                Session["Usuario"] = user;
            }

            if (user == null || !user.Activo)
            {
                TempData["Error"] = "La sesion ya no es valida. Inicia sesion nuevamente.";
                return RedirectToAction("Logout");
            }

            if (!RequiereValidacionUbicacionPostLogin(user))
            {
                Session[SessionKeyUbicacionLoginValidada] = true;
                return RedirigirPostLogin(model.ReturnUrl);
            }

            string errorUbicacion = null;
            if (!ValidarUbicacionLoginEmpresa(user, model, out errorUbicacion))
            {
                model.Error = errorUbicacion;
                return View("~/Views/Login/ValidarUbicacion.cshtml", model);
            }

            Session[SessionKeyUbicacionLoginValidada] = true;
            return RedirigirPostLogin(model.ReturnUrl);
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            var usuarioSesion = Session["Usuario"] as Entidades.Usuario;
            if (usuarioSesion == null)
                return RedirectToAction("Index", "Login");

            var model = new ChangePasswordVm
            {
                Error = TempData["Error"] as string,
                Success = TempData["Success"] as string
            };

            return View("~/Views/Login/ChangePassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordVm model)
        {
            var usuarioSesion = Session["Usuario"] as Entidades.Usuario;
            if (usuarioSesion == null)
                return RedirectToAction("Index", "Login");

            model = model ?? new ChangePasswordVm();

            IEmpresaContext empresa = new EmpresaContextWin(usuarioSesion.IdEmpresa);
            oUsuarioN = new Negocio.Usuario(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);

            ValidarNuevaClave(model.NuevaClave);

            var usuarioActual = oUsuarioN.getUsuarioById(usuarioSesion.Id);
            if (usuarioActual == null || !usuarioActual.Activo)
                ModelState.AddModelError("", "No fue posible actualizar la contraseÃ±a.");
            else if (oUsuarioN.ValidarUsuarioWeb(usuarioActual.User, model.ClaveActual) == null)
                ModelState.AddModelError("ClaveActual", "La contraseÃ±a actual es incorrecta.");

            if (!ModelState.IsValid)
                return View("~/Views/Login/ChangePassword.cshtml", model);

            oUsuarioN.ActualizarPasswordWebSeguro(usuarioActual.Id, model.NuevaClave);
            oUsuarioN.InvalidarTokensPendientesUsuario(usuarioActual.Id, "reset");

            TempData["Success"] = "Tu contraseÃ±a fue actualizada correctamente.";
            return RedirectToAction("ChangePassword", "Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(PasswordRecoveryRequestVm model)
        {
            model = model ?? new PasswordRecoveryRequestVm();
            model.UsuarioOEmail = (model.UsuarioOEmail ?? "").Trim();

            if (!ModelState.IsValid)
                return View("~/Views/Login/ForgotPassword.cshtml", model);

            try
            {
                var usuarios = oUsuarioN.BuscarUsuariosPorIdentificador(model.UsuarioOEmail, true)
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

                    oUsuarioN.CrearTokenRecuperacion(new UsuarioPasswordResetToken
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
                        string resetUrl = Url.Action("ResetPassword", "Login", new { token = rawToken }, Request != null && Request.Url != null ? Request.Url.Scheme : "http");
                        SmtpMailHelper.SendPasswordReset(usuario.Email, usuario.Nombre, resetUrl, expirationMinutes);
                    }
                }
            }
            catch
            {
                // Se responde igual para no filtrar información sensible.
            }

            TempData["Success"] = GenericRecoveryMessage;
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ResetPassword(string token = "")
        {
            var model = new PasswordResetVm
            {
                Token = token ?? "",
                TokenValido = TokenEsValido(token, "reset")
            };

            if (!model.TokenValido)
                model.Mensaje = "El enlace de recuperación no es válido o ya venció.";

            return View("~/Views/Login/ResetPassword.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ResetPassword(PasswordResetVm model)
        {
            model = model ?? new PasswordResetVm();
            model.Token = model.Token ?? "";
            model.TokenValido = TokenEsValido(model.Token, "reset");

            ValidarNuevaClave(model);

            if (!model.TokenValido)
                ModelState.AddModelError("", "El enlace de recuperación no es válido o ya venció.");

            if (!ModelState.IsValid)
                return View("~/Views/Login/ResetPassword.cshtml", model);

            string tokenHash = PasswordSecurity.ComputeSha256Base64(model.Token);
            var token = oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
            if (token == null || token.Usado || token.Proposito != "reset" || token.FechaExpiracionUtc < DateTime.UtcNow)
            {
                model.TokenValido = false;
                model.Mensaje = "El enlace de recuperación no es válido o ya venció.";
                ModelState.AddModelError("", model.Mensaje);
                return View("~/Views/Login/ResetPassword.cshtml", model);
            }

            var usuario = oUsuarioN.getUsuarioById(token.IdUsuario);
            if (usuario == null || !usuario.Activo)
            {
                model.TokenValido = false;
                ModelState.AddModelError("", "No fue posible actualizar la contraseña.");
                return View("~/Views/Login/ResetPassword.cshtml", model);
            }

            oUsuarioN.ActualizarPasswordWebSeguro(usuario.Id, model.NuevaClave);
            oUsuarioN.MarcarTokenRecuperacionComoUsado(token.Id);
            oUsuarioN.InvalidarTokensPendientesUsuario(usuario.Id, "reset");

            TempData["Success"] = "Tu contraseña fue actualizada correctamente. Ya podés iniciar sesión.";
            return RedirectToAction("Index");
        }

        // Mismo patron de 2 pasos que ResetPassword (GET muestra, POST actua): evita que un link
        // cargado pasivamente (ej. previsualizacion de un cliente de correo) dispare el
        // desbloqueo solo -- hace falta un click explicito en el boton del POST.
        [HttpGet]
        public ActionResult UnlockAccount(string token = "")
        {
            var model = new UnlockAccountVm
            {
                Token = token ?? "",
                TokenValido = TokenEsValido(token, "unlock")
            };

            if (!model.TokenValido)
                model.Mensaje = "El enlace de desbloqueo no es válido o ya venció.";

            return View("~/Views/Login/UnlockAccount.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UnlockAccount(UnlockAccountVm model)
        {
            model = model ?? new UnlockAccountVm();
            model.Token = model.Token ?? "";
            model.TokenValido = TokenEsValido(model.Token, "unlock");

            if (!model.TokenValido)
            {
                model.Mensaje = "El enlace de desbloqueo no es válido o ya venció.";
                return View("~/Views/Login/UnlockAccount.cshtml", model);
            }

            string tokenHash = PasswordSecurity.ComputeSha256Base64(model.Token);
            var token = oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
            if (token == null || token.Usado || token.Proposito != "unlock" || token.FechaExpiracionUtc < DateTime.UtcNow)
            {
                model.TokenValido = false;
                model.Mensaje = "El enlace de desbloqueo no es válido o ya venció.";
                return View("~/Views/Login/UnlockAccount.cshtml", model);
            }

            oUsuarioN.DesbloquearUsuario(token.IdUsuario);
            oUsuarioN.MarcarTokenRecuperacionComoUsado(token.Id);
            oUsuarioN.InvalidarTokensPendientesUsuario(token.IdUsuario, "unlock");

            TempData["Success"] = "Tu cuenta fue desbloqueada correctamente. Ya podés iniciar sesión.";
            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            Session.Remove("PARAM_CTX");
            Session.Remove("IdEmpresa");
            Session.Remove(SessionKeyUbicacionLoginValidada);
            Session.Clear();

            if (Request.Cookies["ASP.NET_SessionId"] != null)
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);

            Session.Abandon();
            SessionSecurityHelper.ClearAuthentication(HttpContext);

            return RedirectToAction("Index", "Login");
        }

        private ActionResult RedirigirPostLogin(string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        private bool UbicacionLoginYaValidadaEnSesion()
        {
            object valor = Session[SessionKeyUbicacionLoginValidada];
            return valor is bool && (bool)valor;
        }

        private bool RequiereValidacionUbicacionPostLogin(Entidades.Usuario user)
        {
            if (user == null || user.Sucursal == null)
                return false;

            if (user.Admin || user.PermitirLoginFueraSucursal)
                return false;

            var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            return sucursalesEmpresa.Any(s => s != null && s.ValidarUbicacionLogin);
        }

        // Compara la hora actual del servidor contra las 2 jornadas configuradas en la empresa.
        // Admin ya viene exceptuado por el caller (mismo criterio que la geo-validacion).
        private static bool EstaDentroDelHorarioPermitido(Entidades.Empresa empresa, DateTime ahora)
        {
            if (empresa == null)
                return true; // fail-open: no se pudo resolver la empresa, no bloquear por un dato faltante

            TimeSpan horaActual = ahora.TimeOfDay;
            return EstaEnJornada(horaActual, empresa.HorarioDiurnoDesde, empresa.HorarioDiurnoHasta)
                || EstaEnJornada(horaActual, empresa.HorarioTardeDesde, empresa.HorarioTardeHasta);
        }

        private static bool EstaEnJornada(TimeSpan horaActual, TimeSpan desde, TimeSpan hasta)
        {
            if (desde <= hasta)
                return horaActual >= desde && horaActual <= hasta;

            // Jornada que cruza medianoche (ej. 22:00-02:00) -- no es el caso por defecto, pero se contempla.
            return horaActual >= desde || horaActual <= hasta;
        }

        [HttpPost]
        [SkipAppAntiForgeryValidation]
        public JsonResult CambiarSucursal(int idSucursal)
        {
            try
            {
                var usuario = Session["Usuario"] as Entidades.Usuario;

                if (usuario == null)
                    return Json(new { ok = false, msg = "Sesión expirada" });

                IEmpresaContext empresa = new EmpresaContextWin(usuario.IdEmpresa);

                oUsuarioN = new Negocio.Usuario(empresa);
                oSucursalN = new Negocio.Sucursal(empresa);

                var sucursal = oSucursalN.findById(idSucursal);
                if (sucursal == null)
                    return Json(new { ok = false, msg = "Sucursal inválida" });

                usuario.IdSucursal = sucursal.IdSucursal;
                usuario.Sucursal = sucursal;
                usuario.SucursalNombre = sucursal.SucursalNombre;

                oUsuarioN.setSucursalUsuario(usuario);
                Session["Usuario"] = usuario;

                return Json(new
                {
                    ok = true,
                    sucursalNombre = usuario.SucursalNombre,
                    idSucursal = usuario.IdSucursal
                });
            }
            catch
            {
                return Json(new { ok = false });
            }
        }

        private bool TokenEsValido(string tokenRaw, string proposito)
        {
            if (string.IsNullOrWhiteSpace(tokenRaw))
                return false;

            try
            {
                string tokenHash = PasswordSecurity.ComputeSha256Base64(tokenRaw);
                var token = oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
                return token != null && !token.Usado && token.Proposito == proposito && token.FechaExpiracionUtc >= DateTime.UtcNow;
            }
            catch
            {
                return false;
            }
        }

        private int GetPasswordResetExpirationMinutes()
        {
            int minutes;
            if (int.TryParse(ConfigurationManager.AppSettings["PasswordResetTokenMinutes"], out minutes) && minutes > 0)
                return minutes;

            return 60;
        }

        private int GetAccountLockoutMaxAttempts()
        {
            int intentos;
            if (int.TryParse(ConfigurationManager.AppSettings["Security:AccountLockoutMaxAttempts"], out intentos) && intentos > 0)
                return intentos;

            return 5;
        }

        // Genera el token de desbloqueo (mismo mecanismo que ForgotPassword) y manda el mail, solo
        // si SMTP esta configurado y el usuario tiene email cargado. Si no tiene email, no hay
        // forma de mandarlo -- queda como deuda documentada, la unica via de desbloqueo pasa a ser
        // un admin (UsuariosController.DesbloquearUsuario). No se propaga ninguna excepcion: un
        // fallo de envio no debe romper el flujo de login (mismo criterio que ForgotPassword).
        private void EnviarMailDesbloqueo(Entidades.Usuario usuario)
        {
            if (usuario == null || string.IsNullOrWhiteSpace(usuario.Email) || !SmtpMailHelper.IsConfigured())
                return;

            try
            {
                string rawToken = PasswordSecurity.GenerateToken();
                string tokenHash = PasswordSecurity.ComputeSha256Base64(rawToken);
                int expirationMinutes = GetPasswordResetExpirationMinutes();
                DateTime nowUtc = DateTime.UtcNow;

                oUsuarioN.CrearTokenRecuperacion(new UsuarioPasswordResetToken
                {
                    IdUsuario = usuario.Id,
                    IdEmpresa = usuario.IdEmpresa,
                    TokenHash = tokenHash,
                    FechaCreacionUtc = nowUtc,
                    FechaExpiracionUtc = nowUtc.AddMinutes(expirationMinutes),
                    Usado = false,
                    IdentificadorSolicitado = usuario.User ?? "",
                    EmailDestino = usuario.Email ?? "",
                    Proposito = "unlock"
                });

                string unlockUrl = Url.Action("UnlockAccount", "Login", new { token = rawToken }, Request != null && Request.Url != null ? Request.Url.Scheme : "http");
                SmtpMailHelper.SendAccountUnlock(usuario.Email, usuario.Nombre, unlockUrl, expirationMinutes);
            }
            catch
            {
                // No debe romper el flujo de login por un fallo de envio de mail.
            }
        }

        private void ValidarNuevaClave(string nuevaClave)
        {
            if (string.IsNullOrWhiteSpace(nuevaClave))
                return;

            if (nuevaClave.Contains(" "))
                ModelState.AddModelError("NuevaClave", "La contraseÃ±a no puede contener espacios en blanco.");
        }

        private void ValidarNuevaClave(PasswordResetVm model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaClave))
                return;

            if (model.NuevaClave.Contains(" "))
                ModelState.AddModelError("NuevaClave", "La contraseña no puede contener espacios en blanco.");
        }
        private bool ValidarUbicacionLogin(Entidades.Usuario user, LoginIndexVm model, out string mensajeError)
        {
            mensajeError = null;
            if (user == null)
                return false;

            var sucursal = user.Sucursal;
            var sucursalLog = sucursal;
            var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var sucursalesConValidacion = sucursalesEmpresa
                .Where(s => s != null && s.ValidarUbicacionLogin)
                .ToList();
            decimal? latitud = ParseNullableDecimal(model != null ? model.Latitud : null);
            decimal? longitud = ParseNullableDecimal(model != null ? model.Longitud : null);
            decimal? precisionMetros = ParseNullableDecimal(model != null ? model.PrecisionMetros : null);
            decimal? distanciaMetros = null;
            string ip = ObtenerDireccionIp();
            string motivo;
            bool permitido;

            if (sucursal == null)
            {
                permitido = true;
                motivo = "Usuario sin sucursal asignada.";
            }
            else if (sucursalesConValidacion.Count == 0)
            {
                permitido = true;
                motivo = "Sucursal sin validaciÃ³n de ubicaciÃ³n.";
            }
            else if (user.Admin || user.PermitirLoginFueraSucursal)
            {
                permitido = true;
                motivo = "Usuario exceptuado de validaciÃ³n de ubicaciÃ³n.";
            }
            else if (sucursalesConValidacion.Any(s => !s.Latitud.HasValue || !s.Longitud.HasValue))
            {
                permitido = true;
                motivo = "Sucursal sin coordenadas configuradas.";
                mensajeError = "No fue posible validar la ubicaciÃ³n de la sucursal asignada. Contacte a un administrador.";
            }
            else if (!latitud.HasValue || !longitud.HasValue)
            {
                permitido = false;
                motivo = "El navegador no enviÃ³ coordenadas.";
                mensajeError = "Debe permitir la ubicaciÃ³n del navegador para iniciar sesiÃ³n en esta sucursal.";
            }
            else if (!precisionMetros.HasValue)
            {
                permitido = false;
                motivo = "El navegador no informÃ³ la precisiÃ³n de la ubicaciÃ³n.";
                mensajeError = "No fue posible validar la precisiÃ³n de tu ubicaciÃ³n. IntentÃ¡ nuevamente.";
            }
            else if (precisionMetros.Value > PrecisionMaximaLoginMetros)
            {
                permitido = false;
                motivo = "PrecisiÃ³n insuficiente de ubicaciÃ³n.";
                mensajeError = "La precisiÃ³n de la ubicaciÃ³n es insuficiente. IntentÃ¡ nuevamente desde una zona con mejor seÃ±al.";
            }
            else
            {
                distanciaMetros = CalcularDistanciaHaversineMetros(
                    sucursal.Latitud.Value,
                    sucursal.Longitud.Value,
                    latitud.Value,
                    longitud.Value);

                if (distanciaMetros.Value > sucursal.RadioLoginMetros)
                {
                    permitido = false;
                    motivo = "UbicaciÃ³n fuera del radio permitido.";
                    mensajeError = "No fue posible iniciar sesiÃ³n porque estÃ¡s fuera del radio permitido para la sucursal asignada.";
                }
                else
                {
                    permitido = true;
                    motivo = "UbicaciÃ³n validada correctamente.";
                }
            }

            RegistrarLoginUbicacion(user, sucursal, latitud, longitud, precisionMetros, distanciaMetros, permitido, motivo, ip);
            return permitido;
        }

        private bool ValidarUbicacionLoginEmpresa(Entidades.Usuario user, LoginIndexVm model, out string mensajeError)
        {
            mensajeError = null;
            if (user == null)
                return false;

            decimal? latitud = ParseNullableDecimal(model != null ? model.Latitud : null);
            decimal? longitud = ParseNullableDecimal(model != null ? model.Longitud : null);
            decimal? precisionMetros = ParseNullableDecimal(model != null ? model.PrecisionMetros : null);
            decimal? distanciaMetros = null;
            string ip = ObtenerDireccionIp();
            string motivo;
            bool permitido;
            var sucursalLog = user.Sucursal;

            if (user.Sucursal == null)
            {
                permitido = true;
                motivo = "Usuario sin sucursal asignada.";
            }
            else
            {
                var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
                var sucursalesConValidacion = sucursalesEmpresa
                    .Where(s => s != null && s.ValidarUbicacionLogin)
                    .ToList();

                if (sucursalesConValidacion.Count == 0)
                {
                    permitido = true;
                    motivo = "Empresa sin sucursales con validacion de ubicacion.";
                }
                else if (user.Admin || user.PermitirLoginFueraSucursal)
                {
                    permitido = true;
                    motivo = "Usuario exceptuado de validacion de ubicacion.";
                }
                else if (sucursalesConValidacion.Any(s => !s.Latitud.HasValue || !s.Longitud.HasValue))
                {
                    permitido = true;
                    sucursalLog = sucursalesConValidacion.FirstOrDefault(s => !s.Latitud.HasValue || !s.Longitud.HasValue) ?? user.Sucursal;
                    motivo = "Existe al menos una sucursal con validacion sin coordenadas configuradas.";
                }
                else if (!latitud.HasValue || !longitud.HasValue)
                {
                    permitido = false;
                    motivo = "El navegador no envio coordenadas.";
                    mensajeError = "Debe permitir la ubicacion del navegador para iniciar sesion.";
                }
                else if (!precisionMetros.HasValue)
                {
                    permitido = false;
                    motivo = "El navegador no informo la precision de la ubicacion.";
                    mensajeError = "No fue posible validar la precision de tu ubicacion. Intenta nuevamente.";
                }
                else if (precisionMetros.Value > PrecisionMaximaLoginMetros)
                {
                    permitido = false;
                    motivo = "Precision insuficiente de ubicacion.";
                    mensajeError = "La precision de la ubicacion es insuficiente. Intenta nuevamente desde una zona con mejor senal.";
                }
                else
                {
                    Entidades.Sucursal sucursalPermitida = null;
                    Entidades.Sucursal sucursalMasCercana = null;
                    decimal? distanciaMinima = null;

                    foreach (var sucursalEmpresa in sucursalesConValidacion)
                    {
                        decimal distanciaSucursal = CalcularDistanciaHaversineMetros(
                            sucursalEmpresa.Latitud.Value,
                            sucursalEmpresa.Longitud.Value,
                            latitud.Value,
                            longitud.Value);

                        if (!distanciaMinima.HasValue || distanciaSucursal < distanciaMinima.Value)
                        {
                            distanciaMinima = distanciaSucursal;
                            sucursalMasCercana = sucursalEmpresa;
                        }

                        if (distanciaSucursal <= sucursalEmpresa.RadioLoginMetros)
                        {
                            sucursalPermitida = sucursalEmpresa;
                            distanciaMetros = distanciaSucursal;
                            break;
                        }
                    }

                    if (sucursalPermitida != null)
                    {
                        permitido = true;
                        sucursalLog = sucursalPermitida;
                        motivo = "Ubicacion validada correctamente contra una sucursal de la empresa.";
                    }
                    else
                    {
                        permitido = false;
                        sucursalLog = sucursalMasCercana ?? user.Sucursal;
                        distanciaMetros = distanciaMinima;
                        motivo = "Ubicacion fuera del radio permitido en todas las sucursales validadas.";
                        mensajeError = "No fue posible iniciar sesion porque no estas dentro del radio permitido de ninguna sucursal habilitada.";
                    }
                }
            }

            RegistrarLoginUbicacion(user, sucursalLog, latitud, longitud, precisionMetros, distanciaMetros, permitido, motivo, ip);
            return permitido;
        }

        private bool ValidarUbicacionLoginEmpresa(Entidades.Usuario user, LoginUbicacionVm model, out string mensajeError)
        {
            return ValidarUbicacionLoginEmpresa(
                user,
                model != null ? model.Latitud : null,
                model != null ? model.Longitud : null,
                model != null ? model.PrecisionMetros : null,
                out mensajeError);
        }

        private bool ValidarUbicacionLoginEmpresa(
            Entidades.Usuario user,
            string latitudTexto,
            string longitudTexto,
            string precisionMetrosTexto,
            out string mensajeError)
        {
            mensajeError = null;
            if (user == null)
                return false;

            decimal? latitud = ParseNullableDecimal(latitudTexto);
            decimal? longitud = ParseNullableDecimal(longitudTexto);
            decimal? precisionMetros = ParseNullableDecimal(precisionMetrosTexto);
            decimal? distanciaMetros = null;
            string ip = ObtenerDireccionIp();
            string motivo;
            bool permitido;
            var sucursalLog = user.Sucursal;

            if (user.Sucursal == null)
            {
                permitido = true;
                motivo = "Usuario sin sucursal asignada.";
            }
            else
            {
                var sucursalesEmpresa = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
                var sucursalesConValidacion = sucursalesEmpresa
                    .Where(s => s != null && s.ValidarUbicacionLogin)
                    .ToList();

                if (sucursalesConValidacion.Count == 0)
                {
                    permitido = true;
                    motivo = "Empresa sin sucursales con validacion de ubicacion.";
                }
                else if (user.Admin || user.PermitirLoginFueraSucursal)
                {
                    permitido = true;
                    motivo = "Usuario exceptuado de validacion de ubicacion.";
                }
                else if (sucursalesConValidacion.Any(s => !s.Latitud.HasValue || !s.Longitud.HasValue))
                {
                    permitido = true;
                    sucursalLog = sucursalesConValidacion.FirstOrDefault(s => !s.Latitud.HasValue || !s.Longitud.HasValue) ?? user.Sucursal;
                    motivo = "Existe al menos una sucursal con validacion sin coordenadas configuradas.";
                }
                else if (!latitud.HasValue || !longitud.HasValue)
                {
                    permitido = false;
                    motivo = "El navegador no envio coordenadas.";
                    mensajeError = "Debe permitir la ubicacion del navegador para continuar.";
                }
                else if (!precisionMetros.HasValue)
                {
                    permitido = false;
                    motivo = "El navegador no informo la precision de la ubicacion.";
                    mensajeError = "No fue posible validar la precision de tu ubicacion. Intenta nuevamente.";
                }
                else if (precisionMetros.Value > PrecisionMaximaLoginMetros)
                {
                    permitido = false;
                    motivo = "Precision insuficiente de ubicacion.";
                    mensajeError = "La precision de la ubicacion es insuficiente. Intenta nuevamente desde una zona con mejor senal.";
                }
                else
                {
                    Entidades.Sucursal sucursalPermitida = null;
                    Entidades.Sucursal sucursalMasCercana = null;
                    decimal? distanciaMinima = null;

                    foreach (var sucursalEmpresa in sucursalesConValidacion)
                    {
                        decimal distanciaSucursal = CalcularDistanciaHaversineMetros(
                            sucursalEmpresa.Latitud.Value,
                            sucursalEmpresa.Longitud.Value,
                            latitud.Value,
                            longitud.Value);

                        if (!distanciaMinima.HasValue || distanciaSucursal < distanciaMinima.Value)
                        {
                            distanciaMinima = distanciaSucursal;
                            sucursalMasCercana = sucursalEmpresa;
                        }

                        if (distanciaSucursal <= sucursalEmpresa.RadioLoginMetros)
                        {
                            sucursalPermitida = sucursalEmpresa;
                            distanciaMetros = distanciaSucursal;
                            break;
                        }
                    }

                    if (sucursalPermitida != null)
                    {
                        permitido = true;
                        sucursalLog = sucursalPermitida;
                        motivo = "Ubicacion validada correctamente contra una sucursal de la empresa.";
                    }
                    else
                    {
                        permitido = false;
                        sucursalLog = sucursalMasCercana ?? user.Sucursal;
                        distanciaMetros = distanciaMinima;
                        motivo = "Ubicacion fuera del radio permitido en todas las sucursales validadas.";
                        mensajeError = "No fue posible continuar porque no estas dentro del radio permitido de ninguna sucursal habilitada.";
                    }
                }
            }

            RegistrarLoginUbicacion(user, sucursalLog, latitud, longitud, precisionMetros, distanciaMetros, permitido, motivo, ip);
            return permitido;
        }

        private void RegistrarLoginUbicacion(
            Entidades.Usuario user,
            Entidades.Sucursal sucursal,
            decimal? latitud,
            decimal? longitud,
            decimal? precisionMetros,
            decimal? distanciaMetros,
            bool permitido,
            string motivo,
            string ip)
        {
            if (user == null)
                return;

            try
            {
                oUsuarioN.RegistrarLoginUbicacion(new LoginUbicacionLog
                {
                    IdUsuario = user.Id,
                    IdSucursal = sucursal != null ? sucursal.IdSucursal : user.IdSucursal,
                    FechaHora = DateTime.Now,
                    Latitud = latitud,
                    Longitud = longitud,
                    PrecisionMetros = precisionMetros,
                    DistanciaMetros = distanciaMetros,
                    Permitido = permitido,
                    Motivo = motivo ?? string.Empty,
                    Ip = ip ?? string.Empty
                });
            }
            catch
            {
                // La auditoría no debe impedir el login ni ocultar el motivo real del bloqueo.
            }
        }

        private static decimal CalcularDistanciaHaversineMetros(decimal lat1, decimal lon1, decimal lat2, decimal lon2)
        {
            const double radioTierraMetros = 6371000d;

            double dLat = GradosARadianes((double)(lat2 - lat1));
            double dLon = GradosARadianes((double)(lon2 - lon1));
            double origenLat = GradosARadianes((double)lat1);
            double destinoLat = GradosARadianes((double)lat2);

            double a = Math.Sin(dLat / 2d) * Math.Sin(dLat / 2d)
                + Math.Cos(origenLat) * Math.Cos(destinoLat) * Math.Sin(dLon / 2d) * Math.Sin(dLon / 2d);

            double c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            return Convert.ToDecimal(radioTierraMetros * c);
        }

        private static double GradosARadianes(double grados)
        {
            return grados * Math.PI / 180d;
        }

        private static decimal? ParseNullableDecimal(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            decimal parsed;
            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.CurrentCulture, out parsed))
                return parsed;

            string normalizado = value.Replace(",", ".");
            if (decimal.TryParse(normalizado, NumberStyles.Float, CultureInfo.InvariantCulture, out parsed))
                return parsed;

            return null;
        }

        private string ObtenerDireccionIp()
        {
            try
            {
                string forwarded = Request != null ? Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : null;
                if (!string.IsNullOrWhiteSpace(forwarded))
                {
                    string ipForwarded = forwarded
                        .Split(',')
                        .Select(x => (x ?? "").Trim())
                        .FirstOrDefault(x => x.Length > 0);

                    if (!string.IsNullOrWhiteSpace(ipForwarded))
                        return ipForwarded;
                }

                if (Request != null && !string.IsNullOrWhiteSpace(Request.UserHostAddress))
                    return Request.UserHostAddress;
            }
            catch
            {
            }

            return string.Empty;
        }
    }
}
