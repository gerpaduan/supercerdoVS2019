using System;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Fase 3 de la integracion con Mercado Pago Point: alta de Sucursal (Store), Caja (POS) y
    // verificacion de la terminal fisica vinculada, contra la API real de Mercado Pago (ver
    // docs/DECISIONS.md 2026-09-01). Mismo patron de permisos que MercadoPagoController --
    // PuedeAdministrar = admin de la misma empresa. Todas las acciones que llaman a la API de
    // Mercado Pago exigen ademas que la empresa este Conectada (con access_token vigente).
    public class MercadoPagoTerminalesController : BaseController
    {
        private Negocio.MercadoPagoConfig oMercadoPagoConfigN;
        private Negocio.MercadoPagoSucursalConfig oSucursalConfigN;
        private Negocio.TerminalMercadoPago oTerminalN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oMercadoPagoConfigN = Web.Infrastructure.NegocioFactory.CrearMercadoPagoConfig(empresa);
            oSucursalConfigN = Web.Infrastructure.NegocioFactory.CrearMercadoPagoSucursalConfig(empresa);
            oTerminalN = Web.Infrastructure.NegocioFactory.CrearTerminalMercadoPago(empresa);
            oSucursalN = Web.Infrastructure.NegocioFactory.CrearSucursal(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }

            var configMp = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            var terminales = oTerminalN.Listar(empresa.IdEmpresa);

            var model = new MercadoPagoTerminalesIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Conectado = configMp != null && configMp.Conectado
            };

            foreach (var sucursal in oSucursalN.findAll())
            {
                var configSucursal = oSucursalConfigN.ObtenerPorSucursal(sucursal.IdSucursal, empresa.IdEmpresa);
                model.Sucursales.Add(new SucursalTerminalesVm
                {
                    IdSucursal = sucursal.IdSucursal,
                    NombreSucursal = sucursal.SucursalNombre,
                    MpStoreId = configSucursal != null ? configSucursal.MpStoreId : "",
                    Terminales = terminales
                        .Where(t => t.IdSucursal == sucursal.IdSucursal)
                        .Select(t => new TerminalItemVm
                        {
                            Id = t.Id,
                            Alias = t.Alias,
                            PosId = t.PosId,
                            TerminalIdMp = t.TerminalIdMp,
                            Activo = t.Activo
                        })
                        .ToList()
                });
            }

            ViewBag.Title = "Terminales Mercado Pago";
            ViewBag.Seccion = "Mercado Pago";
            return View("~/Views/MercadoPagoTerminales/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CrearStore(int idSucursal, string calle, string numero, string ciudad, string provincia)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }
            if (!PuedeAdministrar(usuario))
            {
                return SinPermisoAAdministrar();
            }

            var configMp = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            if (configMp == null || !configMp.Conectado)
            {
                return SinConexion();
            }

            var sucursal = oSucursalN.findAll().FirstOrDefault(s => s.IdSucursal == idSucursal);
            if (sucursal == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se encontró la sucursal.";
                return RedirectToAction("Index");
            }

            try
            {
                var oauthClient = new Negocio.MercadoPagoPointClient();
                string storeId = oauthClient.CrearStore(configMp.AccessToken, configMp.MpUserId, new Negocio.MercadoPagoPointClient.DatosStore
                {
                    Nombre = sucursal.SucursalNombre,
                    ExternalId = "sucursal-" + sucursal.IdSucursal,
                    Calle = calle,
                    Numero = numero,
                    Ciudad = ciudad,
                    Provincia = provincia,
                    Latitud = sucursal.Latitud,
                    Longitud = sucursal.Longitud
                });

                oSucursalConfigN.GuardarStoreId(idSucursal, empresa.IdEmpresa, storeId);

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "La sucursal se dio de alta en Mercado Pago correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se pudo dar de alta la sucursal en Mercado Pago: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarTerminal(int idSucursal, string alias)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }
            if (!PuedeAdministrar(usuario))
            {
                return SinPermisoAAdministrar();
            }

            var configMp = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            if (configMp == null || !configMp.Conectado)
            {
                return SinConexion();
            }

            var configSucursal = oSucursalConfigN.ObtenerPorSucursal(idSucursal, empresa.IdEmpresa);
            if (configSucursal == null || string.IsNullOrWhiteSpace(configSucursal.MpStoreId))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Primero hay que dar de alta la sucursal en Mercado Pago.";
                return RedirectToAction("Index");
            }

            try
            {
                var client = new Negocio.MercadoPagoPointClient();
                string posId = client.CrearPos(configMp.AccessToken, configSucursal.MpStoreId, alias, "terminal-" + idSucursal + "-" + Guid.NewGuid().ToString("N").Substring(0, 8));

                oTerminalN.Agregar(new Entidades.TerminalMercadoPago
                {
                    IdEmpresa = empresa.IdEmpresa,
                    IdSucursal = idSucursal,
                    PosId = posId,
                    Alias = alias
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Caja creada en Mercado Pago. Ahora emparejá la terminal física escaneando el QR con la app de Mercado Pago, y después presioná \"Verificar vinculación\".";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se pudo crear la caja en Mercado Pago: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult VerificarVinculacion(int idTerminal)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }
            if (!PuedeAdministrar(usuario))
            {
                return SinPermisoAAdministrar();
            }

            var configMp = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            if (configMp == null || !configMp.Conectado)
            {
                return SinConexion();
            }

            var terminal = oTerminalN.Listar(empresa.IdEmpresa).FirstOrDefault(t => t.Id == idTerminal);
            if (terminal == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se encontró la terminal.";
                return RedirectToAction("Index");
            }

            var configSucursal = oSucursalConfigN.ObtenerPorSucursal(terminal.IdSucursal, empresa.IdEmpresa);

            try
            {
                var client = new Negocio.MercadoPagoPointClient();
                string terminalIdMp = client.BuscarTerminalVinculada(configMp.AccessToken, configSucursal.MpStoreId, terminal.PosId);

                if (string.IsNullOrWhiteSpace(terminalIdMp))
                {
                    TempData["AlertType"] = "info";
                    TempData["AlertTitle"] = "Mercado Pago";
                    TempData["AlertMsg"] = "Todavía no hay ninguna terminal física emparejada a esta caja. Escaneá el QR con la app de Mercado Pago y volvé a intentar.";
                }
                else
                {
                    oTerminalN.VincularTerminalFisica(terminal, terminalIdMp);
                    TempData["AlertType"] = "success";
                    TempData["AlertTitle"] = "Mercado Pago";
                    TempData["AlertMsg"] = "Terminal vinculada correctamente: " + terminalIdMp;
                }
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se pudo verificar la vinculación: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ActivarModoIntegrado(int idTerminal)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Mercado Pago");
            }
            if (!PuedeAdministrar(usuario))
            {
                return SinPermisoAAdministrar();
            }

            var configMp = oMercadoPagoConfigN.ObtenerPorEmpresa(empresa.IdEmpresa);
            if (configMp == null || !configMp.Conectado)
            {
                return SinConexion();
            }

            var terminal = oTerminalN.Listar(empresa.IdEmpresa).FirstOrDefault(t => t.Id == idTerminal);
            if (terminal == null || string.IsNullOrWhiteSpace(terminal.TerminalIdMp))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "La terminal todavía no está vinculada -- verificá la vinculación primero.";
                return RedirectToAction("Index");
            }

            try
            {
                var client = new Negocio.MercadoPagoPointClient();
                client.CambiarOperatingMode(configMp.AccessToken, terminal.TerminalIdMp, "PDV");

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "Terminal pasada a modo integrado (PDV).";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mercado Pago";
                TempData["AlertMsg"] = "No se pudo cambiar el modo de la terminal: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        private ActionResult SinPermisoAAdministrar()
        {
            TempData["AlertType"] = "info";
            TempData["AlertTitle"] = "Mercado Pago";
            TempData["AlertMsg"] = "Solo un administrador puede hacer esto.";
            return RedirectToAction("Index");
        }

        private ActionResult SinConexion()
        {
            TempData["AlertType"] = "error";
            TempData["AlertTitle"] = "Mercado Pago";
            TempData["AlertMsg"] = "Primero hay que conectar la cuenta de Mercado Pago.";
            return RedirectToAction("Index", "MercadoPago");
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
