// Port de Web/Controllers/UsuariosController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Gestion de usuarios y permisos de la empresa actual
// (self-service, distinto de SystemAdministrationController.Usuarios, cross-tenant para el
// super-admin de plataforma, ya portado en Modulo 1).
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06) -- ya no hay stub hardcodeado. ObtenerUsuarioActualConPermisos() se reemplaza por
// el usuario de sesion directo -- el original refresca el usuario de Session con sus Permisos si
// hace falta, pero IUsuarioSesionService ya re-resuelve Permisos frescos en cada request (ver
// UsuarioSesionService.ResolverUsuarioActual), asi que no hace falta portar ese refresh aparte.
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

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.Sucursal _oSucursalN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public UsuariosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
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

            // El nombre de la sucursal no viene en la fila del usuario: se completa desde el listado de sucursales (una sola consulta).
            var nombresSucursal = NombresDeSucursales();
            foreach (var item in model.Items)
            {
                if (string.IsNullOrWhiteSpace(item.SucursalNombre) && nombresSucursal.TryGetValue(item.IdSucursalUser, out var nombre))
                    item.SucursalNombre = nombre;
            }

            ViewBag.Title = "Usuarios";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Index.cshtml", model);
        }

        // 2026-11-10 (rediseno de Usuarios): la ficha de un usuario (datos, ingreso y seguridad, permisos) y el alta son la pantalla
        // Asistente. Esta URL se conserva para enlaces viejos y redirige; Editar.cshtml queda solo para los errores del POST Guardar.
        [HttpGet]
        public IActionResult Editar(int id = 0)
        {
            return RedirectToAction("Asistente", new { id });
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

            try
            {
                GuardarDatosUsuario(model, usuarioOriginal, true);

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

            // Limpia ambos bloqueos/contadores (seguro y no seguro) y deja constancia en la
            // auditoria de accesos de quien lo hizo (2026-10-02, ver docs/DECISIONS.md "Login por CUIT,
            // clave rapida (PIN) y politica de clave").
            _oUsuarioN.DesbloquearUsuario(usuario.Id);
            RegistrarEventoAuditoria(usuario, "Usuario desbloqueado por administrador (" + (_usuarioActual.User ?? "") + ")");

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Usuarios";
            TempData["AlertMsg"] = "La cuenta se desbloqueó correctamente.";
            return RedirectToAction("Index");
        }

        // Quita la clave rapida (PIN) de un usuario (por ejemplo si la olvido o se sospecha que otro la
        // conoce): el usuario sigue entrando con su contraseña y puede configurar un PIN nuevo.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuitarPin(int id)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para modificar usuarios.";
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

            _oUsuarioN.QuitarPin(usuario.Id);
            RegistrarEventoAuditoria(usuario, "Clave rápida (PIN) eliminada por administrador (" + (_usuarioActual.User ?? "") + ")");

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Usuarios";
            TempData["AlertMsg"] = "Se quitó la clave rápida del usuario.";
            return RedirectToAction("Index");
        }

        // ===== Asistente de usuarios y permisos (2026-11-10, ver docs/DECISIONS.md) =====
        // Alta y permisos en pasos, agrupados por modulo y mostrando solo los controles que tienen efecto real (ver
        // WebCore.Helpers.CatalogoPermisos). Reemplaza a "Crear usuario nuevo" y "Ver permisos" del listado; la grilla vieja
        // (Permisos / GuardarPermisos) queda como "Vista avanzada" y sigue funcionando igual.

        [HttpGet]
        public IActionResult Asistente(int id = 0)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = id > 0
                    ? "No tiene permisos para modificar usuarios."
                    : "No tiene permisos para crear usuarios.";
                return RedirectToAction("Index");
            }

            var model = new UsuarioAsistenteVm
            {
                EsEdicion = id > 0,
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
                model.Email = usuario.Email ?? "";
                model.IdSucursalUser = usuario.IdSucursal;
                model.Activo = usuario.Activo;
                model.PermitirLoginFueraSucursal = usuario.PermitirLoginFueraSucursal;
                model.RequiereDispositivoSeguro = usuario.RequiereDispositivoSeguro;
                model.MinutosInactividad = usuario.MinutosInactividad;
                model.IdEmpresa = usuario.IdEmpresa;
                model.SucursalNombre = usuario.Sucursal != null ? (usuario.Sucursal.SucursalNombre ?? "") : (usuario.SucursalNombre ?? "");
                if (string.IsNullOrWhiteSpace(model.SucursalNombre) && NombresDeSucursales().TryGetValue(usuario.IdSucursal, out var nombreSucursal))
                    model.SucursalNombre = nombreSucursal;
                model.TienePin = usuario.TienePin;
                model.PinActualizadoUtc = usuario.PinUpdatedAtUtc;
                model.Bloqueado = usuario.Bloqueado;
                model.BloqueadoNoSeguro = usuario.BloqueadoNoSeguro;
                model.TipoUsuario = usuario.Admin ? UsuarioAsistenteVm.TipoUsuarioAdmin
                    : usuario.EsUsuarioProduccion ? UsuarioAsistenteVm.TipoUsuarioProduccion
                    : UsuarioAsistenteVm.TipoUsuarioEmpleado;
                // La ficha abre en la pestaña Permisos (un admin no los necesita: abre en Datos). Solo lo usa la vista de un usuario existente.
                model.PasoInicial = usuario.Admin ? 1 : 3;
            }

            CompletarAsistente(model, null);
            ViewBag.Title = model.EsEdicion ? "Permisos y datos del usuario" : "Nuevo usuario";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Asistente.cshtml", model);
        }

        // Valida el paso 1 (datos) contra el servidor antes de pasar al siguiente: mismas reglas que Editar/Guardar.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ValidarDatosAsistente(UsuarioAsistenteVm model)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
                return StatusCode(403, "No tiene permisos para guardar usuarios.");

            var errores = ValidarAsistente(model, out _, out _);
            return Json(new { ok = errores.Count == 0, errores });
        }

        // Permisos de otro usuario de la empresa, ya traducidos al estado del asistente, para el atajo "Copiar de otro usuario".
        // Los permisos "personalizados" (que el asistente no representa) no se copian: se avisa cuantos fueron.
        [HttpGet]
        public IActionResult PermisosParaCopiar(int id)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
                return StatusCode(403, "No tiene permisos para ver permisos de usuarios.");

            var origen = ObtenerUsuarioSeguro(id);
            if (origen == null)
                return NotFound("No se encontró el usuario seleccionado.");

            var estados = new List<WebCore.Helpers.EstadoPermisoAsistente>();
            int omitidos = 0;
            foreach (var fila in FilasDePermisosUnicas(id))
            {
                var definicion = WebCore.Helpers.CatalogoPermisos.Para(fila.Formulario, fila.IdForm);
                if (!string.IsNullOrEmpty(definicion.SinEfectoHoy)) continue;

                var estado = WebCore.Helpers.AsistentePermisosMapper.EstadoDesde(fila, definicion);
                if (estado.Personalizado) { omitidos++; continue; }
                estados.Add(estado);
            }

            return Json(new { nombre = origen.Nombre ?? "", estados, omitidos });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarAsistente(UsuarioAsistenteVm model)
        {
            if (!PuedeAdministrarUsuarios(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para guardar usuarios.";
                return RedirectToAction("Index");
            }

            var errores = ValidarAsistente(model, out var datosUsuario, out var usuarioOriginal);
            if (errores.Count > 0)
                return VolverAlAsistente(model);

            // 1) Datos del usuario (mismo guardado que Editar, sin tocar la fila "Puede operar POS": la define el paso de permisos).
            int idUsuario;
            try
            {
                idUsuario = GuardarDatosUsuario(datosUsuario, usuarioOriginal, false);
            }
            catch (Exception ex)
            {
                HttpContext.RequestServices.GetRequiredService<ILogger<UsuariosController>>()
                    .LogWarning(ex, "No se pudo guardar el usuario desde el asistente (id {IdUsuario}).", model.Id);
                ModelState.AddModelError("", ex.Message);
                return VolverAlAsistente(model);
            }

            if (idUsuario <= 0)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Usuarios";
                TempData["AlertMsg"] = "No se pudo confirmar el usuario guardado. Revisalo en el listado antes de darle permisos.";
                return RedirectToAction("Index");
            }

            // 2) Permisos. Un admin no los necesita (se dejan como estaban). Son dos guardados sin transaccion comun, igual que
            //    Editar + Permisos: si este falla, el usuario ya existe y se vuelve al asistente para reintentar solo los permisos.
            if (model.TipoUsuario != UsuarioAsistenteVm.TipoUsuarioAdmin)
            {
                try
                {
                    GuardarPermisosDelAsistente(idUsuario, model);
                }
                catch (Exception ex)
                {
                    HttpContext.RequestServices.GetRequiredService<ILogger<UsuariosController>>()
                        .LogWarning(ex, "El usuario {IdUsuario} se guardó pero fallaron sus permisos (asistente).", idUsuario);
                    TempData["AlertType"] = "error";
                    TempData["AlertTitle"] = "Permisos";
                    TempData["AlertMsg"] = "El usuario se guardó, pero no se pudieron guardar sus permisos (" + ex.Message + "). Revisalos y volvé a guardar.";
                    return RedirectToAction("Asistente", new { id = idUsuario });
                }
            }

            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Usuarios";
            TempData["AlertMsg"] = model.Id > 0
                ? "El usuario y sus permisos se actualizaron correctamente."
                : "El usuario se creó correctamente.";
            return RedirectToAction("Index");
        }

        // Vuelve a dibujar el asistente con lo que el usuario habia cargado y los errores (arranca en el paso de datos).
        private IActionResult VolverAlAsistente(UsuarioAsistenteVm model)
        {
            model.EsEdicion = model.Id > 0;
            model.IdEmpresa = _empresa.IdEmpresa;
            model.Clave = "";
            model.PasoInicial = 1;
            // La cabecera de la ficha (sucursal, PIN, bloqueo) no se postea: se vuelve a leer del usuario guardado.
            var guardado = model.Id > 0 ? ObtenerUsuarioSeguro(model.Id) : null;
            if (guardado != null)
            {
                model.SucursalNombre = guardado.Sucursal != null ? (guardado.Sucursal.SucursalNombre ?? "") : (guardado.SucursalNombre ?? "");
                if (string.IsNullOrWhiteSpace(model.SucursalNombre) && NombresDeSucursales().TryGetValue(guardado.IdSucursal, out var nombreSucursal))
                    model.SucursalNombre = nombreSucursal;
                model.TienePin = guardado.TienePin;
                model.PinActualizadoUtc = guardado.PinUpdatedAtUtc;
                model.Bloqueado = guardado.Bloqueado;
                model.BloqueadoNoSeguro = guardado.BloqueadoNoSeguro;
            }
            CompletarAsistente(model, model.Permisos);
            ViewBag.Title = model.EsEdicion ? "Permisos y datos del usuario" : "Nuevo usuario";
            ViewBag.Seccion = "Usuarios";
            return View("~/Views/Usuarios/Asistente.cshtml", model);
        }

        // Filas de permisos del usuario (una por formulario). Con id 0 trae todas las filas sin otorgar (usuario nuevo).
        private List<PermisosUsuarios> FilasDePermisosUnicas(int idUsuario)
        {
            return (_oUsuarioN.getPermisosUsuario(idUsuario) ?? new List<PermisosUsuarios>())
                .GroupBy(p => p.IdForm)
                .Select(g => g.First())
                .ToList();
        }

        // Arma lo que la pagina necesita: sucursales, permisos agrupados por modulo (con el estado actual o el posteado si hubo
        // un error al guardar), perfiles y usuarios para copiar.
        private void CompletarAsistente(UsuarioAsistenteVm model, List<WebCore.Helpers.EstadoPermisoAsistente>? posteados)
        {
            var sucursalesAuxiliar = new UsuarioEditVm { IdSucursalUser = model.IdSucursalUser };
            CargarSucursales(sucursalesAuxiliar);
            model.Sucursales = sucursalesAuxiliar.Sucursales;

            var posteadosPorId = (posteados ?? new List<WebCore.Helpers.EstadoPermisoAsistente>())
                .GroupBy(p => p.IdForm)
                .ToDictionary(g => g.Key, g => g.First());

            var items = new List<(WebCore.Helpers.DefinicionPermiso Definicion, AsistentePermisoItemVm Item)>();
            foreach (var fila in FilasDePermisosUnicas(model.Id))
            {
                var definicion = WebCore.Helpers.CatalogoPermisos.Para(fila.Formulario, fila.IdForm);
                // Lo que hoy ningun chequeo mira no se ofrece (la vista avanzada lo sigue mostrando, marcado).
                if (!string.IsNullOrEmpty(definicion.SinEfectoHoy)) continue;

                var estado = posteadosPorId.TryGetValue(fila.IdForm, out var posteado)
                    ? posteado
                    : WebCore.Helpers.AsistentePermisosMapper.EstadoDesde(fila, definicion);

                items.Add((definicion, new AsistentePermisoItemVm
                {
                    IdForm = fila.IdForm,
                    Titulo = definicion.Titulo,
                    Ayuda = definicion.Ayuda,
                    Tipo = definicion.Tipo.ToString(),
                    TextoVer = definicion.TextoVer,
                    TextoModificar = definicion.TextoModificar,
                    Aviso = definicion.Aviso,
                    UsaDiasVer = definicion.UsaDiasVer,
                    UsaDiasEditar = definicion.UsaDiasEditar,
                    UsaAlcance = definicion.UsaAlcance,
                    PosConVer = definicion.PosConVer,
                    PosConModificar = definicion.PosConModificar,
                    RiesgoAlModificar = definicion.RiesgoAlModificar,
                    BloqueadoProduccion = fila.Formulario != null && EsFormularioBloqueadoParaProduccion(fila.Formulario),
                    Estado = estado
                }));
            }

            var ordenModulos = WebCore.Helpers.CatalogoPermisos.OrdenModulos.ToList();
            model.Modulos = items
                .GroupBy(x => x.Definicion.Modulo)
                .OrderBy(g => { int i = ordenModulos.IndexOf(g.Key); return i < 0 ? int.MaxValue : i; })
                .Select(g => new AsistenteModuloVm
                {
                    Nombre = g.Key,
                    Items = g.OrderBy(x => x.Definicion.Orden).ThenBy(x => x.Item.Titulo).Select(x => x.Item).ToList()
                })
                .ToList();

            model.Perfiles = WebCore.Helpers.CatalogoPermisos.Perfiles
                .Select(p => new AsistentePerfilVm
                {
                    Clave = p.Clave,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Estados = p.Estados.ToList()
                })
                .ToList();

            _oUsuarioN.obtenerUsuarios(false);
            model.UsuariosParaCopiar = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(u => u != null && u.IdEmpresa == _empresa.IdEmpresa && u.Activo && !u.Admin && u.Id != model.Id)
                .OrderBy(u => u.Nombre ?? "")
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = (u.Nombre ?? "") + " (" + (u.User ?? "") + ")"
                })
                .ToList();
        }

        // Valida los datos del asistente con las mismas reglas que Editar/Guardar (ValidarUsuario) y devuelve campo -> mensaje.
        // `datosUsuario` es el modelo de Editar ya normalizado (recortado), listo para GuardarDatosUsuario.
        private Dictionary<string, string> ValidarAsistente(UsuarioAsistenteVm model, out UsuarioEditVm datosUsuario, out Entidades.Usuario? usuarioOriginal)
        {
            usuarioOriginal = null;
            ModelState.Clear();

            datosUsuario = new UsuarioEditVm
            {
                Id = model.Id,
                EsEdicion = model.Id > 0,
                Nombre = model.Nombre ?? "",
                Usuario = model.Usuario ?? "",
                Clave = model.Clave ?? "",
                Email = model.Email ?? "",
                IdSucursalUser = model.IdSucursalUser,
                Activo = model.Activo,
                Admin = model.TipoUsuario == UsuarioAsistenteVm.TipoUsuarioAdmin,
                EsUsuarioProduccion = model.TipoUsuario == UsuarioAsistenteVm.TipoUsuarioProduccion,
                PermitirLoginFueraSucursal = model.PermitirLoginFueraSucursal,
                RequiereDispositivoSeguro = model.RequiereDispositivoSeguro,
                MinutosInactividad = model.MinutosInactividad,
                IdEmpresa = _empresa.IdEmpresa
            };

            if (model.Id > 0)
            {
                usuarioOriginal = ObtenerUsuarioSeguro(model.Id);
                if (usuarioOriginal == null)
                    ModelState.AddModelError("", "No se encontró el usuario a modificar.");
            }

            if (model.TipoUsuario != UsuarioAsistenteVm.TipoUsuarioEmpleado
                && model.TipoUsuario != UsuarioAsistenteVm.TipoUsuarioAdmin
                && model.TipoUsuario != UsuarioAsistenteVm.TipoUsuarioProduccion)
                ModelState.AddModelError("TipoUsuario", "Elegí el tipo de usuario.");

            // El rango de minutos lo validaba el binding de UsuarioEditVm ([Range]); aca se hace a mano.
            if (model.MinutosInactividad < 0 || model.MinutosInactividad > 1440)
                ModelState.AddModelError("MinutosInactividad", "Los minutos de inactividad deben estar entre 0 y 1440.");

            if (usuarioOriginal != null || model.Id == 0)
                ValidarUsuario(datosUsuario, usuarioOriginal);

            return ModelState
                .Where(e => e.Value != null && e.Value.Errors.Count > 0)
                .ToDictionary(e => e.Key, e => string.Join(" ", e.Value!.Errors.Select(x => x.ErrorMessage)));
        }

        // Guarda los permisos que muestra el asistente. Solo escribe las filas que el asistente ofrece (las ocultas por no
        // tener efecto quedan como estaban) y fuerza "sin acceso" en las filas que una cuenta de produccion nunca puede tener.
        private void GuardarPermisosDelAsistente(int idUsuario, UsuarioAsistenteVm model)
        {
            bool esProduccion = model.TipoUsuario == UsuarioAsistenteVm.TipoUsuarioProduccion;
            var posteados = (model.Permisos ?? new List<WebCore.Helpers.EstadoPermisoAsistente>())
                .GroupBy(p => p.IdForm)
                .ToDictionary(g => g.Key, g => g.First());

            var aGuardar = new List<PermisosUsuarios>();
            // Se recorre lo que existe en `formularios`: un idform inventado en el formulario no escribe nada.
            foreach (var fila in FilasDePermisosUnicas(idUsuario))
            {
                var definicion = WebCore.Helpers.CatalogoPermisos.Para(fila.Formulario, fila.IdForm);

                if (esProduccion && fila.Formulario != null && EsFormularioBloqueadoParaProduccion(fila.Formulario))
                {
                    aGuardar.Add(new PermisosUsuarios
                    {
                        IdUsuario = idUsuario,
                        IdForm = fila.IdForm,
                        DiasPermitidosVer = -1,
                        DiasPermitidosEditar = -1,
                        SoloRegistrosPropios = true
                    });
                    continue;
                }

                if (!string.IsNullOrEmpty(definicion.SinEfectoHoy)) continue;
                if (!posteados.TryGetValue(fila.IdForm, out var estado)) continue;

                aGuardar.Add(WebCore.Helpers.AsistentePermisosMapper.Aplicar(idUsuario, estado, definicion, fila));
            }

            _oUsuarioN.AddOrEditPermisos(aGuardar);
        }

        // Deja constancia en la auditoria de accesos (loginubicacionlog). Nunca debe impedir la
        // accion del admin: si falla, solo se deja la excepcion en el log.
        private void RegistrarEventoAuditoria(Entidades.Usuario usuario, string motivo)
        {
            try
            {
                _oUsuarioN.RegistrarLoginUbicacion(new Entidades.LoginUbicacionLog
                {
                    IdUsuario = usuario.Id,
                    IdSucursal = usuario.IdSucursal,
                    FechaHora = DateTime.Now,
                    Permitido = true,
                    Motivo = motivo.Length > 300 ? motivo.Substring(0, 300) : motivo,
                    Ip = WebCore.Helpers.ClientIp.Obtener(HttpContext)
                });
            }
            catch (Exception ex)
            {
                HttpContext.RequestServices.GetRequiredService<ILogger<UsuariosController>>()
                    .LogWarning(ex, "No se pudo registrar el evento de auditoria del usuario {IdUsuario}.", usuario.Id);
            }
        }

        // Guarda los datos de un usuario (alta o modificacion) y devuelve su id (0 si no se pudo determinar). Lo comparten
        // Guardar (Editar) y GuardarAsistente. aplicarPuedeOperarPos = false cuando los permisos los define otro paso (asistente).
        // Las excepciones se propagan: cada llamador decide como mostrarlas.
        private int GuardarDatosUsuario(UsuarioEditVm model, Entidades.Usuario? usuarioOriginal, bool aplicarPuedeOperarPos)
        {
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
                RequiereDispositivoSeguro = model.RequiereDispositivoSeguro,
                IdEmpresa = _empresa.IdEmpresa
            };

            _oUsuarioN.addOrEditUser(usuarioGuardar);

            int idUsuarioPersistido = usuarioGuardar.Id;
            if (idUsuarioPersistido <= 0)
            {
                var usuarioBusqueda = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
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

                _oUsuarioN.setRequiereDispositivoSeguro(new Entidades.Usuario
                {
                    Id = idUsuarioPersistido,
                    RequiereDispositivoSeguro = model.RequiereDispositivoSeguro
                });

                _oUsuarioN.setMinutosInactividad(new Entidades.Usuario
                {
                    Id = idUsuarioPersistido,
                    MinutosInactividad = model.MinutosInactividad
                });

                if (aplicarPuedeOperarPos)
                    AplicarPuedeOperarPOS(idUsuarioPersistido, model.EsUsuarioProduccion, model.PuedeOperarPOS);

                // TODO(claude): el original refresca Session["Usuario"] si el usuario editado
                // es el mismo que esta logueado -- sin sesion real en WebCore, no aplica.
            }

            return idUsuarioPersistido;
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

        // idSucursal -> nombre, para mostrar la sucursal de un usuario por su nombre y no por su numero.
        private Dictionary<int, string> NombresDeSucursales()
        {
            return (_oSucursalN.findAll() ?? new List<Sucursal>())
                .GroupBy(s => s.IdSucursal)
                .ToDictionary(g => g.Key, g => g.First().SucursalNombre ?? "");
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

        // La lista de claves bloqueadas para una cuenta de produccion vive en CatalogoPermisos (2026-11-10), compartida con el asistente.
        private static bool EsFormularioBloqueadoParaProduccion(Entidades.Formulario formulario)
        {
            return WebCore.Helpers.CatalogoPermisos.EsFormularioBloqueadoParaProduccion(formulario);
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

            // Politica de clave segura (ver Negocio.PoliticaClave): aplica a claves nuevas o
            // cambiadas; en una edicion sin clave nueva no se toca la existente.
            if (!string.IsNullOrWhiteSpace(model.Clave))
            {
                string? errorClave = Negocio.PoliticaClave.ValidarClave(model.Clave, model.Usuario);
                if (errorClave != null)
                    ModelState.AddModelError("Clave", errorClave);
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
                Bloqueado = usuario.Bloqueado,
                BloqueadoNoSeguro = usuario.BloqueadoNoSeguro,
                TienePin = usuario.TienePin,
                EsProduccion = usuario.EsUsuarioProduccion,
                RequiereDispositivoSeguro = usuario.RequiereDispositivoSeguro,
                EntraAlPos = EntraAlPos(usuario)
            };
        }

        // Misma regla que VentasController.TieneAlgunPermisoPOS y el indicador "Entra al POS" del asistente: admin siempre; el resto,
        // con cualquier permiso "POS - ..." o modificar Pagos y cobros. Una cuenta de produccion nunca (esos permisos estan bloqueados).
        private static bool EntraAlPos(Entidades.Usuario usuario)
        {
            if (usuario.Admin) return true;
            if (usuario.EsUsuarioProduccion) return false;

            var estados = new List<(WebCore.Helpers.DefinicionPermiso, WebCore.Helpers.EstadoPermisoAsistente)>();
            foreach (var fila in (usuario.Permisos ?? new List<PermisosUsuarios>()).GroupBy(p => p.IdForm).Select(g => g.First()))
            {
                var definicion = WebCore.Helpers.CatalogoPermisos.Para(fila.Formulario, fila.IdForm);
                if (!string.IsNullOrEmpty(definicion.SinEfectoHoy)) continue;
                estados.Add((definicion, WebCore.Helpers.AsistentePermisosMapper.EstadoDesde(fila, definicion)));
            }
            return WebCore.Helpers.AsistentePermisosMapper.DaAccesoAlPos(estados);
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
                SoloRegistrosPropios = permiso == null || permiso.SoloRegistrosPropios,
                SinEfectoHoy = permiso != null ? WebCore.Helpers.CatalogoPermisos.Para(permiso.Formulario, permiso.IdForm).SinEfectoHoy : ""
            };
        }
    }
}
