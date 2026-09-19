using System.Collections.Generic;

namespace Contratos
{
    // Espeja Datos.CorteJerarquiaSucursal completo (3/3 metodos). Ver docs/DECISIONS.md
    // (2026-09-17) para el porque de esta tabla: independiente/enCierreStock de Corte son
    // columnas por (idEmpresa, idCorte), sin sucursal -- esto permite excepcionar ambos
    // valores para una sucursal puntual, sin tocar el idCorteMaestro global.
    public interface ICorteJerarquiaSucursalRepository
    {
        void GuardarExcepcion(int idEmpresa, int idCorte, int idSucursal, bool independiente, bool enCierreStock);
        void QuitarExcepcion(int idEmpresa, int idCorte, int idSucursal);
        Dictionary<int, (bool independiente, bool enCierreStock)> ListarExcepcionesPorCorte(int idEmpresa, int idCorte);
    }
}
