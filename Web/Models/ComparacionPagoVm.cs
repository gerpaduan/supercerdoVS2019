namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Pago (Etapa 5).
    public class ComparacionPagoVm
    {
        public Entidades.Pago SqlServer { get; set; }
        public Entidades.Pago Postgres { get; set; }
    }
}
