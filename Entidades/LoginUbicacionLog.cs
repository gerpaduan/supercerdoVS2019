using System;

namespace Entidades
{
    public class LoginUbicacionLog
    {
        public int IdUsuario { get; set; }
        public int IdSucursal { get; set; }
        public DateTime FechaHora { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public decimal? PrecisionMetros { get; set; }
        public decimal? DistanciaMetros { get; set; }
        public bool Permitido { get; set; }
        public string Motivo { get; set; }
        public string Ip { get; set; }
    }
}
