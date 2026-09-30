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

        // Concatena una observacion nueva a la existente con una etiqueta (Entrada/Salida) -- Hora
        // comparte una sola fila entre entrada y salida, no se pisa lo que ya se haya dejado escrito.
        private static string CombinarObservacion(string existente, string etiqueta, string nueva)
        {
            if (string.IsNullOrWhiteSpace(nueva)) return existente;
            string parte = $"[{etiqueta}] {nueva.Trim()}";
            return string.IsNullOrWhiteSpace(existente) ? parte : existente + " | " + parte;
        }

        // Jornada/MediaJornada asisten una sola vez por dia (a diferencia de Hora, que puede tener
        // varios pares entrada/salida en el mismo dia -- turno mañana + turno tarde, etc.). Se usa
        // tanto en la carga en tiempo real (CargarJornada) como en el alta manual/autoreporte
        // (AgregarManual) para que ninguna de las dos vias termine duplicando el dia.
        private bool YaAsistioEseDia(int idEmpleado, DateTime fecha)
        {
            return oRegistroD.ListarPorEmpleadoYRango(idEmpleado, fecha.Date, fecha.Date).Count > 0;
        }

        // Fichaje de entrada/salida (empleados por Hora): resuelve automaticamente si corresponde
        // Ingreso o Salida segun si hay un registro de hoy abierto. horaCorregida/motivo: solo si
        // el empleado uso "Modificar hora" en el kiosco (Jornadas/Fichaje) -- en ese caso SI cuenta
        // como correccion (motivo obligatorio, queda marcado para el admin). Completar la salida
        // con la hora real (sin corregir) es el flujo normal, no cuenta como correccion.
        // observacion: nota libre opcional (ej. "llegue tarde por trafico"), independiente de
        // horaCorregida/motivo -- nunca genera una fila de historial por si sola.
        public (bool esIngreso, DateTime horaRegistrada, Entidades.RegistroJornada registro) Fichar(
            Entidades.Empleado empleado, int idUsuarioAccion, int? idDispositivoSeguro, DateTime? horaCorregida, string motivo, string observacion = null)
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
                abierto.Observaciones = CombinarObservacion(abierto.Observaciones, "Salida", observacion);

                if (horaCorregida.HasValue)
                {
                    abierto.MotivoCorreccion = motivo.Trim();
                    abierto.ActualizadoPor = idUsuarioAccion;
                    oRegistroD.Editar(abierto);
                }
                else
                {
                    oRegistroD.CompletarSalida(abierto.Id, empleado.IdEmpresa, ahora.TimeOfDay, abierto.Observaciones);
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
                MotivoCorreccion = horaCorregida.HasValue ? motivo.Trim() : null,
                Observaciones = CombinarObservacion(null, "Entrada", observacion)
            };
            oRegistroD.Agregar(nuevo);
            return (true, ahora, nuevo);
        }

        // Carga de jornada/media jornada completa (empleados por Jornada o MediaJornada), desde
        // "Mi Jornada" -- una sola carga por dia (ver YaAsistioEseDia): no se puede fichar Jornada
        // ni MediaJornada dos veces el mismo dia.
        public Entidades.RegistroJornada CargarJornada(Entidades.Empleado empleado, DateTime fecha, Entidades.Turno turno,
            int idUsuarioAccion, string observacion = null)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.Jornada
                && empleado.FormaLiquidacion != Entidades.Empleado.formaLiquidacion.MediaJornada)
                throw new InvalidOperationException("Solo los empleados por jornada o media jornada cargan su jornada así.");

            if (YaAsistioEseDia(empleado.Id, fecha))
                throw new InvalidOperationException("Ya se registró la jornada de este empleado para ese día.");

            var nuevo = new Entidades.RegistroJornada
            {
                IdEmpresa = empleado.IdEmpresa,
                IdEmpleado = empleado.Id,
                Fecha = fecha.Date,
                Turno = turno,
                Cantidad = 1m,
                RegistradoPor = idUsuarioAccion,
                Observaciones = string.IsNullOrWhiteSpace(observacion) ? null : observacion.Trim()
            };
            oRegistroD.Agregar(nuevo);
            return nuevo;
        }

        // Correccion retroactiva de un admin (turno/horas/cantidad/feriado) -- solo permitida
        // mientras la fecha del registro no forme parte de una liquidacion confirmada. Deja una
        // fila en el historial con el estado ANTERIOR antes de aplicar el cambio (2026-09-30, ver
        // docs/DECISIONS.md) -- si la misma marcacion se corrige varias veces, cada una queda.
        public void Corregir(int idRegistroJornada, int idEmpresa, TimeSpan? horaEntrada, TimeSpan? horaSalida,
            decimal? cantidad, Entidades.Turno? turno, bool? esFeriado, string motivo, int idUsuarioAccion)
        {
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("El motivo es obligatorio para corregir una marcación.");

            var registro = oRegistroD.ObtenerPorId(idRegistroJornada, idEmpresa);
            if (registro == null)
                throw new InvalidOperationException("La marcación no existe.");

            if (oLiquidacionD.EstaCubiertaPorLiquidacionConfirmada(registro.IdEmpleado, registro.Fecha))
                throw new InvalidOperationException(
                    "Esta marcación ya forma parte de una liquidación confirmada. Elimine esa liquidación antes de corregirla.");

            oRegistroD.RegistrarHistorial(new Entidades.RegistroJornadaHistorial
            {
                IdEmpresa = idEmpresa,
                IdRegistroJornada = registro.Id,
                IdEmpleado = registro.IdEmpleado,
                FechaRegistroJornada = registro.Fecha,
                FechaAnterior = registro.Fecha,
                TurnoAnterior = registro.Turno,
                HoraEntradaAnterior = registro.HoraEntrada,
                HoraSalidaAnterior = registro.HoraSalida,
                CantidadAnterior = registro.Cantidad,
                EsFeriadoAnterior = registro.EsFeriado,
                TipoEvento = Entidades.RegistroJornadaHistorial.tipoEvento.Correccion,
                Motivo = motivo.Trim(),
                ModificadoPor = idUsuarioAccion,
                FechaEvento = DateTime.Now
            });

            registro.HoraEntrada = horaEntrada;
            registro.HoraSalida = horaSalida;
            registro.Cantidad = cantidad;
            if (turno.HasValue) registro.Turno = turno;
            if (esFeriado.HasValue) registro.EsFeriado = esFeriado.Value;
            registro.MotivoCorreccion = motivo.Trim();
            registro.ActualizadoPor = idUsuarioAccion;

            oRegistroD.Editar(registro);
        }

        // Alta manual de una marcacion que no se cargo en el momento -- sirve tanto para un admin
        // (esAutoreporte:false, Jornadas/Index) como para el propio empleado cargando un dia
        // olvidado desde "Mi Jornada" (esAutoreporte:true). Motivo siempre obligatorio. Cuando
        // esAutoreporte, esFeriado se ignora y queda en false -- un empleado no puede
        // autoasignarse un feriado (ver docs/DECISIONS.md).
        public Entidades.RegistroJornada AgregarManual(Entidades.Empleado empleado, DateTime fecha, Entidades.Turno? turno,
            TimeSpan? horaEntrada, TimeSpan? horaSalida, decimal? cantidad, bool esFeriado, string motivo,
            int idUsuarioAccion, bool esAutoreporte)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (string.IsNullOrWhiteSpace(motivo))
                throw new InvalidOperationException("El motivo es obligatorio para cargar una marcación manual.");

            if (oLiquidacionD.EstaCubiertaPorLiquidacionConfirmada(empleado.Id, fecha))
                throw new InvalidOperationException(
                    "Ese período ya tiene una liquidación confirmada. Elimínela antes de agregar una marcación ahí.");

            // Jornada/MediaJornada: una sola asistencia por dia, tambien en el alta manual (admin) y
            // en el autoreporte propio -- Hora queda afuera, puede tener varios pares entrada/salida
            // en el mismo dia (ver YaAsistioEseDia).
            if ((empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.Jornada
                 || empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.MediaJornada)
                && YaAsistioEseDia(empleado.Id, fecha))
                throw new InvalidOperationException("Ya se registró la jornada de este empleado para ese día.");

            var nuevo = new Entidades.RegistroJornada
            {
                IdEmpresa = empleado.IdEmpresa,
                IdEmpleado = empleado.Id,
                Fecha = fecha.Date,
                Turno = turno,
                HoraEntrada = horaEntrada,
                HoraSalida = horaSalida,
                Cantidad = cantidad,
                EsFeriado = esAutoreporte ? false : esFeriado,
                MotivoCorreccion = motivo.Trim(),
                RegistradoPor = idUsuarioAccion
            };
            oRegistroD.Agregar(nuevo);

            oRegistroD.RegistrarHistorial(new Entidades.RegistroJornadaHistorial
            {
                IdEmpresa = empleado.IdEmpresa,
                IdRegistroJornada = nuevo.Id,
                IdEmpleado = empleado.Id,
                FechaRegistroJornada = nuevo.Fecha,
                TipoEvento = esAutoreporte
                    ? Entidades.RegistroJornadaHistorial.tipoEvento.AltaManualPropia
                    : Entidades.RegistroJornadaHistorial.tipoEvento.AltaManualAdmin,
                Motivo = motivo.Trim(),
                ModificadoPor = idUsuarioAccion,
                FechaEvento = DateTime.Now
            });

            return nuevo;
        }

        // "Correcciones pendientes de revisar" en Jornadas/Index -- historial completo (una fila por
        // cada correccion o alta manual, no una por marcacion), para no perder el rastro si la misma
        // marcacion se corrigio mas de una vez.
        public List<Entidades.RegistroJornadaHistorial> ListarCorreccionesPendientes(int idEmpresa, int? idEmpleado)
        {
            return oRegistroD.ListarHistorial(idEmpresa, idEmpleado);
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
