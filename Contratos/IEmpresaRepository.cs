namespace Contratos
{
    // Espeja Datos.Empresa completo (2/2 metodos, pantalla "Mi Empresa"). No confundir con
    // Web/Helpers/SystemAdministrationRepository (CRUD cross-tenant del super-admin de
    // plataforma, fuera de esta migracion) ni con Contratos.ISucursalRepository.findEmpresaById
    // (lectura de solo lectura ya migrada, distinta tabla de acceso).
    public interface IEmpresaRepository
    {
        Entidades.Empresa findById(int idEmpresa);
        void ActualizarDatosBasicos(Entidades.Empresa oEmpresaE);
    }
}
