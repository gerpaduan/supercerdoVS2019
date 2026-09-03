using System;
using System.Web.Mvc;
using Web.Models;

namespace Web.Controllers
{
    // Pantalla "Codigos de barra" (Configuracion): formatos de codigo interno de balanza
    // (EAN-13, prefijo 20-29) por empresa -- ver Negocio/BarcodeInterpreter.cs para como se
    // usan estos formatos al interpretar un codigo escaneado en el POS. Mismo patron de
    // permiso que DispositivosSegurosController/SucursalController: cualquier usuario de la
    // empresa puede ver, solo un Admin puede administrar (crear/editar/desactivar).
    public class CodigosBarraController : BaseController
    {
        private Negocio.FormatoCodigoBarras oFormatoN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oFormatoN = Web.Infrastructure.NegocioFactory.CrearFormatoCodigoBarras(empresa);
        }

        [HttpGet]
        public ActionResult Index()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa)
            {
                return VistaAccesoDenegado("Códigos de barra");
            }

            var model = new FormatosCodigoBarraIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(usuario),
                Items = oFormatoN.Listar(empresa.IdEmpresa)
            };

            ViewBag.Title = "Códigos de barra";
            ViewBag.Seccion = "Códigos de barra";
            return View("~/Views/CodigosBarra/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult Nuevo()
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa || !PuedeAdministrar(usuario))
            {
                return VistaAccesoDenegado("Códigos de barra");
            }

            var model = new FormatoCodigoBarraEditVm
            {
                EsNuevo = true,
                LongitudTotal = 13,
                Activo = true
            };

            ViewBag.Title = "Nuevo formato de código de barra";
            ViewBag.Seccion = "Códigos de barra";
            return View("~/Views/CodigosBarra/Editar.cshtml", model);
        }

        [HttpGet]
        public ActionResult Editar(int id)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa || !PuedeAdministrar(usuario))
            {
                return VistaAccesoDenegado("Códigos de barra");
            }

            var formato = oFormatoN.ObtenerPorId(id, empresa.IdEmpresa);
            if (formato == null)
            {
                TempData["AlertType"] = "danger";
                TempData["AlertTitle"] = "Códigos de barra";
                TempData["AlertMsg"] = "No se encontró el formato solicitado.";
                return RedirectToAction("Index");
            }

            var model = new FormatoCodigoBarraEditVm
            {
                Id = formato.Id,
                EsNuevo = false,
                Nombre = formato.Nombre,
                Prefijo = formato.Prefijo,
                LongitudTotal = formato.LongitudTotal,
                PosicionCodigo = formato.PosicionCodigo,
                LongitudCodigo = formato.LongitudCodigo,
                PosicionValor = formato.PosicionValor,
                LongitudValor = formato.LongitudValor,
                TipoValor = formato.TipoValor,
                CantidadDecimales = formato.CantidadDecimales,
                Activo = formato.Activo
            };

            ViewBag.Title = "Editar formato de código de barra";
            ViewBag.Seccion = "Códigos de barra";
            return View("~/Views/CodigosBarra/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(FormatoCodigoBarraEditVm model)
        {
            var usuario = ObtenerUsuarioActual();
            if (usuario == null || usuario.IdEmpresa != empresa.IdEmpresa || !PuedeAdministrar(usuario))
            {
                return VistaAccesoDenegado("Códigos de barra");
            }

            model = model ?? new FormatoCodigoBarraEditVm();

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.EsNuevo ? "Nuevo formato de código de barra" : "Editar formato de código de barra";
                ViewBag.Seccion = "Códigos de barra";
                return View("~/Views/CodigosBarra/Editar.cshtml", model);
            }

            try
            {
                var formato = new Entidades.FormatoCodigoBarras
                {
                    Id = model.Id,
                    IdEmpresa = empresa.IdEmpresa,
                    Nombre = (model.Nombre ?? "").Trim(),
                    Prefijo = model.Prefijo,
                    LongitudTotal = model.LongitudTotal,
                    PosicionCodigo = model.PosicionCodigo,
                    LongitudCodigo = model.LongitudCodigo,
                    PosicionValor = model.PosicionValor,
                    LongitudValor = model.LongitudValor,
                    TipoValor = model.TipoValor,
                    CantidadDecimales = model.CantidadDecimales,
                    Activo = model.Activo
                };

                if (model.EsNuevo)
                {
                    formato.IdUsuarioCreador = usuario.Id;
                    oFormatoN.Agregar(formato);
                }
                else
                {
                    var actual = oFormatoN.ObtenerPorId(model.Id, empresa.IdEmpresa);
                    if (actual == null)
                    {
                        TempData["AlertType"] = "danger";
                        TempData["AlertTitle"] = "Códigos de barra";
                        TempData["AlertMsg"] = "No se encontró el formato solicitado.";
                        return RedirectToAction("Index");
                    }

                    // El prefijo no se edita desde la UI (ver Web/Models/FormatoCodigoBarraVm.cs) --
                    // se conserva el original aunque el form lo mande de otra forma.
                    formato.Prefijo = actual.Prefijo;
                    formato.IdUsuarioModificador = usuario.Id;
                    oFormatoN.Actualizar(formato);
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Códigos de barra";
                TempData["AlertMsg"] = "El formato se guardó correctamente.";
                return RedirectToAction("Index");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = model.EsNuevo ? "Nuevo formato de código de barra" : "Editar formato de código de barra";
                ViewBag.Seccion = "Códigos de barra";
                return View("~/Views/CodigosBarra/Editar.cshtml", model);
            }
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
