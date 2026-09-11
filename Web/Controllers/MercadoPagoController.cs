using System;
using System.Configuration;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Panel "Conectar con Mercado Pago" (Fase 2 de la integracion con Point, ver
    // docs/DECISIONS.md). Mismo patron de permiso que EmpresaController/WhatsAppController:
    // cualquier usuario de la empresa puede ver el estado, solo un Admin puede conectar/
    // desconectar. Flujo OAuth authorization_code + PKCE contra una unica Aplicacion "CarniSys"
    // (Web.config: MercadoPagoRedirectUri; Web/Config/appSettings.secrets.config: ClientId/
    // ClientSecret) -- una sola Aplicacion para toda la plataforma, cada empresa autoriza la
    // suya (Negocio.MercadoPagoConfig guarda un access_token/refresh_token por empresa,
    // cifrados).
    public class MercadoPagoController : BaseController
    {
        private Negocio.MercadoPagoConfig oMercadoPagoConfigN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oMercadoPagoConfigN = Web.Infrastructure.NegocioFactory.CrearMercadoPagoConfig(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }

            var config = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            var model = new MercadoPagoIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Conectado = config != null && config.Conectado,
                FechaConexionUtc = config != null ? config.FechaConexionUtc : null,
                TokenExpiraUtc = config != null ? config.TokenExpiraUtc : null
            };

            ViewBag.Title = "Mercado Pago";
            ViewBag.Seccion = "Mercado Pago";
            return View("~/Views/MercadoPago/Index.cshtml", model);
        }

        // Arranca el flujo OAuth: genera state (guardado en mercadopago_config) y code_verifier
        // de PKCE (guardado en Session -- solo hace falta durante el ida y vuelta con Mercado
        // Pago, en el mismo navegador del admin) y redirige a la pantalla de autorizacion.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Conectar()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }

            if (!PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Solo un administrador puede conectar la cuenta de Mercado Pago.";
                return RedirectToAction("Index");
            }

            string clientId = ConfigurationManager.AppSettings["MercadoPagoClientId"];
            string redirectUri = ConfigurationManager.AppSettings["MercadoPagoRedirectUri"];
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Falta configurar MercadoPagoClientId/MercadoPagoRedirectUri en el servidor.";
                return RedirectToAction("Index");
            }

            string state = oMercadoPagoConfigN.IniciarConexion(empresa.IdEmpresa);
            string codeVerifier = Utilidades.PasswordSecurity.GenerateToken(32);
            string codeChallenge = Utilidades.PasswordSecurity.ComputeSha256UrlSafeBase64(codeVerifier);
            Session["MercadoPagoCodeVerifier_" + empresa.IdEmpresa] = codeVerifier;

            var oauthClient = new Negocio.MercadoPagoOAuthClient();
            string urlAutorizacion = oauthClient.ConstruirUrlAutorizacion(clientId, redirectUri, state, codeChallenge);
            return Redirect(urlAutorizacion);
        }

        // Mercado Pago redirige aca despues de que el admin autoriza (o cancela) en su pantalla.
        // Sigue siendo la sesion normal del admin en CarniSys (mismo navegador, mismo login) --
        // no hace falta [AllowAnonymous] aca, a diferencia del webhook de Fase 4.
        [HttpGet]
        public ActionResult Callback(string code, string state)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Mercado Pago no envió los datos esperados. Probá conectar de nuevo.";
                return RedirectToAction("Index");
            }

            var configActual = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            if (configActual == null || string.IsNullOrEmpty(configActual.ClientState) || configActual.ClientState != state)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "El intento de conexión no es válido (no coincide con el que inició CarniSys). Probá conectar de nuevo.";
                return RedirectToAction("Index");
            }

            string codeVerifier = Session["MercadoPagoCodeVerifier_" + empresa.IdEmpresa] as string;
            if (string.IsNullOrWhiteSpace(codeVerifier))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Se perdió el dato de seguridad de la conexión (la sesión pudo haber expirado). Probá conectar de nuevo desde el principio.";
                return RedirectToAction("Index");
            }

            string clientId = ConfigurationManager.AppSettings["MercadoPagoClientId"];
            string clientSecret = ConfigurationManager.AppSettings["MercadoPagoClientSecret"];
            string redirectUri = ConfigurationManager.AppSettings["MercadoPagoRedirectUri"];

            try
            {
                var oauthClient = new Negocio.MercadoPagoOAuthClient();
                // modoPrueba fijo en true (Fase 2 arranca con credenciales de Prueba, decision
                // confirmada con el usuario) -- pasar a false requiere decision explicita antes
                // de mover esto a Productivas.
                var resultado = oauthClient.IntercambiarCodigoPorTokens(clientId, clientSecret, code, redirectUri, codeVerifier, modoPrueba: true);

                long segundos = resultado.ExpiresInSeconds > 0 ? resultado.ExpiresInSeconds : 15552000; // 180 dias, default documentado por MP
                DateTime tokenExpiraUtc = DateTime.UtcNow.AddSeconds(segundos);
                oMercadoPagoConfigN.GuardarTokens(empresa.IdEmpresa, resultado.AccessToken, resultado.RefreshToken, tokenExpiraUtc, usuario.Id, resultado.UserId);

                Session.Remove("MercadoPagoCodeVerifier_" + empresa.IdEmpresa);

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "La cuenta de Mercado Pago se conectó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se pudo completar la conexión con Mercado Pago: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Desconectar()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }

            if (!PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "info";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Solo un administrador puede desconectar la cuenta de Mercado Pago.";
                return RedirectToAction("Index");
            }

            oMercadoPagoConfigN.Desconectar(empresa.IdEmpresa);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Mercado Pago";
            TempData["AlertMsg"] = "Se desconectó la cuenta de Mercado Pago.";
            return RedirectToAction("Index");
        }

        private bool PuedeAdministrar(Entidades.Usuario usuario)
        {
            return usuario != null && usuario.IdEmpresa == empresa.IdEmpresa && usuario.Admin;
        }

        private Entidades.Usuario ObtenerUsuarioActual()
        {
            return Session["Usuario"] as Entidades.Usuario;
        }
    }
}
