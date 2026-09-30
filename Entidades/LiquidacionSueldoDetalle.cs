namespace Entidades
{
    // Linea de desglose de una LiquidacionSueldo (transparencia/auditoria). Automatico = calculado
    // por el sistema a partir de RegistroJornada, dias de vacaciones o el fijo del periodo. Manual =
    // agregado por el usuario en el preview (Premio, Bono, Aguinaldo, ajuste). Ver
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class LiquidacionSueldoDetalle
    {
        public enum origenDetalle
        {
            Automatico,
            Manual,
        }

        public int Id { get; set; }
        public int IdLiquidacionSueldo { get; set; }

        public string Concepto { get; set; }
        public origenDetalle Origen { get; set; }

        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Subtotal { get; set; }

        // true cuando el sistema no encontro una EmpleadoTarifa para esta combinacion turno/dia:
        // ValorUnitario/Subtotal quedan en 0 y la linea se muestra resaltada para que el usuario la
        // complete a mano antes de aprobar (nunca se inventa un valor -- ver CLAUDE.md §2.7).
        public bool SinTarifaConfigurada { get; set; }
    }
}
