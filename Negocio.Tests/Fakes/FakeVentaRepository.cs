using System;
using System.Collections.Generic;
using System.Data;
using Entidades;

namespace NegocioTests.Fakes
{
    // Fake minimo de IVentaRepository -- solo implementa los metodos que un escenario
    // "Efectivo, sin lineas, sin expendios" de agregarVenta/modificarVenta realmente llama
    // (IniciarUnitOfWork, agregarVenta, modificarVenta). El resto queda sin implementar
    // (NotImplementedException): si un test futuro necesita mas cobertura, se extiende ahi,
    // no antes.
    public sealed class FakeVentaRepository : Contratos.IVentaRepository
    {
        private readonly Contratos.IUnitOfWork _unitOfWorkAEntregar;
        private readonly Exception _excepcionAlAgregar;
        private readonly Exception _excepcionAlModificar;
        public int IdVentaAAsignar { get; set; } = 1;
        public bool AgregarVentaFueLlamado { get; private set; }
        public bool ModificarVentaFueLlamado { get; private set; }

        public FakeVentaRepository(Contratos.IUnitOfWork unitOfWorkAEntregar = null, Exception excepcionAlAgregar = null, Exception excepcionAlModificar = null)
        {
            _unitOfWorkAEntregar = unitOfWorkAEntregar;
            _excepcionAlAgregar = excepcionAlAgregar;
            _excepcionAlModificar = excepcionAlModificar;
        }

        public Contratos.IUnitOfWork IniciarUnitOfWork() => _unitOfWorkAEntregar;

        public int agregarVenta(Venta oVentaE, Contratos.IUnitOfWork unitOfWork = null)
        {
            AgregarVentaFueLlamado = true;
            if (_excepcionAlAgregar != null) throw _excepcionAlAgregar;
            return IdVentaAAsignar;
        }

        public void modificarVenta(Venta oVentaE, int sucAnterior, bool eliminarLineas, Contratos.IUnitOfWork unitOfWork = null)
        {
            ModificarVentaFueLlamado = true;
            if (_excepcionAlModificar != null) throw _excepcionAlModificar;
        }

        public Venta getVentaById(int idVenta) => throw new NotImplementedException();
        public List<Venta> getAllVentas(DateTime fechaDesde, DateTime fechaHasta, string texto, int? idVendedor, int? idCliente, int? idSucursal, bool soloAnulados, bool cargarLineas) => throw new NotImplementedException();
        public List<Venta> getVentasBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal) => throw new NotImplementedException();
        public decimal getTotalKgsPesablesBalancePeriodo(DateTime fechaDesde, DateTime fechaHasta, int? idSucursal, bool incluirVentasCuentaCorriente) => throw new NotImplementedException();
        public DataTable obtenerVentas(int idSucursal, int idCliente, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool soloAnulados) => throw new NotImplementedException();
        public DataTable getVentasVendedorCierreCaja(CierreCaja oCierreE, bool soloAnulados) => throw new NotImplementedException();
        public float getTotalVenta(int idVenta) => throw new NotImplementedException();
        public float getTotalKgsVenta(int idVenta) => throw new NotImplementedException();
        public float obtenerTotalVentas(int idVendedor, int idSucursal, DateTime? fechaDesde, DateTime? fechaHasta) => throw new NotImplementedException();
        public LineaVenta agregarLineaVenta(LineaVenta oLineaE, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public void actualizarAlicuotaLineaVenta(int idLineaVenta, int idAlicuotaIva, float alicuotaIva) => throw new NotImplementedException();
        public void eliminarLineasVenta(int idVenta) => throw new NotImplementedException();
        public Venta getUltimaVentaVendedor(CierreCaja oCierreE) => throw new NotImplementedException();
        public List<LineaVenta> obtenerLineasVenta(int idVenta) => throw new NotImplementedException();
        public DataTable obtenerUltimosPreciosPorCliente(int idPersona, int topVentas = 10) => throw new NotImplementedException();
        public void agregarStockVenta(Venta oVentaE) => throw new NotImplementedException();
        public void agregarTemporalLineaVenta(TemporalLineaVenta oTemporalLV) => throw new NotImplementedException();
        public DataTable obtenerTemporalLineaVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto, bool conVentas) => throw new NotImplementedException();
        public DataTable getAllLineasVenta(int idSucursal, int idVendedor, DateTime fechaDesde, DateTime fechaHasta, string texto) => throw new NotImplementedException();
        public DataTable ultimasVentasCliente(int idSucursal, int idPersona) => throw new NotImplementedException();
        public void actualizarLetraId_TipoCbte(int idVenta, char letraId_tipoCbte) => throw new NotImplementedException();
        public void actualizarCliente(int idVenta, int idPersona) => throw new NotImplementedException();
        public DataTable obtenerSectores() => throw new NotImplementedException();
        public bool existeSector(string sector, string sectorActual = "") => throw new NotImplementedException();
        public void agregarSector(string sector) => throw new NotImplementedException();
        public void modificarSector(string sectorActual, string sectorNuevo) => throw new NotImplementedException();
        public bool sectorEstaEnUso(string sector) => throw new NotImplementedException();
        public void eliminarSector(string sector) => throw new NotImplementedException();
        public string getUltimoSectorSelect(string serialCPU) => throw new NotImplementedException();
        public int agregarExpendio(Venta oVentaE) => throw new NotImplementedException();
        public LineaVenta agregarLineaExprendio(LineaVenta oLineaE) => throw new NotImplementedException();
        public void asignarVentaEnExpendio(int idVenta, int idExpendio, Contratos.IUnitOfWork unitOfWork = null) => throw new NotImplementedException();
        public DataTable obtenerUltimosExpendios(int ultimosMinutos, int idSucursal) => throw new NotImplementedException();
        public DataTable obtenerExpendiosPorUsuario(int idSucursal, int idVendedor, int top = 100, DateTime? fechaDesde = null, DateTime? fechaHasta = null) => throw new NotImplementedException();
        public DataTable obtenerExpendiosEmpresa(int top = 300, DateTime? fechaDesde = null, DateTime? fechaHasta = null) => throw new NotImplementedException();
        public Venta getExpedioById(int idExpendio) => throw new NotImplementedException();
        public int esVentaSinFacturar(int idVenta, bool esNotaCredito) => throw new NotImplementedException();
        public int existeFacturaElect(int idVenta) => throw new NotImplementedException();
        public int existeNotaCreditoElect(int idVenta) => throw new NotImplementedException();
        public void addOrEditFactuElec(FacturaElectronica oFacturaElectronicaE) => throw new NotImplementedException();
        public FacturaElectronica getFactuElecById(int idFactuElec) => throw new NotImplementedException();
        public List<FacturaElectronica> BuscarFacturasPagina(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante, int pagina, int cantidad, int cantidadExtra) => throw new NotImplementedException();
        public (int Cantidad, decimal Total) ObtenerFacturasResumen(DateTime fechaDesde, DateTime fechaHasta, int idSucursal, string cliente, string vendedor, List<string> formasPago, List<int> codigosComprobante) => throw new NotImplementedException();
    }
}
