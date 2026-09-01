// Port de Web/Models/TipoProductoGlobalImportItemVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
namespace WebCore.Models
{
    public class TipoProductoGlobalImportItemVm
    {
        public string Tipo { get; set; } = "";
        public int Orden { get; set; }
        public bool YaExisteEnEmpresa { get; set; }
        public string MensajeEstado { get; set; } = "";
    }
}
