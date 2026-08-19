namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Movimiento (Etapa 11b).
    public class ComparacionMovimientoVm
    {
        public Entidades.Movimiento SqlServer { get; set; }
        public Entidades.Movimiento Postgres { get; set; }
    }
}
