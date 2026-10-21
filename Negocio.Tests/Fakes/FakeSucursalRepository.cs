using System.Collections.Generic;
using System.Data;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake minimo de ISucursalRepository: findById devuelve una sucursal con ese id (lo unico que necesita
    // Negocio.CierreCaja.convertDatatableToList para armar la caja). El resto devuelve vacio.
    public sealed class FakeSucursalRepository : Contratos.ISucursalRepository
    {
        public DataTable obtenerSucursales() => new DataTable();
        public Sucursal findById(int id) => new Sucursal { IdSucursal = id, SucursalNombre = "Sucursal " + id };
        public List<Sucursal> findAll() => new List<Sucursal>();
        public Empresa findEmpresaById(int idEmpresa) => null;
        public Empresa findEmpresaByCuit(long cuit) => null;
        public void ActualizarDatosBasicos(Sucursal oSucursalE) { }
        public DataTable obtenerSucursalSanMartin() => new DataTable();
        public DataTable obtenerSucursalSanLorenzo() => new DataTable();
        public DataTable obtenerConexiones(bool? mostrarEnPrincipal, bool? mostrarEnStockActual) => new DataTable();
        public int getIdSucursalByConexion(string nameConnString) => 0;
    }
}
