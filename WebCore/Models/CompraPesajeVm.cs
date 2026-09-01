using System.Collections.Generic;

namespace WebCore.Models
{
    public class ProductoNoCargadoCierreVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; } = "";
        public float StockActual { get; set; }
    }

    public class TablaModalStockVm
    {
        public List<ColumnaModalStockVm> columnas { get; set; }
        public List<List<string>> filas { get; set; }

        public TablaModalStockVm()
        {
            columnas = new List<ColumnaModalStockVm>();
            filas = new List<List<string>>();
        }
    }

    public class ColumnaModalStockVm
    {
        public string nombre { get; set; } = "";
        public bool oculta { get; set; }
        public bool alineacionDerecha { get; set; }
        public bool formatoTresDecimales { get; set; }
    }

    public class CompraPesajeListadoVm
    {
        public int IdCompra { get; set; }
        public int IdProveedor { get; set; }
        public string FechaCompraTexto { get; set; } = "";
        public string Proveedor { get; set; } = "";
        public string TipoCompra { get; set; } = "";
        public int CantMedias { get; set; }
        public float KgsMedias { get; set; }
        public float TotalKg { get; set; }
        public string Sucursal { get; set; } = "";
        public bool EsActual { get; set; }
    }

    public class CompraPesajeSeleccionLineaVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; } = "";
        public string CantidadTexto { get; set; } = "";
        public string KilosTexto { get; set; } = "";
        public bool Pesable { get; set; }
    }
}
