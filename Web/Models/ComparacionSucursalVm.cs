namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Sucursal (Etapa 3).
    public class ComparacionSucursalVm
    {
        public Entidades.Sucursal SqlServer { get; set; }
        public Entidades.Sucursal Postgres { get; set; }
    }
}
