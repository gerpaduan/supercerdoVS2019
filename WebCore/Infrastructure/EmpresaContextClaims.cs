using Microsoft.AspNetCore.Http;
using Utilidades;

namespace WebCore.Infrastructure
{
    // Reemplazo de Web/EmpresaContextWeb.cs (lee Session["IdEmpresa"] via HttpContext.Current) --
    // aca el tenant activo sale del claim IdEmpresa de la cookie de autenticacion, resuelto por
    // request via IHttpContextAccessor. Registrado scoped en Program.cs; usado solo desde
    // UsuarioSesionService, ningun controller lo instancia directo.
    public sealed class EmpresaContextClaims : IEmpresaContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmpresaContextClaims(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int IdEmpresa
        {
            get
            {
                var value = _httpContextAccessor.HttpContext?.User.FindFirst(SesionClaims.IdEmpresa)?.Value;
                return int.TryParse(value, out var idEmpresa) ? idEmpresa : 0;
            }
        }
    }
}
