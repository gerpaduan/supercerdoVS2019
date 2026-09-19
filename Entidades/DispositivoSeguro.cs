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

        // Login solo desde dispositivos seguros (2026-09-19, ver docs/DECISIONS.md). Origen: "Manual"
        // (alta por un admin, ej. CPU ID del agente) o "Autoservicio" (el usuario lo autorizo con el
        // codigo enviado a su mail). EmailAlta: mail al que llego el codigo (solo Autoservicio).
        // Bloqueado: el admin lo bloqueo -- no cuenta como seguro y no se puede re-autorizar por mail.
        public string Origen { get; set; } = "Manual";
        public string EmailAlta { get; set; }
        public bool Bloqueado { get; set; }
    }
}
