namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Corte (Etapa 6).
    public class ComparacionCorteVm
    {
        public Entidades.Corte SqlServer { get; set; }
        public Entidades.Corte Postgres { get; set; }
    }
}
