using System;

namespace Negocio
{
    // Toggle "Conectado con Point Mercado Pago" por Sucursal (default de admin + ultima
    // eleccion del cajero, ver Entidades.MercadoPagoSucursalConfig y docs/DECISIONS.md). Sin
    // precedente en SQL Server -- nace directo sobre Postgres.
    public class MercadoPagoSucursalConfig
    {
        private readonly Contratos.IMercadoPagoSucursalConfigRepository oConfigD;

        public MercadoPagoSucursalConfig(Contratos.IMercadoPagoSucursalConfigRepository repositorio)
        {
            oConfigD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public Entidades.MercadoPagoSucursalConfig ObtenerPorSucursal(int idSucursal, int idEmpresa)
        {
            return oConfigD.ObtenerPorSucursal(idSucursal, idEmpresa);
        }

        public void GuardarDefault(int idSucursal, int idEmpresa, bool conectadoDefault)
        {
            oConfigD.GuardarDefault(idSucursal, idEmpresa, conectadoDefault);
        }

        public void GuardarUltimaEleccion(int idSucursal, int idEmpresa, bool conectado)
        {
            oConfigD.GuardarUltimaEleccion(idSucursal, idEmpresa, conectado);
        }

        public void GuardarStoreId(int idSucursal, int idEmpresa, string mpStoreId)
        {
            oConfigD.GuardarStoreId(idSucursal, idEmpresa, mpStoreId);
        }
    }
}
