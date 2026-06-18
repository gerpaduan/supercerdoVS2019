using System.Collections.Generic;

namespace Web.Models
{
    public class ImportarProductosGlobalesRequest
    {
        public List<ProductoGlobalSeleccionVm> Productos { get; set; }

        public ImportarProductosGlobalesRequest()
        {
            Productos = new List<ProductoGlobalSeleccionVm>();
        }
    }
}
