using System;
using System.Linq;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Pantalla "Dispositivos seguros" (Configuracion): un admin registra PCs conocidas por su
    // numero de serie (CPU ID via WMI, mismo mecanismo que WinForms). Loguearse desde una de
    // ellas salta el bloqueo por IP del login (LoginRateLimiter) -- no el bloqueo persistente por
    // cuenta, que sigue aplicando igual. Mismo patron de permiso que EmpresaController/
    // SucursalController: cualquier usuario de la empresa puede ver, solo un Admin puede
    // administrar.
    public class DispositivosSegurosController : BaseController
    {
        private Negocio.DispositivoSeguro oDispositivoN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oDispositivoN = new Negocio.DispositivoSeguro(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Dispositivos seguros");
            }

            var model = new DispositivosSegurosIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Items = oDispositivoN.Listar(empresa.IdEmpresa)
            };

            ViewBag.Title = "Dispositivos seguros";
            ViewBag.Seccion = "Dispositivos seguros";
            return View("~/Views/DispositivosSeguros/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Agregar(string numeroSerie, string descripcion)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa || !PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para agregar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            numeroSerie = (numeroSerie ?? "").Trim();
            if (string.IsNullOrWhiteSpace(numeroSerie))
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El número de serie es obligatorio.";
                return RedirectToAction("Index");
            }

            try
            {
                oDispositivoN.Agregar(new Entidades.DispositivoSeguro
                {
                    IdEmpresa = empresa.IdEmpresa,
                    NumeroSerie = numeroSerie,
                    Descripcion = (descripcion ?? "").Trim(),
                    IdUsuarioCreador = usuario.Id
                });

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "El dispositivo se agregó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Dispositivos seguros";
                TempData["AlertMsg"] = "No se pudo agregar el dispositivo: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Eliminar(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa || !PuedeAdministrar(usuario))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para eliminar dispositivos seguros.";
                return RedirectToAction("Index");
            }

            oDispositivoN.Eliminar(id, empresa.IdEmpresa);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Dispositivos seguros";
            TempData["AlertMsg"] = "El dispositivo se eliminó correctamente.";
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
