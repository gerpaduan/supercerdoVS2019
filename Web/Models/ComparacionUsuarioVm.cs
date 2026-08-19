namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para Usuario (Etapa 13a).
    public class ComparacionUsuarioVm
    {
        public Entidades.Usuario SqlServer { get; set; }
        public Entidades.Usuario Postgres { get; set; }
    }
}
