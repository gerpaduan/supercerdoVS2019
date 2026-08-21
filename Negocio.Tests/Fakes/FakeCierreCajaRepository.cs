using System;
using System.Collections.Generic;
using System.Data;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake minimo de ICierreCajaRepository -- solo addOrEditEgresoCaja tiene cuerpo real (lo
    // unico que egresoCajaPagoTarjeta llama para formas de pago distintas de Efectivo). El
    // resto tira NotImplementedException.
    public sealed class FakeCierreCajaRepository : Contratos.ICierreCajaRepository
    {
        public bool AddOrEditEgresoCajaFueLlamado { get; private set; }

        public EgresoCaja addOrEditEgresoCaja(EgresoCaja oEgresoCaja, Contratos.IUnitOfWork unitOfWork = null)
        {
            AddOrEditEgresoCajaFueLlamado = true;
            return oEgresoCaja;
        }

        public DataTable findCierreCaja(CierreCaja oCierreParam, CierreCaja.tipoBusqueda tipoBusquedaParam, string texto, DateTime? fechaDesde) => throw new NotImplementedException();
        public void addOrEditCierreCaja(CierreCaja oCierreCajaE) => throw new NotImplementedException();
        public DataTable findCierreCajaMultiples(List<CierreCaja> listaCierreCaja) => throw new NotImplementedException();
        public DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso) => throw new NotImplementedException();
        public void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto) => throw new NotImplementedException();
        public void eliminarTipoEgreso(int id) => throw new NotImplementedException();
        public DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta) => throw new NotImplementedException();
        public DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta) => throw new NotImplementedException();
        public DataTable obtenerGastosAgrupadosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal) => throw new NotImplementedException();
        public EgresoCaja getEgresoCajaById(int idEgresoCaja) => throw new NotImplementedException();
        public List<EgresoCaja> getEgresosCajaByIds(List<int> ids) => throw new NotImplementedException();
        public EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID) => throw new NotImplementedException();
        public float getMontoEgresosCajaVendedor(CierreCaja oCierre) => throw new NotImplementedException();
        public DataTable getEgresosCajaVendedor(CierreCaja oCierre) => throw new NotImplementedException();
        public Contratos.CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(CierreCaja cierreCaja, int idSucursalNueva) => throw new NotImplementedException();
        public Contratos.CambioSucursalCajaResultado cambiarSucursalCaja(CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor) => throw new NotImplementedException();
    }
}
