using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class CabeceraDetalleCampoVm
    {
        public string Etiqueta { get; set; }
        public string Valor { get; set; }
    }

    public class VentaLineasIndexVm
    {
        public VentaLineasIndexVm()
        {
            Ventas = new List<VentaLineasGrupoVm>();
            FormasPagoSeleccionadas = new List<string>();
        }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int IdSucursal { get; set; }
        public string Cliente { get; set; }
        public string Vendedor { get; set; }
        public string Producto { get; set; }
        public string FormasPagoCsv { get; set; }
        public List<string> FormasPagoSeleccionadas { get; set; }
        public List<VentaLineasGrupoVm> Ventas { get; set; }
    }

    public class VentaLineasGrupoVm
    {
        public VentaLineasGrupoVm()
        {
            Campos = new List<CabeceraDetalleCampoVm>();
            Lineas = new List<VentaLineaDetalleVm>();
        }

        public int IdVenta { get; set; }
        public string CollapseId { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string ResumenCompacto { get; set; }
        public string ResumenSecundario { get; set; }
        public string TotalTexto { get; set; }
        public string EditUrl { get; set; }
        public decimal TotalImporte { get; set; }
        public decimal TotalKg { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<VentaLineaDetalleVm> Lineas { get; set; }
    }

    public class VentaLineaDetalleVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public string CantidadKgTexto { get; set; }
        public string PrecioTexto { get; set; }
        public string TotalTexto { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }

    public class CompraLineasIndexVm
    {
        public CompraLineasIndexVm()
        {
            Compras = new List<CompraLineasGrupoVm>();
        }

        public int IdSucursal { get; set; }
        public string TipoCompra { get; set; }
        public string Texto { get; set; }
        public string Producto { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public bool PermiteMediaRes { get; set; }
        public List<CompraLineasGrupoVm> Compras { get; set; }
    }

    public class CompraLineasGrupoVm
    {
        public CompraLineasGrupoVm()
        {
            Campos = new List<CabeceraDetalleCampoVm>();
            Lineas = new List<CompraLineaDetalleVm>();
        }

        public int IdCompra { get; set; }
        public string CollapseId { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string ResumenCompacto { get; set; }
        public string ResumenSecundario { get; set; }
        public string TotalTexto { get; set; }
        public string EditUrl { get; set; }
        public decimal TotalImporte { get; set; }
        public decimal TotalKg { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<CompraLineaDetalleVm> Lineas { get; set; }
    }

    public class CompraLineaDetalleVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public string CantidadKgTexto { get; set; }
        public string PrecioTexto { get; set; }
        public string TotalTexto { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }

    public class StockLineasIndexVm
    {
        public StockLineasIndexVm()
        {
            Registros = new List<StockLineasGrupoVm>();
        }

        public int IdSucursal { get; set; }
        public string TipoCompra { get; set; }
        public string Producto { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public List<StockLineasGrupoVm> Registros { get; set; }
    }

    public class StockLineasGrupoVm
    {
        public StockLineasGrupoVm()
        {
            Campos = new List<CabeceraDetalleCampoVm>();
            Lineas = new List<StockLineaDetalleVm>();
        }

        public int IdCompra { get; set; }
        public string CollapseId { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string ResumenCompacto { get; set; }
        public string ResumenSecundario { get; set; }
        public string EditUrl { get; set; }
        public decimal TotalKg { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<StockLineaDetalleVm> Lineas { get; set; }
    }

    public class StockLineaDetalleVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public string CantidadKgTexto { get; set; }
        public string Signo { get; set; }
        public string Observacion { get; set; }
        public bool Balanza { get; set; }
        public string CreadoTexto { get; set; }
        public decimal CantidadKg { get; set; }
    }

    public class MovimientoLineasIndexPageVm
    {
        public MovimientoLineasIndexPageVm()
        {
            Movimientos = new List<MovimientoLineasGrupoVm>();
        }

        public int IdSucursalOrigen { get; set; }
        public int IdSucursalDestino { get; set; }
        public string Producto { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public List<MovimientoLineasGrupoVm> Movimientos { get; set; }
    }

    public class MovimientoLineasGrupoVm
    {
        public MovimientoLineasGrupoVm()
        {
            Campos = new List<CabeceraDetalleCampoVm>();
            Lineas = new List<MovimientoLineaDetalleItemVm>();
        }

        public int IdMovimiento { get; set; }
        public string CollapseId { get; set; }
        public string Titulo { get; set; }
        public string Subtitulo { get; set; }
        public string ResumenCompacto { get; set; }
        public string ResumenSecundario { get; set; }
        public string EditUrl { get; set; }
        public decimal TotalKg { get; set; }
        public decimal TotalUnidades { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<MovimientoLineaDetalleItemVm> Lineas { get; set; }
    }

    public class MovimientoLineaDetalleItemVm
    {
        public string Codigo { get; set; }
        public string Producto { get; set; }
        public string CantidadKgTexto { get; set; }
        public string CantidadUnidadTexto { get; set; }
        public string Observacion { get; set; }
        public decimal CantidadKg { get; set; }
        public decimal CantidadUnidad { get; set; }
    }
}
