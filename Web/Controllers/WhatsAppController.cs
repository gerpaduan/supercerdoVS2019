using System;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    public class WhatsAppController : BaseController
    {
        private Negocio.WhatsApp oWhatsAppN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oWhatsAppN = new Negocio.WhatsApp(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (!PuedeAdministrar(usuario))
                return VistaAccesoDenegado("WhatsApp");

            var configuracion = oWhatsAppN.ObtenerOCrearConfiguracion(usuario.Id);
            var model = MapModel(configuracion, true);

            ViewBag.Title = "WhatsApp";
            ViewBag.Seccion = "WhatsApp";
            return View("~/Views/WhatsApp/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(WhatsAppConfiguracionVm model)
        {
            var usuario = ObtenerUsuarioActual();
            if (!PuedeAdministrar(usuario))
                return VistaAccesoDenegado("WhatsApp");

            model = model ?? new WhatsAppConfiguracionVm();
            model.IdEmpresa = empresa.IdEmpresa;
            model.PuedeAdministrar = true;
            model.SoloLecturaInicial = false;
            model.MensajePermiso = "La configuración se carga inicialmente en modo lectura. Presione Modificar para editar y Guardar para aplicar los cambios a esta empresa.";

            NormalizarModel(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "WhatsApp";
                ViewBag.Seccion = "WhatsApp";
                return View("~/Views/WhatsApp/Index.cshtml", model);
            }

            try
            {
                oWhatsAppN.GuardarConfiguracion(new Entidades.ConfiguracionWhatsApp
                {
                    IdConfiguracionWhatsApp = model.IdConfiguracionWhatsApp,
                    IdEmpresa = empresa.IdEmpresa,
                    Activo = model.Activo,
                    MetaApiVersion = string.IsNullOrWhiteSpace(model.MetaApiVersion) ? "v22.0" : model.MetaApiVersion.Trim(),
                    PhoneNumberId = (model.PhoneNumberId ?? "").Trim(),
                    BusinessAccountId = (model.BusinessAccountId ?? "").Trim(),
                    AccessToken = (model.AccessToken ?? "").Trim(),
                    IdUsuarioModificacion = usuario.Id
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "WhatsApp";
                TempData["AlertMsg"] = "La configuración de WhatsApp se guardó correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = "WhatsApp";
                ViewBag.Seccion = "WhatsApp";
                return View("~/Views/WhatsApp/Index.cshtml", model);
            }
        }

        private static void NormalizarModel(WhatsAppConfiguracionVm model)
        {
            if (model == null) return;

            model.MetaApiVersion = (model.MetaApiVersion ?? "").Trim();
            model.PhoneNumberId = (model.PhoneNumberId ?? "").Trim();
            model.BusinessAccountId = (model.BusinessAccountId ?? "").Trim();
            model.AccessToken = (model.AccessToken ?? "").Trim();

            if (string.IsNullOrWhiteSpace(model.MetaApiVersion))
                model.MetaApiVersion = "v22.0";
        }

        private WhatsAppConfiguracionVm MapModel(Entidades.ConfiguracionWhatsApp configuracion, bool soloLecturaInicial)
        {
            configuracion = configuracion ?? new Entidades.ConfiguracionWhatsApp
            {
                IdEmpresa = empresa.IdEmpresa,
                MetaApiVersion = "v22.0"
            };

            return new WhatsAppConfiguracionVm
            {
                IdConfiguracionWhatsApp = configuracion.IdConfiguracionWhatsApp,
                IdEmpresa = configuracion.IdEmpresa,
                Activo = configuracion.Activo,
                MetaApiVersion = string.IsNullOrWhiteSpace(configuracion.MetaApiVersion) ? "v22.0" : configuracion.MetaApiVersion,
                PhoneNumberId = configuracion.PhoneNumberId ?? "",
                BusinessAccountId = configuracion.BusinessAccountId ?? "",
                AccessToken = configuracion.AccessToken ?? "",
                PuedeAdministrar = true,
                SoloLecturaInicial = soloLecturaInicial,
                MensajePermiso = "La configuración se carga inicialmente en modo lectura. Presione Modificar para editar y Guardar para aplicar los cambios a esta empresa."
            };
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
