using System.Collections.Generic;
using System.Web.Mvc;
using Web.Models;

namespace Web.Helpers
{
    // Contrato del modulo de administracion de plataforma (alta/edicion de Empresas, Sucursales,
    // Usuarios cruzando todos los tenants, gate de superadmin, alicuotas IVA). Vive en Web/ y no
    // en Contratos/ (a diferencia del resto de las interfaces de repositorio del repo) porque su
    // firma usa Web.Models.* y System.Web.Mvc.SelectListItem -- tipos que no existen en el
    // netstandard2.0 puro de Contratos/DatosPostgres, y que esos proyectos no pueden referenciar
    // sin crear una dependencia circular con Web (ver docs/DECISIONS.md 2026-08-25). Derivada 1:1
    // de los metodos publicos de SystemAdministrationRepository.cs (implementacion SQL Server).
    public interface ISystemAdministrationRepository
    {
        bool EsSuperAdmin(int idUsuario);

        bool TablaSucursalTieneTelefono();
        bool TablaSucursalTieneActiva();

        List<SystemAdministrationEmpresaResumenVm> ObtenerEmpresas();
        SystemAdministrationEmpresaEditVm ObtenerEmpresa(int idEmpresa);
        int CrearEmpresa(SystemAdministrationEmpresaEditVm model);
        void ActualizarEmpresa(SystemAdministrationEmpresaEditVm model);

        List<SystemAdministrationSucursalResumenVm> ObtenerSucursales(int idEmpresa = 0);
        SystemAdministrationSucursalEditVm ObtenerSucursal(int idSucursal);
        int CrearSucursal(SystemAdministrationSucursalEditVm model);
        void ActualizarSucursal(SystemAdministrationSucursalEditVm model);

        List<SystemAdministrationUsuarioResumenVm> ObtenerUsuarios(int idEmpresa = 0);
        SystemAdministrationUsuarioEditVm ObtenerUsuario(int idUsuario);
        int CrearUsuario(SystemAdministrationUsuarioEditVm model);
        void ActualizarUsuario(SystemAdministrationUsuarioEditVm model);

        int CrearAltaRapida(SystemAdministrationAltaRapidaVm model);

        bool ExisteCuit(long cuit, int idEmpresaExcluir);
        bool ExisteUsuario(string usuario, int idUsuarioExcluir);
        bool ExisteEmail(string email, int idUsuarioExcluir);

        List<SelectListItem> ObtenerEmpresasSelectList(int idSeleccionado = 0, bool incluirTodas = false);
        List<SelectListItem> ObtenerSucursalesSelectList(int idEmpresa, int idSeleccionado = 0);

        List<SystemAdministrationAlicuotaIvaVm> ObtenerAlicuotasIva();

        // Catalogo global (idEmpresa=0) de "Condicion frente al IVA", el mismo que puebla el
        // combo de /Personas -- se usa para precargar sugerencias en AltaRapidaEmpresa.cshtml.
        List<SystemAdministrationCondicionIvaVm> ObtenerCondicionesIva();
    }
}
