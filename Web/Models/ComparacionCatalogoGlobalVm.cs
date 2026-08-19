using System.Collections.Generic;

namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para CatalogoGlobalProducto.
    public class ComparacionCatalogoGlobalVm
    {
        public List<Entidades.CatalogoGlobalProducto> SqlServer { get; set; }
        public List<Entidades.CatalogoGlobalProducto> Postgres { get; set; }
    }
}
