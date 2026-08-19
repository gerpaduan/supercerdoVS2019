using System.Collections.Generic;

namespace Web.Models
{
    // Modelo de la vista de comparacion SQL Server vs Postgres para PermisosUsuarios (Etapa 13b).
    public class ComparacionPermisosUsuarioVm
    {
        public List<Entidades.PermisosUsuarios> SqlServer { get; set; }
        public List<Entidades.PermisosUsuarios> Postgres { get; set; }
    }
}
