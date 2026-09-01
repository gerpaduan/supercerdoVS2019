using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    // StockLineaDetalleVm ya existe en CompraIndexVm.cs (mismo namespace) -- no se redefine aca.

    public class CabeceraDetalleCampoVm
    {
        public string Etiqueta { get; set; } = "";
        public string Valor { get; set; } = "";
    }

    public class StockLineasIndexVm
    {
        public StockLineasIndexVm()
        {
            Registros = new List<StockLineasGrupoVm>();
        }

        public int IdSucursal { get; set; }
        public string TipoCompra { get; set; } = "";
        public string Producto { get; set; } = "";
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
        public string CollapseId { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Subtitulo { get; set; } = "";
        public string ResumenCompacto { get; set; } = "";
        public string ResumenSecundario { get; set; } = "";
        public string EditUrl { get; set; } = "";
        public decimal TotalKg { get; set; }
        public List<CabeceraDetalleCampoVm> Campos { get; set; }
        public List<StockLineaDetalleVm> Lineas { get; set; }
    }
}
