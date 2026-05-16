using System;
using System.Collections.Generic;

namespace Web.Models
{
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
        public string SucursalNombre { get; set; }
        public bool DesdePos { get; set; }
        public int IdCierre { get; set; }
        public List<ComisionElectronicaFormaVm> FormasPago { get; set; }
        public decimal TotalEgreso { get; set; }
    }
}
