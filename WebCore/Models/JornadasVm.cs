using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    // "Mi Jornada": pantalla self-service del propio empleado logueado (ver
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md). Sin montos de dinero (2026-09-30, ver
    // docs/DECISIONS.md) -- solo marcado y listado de marcaciones recientes.
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

        // Texto del encabezado de "Marcaciones recientes" -- varia segun si tiene una liquidacion
        // Confirmada previa (arranca ahi) o no (cae a los ultimos 14 dias). Ver JornadasController.MiJornada.
        public string MarcacionesTitulo { get; set; } = "Marcaciones recientes (últimos 14 días)";

        public List<Entidades.RegistroJornada> MarcacionesRecientes { get; set; } = new List<Entidades.RegistroJornada>();
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
        public string Identificacion { get; set; } = "";
    }

    // Bifurca por FormaLiquidacion: Hora usa EsIngreso/HoraPropuestaTexto (igual que siempre);
    // Jornada/MediaJornada usa TurnoEstimado (calculado por la hora del reloj, corregible a mano).
    public class FichajeConfirmarVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public Entidades.Empleado.formaLiquidacion FormaLiquidacion { get; set; }

        public bool EsIngreso { get; set; }
        public string HoraPropuestaTexto { get; set; } = "";

        public Entidades.Turno? TurnoEstimado { get; set; }

        public static readonly string[] MotivosTipicos =
        {
            "Me olvidé de fichar",
            "Problema con el dispositivo/red",
            "Corrección administrativa"
        };
    }

    // Vista admin: registros de un empleado en un rango, mas "Correcciones pendientes de revisar"
    // (historial completo de correcciones/altas manuales, no solo el ultimo cambio -- 2026-09-30,
    // ver docs/DECISIONS.md).
    public class JornadasIndexVm
    {
        public List<EmpleadoResumenVm> Empleados { get; set; } = new List<EmpleadoResumenVm>();
        public int? IdEmpleadoSeleccionado { get; set; }
        public string FormaLiquidacionSeleccionado { get; set; }
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public List<Entidades.RegistroJornada> Registros { get; set; } = new List<Entidades.RegistroJornada>();
        public List<Entidades.RegistroJornadaHistorial> CorreccionesPendientes { get; set; } = new List<Entidades.RegistroJornadaHistorial>();

        // Resumen del rango filtrado para el empleado seleccionado (jornadas/medias jornadas u
        // horas trabajadas, segun su FormaLiquidacion) -- solo un conteo informativo en pantalla,
        // no toca tarifas/plata (eso lo hace Negocio.LiquidacionSueldo al liquidar).
        public string ResumenTotales { get; set; } = "";
    }
}
