namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Parametros/EmpresaParametros.
    public class ComparacionParametrosVm
    {
        public System.Data.DataTable SqlServer { get; set; }
        public System.Data.DataTable Postgres { get; set; }
    }
}
