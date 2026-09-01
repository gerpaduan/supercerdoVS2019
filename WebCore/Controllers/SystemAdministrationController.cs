// Port de Web/Controllers/SystemAdministrationController.cs (ver docs/DECISIONS.md, migracion
// ASP.NET Core) -- Modulo 1 completo: Empresas, Sucursales, Usuarios y Alta rapida. Deliberadamente
// NO reproducido (igual criterio que AuditoriaLoginController): el gate
// SystemAdministrationAccessHelper.PuedeAdministrarSistema (depende de Session["Usuario"], que
// WebCore todavia no tiene) -- el original tambien puebla ViewBag.AlicuotasIva/CondicionesIva en
// OnActionExecuting para todo el controller; aca se puebla accion por accion donde se necesita
// (EditarEmpresa/GuardarEmpresa/AltaRapidaEmpresa/GuardarAltaRapidaEmpresa), mismo resultado.
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using WebCore.Helpers;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class SystemAdministrationController : Controller
    {
        private readonly SystemAdministrationRepository _repo = new SystemAdministrationRepository();

        public IActionResult Empresas()
        {
            var model = new SystemAdministrationEmpresaIndexVm
            {
                Items = _repo.ObtenerEmpresas()
            };

            ViewBag.Title = "Empresas";
            return View(model);
        }

        [HttpGet]
        public IActionResult EditarEmpresa(int id = 0)
        {
            var model = id > 0
                ? _repo.ObtenerEmpresa(id)
                : new SystemAdministrationEmpresaEditVm { Activa = true };

            if (model == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Empresas";
                TempData["AlertMsg"] = "No se encontro la empresa seleccionada.";
                return RedirectToAction("Empresas");
            }

            model.EsEdicion = id > 0;
            ViewBag.Title = model.EsEdicion ? "Modificar empresa" : "Nueva empresa";
            ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
            ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            ValidarEmpresa(model);

            if (!ModelState.IsValid)
            {
                ViewBag.Title = model.IdEmpresa > 0 ? "Modificar empresa" : "Nueva empresa";
                model.EsEdicion = model.IdEmpresa > 0;
                ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
                ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
                return View("EditarEmpresa", model);
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
                model.EsEdicion = model.IdEmpresa > 0;
                ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
                ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
                return View("EditarEmpresa", model);
            }
        }

        public IActionResult Sucursales(int idEmpresa = 0)
        {
            var model = new SystemAdministrationSucursalIndexVm
            {
                FiltroEmpresaId = idEmpresa,
                Empresas = _repo.ObtenerEmpresasSelectList(idEmpresa, true),
                Items = _repo.ObtenerSucursales(0),
                TieneTelefono = _repo.TablaSucursalTieneTelefono(),
                TieneActiva = _repo.TablaSucursalTieneActiva()
            };

            ViewBag.Title = "Sucursales";
            return View(model);
        }

        [HttpGet]
        public IActionResult EditarSucursal(int id = 0, int idEmpresa = 0)
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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarSucursal(SystemAdministrationSucursalEditVm model)
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
                return View("EditarSucursal", model);
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
                return View("EditarSucursal", model);
            }
        }

        public IActionResult Usuarios(int idEmpresa = 0)
        {
            var model = new SystemAdministrationUsuarioIndexVm
            {
                FiltroEmpresaId = idEmpresa,
                Empresas = _repo.ObtenerEmpresasSelectList(idEmpresa, true),
                Items = _repo.ObtenerUsuarios(0)
            };

            ViewBag.Title = "Usuarios";
            return View(model);
        }

        [HttpGet]
        public IActionResult EditarUsuario(int id = 0, int idEmpresa = 0)
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
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarUsuario(SystemAdministrationUsuarioEditVm model)
        {
            ValidarUsuario(model);

            if (!ModelState.IsValid)
            {
                model.Empresas = _repo.ObtenerEmpresasSelectList(model.IdEmpresa);
                model.Sucursales = _repo.ObtenerSucursalesSelectList(model.IdEmpresa, model.IdSucursalUser);
                model.EsEdicion = model.Id > 0;
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                return View("EditarUsuario", model);
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
                return View("EditarUsuario", model);
            }
        }

        [HttpGet]
        public IActionResult AltaRapidaEmpresa()
        {
            var model = new SystemAdministrationAltaRapidaVm
            {
                TieneTelefonoSucursal = _repo.TablaSucursalTieneTelefono(),
                TieneActivaSucursal = _repo.TablaSucursalTieneActiva()
            };

            ViewBag.Title = "Alta rapida de empresa";
            ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
            ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarAltaRapidaEmpresa(SystemAdministrationAltaRapidaVm model)
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
                ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
                ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
                return View("AltaRapidaEmpresa", model);
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
                ViewBag.AlicuotasIva = _repo.ObtenerAlicuotasIva();
                ViewBag.CondicionesIva = _repo.ObtenerCondicionesIva();
                return View("AltaRapidaEmpresa", model);
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

            return Json(items);
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
    }
}
