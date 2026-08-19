using System;
using System.Collections.Generic;
using Utilidades;

namespace Negocio
{
    // Wrapper delgado sobre Datos.CortePuntoStockSucursal, mismo estilo que Negocio/Sucursal.cs.
    public class CortePuntoStockSucursal
    {
        private readonly Contratos.ICortePuntoStockSucursalRepository oCortePuntoStockSucursalD;

        public CortePuntoStockSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            oCortePuntoStockSucursalD = new Datos.CortePuntoStockSucursal(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // ICortePuntoStockSucursalRepository (ej. DatosPostgres.CortePuntoStockSucursalPg).
        public CortePuntoStockSucursal(Contratos.ICortePuntoStockSucursalRepository repositorio)
        {
            oCortePuntoStockSucursalD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public void CrearParaTodasLasSucursales(int idEmpresa, int idCorte, int puntoStockInicial)
        {
            oCortePuntoStockSucursalD.CrearParaTodasLasSucursales(idEmpresa, idCorte, puntoStockInicial);
        }

        public void GuardarPuntosStockLote(int idEmpresa, int idCorte, List<(int idSucursal, int puntoStock)> valores)
        {
            oCortePuntoStockSucursalD.GuardarPuntosStockLote(idEmpresa, idCorte, valores);
        }

        public Dictionary<int, int> FindPorSucursal(int idSucursal)
        {
            return oCortePuntoStockSucursalD.FindPorSucursal(idSucursal);
        }
    }
}
