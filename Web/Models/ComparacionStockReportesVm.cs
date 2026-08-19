namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para CierreStockWeb (Etapa 11c).
    public class ComparacionStockReportesVm
    {
        public System.Data.DataTable SqlServer { get; set; }
        public System.Data.DataTable Postgres { get; set; }
    }
}
