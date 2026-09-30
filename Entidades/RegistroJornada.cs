using System;

namespace Entidades
{
    // Marcacion de trabajo de un empleado -- solo aplica a empleados con FormaLiquidacion en
    // {Hora, MediaJornada, Jornada} (ver Empleado.RequiereRegistroJornada). Se carga desde el
    // fichaje en un dispositivo habilitado (JornadasController.Fichaje), desde "Mi Jornada", o la
    // corrige un admin. Ver docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class RegistroJornada
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdEmpleado { get; set; }

        public DateTime Fecha { get; set; }

        // Obligatorio para Jornada/MediaJornada (determina que EmpleadoTarifa aplica); null para Hora.
        public Turno? Turno { get; set; }

        // Solo Hora: permite mas de una entrada/salida por dia.
        public TimeSpan? HoraEntrada { get; set; }
        public TimeSpan? HoraSalida { get; set; }

        // 1 para Jornada; 1 por cada media jornada cargada (permite varias por dia); no aplica a Hora
        // (las horas trabajadas se derivan de HoraEntrada/HoraSalida al calcular la liquidacion).
        public decimal? Cantidad { get; set; }

        // Quien cargo la marcacion (el propio empleado via fichaje/Mi Jornada, o un admin corrigiendo).
        public int RegistradoPor { get; set; }

        // Desde que dispositivo habilitado se fichó, si fue por ese medio. Null si se cargo
        // manualmente desde una pantalla admin.
        public int? IdDispositivoSeguro { get; set; }

        // Motivo cuando se ficha con "Modificar hora" o se corrige un registro ya cargado.
        public string MotivoCorreccion { get; set; }

        // Marca administrativa (nunca autoasignable por el propio empleado al fichar -- ver
        // Negocio/RegistroJornada.cs) para que esta fecha pague la EmpleadoTarifa de
        // DiaSemana=Feriado si existe, antes que la del dia real.
        public bool EsFeriado { get; set; }

        // Nota libre que el empleado puede dejar al fichar (ej. "llegue tarde por trafico") --
        // NO implica una correccion de hora, distinta de MotivoCorreccion. Si Hora ficha entrada y
        // salida por separado sobre el mismo registro, se concatenan con etiqueta (ver
        // Negocio.RegistroJornada.Fichar).
        public string Observaciones { get; set; }

        public DateTime? Creado { get; set; }

        // Si tienen valor, el registro fue corregido despues de cargado originalmente (no es la
        // carga inicial) -- dispara la advertencia "Correcciones pendientes de revisar" al admin.
        public DateTime? Actualizado { get; set; }
        public int? ActualizadoPor { get; set; }

        public bool FueCorregido => Actualizado.HasValue;
    }
}
