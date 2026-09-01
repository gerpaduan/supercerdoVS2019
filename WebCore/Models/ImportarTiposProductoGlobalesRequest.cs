// Port de Web/Models/ImportarTiposProductoGlobalesRequest.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;

namespace WebCore.Models
{
    public class ImportarTiposProductoGlobalesRequest
    {
        public List<TipoProductoGlobalSeleccionVm> Tipos { get; set; } = new List<TipoProductoGlobalSeleccionVm>();
    }
}
