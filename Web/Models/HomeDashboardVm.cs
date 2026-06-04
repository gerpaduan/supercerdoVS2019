using System.Collections.Generic;
using System.Web.Mvc;

namespace Web.Models
{
    public class HomeDashboardIndexVm
    {
        public HomeDashboardIndexVm()
        {
            Sucursales = new List<SelectListItem>();
            PeriodoDefault = "hoy";
            Finanzas = new DashboardFinanzasViewModel();
        }

        public bool PuedeVerDashboardDatos { get; set; }
        public int IdSucursalSeleccionada { get; set; }
        public string PeriodoDefault { get; set; }
        public List<SelectListItem> Sucursales { get; set; }
        public DashboardFinanzasViewModel Finanzas { get; set; }
    }

    public class DashboardResumenVm
    {
        public decimal VentasTotales { get; set; }
        public int CantidadVentas { get; set; }
        public int CantidadClientes { get; set; }
        public decimal PromedioPorVenta { get; set; }
        public decimal SaldoACobrar { get; set; }
        public decimal SaldoAPagar { get; set; }
        public string PeriodoEtiqueta { get; set; }
        public string SucursalEtiqueta { get; set; }
    }

    public class DashboardSerieHoraVm
    {
        public string Hora { get; set; }
        public decimal Total { get; set; }
        public int CantidadVentas { get; set; }
    }

    public class DashboardTopProductoVm
    {
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Producto { get; set; }
        public decimal Kg { get; set; }
        public decimal Importe { get; set; }
    }

    public class DashboardSaldoPersonaVm
    {
        public int IdPersona { get; set; }
        public string Persona { get; set; }
        public string Identificacion { get; set; }
        public decimal Saldo { get; set; }
    }

    public class DashboardUltimaVentaVm
    {
        public int IdVenta { get; set; }
        public string FechaHora { get; set; }
        public string Cliente { get; set; }
        public decimal Total { get; set; }
        public string Usuario { get; set; }
        public string Sucursal { get; set; }
        public string DetalleUrl { get; set; }
    }

    public class DashboardUltimoElaboradoVm
    {
        public int Id { get; set; }
        public string Fecha { get; set; }
        public string Producto { get; set; }
        public decimal Cantidad { get; set; }
        public string Sucursal { get; set; }
        public string Usuario { get; set; }
    }

    public class DashboardFinanzasViewModel
    {
        public DashboardFinanzasViewModel()
        {
            Resumen = new DashboardFinanzasResumenVm();
            Movimientos = new List<DashboardFinanzasMovimientoVm>();
            Cheques = new List<DashboardFinanzasChequeVm>();
        }

        public DashboardFinanzasResumenVm Resumen { get; set; }
        public List<DashboardFinanzasMovimientoVm> Movimientos { get; set; }
        public List<DashboardFinanzasChequeVm> Cheques { get; set; }
    }

    public class DashboardFinanzasResumenVm
    {
        public int CantidadConSaldo { get; set; }
        public int CantidadDeudores { get; set; }
        public int CantidadAcreedores { get; set; }
        public decimal TotalACobrar { get; set; }
        public decimal TotalAPagar { get; set; }
    }

    public class DashboardFinanzasMovimientoVm
    {
        public string Fecha { get; set; }
        public string Persona { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
    }

    public class DashboardMovimientoResumenVm
    {
        public string Fecha { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
    }

    public class DashboardFinanzasChequeVm
    {
        public string NroCheque { get; set; }
        public string Banco { get; set; }
        public string Titular { get; set; }
        public string FechaPago { get; set; }
        public decimal Importe { get; set; }
        public string Estado { get; set; }
        public string ObservacionCalculada { get; set; }
    }
}
