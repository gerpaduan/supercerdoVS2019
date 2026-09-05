using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebCore.Models;

namespace WebCore.Helpers
{
    // Extraida el 2026-09-05 (ver docs/DECISIONS.md, "Postgres es la base oficial y unica") para
    // que WebCore.Infrastructure.NegocioFactory pueda devolver SystemAdministrationRepository
    // (SQL Server) o SystemAdministrationRepositoryPg segun el DataEngine configurado -- mismo
    // criterio que Web/Helpers/ISystemAdministrationRepository.cs (clasico), que ya tenia esta
    // interfaz desde el principio.
    public interface ISystemAdministrationRepository
    {
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
        List<SystemAdministrationCondicionIvaVm> ObtenerCondicionesIva();
    }
}
