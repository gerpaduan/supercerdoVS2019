using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Utilidades;
using WebCore.Infrastructure;

namespace WebCore.Services
{
    // Implementacion scoped (una instancia por request, ver Program.cs) de IUsuarioSesionService.
    // Todo se resuelve lazy y se cachea en la instancia -- varios controllers/vistas pueden pedir
    // UsuarioActual en el mismo request sin repetir las consultas a la base.
    public sealed class UsuarioSesionService : IUsuarioSesionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmpresaContext _empresa;
        private IParametrosContext _parametros;
        private Entidades.Usuario _usuarioActual;
        private bool _usuarioResuelto;

        public UsuarioSesionService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _empresa = new EmpresaContextClaims(httpContextAccessor);
        }

        public bool EstaAutenticado =>
            _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

        public IEmpresaContext Empresa => _empresa;

        public IParametrosContext Parametros
        {
            get
            {
                if (_parametros == null)
                {
                    _parametros = WebCore.Infrastructure.NegocioFactory.CrearParametros(_empresa);
                    _parametros.Reload();
                }

                return _parametros;
            }
        }

        public Entidades.Usuario UsuarioActual
        {
            get
            {
                if (!_usuarioResuelto)
                {
                    _usuarioActual = ResolverUsuarioActual();
                    _usuarioResuelto = true;
                }

                if (_usuarioActual == null)
                    throw new InvalidOperationException("No hay un usuario autenticado en este request -- verificar que la accion tenga [Authorize] o que el FallbackPolicy global este activo.");

                return _usuarioActual;
            }
        }

        private Entidades.Usuario ResolverUsuarioActual()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            if (principal?.Identity?.IsAuthenticated != true)
                return null;

            var idTexto = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idTexto, out var id) || id <= 0)
                return null;

            var oUsuarioN = WebCore.Infrastructure.NegocioFactory.CrearUsuario(_empresa, Parametros);
            var usuario = oUsuarioN.getUsuarioById(id, sinRestriccionDeTenant: true);
            if (usuario == null || usuario.Id <= 0)
                return null;

            // Mismo criterio que Web/Controllers/BaseController.cs clasico: Sucursal y Permisos se
            // re-resuelven frescos en cada request, nunca se confia en un valor cacheado en la
            // cookie (los claims solo traen IdSucursal, no el objeto Sucursal completo).
            var oSucursalN = WebCore.Infrastructure.NegocioFactory.CrearSucursal(_empresa, Parametros);
            usuario.Sucursal = usuario.IdSucursal > 0 ? oSucursalN.findById(usuario.IdSucursal) : null;
            usuario.SucursalNombre = usuario.Sucursal?.SucursalNombre ?? "";
            usuario.Permisos = oUsuarioN.getPermisosUsuario(usuario.Id) ?? new List<Entidades.PermisosUsuarios>();

            return usuario;
        }
    }
}
