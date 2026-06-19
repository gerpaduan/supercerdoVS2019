using System.Collections.Generic;

namespace Web.Models
{
    public class CatalogoGlobalTiposProductoVm
    {
        public string Busqueda { get; set; }
        public List<TipoProductoGlobalImportItemVm> Tipos { get; set; }

        public CatalogoGlobalTiposProductoVm()
        {
            Tipos = new List<TipoProductoGlobalImportItemVm>();
        }
    }
}
