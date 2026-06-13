using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class StockEditVm
    {
        public int IdCompra { get; set; }

        public bool EsEdicion { get; set; }

        public string TipoCompra { get; set; }

        public int IdSucursal { get; set; }

        public string SucursalNombre { get; set; }

        public DateTime FechaCompra { get; set; }

        public string Observaciones { get; set; }

        public string Estado { get; set; }

        public int IdProveedor { get; set; }

        public string ProveedorNombre { get; set; }

        public string ProveedorCuit { get; set; }

        public int? CantMedias { get; set; }

        public float? KgsMedias { get; set; }

        public string Creado { get; set; }

        public string CreadoPor { get; set; }

        public string Actualizado { get; set; }

        public string ActualizadoPor { get; set; }

        public List<StockLineaVm> Lineas { get; set; }

        public List<int> PesajesVinculadosIds { get; set; }

        public int CantItems { get; set; }

        public float TotalKg { get; set; }

        public string DraftKey { get; set; }

        public StockEditVm()
        {
            TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.IngresoStock);
            FechaCompra = DateTime.Now;
            Lineas = new List<StockLineaVm>();
            PesajesVinculadosIds = new List<int>();
        }
    }

    public class StockLineaVm
    {
        public int Index { get; set; }

        public int? IdCorte { get; set; }

        public long? Codigo { get; set; }

        public string Producto { get; set; }

        public float CantKgs { get; set; }

        public bool Balanza { get; set; }

        public string CreadoTexto { get; set; }

        public bool Pesable { get; set; }

        public int? IdPesajeVinculado { get; set; }

        public string PesajeVinculadoTexto { get; set; }
    }
}
