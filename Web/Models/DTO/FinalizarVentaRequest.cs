using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.DTO
{
    public class FinalizarVentaRequest
    {
        public string FormaPago { get; set; }
        public bool EsPagoMixto { get; set; }
        public float Efectivo { get; set; }
        public int IdPersona { get; set; }
        public List<LineaVentaDto> LineasVenta { get; set; }
    }

}