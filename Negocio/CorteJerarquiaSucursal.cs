using System;
using System.Collections.Generic;
using Utilidades;

namespace Negocio
{
    // Wrapper delgado sobre Datos.CorteJerarquiaSucursal, mismo estilo que
    // Negocio/CortePuntoStockSucursal.cs (standalone, no dentro de Negocio/Corte.cs).
    public class CorteJerarquiaSucursal
    {
        private readonly Contratos.ICorteJerarquiaSucursalRepository oCorteJerarquiaSucursalD;

        public CorteJerarquiaSucursal(IEmpresaContext empresa, IParametrosContext param = null)
        {
            oCorteJerarquiaSucursalD = new Datos.CorteJerarquiaSucursal(empresa, param);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de
        // ICorteJerarquiaSucursalRepository (ej. DatosPostgres.CorteJerarquiaSucursalPg).
        public CorteJerarquiaSucursal(Contratos.ICorteJerarquiaSucursalRepository repositorio)
        {
            oCorteJerarquiaSucursalD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public void GuardarExcepcion(int idEmpresa, int idCorte, int idSucursal, bool independiente, bool enCierreStock)
        {
            oCorteJerarquiaSucursalD.GuardarExcepcion(idEmpresa, idCorte, idSucursal, independiente, enCierreStock);
        }

        public void QuitarExcepcion(int idEmpresa, int idCorte, int idSucursal)
        {
            oCorteJerarquiaSucursalD.QuitarExcepcion(idEmpresa, idCorte, idSucursal);
        }

        public Dictionary<int, (bool independiente, bool enCierreStock)> ListarExcepcionesPorCorte(int idEmpresa, int idCorte)
        {
            return oCorteJerarquiaSucursalD.ListarExcepcionesPorCorte(idEmpresa, idCorte);
        }
    }
}
