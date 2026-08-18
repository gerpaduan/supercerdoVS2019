namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres del piloto de migracion
    // (Etapa 2). Solo para la herramienta de verificacion en MigracionPostgresController.
    public class ComparacionPersonaVm
    {
        public Entidades.Persona SqlServer { get; set; }
        public Entidades.Persona Postgres { get; set; }
    }
}
