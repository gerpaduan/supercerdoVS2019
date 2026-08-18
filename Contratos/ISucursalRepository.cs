using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja exactamente los metodos publicos de Datos.Sucursal (SQL Server) tal como
    // estan hoy, mismo criterio que IPersonaRepository (Etapa 2).
    public interface ISucursalRepository
    {
        DataTable obtenerSucursales();
        Entidades.Sucursal findById(int id);
        List<Entidades.Sucursal> findAll();
        Entidades.Empresa findEmpresaById(int idEmpresa);
        Entidades.Empresa findEmpresaByCuit(long cuit);
        void ActualizarDatosBasicos(Entidades.Sucursal oSucursalE);
        DataTable obtenerSucursalSanMartin();
        DataTable obtenerSucursalSanLorenzo();
        DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual);
        int getIdSucursalByConexion(string nameConnString);
    }
}
