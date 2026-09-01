// Port de Web/Models/ImportarProductosGlobalesRequest.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;

namespace WebCore.Models
{
    public class ImportarProductosGlobalesRequest
    {
        public List<ProductoGlobalSeleccionVm> Productos { get; set; } = new List<ProductoGlobalSeleccionVm>();
    }
}
