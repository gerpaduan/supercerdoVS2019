// Port de Web/Models/FacturasVm.cs y Web/Models/LineasAgrupadasVm.cs (solo la porcion de Ventas)
// para el Modulo 8 (Ventas y POS), slice 1 -- ver docs/DECISIONS.md. CabeceraDetalleCampoVm ya
// existe en WebCore/Models/StockLineasIndexVm.cs (reutilizado por Stock/Compras), no se duplica.
using System;
using System.Collections.Generic;
using Entidades;

namespace WebCore.Models
{
    // Unica fuente de verdad para el mapeo Etiqueta <-> Codigo AFIP de tipo de comprobante en la
    // pantalla de Facturas. Se usa para: armar el dropdown de filtro, armar la etiqueta de cada
    // fila, y traducir las etiquetas elegidas a codigos AFIP para el filtro server-side
    // (VentasController.BuscarFacturas).
    public static class TipoComprobanteFacturas
    {
        public static readonly List<(string Etiqueta, int Codigo)> Mapeo = new List<(string, int)>
        {
            ("Factura A", FacturaElectronica.codFacturaA_Afip),
            ("NC A", FacturaElectronica.codNotaCreditoA_Afip),
            ("Factura B", FacturaElectronica.codFacturaB_Afip),
            ("NC B", FacturaElectronica.codNotaCreditoB_Afip),
            ("Factura C", FacturaElectronica.codFacturaC_Afip),
            ("NC C", FacturaElectronica.codNotaCreditoC_Afip),
        };

        public static string ObtenerEtiqueta(int codigoAfip)
        {
            foreach (var par in Mapeo)
            {
                if (par.Codigo == codigoAfip)
                    return par.Etiqueta;
            }

            return "";
        }

        public static List<int> ObtenerCodigos(IEnumerable<string> etiquetas)
        {
            var codigos = new List<int>();
            if (etiquetas == null)
                return codigos;

            foreach (var etiqueta in etiquetas)
            {
                foreach (var par in Mapeo)
                {
                    if (string.Equals(par.Etiqueta, etiqueta, StringComparison.OrdinalIgnoreCase))
                    {
                        codigos.Add(par.Codigo);
                        break;
                    }
                }
            }

            return codigos;
        }
    }

    public class FacturasIndexVm
    {
        public FacturasIndexVm()
        {
            Facturas = new List<FacturaListadoItemVm>();
        }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int IdSucursal { get; set; }
        public string Cliente { get; set; }
        public string Vendedor { get; set; }
        // Filtro EXACTO por ID del selector multi-seleccion (CSV de IDs ya validados). Si hay IDs, el
        // texto Cliente/Vendedor no se usa; las listas traen los nombres para pintar el input.
        public string IdsCliente { get; set; } = "";
        public string IdsVendedor { get; set; } = "";
        public List<WebCore.Services.FiltroPickerItem> ClientesSeleccionados { get; set; } = new List<WebCore.Services.FiltroPickerItem>();
        public List<WebCore.Services.FiltroPickerItem> VendedoresSeleccionados { get; set; } = new List<WebCore.Services.FiltroPickerItem>();
        public string FormasPagoCsv { get; set; }
        public string TiposComprobanteCsv { get; set; }
        // Filtro de entorno: "PROD" (default), "PRUEBA" o "TODAS". Solo se muestra si hay facturas de
        // prueba (MostrarFiltroEntorno); si no, no filtra nada.
        public string Entorno { get; set; }
        public bool MostrarFiltroEntorno { get; set; }
        // Cantidad/TotalFacturado son del filtro completo (Negocio.Venta.ObtenerFacturasResumen),
        // no de Facturas.Count -- que con paginacion solo trae la pagina actual (50).
        public int Cantidad { get; set; }
        public decimal TotalFacturado { get; set; }
        public bool HayMas { get; set; }
        public List<FacturaListadoItemVm> Facturas { get; set; }
    }

    public class FacturaListadoItemVm
    {
        public FacturaElectronica Factura { get; set; }
        public Venta Venta { get; set; }
        public FacturaElectronica FacturaAsociada { get; set; }
        public FacturaElectronica NotaCreditoAsociada { get; set; }
    }

    public class FacturaDetalleVm
    {
        public FacturaElectronica Factura { get; set; }
        public Venta Venta { get; set; }
        public string ReturnUrl { get; set; }
        public FacturaElectronica FacturaAsociada { get; set; }
        public FacturaElectronica NotaCreditoAsociada { get; set; }
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
        // Filtro EXACTO por ID del selector multi-seleccion (CSV de IDs ya validados). Si hay IDs, el
        // texto Cliente/Vendedor no se usa; las listas traen los nombres (pintar el input y el
        // encabezado de los exports PDF/Excel).
        public string IdsCliente { get; set; } = "";
        public string IdsVendedor { get; set; } = "";
        public List<WebCore.Services.FiltroPickerItem> ClientesSeleccionados { get; set; } = new List<WebCore.Services.FiltroPickerItem>();
        public List<WebCore.Services.FiltroPickerItem> VendedoresSeleccionados { get; set; } = new List<WebCore.Services.FiltroPickerItem>();
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
        // Fecha y hora reales de la venta (ResumenCompacto es solo el texto): la usa el Excel compacto para ordenar.
        public DateTime FechaVenta { get; set; }
        // Fecha de creacion del registro: la usa la vista "Ver solo lineas" para ordenar por Creado.
        public DateTime Creado { get; set; }
        // IDs exactos de la persona y del vendedor de la venta: el filtro en vivo del selector
        // multi-seleccion los compara contra data-id-cliente / data-id-vendedor de la tarjeta.
        public int IdCliente { get; set; }
        public int IdVendedor { get; set; }
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
}
