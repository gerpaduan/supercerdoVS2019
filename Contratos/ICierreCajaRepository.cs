using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja Datos.CierreCaja completo: CierreCaja/EgresosCaja/TiposEgresoCaja (Etapa 8) mas
    // cambiarSucursalCaja/obtenerPreviewCambioSucursalCaja (Etapa 10, operacion cross-cutting
    // que toca Ventas/Compras/CortePorCompra/MediaRes/Pagos/MovCtaCte/Expendios/
    // TemporalLineaVenta ademas de CierreCaja). Ver docs/DECISIONS.md.
    public interface ICierreCajaRepository
    {
        DataTable findCierreCaja(Entidades.CierreCaja oCierreParam, Entidades.CierreCaja.tipoBusqueda tipoBusquedaParam, string texto, DateTime? fechaDesde);
        void addOrEditCierreCaja(Entidades.CierreCaja oCierreCajaE);
        DataTable findCierreCajaMultiples(List<Entidades.CierreCaja> listaCierreCaja);

        DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso);
        void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto);
        void eliminarTipoEgreso(int id);

        DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerGastosAgrupadosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal);
        Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja);
        Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja);
        List<Entidades.EgresoCaja> getEgresosCajaByIds(List<int> ids);
        Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID);
        float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierre);
        DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierre);

        CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva);
        CambioSucursalCajaResultado cambiarSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor);
    }
}
