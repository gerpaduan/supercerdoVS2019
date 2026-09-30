// Modulo Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). ABM unificado:
// crea/vincula una Persona y un Usuario en un solo paso (Negocio.Empleado.GuardarUnificado), mas
// las pestañas Vacaciones e Historial. Solo Postgres -- NegocioFactory.CrearEmpleado tira si
// DataEngine=SqlServer (el modulo no tiene rama legada).
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Empleado _oEmpleadoN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.LiquidacionSueldo _oLiquidacionN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public EmpleadosController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oEmpleadoN = WebCore.Infrastructure.NegocioFactory.CrearEmpleado(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oLiquidacionN = WebCore.Infrastructure.NegocioFactory.CrearLiquidacionSueldo(_empresa, _param);
        }

        public IActionResult Index(string texto)
        {
            var items = _oEmpleadoN.Listar(_empresa.IdEmpresa, texto, null, null)
                .Select(MapResumen)
                .ToList();

            var model = new EmpleadoIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(_usuarioActual),
                Texto = texto ?? "",
                Items = items
            };

            ViewBag.Title = "Empleados";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Empleados/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Editar(int id = 0)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = id > 0 ? "No tiene permisos para modificar empleados." : "No tiene permisos para crear empleados.";
                return RedirectToAction("Index");
            }

            var model = new EmpleadoEditVm { IdEmpresa = _empresa.IdEmpresa, FechaIngreso = DateTime.Today, Activo = true };

            if (id > 0)
            {
                var empleado = _oEmpleadoN.ObtenerPorId(id, _empresa.IdEmpresa);
                if (empleado == null) return NotFoundEmpleado();

                var persona = _oPersonaN.findById(empleado.Persona.idPersona);
                var usuario = _oUsuarioN.getUsuarioById(empleado.Usuario.Id);

                model.Id = empleado.Id;
                model.EsEdicion = true;
                model.IdPersonaExistente = empleado.Persona.idPersona;
                model.PersonaRazonSocial = persona?.razonSocial ?? empleado.Persona.razonSocial ?? "";
                model.PersonaIdentificacion = persona?.Identificacion ?? "";
                model.PersonaTelefono = persona?.Telefono ?? "";
                model.PersonaDomicilio = persona?.Domicilio ?? "";
                model.IdUsuarioExistente = empleado.Usuario.Id;
                model.UsuarioNombre = usuario?.Nombre ?? empleado.Usuario.Nombre ?? "";
                model.UsuarioLogin = usuario?.User ?? empleado.Usuario.User ?? "";
                model.Legajo = empleado.Legajo ?? "";
                model.FormaLiquidacion = empleado.FormaLiquidacion;
                model.FechaIngreso = empleado.FechaIngreso;
                model.Activo = empleado.Activo;
                model.TarifasActuales = _oEmpleadoN.ListarTarifas(id).Select(MapTarifa).ToList();
            }

            CargarCombos();
            ViewBag.Title = model.EsEdicion ? "Modificar empleado" : "Nuevo empleado";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Empleados/Editar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(EmpleadoEditVm model, decimal?[] tarifaValor, string[] tarifaTurno, string[] tarifaDiaSemana, DateTime?[] tarifaVigenteDesde)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para guardar empleados.";
                return RedirectToAction("Index");
            }

            // PersonaRazonSocial/UsuarioNombre/UsuarioLogin no llevan [Required] en el ViewModel
            // porque solo son obligatorios cuando se crea una Persona/Usuario nueva -- si se vincula
            // uno existente (IdPersonaExistente/IdUsuarioExistente > 0), esos campos quedan ocultos
            // y vacíos en el formulario a propósito.
            if ((model.IdPersonaExistente ?? 0) <= 0 && string.IsNullOrWhiteSpace(model.PersonaRazonSocial))
                ModelState.AddModelError(nameof(model.PersonaRazonSocial), "La razón social es obligatoria.");

            if ((model.IdUsuarioExistente ?? 0) <= 0)
            {
                if (string.IsNullOrWhiteSpace(model.UsuarioNombre))
                    ModelState.AddModelError(nameof(model.UsuarioNombre), "El nombre del usuario es obligatorio.");
                if (string.IsNullOrWhiteSpace(model.UsuarioLogin))
                    ModelState.AddModelError(nameof(model.UsuarioLogin), "El login del usuario es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                CargarCombos();
                ViewBag.Title = model.Id > 0 ? "Modificar empleado" : "Nuevo empleado";
                ViewBag.Seccion = "Empleados";
                return View("~/Views/Empleados/Editar.cshtml", model);
            }

            try
            {
                var persona = new Entidades.Persona
                {
                    idPersona = model.IdPersonaExistente ?? 0,
                    razonSocial = (model.PersonaRazonSocial ?? "").Trim(),
                    Identificacion = (model.PersonaIdentificacion ?? "").Trim(),
                    Telefono = (model.PersonaTelefono ?? "").Trim(),
                    Domicilio = (model.PersonaDomicilio ?? "").Trim(),
                    IdIva = Entidades.Persona.idIndefinido
                };

                var usuario = new Entidades.Usuario
                {
                    Id = model.IdUsuarioExistente ?? 0,
                    Nombre = (model.UsuarioNombre ?? "").Trim(),
                    User = (model.UsuarioLogin ?? "").Trim(),
                    Clave = model.UsuarioClave ?? "",
                    Activo = model.Activo
                };

                var empleado = new Entidades.Empleado
                {
                    Id = model.Id,
                    IdEmpresa = _empresa.IdEmpresa,
                    Legajo = model.Legajo,
                    FormaLiquidacion = model.FormaLiquidacion,
                    FechaIngreso = model.FechaIngreso,
                    Activo = model.Activo,
                    ActualizadoPor = _usuarioActual.Id,
                    CreadoPor = _usuarioActual.Id
                };

                int idEmpleado = _oEmpleadoN.GuardarUnificado(empleado, persona, usuario);

                // Tarifas cargadas en la grilla del formulario: cada fila con Valor>0 se agrega
                // como una tarifa nueva (append-only, nunca reemplaza una existente).
                if (tarifaValor != null)
                {
                    for (int i = 0; i < tarifaValor.Length; i++)
                    {
                        if (!tarifaValor[i].HasValue || tarifaValor[i].Value <= 0) continue;

                        Entidades.Turno? turno = (tarifaTurno != null && i < tarifaTurno.Length
                            && Enum.TryParse<Entidades.Turno>(tarifaTurno[i], out var turnoParseado)) ? turnoParseado : (Entidades.Turno?)null;
                        Entidades.DiaSemana? dia = (tarifaDiaSemana != null && i < tarifaDiaSemana.Length
                            && Enum.TryParse<Entidades.DiaSemana>(tarifaDiaSemana[i], out var diaParseado)) ? diaParseado : (Entidades.DiaSemana?)null;
                        DateTime vigenteDesde = (tarifaVigenteDesde != null && i < tarifaVigenteDesde.Length && tarifaVigenteDesde[i].HasValue)
                            ? tarifaVigenteDesde[i].Value : DateTime.Today;

                        _oEmpleadoN.AgregarTarifa(new Entidades.EmpleadoTarifa
                        {
                            IdEmpresa = _empresa.IdEmpresa,
                            IdEmpleado = idEmpleado,
                            Turno = turno,
                            DiaSemana = dia,
                            Valor = tarifaValor[i].Value,
                            VigenteDesde = vigenteDesde,
                            CreadoPor = _usuarioActual.Id
                        });
                    }
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Empleados";
                TempData["AlertMsg"] = model.Id > 0 ? "El empleado se actualizó correctamente." : "El empleado se creó correctamente.";
                return RedirectToAction("Editar", new { id = idEmpleado });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                CargarCombos();
                ViewBag.Title = model.Id > 0 ? "Modificar empleado" : "Nuevo empleado";
                ViewBag.Seccion = "Empleados";
                return View("~/Views/Empleados/Editar.cshtml", model);
            }
        }

        // Baja/reactivacion, con cascada al Usuario vinculado (ver Negocio/Empleado.cs).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SetActivo(int id, bool activo)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para dar de baja empleados.";
                return RedirectToAction("Index");
            }

            try
            {
                _oEmpleadoN.SetActivo(id, _empresa.IdEmpresa, activo);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Empleados";
                TempData["AlertMsg"] = activo
                    ? "El empleado fue reactivado (y su usuario también)."
                    : "El empleado fue dado de baja (y su usuario también quedó inactivo, no podrá loguearse).";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Empleados";
                TempData["AlertMsg"] = ex.Message;
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Vacaciones(int id)
        {
            var empleado = _oEmpleadoN.ObtenerPorId(id, _empresa.IdEmpresa);
            if (empleado == null) return NotFoundEmpleado();

            var model = new EmpleadoVacacionesVm
            {
                IdEmpleado = id,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                Items = _oEmpleadoN.ListarVacaciones(id),
                NuevaFechaDesde = DateTime.Today,
                NuevaFechaHasta = DateTime.Today
            };

            ViewBag.Title = "Vacaciones de " + model.EmpleadoNombre;
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Empleados/Vacaciones.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarVacacion(int idEmpleado, DateTime fechaDesde, DateTime fechaHasta, string observaciones)
        {
            if (!PuedeAdministrar(_usuarioActual))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para cargar vacaciones.";
                return RedirectToAction("Vacaciones", new { id = idEmpleado });
            }

            if (_oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa) == null) return NotFoundEmpleado();

            try
            {
                _oEmpleadoN.AgregarVacacion(new Entidades.EmpleadoVacacion
                {
                    IdEmpresa = _empresa.IdEmpresa,
                    IdEmpleado = idEmpleado,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Observaciones = observaciones,
                    CreadoPor = _usuarioActual.Id
                });
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Vacaciones";
                TempData["AlertMsg"] = "El período de vacaciones se cargó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Vacaciones";
                TempData["AlertMsg"] = ex.Message;
            }
            return RedirectToAction("Vacaciones", new { id = idEmpleado });
        }

        [HttpGet]
        public IActionResult Historial(int id, DateTime? desde, DateTime? hasta)
        {
            var empleado = _oEmpleadoN.ObtenerPorId(id, _empresa.IdEmpresa);
            if (empleado == null) return NotFoundEmpleado();

            DateTime desdeReal = (desde ?? DateTime.Today.AddMonths(-3)).Date;
            DateTime hastaReal = (hasta ?? DateTime.Today).Date;

            var model = new EmpleadoHistorialVm
            {
                IdEmpleado = id,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                Desde = desdeReal,
                Hasta = hastaReal,
                Tarifas = _oEmpleadoN.ListarTarifas(id).Where(t => t.VigenteDesde.Date <= hastaReal).ToList(),
                Liquidaciones = _oLiquidacionN.ListarPorEmpleado(id)
                    .Where(l => l.PeriodoDesde.Date <= hastaReal && l.PeriodoHasta.Date >= desdeReal)
                    .ToList()
            };

            ViewBag.Title = "Historial de " + model.EmpleadoNombre;
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Empleados/Historial.cshtml", model);
        }

        // Busqueda para el modal de "vincular Persona existente" en el alta (Editar.cshtml, solo
        // !EsEdicion). Excluye preventivamente a quien ya es empleado de otro -- ver
        // docs/DECISIONS.md 2026-09-30. GuardarUnificado revalida lo mismo al confirmar.
        [HttpGet]
        public JsonResult BuscarPersonas(string filtro)
        {
            DataTable dt = _oPersonaN.buscarPersona(filtro ?? "", false) ?? new DataTable();

            var personas = dt.AsEnumerable()
                .Where(row => Convert.ToInt32(row["idEmpresa"]) == _empresa.IdEmpresa || Convert.ToInt32(row["idEmpresa"]) == 0)
                .Select(row => new
                {
                    idPersona = Convert.ToInt32(row["idPersona"]),
                    razonSocial = row["razonSocial"] == DBNull.Value ? "" : row["razonSocial"].ToString(),
                    identificacion = row.Table.Columns.Contains("nombreIdentif") && row["nombreIdentif"] != DBNull.Value ? row["nombreIdentif"].ToString() : "",
                    cuit = row["cuit"] == DBNull.Value ? "" : row["cuit"].ToString()
                })
                .Where(p => !_oEmpleadoN.ExistePersonaVinculada(p.idPersona, _empresa.IdEmpresa))
                .OrderBy(p => p.razonSocial)
                .Take(30)
                .ToList();

            return Json(personas);
        }

        // Busqueda para el modal de "vincular Usuario existente" en el alta. No hay un metodo de
        // negocio de busqueda por texto libre (BuscarUsuariosPorIdentificador es para recuperacion
        // de clave), asi que se filtra en memoria sobre obtenerUsuarios(soloActivos:true) (ya
        // limitado a la empresa actual).
        [HttpGet]
        public JsonResult BuscarUsuarios(string filtro)
        {
            filtro = (filtro ?? "").Trim();
            DataTable dt = _oUsuarioN.obtenerUsuarios(true) ?? new DataTable();

            var usuarios = dt.AsEnumerable()
                .Select(row => new
                {
                    id = Convert.ToInt32(row["id"]),
                    nombre = row["nombre"] == DBNull.Value ? "" : row["nombre"].ToString(),
                    usuarioLogin = row["usuario"] == DBNull.Value ? "" : row["usuario"].ToString()
                })
                .Where(u => string.IsNullOrEmpty(filtro)
                    || u.nombre.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0
                    || u.usuarioLogin.IndexOf(filtro, StringComparison.OrdinalIgnoreCase) >= 0)
                .Where(u => !_oEmpleadoN.ExisteUsuarioVinculado(u.id, _empresa.IdEmpresa))
                .OrderBy(u => u.nombre)
                .Take(30)
                .ToList();

            return Json(usuarios);
        }

        private IActionResult NotFoundEmpleado()
        {
            TempData["AlertType"] = "error";
            TempData["AlertTitle"] = "No encontrado";
            TempData["AlertMsg"] = "No se encontró el empleado seleccionado.";
            return RedirectToAction("Index");
        }

        private void CargarCombos()
        {
            ViewBag.FormasLiquidacion = Enum.GetValues(typeof(Entidades.Empleado.formaLiquidacion))
                .Cast<Entidades.Empleado.formaLiquidacion>()
                .Select(f => new SelectListItem { Value = f.ToString(), Text = f.ToString() })
                .ToList();
            ViewBag.Turnos = new List<SelectListItem>
            {
                new SelectListItem { Value = "", Text = "(general, sin distinguir turno)" },
                new SelectListItem { Value = Entidades.Turno.Manana.ToString(), Text = "Mañana" },
                new SelectListItem { Value = Entidades.Turno.Tarde.ToString(), Text = "Tarde" }
            };
            ViewBag.DiasSemana = new List<SelectListItem> { new SelectListItem { Value = "", Text = "(todos los días)" } }
                .Concat(Enum.GetValues(typeof(Entidades.DiaSemana)).Cast<Entidades.DiaSemana>()
                    .Select(d => new SelectListItem { Value = d.ToString(), Text = d.ToString() }))
                .ToList();
        }

        internal bool PuedeAdministrar(Entidades.Usuario usuarioActual)
        {
            if (usuarioActual == null) return false;
            if (usuarioActual.Admin) return true;
            if (usuarioActual.Permisos == null || usuarioActual.Permisos.Count == 0) return false;
            return _oUsuarioN.tienePermiso(usuarioActual, Entidades.Permisos.Empleado.NuevoEmpleado, DateTime.Today, usuarioActual.Id);
        }

        private static EmpleadoResumenVm MapResumen(Entidades.Empleado empleado)
        {
            return new EmpleadoResumenVm
            {
                Id = empleado.Id,
                Legajo = empleado.Legajo ?? "",
                RazonSocial = empleado.Persona?.razonSocial ?? "",
                UsuarioLogin = empleado.Usuario?.User ?? "",
                FormaLiquidacion = empleado.FormaLiquidacion.ToString(),
                Activo = empleado.Activo
            };
        }

        private static EmpleadoTarifaItemVm MapTarifa(Entidades.EmpleadoTarifa tarifa)
        {
            return new EmpleadoTarifaItemVm
            {
                Turno = tarifa.Turno?.ToString() ?? "",
                TurnoTexto = tarifa.Turno == Entidades.Turno.Manana ? "Mañana" : tarifa.Turno == Entidades.Turno.Tarde ? "Tarde" : "General",
                DiaSemana = tarifa.DiaSemana?.ToString() ?? "",
                Valor = tarifa.Valor,
                VigenteDesde = tarifa.VigenteDesde
            };
        }
    }
}
