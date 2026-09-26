using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja la totalidad de Datos.Venta (47 metodos): Ventas/LineaVenta/TemporalLineaVenta
    // (Etapa 7), Sectores/Licencias (Etapa 12a), Expendios/LineaExpendio (Etapa 12b) y
    // FacturaElectronica (Etapa 12c). Ver docs/DECISIONS.md.
    // obtenerLineasExpendio y getAlicuotaIvaFactura NO estan en esta interfaz a proposito: sin
    // caller externo, solo se usan internamente dentro de getExpedioById/getFactuElecById
    // (mismo patron que Datos.Venta).
    public interface IVentaRepository
    {
        // Ver Contratos/IUnitOfWork.cs. Implementacion SQL Server: devuelve null (agregarVenta
        // sigue usando TransactionScope como siempre). Implementacion Postgres: abre una
        // DatosPostgres.UnitOfWorkPg real, que Negocio.Venta.agregarVenta comparte con
        // CierreCajaPg/CuentaCorrientePg en vez de usar TransactionScope (incompatible con el
        // patron de conexion-por-metodo de Npgsql, ver docs/DECISIONS.md 2026-08-20).
        Contratos.IUnitOfWork IniciarUnitOfWork();

        Entidades.Venta getVentaById(int idVenta);
        List<Entidades.Venta> getAllVentas(DateTime fechaDesde, DateTime fechaHasta, string texto, int? idVendedor, int? idCliente, int? idSucursal, bool soloAnulados, bool cargarLineas);
        List<Entidades.Venta> getVentasBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal);
        decimal getTotalKgsPesablesBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, bool incluirVentasCuentaCorriente);
        // unitOfWork opcional: ver Contratos/IUnitOfWork.cs. Si se pasa, la implementacion
        // Postgres reusa esa conexion/transaccion compartida en vez de abrir la suya propia
        // (necesario para que agregarVenta/asignarVentaEnExpendio/agregarLineaVenta,
        // CierreCajaPg.addOrEditEgresoCaja y CuentaCorrientePg.*MovCtaCte* sean atomicos entre
        // si). Ignorado por la implementacion SQL Server (TransactionScope sigue como siempre).
        int agregarVenta(Entidades.Venta oVentaE, Contratos.IUnitOfWork unitOfWork = null);

        // El reverso en EgresosCaja (venta con egreso previo cuyo monto cambia) esta
        // implementado desde la Etapa 8 (EgresosCaja/TiposEgresoCaja ya migrados). unitOfWork
        // opcional: ver Contratos/IUnitOfWork.cs.
        void modificarVenta(Entidades.Venta oVentaE, int sucAnterior, bool eliminarLineas, Contratos.IUnitOfWork unitOfWork = null);

        DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados);
        DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados);
        float getTotalVenta(int idVenta);
        float getTotalKgsVenta(int idVenta);
        float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta);
        Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE, Contratos.IUnitOfWork unitOfWork = null);
        void actualizarAlicuotaLineaVenta(int idLineaVenta, int idAlicuotaIva, float alicuotaIva);
        void eliminarLineasVenta(int idVenta);
        Entidades.Venta getUltimaVentaVendedor(Entidades.CierreCaja oCierreE);
        List<Entidades.LineaVenta> obtenerLineasVenta(int idVenta);
        DataTable obtenerUltimosPreciosPorCliente(int idPersona, int topVentas = 10);

        // No-op deliberado: el SP real solo actualiza StockCorteSucursal (cascada de stock),
        // tabla que nunca se porta a Postgres (confirmado obsoleta, Etapa 6). No es un gap.
        void agregarStockVenta(Entidades.Venta oVentaE);

        void agregarTemporalLineaVenta(Entidades.TemporalLineaVenta oTemporalLV);
        DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas);
        DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto);
        DataTable ultimasVentasCliente(int idSucursal, int idPersona);
        void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte);
        void actualizarCliente(int idVenta, int idPersona);

        DataTable obtenerSectores();
        bool existeSector(string sector, string sectorActual = "");
        void agregarSector(string sector);
        void modificarSector(string sectorActual, string sectorNuevo);
        bool sectorEstaEnUso(string sector);
        void eliminarSector(string sector);
        string getUltimoSectorSelect(string serialCPU);

        int agregarExpendio(Entidades.Venta oVentaE);
        Entidades.LineaVenta agregarLineaExprendio(Entidades.LineaVenta oLineaE);
        void asignarVentaEnExpendio(int idVenta, int idExpendio, Contratos.IUnitOfWork unitOfWork = null);
        DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal);
        DataTable obtenerExpendiosAvanzado(DateTime fechaDesde, DateTime? fechaHasta, int idSucursal);
        DataTable obtenerExpendiosPorUsuario(int idSucursal, int idVendedor, int top = 100, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        DataTable obtenerExpendiosEmpresa(int top = 300, DateTime? fechaDesde = null, DateTime? fechaHasta = null);
        Entidades.Venta getExpedioById(int idExpendio);

        // Remitos (2026-09-26, ver Negocio.NroRemito). Solo Postgres persiste el numero de remito del
        // expendio (columna expendios.nroremito); la implementacion SQL Server devuelve 0 / false.
        // ultimoCorrelativoRemito: mayor correlativo usado en la sucursal con el prefijo dado
        // (0 si no hay). existeNroRemito: ya hay un expendio de esa sucursal con ese numero.
        long ultimoCorrelativoRemito(int idSucursal, string prefijo);
        bool existeNroRemito(int idSucursal, string nroRemito);

        // ignorarPrueba: no cuenta las facturas emitidas en homologacion (esprueba). Solo Postgres; SQL Server lo ignora.
        int esVentaSinFacturar(int idVenta, bool esNotaCredito, bool ignorarPrueba = false);
        int existeFacturaElect(int idVenta, bool ignorarPrueba = false);
        int existeNotaCreditoElect(int idVenta, bool ignorarPrueba = false);
        void addOrEditFactuElec(Entidades.FacturaElectronica oFacturaElectronicaE);
        Entidades.FacturaElectronica getFactuElecById(int idFactuElec);
        List<Entidades.FacturaElectronica> BuscarFacturasPagina(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante,
            int pagina, int cantidad, int cantidadExtra, string entorno = null);
        (int Cantidad, decimal Total) ObtenerFacturasResumen(
            DateTime fechaDesde, DateTime fechaHasta, int idSucursal,
            string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante,
            string entorno = null);
        // true si la empresa tiene al menos una factura emitida en homologacion (esprueba). Sirve para
        // mostrar el filtro Produccion/Pruebas solo cuando hace falta. SQL Server: siempre false.
        bool ExistenFacturasPrueba();
    }
}
