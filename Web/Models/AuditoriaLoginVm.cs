using System;
using System.Collections.Generic;

namespace Web.Models
{
    public class AuditoriaLoginIndexVm
    {
        public string FechaDesde { get; set; }
        public string FechaHasta { get; set; }
        public List<AuditoriaLoginItemVm> Items { get; set; }

        public AuditoriaLoginIndexVm()
        {
            Items = new List<AuditoriaLoginItemVm>();
        }
    }

    public class AuditoriaLoginItemVm
    {
        public string UsuarioNombre { get; set; }
        public string SucursalNombre { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? PrecisionMetros { get; set; }
        public decimal? DistanciaMetros { get; set; }
        public bool Permitido { get; set; }
        public string Motivo { get; set; }
        public string Ip { get; set; }

        public bool TieneCoordenadas
        {
            get { return Latitud.HasValue && Longitud.HasValue; }
        }
    }
}
