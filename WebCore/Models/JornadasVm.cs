using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    // "Mi Jornada": pantalla self-service del propio empleado logueado (ver
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md).
    public class MiJornadaVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public Entidades.Empleado.formaLiquidacion FormaLiquidacion { get; set; }
        public bool RequiereRegistro { get; set; }

        // Solo Hora: hay una entrada de hoy sin salida (para mostrar "marcar salida" en vez de "marcar entrada").
        public bool EstaAbierto { get; set; }

        // No vacio si hoy cae dentro de un periodo de vacaciones del empleado.
        public string VacacionAvisoHasta { get; set; } = "";

        public List<Entidades.RegistroJornada> MarcacionesRecientes { get; set; } = new List<Entidades.RegistroJornada>();

        // "Jornada activa": todo lo marcado desde el final de la ultima liquidacion Confirmada
        // hasta hoy (estimado, no es una liquidacion confirmada).
        public DateTime JornadaActivaDesde { get; set; }
        public List<Entidades.LiquidacionSueldoDetalle> JornadaActivaDetalle { get; set; } = new List<Entidades.LiquidacionSueldoDetalle>();
        public decimal JornadaActivaTotalEstimado { get; set; }
    }

    // Fichaje en un dispositivo habilitado (Jornadas/Fichaje) -- ver
    // "Fichaje en dispositivos habilitados" en docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class FichajeListaVm
    {
        public bool DispositivoHabilitado { get; set; }
        public List<EmpleadoFichajeItemVm> Empleados { get; set; } = new List<EmpleadoFichajeItemVm>();
    }

    public class EmpleadoFichajeItemVm
    {
        public int IdEmpleado { get; set; }
        public string Nombre { get; set; } = "";
        public string Legajo { get; set; } = "";
    }

    public class FichajeConfirmarVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public bool EsIngreso { get; set; }
        public string HoraPropuestaTexto { get; set; } = "";

        public static readonly string[] MotivosTipicos =
        {
            "Me olvidé de fichar",
            "Problema con el dispositivo/red",
            "Corrección administrativa"
        };
    }

    // Vista admin: registros de un empleado en un rango, mas "Correcciones pendientes de revisar".
    public class JornadasIndexVm
    {
        public List<EmpleadoResumenVm> Empleados { get; set; } = new List<EmpleadoResumenVm>();
        public int? IdEmpleadoSeleccionado { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public List<Entidades.RegistroJornada> Registros { get; set; } = new List<Entidades.RegistroJornada>();
        public List<Entidades.RegistroJornada> CorreccionesPendientes { get; set; } = new List<Entidades.RegistroJornada>();
    }
}
