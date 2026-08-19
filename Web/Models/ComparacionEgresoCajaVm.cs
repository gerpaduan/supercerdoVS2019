namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para EgresoCaja (Etapa 8).
    public class ComparacionEgresoCajaVm
    {
        public Entidades.EgresoCaja SqlServer { get; set; }
        public Entidades.EgresoCaja Postgres { get; set; }
    }
}
