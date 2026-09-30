using System;

namespace Entidades
{
    // Auditoria de RegistroJornada (2026-09-30, ver docs/DECISIONS.md): una fila por cada
    // correccion o alta manual -- nunca se pisa una entrada anterior, aunque la misma marcacion se
    // corrija varias veces. RegistroJornada.Actualizado/ActualizadoPor/MotivoCorreccion siguen
    // existiendo como "resumen del ultimo cambio" (para no hacer join en vistas que solo necesitan
    // saber si se corrigio alguna vez); esta tabla es el historial completo.
    public class RegistroJornadaHistorial
    {
        public enum tipoEvento
        {
            Correccion,
            AltaManualAdmin,
            // Cargado por el propio empleado desde "Mi Jornada" (dia olvidado) -- queda
            // diferenciado de una correccion/alta de admin porque tiene menos verificacion (nadie
            // mas lo confirmo en el momento).
            AltaManualPropia,
        }

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdRegistroJornada { get; set; }
        public int IdEmpleado { get; set; }
        public DateTime FechaRegistroJornada { get; set; }

        // Foto del estado ANTES de este evento puntual. Todos nulos si TipoEvento es un alta
        // manual (no habia nada antes).
        public DateTime? FechaAnterior { get; set; }
        public Turno? TurnoAnterior { get; set; }
        public TimeSpan? HoraEntradaAnterior { get; set; }
        public TimeSpan? HoraSalidaAnterior { get; set; }
        public decimal? CantidadAnterior { get; set; }
        public bool? EsFeriadoAnterior { get; set; }

        public tipoEvento TipoEvento { get; set; }
        public string Motivo { get; set; }
        public int ModificadoPor { get; set; }
        public DateTime FechaEvento { get; set; }
    }
}
