using System;

namespace Entidades
{
    // Tarifa vigente de UN empleado especifico -- nunca un valor general aplicado automaticamente
    // ("el sueldo es por empleado", decision del usuario 2026-09-29). Versionada en el tiempo
    // (append-only: nunca se edita/borra una fila, se inserta una nueva con su propio
    // VigenteDesde) y con dos dimensiones opcionales: Turno y DiaSemana (permite, ej., que el
    // sabado de manana pague distinto que el resto de la semana). Ver docs/DECISIONS.md y
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md ("Resolucion de la tarifa a aplicar").
    public class EmpleadoTarifa
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdEmpleado { get; set; }

        // Obligatorio cuando la FormaLiquidacion del empleado es Jornada o MediaJornada; null
        // para Hora/Semana/Quincena/Mes (no distinguen turno).
        public Turno? Turno { get; set; }

        // Override opcional para un dia puntual (ej. Sabado con valor mas alto). Null = aplica a
        // cualquier dia que no tenga una fila mas especifica para ese Turno.
        public DiaSemana? DiaSemana { get; set; }

        public decimal Valor { get; set; }
        public DateTime VigenteDesde { get; set; }

        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }
    }
}
