using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Web.Models
{
    public class CompraEditVm
    {
        public int IdCompra { get; set; }

        // Generado fresco en cada GET de Editar.cshtml (nunca reutilizado entre cargas de
        // pagina). ComprasController.Guardar lo usa como clave de deduplicacion -- cualquier
        // reintento de ESTE mismo formulario (doble click, reintento manual, o lo que sea que
        // dispare un segundo POST) queda bloqueado sin importar cuanto tiempo pase entre uno y
        // otro. Ver docs/DECISIONS.md.
        public string SubmissionToken { get; set; }

        public string Origen { get; set; }

        public bool DesdePos { get; set; }

        public bool EsEdicion { get; set; }

        public long EmpresaCuit { get; set; }

        public bool PermiteMediaRes { get; set; }

        public bool SucursalEditable { get; set; }

        public bool PuedeEditar { get; set; }

        [Required]
        public string TipoCompra { get; set; }

        public List<string> TiposCompraDisponibles { get; set; }

        [Required]
        public int IdSucursal { get; set; }

        public string SucursalNombre { get; set; }

        [Required]
        public DateTime FechaCompra { get; set; }

        [Required]
        public int IdProveedor { get; set; }

        public string ProveedorNombre { get; set; }

        public string ProveedorCuit { get; set; }

        public bool EnCtaCte { get; set; }

        public string NroRemito { get; set; }

        public string Observaciones { get; set; }

        public int? CantMedias { get; set; }

        public float? KgsMedias { get; set; }

        public string Creado { get; set; }

        public string CreadoPor { get; set; }

        public string Actualizado { get; set; }

        public string ActualizadoPor { get; set; }

        public List<CompraLineaVm> Lineas { get; set; }

        public int CantItems { get; set; }

        public float TotalKg { get; set; }

        public float TotalImporte { get; set; }

        public string DraftKey { get; set; }

        public CompraEditVm()
        {
            Origen = "layout";
            TipoCompra = Entidades.Compra.tipoCompraToString(Entidades.Compra.tipoCompraEnum.Cortes);
            TiposCompraDisponibles = new List<string>();
            Lineas = new List<CompraLineaVm>();
            FechaCompra = DateTime.Now;
        }
    }

    public class CompraLineaVm
    {
        public int Index { get; set; }

        public string TipoLinea { get; set; }

        public int? IdCorte { get; set; }

        public long? Codigo { get; set; }

        public string CorteNombre { get; set; }

        public float CantKgs { get; set; }

        public float PrecioKg { get; set; }

        public float PrecioVenta { get; set; }

        public bool ActualizarPrecioVenta { get; set; }

        public float Margen { get; set; }

        public float DescRecargo { get; set; }

        public float IvaCompra { get; set; }

        public bool Balanza { get; set; }

        public string NroTropa { get; set; }

        public float KgMedia { get; set; }

        public float PrecioMedia { get; set; }

        public float TotalLinea { get; set; }

        public bool EsMediaRes
        {
            get { return string.Equals(TipoLinea, "MediaRes", StringComparison.OrdinalIgnoreCase); }
        }
    }
}
