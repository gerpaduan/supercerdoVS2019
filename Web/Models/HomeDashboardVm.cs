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
        }

        public bool PuedeVerDashboardDatos { get; set; }
        public int IdSucursalSeleccionada { get; set; }
        public string PeriodoDefault { get; set; }
        public List<SelectListItem> Sucursales { get; set; }
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
}
