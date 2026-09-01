using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class TipoEgresoCajaEditVm
    {
        public int Id { get; set; }
        public string TipoEgresoCaja { get; set; } = "";
        public bool EsGasto { get; set; }
        public bool Reservado { get; set; }
    }

    public class CalcularComisionesElectronicasVm
    {
        public CalcularComisionesElectronicasVm()
        {
            FormasPago = new List<ComisionElectronicaFormaVm>();
        }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public DateTime FechaEgreso { get; set; }
        public int IdTipoEgresoCaja { get; set; }
        public int IdSucursal { get; set; }
        public string SucursalNombre { get; set; } = "";
        public bool DesdePos { get; set; }
        public int IdCierre { get; set; }
        public List<ComisionElectronicaFormaVm> FormasPago { get; set; }
        public decimal TotalEgreso { get; set; }
    }

    public class ComisionElectronicaFormaVm
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public decimal TotalCobrado { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal ImporteComision { get; set; }
    }
}
