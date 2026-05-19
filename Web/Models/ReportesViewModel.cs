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
        public bool Buscar { get; set; }
        public bool ConsultaRealizada { get; set; }
        public bool HayResultados { get; set; }
        public bool UsaCierreEnFechaDesde { get; set; }
        public bool UsaCierreEnFechaHasta { get; set; }
        public bool MostrarFiltroBusquedaProducto { get; set; }
        public bool MostrarFiltroTipoProducto { get; set; }
        public bool MostrarFiltroMarca { get; set; }
        public bool MostrarFiltroEstadoStock { get; set; }
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
        public List<ReporteCierreOptionVm> CierresDisponibles { get; set; }
        public ReporteTotalesVm Totales { get; set; }
        public List<ReporteStockFilaVm> FilasStockActual { get; set; }

        public ReportesViewModel()
        {
            TipoReporteSeleccionado = "";
            BusquedaProducto = "";
            TipoProducto = "";
            EstadoStock = "Todos";
            Mensaje = "";
            NotaReporte = "";
            FechaDesdeValor = "";
            FechaHastaValor = "";
            TiposReporte = new List<SelectListItem>();
            Sucursales = new List<SelectListItem>();
            Marcas = new List<SelectListItem>();
            TiposProducto = new List<SelectListItem>();
            EstadosStock = new List<SelectListItem>();
            CierresDisponibles = new List<ReporteCierreOptionVm>();
            Totales = new ReporteTotalesVm();
            FilasStockActual = new List<ReporteStockFilaVm>();
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
        public decimal Promedio { get; set; }
        public decimal PuntoStock { get; set; }
        public string EstadoStock { get; set; }
    }
}
