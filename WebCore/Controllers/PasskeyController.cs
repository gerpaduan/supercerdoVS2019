// Alta, listado y baja de huellas (passkeys WebAuthn) del usuario logueado, desde "Mi huella" del
// menu de usuario. El LOGIN con huella vive en LoginController (PasskeyOptions/PasskeyLogin, pre-
// sesion). Ver docs/DECISIONS.md "Login por huella (passkeys)". Solo Postgres.
using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using WebCore.Helpers;
using WebCore.Services;

namespace WebCore.Controllers
{
    [Authorize]
    public class PasskeyController : Controller
    {
        private const string SessionAttestationOptions = "fido2.attestationOptions";

        private readonly IUsuarioSesionService _sesion;
        private readonly ILogger<PasskeyController> _logger;

        public PasskeyController(IUsuarioSesionService sesion, ILogger<PasskeyController> logger)
        {
            _sesion = sesion;
            _logger = logger;
        }

        private JsonResult Error(string mensaje, int status = 400)
        {
            Response.StatusCode = status;
            return Json(new { ok = false, error = mensaje });
        }

        private Negocio.UsuarioPasskey CrearNegocio()
        {
            return WebCore.Infrastructure.NegocioFactory.CrearUsuarioPasskey(_sesion.Empresa);
        }

        // Lista las huellas del usuario logueado (nunca expone clave publica ni credentialId).
        [HttpGet]
        public IActionResult Listar()
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            var usuario = _sesion.UsuarioActual;
            var huellas = CrearNegocio().ListarPorUsuario(usuario.Id, usuario.IdEmpresa)
                .Select(p => new
                {
                    id = p.Id,
                    nombre = p.Nombre,
                    fechaAlta = p.FechaAltaUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm"),
                    ultimoUso = p.UltimoUsoUtc.HasValue ? p.UltimoUsoUtc.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm") : ""
                });

            return Json(new { ok = true, huellas });
        }

        // Paso 1 del alta: opciones para que el navegador cree la credencial (pide la huella).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegistrationOptions()
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            var usuario = _sesion.UsuarioActual;
            var oPasskeyN = CrearNegocio();

            // WebAuthn exige el mismo userHandle para todas las passkeys de un usuario en el sitio.
            // Aleatorio (nunca el id ni datos personales).
            byte[] userHandle = oPasskeyN.ObtenerUserHandleExistente(usuario.Id, usuario.IdEmpresa)
                ?? RandomNumberGenerator.GetBytes(32);

            string nombreVisible = string.IsNullOrWhiteSpace(usuario.Nombre) ? usuario.User : usuario.Nombre;
            var fidoUser = new Fido2User
            {
                Id = userHandle,
                Name = usuario.User,
                DisplayName = nombreVisible
            };

            // Excluye las huellas ya registradas: el navegador no deja registrar dos veces la misma.
            var yaRegistradas = oPasskeyN.ListarPorUsuario(usuario.Id, usuario.IdEmpresa)
                .Select(p => new PublicKeyCredentialDescriptor(p.CredentialId))
                .ToList();

            var fido2 = HttpContext.RequestServices.GetRequiredService<IFido2>();
            var options = fido2.RequestNewCredential(new RequestNewCredentialParams
            {
                User = fidoUser,
                ExcludeCredentials = yaRegistradas,
                AuthenticatorSelection = new AuthenticatorSelection
                {
                    // Credencial descubrible: es lo que permite ingresar sin escribir el usuario.
                    ResidentKey = ResidentKeyRequirement.Required,
                    UserVerification = UserVerificationRequirement.Required
                },
                AttestationPreference = AttestationConveyancePreference.None
            });

            HttpContext.Session.SetString(SessionAttestationOptions, options.ToJson());
            return Json(options);
        }

        // Paso 2 del alta: verifica la respuesta del autenticador y guarda la clave publica.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrationComplete([FromBody] AuthenticatorAttestationRawResponse attestationResponse, string? nombre = "")
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            // Desafio de un solo uso: se borra siempre, aunque la verificacion falle.
            string? optionsJson = HttpContext.Session.GetString(SessionAttestationOptions);
            HttpContext.Session.Remove(SessionAttestationOptions);
            if (string.IsNullOrEmpty(optionsJson))
                return Error("La solicitud venció. Volvé a intentar.");

            var options = CredentialCreateOptions.FromJson(optionsJson);
            var usuario = _sesion.UsuarioActual;
            var oPasskeyN = CrearNegocio();

            try
            {
                var fido2 = HttpContext.RequestServices.GetRequiredService<IFido2>();
                var credencial = await fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
                {
                    AttestationResponse = attestationResponse,
                    OriginalOptions = options,
                    // El credentialId es unico en todo el sistema (indice unico en la tabla).
                    IsCredentialIdUniqueToUserCallback = (args, cancellationToken) =>
                        Task.FromResult(oPasskeyN.ObtenerPorCredentialIdSinTenant(args.CredentialId) == null)
                });

                oPasskeyN.Registrar(
                    usuario.Id, usuario.IdEmpresa, credencial.Id, credencial.PublicKey, credencial.SignCount,
                    options.User.Id, credencial.AaGuid, string.Join(",", credencial.Transports ?? []), nombre ?? "");

                return Json(new { ok = true });
            }
            catch (Fido2VerificationException ex)
            {
                _logger.LogWarning(ex, "Alta de huella rechazada para el usuario {IdUsuario}.", usuario.Id);
                return Error("No se pudo registrar la huella. Volvé a intentar.");
            }
            catch (InvalidOperationException ex)
            {
                // Tope de huellas por usuario (Negocio.UsuarioPasskey): el mensaje es apto para el usuario.
                return Error(ex.Message);
            }
        }

        // Da de baja una huella propia (solo borra si pertenece al usuario logueado).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Revocar(int id)
        {
            if (!PasskeySettings.Habilitado)
                return NotFound();

            var usuario = _sesion.UsuarioActual;
            bool borrada = CrearNegocio().Eliminar(id, usuario.Id, usuario.IdEmpresa);
            return borrada ? Json(new { ok = true }) : Error("No se encontró la huella.", 404);
        }
    }
}
