using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class StockEditVm
    {
        public int IdCompra { get; set; }

        public bool EsEdicion { get; set; }
        public bool SoloLecturaInicial { get; set; }
        public bool PuedeHabilitarEdicion { get; set; }

        public string TipoCompra { get; set; } = "";

        public int IdSucursal { get; set; }

        public string SucursalNombre { get; set; } = "";

        public DateTime FechaCompra { get; set; }

        public string Observaciones { get; set; } = "";

        public string Estado { get; set; } = "";

        // Quien esta operando esta pantalla ahora -- con la cuenta de produccion, es el
        // operador real ya seleccionado antes de entrar (ver StockController.Editar,
        // docs/DECISIONS.md), no "User Produccion". Mismo campo que ya usan Movimientos y
        // Elaborados.
        public string UsuarioNombre { get; set; } = "";

        public int IdProveedor { get; set; }

        public string ProveedorNombre { get; set; } = "";

        public string ProveedorCuit { get; set; } = "";

        public int? CantMedias { get; set; }

        public float? KgsMedias { get; set; }

        public bool GuardarSinPesaje { get; set; }

        public string Creado { get; set; } = "";

        public string CreadoPor { get; set; } = "";

        public string Actualizado { get; set; } = "";

        public string ActualizadoPor { get; set; } = "";

        public int? IdPesajeAjustado { get; set; }

        public DateTime? FechaCompraVinculada { get; set; }

        public string ProveedorCompraVinculada { get; set; } = "";

        public int? CantMediasCompraVinculada { get; set; }

        public float? KgsCompraVinculada { get; set; }

        public string EstadoCompraVinculada { get; set; } = "";

        public bool CompraVinculadaEsPesaje { get; set; }

        public DateTime? FechaPesajeAjustado { get; set; }

        public string ProveedorPesajeAjustado { get; set; } = "";

        public int? CantMediasPesajeAjustado { get; set; }

        public float? KgsPesajeAjustado { get; set; }

        public string EstadoPesajeAjustado { get; set; } = "";

        public List<StockLineaVm> Lineas { get; set; }

        public List<int> PesajesVinculadosIds { get; set; }

        public int CantItems { get; set; }

        public float TotalKg { get; set; }

        public string DraftKey { get; set; } = "";

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

        public string Producto { get; set; } = "";

        public float CantKgs { get; set; }

        public bool Balanza { get; set; }

        public string CreadoTexto { get; set; } = "";

        public bool Pesable { get; set; }

        public int? IdPesajeVinculado { get; set; }

        public string PesajeVinculadoTexto { get; set; } = "";
    }
}
