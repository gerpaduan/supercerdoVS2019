namespace WebCore.Models
{
    // Fila de la seccion "Jerarquia por sucursal" de Productos/AddOrEdit.cshtml -- una por
    // cada sucursal de la empresa. TieneExcepcion=false significa que esta sucursal usa el
    // valor global del corte (Independiente/CorteMaestroNombre en el VM principal).
    public class CorteJerarquiaSucursalItemVm
    {
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; } = "";
        public bool TieneExcepcion { get; set; }
        public bool IndependienteEfectivo { get; set; }
        public bool EnCierreStockEfectivo { get; set; }
    }
}
