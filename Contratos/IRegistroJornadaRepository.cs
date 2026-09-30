using System;
using System.Collections.Generic;

namespace Contratos
{
    // Marcaciones de jornada (Entidades/RegistroJornada.cs). Solo Postgres (ver IEmpleadoRepository.cs).
    public interface IRegistroJornadaRepository
    {
        List<Entidades.RegistroJornada> ListarPorEmpleadoYRango(int idEmpleado, DateTime desde, DateTime hasta);
        Entidades.RegistroJornada ObtenerPorId(int id, int idEmpresa);

        // Registro de Hora de hoy con HoraEntrada sin HoraSalida (evento abierto) -- usado por el
        // fichaje y "Mi Jornada" para resolver automaticamente si toca Ingreso o Salida.
        Entidades.RegistroJornada ObtenerAbiertoHoy(int idEmpleado, DateTime fecha);

        int Agregar(Entidades.RegistroJornada registro);

        // Completa la salida de un registro abierto con la hora real de fichaje -- flujo normal,
        // NO cuenta como correccion (no marca Actualizado). Distinto de Editar. observaciones: nota
        // libre opcional del empleado al fichar la salida (ver Entidades/RegistroJornada.cs).
        void CompletarSalida(int id, int idEmpresa, TimeSpan horaSalida, string observaciones);

        // Corrige un registro ya cargado (valor distinto al original, o "Modificar hora" con
        // motivo): siempre marca Actualizado/ActualizadoPor/MotivoCorreccion.
        void Editar(Entidades.RegistroJornada registro);

        // Historial de auditoria (2026-09-30): una fila por cada correccion o alta manual, nunca se
        // pisa una entrada anterior. RegistrarHistorial se llama ANTES de aplicar el cambio (guarda
        // el estado "anterior"). ListarHistorial alimenta "Correcciones pendientes de revisar" en
        // Jornadas/Index -- idEmpleado null = todos los de la empresa.
        void RegistrarHistorial(Entidades.RegistroJornadaHistorial evento);
        List<Entidades.RegistroJornadaHistorial> ListarHistorial(int idEmpresa, int? idEmpleado);
    }
}
