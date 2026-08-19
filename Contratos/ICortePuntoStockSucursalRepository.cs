using System.Collections.Generic;

namespace Contratos
{
    // Espeja Datos.CortePuntoStockSucursal completo (3/3 metodos).
    public interface ICortePuntoStockSucursalRepository
    {
        void CrearParaTodasLasSucursales(int idEmpresa, int idCorte, int puntoStockInicial);
        void GuardarPuntosStockLote(int idEmpresa, int idCorte, List<(int idSucursal, int puntoStock)> valores);
        Dictionary<int, int> FindPorSucursal(int idSucursal);
    }
}
