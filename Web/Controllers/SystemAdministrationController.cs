using System;
using System.Linq;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class SystemAdministrationController : BaseController
    {
        private ISystemAdministrationRepository _repo;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null)
                return;

            _repo = Web.Infrastructure.NegocioFactory.CrearSystemAdministrationRepository();

            if (!SystemAdministrationAccessHelper.PuedeAdministrarSistema(Session))
            {
                ViewBag.Title = "Administracion del sistema";
                ViewBag.Seccion = "Administracion del sistema";
                filterContext.Result = View("~/Views/Shared/AccesoDenegado.cshtml");
                return;
            }

            // Usado por el combo de IVA del producto "Codigo Generico" en EditarEmpresa.cshtml y
            // AltaRapidaEmpresa.cshtml -- se puebla para todas las acciones del controller (lista
            // chica, sin costo real) en vez de repetirlo en cada action/fallback que renderiza esas 2 vistas.
            ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();

            // Sugerencias (datalist) para el input de "Condicion IVA" de AltaRapidaEmpresa.cshtml,
            // mismo catalogo que el combo de /Personas. Misma logica de "poblar para todo el
            // controller" que AlicuotasIva de arriba.
            ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
        }

        public ActionResult Empresas()
        {
            var model = new SystemAdministrationEmpresaIndexVm
            {
                Items = _repo.ObtenerEmpresas()
            };

            ViewBag.Title = "Empresas";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/Empresas.cshtml", model);
        }

        [HttpGet]
        public ActionResult EditarEmpresa(int id = 0)
        {
            var model = id > 0
                ? _repo.ObtenerEmpresa(id)
                : new SystemAdministrationEmpresaEditVm
                {
                    Activa = true
                };

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Empresas";
                TempData["AlertMsg"] = "No se encontro la empresa seleccionada.";
                return RedirectToAction("Empresas");
            }

            model.EsEdicion = id > 0;
            ViewBag.Title = model.EsEdicion ? "Modificar empresa" : "Nueva empresa";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/EditarEmpresa.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            ValidarEmpresa(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.IdEmpresa > 0 ? "Modificar empresa" : "Nueva empresa";
                ViewBag.Seccion = "Administracion del sistema";
                model.EsEdicion = model.IdEmpresa > 0;
                return View("~/Views/SystemAdministration/EditarEmpresa.cshtml", model);
            }

            try
            {
                if (model.IdEmpresa > 0)
                {
                    _repo.ActualizarEmpresa(model);
                    TempData["AlertMsg"] = "La empresa se actualizo correctamente.";
                }
                else
                {
                    _repo.CrearEmpresa(model);
                    TempData["AlertMsg"] = "La empresa se creo correctamente.";
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Empresas";
                return RedirectToAction("Empresas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = model.IdEmpresa > 0 ? "Modificar empresa" : "Nueva empresa";
                ViewBag.Seccion = "Administracion del sistema";
                model.EsEdicion = model.IdEmpresa > 0;
                return View("~/Views/SystemAdministration/EditarEmpresa.cshtml", model);
            }
        }

        public ActionResult Sucursales(int idEmpresa = 0)
        {
            var model = new SystemAdministrationSucursalIndexVm
            {
                FiltroEmpresaId = idEmpresa,
                Empresas = _repo.ObtenerEmpresasSelectList(idEmpresa, true),
                Items = _repo.ObtenerSucursales(idEmpresa),
                TieneTelefono = _repo.TablaSucursalTieneTelefono(),
                TieneActiva = _repo.TablaSucursalTieneActiva()
            };

            ViewBag.Title = "Sucursales";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/Sucursales.cshtml", model);
        }

        [HttpGet]
        public ActionResult EditarSucursal(int id = 0, int idEmpresa = 0)
        {
            var model = id > 0
                ? _repo.ObtenerSucursal(id)
                : new SystemAdministrationSucursalEditVm
                {
                    IdEmpresa = idEmpresa,
                    TieneTelefono = _repo.TablaSucursalTieneTelefono(),
                    TieneActiva = _repo.TablaSucursalTieneActiva(),
                    Activa = true
                };

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Sucursales";
                TempData["AlertMsg"] = "No se encontro la sucursal seleccionada.";
                return RedirectToAction("Sucursales");
            }

            model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
            model.EsEdicion = id > 0;

            if (!model.EsEdicion && model.IdEmpresa > 0)
            {
                model.SucursalesParaCopiarPuntoStock = _repo.ObtenerSucursalesSelectList(model.IdEmpresa);
            }

            ViewBag.Title = model.EsEdicion ? "Modificar sucursal" : "Nueva sucursal";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/EditarSucursal.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarSucursal(SystemAdministrationSucursalEditVm model)
        {
            ValidarSucursal(model);

            if (!ModelState.IsValid)
            {
                model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
                model.TieneTelefono = _repo.TablaSucursalTieneTelefono();
                model.TieneActiva = _repo.TablaSucursalTieneActiva();
                model.EsEdicion = model.IdSucursal > 0;
                if (!model.EsEdicion)
                {
                    model.SucursalesParaCopiarPuntoStock = _repo.ObtenerSucursalesSelectList(model.IdEmpresa);
                }
                ViewBag.Title = model.EsEdicion ? "Modificar sucursal" : "Nueva sucursal";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/EditarSucursal.cshtml", model);
            }

            try
            {
                if (model.IdSucursal > 0)
                {
                    _repo.ActualizarSucursal(model);
                    TempData["AlertMsg"] = "La sucursal se actualizo correctamente.";
                }
                else
                {
                    _repo.CrearSucursal(model);
                    TempData["AlertMsg"] = "La sucursal se creo correctamente.";
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sucursales";
                return RedirectToAction("Sucursales", new { idEmpresa = model.IdEmpresa });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
                model.TieneTelefono = _repo.TablaSucursalTieneTelefono();
                model.TieneActiva = _repo.TablaSucursalTieneActiva();
                model.EsEdicion = model.IdSucursal > 0;
                if (!model.EsEdicion)
                {
                    model.SucursalesParaCopiarPuntoStock = _repo.ObtenerSucursalesSelectList(model.IdEmpresa);
                }
                ViewBag.Title = model.EsEdicion ? "Modificar sucursal" : "Nueva sucursal";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/EditarSucursal.cshtml", model);
            }
        }

        public ActionResult Usuarios(int idEmpresa = 0)
        {
            var model = new SystemAdministrationUsuarioIndexVm
            {
                FiltroEmpresaId = idEmpresa,
                Empresas = _repo.ObtenerEmpresasSelectList(idEmpresa, true),
                Items = _repo.ObtenerUsuarios(idEmpresa)
            };

            ViewBag.Title = "Usuarios";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/Usuarios.cshtml", model);
        }

        [HttpGet]
        public ActionResult EditarUsuario(int id = 0, int idEmpresa = 0)
        {
            var model = id > 0
                ? _repo.ObtenerUsuario(id)
                : new SystemAdministrationUsuarioEditVm
                {
                    IdEmpresa = idEmpresa,
                    Activo = true
                };

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Usuarios";
                TempData["AlertMsg"] = "No se encontro el usuario seleccionado.";
                return RedirectToAction("Usuarios");
            }

            model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
            model.Sucursales = _repo.ObtenerSucursalesSelectList(model.IdEmpresa, model.IdSucursalUser);
            model.EsEdicion = id > 0;

            ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/EditarUsuario.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarUsuario(SystemAdministrationUsuarioEditVm model)
        {
            ValidarUsuario(model);

            if (!ModelState.IsValid)
            {
                model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
                model.Sucursales = _repo.ObtenerSucursalesSelectList(model.IdEmpresa, model.IdSucursalUser);
                model.EsEdicion = model.Id > 0;
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/EditarUsuario.cshtml", model);
            }

            try
            {
                if (model.Id > 0)
                {
                    _repo.ActualizarUsuario(model);
                    TempData["AlertMsg"] = "El usuario se actualizo correctamente.";
                }
                else
                {
                    _repo.CrearUsuario(model);
                    TempData["AlertMsg"] = "El usuario se creo correctamente.";
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Usuarios";
                return RedirectToAction("Usuarios", new { idEmpresa = model.IdEmpresa });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
                model.Sucursales = _repo.ObtenerSucursalesSelectList(model.IdEmpresa, model.IdSucursalUser);
                model.EsEdicion = model.Id > 0;
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/EditarUsuario.cshtml", model);
            }
        }

        [HttpGet]
        public ActionResult AltaRapidaEmpresa()
        {
            var model = new SystemAdministrationAltaRapidaVm
            {
                TieneTelefonoSucursal = _repo.TablaSucursalTieneTelefono(),
                TieneActivaSucursal = _repo.TablaSucursalTieneActiva()
            };

            ViewBag.Title = "Alta rapida de empresa";
            ViewBag.Seccion = "Administracion del sistema";
            return View("~/Views/SystemAdministration/AltaRapidaEmpresa.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarAltaRapidaEmpresa(SystemAdministrationAltaRapidaVm model)
        {
            model = model ?? new SystemAdministrationAltaRapidaVm();
            model.TieneTelefonoSucursal = _repo.TablaSucursalTieneTelefono();
            model.TieneActivaSucursal = _repo.TablaSucursalTieneActiva();

            ValidarEmpresa(model.Empresa);
            ValidarSucursal(model.Sucursal, true);
            ValidarUsuario(model.Usuario, true);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = "Alta rapida de empresa";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/AltaRapidaEmpresa.cshtml", model);
            }

            try
            {
                _repo.CrearAltaRapida(model);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Alta rapida";
                TempData["AlertMsg"] = "La empresa, la sucursal inicial y el usuario administrador se crearon correctamente.";
                return RedirectToAction("Empresas");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewBag.Title = "Alta rapida de empresa";
                ViewBag.Seccion = "Administracion del sistema";
                return View("~/Views/SystemAdministration/AltaRapidaEmpresa.cshtml", model);
            }
        }

        [HttpGet]
        public JsonResult SucursalesPorEmpresa(int idEmpresa)
        {
            var items = _repo.ObtenerSucursalesSelectList(idEmpresa)
                .Select(x => new
                {
                    value = x.Value,
                    text = x.Text
                })
                .ToList();

            return Json(items, JsonRequestBehavior.AllowGet);
        }

        private void ValidarEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "No se recibieron datos de la empresa.");
                return;
            }

            model.RazonSocialAfip = (model.RazonSocialAfip ?? "").Trim();
            model.Cuit = (model.Cuit ?? "").Trim();
            model.CondicionIVA = (model.CondicionIVA ?? "").Trim();
            model.Email = (model.Email ?? "").Trim();

            long cuit;
            if (!long.TryParse(model.Cuit, out cuit) || cuit <= 0)
                ModelState.AddModelError("Cuit", "El CUIT no es valido.");
            else if (_repo.ExisteCuit(cuit, model.IdEmpresa))
                ModelState.AddModelError("Cuit", "Ya existe una empresa con ese CUIT.");

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var usuarioValidador = new Entidades.Usuario();
                if (!usuarioValidador.EsEmailValido(model.Email))
                    ModelState.AddModelError("Email", "El email no es valido.");
            }
        }

        private void ValidarSucursal(SystemAdministrationSucursalEditVm model, bool permitirEmpresaPendiente = false)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "No se recibieron datos de la sucursal.");
                return;
            }

            model.Sucursal = (model.Sucursal ?? "").Trim();
            if (!permitirEmpresaPendiente && model.IdEmpresa <= 0)
                ModelState.AddModelError("IdEmpresa", "Debe seleccionar una empresa.");
        }

        private void ValidarUsuario(SystemAdministrationUsuarioEditVm model, bool permitirEmpresaPendiente = false)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "No se recibieron datos del usuario.");
                return;
            }

            model.Nombre = (model.Nombre ?? "").Trim();
            model.Usuario = (model.Usuario ?? "").Trim();
            model.Email = (model.Email ?? "").Trim();
            model.Clave = model.Clave ?? "";
            model.ConfirmarClave = model.ConfirmarClave ?? "";

            if (!permitirEmpresaPendiente && model.IdEmpresa <= 0)
                ModelState.AddModelError("IdEmpresa", "Debe seleccionar una empresa.");

            if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Clave))
                ModelState.AddModelError("Clave", "La clave es obligatoria para un usuario nuevo.");

            if (!string.IsNullOrWhiteSpace(model.Clave) && !string.Equals(model.Clave, model.ConfirmarClave, StringComparison.Ordinal))
                ModelState.AddModelError("ConfirmarClave", "La confirmacion de clave no coincide.");

            if (_repo.ExisteUsuario(model.Usuario, model.Id))
                ModelState.AddModelError("Usuario", "Ya existe un usuario con ese nombre de acceso.");

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var usuarioValidador = new Entidades.Usuario();
                if (!usuarioValidador.EsEmailValido(model.Email))
                    ModelState.AddModelError("Email", "El email no es valido.");
                else if (_repo.ExisteEmail(model.Email, model.Id))
                    ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
            }
        }
    }
}
