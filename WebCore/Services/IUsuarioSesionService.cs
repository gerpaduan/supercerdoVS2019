using Utilidades;

namespace WebCore.Services
{
    // Reemplazo del stub _usuarioActual/StubEmpresaContext que hoy declara cada controller por su
    // cuenta (ver docs/DECISIONS.md "Login/Sesion real"). Resuelve, UNA sola vez por request
    // (cacheado en la instancia scoped), el usuario logueado completo -- con Sucursal y Permisos
    // frescos desde la base, igual criterio que Web/Controllers/BaseController.cs clasico
    // re-resuelve Sucursal en cada request en vez de confiar en un valor cacheado en la cookie.
    public interface IUsuarioSesionService
    {
        // true si hay una cookie de autenticacion valida. Los controllers no deberian necesitar
        // chequear esto directo: [Authorize]/el FallbackPolicy global de Program.cs ya garantiza
        // que UsuarioActual no sea null cuando una accion se ejecuta.
        bool EstaAutenticado { get; }

        // Usuario logueado completo (Id/Admin/IdEmpresa/IdSucursal/Sucursal/Permisos ya cargados).
        // Lanza InvalidOperationException si se accede sin estar autenticado -- un controller con
        // [Authorize] nunca deberia llegar a ese caso.
        Entidades.Usuario UsuarioActual { get; }

        IEmpresaContext Empresa { get; }

        IParametrosContext Parametros { get; }
    }
}
