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

        // Auditoria de cierres y reapertura de la ultima caja (2026-10-06, ver docs/DECISIONS.md). Solo Postgres:
        // SoportaAuditoriaCierre es false en SQL Server y entonces ni se avisa ni se registra nada.
        bool SoportaAuditoriaCierre { get; }
        // Cierre YA CERRADO (usuariocierre <> '0') del dueno/sucursal cuyo rango contiene la fecha, o tabla vacia.
        // La relacion registro->caja es por rango (no hay FK): dueno + sucursal + fecha entre apertura y cierre.
        DataTable findCierreCerradoQueContiene(int idDueno, int idSucursal, DateTime fecha);
        void registrarAuditoriaCierre(Entidades.AuditoriaCierreCaja auditoria);
        // Egreso de caja (tipo compra) de una compra por egresoscaja.idcompra, o egreso vacio (Id 0) si no tiene: solo las
        // compras hechas desde el POS generan egreso y por lo tanto mueven una caja.
        Entidades.EgresoCaja findEgresoCajaPorCompra(int idCompra);
        // Registros de auditoria de un cierre (mas reciente primero), con el nombre del usuario.
        DataTable obtenerAuditoriaCierre(int idCierre);
        PreviewReapertura obtenerPreviewReapertura(int idCierre);
        ResultadoReapertura reabrirCierreCaja(int idCierre, int idUsuarioEjecutor, string usuarioEjecutor, string motivo);

        DataTable obtenerTiposEgresoCaja(string buscarText, int idTipoEgreso);
        void addOrEditTipoEgreso(int id, string tipoEgresoCaja, bool esGasto);
        void eliminarTipoEgreso(int id);

        DataTable obtenerEgresosCaja(int idSucursal, int idUsuario, int idTipoEgresoCaja, string texto, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerEgresosCajaGastosBalance(int idSucursal, DateTime fechaDesde, DateTime fechaHasta);
        DataTable obtenerGastosAgrupadosBalance(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal);
        // unitOfWork opcional: ver Contratos/IUnitOfWork.cs.
        Entidades.EgresoCaja addOrEditEgresoCaja(Entidades.EgresoCaja oEgresoCaja, Contratos.IUnitOfWork unitOfWork = null);
        Entidades.EgresoCaja getEgresoCajaById(int idEgresoCaja);
        List<Entidades.EgresoCaja> getEgresosCajaByIds(List<int> ids);
        Entidades.EgresoCaja findEgresoCajaByTablaYId(string tabla, int tablaID);
        float getMontoEgresosCajaVendedor(Entidades.CierreCaja oCierre);
        DataTable getEgresosCajaVendedor(Entidades.CierreCaja oCierre);

        CambioSucursalCajaPreview obtenerPreviewCambioSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva);
        CambioSucursalCajaResultado cambiarSucursalCaja(Entidades.CierreCaja cierreCaja, int idSucursalNueva, int idUsuarioEjecutor, string usuarioEjecutor);
    }
}
