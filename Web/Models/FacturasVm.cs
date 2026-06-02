using System;
using System.Collections.Generic;
using Entidades;

namespace Web.Models
{
    public class FacturasIndexVm
    {
        public FacturasIndexVm()
        {
            Facturas = new List<FacturaListadoItemVm>();
        }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public int IdSucursal { get; set; }
        public decimal TotalFacturado { get; set; }
        public List<FacturaListadoItemVm> Facturas { get; set; }
    }

    public class FacturaListadoItemVm
    {
        public FacturaElectronica Factura { get; set; }
        public Venta Venta { get; set; }
        public FacturaElectronica FacturaAsociada { get; set; }
        public FacturaElectronica NotaCreditoAsociada { get; set; }
    }

    public class FacturaDetalleVm
    {
        public FacturaElectronica Factura { get; set; }
        public Venta Venta { get; set; }
        public string ReturnUrl { get; set; }
        public FacturaElectronica FacturaAsociada { get; set; }
        public FacturaElectronica NotaCreditoAsociada { get; set; }
    }
}
