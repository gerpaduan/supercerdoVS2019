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
        // NO cuenta como correccion (no marca Actualizado). Distinto de Editar.
        void CompletarSalida(int id, int idEmpresa, TimeSpan horaSalida);

        // Corrige un registro ya cargado (valor distinto al original, o "Modificar hora" con
        // motivo): siempre marca Actualizado/ActualizadoPor/MotivoCorreccion.
        void Editar(Entidades.RegistroJornada registro);

        // Registros con Actualizado o MotivoCorreccion no nulos -- "Correcciones pendientes de
        // revisar" en la vista admin de Jornadas. idEmpleado null = todos los de la empresa.
        List<Entidades.RegistroJornada> ListarCorreccionesPendientes(int idEmpresa, int? idEmpleado);
    }
}
