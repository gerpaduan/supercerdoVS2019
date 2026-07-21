using System.Collections.Generic;

namespace Web.Models
{
    public class CatalogoGlobalProductosVm
    {
        public string Busqueda { get; set; }
        public string Tipo { get; set; }
        public int Pagina { get; set; }
        public bool HayMas { get; set; }
        public List<string> Tipos { get; set; }
        public List<ProductoGlobalImportItemVm> Productos { get; set; }

        public CatalogoGlobalProductosVm()
        {
            Pagina = 1;
            Tipos = new List<string>();
            Productos = new List<ProductoGlobalImportItemVm>();
        }
    }
}
