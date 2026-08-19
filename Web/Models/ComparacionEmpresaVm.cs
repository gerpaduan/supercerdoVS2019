namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Empresa (pantalla "Mi Empresa").
    public class ComparacionEmpresaVm
    {
        public Entidades.Empresa SqlServer { get; set; }
        public Entidades.Empresa Postgres { get; set; }
    }
}
