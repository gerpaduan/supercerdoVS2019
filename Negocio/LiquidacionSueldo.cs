using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Calculo y confirmacion de liquidaciones de sueldo. Solo Postgres (ver Negocio/Empleado.cs).
    // "Liquidación" = devengamiento del sueldo bruto del período: se acredita en la cuenta
    // corriente de la Persona del empleado (Crédito, mismo signo que usa Compra al acreditar a un
    // proveedor). El cobro real se hace despues con Finanzas > Pagos, reutilizando esa misma cta
    // cte -- ver docs/03-modulos/empleados-y-liquidacion-sueldos.md ("Naming").
    public class LiquidacionSueldo
    {
        private readonly Contratos.ILiquidacionSueldoRepository oLiquidacionD;
        private readonly Contratos.IRegistroJornadaRepository oRegistroD;
        private readonly Contratos.IEmpleadoRepository oEmpleadoD;
        private readonly Negocio.CuentaCorriente oCtaCteN;

        public LiquidacionSueldo(Contratos.ILiquidacionSueldoRepository repositorio, Contratos.IRegistroJornadaRepository registroJornadaRepositorio,
            Contratos.IEmpleadoRepository empleadoRepositorio, Negocio.CuentaCorriente ctaCteN)
        {
            oLiquidacionD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            oRegistroD = registroJornadaRepositorio ?? throw new ArgumentNullException(nameof(registroJornadaRepositorio));
            oEmpleadoD = empleadoRepositorio ?? throw new ArgumentNullException(nameof(empleadoRepositorio));
            oCtaCteN = ctaCteN ?? throw new ArgumentNullException(nameof(ctaCteN));
        }

        public List<Entidades.LiquidacionSueldo> ListarPorEmpleado(int idEmpleado)
        {
            return oLiquidacionD.ListarPorEmpleado(idEmpleado);
        }

        public Entidades.LiquidacionSueldo ObtenerPorId(int id, int idEmpresa)
        {
            return oLiquidacionD.ObtenerPorId(id, idEmpresa);
        }

        public Entidades.LiquidacionSueldo ObtenerUltimaConfirmada(int idEmpleado)
        {
            return oLiquidacionD.ObtenerUltimaConfirmada(idEmpleado);
        }

        public class ResultadoPreview
        {
            public bool TieneConflicto { get; set; }
            public Entidades.LiquidacionSueldo LiquidacionEnConflicto { get; set; }
            public Entidades.LiquidacionSueldo Liquidacion { get; set; }
        }

        // Calcula (sin persistir) la liquidacion de un empleado para un rango de fecha+hora. Nunca
        // inventa una tarifa faltante (CLAUDE.md §2.7): las lineas sin EmpleadoTarifa configurada
        // quedan con ValorUnitario=0 y SinTarifaConfigurada=true, para que el usuario las complete
        // a mano antes de confirmar.
        public ResultadoPreview CalcularPreview(Entidades.Empleado empleado, DateTime desde, DateTime hasta)
        {
            if (empleado == null) throw new ArgumentNullException(nameof(empleado));
            if (hasta <= desde) throw new InvalidOperationException("La fecha/hora hasta debe ser posterior a la fecha/hora desde.");

            var solapada = oLiquidacionD.ObtenerSolapada(empleado.Id, desde, hasta, 0);
            if (solapada != null)
                return new ResultadoPreview { TieneConflicto = true, LiquidacionEnConflicto = solapada };

            var liquidacion = new Entidades.LiquidacionSueldo
            {
                IdEmpresa = empleado.IdEmpresa,
                IdEmpleado = empleado.Id,
                PeriodoDesde = desde,
                PeriodoHasta = hasta,
                FechaLiquidacion = DateTime.Today,
                Estado = Entidades.LiquidacionSueldo.estadoLiquidacion.Confirmada
            };

            var historialTarifas = oEmpleadoD.ListarTarifas(empleado.Id);
            string resumenTexto;

            switch (empleado.FormaLiquidacion)
            {
                case Entidades.Empleado.formaLiquidacion.Hora:
                    resumenTexto = CalcularHora(empleado, desde, hasta, historialTarifas, liquidacion);
                    break;
                case Entidades.Empleado.formaLiquidacion.Jornada:
                case Entidades.Empleado.formaLiquidacion.MediaJornada:
                    resumenTexto = CalcularJornadaOMediaJornada(empleado, desde, hasta, historialTarifas, liquidacion);
                    break;
                default: // Semana, Quincena, Mes: fijo del periodo, sin registro de jornada (MVP)
                    resumenTexto = CalcularFijo(empleado, hasta, historialTarifas, liquidacion);
                    break;
            }

            AgregarLineaVacaciones(empleado, desde, hasta, historialTarifas, liquidacion);

            foreach (var linea in liquidacion.Detalle)
                linea.Subtotal = linea.Cantidad * linea.ValorUnitario;
            liquidacion.TotalLiquidado = liquidacion.Detalle.Sum(d => d.Subtotal);

            liquidacion.DetalleCtaCte = $"Liquidación de sueldo del {desde:dd/MM/yyyy HH:mm} al {hasta:dd/MM/yyyy HH:mm}"
                + (string.IsNullOrEmpty(resumenTexto) ? "" : " - " + resumenTexto);

            return new ResultadoPreview { TieneConflicto = false, Liquidacion = liquidacion };
        }

        private string CalcularHora(Entidades.Empleado empleado, DateTime desde, DateTime hasta,
            List<Entidades.EmpleadoTarifa> historialTarifas, Entidades.LiquidacionSueldo liquidacion)
        {
            var registros = oRegistroD.ListarPorEmpleadoYRango(empleado.Id, desde.Date, hasta.Date)
                .Where(r => r.HoraEntrada.HasValue && r.HoraSalida.HasValue);

            // Agrupa las horas por si el dia esta marcado feriado o no (2026-09-30, ver
            // docs/DECISIONS.md) -- Hora no distingue turno (Turno=null en EmpleadoTarifa), pero SI
            // puede tener una tarifa especial de DiaSemana=Feriado.
            var horasPorFeriado = new Dictionary<bool, decimal>();
            foreach (var r in registros)
            {
                DateTime inicio = r.Fecha.Date + r.HoraEntrada.Value;
                DateTime fin = r.Fecha.Date + r.HoraSalida.Value;
                if (fin <= inicio) continue; // dato invalido (salida antes que entrada): se ignora, no se inventa

                DateTime inicioClip = inicio < desde ? desde : inicio;
                DateTime finClip = fin > hasta ? hasta : fin;
                if (finClip <= inicioClip) continue;

                horasPorFeriado.TryGetValue(r.EsFeriado, out var acumulado);
                horasPorFeriado[r.EsFeriado] = acumulado + (decimal)(finClip - inicioClip).TotalHours;
            }

            decimal totalHoras = 0;
            foreach (var grupo in horasPorFeriado.OrderBy(g => g.Key))
            {
                if (grupo.Value <= 0) continue;
                totalHoras += grupo.Value;

                var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, null, hasta.Date, grupo.Key);
                string sufijo = grupo.Key ? " (feriado)" : "";
                liquidacion.Detalle.Add(new Entidades.LiquidacionSueldoDetalle
                {
                    Concepto = $"{grupo.Value:0.##} horas trabajadas{sufijo}",
                    Origen = Entidades.LiquidacionSueldoDetalle.origenDetalle.Automatico,
                    Cantidad = grupo.Value,
                    ValorUnitario = tarifa?.Valor ?? 0,
                    SinTarifaConfigurada = tarifa == null
                });
            }

            if (totalHoras <= 0) return "0 hs";
            return $"{totalHoras:0.##} hs";
        }

        private string CalcularJornadaOMediaJornada(Entidades.Empleado empleado, DateTime desde, DateTime hasta,
            List<Entidades.EmpleadoTarifa> historialTarifas, Entidades.LiquidacionSueldo liquidacion)
        {
            var registros = oRegistroD.ListarPorEmpleadoYRango(empleado.Id, desde.Date, hasta.Date)
                .Where(r => r.Turno.HasValue && r.Cantidad.HasValue)
                .ToList();

            string unidad = empleado.FormaLiquidacion == Entidades.Empleado.formaLiquidacion.MediaJornada ? "media jornada" : "jornada";

            // Agrupa por (Turno, tarifa resuelta para esa fecha): si dos dias del mismo turno
            // resuelven a tarifas distintas (ej. sabado con valor especial), quedan como lineas
            // separadas -- mismo turno, distinto ValorUnitario (ver docs/DECISIONS.md).
            var grupos = new Dictionary<(Entidades.Turno turno, int? idTarifa), (decimal cantidad, Entidades.EmpleadoTarifa tarifa)>();
            var totalesPorTurno = new Dictionary<Entidades.Turno, decimal>();

            foreach (var r in registros)
            {
                var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, r.Turno, r.Fecha, r.EsFeriado);
                var clave = (r.Turno.Value, tarifa?.Id);
                grupos.TryGetValue(clave, out var acumulado);
                acumulado.tarifa = tarifa;
                acumulado.cantidad += r.Cantidad.Value;
                grupos[clave] = acumulado;

                totalesPorTurno.TryGetValue(r.Turno.Value, out var totalTurno);
                totalesPorTurno[r.Turno.Value] = totalTurno + r.Cantidad.Value;
            }

            foreach (var grupo in grupos.OrderBy(g => g.Key.turno))
            {
                string nombreTurno = grupo.Key.turno == Entidades.Turno.Manana ? "mañana" : "tarde";
                liquidacion.Detalle.Add(new Entidades.LiquidacionSueldoDetalle
                {
                    Concepto = $"{grupo.Value.cantidad:0.##} {unidad}(s) turno {nombreTurno}",
                    Origen = Entidades.LiquidacionSueldoDetalle.origenDetalle.Automatico,
                    Cantidad = grupo.Value.cantidad,
                    ValorUnitario = grupo.Value.tarifa?.Valor ?? 0,
                    SinTarifaConfigurada = grupo.Value.tarifa == null
                });
            }

            var partes = new List<string>();
            if (totalesPorTurno.TryGetValue(Entidades.Turno.Manana, out var manana) && manana > 0)
                partes.Add($"{manana:0.##} mañana(s)");
            if (totalesPorTurno.TryGetValue(Entidades.Turno.Tarde, out var tarde) && tarde > 0)
                partes.Add($"{tarde:0.##} tarde(s)");

            return string.Join(" / ", partes);
        }

        private string CalcularFijo(Entidades.Empleado empleado, DateTime hasta,
            List<Entidades.EmpleadoTarifa> historialTarifas, Entidades.LiquidacionSueldo liquidacion)
        {
            var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, null, hasta.Date);
            string nombreForma = NombreForma(empleado.FormaLiquidacion);

            liquidacion.Detalle.Add(new Entidades.LiquidacionSueldoDetalle
            {
                Concepto = "Sueldo " + nombreForma,
                Origen = Entidades.LiquidacionSueldoDetalle.origenDetalle.Automatico,
                Cantidad = 1,
                ValorUnitario = tarifa?.Valor ?? 0,
                SinTarifaConfigurada = tarifa == null
            });

            return nombreForma;
        }

        private static string NombreForma(Entidades.Empleado.formaLiquidacion forma)
        {
            switch (forma)
            {
                case Entidades.Empleado.formaLiquidacion.Semana: return "semanal";
                case Entidades.Empleado.formaLiquidacion.Quincena: return "quincenal";
                case Entidades.Empleado.formaLiquidacion.Mes: return "mensual";
                default: return forma.ToString();
            }
        }

        // Vacaciones: solo para empleados que registran jornada (Hora/Jornada/MediaJornada) -- los
        // de Semana/Quincena/Mes ya cobran el fijo completo sin importar ausencias (decision MVP),
        // sumarles ademas una linea de vacaciones los pagaria dos veces. Calculo simplificado (tarifa
        // de Jornada del empleado por dia), no reproduce la formula legal exacta de la LCT art. 155
        // -- ver docs/DECISIONS.md.
        private void AgregarLineaVacaciones(Entidades.Empleado empleado, DateTime desde, DateTime hasta,
            List<Entidades.EmpleadoTarifa> historialTarifas, Entidades.LiquidacionSueldo liquidacion)
        {
            if (!Entidades.Empleado.RequiereRegistroJornada(empleado.FormaLiquidacion)) return;

            var vacaciones = oEmpleadoD.ListarVacaciones(empleado.Id)
                .Where(v => v.FechaDesde.Date <= hasta.Date && v.FechaHasta.Date >= desde.Date)
                .ToList();
            if (vacaciones.Count == 0) return;

            int dias = 0;
            foreach (var v in vacaciones)
            {
                DateTime ini = v.FechaDesde.Date < desde.Date ? desde.Date : v.FechaDesde.Date;
                DateTime fin = v.FechaHasta.Date > hasta.Date ? hasta.Date : v.FechaHasta.Date;
                if (fin >= ini) dias += (fin - ini).Days + 1;
            }
            if (dias <= 0) return;

            var tarifa = Negocio.Empleado.ResolverTarifaVigente(historialTarifas, Entidades.Turno.Manana, hasta.Date)
                ?? Negocio.Empleado.ResolverTarifaVigente(historialTarifas, Entidades.Turno.Tarde, hasta.Date)
                ?? Negocio.Empleado.ResolverTarifaVigente(historialTarifas, null, hasta.Date);

            liquidacion.Detalle.Add(new Entidades.LiquidacionSueldoDetalle
            {
                Concepto = $"Vacaciones ({dias} día{(dias == 1 ? "" : "s")})",
                Origen = Entidades.LiquidacionSueldoDetalle.origenDetalle.Automatico,
                Cantidad = dias,
                ValorUnitario = tarifa?.Valor ?? 0,
                SinTarifaConfigurada = tarifa == null
            });
        }

        // Persiste la liquidacion (con el Detalle ya revisado/editado por el usuario en el preview)
        // y acredita el total en la cta cte de la Persona del empleado, en una unica transaccion.
        // Recalcula Subtotal/TotalLiquidado server-side: nunca se confia en el total que mande el
        // cliente (CLAUDE.md §2.2).
        public int Confirmar(Entidades.LiquidacionSueldo liquidacion, Entidades.Persona persona, string detalleCtaCteFinal, Entidades.Usuario usuarioAccion)
        {
            if (liquidacion == null) throw new ArgumentNullException(nameof(liquidacion));
            if (persona == null || persona.idPersona <= 0)
                throw new InvalidOperationException("El empleado no tiene una persona vinculada válida.");

            var solapada = oLiquidacionD.ObtenerSolapada(liquidacion.IdEmpleado, liquidacion.PeriodoDesde, liquidacion.PeriodoHasta, 0);
            if (solapada != null)
                throw new InvalidOperationException(
                    $"El período se superpone con la liquidación #{solapada.Id} ({solapada.PeriodoDesde:dd/MM/yyyy} al {solapada.PeriodoHasta:dd/MM/yyyy}).");

            foreach (var linea in liquidacion.Detalle ?? new List<Entidades.LiquidacionSueldoDetalle>())
                linea.Subtotal = linea.Cantidad * linea.ValorUnitario;
            liquidacion.TotalLiquidado = (liquidacion.Detalle ?? new List<Entidades.LiquidacionSueldoDetalle>()).Sum(d => d.Subtotal);

            if (liquidacion.TotalLiquidado <= 0)
                throw new InvalidOperationException("El total a liquidar debe ser mayor a 0.");

            liquidacion.DetalleCtaCte = !string.IsNullOrWhiteSpace(detalleCtaCteFinal) ? detalleCtaCteFinal.Trim() : liquidacion.DetalleCtaCte;
            liquidacion.Estado = Entidades.LiquidacionSueldo.estadoLiquidacion.Confirmada;
            liquidacion.FechaLiquidacion = DateTime.Today;
            liquidacion.CreadoPor = usuarioAccion?.Id;

            var unitOfWork = oLiquidacionD.IniciarUnitOfWork();
            using (unitOfWork)
            {
                try
                {
                    int idLiquidacion = oLiquidacionD.Agregar(liquidacion, unitOfWork);

                    oCtaCteN.crearMovCtaCte(persona, DateTime.Now, Entidades.MovCtaCte.tablas.Liquidaciones, idLiquidacion, "",
                        liquidacion.DetalleCtaCte, Entidades.MovCtaCte.tipoMov.Credito, (float)liquidacion.TotalLiquidado,
                        usuarioAccion?.Sucursal, DateTime.Now, usuarioAccion, null, null, true, null, null, null, unitOfWork);

                    unitOfWork.Completar();
                    liquidacion.Id = idLiquidacion;
                    return idLiquidacion;
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al confirmar la liquidación: " + ex.Message, ex);
                }
            }
        }

        // Elimina (anula) una liquidacion ya confirmada, generando el asiento opuesto en la cta cte
        // del empleado (mismo mecanismo de reversa que usan Compras/Ventas). El periodo queda libre
        // para volver a liquidarse.
        public void Eliminar(int idLiquidacion, int idEmpresa, Entidades.Persona persona, Entidades.Usuario usuarioAccion)
        {
            var liquidacion = oLiquidacionD.ObtenerPorId(idLiquidacion, idEmpresa);
            if (liquidacion == null) throw new InvalidOperationException("La liquidación no existe.");
            if (liquidacion.Estado == Entidades.LiquidacionSueldo.estadoLiquidacion.Anulada)
                throw new InvalidOperationException("La liquidación ya fue eliminada.");

            var unitOfWork = oLiquidacionD.IniciarUnitOfWork();
            using (unitOfWork)
            {
                try
                {
                    oLiquidacionD.Anular(idLiquidacion, idEmpresa, unitOfWork);

                    // crearMovCtaCte=false: genera el asiento opuesto y no crea uno nuevo.
                    oCtaCteN.crearMovCtaCte(persona, DateTime.Now, Entidades.MovCtaCte.tablas.Liquidaciones, idLiquidacion, "",
                        liquidacion.DetalleCtaCte, Entidades.MovCtaCte.tipoMov.Credito, (float)liquidacion.TotalLiquidado,
                        usuarioAccion?.Sucursal, DateTime.Now, usuarioAccion, null, null, false, null, null, null, unitOfWork);

                    unitOfWork.Completar();
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al eliminar la liquidación: " + ex.Message, ex);
                }
            }
        }
    }
}
