using System;
using System.Collections.Generic;

namespace WebCore.Models
{
    public class LiquidacionIndexVm
    {
        public List<EmpleadoResumenVm> Empleados { get; set; } = new List<EmpleadoResumenVm>();
        public int? IdEmpleadoSeleccionado { get; set; }
        public List<Entidades.LiquidacionSueldo> Items { get; set; } = new List<Entidades.LiquidacionSueldo>();
        public bool PuedeAdministrar { get; set; }
    }

    public class LiquidacionNuevaVm
    {
        public List<EmpleadoResumenVm> Empleados { get; set; } = new List<EmpleadoResumenVm>();
        public int IdEmpleado { get; set; }
        public DateTime Desde { get; set; } = DateTime.Today;
        public DateTime Hasta { get; set; } = DateTime.Today;
    }

    // Preview de una liquidacion antes de confirmar. Detalle incluye las lineas automaticas
    // calculadas por el sistema MAS unas filas manuales en blanco (Premio/Bono/Aguinaldo/otro) que
    // el usuario puede completar -- ver "Conceptos adicionales" en
    // docs/03-modulos/empleados-y-liquidacion-sueldos.md.
    public class LiquidacionPreviewVm
    {
        public int IdEmpleado { get; set; }
        public string EmpleadoNombre { get; set; } = "";
        public DateTime Desde { get; set; }
        public DateTime Hasta { get; set; }
        public List<LiquidacionDetalleItemVm> Detalle { get; set; } = new List<LiquidacionDetalleItemVm>();
        public decimal Total { get; set; }
        public string DetalleCtaCte { get; set; } = "";

        public bool TieneConflicto { get; set; }
        public string MensajeConflicto { get; set; } = "";

        // Cuantas filas manuales en blanco se agregan debajo del detalle automatico para que el
        // usuario cargue Premio/Bono/Aguinaldo/ajustes -- simplificacion sin JS dinamico.
        public const int CantidadLineasManuales = 4;
    }

    public class LiquidacionDetalleItemVm
    {
        public string Concepto { get; set; } = "";
        public string Origen { get; set; } = "Automatico";
        public decimal Cantidad { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public bool SinTarifaConfigurada { get; set; }
    }
}
