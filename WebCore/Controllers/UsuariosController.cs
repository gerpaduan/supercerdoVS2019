// Port de Web/Controllers/UsuariosController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Gestion de usuarios y permisos de la empresa actual
// (self-service, distinto de SystemAdministrationController.Usuarios, cross-tenant para el
// super-admin de plataforma, ya portado en Modulo 1).
//
// Mismo criterio de stub que el resto del modulo (Id=2, Admin=true, IdEmpresa=1, IdSucursal=2,
// Nombre="ger"). Con Admin=true, TienePermisoUsuarios/PuedeVerUsuarios/PuedeAdministrarUsuarios
// siempre dan true (el chequeo real de negocio ya contempla el bypass de Admin, no es una
// omision). ObtenerUsuarioActualConPermisos() se reemplaza por el stub directo -- el original
// refresca el usuario de Session con sus Permisos si hace falta, pero con Admin=true el bypass
// de permisos nunca llega a mirar la lista de Permisos, asi que no hace falta portar ese refresh.
using Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class UsuariosController : Controller
    {
        // idForm del formulario "Ventas" (formConsulta='formVentas', formEdicion='formNuevaVenta')
        // -- mismo hardcodeo que el original (no hay tabla de mapeo clave->idform en el proyecto).
        private const int IdFormVentas = 7;

        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Sucursal _oSucursalN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public UsuariosController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oUsuarioN = new Negocio.Usuario(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
        }

        public IActionResult Index()
        {
            _oUsuarioN.obtenerUsuarios(false);
            var usuarios = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(u => u != null && u.IdEmpresa == _empresa.IdEmpresa)
                .OrderBy(u => u.Nombre ?? "")
                .ThenBy(u => u.User ?? "")
                .ToList();

            var model = new UsuarioIndexVm
            {
                PuedeAdministrar = PuedeAdministrarUsuarios(_usuarioActual),
                Items = usuarios.Select(MapResumen).ToList()
            };

            ViewBag.Title = "Usuarios";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Editar(int id = 0)
        {
            bool puedeAdministrar = PuedeAdministrarUsuarios(_usuarioActual);
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
                IdEmpresa = _empresa.IdEmpresa,
                IdSucursalUser = _usuarioActual.IdSucursal
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
                model.PermitirLoginFueraSucursal = usuario.PermitirLoginFueraSucursal;
                model.EsUsuarioProduccion = usuario.EsUsuarioProduccion;
                model.IdEmpresa = usuario.IdEmpresa;
                model.PuedeOperarPOS = (_oUsuarioN.getPermisosUsuario(usuario.Id) ?? new List<PermisosUsuarios>())
                    .Any(p => p.IdForm == IdFormVentas && p.DiasPermitidosEditar >= 0);
            }

            CargarSucursales(model);
            ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(UsuarioEditVm model)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para guardar usuarios.";
                return RedirectToAction("Index");
            }

            Entidades.Usuario? usuarioOriginal = null;
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
                model.IdEmpresa = _empresa.IdEmpresa;
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
                PermitirLoginFueraSucursal = model.PermitirLoginFueraSucursal,
                EsUsuarioProduccion = model.EsUsuarioProduccion,
                IdEmpresa = _empresa.IdEmpresa
            };

            try
            {
                _oUsuarioN.addOrEditUser(usuarioGuardar);

                int idUsuarioPersistido = usuarioGuardar.Id;
                if (idUsuarioPersistido <= 0)
                {
                    var usuarioBusqueda = new Negocio.Usuario(_empresa, _param);
                    usuarioBusqueda.obtenerUsuarios(false);
                    idUsuarioPersistido = (usuarioBusqueda.listaUsuario() ?? new List<Entidades.Usuario>())
                        .Where(u => u != null && u.IdEmpresa == _empresa.IdEmpresa)
                        .Where(u => string.Equals(u.User ?? "", usuarioGuardar.User ?? "", StringComparison.OrdinalIgnoreCase))
                        .Select(u => u.Id)
                        .FirstOrDefault();
                }

                if (idUsuarioPersistido > 0)
                {
                    if (!string.IsNullOrWhiteSpace(model.Clave))
                    {
                        _oUsuarioN.ActualizarPasswordWebSeguro(idUsuarioPersistido, model.Clave.Trim());
                    }

                    _oUsuarioN.setSucursalUsuario(new Entidades.Usuario
                    {
                        Id = idUsuarioPersistido,
                        IdSucursal = model.IdSucursalUser
                    });

                    _oUsuarioN.setPermitirLoginFueraSucursal(new Entidades.Usuario
                    {
                        Id = idUsuarioPersistido,
                        PermitirLoginFueraSucursal = model.PermitirLoginFueraSucursal
                    });

                    _oUsuarioN.setEsUsuarioProduccion(new Entidades.Usuario
                    {
                        Id = idUsuarioPersistido,
                        EsUsuarioProduccion = model.EsUsuarioProduccion
                    });

                    AplicarPuedeOperarPOS(idUsuarioPersistido, model.EsUsuarioProduccion, model.PuedeOperarPOS);

                    // TODO(claude): el original refresca Session["Usuario"] si el usuario editado
                    // es el mismo que esta logueado -- sin sesion real en WebCore, no aplica.
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
                model.IdEmpresa = _empresa.IdEmpresa;
                CargarSucursales(model);
                ViewBag.Title = model.EsEdicion ? "Modificar usuario" : "Nuevo usuario";
                ViewBag.Seccion = "Usuarios";
                return View("~/Views/Usuarios/Editar.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult Permisos(int id)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
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

            var permisos = (_oUsuarioN.getPermisosUsuario(id) ?? new List<PermisosUsuarios>())
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
        public IActionResult GuardarPermisos(UsuarioPermisosVm model)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
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
                const int idFormCierresDeCaja = 9;

                HashSet<int>? idFormsBloqueados = null;
                if (usuario.EsUsuarioProduccion)
                {
                    idFormsBloqueados = (_oUsuarioN.getPermisosUsuario(usuario.Id) ?? new List<PermisosUsuarios>())
                        .Where(p => p.Formulario != null && EsFormularioBloqueadoParaProduccion(p.Formulario))
                        .Select(p => p.IdForm)
                        .ToHashSet();
                }

                var permisos = (model.Items ?? new List<UsuarioPermisoItemVm>())
                    .GroupBy(x => x.IdForm)
                    .Select(g => g.First())
                    .Select(x =>
                    {
                        if (idFormsBloqueados != null && idFormsBloqueados.Contains(x.IdForm))
                        {
                            x.PuedeVer = false;
                            x.PuedeEditar = false;
                        }

                        bool puedeEditar = x.PuedeEditar || (x.IdForm == idFormCierresDeCaja && x.PuedeVer);
                        int diasEditar = x.IdForm == idFormCierresDeCaja && x.PuedeVer ? x.DiasVer : x.DiasEditar;

                        return new PermisosUsuarios
                        {
                            IdUsuario = usuario.Id,
                            IdForm = x.IdForm,
                            DiasPermitidosVer = x.PuedeVer ? Math.Max(0, x.DiasVer) : -1,
                            DiasPermitidosEditar = puedeEditar ? Math.Max(0, diasEditar) : -1,
                            SoloRegistrosPropios = puedeEditar ? x.SoloRegistrosPropios : true
                        };
                    })
                    .ToList();

                _oUsuarioN.AddOrEditPermisos(permisos);

                // TODO(claude): el original refresca Session["Usuario"] si corresponde -- sin
                // sesion real en WebCore, no aplica.

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DesbloquearUsuario(int id)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para desbloquear usuarios.";
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

            _oUsuarioN.DesbloquearUsuario(usuario.Id);

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Usuarios";
            TempData["AlertMsg"] = "La cuenta se desbloqueó correctamente.";
            return RedirectToAction("Index");
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

            return _oUsuarioN.tienePermiso(
                usuarioActual,
                permiso,
                DateTime.Today,
                validarEdicion ? usuarioActual.Id : -1
            );
        }

        private Entidades.Usuario? ObtenerUsuarioSeguro(int id)
        {
            var usuario = _oUsuarioN.getUsuarioById(id);
            if (usuario == null) return null;
            return usuario.IdEmpresa == _empresa.IdEmpresa ? usuario : null;
        }

        private void CargarSucursales(UsuarioEditVm model)
        {
            var sucursales = _oSucursalN.findAll() ?? new List<Sucursal>();
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

        private static readonly HashSet<string> ClavesBloqueadasUsuarioProduccion = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            Entidades.Permisos.Venta.Bonificar, Entidades.Permisos.Venta.VerVentas, Entidades.Permisos.Venta.VerBonificar,
            Entidades.Permisos.Venta.VerVentasVendedor, Entidades.Permisos.Venta.VerGetAllLineaVenta, Entidades.Permisos.Venta.NuevaVenta,
            Entidades.Permisos.Venta.UltimaVenta, Entidades.Permisos.Venta.GetAllLineaVenta,
            Entidades.Permisos.Finanza.VerCheques, Entidades.Permisos.Finanza.VerPagos, Entidades.Permisos.Finanza.AddOrEditPago,
            Entidades.Permisos.Finanza.VerCtasCtes, Entidades.Permisos.Finanza.VerCtaCtePersona,
            Entidades.Permisos.Elaborado.VerFormulas, Entidades.Permisos.Elaborado.IngresoFormula
        };

        private static bool EsFormularioBloqueadoParaProduccion(Entidades.Formulario formulario)
        {
            return ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormConsulta ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicion ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicionExtra1 ?? "")
                || ClavesBloqueadasUsuarioProduccion.Contains(formulario.FormEdicionExtra2 ?? "");
        }

        private void AplicarPuedeOperarPOS(int idUsuario, bool esUsuarioProduccion, bool puedeOperarPOS)
        {
            if (esUsuarioProduccion)
                puedeOperarPOS = false;

            var permisoActual = (_oUsuarioN.getPermisosUsuario(idUsuario) ?? new List<PermisosUsuarios>())
                .FirstOrDefault(p => p.IdForm == IdFormVentas);

            bool yaOtorgado = permisoActual != null && permisoActual.DiasPermitidosEditar >= 0;

            if (puedeOperarPOS == yaOtorgado)
                return;

            _oUsuarioN.AddOrEditPermisos(new List<PermisosUsuarios>
            {
                new PermisosUsuarios
                {
                    IdUsuario = idUsuario,
                    IdForm = IdFormVentas,
                    DiasPermitidosVer = permisoActual != null ? permisoActual.DiasPermitidosVer : -1,
                    DiasPermitidosEditar = puedeOperarPOS ? 0 : -1,
                    SoloRegistrosPropios = permisoActual != null ? permisoActual.SoloRegistrosPropios : true
                }
            });
        }

        private void ValidarUsuario(UsuarioEditVm model, Entidades.Usuario? usuarioOriginal)
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

            var sucursalValida = model.IdSucursalUser <= 0 || (_oSucursalN.findAll() ?? new List<Sucursal>()).Any(s => s.IdSucursal == model.IdSucursalUser);
            if (!sucursalValida)
                ModelState.AddModelError("IdSucursalUser", "La sucursal seleccionada no es válida.");

            if (model.EsUsuarioProduccion && model.Admin)
                ModelState.AddModelError("", "Un usuario de producción no puede ser Admin.");

            _oUsuarioN.obtenerUsuarios(false);
            var usuarios = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(u => u != null && u.IdEmpresa == _empresa.IdEmpresa)
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
                IdEmpresa = usuario.IdEmpresa,
                Bloqueado = usuario.Bloqueado
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
