namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Expendio (Etapa 12b).
    public class ComparacionExpendioVm
    {
        public Entidades.Venta SqlServer { get; set; }
        public Entidades.Venta Postgres { get; set; }
    }
}
