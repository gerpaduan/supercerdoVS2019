// Port de Web/Models/ProductoGlobalSeleccionVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
namespace WebCore.Models
{
    public class ProductoGlobalSeleccionVm
    {
        public int IdProductoGlobal { get; set; }
        public long CodigoDestino { get; set; }
        public string Precio { get; set; } = "";
    }
}
