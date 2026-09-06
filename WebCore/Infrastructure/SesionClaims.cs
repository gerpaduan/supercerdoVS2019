// Nombres de los claims custom que viaja la cookie de autenticacion (ver LoginController.cs y
// UsuarioSesionService.cs). Se usa ClaimTypes.NameIdentifier para el Id y ClaimTypes.Name para el
// nombre de usuario (User) -- estos 6 son los unicos custom, ver docs/DECISIONS.md "Login/Sesion
// real" para el razonamiento completo de que va en la cookie y que se re-resuelve por request.
namespace WebCore.Infrastructure
{
    public static class SesionClaims
    {
        public const string IdEmpresa = "IdEmpresa";
        public const string IdSucursal = "IdSucursal";
        public const string Admin = "Admin";
        public const string EsUsuarioProduccion = "EsUsuarioProduccion";
        public const string PermitirLoginFueraSucursal = "PermitirLoginFueraSucursal";
        public const string NombreCompleto = "NombreCompleto";
    }
}
