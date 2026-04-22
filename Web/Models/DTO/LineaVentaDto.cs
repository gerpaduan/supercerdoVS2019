using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models.DTO
{
    public class LineaVentaDto
    {

        public int IdLineaVenta { get; set; }
        public int IdCorte { get; set; }
        public long Codigo { get; set; }
        public string Descripcion { get; set; }
        public float CantKg { get; set; }
        public float PrecioKg { get; set; }
        public float Importe { get; set; }
        public float IdAlicuotaIva { get; set; }        
        public float AlicuotaIva { get; set; }
        public float Bonificacion { get; set; }
        public int Estado { get; set; }
        public bool Balanza { get; set; }
        public int IndexAnulado { get; set; } = -1;
    }

}
