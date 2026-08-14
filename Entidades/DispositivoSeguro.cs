using System;

namespace Entidades
{
    // Dispositivo (PC) marcado como seguro por un admin de la empresa. Loguearse desde uno de
    // ellos salta el bloqueo por IP del login (LoginRateLimiter), no el bloqueo por cuenta.
    public class DispositivoSeguro
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public string NumeroSerie { get; set; }
        public string Descripcion { get; set; }
        public DateTime CreadoUtc { get; set; }
        public int? IdUsuarioCreador { get; set; }
        public string NombreUsuarioCreador { get; set; }
    }
}
