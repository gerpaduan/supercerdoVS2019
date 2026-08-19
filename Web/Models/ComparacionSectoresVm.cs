namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Sectores (Etapa 12a).
    public class ComparacionSectoresVm
    {
        public System.Data.DataTable SqlServer { get; set; }
        public System.Data.DataTable Postgres { get; set; }
    }
}
