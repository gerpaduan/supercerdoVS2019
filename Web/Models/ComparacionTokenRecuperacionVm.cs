namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para UsuarioPasswordResetToken (Etapa 13c).
    public class ComparacionTokenRecuperacionVm
    {
        public Entidades.UsuarioPasswordResetToken SqlServer { get; set; }
        public Entidades.UsuarioPasswordResetToken Postgres { get; set; }
    }
}
