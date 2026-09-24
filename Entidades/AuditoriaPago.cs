using System;

namespace Entidades
{
    // Registro append-only de un cambio sensible sobre un pago/cobro: cambio de persona o eliminacion.
    // Tabla auditoriapagos (Postgres) / AuditoriaPagos (SQL Server).
    public class AuditoriaPago
    {
        public const string TipoCambioPersona = "CAMBIO_PERSONA";
        public const string TipoEliminacion = "ELIMINACION";

        public int Id { get; set; }
        public int IdPago { get; set; }
        public string Tipo { get; set; }
        public int? IdPersonaAnterior { get; set; }
        public int? IdPersonaNueva { get; set; }
        public int IdUsuario { get; set; }
        public DateTime Fecha { get; set; }
        public string Detalle { get; set; }
    }
}
