namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para LoginUbicacionLog (Etapa 13d).
    public class ComparacionLoginUbicacionLogVm
    {
        public System.Data.DataTable SqlServer { get; set; }
        public System.Data.DataTable Postgres { get; set; }
    }
}
