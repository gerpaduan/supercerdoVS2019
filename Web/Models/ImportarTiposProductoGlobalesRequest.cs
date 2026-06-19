using System.Collections.Generic;

namespace Web.Models
{
    public class ImportarTiposProductoGlobalesRequest
    {
        public List<TipoProductoGlobalSeleccionVm> Tipos { get; set; }

        public ImportarTiposProductoGlobalesRequest()
        {
            Tipos = new List<TipoProductoGlobalSeleccionVm>();
        }
    }
}
