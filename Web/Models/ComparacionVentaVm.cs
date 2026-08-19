namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Venta (Etapa 7).
    public class ComparacionVentaVm
    {
        public Entidades.Venta SqlServer { get; set; }
        public Entidades.Venta Postgres { get; set; }
    }
}
