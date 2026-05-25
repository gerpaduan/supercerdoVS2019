using System.Collections.Generic;

namespace Web.Models
{
    public class IngresoBilletesPrintVm
    {
        public int TicketMm { get; set; }
        public decimal Monedas { get; set; }
        public decimal Total { get; set; }
        public List<IngresoBilletesDenominacionVm> Denominaciones { get; set; }
    }

    public class IngresoBilletesDenominacionVm
    {
        public int Denominacion { get; set; }
        public int Cantidad { get; set; }
    }
}
