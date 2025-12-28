using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.DTO
{
    public class LineaVentaDto
    {
        public long Codigo { get; set; }
        public float CantKg { get; set; }
        public float PrecioKg { get; set; }
        public float Bonificacion { get; set; }
        public int Estado { get; set; }
        public bool Balanza { get; set; }
    }

}