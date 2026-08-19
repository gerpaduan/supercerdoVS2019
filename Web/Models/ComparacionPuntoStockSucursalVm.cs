using System.Collections.Generic;

namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para CortePuntoStockSucursal.
    public class ComparacionPuntoStockSucursalVm
    {
        public Dictionary<int, int> SqlServer { get; set; }
        public Dictionary<int, int> Postgres { get; set; }
    }
}
