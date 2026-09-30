// Modulo Empleados y Liquidacion de Sueldos (2026-09-29/30, ver docs/DECISIONS.md). "Mi Jornada"
// (self-service del propio empleado, o seleccion para el usuario de produccion), fichaje en
// dispositivos habilitados (Jornadas/Fichaje -- ver nota operativa abajo) y la vista admin de
// correcciones. Solo Postgres.
//
// NOTA OPERATIVA sobre el fichaje "sin contraseña": el pipeline global de WebCore exige sesion
// autenticada en TODA accion (Program.cs, RequireAuthenticatedUser fallback policy) -- por eso
// Jornadas/Fichaje no puede ser [AllowAnonymous]. La forma de usarlo es: alguien (un encargado)
// inicia sesion UNA VEZ en el dispositivo/tablet compartido y deja el navegador abierto en esa
// pantalla; a partir de ahi, cualquier empleado puede tocar su nombre sin volver a loguearse -- la
// confianza está puesta en el DISPOSITIVO (EsFichajeHabilitado), no en quien tiene la sesion de
// ese navegador. El "sin contraseña" es por-empleado, no por-dispositivo. El usuario de produccion
// (Usuario.EsUsuarioProduccion) es la segunda forma de llegar al mismo mecanismo: su propia sesion
// ya autenticada habilita el gate, sin necesitar ademas un dispositivo marcado.
using System;
using System.Collections.Generic;
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
        private readonly Negocio.DispositivoSeguro _oDispositivoN;
        private readonly Negocio.Usuario _oUsuarioN;
        private readonly Negocio.LiquidacionSueldo _oLiquidacionN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public JornadasController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oEmpleadoN = WebCore.Infrastructure.NegocioFactory.CrearEmpleado(_empresa, _param);
            _oRegistroN = WebCore.Infrastructure.NegocioFactory.CrearRegistroJornada(_empresa);
            _oDispositivoN = WebCore.Infrastructure.NegocioFactory.CrearDispositivoSeguro(_empresa);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
            _oLiquidacionN = WebCore.Infrastructure.NegocioFactory.CrearLiquidacionSueldo(_empresa, _param);
        }

        // ---------- Mi Jornada (self-service, o seleccion para produccion) ----------

        [HttpGet]
        public IActionResult MiJornada()
        {
            if (_usuarioActual.EsUsuarioProduccion)
            {
                var empleadosJornada = _oEmpleadoN.Listar(_empresa.IdEmpresa, null, null, true)
                    .Where(e => Entidades.Empleado.RequiereRegistroJornada(e.FormaLiquidacion))
                    .Select(e => new { id = e.Usuario.Id, nombre = e.Persona?.razonSocial ?? "" })
                    .OrderBy(e => e.nombre)
                    .ToList();

                ViewBag.UsuariosJornada = System.Text.Json.JsonSerializer.Serialize(empleadosJornada);
                ViewBag.Title = "Mi jornada";
                ViewBag.Seccion = "Empleados";
                return View("~/Views/Jornadas/MiJornadaProduccion.cshtml");
            }

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
                // Desde la ultima liquidacion Confirmada (exclusive: ese dia ya quedo liquidado) en
                // vez de una ventana fija -- si todavia no tiene ninguna, cae a los ultimos 14 dias
                // (mismo criterio que antes) para no mostrar un historial enorme a un empleado nuevo.
                var ultimaLiquidacion = _oLiquidacionN.ObtenerUltimaConfirmada(empleado.Id);
                DateTime desdeMarcaciones = ultimaLiquidacion != null
                    ? ultimaLiquidacion.PeriodoHasta.Date.AddDays(1)
                    : DateTime.Today.AddDays(-14);

                model.MarcacionesRecientes = _oRegistroN.ListarPorEmpleadoYRango(empleado.Id, desdeMarcaciones, DateTime.Today);
                model.MarcacionesTitulo = ultimaLiquidacion != null
                    ? $"Marcaciones desde tu última liquidación ({desdeMarcaciones:dd/MM/yyyy})"
                    : "Marcaciones recientes (últimos 14 días)";

                if (empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.Hora)
                    model.EstaAbierto = _oRegistroN.ObtenerAbiertoHoy(empleado.Id) != null;
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

        // Autoreporte de un dia anterior olvidado, desde "Mi Jornada" individual (no produccion, no
        // tiene un Empleado propio fijo). Nunca toma un idEmpleado del formulario -- siempre el
        // propio, para que nadie pueda autoreportar en nombre de otro. Motivo siempre obligatorio y
        // nunca puede marcar feriado (ver Negocio.RegistroJornada.AgregarManual).
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarPropio(DateTime fecha, Entidades.Turno? turno, string horaEntrada, string horaSalida, decimal? cantidad, string motivo)
        {
            var empleado = ObtenerMiEmpleado();
            if (empleado == null) return RedirectToAction("MiJornada");

            TimeSpan? entrada = TimeSpan.TryParse(horaEntrada, out var e) ? e : (TimeSpan?)null;
            TimeSpan? salida = TimeSpan.TryParse(horaSalida, out var s) ? s : (TimeSpan?)null;

            try
            {
                _oRegistroN.AgregarManual(empleado, fecha, turno, entrada, salida, cantidad, esFeriado: false,
                    motivo: motivo, idUsuarioAccion: _usuarioActual.Id, esAutoreporte: true);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Mi jornada";
                TempData["AlertMsg"] = "La marcación se cargó correctamente.";
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

        // ---------- Fichaje en dispositivo habilitado (o usuario de produccion) ----------

        private bool FichajeHabilitadoAqui()
        {
            return _oDispositivoN.EsFichajeHabilitado(SerieDeEsteDispositivo(), _empresa.IdEmpresa) || _usuarioActual.EsUsuarioProduccion;
        }

        [HttpGet]
        public IActionResult Fichaje()
        {
            bool habilitado = FichajeHabilitadoAqui();
            var model = new FichajeListaVm { DispositivoHabilitado = habilitado };

            if (habilitado)
            {
                model.Empleados = _oEmpleadoN.Listar(_empresa.IdEmpresa, null, null, true)
                    .Where(e => Entidades.Empleado.RequiereRegistroJornada(e.FormaLiquidacion))
                    .Select(e => new EmpleadoFichajeItemVm { IdEmpleado = e.Id, Nombre = e.Persona?.razonSocial ?? "", Identificacion = e.Persona?.Identificacion ?? "" })
                    .OrderBy(e => e.Nombre)
                    .ToList();
            }

            ViewBag.Title = "Fichaje";
            return View("~/Views/Jornadas/Fichaje.cshtml", model);
        }

        [HttpGet]
        public IActionResult FichajeConfirmar(int idEmpleado = 0, int idUsuario = 0)
        {
            if (!FichajeHabilitadoAqui())
                return RedirectToAction("Fichaje");

            var empleado = idEmpleado > 0
                ? _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa)
                : _oEmpleadoN.ObtenerPorIdUsuario(idUsuario, _empresa.IdEmpresa);

            if (empleado == null || !empleado.Activo || !Entidades.Empleado.RequiereRegistroJornada(empleado.FormaLiquidacion))
                return RedirectToAction(_usuarioActual.EsUsuarioProduccion ? "MiJornada" : "Fichaje");

            var model = new FichajeConfirmarVm
            {
                IdEmpleado = empleado.Id,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                FormaLiquidacion = empleado.FormaLiquidacion
            };

            if (empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.Hora)
            {
                model.EsIngreso = _oRegistroN.ObtenerAbiertoHoy(empleado.Id) == null;
                model.HoraPropuestaTexto = DateTime.Now.ToString("HH:mm");
            }
            else
            {
                // Jornada/MediaJornada: se estima el turno por la hora del reloj -- antes de las
                // 12:00 Mañana, desde las 12:00 Tarde (2026-09-30, ver docs/DECISIONS.md). Editable
                // en la pantalla si el sistema se equivoca (ej. horario partido).
                model.TurnoEstimado = DateTime.Now.Hour < 12 ? Entidades.Turno.Manana : Entidades.Turno.Tarde;
            }

            ViewBag.Title = "Fichaje";
            return View("~/Views/Jornadas/FichajeConfirmar.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult FichajeRegistrar(int idEmpleado, string horaCorregida, string motivo, string motivoOtro, string observacion, Entidades.Turno? turno)
        {
            if (!FichajeHabilitadoAqui())
                return RedirectToAction("Fichaje");

            var dispositivo = _oDispositivoN.ObtenerPorSerie(SerieDeEsteDispositivo(), _empresa.IdEmpresa);
            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null) return RedirectToAction("Fichaje");

            try
            {
                if (empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.Hora)
                {
                    DateTime? horaFinal = null;
                    string motivoFinal = null;
                    if (!string.IsNullOrWhiteSpace(horaCorregida) && TimeSpan.TryParse(horaCorregida, out var hora))
                    {
                        horaFinal = DateTime.Today + hora;
                        motivoFinal = string.Equals(motivo, "Otro", StringComparison.OrdinalIgnoreCase) ? motivoOtro : motivo;
                    }

                    var resultado = _oRegistroN.Fichar(empleado, empleado.Usuario.Id, dispositivo?.Id, horaFinal, motivoFinal, observacion);
                    TempData["AlertMsg"] = "Hola " + empleado.Persona?.razonSocial + ", " + (resultado.esIngreso ? "ingreso" : "salida")
                        + " registrado a las " + resultado.horaRegistrada.ToString("HH:mm") + ".";
                }
                else
                {
                    var turnoElegido = turno ?? (DateTime.Now.Hour < 12 ? Entidades.Turno.Manana : Entidades.Turno.Tarde);
                    _oRegistroN.CargarJornada(empleado, DateTime.Today, turnoElegido, empleado.Usuario.Id, observacion);
                    TempData["AlertMsg"] = "Hola " + empleado.Persona?.razonSocial + ", se registró tu jornada de hoy (turno "
                        + (turnoElegido == Entidades.Turno.Manana ? "mañana" : "tarde") + ").";
                }

                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Fichaje";
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

        // Resumen informativo del rango filtrado en Jornadas/Index -- el conteo (horas/jornadas) es
        // exacto; el "estimado" de sueldo es una cuenta aproximada con la tarifa vigente de cada
        // registro (misma resolucion que usa Negocio.LiquidacionSueldo, ver Negocio/Empleado.cs
        // ResolverTarifaVigente), pero NO reemplaza a una liquidacion real: no agrupa por feriado,
        // no suma vacaciones ni lineas manuales. Solo se muestra si el empleado tiene al menos una
        // tarifa configurada (sin eso no hay nada que estimar).
        private string ResumirTotales(List<Entidades.RegistroJornada> registros, string formaLiquidacion, int idEmpleado)
        {
            if (registros == null || registros.Count == 0) return "";

            var historialTarifas = _oEmpleadoN.ListarTarifas(idEmpleado);
            bool tieneAlgunaTarifa = false;
            bool faltaAlgunaTarifa = false;
            decimal totalEstimado = 0;

            if (formaLiquidacion == Entidades.Empleado.formaLiquidacion.Hora.ToString())
            {
                decimal totalHoras = 0;
                foreach (var r in registros)
                {
                    if (!r.HoraEntrada.HasValue || !r.HoraSalida.HasValue || r.HoraSalida <= r.HoraEntrada) continue;
                    decimal horas = (decimal)(r.HoraSalida.Value - r.HoraEntrada.Value).TotalHours;
                    totalHoras += horas;

                    var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, null, r.Fecha, r.EsFeriado);
                    if (tarifa == null) { faltaAlgunaTarifa = true; continue; }
                    tieneAlgunaTarifa = true;
                    totalEstimado += horas * tarifa.Valor;
                }

                string texto = $"Total del período: {totalHoras:0.##} horas trabajadas.";
                if (tieneAlgunaTarifa)
                    texto += $" Estimado: ${totalEstimado:N2}" + (faltaAlgunaTarifa ? " (incompleto: hay días sin tarifa configurada)." : ".");
                return texto;
            }

            if (formaLiquidacion == Entidades.Empleado.formaLiquidacion.Jornada.ToString()
                || formaLiquidacion == Entidades.Empleado.formaLiquidacion.MediaJornada.ToString())
            {
                decimal totalCantidad = 0;
                foreach (var r in registros.Where(r => r.Cantidad.HasValue && r.Turno.HasValue))
                {
                    totalCantidad += r.Cantidad.Value;

                    var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, r.Turno, r.Fecha, r.EsFeriado);
                    if (tarifa == null) { faltaAlgunaTarifa = true; continue; }
                    tieneAlgunaTarifa = true;
                    totalEstimado += r.Cantidad.Value * tarifa.Valor;
                }

                string unidad = formaLiquidacion == Entidades.Empleado.formaLiquidacion.MediaJornada.ToString() ? "media(s) jornada(s)" : "jornada(s)";
                string texto = $"Total del período: {totalCantidad:0.##} {unidad}.";
                if (tieneAlgunaTarifa)
                    texto += $" Estimado: ${totalEstimado:N2}" + (faltaAlgunaTarifa ? " (incompleto: hay días sin tarifa configurada)." : ".");
                return texto;
            }

            return "";
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

            // Sin fecha explicita en la URL: si hay un empleado elegido, arranca desde su ultima
            // liquidacion Confirmada (igual criterio que "Mi Jornada", ver DECISIONS.md 2026-09-30);
            // sin liquidacion previa o sin empleado todavia, cae a los ultimos 14 dias de siempre.
            DateTime desdeReal;
            if (desde.HasValue)
            {
                desdeReal = desde.Value.Date;
            }
            else if (idEmpleado.HasValue)
            {
                var ultimaLiquidacion = _oLiquidacionN.ObtenerUltimaConfirmada(idEmpleado.Value);
                desdeReal = ultimaLiquidacion != null ? ultimaLiquidacion.PeriodoHasta.Date.AddDays(1) : DateTime.Today.AddDays(-14);
            }
            else
            {
                desdeReal = DateTime.Today.AddDays(-14);
            }
            DateTime hastaReal = (hasta ?? DateTime.Today).Date;

            var model = new JornadasIndexVm
            {
                Empleados = _oEmpleadoN.Listar(_empresa.IdEmpresa, null, null, true)
                    .Select(e => new EmpleadoResumenVm { Id = e.Id, RazonSocial = e.Persona?.razonSocial ?? "", Identificacion = e.Persona?.Identificacion ?? "", FormaLiquidacion = e.FormaLiquidacion.ToString() })
                    .ToList(),
                IdEmpleadoSeleccionado = idEmpleado,
                Desde = desdeReal,
                Hasta = hastaReal,
                CorreccionesPendientes = _oRegistroN.ListarCorreccionesPendientes(_empresa.IdEmpresa, idEmpleado)
            };

            if (idEmpleado.HasValue)
            {
                model.Registros = _oRegistroN.ListarPorEmpleadoYRango(idEmpleado.Value, desdeReal, hastaReal);
                model.FormaLiquidacionSeleccionado = model.Empleados.FirstOrDefault(e => e.Id == idEmpleado.Value)?.FormaLiquidacion;
                model.ResumenTotales = ResumirTotales(model.Registros, model.FormaLiquidacionSeleccionado, idEmpleado.Value);
            }

            var nombresPorUsuario = new Dictionary<int, string>();
            foreach (var idUsuarioModif in model.CorreccionesPendientes.Select(h => h.ModificadoPor).Distinct())
            {
                if (nombresPorUsuario.ContainsKey(idUsuarioModif)) continue;
                nombresPorUsuario[idUsuarioModif] = _oUsuarioN.getUsuarioById(idUsuarioModif)?.Nombre ?? ("Usuario #" + idUsuarioModif);
            }
            ViewBag.NombresPorUsuario = nombresPorUsuario;

            ViewBag.Title = "Jornadas";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Jornadas/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Corregir(int idRegistroJornada, string horaEntrada, string horaSalida, decimal? cantidad,
            Entidades.Turno? turno, bool esFeriado, string motivo, int idEmpleadoVolver)
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
                _oRegistroN.Corregir(idRegistroJornada, _empresa.IdEmpresa, entrada, salida, cantidad, turno, esFeriado, motivo, _usuarioActual.Id);
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

        // Alta manual de un admin (empleado que se olvido de fichar) -- distinta del autoreporte
        // (AgregarPropio): acá sí puede marcarse feriado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AgregarRegistro(int idEmpleado, DateTime fecha, Entidades.Turno? turno, string horaEntrada,
            string horaSalida, decimal? cantidad, bool esFeriado, string motivo)
        {
            if (!PuedeAdministrar())
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sin permiso";
                TempData["AlertMsg"] = "No tiene permisos para agregar marcaciones.";
                return RedirectToAction("Index", new { idEmpleado });
            }

            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Jornadas";
                TempData["AlertMsg"] = "No se encontró el empleado seleccionado.";
                return RedirectToAction("Index");
            }

            TimeSpan? entrada = TimeSpan.TryParse(horaEntrada, out var e) ? e : (TimeSpan?)null;
            TimeSpan? salida = TimeSpan.TryParse(horaSalida, out var s) ? s : (TimeSpan?)null;

            try
            {
                _oRegistroN.AgregarManual(empleado, fecha, turno, entrada, salida, cantidad, esFeriado, motivo, _usuarioActual.Id, esAutoreporte: false);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Jornadas";
                TempData["AlertMsg"] = "La marcación se agregó correctamente.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Jornadas";
                TempData["AlertMsg"] = ex.Message;
            }

            return RedirectToAction("Index", new { idEmpleado });
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
