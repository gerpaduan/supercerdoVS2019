// Port de Web/Models/CatalogoGlobalProductosVm.cs (ver docs/DECISIONS.md, migracion ASP.NET Core).
using System.Collections.Generic;

namespace WebCore.Models
{
    public class CatalogoGlobalProductosVm
    {
        public string Busqueda { get; set; } = "";
        public string Tipo { get; set; } = "";
        public int Pagina { get; set; } = 1;
        public bool HayMas { get; set; }
        public List<string> Tipos { get; set; } = new List<string>();
        public List<ProductoGlobalImportItemVm> Productos { get; set; } = new List<ProductoGlobalImportItemVm>();
    }
}
