using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Web.Models
{
    public class ReportesViewModel
    {
        public string TipoReporteSeleccionado { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int SucursalId { get; set; }
        public string BusquedaProducto { get; set; }
        public string TipoProducto { get; set; }
        public int MarcaId { get; set; }
        public string EstadoStock { get; set; }
        public string AgrupacionTemporal { get; set; }
        public bool Buscar { get; set; }
        public bool ConsultaRealizada { get; set; }
        public bool HayResultados { get; set; }
        public bool UsaCierreEnFechaDesde { get; set; }
        public bool UsaCierreEnFechaHasta { get; set; }
        public bool MostrarFiltroBusquedaProducto { get; set; }
        public bool MostrarFiltroTipoProducto { get; set; }
        public bool MostrarFiltroMarca { get; set; }
        public bool MostrarFiltroEstadoStock { get; set; }
        public bool MostrarFiltroAgrupacionTemporal { get; set; }
        public bool MostrarGrafico { get; set; }
        public string Mensaje { get; set; }
        public string NotaReporte { get; set; }
        public string FechaDesdeValor { get; set; }
        public string FechaHastaValor { get; set; }
        public List<SelectListItem> TiposReporte { get; set; }
        public List<SelectListItem> Sucursales { get; set; }
        public List<SelectListItem> Marcas { get; set; }
        public List<SelectListItem> TiposProducto { get; set; }
        public List<SelectListItem> EstadosStock { get; set; }
        public List<SelectListItem> AgrupacionesTemporales { get; set; }
        public List<ReporteCierreOptionVm> CierresDisponibles { get; set; }
        public ReporteTotalesVm Totales { get; set; }
        public List<ReporteStockFilaVm> FilasStockActual { get; set; }
        public List<ReportePeriodoComparativoVm> PeriodosComparativosVentas { get; set; }
        public List<ReporteVentasProductoFilaVm> FilasVentasProducto { get; set; }
        public List<ReporteProyeccionVentasStockFilaVm> FilasProyeccionVentasStock { get; set; }
        public List<ReporteProyeccionSucursalVm> SucursalesProyeccion { get; set; }
        public bool IncluirVentasCuentaCorrienteBalance { get; set; }
        public List<ReporteBalancePeriodoVm> PeriodosBalance { get; set; }
        public List<ReporteBalanceGastoCategoriaVm> GastosBalanceCategorias { get; set; }

        public ReportesViewModel()
        {
            TipoReporteSeleccionado = "";
            BusquedaProducto = "";
            TipoProducto = "";
            EstadoStock = "Todos";
            AgrupacionTemporal = "hora";
            Mensaje = "";
            NotaReporte = "";
            FechaDesdeValor = "";
            FechaHastaValor = "";
            TiposReporte = new List<SelectListItem>();
            Sucursales = new List<SelectListItem>();
            Marcas = new List<SelectListItem>();
            TiposProducto = new List<SelectListItem>();
            EstadosStock = new List<SelectListItem>();
            AgrupacionesTemporales = new List<SelectListItem>();
            CierresDisponibles = new List<ReporteCierreOptionVm>();
            Totales = new ReporteTotalesVm();
            FilasStockActual = new List<ReporteStockFilaVm>();
            PeriodosComparativosVentas = new List<ReportePeriodoComparativoVm>();
            FilasVentasProducto = new List<ReporteVentasProductoFilaVm>();
            FilasProyeccionVentasStock = new List<ReporteProyeccionVentasStockFilaVm>();
            SucursalesProyeccion = new List<ReporteProyeccionSucursalVm>();
            PeriodosBalance = new List<ReporteBalancePeriodoVm>();
            GastosBalanceCategorias = new List<ReporteBalanceGastoCategoriaVm>();
        }
    }

    public class ReporteCierreOptionVm
    {
        public int IdCompra { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public DateTime FechaCompra { get; set; }
        public string ValorIso { get; set; }
        public string Texto { get; set; }
    }

    public class ReporteTotalesVm
    {
        public decimal TotalKg { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TotalEgresos { get; set; }
        public int CantidadProductos { get; set; }
        public int CantidadRegistros { get; set; }
    }

    public class ReporteStockFilaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public string TipoProducto { get; set; }
        public int MarcaId { get; set; }
        public string Marca { get; set; }
        public DateTime? FechaUltimoCierre { get; set; }
        public decimal StockInicial { get; set; }
        public decimal Compras { get; set; }
        public decimal IngresoElaborado { get; set; }
        public decimal IngresoStock { get; set; }
        public decimal IngresoMovimiento { get; set; }
        public decimal AjusteStock { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal EgresoStock { get; set; }
        public decimal EgresoMovimiento { get; set; }
        public decimal EgresoElaborado { get; set; }
        public decimal Ventas { get; set; }
        public decimal TotalEgresos { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockCierre { get; set; }
        public decimal Promedio { get; set; }
        public decimal PuntoStock { get; set; }
        public bool Pesable { get; set; }
        public string EstadoStock { get; set; }
    }

    public class ReportePeriodoComparativoVm
    {
        public string Id { get; set; }
        public string Titulo { get; set; }
        public bool EsPrincipal { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string FechaDesdeValor
        {
            get { return FechaDesde.ToString("yyyy-MM-ddTHH:mm"); }
        }
        public string FechaHastaValor
        {
            get { return FechaHasta.ToString("yyyy-MM-ddTHH:mm"); }
        }
    }

    public class ReporteVentasProductoFilaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public string TipoProducto { get; set; }
        public int MarcaId { get; set; }
        public string Marca { get; set; }
        public List<ReporteVentasProductoValorVm> ValoresPeriodo { get; set; }

        public ReporteVentasProductoFilaVm()
        {
            ValoresPeriodo = new List<ReporteVentasProductoValorVm>();
        }
    }

    public class ReporteVentasProductoValorVm
    {
        public string PeriodoId { get; set; }
        public decimal Kg { get; set; }
        public decimal Importe { get; set; }
    }

    public class ReporteProyeccionVentasStockFilaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public string TipoProducto { get; set; }
        public int MarcaId { get; set; }
        public string Marca { get; set; }
        public decimal StockActual { get; set; }
        public decimal Promedio { get; set; }
        public bool Pesable { get; set; }
        public List<ReporteProyeccionVentasStockValorVm> ValoresPeriodo { get; set; }
        public List<ReporteProyeccionSucursalStockVm> StocksPorSucursal { get; set; }
        public List<ReporteProyeccionVentasStockSucursalValorVm> ValoresPeriodoSucursal { get; set; }

        public ReporteProyeccionVentasStockFilaVm()
        {
            ValoresPeriodo = new List<ReporteProyeccionVentasStockValorVm>();
            StocksPorSucursal = new List<ReporteProyeccionSucursalStockVm>();
            ValoresPeriodoSucursal = new List<ReporteProyeccionVentasStockSucursalValorVm>();
        }
    }

    public class ReporteProyeccionVentasStockValorVm
    {
        public string PeriodoId { get; set; }
        public decimal Ventas { get; set; }
        public decimal Diferencia { get; set; }
    }

    public class ReporteProyeccionSucursalVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
    }

    public class ReporteProyeccionSucursalStockVm
    {
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public decimal StockActual { get; set; }
        public decimal Promedio { get; set; }
        public bool Pesable { get; set; }
    }

    public class ReporteProyeccionVentasStockSucursalValorVm
    {
        public string PeriodoId { get; set; }
        public int IdSucursal { get; set; }
        public string Sucursal { get; set; }
        public decimal Ventas { get; set; }
        public decimal Diferencia { get; set; }
    }

    public class ReporteBalancePeriodoVm
    {
        public string PeriodoId { get; set; }
        public string Titulo { get; set; }
        public bool EsPrincipal { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public decimal VentasConsumidorFinal { get; set; }
        public decimal VentasCuentaCorriente { get; set; }
        public decimal VentasEfectivo { get; set; }
        public decimal VentasBancarizadas { get; set; }
        public decimal TotalVentas { get; set; }
        public decimal VentasConsideradasGrafico { get; set; }
        public decimal KilosVendidos { get; set; }
        public decimal KilosVendidosGrafico { get; set; }
        public decimal KilosVendidosPesables { get; set; }
        public decimal KilosVendidosPesablesGrafico { get; set; }
        public int CantidadVentas { get; set; }
        public int CantidadVentasGrafico { get; set; }
        public decimal Compras { get; set; }
        public decimal Gastos { get; set; }
        public decimal BalanceEconomico { get; set; }
        public decimal Cobros { get; set; }
        public decimal Pagos { get; set; }
        public decimal DiferenciaCobrosPagos { get; set; }
    }

    public class ReporteBalanceGastoCategoriaVm
    {
        public string PeriodoId { get; set; }
        public string Categoria { get; set; }
        public decimal Total { get; set; }
    }
}
