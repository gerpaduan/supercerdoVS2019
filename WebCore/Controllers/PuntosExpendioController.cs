// Port parcial de Web/Controllers/PuntosExpendioController.cs (1286 lineas, 20 acciones) para el
// Modulo 8 (Ventas y POS) -- ver docs/DECISIONS.md y docs/10-migracion-aspnet-core/README.md. Este
// slice porta SOLO: ExpendiosGenerados/ExpendiosGeneradosData (listado de solo lectura de
// expendios ya generados) y Sectores/GuardarSector/EliminarSector (catalogo simple de sectores,
// mismo perfil de riesgo que TiposEgresoCaja de Modulo 7 -- CRUD chico, sin dinero involucrado).
//
// Las 14 acciones restantes del original NO se portan en este slice:
//  - POS transaccional (crea una Venta/expendio real, acoplado al estado de caja de Modulo 7,
//    requiere su propio plan y juez de paridad antes de tocarlo, CLAUDE.md seccion 11.1):
//    Abrir, POS, AutorizarOperadorPOS, CerrarOperadorPOS, Guardar, FinalizarPOS,
//    BuscarProducto, BuscarProductoPorCodigo, BuscarProductoPOS, MisExpendiosPOS (este ultimo
//    ademas depende de ResolverOperadorPOS, infraestructura de POS que no existe sin Session).
//  - Impresion/PDF (mismo bloqueante de iTextSharp ya documentado en Modulo 7/8, CLAUDE.md
//    seccion 1.2): ImprimirTicket, ImprimirTicketPayload, DescargarAgenteImpresion, ImprimirPdf.
//  - Envio real de email (CLAUDE.md seccion 4): ObtenerDatosEmailExpendio, EnviarComprobanteEmailExpendio.
//
// Consecuencia visible en la vista: el boton "Imprimir" de cada card de ExpendiosGenerados (abre
// _ModalPostPuntoExpendio, 100% print/PDF/email) se excluye -- mismo criterio ya usado en
// VentasController para los botones de Factura/Imprimir/Email.
//
// Bypass de permisos, mismo criterio de toda la migracion: el usuario stub (Admin=true) hace que
// PermisosHelper.TienePermiso(Session, Permisos.Venta.NuevaVenta, ...) del original resuelva
// siempre "sin restriccion" -- se omite directamente en las 3 acciones portadas.
using Entidades;
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
    public class PuntosExpendioController : Controller
    {
        private sealed class StubEmpresaContext : IEmpresaContext
        {
            public int IdEmpresa => 1;
        }

        private readonly IEmpresaContext _empresa = new StubEmpresaContext();
        private readonly IParametrosContext _param;

        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Usuario _oUsuarioN;

        private readonly Entidades.Usuario _usuarioActual = new Entidades.Usuario
        {
            Id = 2,
            Admin = true,
            IdEmpresa = 1,
            IdSucursal = 2,
            Nombre = "ger"
        };

        public PuntosExpendioController()
        {
            _param = new Negocio.Parametros(_empresa);
            _param.Reload();

            _oVentaN = new Negocio.Venta(_empresa, _param);
            _oSucursalN = new Negocio.Sucursal(_empresa, _param);
            _oUsuarioN = new Negocio.Usuario(_empresa, _param);

            _usuarioActual.Sucursal = _oSucursalN.findById(_usuarioActual.IdSucursal);
        }

        public IActionResult ExpendiosGenerados()
        {
            var sectoresDt = _oVentaN.obtenerSectores();
            var sucursales = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            _oUsuarioN.obtenerUsuarios(true);
            var usuarios = (_oUsuarioN.listaUsuario() ?? new List<Entidades.Usuario>())
                .Where(x => x != null && x.Activo)
                .OrderBy(x => x.Nombre ?? "")
                .ToList();

            ViewBag.Title = "Expendios generados";
            ViewBag.FechaHoy = DateTime.Today.ToString("yyyy-MM-ddTHH:mm:ss");
            ViewBag.SectoresExpendio = (sectoresDt != null
                ? sectoresDt.AsEnumerable()
                    .Select(r => Convert.ToString(r["sector"] ?? ""))
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(s => s)
                    .Select(s => new SelectListItem { Value = s, Text = s })
                    .ToList()
                : new List<SelectListItem>());
            ViewBag.SucursalesExpendio = sucursales
                .Where(s => s != null && s.idSucursal > 0)
                .OrderBy(s => s.sucursal ?? "")
                .Select(s => new SelectListItem { Value = s.sucursal ?? "", Text = s.sucursal ?? "" })
                .ToList();
            ViewBag.UsuariosExpendio = usuarios
                .Where(u => u.Id > 0)
                .Select(u => new SelectListItem { Value = u.Nombre ?? "", Text = u.Nombre ?? "" })
                .ToList();

            return View("~/Views/PuntosExpendio/ExpendiosGenerados.cshtml");
        }

        [HttpGet]
        public IActionResult ExpendiosGeneradosData(string fechaDesde = null, string fechaHasta = null, int top = 300)
        {
            var user = _usuarioActual;
            if (user.IdSucursal == 0)
                return Json(new { ok = false, mensaje = "Sesión inválida o sucursal no seleccionada." });

            try
            {
                DateTime fechaDesdeValue;
                DateTime fechaHastaValue;
                DateTime? fechaDesdeFiltro = DateTime.TryParse(fechaDesde, out fechaDesdeValue) ? (DateTime?)fechaDesdeValue : null;
                DateTime? fechaHastaFiltro = null;
                if (DateTime.TryParse(fechaHasta, out fechaHastaValue))
                {
                    fechaHastaFiltro = fechaHastaValue.TimeOfDay == TimeSpan.Zero
                        ? fechaHastaValue.AddDays(1).AddSeconds(-1)
                        : fechaHastaValue;
                }
                DataTable dt = _oVentaN.obtenerExpendiosEmpresa(top <= 0 ? 300 : top, fechaDesdeFiltro, fechaHastaFiltro);

                var items = dt.AsEnumerable()
                    .Select(row =>
                    {
                        DateTime fechaExpendio = row["fechaExpendio"] != DBNull.Value
                            ? Convert.ToDateTime(row["fechaExpendio"])
                            : DateTime.MinValue;

                        int idExpendio = row["idExpendio"] != DBNull.Value ? Convert.ToInt32(row["idExpendio"]) : 0;
                        int idVenta = row["idVenta"] != DBNull.Value ? Convert.ToInt32(row["idVenta"]) : 0;
                        var expendio = idExpendio > 0 ? _oVentaN.getExpedioById(idExpendio) : null;
                        var lineas = (expendio != null ? expendio.LineasVenta : null) ?? new List<Entidades.LineaVenta>();

                        return new
                        {
                            fechaExpendio = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("yyyy-MM-ddTHH:mm:ss") : "",
                            fecha = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("dd/MM/yyyy") : "",
                            hora = fechaExpendio != DateTime.MinValue ? fechaExpendio.ToString("HH:mm") : "",
                            idExpendio = idExpendio,
                            identificacionExpendio = Convert.ToString(row["identificacionExpendio"] ?? ""),
                            sucursal = Convert.ToString(row["sucursal"] ?? ""),
                            sector = Convert.ToString(row["sector"] ?? ""),
                            usuario = Convert.ToString(row["vendedor"] ?? ""),
                            cantItems = Convert.ToString(row["cantItems"] ?? "0"),
                            totalKg = row["totalKg"] != DBNull.Value ? Convert.ToDecimal(row["totalKg"]) : 0m,
                            totalImporte = row["importe"] != DBNull.Value ? Convert.ToDecimal(row["importe"]) : 0m,
                            idVenta = idVenta,
                            estado = idVenta > 0 && idVenta != idExpendio ? "Asignado" : "Pendiente",
                            lineas = lineas.Select(l => new
                            {
                                codigo = l.Corte != null ? l.Corte.Codigo : 0,
                                producto = l.Corte != null
                                    ? (!string.IsNullOrWhiteSpace(l.Corte.corte) ? l.Corte.corte : l.Corte.CorteDesc)
                                    : "",
                                cantKg = l.CantKg,
                                precioKg = l.PrecioKg,
                                total = l.CantKg * l.PrecioKg
                            }).ToList()
                        };
                    })
                    .ToList();

                return Json(new { ok = true, items = items });
            }
            catch (Exception ex)
            {
                return Json(new { ok = false, mensaje = "No se pudieron consultar los expendios: " + ex.Message });
            }
        }

        public IActionResult Sectores(string editar = "")
        {
            var model = new SectorAbmVm
            {
                SectorOriginal = editar ?? "",
                Nombre = editar ?? ""
            };

            CargarSectores(model);
            ViewBag.Title = "Sectores";
            return View("~/Views/PuntosExpendio/Sectores.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GuardarSector(SectorAbmVm model)
        {
            string nombre = (model != null ? model.Nombre : "") ?? "";
            string nombreNormalizado = nombre.Trim();
            string sectorOriginal = (model != null ? model.SectorOriginal : "") ?? "";

            if (string.IsNullOrWhiteSpace(nombreNormalizado))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Debe ingresar un nombre de sector.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (_oVentaN.existeSector(nombreNormalizado, sectorOriginal))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "Ya existe otro sector con ese nombre en esta empresa.";
                return RedirectToAction("Sectores", new { editar = sectorOriginal });
            }

            if (string.IsNullOrWhiteSpace(sectorOriginal))
            {
                _oVentaN.agregarSector(nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se creó correctamente.";
            }
            else
            {
                _oVentaN.modificarSector(sectorOriginal, nombreNormalizado);
                TempData["AlertType"] = "success";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "El sector se actualizó correctamente.";
            }

            return RedirectToAction("Sectores");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarSector(string sector)
        {
            string nombre = (sector ?? "").Trim();
            if (string.IsNullOrWhiteSpace(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se recibió un sector válido para eliminar.";
                return RedirectToAction("Sectores");
            }

            if (_oVentaN.sectorEstaEnUso(nombre))
            {
                TempData["AlertType"] = "warning";
                TempData["AlertTitle"] = "Sectores";
                TempData["AlertMsg"] = "No se puede eliminar el sector porque está en uso en puntos de expendio.";
                return RedirectToAction("Sectores");
            }

            _oVentaN.eliminarSector(nombre);
            TempData["AlertType"] = "success";
            TempData["AlertTitle"] = "Sectores";
            TempData["AlertMsg"] = "El sector se eliminó correctamente.";
            return RedirectToAction("Sectores");
        }

        private List<string> ObtenerSectores()
        {
            DataTable dt = _oVentaN.obtenerSectores() ?? new DataTable();
            return dt.Rows
                .Cast<DataRow>()
                .Select(r => Convert.ToString(r["sector"] ?? "").Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(s => s)
                .ToList();
        }

        private void CargarSectores(SectorAbmVm model)
        {
            if (model == null)
                return;

            model.Sectores = ObtenerSectores()
                .Select(s => new SectorResumenVm
                {
                    Nombre = s,
                    EnUso = _oVentaN.sectorEstaEnUso(s)
                })
                .ToList();
        }
    }
}
