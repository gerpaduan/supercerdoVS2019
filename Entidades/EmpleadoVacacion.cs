using System;

namespace Entidades
{
    // Periodo de vacaciones de un empleado. Efectos: "Mi Jornada" no exige marcacion esos dias, y
    // CalcularPreview de la liquidacion agrega automaticamente una linea "Vacaciones (N dias)" por
    // esos dias en vez de exigir RegistroJornada. Calculo simplificado (tarifa vigente de Jornada
    // del empleado por dia), no reproduce la formula legal exacta de la LCT art. 155 -- ver
    // docs/DECISIONS.md.
    public class EmpleadoVacacion
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdEmpleado { get; set; }

        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
        public string Observaciones { get; set; }

        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }
    }
}
