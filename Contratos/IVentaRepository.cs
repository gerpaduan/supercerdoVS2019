using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja el bloque Ventas/LineaVenta/TemporalLineaVenta de Datos.Venta (Etapa 7) mas
    // Sectores/Licencias (Etapa 12a). Expendios/LineaExpendio y FacturaElectronica quedan
    // fuera de esta interfaz -- se agregan en etapas futuras. Ver docs/DECISIONS.md.
    public interface IVentaRepository
    {
        Entidades.Venta getVentaById(int idVenta);
        List<Entidades.Venta> getAllVentas(DateTime fechaDesde, DateTime fechaHasta, string texto, int? idVendedor, int? idCliente, int? idSucursal, bool soloAnulados, bool cargarLineas);
        List<Entidades.Venta> getVentasBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal);
        decimal getTotalKgsPesablesBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, bool incluirVentasCuentaCorriente);
        int agregarVenta(Entidades.Venta oVentaE);

        // El efecto colateral real de modificarVenta (reverso en EgresosCaja cuando la venta
        // cta-cte tenia un egreso previo y se borran sus lineas) NO esta implementado -- depende
        // de EgresosCaja/TiposEgresoCaja, dominio de CierreCaja.cs, todavia sin migrar. Ver
        // docs/GAPS.md. El resto del metodo (borrar lineas + update Ventas) SI esta completo.
        void modificarVenta(Entidades.Venta oVentaE, int sucAnterior, bool eliminarLineas);

        DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados);
        DataTable getVentasVendedorCierreCaja(Entidades.CierreCaja oCierreE, bool soloAnulados);
        float getTotalVenta(int idVenta);
        float getTotalKgsVenta(int idVenta);
        float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta);
        Entidades.LineaVenta agregarLineaVenta(Entidades.LineaVenta oLineaE);
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
    }
}
