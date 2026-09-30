// Modulo Empleados y Liquidacion de Sueldos (2026-09-29, ver docs/DECISIONS.md). Flujo:
// Nueva (elegir empleado + rango) -> Calcular (preview editable, Negocio.LiquidacionSueldo.
// CalcularPreview) -> Confirmar (persiste + acredita en cta cte) -> Index/Eliminar (reversa).
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class LiquidacionesController : Controller
    {
        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;
        private readonly Negocio.Empleado _oEmpleadoN;
        private readonly Negocio.LiquidacionSueldo _oLiquidacionN;
        private readonly Negocio.Persona _oPersonaN;
        private readonly Negocio.Usuario _oUsuarioN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public LiquidacionesController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oEmpleadoN = WebCore.Infrastructure.NegocioFactory.CrearEmpleado(_empresa, _param);
            _oLiquidacionN = WebCore.Infrastructure.NegocioFactory.CrearLiquidacionSueldo(_empresa, _param);
            _oPersonaN = WebCore.Infrastructure.NegocioFactory.CrearPersona(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        [HttpGet]
        public IActionResult Index(int? idEmpleado)
        {
            var model = new LiquidacionIndexVm
            {
                PuedeAdministrar = PuedeAdministrar(),
                Empleados = ListarEmpleadosResumen(),
                IdEmpleadoSeleccionado = idEmpleado
            };

            if (idEmpleado.HasValue)
                model.Items = _oLiquidacionN.ListarPorEmpleado(idEmpleado.Value);

            ViewBag.Title = "Liquidación de sueldos";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Liquidaciones/Index.cshtml", model);
        }

        [HttpGet]
        public IActionResult Nueva(int? idEmpleado)
        {
            if (!PuedeAdministrar()) return SinPermiso();

            var model = new LiquidacionNuevaVm
            {
                Empleados = ListarEmpleadosResumen(),
                IdEmpleado = idEmpleado ?? 0,
                Desde = DateTime.Today,
                Hasta = DateTime.Today
            };

            ViewBag.Title = "Nueva liquidación";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Liquidaciones/Nueva.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Calcular(int idEmpleado, DateTime desde, DateTime hasta)
        {
            if (!PuedeAdministrar()) return SinPermiso();

            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null) return SinEmpleado();

            var resultado = _oLiquidacionN.CalcularPreview(empleado, desde, hasta);

            if (resultado.TieneConflicto)
            {
                var conflicto = resultado.LiquidacionEnConflicto;
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Período superpuesto";
                TempData["AlertMsg"] = $"El período se superpone con la liquidación #{conflicto.Id} "
                    + $"({conflicto.PeriodoDesde:dd/MM/yyyy HH:mm} al {conflicto.PeriodoHasta:dd/MM/yyyy HH:mm}).";
                return RedirectToAction("Nueva", new { idEmpleado });
            }

            var liquidacion = resultado.Liquidacion;
            var model = new LiquidacionPreviewVm
            {
                IdEmpleado = idEmpleado,
                EmpleadoNombre = empleado.Persona?.razonSocial ?? "",
                Desde = desde,
                Hasta = hasta,
                Total = liquidacion.TotalLiquidado,
                DetalleCtaCte = liquidacion.DetalleCtaCte,
                Detalle = liquidacion.Detalle.Select(d => new LiquidacionDetalleItemVm
                {
                    Concepto = d.Concepto,
                    Origen = d.Origen.ToString(),
                    Cantidad = d.Cantidad,
                    ValorUnitario = d.ValorUnitario,
                    Subtotal = d.Subtotal,
                    SinTarifaConfigurada = d.SinTarifaConfigurada
                }).ToList()
            };

            ViewBag.Title = "Confirmar liquidación";
            ViewBag.Seccion = "Empleados";
            return View("~/Views/Liquidaciones/Preview.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmar(int idEmpleado, DateTime desde, DateTime hasta, string detalleCtaCte,
            string[] concepto, string[] origen, decimal[] cantidad, decimal[] valorUnitario)
        {
            if (!PuedeAdministrar()) return SinPermiso();

            var empleado = _oEmpleadoN.ObtenerPorId(idEmpleado, _empresa.IdEmpresa);
            if (empleado == null) return SinEmpleado();

            var persona = _oPersonaN.findById(empleado.Persona.idPersona);
            if (persona == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = "No se pudo resolver la persona del empleado.";
                return RedirectToAction("Nueva", new { idEmpleado });
            }

            var liquidacion = new Entidades.LiquidacionSueldo
            {
                IdEmpresa = _empresa.IdEmpresa,
                IdEmpleado = idEmpleado,
                PeriodoDesde = desde,
                PeriodoHasta = hasta
            };

            if (concepto != null)
            {
                for (int i = 0; i < concepto.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(concepto[i])) continue;

                    decimal cant = (cantidad != null && i < cantidad.Length && cantidad[i] != 0) ? cantidad[i] : 1;
                    decimal valor = (valorUnitario != null && i < valorUnitario.Length) ? valorUnitario[i] : 0;
                    var origenLinea = (origen != null && i < origen.Length
                        && Enum.TryParse<Entidades.LiquidacionSueldoDetalle.origenDetalle>(origen[i], out var origenParseado))
                        ? origenParseado
                        : Entidades.LiquidacionSueldoDetalle.origenDetalle.Manual;

                    liquidacion.Detalle.Add(new Entidades.LiquidacionSueldoDetalle
                    {
                        Concepto = concepto[i].Trim(),
                        Origen = origenLinea,
                        Cantidad = cant,
                        ValorUnitario = valor
                    });
                }
            }

            try
            {
                int idLiquidacion = _oLiquidacionN.Confirmar(liquidacion, persona, detalleCtaCte, _usuarioActual);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = $"Liquidación #{idLiquidacion} confirmada por ${liquidacion.TotalLiquidado:N2}.";
                return RedirectToAction("Index", new { idEmpleado });
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = ex.Message;
                return RedirectToAction("Nueva", new { idEmpleado });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id, int idEmpleado)
        {
            if (!PuedeAdministrar()) return SinPermiso();

            var liquidacion = _oLiquidacionN.ObtenerPorId(id, _empresa.IdEmpresa);
            if (liquidacion == null)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = "La liquidación no existe.";
                return RedirectToAction("Index", new { idEmpleado });
            }

            var empleado = _oEmpleadoN.ObtenerPorId(liquidacion.IdEmpleado, _empresa.IdEmpresa);
            var persona = empleado != null ? _oPersonaN.findById(empleado.Persona.idPersona) : null;

            try
            {
                _oLiquidacionN.Eliminar(id, _empresa.IdEmpresa, persona, _usuarioActual);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = "La liquidación se eliminó: se generó el asiento opuesto en la cuenta corriente y el período vuelve a estar disponible.";
            }
            catch (Exception ex)
            {
                TempData["AlertType"] = "error";
                TempData["AlertTitle"] = "Liquidaciones";
                TempData["AlertMsg"] = ex.Message;
            }

            return RedirectToAction("Index", new { idEmpleado });
        }

        private List<EmpleadoResumenVm> ListarEmpleadosResumen()
        {
            return _oEmpleadoN.Listar(_empresa.IdEmpresa, null, null, true)
                .Select(e => new EmpleadoResumenVm { Id = e.Id, RazonSocial = e.Persona?.razonSocial ?? "", Legajo = e.Legajo ?? "" })
                .ToList();
        }

        private IActionResult SinPermiso()
        {
            TempData["AlertType"] = "warning";
            TempData["AlertTitle"] = "Sin permiso";
            TempData["AlertMsg"] = "No tiene permisos para generar liquidaciones.";
            return RedirectToAction("Index");
        }

        private IActionResult SinEmpleado()
        {
            TempData["AlertType"] = "error";
            TempData["AlertTitle"] = "No encontrado";
            TempData["AlertMsg"] = "No se encontró el empleado seleccionado.";
            return RedirectToAction("Index");
        }

        private bool PuedeAdministrar()
        {
            if (_usuarioActual == null) return false;
            if (_usuarioActual.Admin) return true;
            if (_usuarioActual.Permisos == null || _usuarioActual.Permisos.Count == 0) return false;
            return _oUsuarioN.tienePermiso(_usuarioActual, Entidades.Permisos.Empleado.NuevaLiquidacion, DateTime.Today, _usuarioActual.Id);
        }
    }
}
