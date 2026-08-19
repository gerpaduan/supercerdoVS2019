namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Compra (Etapa 9).
    public class ComparacionCompraVm
    {
        public Entidades.Compra SqlServer { get; set; }
        public Entidades.Compra Postgres { get; set; }
    }
}
