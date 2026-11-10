namespace WebCore.Models
{
    // Punto de stock de un producto en una sucursal, para mostrarlo en solo lectura en Productos/AddOrEdit.
    public class PuntoStockSucursalProductoVm
    {
        public string SucursalNombre { get; set; } = "";
        public int PuntoStock { get; set; }
    }
}
