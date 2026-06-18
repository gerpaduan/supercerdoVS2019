using System.Collections.Generic;

namespace Web.Models
{
    public class CatalogoGlobalProductosVm
    {
        public string Busqueda { get; set; }
        public List<ProductoGlobalImportItemVm> Productos { get; set; }

        public CatalogoGlobalProductosVm()
        {
            Productos = new List<ProductoGlobalImportItemVm>();
        }
    }
}
