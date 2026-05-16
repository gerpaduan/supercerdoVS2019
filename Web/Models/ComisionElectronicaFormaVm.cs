using System;

namespace Web.Models
{
    public class ComisionElectronicaFormaVm
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal TotalCobrado { get; set; }
        public decimal Porcentaje { get; set; }
        public decimal ImporteComision { get; set; }
    }
}
