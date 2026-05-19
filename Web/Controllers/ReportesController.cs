using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers
{
    public class ReportesController : BaseController
    {
        private const string TipoReporteStockActual = "Stock Actual";
        private const string TipoReporteCierreStock = "Cierre Stock";
        private const string TipoReporteStockRetroactivo = "Stock Retroactivo";
        private const string TipoReporteProyeccion = "Proyeccion Ventas vs Stock";
        private const string TipoReporteVentasProducto = "Ventas por Producto";
        private const string TipoReporteBalance = "Balance Economico";

        private Negocio.Sucursal oSucursalN;
        private Negocio.Corte oCorteN;
        private Negocio.Compra oCompraN;
        private Negocio.Venta oVentaN;
        private Negocio.CierreCaja oCierreN;
        private Negocio.CuentaCorriente oCuentaCorrienteN;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oSucursalN = new Negocio.Sucursal(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oCompraN = new Negocio.Compra(empresa, param);
            oVentaN = new Negocio.Venta(empresa, param);
            oCierreN = new Negocio.CierreCaja(empresa, param);
            oCuentaCorrienteN = new Negocio.CuentaCorriente(empresa, param);
        }

        [HttpGet]
        public ActionResult Index(
            string tipoReporte = TipoReporteStockActual,
            DateTime? fechaDesde = null,
            DateTime? fechaHasta = null,
            int? idSucursal = null,
            string busquedaProducto = "",
            string tipoProducto = "",
            int marcaId = 0,
            string agrupacionTemporal = "hora",
            string estadoStock = "Todos",
            string[] periodoComparativoDesde = null,
            bool incluirVentasCuentaCorrienteBalance = false,
            bool buscar = false)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return RedirectToAction("Index", "Login");

            DateTime desde = fechaDesde ?? DateTime.Today.AddDays(-7);
            DateTime hasta = fechaHasta ?? DateTime.Now;

            if (!PermisosHelper.TienePermiso(Session, Permisos.Stock.VerStock, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()))
            {
                if (AjustarFechaSiNoTienePermiso(Permisos.Stock.VerStock, ref desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo()) && hasta < desde)
                    hasta = desde;
                else
                    return VistaAccesoDenegado("Reportes", Permisos.Stock.VerStock, desde, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());
            }

            int sucursalSeleccionada = idSucursal.HasValue
                ? idSucursal.Value
                : (user.IdSucursal > 0 ? user.IdSucursal : 0);

            sucursalSeleccionada = AjustarSucursalSegunReporte(tipoReporte, sucursalSeleccionada, user);

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
                buscar);

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
            ConfigurarAdvertenciaFechaEnVivo("fechaDesdeReporte", Permisos.Stock.VerStock, Utilidades.ValoresParametrosMetodos.IdCreadorNulo());

            return View("~/Views/Reportes/Index.cshtml", model);
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
            string[] periodoComparativoDesde = null)
        {
            var user = Session["Usuario"] as Entidades.Usuario;
            if (user == null)
                return Json(new { ok = false, mensaje = "Sesion vencida." }, JsonRequestBehavior.AllowGet);

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
                var dt = oCorteN.ObtenerSerieVentasPorCorte(
                    idCorte,
                    idSucursal,
                    periodo.FechaDesde,
                    periodo.FechaHasta,
                    tipoProducto,
                    marcaId,
                    agrupacionAplicada);

                var serie = CompletarSeriePeriodo(periodo, dt, agrupacionAplicada);
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
            }, JsonRequestBehavior.AllowGet);
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
            bool buscar)
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

            var cortes = oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>();
            var sucursales = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            model.CierresDisponibles = ObtenerCierresDisponibles();

            model.Sucursales.Add(new SelectListItem
            {
                Value = "0",
                Text = "Todas",
                Selected = model.SucursalId <= 0
            });

            foreach (var sucursal in sucursales
                .Where(x => x != null && x.IdSucursal > 0)
                .GroupBy(x => x.IdSucursal)
                .Select(g => g.First())
                .OrderBy(x => x.SucursalNombre))
            {
                model.Sucursales.Add(new SelectListItem
                {
                    Value = sucursal.IdSucursal.ToString(),
                    Text = sucursal.SucursalNombre,
                    Selected = sucursal.IdSucursal == model.SucursalId
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

            model.MostrarFiltroBusquedaProducto = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroTipoProducto = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroMarca = esReporteStock || esReporteVentasProducto || esReporteProyeccion;
            model.MostrarFiltroEstadoStock = esReporteStock;
            model.MostrarFiltroAgrupacionTemporal = esReporteVentasProducto;
            model.MostrarGrafico = esReporteBalance;
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

            var sucursal = (oSucursalN.findAll() ?? new List<Entidades.Sucursal>())
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
            var dt = oCorteN.CierreStock(
                1,
                "",
                model.SucursalId > 0 ? model.SucursalId : 0,
                model.FechaDesde,
                model.FechaHasta,
                null,
                "",
                0,
                0);

            model.ConsultaRealizada = true;
            model.FilasStockActual.Clear();

            var cortes = (oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>())
                .Where(x => x != null)
                .GroupBy(x => x.IdCorte)
                .ToDictionary(g => g.Key, g => g.First());

            if (dt == null || dt.Rows.Count == 0)
            {
                model.HayResultados = false;
                model.Mensaje = "No se encontraron datos para los filtros principales seleccionados.";
                return;
            }

            foreach (System.Data.DataRow row in dt.Rows)
            {
                int idCorte = LeerInt(row, "idCorte");
                Entidades.Corte corteMeta = null;
                cortes.TryGetValue(idCorte, out corteMeta);

                decimal stockActual = LeerDecimal(row, "Faltante");
                decimal puntoStock = LeerDecimal(row, "Pto.Stock");
                string estado = CalcularEstadoStock(stockActual, puntoStock);

                model.FilasStockActual.Add(new ReporteStockFilaVm
                {
                    IdCorte = idCorte,
                    Codigo = LeerLong(row, "Codigo"),
                    Producto = LeerString(row, "Corte"),
                    IdSucursal = model.SucursalId,
                    Sucursal = LeerString(row, "Sucursal"),
                    TipoProducto = corteMeta != null ? (corteMeta.Tipo ?? "") : "",
                    MarcaId = corteMeta != null && corteMeta.Marca != null ? corteMeta.Marca.IdPersona : 0,
                    Marca = corteMeta != null ? (corteMeta.MarcaNombre ?? "") : "",
                    FechaUltimoCierre = model.FechaDesde,
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
                    Promedio = LeerDecimal(row, "promedio"),
                    PuntoStock = puntoStock,
                    EstadoStock = estado
                });
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
            model.Mensaje = "Reporte cargado. Los filtros secundarios se aplican en vivo sobre los datos visibles.";
            model.MostrarGrafico = false;
        }

        private void CargarReporteVentasPorProducto(ReportesViewModel model, string[] periodoComparativoDesde)
        {
            model.ConsultaRealizada = true;
            model.FilasVentasProducto.Clear();
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(model.FechaDesde, model.FechaHasta, periodoComparativoDesde);

            var cortes = (oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>())
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

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                var dt = oCorteN.TotalPorCortesVendidos(
                    model.BusquedaProducto,
                    model.SucursalId,
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

                    ReporteVentasProductoFilaVm fila;
                    if (!filas.TryGetValue(filaKey, out fila))
                    {
                        fila = new ReporteVentasProductoFilaVm
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

                    fila.ValoresPeriodo.Add(new ReporteVentasProductoValorVm
                    {
                        PeriodoId = periodo.Id,
                        Kg = LeerDecimalPrimeraCoincidencia(row, "Total Kgs", "Total Kg", "Kgs", "Kg", "cantKg", "CantKg"),
                        Importe = LeerDecimalPrimeraCoincidencia(row, "totalS", "Total $", "Total", "Importe", "Total Importe")
                    });
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

        private void CargarReporteProyeccionVentasVsStock(ReportesViewModel model, string[] periodoComparativoDesde)
        {
            model.ConsultaRealizada = true;
            model.FilasProyeccionVentasStock.Clear();
            model.SucursalesProyeccion.Clear();
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(model.FechaDesde, model.FechaHasta, periodoComparativoDesde);

            var cortes = (oCorteN.findAllCortes(false, 0) ?? new List<Entidades.Corte>())
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

            var cierres = FiltrarCierresPorSucursal(ObtenerCierresDisponibles(), model.SucursalId);
            var fechaUltimoCierreStock = cierres.FirstOrDefault() != null
                ? cierres.First().FechaCompra
                : DateTime.Today;
            var sucursalesBase = oSucursalN.findAll() ?? new List<Entidades.Sucursal>();
            var sucursalesProyeccion = (model.SucursalId > 0
                    ? sucursalesBase.Where(x => x != null && x.IdSucursal == model.SucursalId)
                    : sucursalesBase.Where(x => x != null && x.IdSucursal > 0))
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

            var stockPorSucursal = new Dictionary<int, Dictionary<long, decimal>>();
            foreach (var sucursal in model.SucursalesProyeccion)
            {
                var dtStockActual = oCorteN.CierreStock(
                    1,
                    model.BusquedaProducto,
                    sucursal.IdSucursal,
                    fechaUltimoCierreStock,
                    DateTime.Now,
                    null,
                    model.TipoProducto,
                    0,
                    model.MarcaId);

                var stockPorCodigo = new Dictionary<long, decimal>();
                if (dtStockActual != null)
                {
                    foreach (System.Data.DataRow row in dtStockActual.Rows)
                    {
                        var codigo = LeerLong(row, "Codigo");
                        if (codigo <= 0)
                            continue;

                        stockPorCodigo[codigo] = LeerDecimalPrimeraCoincidencia(row, "Faltante", "DIF");
                    }
                }

                stockPorSucursal[sucursal.IdSucursal] = stockPorCodigo;
            }

            var filas = new Dictionary<string, ReporteProyeccionVentasStockFilaVm>(StringComparer.OrdinalIgnoreCase);

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                foreach (var sucursal in model.SucursalesProyeccion)
                {
                    var dt = oCorteN.acum_Ventas(
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
                        Dictionary<long, decimal> stockSucursalActual;
                        if (codigo > 0
                            && stockPorSucursal.TryGetValue(sucursal.IdSucursal, out stockSucursalActual)
                            && stockSucursalActual != null)
                        {
                            stockSucursalActual.TryGetValue(codigo, out stockActualSucursal);
                        }

                        var stockSucursalVm = fila.StocksPorSucursal.FirstOrDefault(x => x.IdSucursal == sucursal.IdSucursal);
                        if (stockSucursalVm == null)
                        {
                            fila.StocksPorSucursal.Add(new ReporteProyeccionSucursalStockVm
                            {
                                IdSucursal = sucursal.IdSucursal,
                                Sucursal = sucursal.Sucursal,
                                StockActual = stockActualSucursal
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
                        Dictionary<long, decimal> stockSucursalActual;
                        if (fila.Codigo > 0
                            && stockPorSucursal.TryGetValue(sucursal.IdSucursal, out stockSucursalActual)
                            && stockSucursalActual != null)
                        {
                            stockSucursalActual.TryGetValue(fila.Codigo, out stockActualSucursal);
                        }

                        stockSucursal = new ReporteProyeccionSucursalStockVm
                        {
                            IdSucursal = sucursal.IdSucursal,
                            Sucursal = sucursal.Sucursal,
                            StockActual = stockActualSucursal
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
            model.PeriodosComparativosVentas = ConstruirPeriodosComparativosVentas(fechaDesdeBase, fechaHastaBase, periodoComparativoDesde);

            foreach (var periodo in model.PeriodosComparativosVentas)
            {
                var metricas = CalcularBalancePeriodo(
                    periodo,
                    model.SucursalId,
                    model.IncluirVentasCuentaCorrienteBalance);

                model.PeriodosBalance.Add(metricas.Periodo);
                model.GastosBalanceCategorias.AddRange(metricas.GastosPorCategoria);
            }

            var periodoPrincipal = model.PeriodosBalance.FirstOrDefault();
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

        private BalancePeriodoResultado CalcularBalancePeriodo(
            ReportePeriodoComparativoVm periodo,
            int sucursalId,
            bool incluirVentasCuentaCorriente)
        {
            int sucursalConsulta = sucursalId > 0 ? sucursalId : -1;
            DateTime desde = periodo.FechaDesde.Date;
            DateTime hasta = periodo.FechaHasta.Date;
            DateTime hastaFinDia = hasta.AddDays(1).AddMilliseconds(-1);

            var ventas = (oVentaN.getAllVentas(desde, hasta, "", -1, -1, sucursalConsulta, false, false) ?? new List<Entidades.Venta>())
                .Where(x => x != null)
                .Where(x => !string.Equals((x.Estado ?? "").Trim(), "ANULADO", StringComparison.OrdinalIgnoreCase))
                .ToList();

            decimal ventasCuentaCorriente = ventas
                .Where(EsVentaCuentaCorriente)
                .Sum(x => Convert.ToDecimal(x.TotalImporte));
            decimal totalVentas = ventas.Sum(x => Convert.ToDecimal(x.TotalImporte));
            decimal ventasConsumidorFinal = totalVentas - ventasCuentaCorriente;
            decimal ventasEfectivo = ventas.Sum(CalcularVentaEfectivo);
            decimal ventasBancarizadas = ventas.Sum(CalcularVentaBancarizada);

            System.Data.DataTable dtCompras = oCompraN.obtenerCompras(
                sucursalId > 0 ? sucursalId : 0,
                "Todos",
                "",
                desde,
                hastaFinDia,
                null) ?? new System.Data.DataTable();

            decimal totalCompras = dtCompras.AsEnumerable()
                .Where(EsCompraEconomica)
                .Sum(x => LeerDecimal(x, "totalS"));

            var egresosGasto = ObtenerEgresosGastoBalance(sucursalId, desde, hastaFinDia);

            decimal totalGastos = egresosGasto.Sum(x => LeerDecimalPrimeraCoincidencia(x, "monto", "Monto"));
            var gastosPorCategoria = egresosGasto
                .GroupBy(x =>
                {
                    string categoria = LeerString(x, "TipoEgresoCaja");
                    if (string.IsNullOrWhiteSpace(categoria))
                        categoria = LeerString(x, "tipoEgresoCaja");
                    return string.IsNullOrWhiteSpace(categoria) ? "Otros gastos" : categoria;
                })
                .Select(g => new ReporteBalanceGastoCategoriaVm
                {
                    PeriodoId = periodo.Id,
                    Categoria = g.Key,
                    Total = g.Sum(x => LeerDecimalPrimeraCoincidencia(x, "monto", "Monto"))
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            System.Data.DataTable dtPagos = oCuentaCorrienteN.obtenerPagos("", desde, hasta) ?? new System.Data.DataTable();
            decimal totalCobros = 0m;
            decimal totalPagos = 0m;

            foreach (System.Data.DataRow row in dtPagos.Rows)
            {
                int idPago = LeerInt(row, "id");
                if (idPago <= 0)
                    continue;

                var pago = oCuentaCorrienteN.getPagoById(idPago);
                if (pago == null)
                    continue;

                if (sucursalId > 0 && pago.IdSucursal != sucursalId)
                    continue;

                decimal importePago = Convert.ToDecimal(pago.Importe);
                if (pago.AProveedor)
                    totalPagos += importePago;
                else
                    totalCobros += importePago;
            }

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

        private List<DataRow> ObtenerEgresosGastoBalance(int sucursalId, DateTime desde, DateTime hasta)
        {
            var filas = new List<DataRow>();

            var sucursalesConsulta = (sucursalId > 0
                    ? (oSucursalN.findAll() ?? new List<Entidades.Sucursal>()).Where(x => x != null && x.IdSucursal == sucursalId)
                    : (oSucursalN.findAll() ?? new List<Entidades.Sucursal>()).Where(x => x != null && x.IdSucursal > 0))
                .GroupBy(x => x.IdSucursal)
                .Select(x => x.First())
                .ToList();

            foreach (var sucursal in sucursalesConsulta)
            {
                System.Data.DataTable dtEgresos = oCierreN.obtenerEgresosCajaGastosBalance(
                    sucursal.IdSucursal,
                    desde,
                    hasta) ?? new System.Data.DataTable();

                foreach (System.Data.DataRow row in dtEgresos.Rows)
                    filas.Add(row);
            }

            return filas;
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
            System.Data.DataTable dt = oCompraN.obtenerCompras(
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
            var valor = Request != null ? Request[key] : null;
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

        private int LeerInt(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToInt32(row[columna], CultureInfo.InvariantCulture)
                : 0;
        }

        private long LeerLong(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToInt64(row[columna], CultureInfo.InvariantCulture)
                : 0L;
        }

        private string LeerString(System.Data.DataRow row, string columna)
        {
            return row != null && row.Table.Columns.Contains(columna) && row[columna] != DBNull.Value
                ? Convert.ToString(row[columna])
                : "";
        }

        private string NormalizarTextoComparacion(string valor)
        {
            return string.IsNullOrWhiteSpace(valor) ? "" : valor.Trim();
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

        private ReporteVentasProductoValorVm ObtenerValorPeriodo(ReporteVentasProductoFilaVm fila, string periodoId)
        {
            return (fila != null ? fila.ValoresPeriodo : null)?
                .FirstOrDefault(x => string.Equals(x.PeriodoId, periodoId, StringComparison.OrdinalIgnoreCase))
                ?? new ReporteVentasProductoValorVm { PeriodoId = periodoId, Kg = 0m, Importe = 0m };
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
            public string EtiquetaRelativa { get; set; }
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
