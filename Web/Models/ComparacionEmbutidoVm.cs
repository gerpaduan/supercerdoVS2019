namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Embutido (Etapa 11a).
    public class ComparacionEmbutidoVm
    {
        public Entidades.Embutido SqlServer { get; set; }
        public Entidades.Embutido Postgres { get; set; }
    }
}
