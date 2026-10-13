using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    // CabeceraDetalleCampoVm ya existe en StockLineasIndexVm.cs (mismo namespace) -- no se redefine.

    public class CompraLineasIndexVm
    {
        public CompraLineasIndexVm()
        {
            Compras = new List<CompraLineasGrupoVm>();
        }

        public int IdSucursal { get; set; }
        public string TipoCompra { get; set; } = "";
        public string Texto { get; set; } = "";
        // Filtro de Proveedor: texto escrito (solo sin seleccion) y/o seleccion exacta del selector
        // multi-seleccion (CSV de IDs ya validados + nombres para pintar el input).
        public string Proveedor { get; set; } = "";
        public string IdsProveedor { get; set; } = "";
        public List<WebCore.Services.FiltroPickerItem> ProveedoresSeleccionados { get; set; } = new List<WebCore.Services.FiltroPickerItem>();
        public string Producto { get; set; } = "";
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
        // ID exacto del proveedor de la compra: el filtro en vivo del selector multi-seleccion lo
        // compara contra data-id-proveedor de la tarjeta.
        public int IdProveedor { get; set; }
        public string CollapseId { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public string ResumenCompacto { get; set; } = "";
        public string ResumenSecundario { get; set; } = "";
        public string TotalTexto { get; set; } = "";
        public string EditUrl { get; set; } = "";
        public decimal TotalImporte { get; set; }
        public decimal TotalKg { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<CompraLineaDetalleVm> Lineas { get; set; }
    }

    public class CompraLineaDetalleVm
    {
        public string Codigo { get; set; } = "";
        public string Producto { get; set; } = "";
        public string CantidadKgTexto { get; set; } = "";
        public string PrecioTexto { get; set; } = "";
        public string TotalTexto { get; set; } = "";
        public decimal CantidadKg { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }
}
