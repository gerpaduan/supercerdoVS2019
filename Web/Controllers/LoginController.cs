using Entidades;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Utilidades;
using Web.Helpers;
using Web.Models;
using wsAFIPvs2008;

namespace Web.Controllers
{
    public class LoginController : Controller
    {
        private const string GenericRecoveryMessage = "Si los datos ingresados corresponden a un usuario registrado, recibirás instrucciones para recuperar tu contraseña.";

        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            IEmpresaContext empresa = new EmpresaContextNulo();
            oUsuarioN = new Negocio.Usuario(empresa);
            oSucursalN = new Negocio.Sucursal(empresa);
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
            model.ReturnUrl = model.ReturnUrl ?? "";

            if (!ModelState.IsValid)
                return View("~/Views/Login/Index.cshtml", model);

            var user = oUsuarioN.ValidarUsuarioWeb(model.Usuario, model.Clave);

            if (user != null && user.Activo)
            {
                IEmpresaContext empresa = new EmpresaContextWin(user.IdEmpresa);

                oUsuarioN = new Negocio.Usuario(empresa);
                oSucursalN = new Negocio.Sucursal(empresa);

                user = oUsuarioN.getUsuarioById(user.Id);
                if (user != null)
                    user.Permisos = oUsuarioN.getPermisosUsuario(user.Id);

                string sucNombre = user != null && user.Sucursal == null
                    ? "Seleccione Sucursal"
                    : (user != null && user.Sucursal != null ? user.Sucursal.SucursalNombre : "");

                if (user != null)
                    user.SucursalNombre = sucNombre ?? "";

                Session["Usuario"] = user;
                Session["IdEmpresa"] = user != null ? user.IdEmpresa : 0;
                Session.Remove("PARAM_CTX");

                IParametrosContext paramCtx = new Negocio.Parametros(new EmpresaContextWeb());
                paramCtx.Reload();
                Session["PARAM_CTX"] = paramCtx;

                if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                return RedirectToAction("Index", "Home");
            }

            model.Clave = "";
            model.Error = user != null && !user.Activo
                ? "No fue posible iniciar sesión. Verificá tus datos o consultá a un administrador si tu usuario está inactivo."
                : "Usuario o contraseña incorrectos.";

            return View("~/Views/Login/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View("~/Views/Login/ForgotPassword.cshtml", new PasswordRecoveryRequestVm());
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
                TokenValido = TokenEsValido(token)
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
            model.TokenValido = TokenEsValido(model.Token);

            ValidarNuevaClave(model);

            if (!model.TokenValido)
                ModelState.AddModelError("", "El enlace de recuperación no es válido o ya venció.");

            if (!ModelState.IsValid)
                return View("~/Views/Login/ResetPassword.cshtml", model);

            string tokenHash = PasswordSecurity.ComputeSha256Base64(model.Token);
            var token = oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
            if (token == null || token.Usado || token.FechaExpiracionUtc < DateTime.UtcNow)
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
            oUsuarioN.InvalidarTokensPendientesUsuario(usuario.Id);

            TempData["Success"] = "Tu contraseña fue actualizada correctamente. Ya podés iniciar sesión.";
            return RedirectToAction("Index");
        }

        public ActionResult Logout()
        {
            Session.Remove("PARAM_CTX");
            Session.Remove("IdEmpresa");
            Session.Clear();

            if (Request.Cookies["ASP.NET_SessionId"] != null)
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);

            Session.Abandon();

            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
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

        private bool TokenEsValido(string tokenRaw)
        {
            if (string.IsNullOrWhiteSpace(tokenRaw))
                return false;

            try
            {
                string tokenHash = PasswordSecurity.ComputeSha256Base64(tokenRaw);
                var token = oUsuarioN.ObtenerTokenRecuperacion(tokenHash);
                return token != null && !token.Usado && token.FechaExpiracionUtc >= DateTime.UtcNow;
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

        private void ValidarNuevaClave(PasswordResetVm model)
        {
            if (string.IsNullOrWhiteSpace(model.NuevaClave))
                return;

            if (model.NuevaClave.Contains(" "))
                ModelState.AddModelError("NuevaClave", "La contraseña no puede contener espacios en blanco.");
        }
    }
}
