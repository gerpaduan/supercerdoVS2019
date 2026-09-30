using System;
using System.Collections.Generic;

namespace Negocio
{
    // Marcaciones de jornada. Solo Postgres (ver Negocio/Empleado.cs).
    public class RegistroJornada
    {
        private readonly Contratos.IRegistroJornadaRepository oRegistroD;
        private readonly Contratos.ILiquidacionSueldoRepository oLiquidacionD;

        public RegistroJornada(Contratos.IRegistroJornadaRepository repositorio, Contratos.ILiquidacionSueldoRepository liquidacionRepositorio)
        {
            oRegistroD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oLiquidacionD = liquidacionRepositorio ?? throw new ArgumentNullException(nameof(liquidacionRepositorio));
        }

        public List<Entidades.RegistroJornada> ListarPorEmpleadoYRango(int idEmpleado, DateTime desde, DateTime hasta)
        {
            return oRegistroD.ListarPorEmpleadoYRango(idEmpleado, desde, hasta);
        }

        public Entidades.RegistroJornada ObtenerAbiertoHoy(int idEmpleado)
        {
            return oRegistroD.ObtenerAbiertoHoy(idEmpleado, DateTime.Today);
        }

        // Fichaje de entrada/salida (empleados por Hora): resuelve automaticamente si corresponde
        // Ingreso o Salida segun si hay un registro de hoy abierto. horaCorregida/motivo: solo si
        // el empleado uso "Modificar hora" en el kiosco (Jornadas/Fichaje) -- en ese caso SI cuenta
        // como correccion (motivo obligatorio, queda marcado para el admin). Completar la salida
        // con la hora real (sin corregir) es el flujo normal, no cuenta como correccion.
        public (bool esIngreso, DateTime horaRegistrada, Entidades.RegistroJornada registro) Fichar(
            Entidades.Empleado empleado, int idUsuarioAccion, int? idDispositivoSeguro, DateTime? horaCorregida, string motivo)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.Hora)
                throw new InvalidOperationException("El fichaje de entrada/salida solo aplica a empleados que cobran por hora.");
            if (horaCorregida.HasValue && string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("El motivo es obligatorio para modificar la hora.");

            DateTime ahora = horaCorregida ?? DateTime.Now;
            var abierto = oRegistroD.ObtenerAbiertoHoy(empleado.Id, DateTime.Today);

            if (abierto != null)
            {
                abierto.HoraSalida = ahora.TimeOfDay;
                if (horaCorregida.HasValue)
                {
                    abierto.MotivoCorreccion = motivo.Trim();
                    abierto.ActualizadoPor = idUsuarioAccion;
                    oRegistroD.Editar(abierto);
                }
                else
                {
                    oRegistroD.CompletarSalida(abierto.Id, empleado.IdEmpresa, ahora.TimeOfDay);
                }
                return (false, ahora, abierto);
            }

            var nuevo = new Entidades.RegistroJornada
            {
                IdEmpresa = empleado.IdEmpresa,
                IdEmpleado = empleado.Id,
                Fecha = DateTime.Today,
                HoraEntrada = ahora.TimeOfDay,
                RegistradoPor = idUsuarioAccion,
                IdDispositivoSeguro = idDispositivoSeguro,
                MotivoCorreccion = horaCorregida.HasValue ? motivo.Trim() : null
            };
            oRegistroD.Agregar(nuevo);
            return (true, ahora, nuevo);
        }

        // Carga de jornada/media jornada completa (empleados por Jornada o MediaJornada), desde
        // "Mi Jornada" -- permite mas de una carga por dia (ver Empleado.RequiereRegistroJornada).
        public Entidades.RegistroJornada CargarJornada(Entidades.Empleado empleado, DateTime fecha, Entidades.Turno turno,
            int idUsuarioAccion)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.Jornada
                && empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.MediaJornada)
                throw new InvalidOperationException("Solo los empleados por jornada o media jornada cargan su jornada así.");

            var nuevo = new Entidades.RegistroJornada
            {
                IdEmpresa = empleado.IdEmpresa,
                IdEmpleado = empleado.Id,
                Fecha = fecha.Date,
                Turno = turno,
                Cantidad = 1m,
                RegistradoPor = idUsuarioAccion
            };
            oRegistroD.Agregar(nuevo);
            return nuevo;
        }

        // Correccion retroactiva (admin, o el propio empleado corrigiendo un olvido) -- solo
        // permitida mientras la fecha del registro no forme parte de una liquidacion confirmada.
        public void Corregir(int idRegistroJornada, int idEmpresa, TimeSpan? horaEntrada, TimeSpan? horaSalida,
            decimal? cantidad, string motivo, int idUsuarioAccion)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("El motivo es obligatorio para corregir una marcación.");

            var registro = oRegistroD.ObtenerPorId(idRegistroJornada, idEmpresa);
            if (registro == null)
                throw new InvalidOperationException("La marcación no existe.");

            if (oLiquidacionD.EstaCubiertaPorLiquidacionConfirmada(registro.IdEmpleado, registro.Fecha))
                throw new InvalidOperationException(
                    "Esta marcación ya forma parte de una liquidación confirmada. Elimine esa liquidación antes de corregirla.");

            registro.HoraEntrada = horaEntrada;
            registro.HoraSalida = horaSalida;
            registro.Cantidad = cantidad;
            registro.MotivoCorreccion = motivo.Trim();
            registro.ActualizadoPor = idUsuarioAccion;

            oRegistroD.Editar(registro);
        }

        public List<Entidades.RegistroJornada> ListarCorreccionesPendientes(int idEmpresa, int? idEmpleado)
        {
            return oRegistroD.ListarCorreccionesPendientes(idEmpresa, idEmpleado);
        }

        // Usado por Negocio.LiquidacionSueldo antes de calcular: bloquea re-liquidar un registro ya
        // liquidado, y por EmpleadosController/JornadasController para deshabilitar la edicion en
        // pantalla de un registro ya cubierto.
        public bool PuedeEditar(int idEmpleado, DateTime fecha)
        {
            return !oLiquidacionD.EstaCubiertaPorLiquidacionConfirmada(idEmpleado, fecha);
        }
    }
}
