namespace Contratos
{
    public interface IMercadoPagoSucursalConfigRepository
    {
        // Nunca null: si no hay fila todavia, devuelve una config "apagada" por default
        // (ConectadoPointDefault = false, ConectadoPointUltimaEleccion = false) sin tocar la DB.
        Entidades.MercadoPagoSucursalConfig ObtenerPorSucursal(int idSucursal, int idEmpresa);

        void GuardarDefault(int idSucursal, int idEmpresa, bool conectadoDefault);
        void GuardarUltimaEleccion(int idSucursal, int idEmpresa, bool conectado);

        // Fase 3, agregado 2026-09-01: guarda el id de "Store" de Mercado Pago para esa
        // sucursal (una vez creada via API).
        void GuardarStoreId(int idSucursal, int idEmpresa, string mpStoreId);
    }
}
