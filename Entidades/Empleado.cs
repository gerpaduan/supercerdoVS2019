using System;

namespace Entidades
{
    // Ficha de un empleado: vincula 1:1 una Persona (datos personales + cuenta corriente donde se
    // le acredita el sueldo) con un Usuario (login propio para marcar su jornada). Ver
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class Empleado
    {
        public enum formaLiquidacion
        {
            Hora,
            MediaJornada,
            Jornada,
            Semana,
            Quincena,
            Mes,
        }

        public int Id { get; set; }
        public int IdEmpresa { get; set; }

        public Persona Persona { get; set; }
        public Usuario Usuario { get; set; }

        // Codigo interno de busqueda/filtro, distinto del CUIT/DNI (que ya vive en
        // Persona.Identificacion). Unico por empresa.
        public string Legajo { get; set; }

        public formaLiquidacion FormaLiquidacion { get; set; }

        public DateTime FechaIngreso { get; set; }
        public DateTime? FechaBaja { get; set; }

        // Si esta trabajando actualmente. Al pasar a false, Negocio.Empleado desactiva en cascada
        // el Usuario vinculado (ver Negocio/Empleado.cs).
        public bool Activo { get; set; } = true;

        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }
        public DateTime? Actualizado { get; set; }
        public int? ActualizadoPor { get; set; }

        // Solo estos 3 tipos registran jornada (RegistroJornada); Semana/Quincena/Mes cobran el
        // fijo del periodo sin marcar asistencia (decision explicita, MVP -- ver docs/DECISIONS.md).
        public static bool RequiereRegistroJornada(formaLiquidacion forma)
        {
            return forma == formaLiquidacion.Hora || forma == formaLiquidacion.MediaJornada || forma == formaLiquidacion.Jornada;
        }
    }
}
