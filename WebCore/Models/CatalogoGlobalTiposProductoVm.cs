// Port de Web/Models/CatalogoGlobalTiposProductoVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;

namespace WebCore.Models
{
    public class CatalogoGlobalTiposProductoVm
    {
        public string Busqueda { get; set; } = "";
        public List<TipoProductoGlobalImportItemVm> Tipos { get; set; } = new List<TipoProductoGlobalImportItemVm>();
    }
}
