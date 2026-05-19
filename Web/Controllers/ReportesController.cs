using Entidades;
using System;
using System.Collections.Generic;
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

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            if (filterContext.Result != null) return;

            oSucursalN = new Negocio.Sucursal(empresa, param);
            oCorteN = new Negocio.Corte(empresa, param);
            oCompraN = new Negocio.Compra(empresa, param);
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
            string estadoStock = "Todos",
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
                estadoStock,
                buscar);

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

        private ReportesViewModel CrearModeloBase(
            Entidades.Usuario user,
            string tipoReporte,
            DateTime fechaDesde,
            DateTime fechaHasta,
            int sucursalId,
            string busquedaProducto,
            string tipoProducto,
            int marcaId,
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

            model.MostrarFiltroBusquedaProducto = esReporteStock;
            model.MostrarFiltroTipoProducto = esReporteStock;
            model.MostrarFiltroMarca = esReporteStock;
            model.MostrarFiltroEstadoStock = esReporteStock;
            model.MostrarGrafico = false;
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
                ? Convert.ToDecimal(row[columna], CultureInfo.InvariantCulture)
                : 0m;
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

        private string CalcularEstadoStock(decimal stockActual, decimal puntoStock)
        {
            if (stockActual < 0) return "NEGATIVO";
            if (puntoStock > 0 && stockActual <= puntoStock) return "BAJO";
            if (stockActual == 0) return "SIN STOCK";
            return "OK";
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
