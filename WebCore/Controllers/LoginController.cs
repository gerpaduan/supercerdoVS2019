// Port de Web/Controllers/LoginController.cs -- login real con Cookie Authentication de ASP.NET
// Core en vez de Session["Usuario"]+Forms Auth (ver docs/DECISIONS.md "Login/Sesion real para
// WebCore"). Alcance v1 (decidido con el usuario): login + rate limiting por IP + bloqueo de
// cuenta por intentos fallidos + dispositivo seguro + horario laboral. Fuera de alcance v1
// (documentado como gap para v2): geo-validacion de ubicacion de login (ValidarUbicacion,
// LoginUbicacionLog), recuperacion de contraseña por email (ForgotPassword/PasswordReset), mail
// automatico de desbloqueo de cuenta (EnviarMailDesbloqueo) -- el desbloqueo en v1 lo hace un
// admin a mano via UsuariosController.DesbloquearUsuario, que ya existe.
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

        public LoginController()
        {
            IEmpresaContext empresaNula = new EmpresaContextNulo();
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(empresaNula);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(empresaNula);
            _oEmpresaN = WebCore.Infrastructure.NegocioFactory.CrearEmpresa(empresaNula);
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

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
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

            return RedirectToAction("Index", "Home");
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
