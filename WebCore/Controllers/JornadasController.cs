// Modulo Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). "Mi Jornada"
// (self-service del propio empleado), fichaje en dispositivos habilitados (Jornadas/Fichaje -- ver
// nota operativa abajo) y la vista admin de correcciones. Solo Postgres.
//
// NOTA OPERATIVA sobre el fichaje "sin contraseña": el pipeline global de WebCore exige sesion
// autenticada en TODA accion (Program.cs, RequireAuthenticatedUser fallback policy) -- por eso
// Jornadas/Fichaje no puede ser [AllowAnonymous]. La forma de usarlo es: alguien (un encargado)
// inicia sesion UNA VEZ en el dispositivo/tablet compartido y deja el navegador abierto en esa
// pantalla; a partir de ahi, cualquier empleado puede tocar su nombre sin volver a loguearse -- la
// confianza está puesta en el DISPOSITIVO (EsFichajeHabilitado), no en quien tiene la sesion de
// ese navegador. El "sin contraseña" es por-empleado, no por-dispositivo.
using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class JornadasController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Empleado _oEmpleadoN;
        private readonly Negocio.RegistroJornada _oRegistroN;
        private readonly Negocio.LiquidacionSueldo _oLiquidacionN;
        private readonly Negocio.DispositivoSeguro _oDispositivoN;
        private readonly Negocio.Usuario _oUsuarioN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public JornadasController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oEmpleadoN = WebCore.Infrastructure.NegocioFactory.CrearEmpleado(_empresa, _param);
            _oRegistroN = WebCore.Infrastructure.NegocioFactory.CrearRegistroJornada(_empresa);
            _oLiquidacionN = WebCore.Infrastructure.NegocioFactory.CrearLiquidacionSueldo(_empresa, _param);
            _oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(_empresa);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        // ---------- Mi Jornada (self-service) ----------

        [HttpGet]
        public IActionResult MiJornada()
        {
            var empleado = _oEmpleadoN.ObtenerPorIdUsuario(_usuarioActual.Id, _empresa.IdEmpresa);
            if (empleado == null)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = "Tu usuario no tiene un empleado asociado.";
                return RedirectToAction("Index", "Home");
            }

            var model = new MiJornadaVm
            {
                IdEmpleado = empleado.Id,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                FormaLiquidacion = empleado.FormaLiquidacion,
                RequiereRegistro = Entidades.Empleado.RequiereRegistroJornada(empleado.FormaLiquidacion)
            };

            var vacacion = _oEmpleadoN.ObtenerVacacionVigente(empleado.Id, DateTime.Today);
            if (vacacion != null) model.VacacionAvisoHasta = vacacion.FechaHasta.ToString("dd/MM/yyyy");

            if (model.RequiereRegistro)
            {
                model.MarcacionesRecientes = _oRegistroN.ListarPorEmpleadoYRango(empleado.Id, DateTime.Today.AddDays(-14), DateTime.Today);
                if (empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.Hora)
                    model.EstaAbierto = _oRegistroN.ObtenerAbiertoHoy(empleado.Id) != null;

                var ultima = _oLiquidacionN.ObtenerUltimaConfirmada(empleado.Id);
                model.JornadaActivaDesde = ultima?.PeriodoHasta ?? empleado.FechaIngreso;

                if (model.JornadaActivaDesde < DateTime.Now)
                {
                    var preview = _oLiquidacionN.CalcularPreview(empleado, model.JornadaActivaDesde, DateTime.Now);
                    if (!preview.TieneConflicto)
                    {
                        model.JornadaActivaDetalle = preview.Liquidacion.Detalle;
                        model.JornadaActivaTotalEstimado = preview.Liquidacion.TotalLiquidado;
                    }
                }
            }

            ViewBag.Title = "Mi jornada";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Jornadas/MiJornada.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarHora()
        {
            var empleado = ObtenerMiEmpleado();
            if (empleado == null) return RedirectToAction("MiJornada");

            try
            {
                var resultado = _oRegistroN.Fichar(empleado, _usuarioActual.Id, null, null, null);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = (resultado.esIngreso ? "Ingreso" : "Salida") + " registrado a las " + resultado.horaRegistrada.ToString("HH:mm") + ".";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = ex.Message;
            }
            return RedirectToAction("MiJornada");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarcarJornada(Entidades.Turno turno)
        {
            var empleado = ObtenerMiEmpleado();
            if (empleado == null) return RedirectToAction("MiJornada");

            try
            {
                _oRegistroN.CargarJornada(empleado, DateTime.Today, turno, _usuarioActual.Id);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = "Jornada cargada correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = ex.Message;
            }
            return RedirectToAction("MiJornada");
        }

        private Entidades.Empleado ObtenerMiEmpleado()
        {
            var empleado = _oEmpleadoN.ObtenerPorIdUsuario(_usuarioActual.Id, _empresa.IdEmpresa);
            if (empleado == null)
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = "Tu usuario no tiene un empleado asociado.";
            }
            return empleado;
        }

        // ---------- Fichaje en dispositivo habilitado ----------

        [HttpGet]
        public IActionResult Fichaje()
        {
            string serie = SerieDeEsteDispositivo();
            bool habilitado = _oDispositivoN.EsFichajeHabilitado(serie, _empresa.IdEmpresa);

            var model = new FichajeListaVm { DispositivoHabilitado = habilitado };

            if (habilitado)
            {
                model.Empleados = _oEmpleadoN.Listar(_empresa.IdEmpresa, null, Entidades.Empleado.formaLiquidacion.Hora, true)
                    .Select(e => new EmpleadoFichajeItemVm { IdEmpleado = e.Id, Nombre = e.Persona?.razonSocial ?? "", Legajo = e.Legajo ?? "" })
                    .OrderBy(e => e.Nombre)
                    .ToList();
            }

            ViewBag.Title = "Fichaje";
            return View("~/Views/Jornadas/Fichaje.cshtml", model);
        }

        [HttpGet]
        public IActionResult FichajeConfirmar(int idEmpleado)
        {
            if (!_oDispositivoN.EsFichajeHabilitado(SerieDeEsteDispositivo(), _empresa.IdEmpresa))
                return RedirectToAction("Fichaje");

            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null || !empleado.Activo || empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.Hora)
                return RedirectToAction("Fichaje");

            var model = new FichajeConfirmarVm
            {
                IdEmpleado = empleado.Id,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                EsIngreso = _oRegistroN.ObtenerAbiertoHoy(empleado.Id) == null,
                HoraPropuestaTexto = DateTime.Now.ToString("HH:mm")
            };

            ViewBag.Title = "Fichaje";
            return View("~/Views/Jornadas/FichajeConfirmar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FichajeRegistrar(int idEmpleado, string horaCorregida, string motivo, string motivoOtro)
        {
            if (!_oDispositivoN.EsFichajeHabilitado(SerieDeEsteDispositivo(), _empresa.IdEmpresa))
                return RedirectToAction("Fichaje");

            var dispositivo = _oDispositivoN.ObtenerPorSerie(SerieDeEsteDispositivo(), _empresa.IdEmpresa);
            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null) return RedirectToAction("Fichaje");

            DateTime? horaFinal = null;
            string motivoFinal = null;
            if (!string.IsNullOrWhiteSpace(horaCorregida) && TimeSpan.TryParse(horaCorregida, out var hora))
            {
                horaFinal = DateTime.Today + hora;
                motivoFinal = string.Equals(motivo, "Otro", StringComparison.OrdinalIgnoreCase) ? motivoOtro : motivo;
            }

            try
            {
                var resultado = _oRegistroN.Fichar(empleado, empleado.Usuario.Id, dispositivo?.Id, horaFinal, motivoFinal);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Fichaje";
                TempData["AlertMsg"] = "Hola " + empleado.Persona?.razonSocial + ", " + (resultado.esIngreso ? "ingreso" : "salida")
                    + " registrado a las " + resultado.horaRegistrada.ToString("HH:mm") + ".";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Fichaje";
                TempData["AlertMsg"] = ex.Message;
            }

            return RedirectToAction("Fichaje");
        }

        private string SerieDeEsteDispositivo()
        {
            string token = WebCore.Helpers.DispositivoNavegador.AsegurarToken(HttpContext);
            return Negocio.DispositivoSeguro.SerieDeToken(token);
        }

        // ---------- Vista admin: correcciones + listado por empleado/rango ----------

        [HttpGet]
        public IActionResult Index(int? idEmpleado, DateTime? desde, DateTime? hasta)
        {
            if (!PuedeAdministrar())
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para ver las jornadas de los empleados.";
                return RedirectToAction("Index", "Empleados");
            }

            DateTime desdeReal = (desde ?? DateTime.Today.AddDays(-14)).Date;
            DateTime hastaReal = (hasta ?? DateTime.Today).Date;

            var model = new JornadasIndexVm
            {
                Empleados = _oEmpleadoN.Listar(_empresa.IdEmpresa, null, null, true)
                    .Select(e => new EmpleadoResumenVm { Id = e.Id, RazonSocial = e.Persona?.razonSocial ?? "", Legajo = e.Legajo ?? "" })
                    .ToList(),
                IdEmpleadoSeleccionado = idEmpleado,
                Desde = desdeReal,
                Hasta = hastaReal,
                CorreccionesPendientes = _oRegistroN.ListarCorreccionesPendientes(_empresa.IdEmpresa, idEmpleado)
            };

            if (idEmpleado.HasValue)
                model.Registros = _oRegistroN.ListarPorEmpleadoYRango(idEmpleado.Value, desdeReal, hastaReal);

            ViewBag.Title = "Jornadas";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Jornadas/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Corregir(int idRegistroJornada, string horaEntrada, string horaSalida, decimal? cantidad, string motivo, int idEmpleadoVolver)
        {
            if (!PuedeAdministrar())
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para corregir marcaciones.";
                return RedirectToAction("Index", new { idEmpleado = idEmpleadoVolver });
            }

            TimeSpan? entrada = TimeSpan.TryParse(horaEntrada, out var e) ? e : (TimeSpan?)null;
            TimeSpan? salida = TimeSpan.TryParse(horaSalida, out var s) ? s : (TimeSpan?)null;

            try
            {
                _oRegistroN.Corregir(idRegistroJornada, _empresa.IdEmpresa, entrada, salida, cantidad, motivo, _usuarioActual.Id);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Jornadas";
                TempData["AlertMsg"] = "La marcación se corrigió correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Jornadas";
                TempData["AlertMsg"] = ex.Message;
            }

            return RedirectToAction("Index", new { idEmpleado = idEmpleadoVolver });
        }

        private bool PuedeAdministrar()
        {
            if (_usuarioActual == null) return false;
            if (_usuarioActual.Admin) return true;
            if (_usuarioActual.Permisos == null || _usuarioActual.Permisos.Count == 0) return false;
            return _oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Empleado.NuevoEmpleado, DateTime.Today, _usuarioActual.Id);
        }
    }
}
