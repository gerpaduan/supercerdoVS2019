// Port de Web/Controllers/ReportesController.cs (ver docs/DECISIONS.md, migracion ASP.NET Core,
// Modulo 6 -- Reportes y administracion). Es el ultimo controller de este modulo, y el mas grande
// (1676 lineas en el original) -- de solo lectura (ninguna accion escribe datos), asi que no hace
// falta prueba de escritura en vivo, solo verificacion de que cada tipo de reporte trae datos reales.
//
// Usuario/empresa reales via IUsuarioSesionService (login real, ver docs/DECISIONS.md
// 2026-09-06). Permiso real portado 2026-09-09 (Batch B, ver docs/DECISIONS.md "Batch B:
// permisos reales + operador de produccion"): Permisos.Stock.VerStock via _oUsuarioN.tienePermiso
// (unico permiso que chequea este controller en clasico -- no existe Permisos.Reporte.*, los
// nombres "Stock Actual"/"Balance Economico"/etc. son tipos de reporte, no permisos). NO portado
// (deliberado, UX-only): AjustarFechaSiNoTienePermiso/ConfigurarAdvertenciaFechaEnVivo (recortan
// el filtro de fecha en silencio en vez de bloquear -- ver mismo criterio en MovimientosController.cs).
// ObtenerIdEmpresaSesion() (que en el original cae a Session["Usuario"] o a empresa) se simplifica
// a devolver directamente _empresa.IdEmpresa.
using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utilidades;
using WebCore.Models;

namespace WebCore.Controllers
{
    public class ReportesController : Controller
    {
        private const string TipoReporteStockActual = "Stock Actual";
        private const string TipoReporteCierreStock = "Cierre Stock";
        private const string TipoReporteStockRetroactivo = "Stock Retroactivo";
        private const string TipoReporteProyeccion = "Proyeccion Ventas vs Stock";
        private const string TipoReporteVentasProducto = "Ventas por Producto";
        private const string TipoReporteBalance = "Balance Economico";

        private readonly WebCore.Services.IUsuarioSesionService _sesion;
        private readonly IEmpresaContext _empresa;
        private readonly IParametrosContext _param;

        private readonly Negocio.Sucursal _oSucursalN;
        private readonly Negocio.Corte _oCorteN;
        private readonly Negocio.Compra _oCompraN;
        private readonly Negocio.Venta _oVentaN;
        private readonly Negocio.CierreCaja _oCierreN;
        private readonly Negocio.CuentaCorriente _oCuentaCorrienteN;
        private readonly Negocio.Usuario _oUsuarioN;

        private Entidades.Usuario _usuarioActual => _sesion.UsuarioActual;

        public ReportesController(WebCore.Services.IUsuarioSesionService sesion)
        {
            _sesion = sesion;
            _empresa = sesion.Empresa;
            _param = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
            _param.Reload();

            _oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, _param);
            _oCorteN = WebCore.Infrastructure.NegocioFactory.CrearCorte(_empresa, _param);
            _oCompraN = WebCore.Infrastructure.NegocioFactory.CrearCompra(_empresa, _param);
            _oVentaN = WebCore.Infrastructure.NegocioFactory.CrearVenta(_empresa, _param);
            _oCierreN = WebCore.Infrastructure.NegocioFactory.CrearCierreCaja(_empresa, _param);
            _oCuentaCorrienteN = WebCore.Infrastructure.NegocioFactory.CrearCuentaCorriente(_empresa, _param);
            _oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, _param);
        }

        private int ObtenerIdEmpresaSesion()
        {
            return _empresa.IdEmpresa;
        }

        [HttpGet]
        public IActionResult Index(
            string tipoReporte = TipoReporteStockActual,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            int[] idsSucursales = null,
            string busquedaProducto = "",
            string tipoProducto = "",
            int marcaId = 0,
            string agrupacionTemporal = "hora",
            string estadoStock = "Todos",
            string[] periodoComparativoDesde = null,
            bool incluirVentasCuentaCorrienteBalance = false,
            bool buscar = false)
        {
            var user = _usuarioActual;

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime hasta = fechaHasta ?? DateTime.Now;

            // Port de Web/Controllers/ReportesController.cs:79-84.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Stock.VerStock, desde, -1))
            {
                ViewBag.Seccion = "Reportes";
                return View("~/Views/Shared/AccesoDenegado.cshtml");
            }

            // Sucursales tildadas (idsSucursales) o, en enlaces viejos, la unica de idSucursal (0 = todas).
            // Los defaults de siempre (sucursal del usuario / "Todas" en Ventas por Producto) viven en el helper.
            var sucursalesEmpresa = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var sucursalesElegidas = ResolverSucursalesSeleccionadas(idsSucursales, idSucursal, tipoReporte, user, sucursalesEmpresa);
            int sucursalSeleccionada = sucursalesElegidas.Count == 1 ? sucursalesElegidas[0] : 0;

            var model = CrearModeloBase(
                user,
                tipoReporte,
                desde,
                hasta,
                sucursalSeleccionada,
                busquedaProducto,
                tipoProducto,
                marcaId,
                agrupacionTemporal,
                estadoStock,
                buscar,
                sucursalesElegidas,
                sucursalesEmpresa);

            model.IncluirVentasCuentaCorrienteBalance = incluirVentasCuentaCorrienteBalance;

            if (buscar)
            {
                switch ((tipoReporte ?? "").Trim())
                {
                    case TipoReporteStockActual:
                        CargarReporteStockDesdeCierres(model, true);
                        break;
                    case TipoReporteCierreStock:
                        CargarReporteStockDesdeCierres(model, false);
                        break;
                    case TipoReporteStockRetroactivo:
                        CargarReporteStockDesdeCierres(model, false);
                        break;
                    case TipoReporteVentasProducto:
                        CargarReporteVentasPorProducto(model, periodoComparativoDesde);
                        break;
                    case TipoReporteProyeccion:
                        CargarReporteProyeccionVentasVsStock(model, periodoComparativoDesde);
                        break;
                    case TipoReporteBalance:
                        CargarReporteBalanceEconomico(model, periodoComparativoDesde);
                        break;
                    default:
                        model.ConsultaRealizada = true;
                        model.HayResultados = false;
                        model.Mensaje = "El reporte seleccionado ya quedó preparado en la nueva pantalla, pero todavía no está implementado en Web.";
                        break;
                }
            }
            else
            {
                model.Mensaje = "Se modificaron filtros principales. Presioná Buscar para actualizar el reporte.";
            }

            ViewBag.Title = "Reportes";
            ViewBag.Seccion = "Reportes";

            return View("~/Views/Reportes/Index.cshtml", model);
        }

        [HttpGet]
        public PartialViewResult FiltrosSecundarios(
            string tipoReporte = TipoReporteStockActual,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            int[] idsSucursales = null,
            string busquedaProducto = "",
            string tipoProducto = "",
            int marcaId = 0,
            string agrupacionTemporal = "hora",
            string estadoStock = "Todos",
            bool incluirVentasCuentaCorrienteBalance = false)
        {
            var user = _usuarioActual;

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime hasta = fechaHasta ?? DateTime.Now;

            // Port de Web/Controllers/ReportesController.cs:180-185 -- fallo distinto de Index:
            // sin permiso, no bloquea con AccesoDenegado, devuelve la parcial vacia.
            if (!_oUsuarioN.tienePermiso(user, Entidades.Permisos.Stock.VerStock, desde, -1))
                return PartialView("~/Views/Reportes/_FiltrosSecundarios.cshtml", new ReportesViewModel());

            // Misma resolucion de sucursales que Index (la parcial solo la usa para armar el modelo base).
            var sucursalesEmpresa = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var sucursalesElegidas = ResolverSucursalesSeleccionadas(idsSucursales, idSucursal, tipoReporte, user, sucursalesEmpresa);
            int sucursalSeleccionada = sucursalesElegidas.Count == 1 ? sucursalesElegidas[0] : 0;

            var model = CrearModeloBase(
                user,
                tipoReporte,
                desde,
                hasta,
                sucursalSeleccionada,
                busquedaProducto,
                tipoProducto,
                marcaId,
                agrupacionTemporal,
                estadoStock,
                buscar: false,
                sucursalesIds: sucursalesElegidas,
                sucursalesEmpresa: sucursalesEmpresa);

            model.IncluirVentasCuentaCorrienteBalance = incluirVentasCuentaCorrienteBalance;

            return PartialView("~/Views/Reportes/_FiltrosSecundarios.cshtml", model);
        }

        [HttpGet]
        public JsonResult VentasPorProductoSerie(
            int idCorte,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int idSucursal = 0,
            string tipoProducto = "",
            int marcaId = 0,
            string agrupacionTemporal = "hora",
            string[] periodoComparativoDesde = null,
            int[] idsSucursales = null)
        {
            // Una sucursal o todas: la consulta de siempre (una sola). Varias tildadas: una por sucursal y se suman
            // las series punto a punto (los buckets del periodo son los mismos para todas).
            var idsConsulta = ResolverIdsConsultaSerie(idSucursal, idsSucursales);
            var periodos = ConstruirPeriodosComparativosVentas(fechaDesde, fechaHasta, periodoComparativoDesde);
            var agrupacionAplicada = ObtenerAgrupacionVentasComparativa(periodos.FirstOrDefault());
            var labels = ConstruirLabelsSerie(periodos.First(), agrupacionAplicada);
            var colores = new[]
            {
                "#2563eb", "#dc2626", "#16a34a", "#9333ea", "#ea580c", "#0891b2"
            };
            var series = new List<object>();
            var seriesInternas = new List<SerieVentasComparativaPeriodoInfo>();

            decimal totalPrincipal = 0m;

            for (int i = 0; i < periodos.Count; i++)
            {
                var periodo = periodos[i];
                List<SerieVentasComparativaPunto> serie = null;

                foreach (int idSucursalConsulta in idsConsulta)
                {
                    var dt = _oCorteN.ObtenerSerieVentasPorCorte(
                        idCorte,
                        idSucursalConsulta,
                        periodo.FechaDesde,
                        periodo.FechaHasta,
                        tipoProducto,
                        marcaId,
                        agrupacionAplicada);

                    var seriePeriodoSucursal = CompletarSeriePeriodo(periodo, dt, agrupacionAplicada);
                    if (serie == null)
                    {
                        serie = seriePeriodoSucursal;
                    }
                    else
                    {
                        // Misma cantidad de puntos (los arma el periodo, no los datos): se suma por posicion.
                        for (int p = 0; p < serie.Count && p < seriePeriodoSucursal.Count; p++)
                        {
                            serie[p].Kg += seriePeriodoSucursal[p].Kg;
                            serie[p].Importe += seriePeriodoSucursal[p].Importe;
                        }
                    }
                }

                if (periodo.EsPrincipal)
                    totalPrincipal = serie.Sum(x => x.Kg);

                seriesInternas.Add(new SerieVentasComparativaPeriodoInfo
                {
                    Periodo = periodo,
                    Color = colores[i % colores.Length],
                    Puntos = serie
                });
            }

            foreach (var serie in seriesInternas)
            {
                var totalKg = serie.Puntos.Sum(x => x.Kg);
                var promedioKg = serie.Puntos.Count > 0 ? totalKg / serie.Puntos.Count : 0m;
                var mejorPunto = serie.Puntos
                    .OrderByDescending(x => x.Kg)
                    .ThenBy(x => x.Indice)
                    .FirstOrDefault();
                var diferenciaKg = totalKg - totalPrincipal;
                decimal? diferenciaPorcentual = totalPrincipal != 0m
                    ? (decimal?)((diferenciaKg / totalPrincipal) * 100m)
                    : null;

                series.Add(new
                {
                    id = serie.Periodo.Id,
                    titulo = serie.Periodo.Titulo,
                    rangoCorto = string.Format(
                        CultureInfo.InvariantCulture,
                        "{0} - {1}",
                        serie.Periodo.FechaDesde.ToString("dd/MM HH:mm"),
                        serie.Periodo.FechaHasta.ToString("dd/MM HH:mm")),
                    esPrincipal = serie.Periodo.EsPrincipal,
                    color = serie.Color,
                    totalKg,
                    promedioKg,
                    mejor = mejorPunto == null
                        ? null
                        : new
                        {
                            etiquetaRelativa = mejorPunto.EtiquetaRelativa,
                            fechaReal = mejorPunto.FechaReal.ToString("dd/MM/yyyy HH:mm"),
                            kg = mejorPunto.Kg
                        },
                    diferenciaKg,
                    diferenciaPorcentual,
                    puntos = serie.Puntos.Select(x => new
                    {
                        indice = x.Indice,
                        etiquetaRelativa = x.EtiquetaRelativa,
                        fechaReal = x.FechaReal.ToString("dd/MM/yyyy HH:mm"),
                        kg = x.Kg,
                        importe = x.Importe
                    }).ToList()
                });
            }

            return Json(new
            {
                ok = true,
                labels,
                agrupacionAplicada,
                promedioLabel = string.Equals(agrupacionAplicada, "dia", StringComparison.OrdinalIgnoreCase)
                    ? "Promedio diario"
                    : "Promedio por hora",
                series
            });
        }

        private ReportesViewModel CrearModeloBase(
            Entidades.Usuario user,
            string tipoReporte,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int sucursalId,
            string busquedaProducto,
            string tipoProducto,
            int marcaId,
            string agrupacionTemporal,
            string estadoStock,
            bool buscar,
            List<int> sucursalesIds = null,
            List<Entidades.Sucursal> sucursalesEmpresa = null)
        {
            var model = new ReportesViewModel
            {
                TipoReporteSeleccionado = string.IsNullOrWhiteSpace(tipoReporte) ? TipoReporteStockActual : tipoReporte,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta,
                SucursalId = sucursalId,
                BusquedaProducto = busquedaProducto ?? "",
                TipoProducto = tipoProducto ?? "",
                MarcaId = marcaId,
                AgrupacionTemporal = string.IsNullOrWhiteSpace(agrupacionTemporal) ? "hora" : agrupacionTemporal.Trim().ToLowerInvariant(),
                EstadoStock = string.IsNullOrWhiteSpace(estadoStock) ? "Todos" : estadoStock,
                Buscar = buscar,
                NotaReporte = "Los filtros secundarios se aplican en vivo sobre los datos ya cargados. Los cierres se adaptan según el tipo de reporte, siguiendo el comportamiento del sistema de escritorio."
            };

            model.TiposReporte = new List<SelectListItem>
            {
                CrearItemReporte(TipoReporteStockActual, model.TipoReporteSeleccionado),
                CrearItemReporte(TipoReporteCierreStock, model.TipoReporteSeleccionado),
                CrearItemReporte(TipoReporteStockRetroactivo, model.TipoReporteSeleccionado),
                CrearItemReporte(TipoReporteProyeccion, model.TipoReporteSeleccionado),
                CrearItemReporte(TipoReporteVentasProducto, model.TipoReporteSeleccionado),
                CrearItemReporte(TipoReporteBalance, model.TipoReporteSeleccionado)
            };

            bool requiereCatalogoCortes =
                string.Equals(model.TipoReporteSeleccionado, TipoReporteStockActual, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteCierreStock, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteStockRetroactivo, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteProyeccion, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteVentasProducto, StringComparison.OrdinalIgnoreCase);

            int idEmpresaCatalogo = ObtenerIdEmpresaSesion();
            var cortes = requiereCatalogoCortes
                ? (idEmpresaCatalogo > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaCatalogo, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>()))
                : new List<Entidades.Corte>();
            var sucursales = sucursalesEmpresa ?? _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            model.CierresDisponibles = ObtenerCierresDisponibles();

            var sucursalesOrdenadas = sucursales
                .Where(x => x != null && x.IdSucursal > 0)
                .GroupBy(x => x.IdSucursal)
                .Select(g => g.First())
                .OrderBy(x => x.SucursalNombre)
                .ToList();

            // Sucursales tildadas: siempre una lista explicita (con "Todas" van todas las de la empresa). Sin pedido
            // explicito se deriva de la sucursal unica de siempre (0 = todas).
            model.SucursalesIds = (sucursalesIds != null && sucursalesIds.Count > 0)
                ? sucursalesIds.ToList()
                : (sucursalId > 0
                    ? new List<int> { sucursalId }
                    : sucursalesOrdenadas.Select(x => x.IdSucursal).ToList());
            model.EsTodasLasSucursales = sucursalesOrdenadas.Count > 0 &&
                sucursalesOrdenadas.All(x => model.SucursalesIds.Contains(x.IdSucursal));
            // SucursalId conserva su significado de siempre: la sucursal si hay exactamente una, si no 0.
            model.SucursalId = model.SucursalesIds.Count == 1 ? model.SucursalesIds[0] : 0;

            model.Sucursales.Add(new SelectListItem
            {
                Value = "0",
                Text = "Todas",
                Selected = model.EsTodasLasSucursales
            });

            foreach (var sucursal in sucursalesOrdenadas)
            {
                model.Sucursales.Add(new SelectListItem
                {
                    Value = sucursal.IdSucursal.ToString(),
                    Text = sucursal.SucursalNombre,
                    Selected = model.SucursalesIds.Contains(sucursal.IdSucursal)
                });
            }

            model.Marcas.Add(new SelectListItem
            {
                Value = "0",
                Text = "Todas",
                Selected = model.MarcaId <= 0
            });

            foreach (var marca in cortes
                .Where(x => x != null && x.Marca != null && x.Marca.IdPersona > 0 && !string.IsNullOrWhiteSpace(x.Marca.RazonSocial))
                .GroupBy(x => x.Marca.IdPersona)
                .Select(g => g.First())
                .OrderBy(x => x.Marca.RazonSocial))
            {
                model.Marcas.Add(new SelectListItem
                {
                    Value = marca.Marca.IdPersona.ToString(),
                    Text = marca.Marca.RazonSocial,
                    Selected = marca.Marca.IdPersona == model.MarcaId
                });
            }

            model.TiposProducto.Add(new SelectListItem
            {
                Value = "",
                Text = "Todos",
                Selected = string.IsNullOrWhiteSpace(model.TipoProducto)
            });

            foreach (var tipo in cortes
                .Select(x => x != null ? (x.Tipo ?? "").Trim() : "")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x))
            {
                model.TiposProducto.Add(new SelectListItem
                {
                    Value = tipo,
                    Text = tipo,
                    Selected = string.Equals(tipo, model.TipoProducto, StringComparison.OrdinalIgnoreCase)
                });
            }

            foreach (var estado in new[] { "Todos", "OK", "BAJO", "SIN STOCK", "NEGATIVO" })
            {
                model.EstadosStock.Add(new SelectListItem
                {
                    Value = estado,
                    Text = estado,
                    Selected = string.Equals(estado, model.EstadoStock, StringComparison.OrdinalIgnoreCase)
                });
            }

            foreach (var agrupacion in new[] { "hora", "dia" })
            {
                model.AgrupacionesTemporales.Add(new SelectListItem
                {
                    Value = agrupacion,
                    Text = agrupacion == "dia" ? "Por dia" : "Por hora",
                    Selected = string.Equals(agrupacion, model.AgrupacionTemporal, StringComparison.OrdinalIgnoreCase)
                });
            }

            ConfigurarFiltrosDinamicos(model);
            AplicarConfiguracionFechasSegunReporte(model);
            return model;
        }

        private void ConfigurarFiltrosDinamicos(ReportesViewModel model)
        {
            bool esReporteStock =
                string.Equals(model.TipoReporteSeleccionado, TipoReporteStockActual, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteCierreStock, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(model.TipoReporteSeleccionado, TipoReporteStockRetroactivo, StringComparison.OrdinalIgnoreCase);
            bool esReporteVentasProducto =
                string.Equals(model.TipoReporteSeleccionado, TipoReporteVentasProducto, StringComparison.OrdinalIgnoreCase);
            bool esReporteProyeccion =
                string.Equals(model.TipoReporteSeleccionado, TipoReporteProyeccion, StringComparison.OrdinalIgnoreCase);
            bool esReporteBalance =
                string.Equals(model.TipoReporteSeleccionado, TipoReporteBalance, StringComparison.OrdinalIgnoreCase);

            // Stock con 2+ sucursales: cada una con SUS cierres (no hay una sola fecha-desde/hasta para todas).
            model.UsaCierresPorSucursal = esReporteStock && model.SucursalesIds.Count >= 2;

            model.MostrarFiltroBusquedaProducto = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroTipoProducto = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroMarca = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroEstadoStock = esReporteStock;
            model.MostrarFiltroAgrupacionTemporal = esReporteVentasProducto;
            model.MostrarGrafico = esReporteBalance;
        }

        // Resuelve que sucursales estan elegidas: las tildadas (idsSucursales) o, en enlaces anteriores al selector
        // multiple, la unica de idSucursal (0 = todas). Sin pedido valido se conservan los defaults de siempre: la
        // sucursal del usuario ("Todas" en Ventas por Producto; en los de stock siempre alguna concreta).
        // Devuelve ids de ESTA empresa, en el orden del selector (por nombre); un id ajeno se descarta (input externo).
        private List<int> ResolverSucursalesSeleccionadas(
            int[] idsSucursales,
            int? idSucursal,
            string tipoReporte,
            Entidades.Usuario user,
            List<Entidades.Sucursal> sucursalesEmpresa)
        {
            var validas = (sucursalesEmpresa ?? new List<Entidades.Sucursal>())
                .Where(x => x != null && x.IdSucursal > 0)
                .GroupBy(x => x.IdSucursal)
                .Select(g => g.First())
                .OrderBy(x => x.SucursalNombre)
                .Select(x => x.IdSucursal)
                .ToList();

            List<int> elegidas = null;
            if (idsSucursales != null && idsSucursales.Length > 0)
            {
                elegidas = validas.Where(id => idsSucursales.Contains(id)).ToList();
            }
            else if (idSucursal.HasValue)
            {
                elegidas = idSucursal.Value > 0
                    ? validas.Where(id => id == idSucursal.Value).ToList()
                    : validas.ToList();
            }

            if (elegidas != null && elegidas.Count > 0)
                return elegidas;

            // Defaults de siempre (no se pidio nada, o lo pedido no es de esta empresa).
            if (string.Equals(tipoReporte, TipoReporteVentasProducto, StringComparison.OrdinalIgnoreCase))
                return validas;

            int porDefecto = AjustarSucursalSegunReporte(tipoReporte, user != null && user.IdSucursal > 0 ? user.IdSucursal : 0, user);
            return porDefecto > 0 ? new List<int> { porDefecto } : validas;
        }

        // Ids sobre los que se consulta en los reportes que suman por sucursal (Ventas por Producto, Balance).
        // Una sucursal -> [id]; todas -> [0] (UNA consulta, igual que antes de existir la seleccion multiple);
        // varias (no todas) -> la lista, y quien llama recorre y suma.
        private static List<int> IdsParaConsulta(ReportesViewModel model)
        {
            var ids = model.SucursalesIds ?? new List<int>();
            if (ids.Count == 1)
                return new List<int> { ids[0] };

            if (ids.Count == 0 || model.EsTodasLasSucursales)
                return new List<int> { 0 };

            return ids.ToList();
        }

        // Igual que IdsParaConsulta pero para el endpoint del grafico, que recibe los ids crudos del navegador.
        private List<int> ResolverIdsConsultaSerie(int idSucursal, int[] idsSucursales)
        {
            if (idsSucursales == null || idsSucursales.Length < 2)
                return new List<int> { idSucursal };

            var validas = (_oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .Where(x => x != null && x.IdSucursal > 0)
                .Select(x => x.IdSucursal)
                .Distinct()
                .ToList();
            var elegidas = validas.Where(id => idsSucursales.Contains(id)).ToList();

            if (elegidas.Count == 0)
                return new List<int> { idSucursal };

            if (elegidas.Count == 1)
                return elegidas;

            return elegidas.Count >= validas.Count
                ? new List<int> { 0 }
                : elegidas;
        }

        private int AjustarSucursalSegunReporte(string tipoReporte, int sucursalSeleccionada, Entidades.Usuario user)
        {
            bool requiereSucursalEspecifica =
                string.Equals(tipoReporte, TipoReporteStockActual, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tipoReporte, TipoReporteCierreStock, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tipoReporte, TipoReporteStockRetroactivo, StringComparison.OrdinalIgnoreCase);

            if (!requiereSucursalEspecifica || sucursalSeleccionada > 0)
                return sucursalSeleccionada;

            if (user != null && user.IdSucursal > 0)
                return user.IdSucursal;

            var sucursal = (_oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
                .FirstOrDefault(x => x != null && x.IdSucursal > 0);

            return sucursal != null ? sucursal.IdSucursal : 0;
        }

        private void AplicarConfiguracionFechasSegunReporte(ReportesViewModel model)
        {
            var cierresFiltrados = FiltrarCierresPorSucursal(model.CierresDisponibles, model.SucursalId);
            var ultimoCierre = cierresFiltrados.FirstOrDefault();
            var anteUltimoCierre = cierresFiltrados.Skip(1).FirstOrDefault() ?? ultimoCierre;
            bool esStockActual = string.Equals(model.TipoReporteSeleccionado, TipoReporteStockActual, StringComparison.OrdinalIgnoreCase);
            bool esCierreStock = string.Equals(model.TipoReporteSeleccionado, TipoReporteCierreStock, StringComparison.OrdinalIgnoreCase);
            bool esStockRetroactivo = string.Equals(model.TipoReporteSeleccionado, TipoReporteStockRetroactivo, StringComparison.OrdinalIgnoreCase);

            model.UsaCierreEnFechaDesde = esStockActual || esCierreStock || esStockRetroactivo;
            model.UsaCierreEnFechaHasta = esCierreStock;

            if (esStockActual)
            {
                if (ultimoCierre != null && !RequestTieneFecha("fechaDesde"))
                    model.FechaDesde = ultimoCierre.FechaCompra;

                if (!RequestTieneFecha("fechaHasta"))
                    model.FechaHasta = DateTime.Now;
            }
            else if (esCierreStock)
            {
                if (anteUltimoCierre != null && !RequestTieneFecha("fechaDesde"))
                    model.FechaDesde = anteUltimoCierre.FechaCompra;

                if (ultimoCierre != null && !RequestTieneFecha("fechaHasta"))
                    model.FechaHasta = ultimoCierre.FechaCompra;
            }
            else if (esStockRetroactivo)
            {
                if (ultimoCierre != null && !RequestTieneFecha("fechaDesde"))
                    model.FechaDesde = ultimoCierre.FechaCompra;

                if (!RequestTieneFecha("fechaHasta"))
                    model.FechaHasta = DateTime.Now;
            }

            model.FechaDesdeValor = model.FechaDesde.ToString("yyyy-MM-ddTHH:mm");
            model.FechaHastaValor = model.FechaHasta.ToString("yyyy-MM-ddTHH:mm");
        }

        private void CargarReporteStockDesdeCierres(ReportesViewModel model, bool graficarTop)
        {
            model.ConsultaRealizada = true;
            model.FilasStockActual.Clear();
            model.Avisos.Clear();

            int idEmpresaCatalogo = ObtenerIdEmpresaSesion();
            var cortes = (idEmpresaCatalogo > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaCatalogo, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>()))
                .Where(x => x != null)
                .GroupBy(x => x.IdCorte)
                .ToDictionary(g => g.Key, g => g.First());

            if (!model.UsaCierresPorSucursal)
            {
                // Una sola sucursal: lo de siempre, con las fechas elegidas en pantalla (desde/hasta de ESA sucursal).
                AgregarFilasStockSucursal(
                    model,
                    model.SucursalId > 0 ? model.SucursalId : 0,
                    model.SucursalId,
                    model.FechaDesde,
                    model.FechaHasta,
                    cortes,
                    true);
            }
            else
            {
                // Varias sucursales: cada una se calcula con SUS cierres, porque el stock inicial de una sucursal es
                // el de su cierre y una fecha-desde unica no sirve para todas (los cierres no coinciden):
                //  - Cierre Stock:       anteultimo -> ultimo cierre de la sucursal.
                //  - Stock Actual:       ultimo cierre -> ahora.
                //  - Stock Retroactivo:  ultimo cierre -> la fecha "hasta" elegida.
                bool esCierreStock = string.Equals(model.TipoReporteSeleccionado, TipoReporteCierreStock, StringComparison.OrdinalIgnoreCase);
                bool esStockActual = string.Equals(model.TipoReporteSeleccionado, TipoReporteStockActual, StringComparison.OrdinalIgnoreCase);
                var nombresSucursal = model.Sucursales
                    .Where(x => x.Value != "0")
                    .ToDictionary(x => int.Parse(x.Value, CultureInfo.InvariantCulture), x => x.Text);

                foreach (int idSucursal in model.SucursalesIds)
                {
                    var cierresSucursal = FiltrarCierresPorSucursal(model.CierresDisponibles, idSucursal);
                    var ultimoCierre = cierresSucursal.FirstOrDefault();
                    string nombreSucursal;
                    if (!nombresSucursal.TryGetValue(idSucursal, out nombreSucursal))
                        nombreSucursal = "Sucursal " + idSucursal.ToString(CultureInfo.InvariantCulture);

                    if (ultimoCierre == null)
                    {
                        model.Avisos.Add(nombreSucursal + ": no tiene cierres de stock registrados, no se incluye en el reporte.");
                        continue;
                    }

                    // Misma regla que la pantalla de una sola sucursal: sin anteultimo se usa el ultimo.
                    var anteUltimoCierre = cierresSucursal.Skip(1).FirstOrDefault() ?? ultimoCierre;

                    DateTime desdeSucursal = esCierreStock ? anteUltimoCierre.FechaCompra : ultimoCierre.FechaCompra;
                    DateTime hastaSucursal = esCierreStock
                        ? ultimoCierre.FechaCompra
                        : (esStockActual ? DateTime.Now : model.FechaHasta);

                    // El "Stock Cierre" (segunda consulta) solo se muestra en Cierre Stock: en los otros no se pide.
                    AgregarFilasStockSucursal(model, idSucursal, idSucursal, desdeSucursal, hastaSucursal, cortes, esCierreStock);
                }
            }

            if (model.FilasStockActual.Count == 0)
            {
                model.HayResultados = false;
                model.Mensaje = "No se encontraron datos para los filtros principales seleccionados.";
                return;
            }

            model.FilasStockActual = model.FilasStockActual
                .OrderBy(x => x.Codigo)
                .ThenBy(x => x.Producto)
                .ThenBy(x => x.Sucursal)
                .ToList();

            model.HayResultados = model.FilasStockActual.Count > 0;
            if (!model.HayResultados)
            {
                model.Mensaje = "No se encontraron datos para los filtros principales seleccionados.";
                return;
            }

            model.Totales.TotalKg = model.FilasStockActual.Sum(x => x.StockActual);
            model.Totales.TotalIngresos = model.FilasStockActual.Sum(x => x.TotalIngresos);
            model.Totales.TotalEgresos = model.FilasStockActual.Sum(x => x.TotalEgresos);
            model.Totales.CantidadProductos = model.FilasStockActual.Select(x => x.IdCorte).Distinct().Count();
            model.Totales.CantidadRegistros = model.FilasStockActual.Count;
            model.Mensaje = model.UsaCierresPorSucursal
                ? "Reporte cargado. Cada sucursal se calculó con sus propios cierres de stock."
                : "Reporte cargado. Los filtros secundarios se aplican en vivo sobre los datos visibles.";
            model.MostrarGrafico = false;
        }

        // Trae el stock de UNA sucursal (CierreStockWeb) para el rango desde-hasta y agrega sus filas a
        // model.FilasStockActual. idSucursalConsulta: 0 = todas (solo el caso de siempre sin sucursales);
        // idSucursalFila: el id que se guarda en cada fila. traerStockCierre: segunda consulta (hasta..hasta) que
        // alimenta la columna "Stock Cierre"; en el caso de una sola sucursal se pide siempre, como siempre.
        private void AgregarFilasStockSucursal(
            ReportesViewModel model,
            int idSucursalConsulta,
            int idSucursalFila,
            DateTime desde,
            DateTime hasta,
            Dictionary<int, Entidades.Corte> cortes,
            bool traerStockCierre)
        {
            var dt = _oCorteN.CierreStockWeb(
                "",
                idSucursalConsulta > 0 ? idSucursalConsulta : 0,
                desde,
                hasta,
                "",
                0,
                0);

            // Clave por codigo dentro de ESTA sucursal: cada llamada tiene su propio diccionario, asi el stock al
            // cierre de una sucursal nunca se mezcla con el de otra.
            var stockCierrePorCorte = new Dictionary<long, decimal>();
            if (traerStockCierre)
            {
                var dtStockCierre = _oCorteN.CierreStockWeb(
                    "",
                    idSucursalConsulta > 0 ? idSucursalConsulta : 0,
                    hasta,
                    hasta,
                    "",
                    0,
                    0);

                if (dtStockCierre != null)
                {
                    foreach (System.Data.DataRow row in dtStockCierre.Rows)
                    {
                        long codigo = LeerLong(row, "Codigo");
                        if (codigo > 0)
                        {
                            decimal stockCierre = LeerDecimal(row, "Faltante");
                            stockCierrePorCorte[codigo] = stockCierre;
                        }
                    }
                }
            }

            if (dt == null || dt.Rows.Count == 0)
                return;

            bool tieneColumnaStockCierre = dt.Columns.Contains("Stock.Cierre");

            foreach (System.Data.DataRow row in dt.Rows)
            {
                int idCorte = LeerInt(row, "idCorte");
                Entidades.Corte corteMeta = null;
                cortes.TryGetValue(idCorte, out corteMeta);

                decimal stockActual = LeerDecimal(row, "Faltante");
                decimal puntoStock = LeerDecimal(row, "Pto.Stock");
                string estado = CalcularEstadoStock(stockActual, puntoStock);

                long codigo = LeerLong(row, "Codigo");
                decimal stockCierre = tieneColumnaStockCierre
                    ? LeerDecimal(row, "Stock.Cierre")
                    : 0m;

                if (stockCierre == 0m && codigo > 0 && stockCierrePorCorte.TryGetValue(codigo, out var sc))
                {
                    stockCierre = sc;
                }

                model.FilasStockActual.Add(new ReporteStockFilaVm
                {
                    IdCorte = idCorte,
                    Codigo = codigo,
                    Producto = LeerString(row, "Corte"),
                    IdSucursal = idSucursalFila,
                    Sucursal = LeerString(row, "Sucursal"),
                    TipoProducto = corteMeta != null ? (corteMeta.Tipo ?? "") : "",
                    MarcaId = corteMeta != null && corteMeta.Marca != null ? corteMeta.Marca.IdPersona : 0,
                    Marca = corteMeta != null ? (corteMeta.MarcaNombre ?? "") : "",
                    FechaUltimoCierre = desde,
                    FechaHastaConsulta = hasta,
                    StockInicial = LeerDecimal(row, "Stock.Ini"),
                    Compras = LeerDecimal(row, "Compras"),
                    IngresoElaborado = LeerDecimal(row, "Ingr.Elab"),
                    IngresoStock = LeerDecimal(row, "Ingr.Stock"),
                    IngresoMovimiento = LeerDecimal(row, "Ingr. Mov"),
                    AjusteStock = LeerDecimal(row, "Ajus.Stock"),
                    TotalIngresos = LeerDecimal(row, "Tot.INGR"),
                    EgresoStock = LeerDecimal(row, "Egr.Stock"),
                    EgresoMovimiento = LeerDecimal(row, "Egr.Mov"),
                    EgresoElaborado = LeerDecimal(row, "Egr.Elab"),
                    Ventas = LeerDecimal(row, "Ventas"),
                    TotalEgresos = LeerDecimal(row, "Tot.EGR"),
                    StockActual = stockActual,
                    StockCierre = stockCierre,
                    Promedio = LeerDecimal(row, "promedio"),
                    PuntoStock = puntoStock,
                    Pesable = LeerBool(row, "Pesable"),
                    EstadoStock = estado
                });
            }
        }

        private void CargarReporteVentasPorProducto(ReportesViewModel model, string[] periodoComparativoDesde)
        {
            model.ConsultaRealizada = true;
            model.FilasVentasProducto.Clear();
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(model.FechaDesde, model.FechaHasta, periodoComparativoDesde);

            int idEmpresaCatalogo = ObtenerIdEmpresaSesion();
            var cortes = (idEmpresaCatalogo > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaCatalogo, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>()))
                .Where(x => x != null)
                .GroupBy(x => x.IdCorte)
                .ToDictionary(g => g.Key, g => g.First());
            var cortesPorCodigo = cortes.Values
                .Where(x => x.codigo > 0)
                .GroupBy(x => x.codigo)
                .ToDictionary(g => g.Key, g => g.First());
            var cortesPorNombre = cortes.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.corte))
                .GroupBy(x => NormalizarClaveProducto(x.corte))
                .ToDictionary(g => g.Key, g => g.First());

            var filas = new Dictionary<string, ReporteVentasProductoFilaVm>(StringComparer.OrdinalIgnoreCase);

            // Una sucursal -> [id]; todas -> [0] (una consulta, como siempre); varias -> una consulta por sucursal.
            var idsConsulta = IdsParaConsulta(model);

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                for (int indiceConsulta = 0; indiceConsulta < idsConsulta.Count; indiceConsulta++)
                {
                    var dt = _oCorteN.TotalPorCortesVendidos(
                        model.BusquedaProducto,
                        idsConsulta[indiceConsulta],
                        periodo.FechaDesde,
                        periodo.FechaHasta,
                        model.TipoProducto,
                        0,
                        model.MarcaId);

                    if (dt == null)
                        continue;

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        long codigo = LeerLongPrimeraCoincidencia(row, "Codigo", "codigo");
                        string producto = LeerStringPrimeraCoincidencia(row, "Corte", "Producto", "corte");
                        int idCorte = LeerIntPrimeraCoincidencia(row, "idCorte", "IdCorte");
                        Entidades.Corte corteMeta = null;

                        if (idCorte > 0)
                            cortes.TryGetValue(idCorte, out corteMeta);

                        if (corteMeta == null && codigo > 0)
                        {
                            cortesPorCodigo.TryGetValue(codigo, out corteMeta);
                            if (corteMeta != null)
                                idCorte = corteMeta.IdCorte;
                        }

                        if (corteMeta == null && !string.IsNullOrWhiteSpace(producto))
                        {
                            cortesPorNombre.TryGetValue(NormalizarClaveProducto(producto), out corteMeta);
                            if (corteMeta != null)
                                idCorte = corteMeta.IdCorte;
                        }

                        string productoNormalizado = !string.IsNullOrWhiteSpace(producto)
                            ? producto
                            : corteMeta != null
                                ? (!string.IsNullOrWhiteSpace(corteMeta.CorteDesc) ? corteMeta.CorteDesc : (corteMeta.corte ?? ""))
                                : "";

                        var filaKey = idCorte > 0
                            ? "ID:" + idCorte.ToString(CultureInfo.InvariantCulture)
                            : codigo > 0
                                ? "COD:" + codigo.ToString(CultureInfo.InvariantCulture)
                                : "NOM:" + NormalizarClaveProducto(productoNormalizado);

                        if (string.Equals(filaKey, "NOM:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        ReporteVentasProductoFilaVm fila;
                        if (!filas.TryGetValue(filaKey, out fila))
                        {
                            fila = new ReporteVentasProductoFilaVm
                            {
                                IdCorte = idCorte,
                                Codigo = codigo,
                                Producto = productoNormalizado,
                                TipoProducto = corteMeta != null ? (corteMeta.Tipo ?? "") : "",
                                MarcaId = corteMeta != null && corteMeta.Marca != null ? corteMeta.Marca.IdPersona : 0,
                                Marca = corteMeta != null ? (corteMeta.MarcaNombre ?? "") : ""
                            };
                            filas[filaKey] = fila;
                        }

                        if (string.IsNullOrWhiteSpace(fila.Producto))
                            fila.Producto = productoNormalizado;

                        decimal kg = LeerDecimalPrimeraCoincidencia(row, "Total Kgs", "Total Kg", "Kgs", "Kg", "cantKg", "CantKg");
                        decimal importe = LeerDecimalPrimeraCoincidencia(row, "totalS", "Total $", "Total", "Importe", "Total Importe");

                        // Con varias sucursales, un producto que ya trajo otra sucursal en este periodo se SUMA a su
                        // valor (la primera consulta agrega como siempre).
                        var valorExistente = indiceConsulta > 0
                            ? fila.ValoresPeriodo.FirstOrDefault(x => string.Equals(x.PeriodoId, periodo.Id, StringComparison.OrdinalIgnoreCase))
                            : null;

                        if (valorExistente != null)
                        {
                            valorExistente.Kg += kg;
                            valorExistente.Importe += importe;
                        }
                        else
                        {
                            fila.ValoresPeriodo.Add(new ReporteVentasProductoValorVm
                            {
                                PeriodoId = periodo.Id,
                                Kg = kg,
                                Importe = importe
                            });
                        }
                    }
                }
            }

            foreach (var fila in filas.Values)
            {
                foreach (var periodo in model.PeriodosComparativosVentas)
                {
                    if (!fila.ValoresPeriodo.Any(x => string.Equals(x.PeriodoId, periodo.Id, StringComparison.OrdinalIgnoreCase)))
                    {
                        fila.ValoresPeriodo.Add(new ReporteVentasProductoValorVm
                        {
                            PeriodoId = periodo.Id,
                            Kg = 0m,
                            Importe = 0m
                        });
                    }
                }

                fila.ValoresPeriodo = fila.ValoresPeriodo
                    .OrderBy(x => x.PeriodoId)
                    .ToList();
            }

            model.FilasVentasProducto = filas.Values
                .OrderBy(x => x.Codigo)
                .ThenBy(x => x.Producto)
                .ToList();

            model.HayResultados = model.FilasVentasProducto.Count > 0;
            if (!model.HayResultados)
            {
                model.Mensaje = "No se encontraron ventas para los filtros principales seleccionados.";
                return;
            }

            model.Totales.TotalKg = model.FilasVentasProducto.Sum(x => (x.ValoresPeriodo ?? new List<ReporteVentasProductoValorVm>()).Sum(v => v.Kg));
            model.Totales.TotalIngresos = model.FilasVentasProducto.Sum(x => (x.ValoresPeriodo ?? new List<ReporteVentasProductoValorVm>()).Sum(v => v.Importe));
            model.Totales.TotalEgresos = 0m;
            model.Totales.CantidadProductos = model.FilasVentasProducto.Count;
            model.Totales.CantidadRegistros = model.PeriodosComparativosVentas.Count;
            model.Mensaje = "Reporte cargado. Los totales visibles suman todos los periodos comparados.";
        }

        // Trae el stock actual (desde el cierre dado hasta ahora) de una sucursal (0 = todas) y lo vuelca en
        // stockPorSucursal[idSucursal][codigo]. La sucursal de cada valor es la de la fila del resultado.
        private void CargarStockActualProyeccion(
            Dictionary<int, Dictionary<long, (decimal StockActual, decimal Promedio, bool Pesable)>> stockPorSucursal,
            ReportesViewModel model,
            int idSucursalConsulta,
            DateTime fechaDesde)
        {
            var dtStockActual = _oCorteN.CierreStockWeb(
                model.BusquedaProducto,
                idSucursalConsulta,
                fechaDesde,
                DateTime.Now,
                model.TipoProducto,
                0,
                model.MarcaId);

            if (dtStockActual == null)
                return;

            foreach (System.Data.DataRow row in dtStockActual.Rows)
            {
                var codigo = LeerLong(row, "Codigo");
                if (codigo <= 0)
                    continue;

                int idSucursalFila = LeerInt(row, "idSucursal");
                if (!stockPorSucursal.TryGetValue(idSucursalFila, out var stockPorCodigo))
                {
                    stockPorCodigo = new Dictionary<long, (decimal StockActual, decimal Promedio, bool Pesable)>();
                    stockPorSucursal[idSucursalFila] = stockPorCodigo;
                }

                stockPorCodigo[codigo] = (
                    LeerDecimalPrimeraCoincidencia(row, "Faltante", "DIF"),
                    LeerDecimal(row, "promedio"),
                    LeerBool(row, "Pesable"));
            }
        }

        private void CargarReporteProyeccionVentasVsStock(ReportesViewModel model, string[] periodoComparativoDesde)
        {
            model.ConsultaRealizada = true;
            model.FilasProyeccionVentasStock.Clear();
            model.SucursalesProyeccion.Clear();
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(model.FechaDesde, model.FechaHasta, periodoComparativoDesde);

            int idEmpresaCatalogo = ObtenerIdEmpresaSesion();
            var cortes = (idEmpresaCatalogo > 0
                    ? (_oCorteN.ObtenerCortesPorEmpresa(idEmpresaCatalogo, false) ?? new List<Entidades.Corte>())
                    : (_oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>()))
                .Where(x => x != null)
                .GroupBy(x => x.IdCorte)
                .ToDictionary(g => g.Key, g => g.First());
            var cortesPorCodigo = cortes.Values
                .Where(x => x.codigo > 0)
                .GroupBy(x => x.codigo)
                .ToDictionary(g => g.Key, g => g.First());
            var cortesPorNombre = cortes.Values
                .Where(x => !string.IsNullOrWhiteSpace(x.corte))
                .GroupBy(x => NormalizarClaveProducto(x.corte))
                .ToDictionary(g => g.Key, g => g.First());

            var cierresDisponibles = ObtenerCierresDisponibles();
            var sucursalesBase = _oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            // Sucursales tildadas (una, varias o todas): una columna por cada una.
            var sucursalesProyeccion = sucursalesBase
                .Where(x => x != null && x.IdSucursal > 0 &&
                            (model.SucursalesIds.Count == 0 || model.SucursalesIds.Contains(x.IdSucursal)))
                .GroupBy(x => x.IdSucursal)
                .Select(g => g.First())
                .OrderBy(x => x.SucursalNombre)
                .ToList();

            model.SucursalesProyeccion = sucursalesProyeccion
                .Select(x => new ReporteProyeccionSucursalVm
                {
                    IdSucursal = x.IdSucursal,
                    Sucursal = x.SucursalNombre ?? ("Sucursal " + x.IdSucursal.ToString(CultureInfo.InvariantCulture))
                })
                .ToList();

            var stockPorSucursal = new Dictionary<int, Dictionary<long, (decimal StockActual, decimal Promedio, bool Pesable)>>();
            if (model.SucursalesIds.Count <= 1)
            {
                // Una sola sucursal: la consulta de siempre, desde su ultimo cierre.
                var cierres = FiltrarCierresPorSucursal(cierresDisponibles, model.SucursalId);
                var fechaUltimoCierreStock = cierres.FirstOrDefault() != null
                    ? cierres.First().FechaCompra
                    : DateTime.Today;
                CargarStockActualProyeccion(stockPorSucursal, model, model.SucursalId > 0 ? model.SucursalId : 0, fechaUltimoCierreStock);
            }
            else
            {
                // Varias sucursales (o todas): el stock de cada una parte de SU ultimo cierre. Antes se hacia una sola
                // consulta con el cierre mas reciente de cualquier sucursal, y las que tenian su cierre anterior
                // quedaban con el stock mal (sin su stock inicial); si los cierres coinciden da lo mismo.
                foreach (var sucursal in sucursalesProyeccion)
                {
                    var cierresSucursal = FiltrarCierresPorSucursal(cierresDisponibles, sucursal.IdSucursal);
                    var fechaUltimoCierreSucursal = cierresSucursal.FirstOrDefault() != null
                        ? cierresSucursal.First().FechaCompra
                        : DateTime.Today;
                    CargarStockActualProyeccion(stockPorSucursal, model, sucursal.IdSucursal, fechaUltimoCierreSucursal);
                }
            }

            var filas = new Dictionary<string, ReporteProyeccionVentasStockFilaVm>(StringComparer.OrdinalIgnoreCase);

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                foreach (var sucursal in model.SucursalesProyeccion)
                {
                    var dt = _oCorteN.acum_Ventas(
                        model.BusquedaProducto,
                        sucursal.IdSucursal,
                        periodo.FechaDesde,
                        periodo.FechaHasta,
                        model.TipoProducto,
                        0,
                        model.MarcaId);

                    if (dt == null)
                        continue;

                    foreach (System.Data.DataRow row in dt.Rows)
                    {
                        long codigo = LeerLong(row, "Codigo");
                        string producto = LeerString(row, "Corte");
                        int idCorte = LeerInt(row, "idCorte");
                        Entidades.Corte corteMeta = null;

                        if (idCorte > 0)
                            cortes.TryGetValue(idCorte, out corteMeta);

                        if (corteMeta == null && codigo > 0)
                        {
                            cortesPorCodigo.TryGetValue(codigo, out corteMeta);
                            if (corteMeta != null)
                                idCorte = corteMeta.IdCorte;
                        }

                        if (corteMeta == null && !string.IsNullOrWhiteSpace(producto))
                        {
                            cortesPorNombre.TryGetValue(NormalizarClaveProducto(producto), out corteMeta);
                            if (corteMeta != null)
                                idCorte = corteMeta.IdCorte;
                        }

                        var filaKey = idCorte > 0
                            ? "ID:" + idCorte.ToString(CultureInfo.InvariantCulture)
                            : codigo > 0
                                ? "COD:" + codigo.ToString(CultureInfo.InvariantCulture)
                                : "NOM:" + NormalizarClaveProducto(producto);

                        if (string.Equals(filaKey, "NOM:", StringComparison.OrdinalIgnoreCase))
                            continue;

                        ReporteProyeccionVentasStockFilaVm fila;
                        if (!filas.TryGetValue(filaKey, out fila))
                        {
                            fila = new ReporteProyeccionVentasStockFilaVm
                            {
                                IdCorte = idCorte,
                                Codigo = codigo,
                                Producto = producto,
                                TipoProducto = corteMeta != null ? (corteMeta.Tipo ?? "") : "",
                                MarcaId = corteMeta != null && corteMeta.Marca != null ? corteMeta.Marca.IdPersona : 0,
                                Marca = corteMeta != null ? (corteMeta.MarcaNombre ?? "") : ""
                            };
                            filas[filaKey] = fila;
                        }

                        var stockActualSucursal = 0m;
                        var promedioSucursal = 0m;
                        var pesableSucursal = false;
                        Dictionary<long, (decimal StockActual, decimal Promedio, bool Pesable)> stockSucursalActual;
                        if (codigo > 0
                            && stockPorSucursal.TryGetValue(sucursal.IdSucursal, out stockSucursalActual)
                            && stockSucursalActual != null
                            && stockSucursalActual.TryGetValue(codigo, out var stockInfo))
                        {
                            stockActualSucursal = stockInfo.StockActual;
                            promedioSucursal = stockInfo.Promedio;
                            pesableSucursal = stockInfo.Pesable;
                        }

                        var stockSucursalVm = fila.StocksPorSucursal.FirstOrDefault(x => x.IdSucursal == sucursal.IdSucursal);
                        if (stockSucursalVm == null)
                        {
                            fila.StocksPorSucursal.Add(new ReporteProyeccionSucursalStockVm
                            {
                                IdSucursal = sucursal.IdSucursal,
                                Sucursal = sucursal.Sucursal,
                                StockActual = stockActualSucursal,
                                Promedio = promedioSucursal,
                                Pesable = pesableSucursal
                            });
                        }

                        var ventas = LeerDecimalPrimeraCoincidencia(row, "Ventas", "Total Vendido", "Total Kgs", "Total Kg", "Kgs", "Kg", "cantKg", "CantKg");
                        var valorSucursal = fila.ValoresPeriodoSucursal.FirstOrDefault(x =>
                            x.IdSucursal == sucursal.IdSucursal &&
                            string.Equals(x.PeriodoId, periodo.Id, StringComparison.OrdinalIgnoreCase));

                        if (valorSucursal == null)
                        {
                            fila.ValoresPeriodoSucursal.Add(new ReporteProyeccionVentasStockSucursalValorVm
                            {
                                PeriodoId = periodo.Id,
                                IdSucursal = sucursal.IdSucursal,
                                Sucursal = sucursal.Sucursal,
                                Ventas = ventas,
                                Diferencia = stockActualSucursal - ventas
                            });
                        }
                        else
                        {
                            valorSucursal.Ventas += ventas;
                            valorSucursal.Diferencia = stockActualSucursal - valorSucursal.Ventas;
                        }
                    }
                }
            }

            foreach (var fila in filas.Values)
            {
                foreach (var sucursal in model.SucursalesProyeccion)
                {
                    var stockSucursal = fila.StocksPorSucursal.FirstOrDefault(x => x.IdSucursal == sucursal.IdSucursal);
                    if (stockSucursal == null)
                    {
                        var stockActualSucursal = 0m;
                        var promedioSucursal = 0m;
                        var pesableSucursal = false;
                        Dictionary<long, (decimal StockActual, decimal Promedio, bool Pesable)> stockSucursalActual;
                        if (fila.Codigo > 0
                            && stockPorSucursal.TryGetValue(sucursal.IdSucursal, out stockSucursalActual)
                            && stockSucursalActual != null
                            && stockSucursalActual.TryGetValue(fila.Codigo, out var stockInfo))
                        {
                            stockActualSucursal = stockInfo.StockActual;
                            promedioSucursal = stockInfo.Promedio;
                            pesableSucursal = stockInfo.Pesable;
                        }

                        stockSucursal = new ReporteProyeccionSucursalStockVm
                        {
                            IdSucursal = sucursal.IdSucursal,
                            Sucursal = sucursal.Sucursal,
                            StockActual = stockActualSucursal,
                            Promedio = promedioSucursal,
                            Pesable = pesableSucursal
                        };
                        fila.StocksPorSucursal.Add(stockSucursal);
                    }

                    foreach (var periodo in model.PeriodosComparativosVentas)
                    {
                        if (!fila.ValoresPeriodoSucursal.Any(x =>
                            x.IdSucursal == sucursal.IdSucursal &&
                            string.Equals(x.PeriodoId, periodo.Id, StringComparison.OrdinalIgnoreCase)))
                        {
                            fila.ValoresPeriodoSucursal.Add(new ReporteProyeccionVentasStockSucursalValorVm
                            {
                                PeriodoId = periodo.Id,
                                IdSucursal = sucursal.IdSucursal,
                                Sucursal = sucursal.Sucursal,
                                Ventas = 0m,
                                Diferencia = stockSucursal.StockActual
                            });
                        }
                    }
                }

                fila.StocksPorSucursal = fila.StocksPorSucursal
                    .OrderBy(x => x.Sucursal)
                    .ToList();

                fila.ValoresPeriodoSucursal = fila.ValoresPeriodoSucursal
                    .OrderBy(x => x.PeriodoId)
                    .ThenBy(x => x.Sucursal)
                    .ToList();

                fila.StockActual = fila.StocksPorSucursal.Sum(x => x.StockActual);
                var stockRepresentativo = fila.StocksPorSucursal.FirstOrDefault(x => x.Pesable && x.Promedio > 0) ?? fila.StocksPorSucursal.FirstOrDefault();
                fila.Promedio = stockRepresentativo != null ? stockRepresentativo.Promedio : 0m;
                fila.Pesable = stockRepresentativo != null && stockRepresentativo.Pesable;
                fila.ValoresPeriodo = model.PeriodosComparativosVentas
                    .Select(periodo =>
                    {
                        var valoresPeriodo = fila.ValoresPeriodoSucursal
                            .Where(x => string.Equals(x.PeriodoId, periodo.Id, StringComparison.OrdinalIgnoreCase))
                            .ToList();

                        return new ReporteProyeccionVentasStockValorVm
                        {
                            PeriodoId = periodo.Id,
                            Ventas = valoresPeriodo.Sum(x => x.Ventas),
                            Diferencia = valoresPeriodo.Sum(x => x.Diferencia)
                        };
                    })
                    .ToList();

                fila.ValoresPeriodo = fila.ValoresPeriodo
                    .OrderBy(x => x.PeriodoId)
                    .ToList();
            }

            model.FilasProyeccionVentasStock = filas.Values
                .OrderBy(x => x.Codigo)
                .ThenBy(x => x.Producto)
                .ToList();

            model.HayResultados = model.FilasProyeccionVentasStock.Count > 0;
            if (!model.HayResultados)
            {
                model.Mensaje = "No se encontraron datos para la proyeccion con los filtros principales seleccionados.";
                return;
            }

            model.Totales.TotalKg = model.FilasProyeccionVentasStock.Sum(x => x.StockActual);
            model.Totales.TotalIngresos = model.FilasProyeccionVentasStock.Sum(x => (x.ValoresPeriodo ?? new List<ReporteProyeccionVentasStockValorVm>()).Sum(v => v.Ventas));
            model.Totales.TotalEgresos = model.FilasProyeccionVentasStock.Sum(x => (x.ValoresPeriodo ?? new List<ReporteProyeccionVentasStockValorVm>()).Sum(v => v.Diferencia));
            model.Totales.CantidadProductos = model.FilasProyeccionVentasStock.Count;
            model.Totales.CantidadRegistros = model.PeriodosComparativosVentas.Count;
            model.Mensaje = "Reporte cargado. La diferencia se calcula como Stock Actual - Ventas del periodo.";
        }

        private void CargarReporteBalanceEconomico(ReportesViewModel model, string[] periodoComparativoDesde)
        {
            model.ConsultaRealizada = true;
            model.PeriodosBalance.Clear();
            model.GastosBalanceCategorias.Clear();

            var fechaDesdeBase = model.FechaDesde.Date;
            var fechaHastaBase = model.FechaHasta.Date;
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(fechaDesdeBase, fechaHastaBase, periodoComparativoDesde)
                .OrderBy(x => x.FechaDesde)
                .ThenBy(x => x.FechaHasta)
                .Select((x, index) => new ReportePeriodoComparativoVm
                {
                    Id = x.Id,
                    Titulo = "Periodo " + (index + 1).ToString(CultureInfo.InvariantCulture),
                    EsPrincipal = x.EsPrincipal,
                    FechaDesde = x.FechaDesde,
                    FechaHasta = x.FechaHasta
                })
                .ToList();

            // Una sucursal -> [id]; todas -> [0] (consultas con -1/0 como siempre); varias -> se suman por sucursal.
            var idsConsultaBalance = IdsParaConsulta(model);

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                var metricas = CalcularBalancePeriodo(
                    periodo,
                    idsConsultaBalance,
                    model.IncluirVentasCuentaCorrienteBalance);

                model.PeriodosBalance.Add(metricas.Periodo);
                model.GastosBalanceCategorias.AddRange(metricas.GastosPorCategoria);
            }

            var periodoPrincipal = model.PeriodosBalance.FirstOrDefault(x => x.EsPrincipal)
                ?? model.PeriodosBalance.FirstOrDefault();
            model.HayResultados = periodoPrincipal != null;

            if (!model.HayResultados)
            {
                model.Mensaje = "No se encontraron datos para el balance economico con los filtros principales seleccionados.";
                return;
            }

            model.Totales.TotalKg = periodoPrincipal.TotalVentas;
            model.Totales.TotalIngresos = periodoPrincipal.Compras;
            model.Totales.TotalEgresos = periodoPrincipal.Gastos;
            model.Totales.CantidadProductos = model.PeriodosBalance.Count;
            model.Totales.CantidadRegistros = model.PeriodosBalance.Count;
            model.Mensaje = model.IncluirVentasCuentaCorrienteBalance
                ? "Reporte cargado. El balance incluye ventas en cuenta corriente."
                : "Reporte cargado. El balance excluye ventas en cuenta corriente.";
        }

        // idsConsulta: una sola sucursal [id], todas [0] (una consulta con -1/0), o varias (se consulta cada una y se
        // suma: ventas, kilos, compras, gastos y cobros/pagos son aditivos).
        private BalancePeriodoResultado CalcularBalancePeriodo(
            ReportePeriodoComparativoVm periodo,
            List<int> idsConsulta,
            bool incluirVentasCuentaCorriente)
        {
            DateTime desde = periodo.FechaDesde.Date;
            DateTime hasta = periodo.FechaHasta.Date;
            DateTime hastaFinDia = hasta.AddDays(1).AddMilliseconds(-1);

            var ventas = new List<Entidades.Venta>();
            decimal kilosVendidosPesables = 0m;
            decimal kilosVendidosPesablesGrafico = 0m;
            decimal totalCompras = 0m;
            var gastosPorCategoria = new List<ReporteBalanceGastoCategoriaVm>();
            decimal totalCobros = 0m;
            decimal totalPagos = 0m;

            foreach (int idSucursalConsulta in idsConsulta)
            {
                // 0 = todas las sucursales: las consultas de ventas/gastos/pagos usan -1, las de compras 0 (como siempre).
                int sucursalConsulta = idSucursalConsulta > 0 ? idSucursalConsulta : -1;

                ventas.AddRange((_oVentaN.getVentasBalancePeriodo(desde, hasta, sucursalConsulta) ?? new List<Entidades.Venta>())
                    .Where(x => x != null)
                    .Where(x => !string.Equals((x.Estado ?? "").Trim(), "ANULADO", StringComparison.OrdinalIgnoreCase)));

                kilosVendidosPesables += _oVentaN.getTotalKgsPesablesBalancePeriodo(desde, hasta, sucursalConsulta, true);
                kilosVendidosPesablesGrafico += _oVentaN.getTotalKgsPesablesBalancePeriodo(desde, hasta, sucursalConsulta, incluirVentasCuentaCorriente);

                System.Data.DataTable dtCompras = _oCompraN.obtenerCompras(
                    idSucursalConsulta > 0 ? idSucursalConsulta : 0,
                    "Todos",
                    "",
                    desde,
                    hastaFinDia,
                    null) ?? new System.Data.DataTable();

                totalCompras += dtCompras.AsEnumerable()
                    .Where(EsCompraEconomica)
                    .Sum(x => LeerDecimal(x, "totalS"));

                System.Data.DataTable dtGastosAgrupados = _oCierreN.obtenerGastosAgrupadosBalance(desde, hastaFinDia, sucursalConsulta) ?? new System.Data.DataTable();
                gastosPorCategoria.AddRange(dtGastosAgrupados.AsEnumerable()
                    .Select(row => new ReporteBalanceGastoCategoriaVm
                    {
                        PeriodoId = periodo.Id,
                        Categoria = string.IsNullOrWhiteSpace(LeerString(row, "Categoria")) ? "Otros gastos" : LeerString(row, "Categoria"),
                        Total = LeerDecimal(row, "Total")
                    }));

                System.Data.DataTable dtPagosBalance = _oCuentaCorrienteN.obtenerTotalesPagosBalance(desde, hasta, sucursalConsulta) ?? new System.Data.DataTable();
                System.Data.DataRow rowPagosBalance = dtPagosBalance.Rows.Count > 0 ? dtPagosBalance.Rows[0] : null;
                totalCobros += rowPagosBalance != null ? LeerDecimal(rowPagosBalance, "TotalCobros") : 0m;
                totalPagos += rowPagosBalance != null ? LeerDecimal(rowPagosBalance, "TotalPagos") : 0m;
            }

            // Varias sucursales: la misma categoria de gasto que trae mas de una se junta en una sola fila.
            // (Con una sola consulta se deja tal cual vino.)
            if (idsConsulta.Count > 1)
            {
                gastosPorCategoria = gastosPorCategoria
                    .GroupBy(x => x.Categoria, StringComparer.OrdinalIgnoreCase)
                    .Select(g => new ReporteBalanceGastoCategoriaVm
                    {
                        PeriodoId = periodo.Id,
                        Categoria = g.First().Categoria,
                        Total = g.Sum(x => x.Total)
                    })
                    .ToList();
            }

            gastosPorCategoria = gastosPorCategoria
                .OrderByDescending(x => x.Total)
                .ToList();
            decimal totalGastos = gastosPorCategoria.Sum(x => x.Total);

            decimal ventasCuentaCorriente = ventas
                .Where(EsVentaCuentaCorriente)
                .Sum(x => Convert.ToDecimal(x.TotalImporte));
            decimal totalVentas = ventas.Sum(x => Convert.ToDecimal(x.TotalImporte));
            decimal kilosVendidos = ventas.Sum(x => Convert.ToDecimal(x.TotalKgs));
            int cantidadVentas = ventas.Count;
            decimal ventasConsumidorFinal = totalVentas - ventasCuentaCorriente;
            var ventasConsideradasGrafico = incluirVentasCuentaCorriente
                ? ventas
                : ventas.Where(x => !EsVentaCuentaCorriente(x)).ToList();
            decimal ventasGrafico = ventasConsideradasGrafico.Sum(x => Convert.ToDecimal(x.TotalImporte));
            decimal kilosVendidosGrafico = ventasConsideradasGrafico.Sum(x => Convert.ToDecimal(x.TotalKgs));
            int cantidadVentasGrafico = ventasConsideradasGrafico.Count;
            decimal ventasEfectivo = ventas.Sum(CalcularVentaEfectivo);
            decimal ventasBancarizadas = ventas.Sum(CalcularVentaBancarizada);

            decimal baseBalance = incluirVentasCuentaCorriente ? totalVentas : ventasConsumidorFinal;
            decimal balance = baseBalance - totalCompras - totalGastos;

            return new BalancePeriodoResultado
            {
                Periodo = new ReporteBalancePeriodoVm
                {
                    PeriodoId = periodo.Id,
                    Titulo = periodo.Titulo,
                    EsPrincipal = periodo.EsPrincipal,
                    FechaDesde = desde,
                    FechaHasta = hasta,
                    VentasConsumidorFinal = ventasConsumidorFinal,
                    VentasCuentaCorriente = ventasCuentaCorriente,
                    VentasEfectivo = ventasEfectivo,
                    VentasBancarizadas = ventasBancarizadas,
                    TotalVentas = totalVentas,
                    VentasConsideradasGrafico = ventasGrafico,
                    KilosVendidos = kilosVendidos,
                    KilosVendidosGrafico = kilosVendidosGrafico,
                    KilosVendidosPesables = kilosVendidosPesables,
                    KilosVendidosPesablesGrafico = kilosVendidosPesablesGrafico,
                    CantidadVentas = cantidadVentas,
                    CantidadVentasGrafico = cantidadVentasGrafico,
                    Compras = totalCompras,
                    Gastos = totalGastos,
                    BalanceEconomico = balance,
                    Cobros = totalCobros,
                    Pagos = totalPagos,
                    DiferenciaCobrosPagos = totalCobros - totalPagos
                },
                GastosPorCategoria = gastosPorCategoria
            };
        }

        private bool EsVentaCuentaCorriente(Entidades.Venta venta)
        {
            if (venta == null)
                return false;

            return venta.EnCtaCte ||
                string.Equals((venta.FormaPago ?? "").Trim(), Entidades.Venta.formaPagoEnum.CtaCte.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        private decimal CalcularVentaEfectivo(Entidades.Venta venta)
        {
            if (venta == null)
                return 0m;

            if (venta.PagoMixtoEfectivo > 0)
                return Convert.ToDecimal(venta.PagoMixtoEfectivo);

            return string.Equals((venta.FormaPago ?? "").Trim(), Entidades.Venta.formaPagoEnum.Efectivo.ToString(), StringComparison.OrdinalIgnoreCase)
                ? Convert.ToDecimal(venta.TotalImporte)
                : 0m;
        }

        private decimal CalcularVentaBancarizada(Entidades.Venta venta)
        {
            if (venta == null)
                return 0m;

            string formaPago = (venta.FormaPago ?? "").Trim();
            decimal total = Convert.ToDecimal(venta.TotalImporte);
            decimal efectivo = venta.PagoMixtoEfectivo > 0 ? Convert.ToDecimal(venta.PagoMixtoEfectivo) : 0m;

            if (string.Equals(formaPago, Entidades.Venta.formaPagoEnum.Debito.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formaPago, Entidades.Venta.formaPagoEnum.Credito.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formaPago, Entidades.Venta.formaPagoEnum.Qr.ToString(), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(formaPago, Entidades.Venta.formaPagoEnum.Transferencia.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return total - efectivo;
            }

            return 0m;
        }

        private bool EsCompraEconomica(System.Data.DataRow row)
        {
            string tipoCompra = LeerString(row, "tipoCompra");
            return string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(tipoCompra, Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.MediaRes), StringComparison.OrdinalIgnoreCase);
        }

        private string NormalizarClaveProducto(string valor)
        {
            return (valor ?? "").Trim().ToUpperInvariant();
        }

        private List<ReportePeriodoComparativoVm> ConstruirPeriodosComparativosVentas(DateTime fechaDesde, DateTime fechaHasta, string[] periodoComparativoDesde)
        {
            var periodos = new List<ReportePeriodoComparativoVm>();
            var duracion = fechaHasta - fechaDesde;

            periodos.Add(new ReportePeriodoComparativoVm
            {
                Id = "P1",
                Titulo = "Periodo 1",
                EsPrincipal = true,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            });

            var fechasComparativas = (periodoComparativoDesde ?? new string[0])
                .Select(ParseFechaComparativa)
                .Where(x => x.HasValue)
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            for (int i = 0; i < fechasComparativas.Count; i++)
            {
                var fechaComparativa = fechasComparativas[i].Date;
                var desdeComparativo = fechaComparativa.Add(fechaDesde.TimeOfDay);
                periodos.Add(new ReportePeriodoComparativoVm
                {
                    Id = "P" + (i + 2).ToString(CultureInfo.InvariantCulture),
                    Titulo = "Periodo " + (i + 2).ToString(CultureInfo.InvariantCulture),
                    EsPrincipal = false,
                    FechaDesde = desdeComparativo,
                    FechaHasta = desdeComparativo.Add(duracion)
                });
            }

            return periodos;
        }

        private List<ReporteCierreOptionVm> ObtenerCierresDisponibles()
        {
            var cierres = new List<ReporteCierreOptionVm>();
            System.Data.DataTable dt = _oCompraN.obtenerCompras(
                0,
                Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.CierreStock),
                "",
                DateTime.Today.AddYears(-10),
                DateTime.Now.AddMinutes(1),
                null) ?? new System.Data.DataTable();

            foreach (System.Data.DataRow row in dt.Rows)
            {
                DateTime fechaCompra;
                if (!DateTime.TryParse(Convert.ToString(row["fechaCompra"]), out fechaCompra))
                    continue;

                int idSucursal = LeerInt(row, "idSucursal");
                string sucursal = LeerString(row, "sucursal");
                if (string.IsNullOrWhiteSpace(sucursal))
                    sucursal = "Sucursal " + idSucursal;

                cierres.Add(new ReporteCierreOptionVm
                {
                    IdCompra = LeerInt(row, "idCompra"),
                    IdSucursal = idSucursal,
                    Sucursal = sucursal,
                    FechaCompra = fechaCompra,
                    ValorIso = fechaCompra.ToString("yyyy-MM-ddTHH:mm"),
                    Texto = fechaCompra.ToString("dd/MM/yyyy HH:mm")
                });
            }

            return cierres
                .OrderByDescending(x => x.FechaCompra)
                .ThenBy(x => x.Sucursal)
                .ToList();
        }

        private List<ReporteCierreOptionVm> FiltrarCierresPorSucursal(List<ReporteCierreOptionVm> cierres, int sucursalId)
        {
            var lista = cierres ?? new List<ReporteCierreOptionVm>();
            return (sucursalId > 0 ? lista.Where(x => x.IdSucursal == sucursalId) : lista)
                .OrderByDescending(x => x.FechaCompra)
                .ThenBy(x => x.Sucursal)
                .ToList();
        }

        private bool RequestTieneFecha(string key)
        {
            var valor = Request != null ? Request.Query[key].ToString() : null;
            return !string.IsNullOrWhiteSpace(valor);
        }

        private decimal LeerDecimal(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? ConvertirDecimalFlexible(row[columna])
                : 0m;
        }

        private decimal LeerDecimalPrimeraCoincidencia(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas ?? new string[0])
            {
                if (row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                    return ConvertirDecimalFlexible(row[columna]);
            }

            return 0m;
        }

        private int LeerIntPrimeraCoincidencia(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas ?? new string[0])
            {
                if (row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                    return Convert.ToInt32(row[columna], CultureInfo.InvariantCulture);
            }

            return 0;
        }

        private int LeerInt(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToInt32(row[columna], CultureInfo.InvariantCulture)
                : 0;
        }

        private long LeerLongPrimeraCoincidencia(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas ?? new string[0])
            {
                if (row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                    return Convert.ToInt64(row[columna], CultureInfo.InvariantCulture);
            }

            return 0L;
        }

        private long LeerLong(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToInt64(row[columna], CultureInfo.InvariantCulture)
                : 0L;
        }

        private string LeerStringPrimeraCoincidencia(System.Data.DataRow row, params string[] columnas)
        {
            foreach (var columna in columnas ?? new string[0])
            {
                if (row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value)
                    return Convert.ToString(row[columna]) ?? "";
            }

            return "";
        }

        private string LeerString(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? (Convert.ToString(row[columna]) ?? "")
                : "";
        }

        private bool LeerBool(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value &&
                Convert.ToBoolean(row[columna]);
        }

        private string CalcularEstadoStock(decimal stockActual, decimal puntoStock)
        {
            if (stockActual < 0) return "NEGATIVO";
            if (puntoStock > 0 && stockActual <= puntoStock) return "BAJO";
            if (stockActual == 0) return "SIN STOCK";
            return "OK";
        }

        private decimal ConvertirDecimalFlexible(object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return 0m;

            if (valor is decimal)
                return (decimal)valor;

            if (valor is int || valor is long || valor is short || valor is byte)
                return Convert.ToDecimal(valor, CultureInfo.InvariantCulture);

            if (valor is float || valor is double)
                return Convert.ToDecimal(valor, CultureInfo.InvariantCulture);

            var texto = Convert.ToString(valor);
            if (string.IsNullOrWhiteSpace(texto))
                return 0m;

            decimal numero;
            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.GetCultureInfo("es-AR"), out numero))
                return numero;

            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.InvariantCulture, out numero))
                return numero;

            if (decimal.TryParse(texto, NumberStyles.Any, CultureInfo.CurrentCulture, out numero))
                return numero;

            return 0m;
        }

        private DateTime? ParseFechaComparativa(string valor)
        {
            DateTime fecha;
            return DateTime.TryParse(valor, out fecha) ? (DateTime?)fecha : null;
        }

        private List<string> ConstruirLabelsSerie(ReportePeriodoComparativoVm periodoPrincipal, string agrupacionTemporal)
        {
            var labels = new List<string>();
            if (periodoPrincipal == null)
                return labels;

            var cursor = periodoPrincipal.FechaDesde;
            bool porDia = string.Equals(agrupacionTemporal, "dia", StringComparison.OrdinalIgnoreCase);
            int indice = 1;

            while (cursor <= periodoPrincipal.FechaHasta)
            {
                labels.Add((porDia ? "Dia " : "Hora ") + indice.ToString(CultureInfo.InvariantCulture));

                cursor = porDia ? cursor.AddDays(1) : cursor.AddHours(1);
                indice++;
            }

            return labels;
        }

        private string ObtenerAgrupacionVentasComparativa(ReportePeriodoComparativoVm periodoPrincipal)
        {
            if (periodoPrincipal == null)
                return "hora";

            return periodoPrincipal.FechaDesde.Date == periodoPrincipal.FechaHasta.Date
                ? "hora"
                : "dia";
        }

        private List<SerieVentasComparativaPunto> CompletarSeriePeriodo(ReportePeriodoComparativoVm periodo, System.Data.DataTable dt, string agrupacionTemporal)
        {
            var valores = new List<SerieVentasComparativaPunto>();
            var mapa = new Dictionary<DateTime, SerieVentasComparativaPunto>();
            bool porDia = string.Equals(agrupacionTemporal, "dia", StringComparison.OrdinalIgnoreCase);

            if (dt != null)
            {
                foreach (System.Data.DataRow row in dt.Rows)
                {
                    DateTime bucket;
                    if (!DateTime.TryParse(Convert.ToString(row["BucketFecha"]), out bucket))
                        continue;

                    var clave = porDia ? bucket.Date : new DateTime(bucket.Year, bucket.Month, bucket.Day, bucket.Hour, 0, 0);
                    mapa[clave] = new SerieVentasComparativaPunto
                    {
                        FechaReal = clave,
                        Kg = LeerDecimal(row, "TotalKg"),
                        Importe = LeerDecimal(row, "TotalImporte")
                    };
                }
            }

            var cursor = periodo.FechaDesde;
            int indice = 1;
            while (cursor <= periodo.FechaHasta)
            {
                var clave = porDia ? cursor.Date : new DateTime(cursor.Year, cursor.Month, cursor.Day, cursor.Hour, 0, 0);
                SerieVentasComparativaPunto valor;
                mapa.TryGetValue(clave, out valor);

                valores.Add(new SerieVentasComparativaPunto
                {
                    Indice = indice,
                    EtiquetaRelativa = (porDia ? "Dia " : "Hora ") + indice.ToString(CultureInfo.InvariantCulture),
                    FechaReal = clave,
                    Kg = valor != null ? valor.Kg : 0m,
                    Importe = valor != null ? valor.Importe : 0m
                });

                cursor = porDia ? cursor.AddDays(1) : cursor.AddHours(1);
                indice++;
            }

            return valores;
        }

        private class SerieVentasComparativaPeriodoInfo
        {
            public ReportePeriodoComparativoVm Periodo { get; set; }
            public string Color { get; set; }
            public List<SerieVentasComparativaPunto> Puntos { get; set; }
        }

        private class BalancePeriodoResultado
        {
            public ReporteBalancePeriodoVm Periodo { get; set; }
            public List<ReporteBalanceGastoCategoriaVm> GastosPorCategoria { get; set; }

            public BalancePeriodoResultado()
            {
                GastosPorCategoria = new List<ReporteBalanceGastoCategoriaVm>();
            }
        }

        private class SerieVentasComparativaPunto
        {
            public int Indice { get; set; }
            public string EtiquetaRelativa { get; set; } = "";
            public DateTime FechaReal { get; set; }
            public decimal Kg { get; set; }
            public decimal Importe { get; set; }
        }

        private SelectListItem CrearItemReporte(string valor, string seleccionado)
        {
            return new SelectListItem
            {
                Value = valor,
                Text = valor,
                Selected = string.Equals(valor, seleccionado, StringComparison.OrdinalIgnoreCase)
            };
        }
    }
}
