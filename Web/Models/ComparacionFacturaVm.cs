namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para FacturaElectronica (Etapa 12c).
    public class ComparacionFacturaVm
    {
        public Entidades.FacturaElectronica SqlServer { get; set; }
        public Entidades.FacturaElectronica Postgres { get; set; }
    }
}
