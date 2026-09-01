// Port de Web/Models/ProductoGlobalImportItemVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
namespace WebCore.Models
{
    public class ProductoGlobalImportItemVm
    {
        public int IdProductoGlobal { get; set; }
        public long CodigoOriginal { get; set; }
        public long CodigoDestino { get; set; }
        public string Descripcion { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int? IdProductoGlobalMaestro { get; set; }
        public string ProductoGlobalMaestroNombre { get; set; } = "";
        public bool EsPresentacion { get; set; }
        public float Porcentaje { get; set; }
        public bool YaImportado { get; set; }
        public int? IdProductoEmpresaImportado { get; set; }
        public long? CodigoEmpresaImportado { get; set; }
        public bool CodigoDuplicadoEnEmpresa { get; set; }
        public long CodigoSugerido { get; set; }
        public string MensajeEstado { get; set; } = "";
    }
}
