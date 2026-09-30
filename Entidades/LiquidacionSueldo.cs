using System;
using System.Collections.Generic;

namespace Entidades
{
    // Una corrida de liquidacion de sueldo para un empleado y un periodo. Es solo el devengamiento
    // del sueldo bruto del periodo -- se acredita en la cuenta corriente de la Persona del empleado
    // via Negocio.CuentaCorriente.crearMovCtaCte (tabla=Liquidaciones, tipoMov=Credito). El cobro
    // real del saldo se hace despues con el modulo Finanzas > Pagos existente. Ver
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class LiquidacionSueldo
    {
        public enum estadoLiquidacion
        {
            Confirmada,
            Anulada,
        }

        public int Id { get; set; }
        public int IdEmpresa { get; set; }
        public int IdEmpleado { get; set; }

        // Fecha+hora exactas (no solo fecha): necesario para cortar con precision el tiempo
        // trabajado de los empleados por Hora. Por defecto la UI propone las 00:00.
        public DateTime PeriodoDesde { get; set; }
        public DateTime PeriodoHasta { get; set; }

        public DateTime FechaLiquidacion { get; set; }
        public decimal TotalLiquidado { get; set; }

        // Texto propuesto por el sistema para el MovCtaCte.Detalle -- editable por el usuario antes
        // de confirmar.
        public string DetalleCtaCte { get; set; }

        public estadoLiquidacion Estado { get; set; } = estadoLiquidacion.Confirmada;

        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }

        public List<LiquidacionSueldoDetalle> Detalle { get; set; } = new List<LiquidacionSueldoDetalle>();
    }
}
