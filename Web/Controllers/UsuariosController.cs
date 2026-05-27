using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class UsuariosController : BaseController
    {
        private Negocio.Usuario oUsuarioN;
        private Negocio.Sucursal oSucursalN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oUsuarioN = new Negocio.Usuario(empresa, param);
            oSucursalN = new Negocio.Sucursal(empresa, param);
        }

        public ActionResult Index()
        {
            var usuarioActual = ObtenerUsuarioActualConPermisos();
            if (usuarioActual == null || usuarioActual.IdEmpresa != empresa.IdEmpresa)
            {
                ViewBag.Title = "Usuarios";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!PuedeVerUsuarios(usuarioActual))
            {
                ViewBag.Title = "Usuarios";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            oUsuarioN.obtenerUsuarios(false);
            var usuarios = (oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(u => u != null && u.IdEmpresa == empresa.IdEmpresa)
                .OrderBy(u => u.Nombre ?? "")
                .ThenBy(u => u.User ?? "")
                .ToList();

            var model = new UsuarioIndexVm
            {
                PuedeAdministrar = PuedeAdministrarUsuarios(usuarioActual),
                Items = usuarios.Select(MapResumen).ToList()
            };

            ViewBag.Title = "Usuarios";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult Editar(int id = 0)
        {
            var usuarioActual = ObtenerUsuarioActualConPermisos();
            bool puedeAdministrar = PuedeAdministrarUsuarios(usuarioActual);

            if (usuarioActual == null || usuarioActual.IdEmpresa != empresa.IdEmpresa)
            {
                ViewBag.Title = "Usuarios";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!PuedeVerUsuarios(usuarioActual))
            {
                ViewBag.Title = "Usuarios";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!puedeAdministrar)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = id > 0
                    ? "No tiene permisos para modificar usuarios."
                    : "No tiene permisos para crear usuarios.";
                return RedirectToAction("Index");
            }

            var model = new UsuarioEditVm
            {
                EsEdicion = id > 0,
                SoloLectura = false,
                IdEmpresa = empresa.IdEmpresa,
                IdSucursalUser = usuarioActual != null ? usuarioActual.IdSucursal : 0
            };

            if (id > 0)
            {
                var usuario = ObtenerUsuarioSeguro(id);
                if (usuario == null)
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertTitle"] = "No encontrado";
                    TempData["AlertMsg"] = "No se encontró el usuario seleccionado.";
                    return RedirectToAction("Index");
                }

                model.Id = usuario.Id;
                model.Nombre = usuario.Nombre ?? "";
                model.Usuario = usuario.User ?? "";
                model.Admin = usuario.Admin;
                model.Activo = usuario.Activo;
                model.Email = usuario.Email ?? "";
                model.IdSucursalUser = usuario.IdSucursal;
                model.IdEmpresa = usuario.IdEmpresa;
            }

            CargarSucursales(model);
            ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Guardar(UsuarioEditVm model)
        {
            var usuarioActual = ObtenerUsuarioActualConPermisos();
            if (!PuedeAdministrarUsuarios(usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para guardar usuarios.";
                return RedirectToAction("Index");
            }

            Entidades.Usuario usuarioOriginal = null;
            if (model.Id > 0)
            {
                usuarioOriginal = ObtenerUsuarioSeguro(model.Id);
                if (usuarioOriginal == null)
                {
                    TempData["AlertType"] = "error";
                    TempData["AlertTitle"] = "No encontrado";
                    TempData["AlertMsg"] = "No se encontró el usuario a modificar.";
                    return RedirectToAction("Index");
                }
            }

            ValidarUsuario(model, usuarioOriginal);

            if (!ModelState.IsValid)
            {
                model.EsEdicion = model.Id > 0;
                model.SoloLectura = false;
                model.IdEmpresa = empresa.IdEmpresa;
                CargarSucursales(model);
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Usuarios/Editar.cshtml", model);
            }

            string claveFinal = usuarioOriginal != null
                ? (usuarioOriginal.Clave ?? "")
                : (!string.IsNullOrWhiteSpace(model.Clave) ? model.Clave.Trim() : "");

            var usuarioGuardar = new Entidades.Usuario
            {
                Id = model.Id,
                Nombre = (model.Nombre ?? "").Trim(),
                User = (model.Usuario ?? "").Trim(),
                Clave = claveFinal,
                Admin = model.Admin,
                Activo = model.Activo,
                Email = (model.Email ?? "").Trim(),
                ColorForm = usuarioOriginal != null && !string.IsNullOrWhiteSpace(usuarioOriginal.ColorForm)
                    ? usuarioOriginal.ColorForm
                    : "SteelBlue",
                IdSucursal = model.IdSucursalUser,
                IdEmpresa = empresa.IdEmpresa
            };

            try
            {
                oUsuarioN.addOrEditUser(usuarioGuardar);

                int idUsuarioPersistido = usuarioGuardar.Id;
                if (idUsuarioPersistido <= 0)
                {
                    var usuarioBusqueda = new Negocio.Usuario(empresa, param);
                    usuarioBusqueda.obtenerUsuarios(false);
                    idUsuarioPersistido = (usuarioBusqueda.listaUsuario() ?? new List<Entidades.Usuario>())
                        .Where(u => u != null && u.IdEmpresa == empresa.IdEmpresa)
                        .Where(u => string.Equals(u.User ?? "", usuarioGuardar.User ?? "", StringComparison.OrdinalIgnoreCase))
                        .Select(u => u.Id)
                        .FirstOrDefault();
                }

                if (idUsuarioPersistido > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.Clave))
                    {
                        oUsuarioN.ActualizarPasswordWebSeguro(idUsuarioPersistido, model.Clave.Trim());
                    }

                    oUsuarioN.setSucursalUsuario(new Entidades.Usuario
                    {
                        Id = idUsuarioPersistido,
                        IdSucursal = model.IdSucursalUser
                    });

                    if (usuarioActual != null && usuarioActual.Id == idUsuarioPersistido)
                    {
                        var refrescado = oUsuarioN.getUsuarioById(idUsuarioPersistido);
                        if (refrescado != null)
                        {
                            refrescado.Permisos = oUsuarioN.getPermisosUsuario(refrescado.Id);
                            refrescado.SucursalNombre = refrescado.Sucursal == null ? "Seleccione Sucursal" : (refrescado.Sucursal.SucursalNombre ?? "");
                            Session["Usuario"] = refrescado;
                        }
                    }
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Usuarios";
                TempData["AlertMsg"] = model.Id > 0
                    ? "El usuario se actualizó correctamente."
                    : "El usuario se creó correctamente.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.EsEdicion = model.Id > 0;
                model.SoloLectura = false;
                model.IdEmpresa = empresa.IdEmpresa;
                CargarSucursales(model);
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Usuarios/Editar.cshtml", model);
            }
        }

        [HttpGet]
        public ActionResult Permisos(int id)
        {
            var usuarioActual = ObtenerUsuarioActualConPermisos();
            bool puedeAdministrar = PuedeAdministrarUsuarios(usuarioActual);

            if (usuarioActual == null || usuarioActual.IdEmpresa != empresa.IdEmpresa)
            {
                ViewBag.Title = "Permisos";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!PuedeVerUsuarios(usuarioActual))
            {
                ViewBag.Title = "Permisos";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            if (!puedeAdministrar)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para ver o editar permisos de usuarios.";
                return RedirectToAction("Index");
            }

            var usuario = ObtenerUsuarioSeguro(id);
            if (usuario == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "No encontrado";
                TempData["AlertMsg"] = "No se encontró el usuario seleccionado.";
                return RedirectToAction("Index");
            }

            var permisos = (oUsuarioN.getPermisosUsuario(id) ?? new List<PermisosUsuarios>())
                .GroupBy(p => p.IdForm)
                .Select(g => g.First())
                .OrderBy(p => p.Formulario != null ? p.Formulario.NombreForm : "")
                .ToList();

            var model = new UsuarioPermisosVm
            {
                IdUsuario = usuario.Id,
                UsuarioNombre = usuario.Nombre ?? "",
                UsuarioLogin = usuario.User ?? "",
                SoloLectura = false,
                Items = permisos.Select(MapPermiso).ToList()
            };

            ViewBag.Title = "Permisos de usuario";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Permisos.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GuardarPermisos(UsuarioPermisosVm model)
        {
            var usuarioActual = ObtenerUsuarioActualConPermisos();
            if (!PuedeAdministrarUsuarios(usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para guardar permisos de usuarios.";
                return RedirectToAction("Index");
            }

            var usuario = ObtenerUsuarioSeguro(model.IdUsuario);
            if (usuario == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "No encontrado";
                TempData["AlertMsg"] = "No se encontró el usuario seleccionado.";
                return RedirectToAction("Index");
            }

            try
            {
                var permisos = (model.Items ?? new List<UsuarioPermisoItemVm>())
                    .GroupBy(x => x.IdForm)
                    .Select(g => g.First())
                    .Select(x => new PermisosUsuarios
                    {
                        IdUsuario = usuario.Id,
                        IdForm = x.IdForm,
                        DiasPermitidosVer = x.PuedeVer ? Math.Max(0, x.DiasVer) : -1,
                        DiasPermitidosEditar = x.PuedeEditar ? Math.Max(0, x.DiasEditar) : -1,
                        SoloRegistrosPropios = x.PuedeEditar ? x.SoloRegistrosPropios : true
                    })
                    .ToList();

                oUsuarioN.AddOrEditPermisos(permisos);

                if (usuarioActual != null && usuarioActual.Id == usuario.Id)
                {
                    var refrescado = oUsuarioN.getUsuarioById(usuario.Id);
                    if (refrescado != null)
                    {
                        refrescado.Permisos = oUsuarioN.getPermisosUsuario(refrescado.Id);
                        refrescado.SucursalNombre = refrescado.Sucursal == null ? "Seleccione Sucursal" : (refrescado.Sucursal.SucursalNombre ?? "");
                        Session["Usuario"] = refrescado;
                    }
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Permisos";
                TempData["AlertMsg"] = "Los permisos se guardaron correctamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                model.UsuarioNombre = usuario.Nombre ?? "";
                model.UsuarioLogin = usuario.User ?? "";
                model.SoloLectura = false;
                ViewBag.Title = "Permisos de usuario";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Usuarios/Permisos.cshtml", model);
            }
        }

        private bool PuedeVerUsuarios(Entidades.Usuario usuarioActual)
        {
            return TienePermisoUsuarios(usuarioActual, Entidades.Permisos.Usuario.VerUsuarios, false);
        }

        private bool PuedeAdministrarUsuarios(Entidades.Usuario usuarioActual)
        {
            return TienePermisoUsuarios(usuarioActual, Entidades.Permisos.Usuario.NuevoUsuario, true);
        }

        private bool TienePermisoUsuarios(Entidades.Usuario usuarioActual, string permiso, bool validarEdicion)
        {
            if (usuarioActual == null)
                return false;

            if (usuarioActual.Admin)
                return true;

            if (usuarioActual.Permisos == null || usuarioActual.Permisos.Count == 0)
                return false;

            return oUsuarioN.tienePermiso(
                usuarioActual,
                permiso,
                DateTime.Today,
                validarEdicion ? usuarioActual.Id : -1
            );
        }

        private Entidades.Usuario ObtenerUsuarioActualConPermisos()
        {
            var usuarioActual = Session["Usuario"] as Entidades.Usuario;
            if (usuarioActual == null)
                return null;

            if (usuarioActual.Admin && usuarioActual.Permisos != null && usuarioActual.Permisos.Count > 0)
                return usuarioActual;

            if (usuarioActual.Permisos != null && usuarioActual.Permisos.Count > 0)
                return usuarioActual;

            var refrescado = oUsuarioN.getUsuarioById(usuarioActual.Id);
            if (refrescado == null || refrescado.IdEmpresa != empresa.IdEmpresa)
                return usuarioActual;

            refrescado.Permisos = oUsuarioN.getPermisosUsuario(refrescado.Id);
            refrescado.SucursalNombre = refrescado.Sucursal == null ? "Seleccione Sucursal" : (refrescado.Sucursal.SucursalNombre ?? "");
            Session["Usuario"] = refrescado;
            return refrescado;
        }

        private Entidades.Usuario ObtenerUsuarioSeguro(int id)
        {
            var usuario = oUsuarioN.getUsuarioById(id);
            if (usuario == null) return null;
            return usuario.IdEmpresa == empresa.IdEmpresa ? usuario : null;
        }

        private void CargarSucursales(UsuarioEditVm model)
        {
            var sucursales = oSucursalN.findAll() ?? new List<Sucursal>();
            model.Sucursales = sucursales
                .OrderBy(s => s.SucursalNombre ?? "")
                .Select(s => new SelectListItem
                {
                    Value = s.IdSucursal.ToString(),
                    Text = s.SucursalNombre,
                    Selected = s.IdSucursal == model.IdSucursalUser
                })
                .ToList();
        }

        private void ValidarUsuario(UsuarioEditVm model, Entidades.Usuario usuarioOriginal)
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

            if (string.IsNullOrWhiteSpace(model.Nombre))
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");

            if (string.IsNullOrWhiteSpace(model.Usuario))
                ModelState.AddModelError("Usuario", "El usuario es obligatorio.");

            if (model.Id == 0 && string.IsNullOrWhiteSpace(model.Clave))
                ModelState.AddModelError("Clave", "La clave es obligatoria para un usuario nuevo.");

            if (model.Clave.Contains(" "))
                ModelState.AddModelError("Clave", "La clave no puede contener espacios en blanco.");

            if (!string.IsNullOrWhiteSpace(model.Clave))
            {
                if (model.Clave.Length < 1)
                    ModelState.AddModelError("Clave", "La clave debe tener al menos 1 caracter.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                var usuarioValidador = new Entidades.Usuario();
                if (!usuarioValidador.EsEmailValido(model.Email))
                    ModelState.AddModelError("Email", "El email no es válido.");
            }

            if (usuarioOriginal != null && string.Equals(usuarioOriginal.User ?? "", "admin", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("", "El usuario Admin es reservado para el desarrollador del sistema.");

            var sucursalValida = model.IdSucursalUser <= 0 || (oSucursalN.findAll() ?? new List<Sucursal>()).Any(s => s.IdSucursal == model.IdSucursalUser);
            if (!sucursalValida)
                ModelState.AddModelError("IdSucursalUser", "La sucursal seleccionada no es válida.");

            oUsuarioN.obtenerUsuarios(false);
            var usuarios = (oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(u => u != null && u.IdEmpresa == empresa.IdEmpresa)
                .ToList();

            bool usuarioDuplicado = usuarios.Any(u =>
                u.Id != model.Id &&
                string.Equals(u.User ?? "", model.Usuario ?? "", StringComparison.OrdinalIgnoreCase));

            if (usuarioDuplicado)
                ModelState.AddModelError("Usuario", "Ya existe un usuario con ese nombre de acceso.");

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                bool emailDuplicado = usuarios.Any(u =>
                    u.Id != model.Id &&
                    !string.IsNullOrWhiteSpace(u.Email) &&
                    string.Equals(u.Email ?? "", model.Email ?? "", StringComparison.OrdinalIgnoreCase));

                if (emailDuplicado)
                    ModelState.AddModelError("Email", "Ya existe un usuario con ese email.");
            }
        }

        private static UsuarioResumenVm MapResumen(Entidades.Usuario usuario)
        {
            return new UsuarioResumenVm
            {
                Id = usuario.Id,
                Nombre = usuario.Nombre ?? "",
                Usuario = usuario.User ?? "",
                Admin = usuario.Admin,
                Activo = usuario.Activo,
                Email = usuario.Email ?? "",
                IdSucursalUser = usuario.IdSucursal,
                SucursalNombre = usuario.Sucursal != null ? (usuario.Sucursal.SucursalNombre ?? "") : (usuario.SucursalNombre ?? ""),
                IdEmpresa = usuario.IdEmpresa
            };
        }

        private static UsuarioPermisoItemVm MapPermiso(PermisosUsuarios permiso)
        {
            bool puedeVer = permiso != null && permiso.DiasPermitidosVer >= 0;
            bool puedeEditar = permiso != null && permiso.DiasPermitidosEditar >= 0;

            return new UsuarioPermisoItemVm
            {
                IdForm = permiso != null ? permiso.IdForm : 0,
                NombrePermiso = permiso != null && permiso.Formulario != null ? (permiso.Formulario.NombreForm ?? "") : "",
                Detalle = permiso != null && permiso.Formulario != null ? (permiso.Formulario.Descripcion ?? "") : "",
                PuedeVer = puedeVer,
                DiasVer = puedeVer ? permiso.DiasPermitidosVer : 0,
                PuedeEditar = puedeEditar,
                DiasEditar = puedeEditar ? permiso.DiasPermitidosEditar : 0,
                SoloRegistrosPropios = permiso == null || permiso.SoloRegistrosPropios
            };
        }
    }
}
