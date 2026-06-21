using System;
using System.Collections.Generic;
using System.Data;

namespace Web.Models
{
    public class CompraIndexVm
    {
        public DataTable Compras { get; set; }

        public Dictionary<int, CompraIndexDetalleVm> Detalles { get; set; }

        public CompraIndexVm()
        {
            Compras = new DataTable();
            Detalles = new Dictionary<int, CompraIndexDetalleVm>();
        }
    }

    public class CompraIndexDetalleVm
    {
        public CompraIndexDetalleVm()
        {
            Lineas = new List<StockLineaDetalleVm>();
        }

        public int IdCompra { get; set; }
        public DateTime? FechaCompra { get; set; }
        public string NumeroDocumento { get; set; }
        public string Proveedor { get; set; }
        public string TipoCompra { get; set; }
        public float Cantidad { get; set; }
        public int CantidadMedias { get; set; }
        public float Total { get; set; }
        public string Sucursal { get; set; }
        public string Observaciones { get; set; }
        public string Estado { get; set; }
        public bool EnCtaCte { get; set; }
        public int? IdCompraVinculada { get; set; }
        public DateTime? FechaCompraVinculada { get; set; }
        public string ProveedorCompraVinculada { get; set; }
        public int? CantMediasCompraVinculada { get; set; }
        public float? KgsCompraVinculada { get; set; }
        public string EstadoCompraVinculada { get; set; }
        public int? IdPesajeRelacionado { get; set; }
        public int? IdAjusteRelacionado { get; set; }
        public DateTime? FechaPesajeRelacionado { get; set; }
        public string ProveedorPesajeRelacionado { get; set; }
        public int? CantMediasPesajeRelacionado { get; set; }
        public float? KgsPesajeRelacionado { get; set; }
        public string EstadoPesajeRelacionado { get; set; }
        public bool EsPesaje { get; set; }
        public bool EsAjuste { get; set; }
        public string UsuarioCreacion { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public string UsuarioActualizacion { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public List<StockLineaDetalleVm> Lineas { get; set; }
    }
}
