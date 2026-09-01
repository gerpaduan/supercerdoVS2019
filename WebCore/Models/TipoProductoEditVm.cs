// Port de Web/Models/TipoProductoEditVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
namespace WebCore.Models
{
    public class TipoProductoEditVm
    {
        public string TipoOriginal { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int Orden { get; set; }
        public bool Reservado { get; set; }
    }
}
