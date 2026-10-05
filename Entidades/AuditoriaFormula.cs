using System;

namespace Entidades
{
    // Registro append-only de un acceso o cambio sobre una formula secreta (VER / OCULTAR / FALLO /
    // MARCAR_SECRETA / QUITAR_SECRETA). Tabla auditoriaformulas (Postgres) / AuditoriaFormulas
    // (SQL Server). Nunca contiene claves ni datos personales en Detalle.
    public class AuditoriaFormula
    {
        public const string TipoVer = "VER";
        public const string TipoOcultar = "OCULTAR";
        public const string TipoFallo = "FALLO";
        public const string TipoMarcarSecreta = "MARCAR_SECRETA";
        public const string TipoQuitarSecreta = "QUITAR_SECRETA";

        public int Id { get; set; }
        public string Tipo { get; set; }
        // Usuario que se autentico en el step-up (quien ve la formula). Null si el usuario tipeado no existe.
        public int? IdUsuario { get; set; }
        // Usuario logueado en la sesion que opera la pantalla (quien pidio verla).
        public int? IdUsuarioSesion { get; set; }
        // Producto elaborado (formulas.idembutido guarda el idcorte).
        public int? IdCorte { get; set; }
        public string Ip { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; }
    }
}
