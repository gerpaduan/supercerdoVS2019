using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class MovimientoIndexVm
    {
        public MovimientoIndexVm()
        {
            Movimientos = new List<MovimientoResumenVm>();
        }

        public int IdSucursalOrigen { get; set; }
        public int IdSucursalDestino { get; set; }
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public bool VerDetalles { get; set; }
        public bool MostrarColumnasInternas { get; set; }
        public List<MovimientoResumenVm> Movimientos { get; set; }
    }

    public class MovimientoResumenVm
    {
        public int IdMovimiento { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string Origen { get; set; }
        public string Destino { get; set; }
        public string DeOrigen { get; set; }
        public string Estado { get; set; }
        public decimal TotalUnidad { get; set; }
        public decimal TotalKilos { get; set; }
        public string Observaciones { get; set; }
        public bool TieneObservaciones { get; set; }
    }

    public class MovimientoDetalleVm
    {
        public MovimientoDetalleVm()
        {
            Lineas = new List<MovimientoLineaVm>();
        }

        public int IdMovimiento { get; set; }
        public string Observaciones { get; set; }
        public List<MovimientoLineaVm> Lineas { get; set; }
    }
}
