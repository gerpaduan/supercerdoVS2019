using System;

namespace Entidades
{
    // Tarifa estandar/de referencia del comercio, NO ligada a ningun empleado. Sirve unicamente
    // como plantilla para proponer valores al dar de alta un empleado (boton "Copiar tarifa
    // estandar" en Empleados/Editar) -- nunca se usa automaticamente para calcular una liquidacion.
    // Mismas dimensiones que EmpleadoTarifa menos IdEmpleado. Ver docs/DECISIONS.md 2026-09-29.
    public class TarifaGeneral
    {
        public int Id { get; set; }
        public int IdEmpresa { get; set; }

        public Empleado.formaLiquidacion FormaLiquidacion { get; set; }
        public Turno? Turno { get; set; }
        public DiaSemana? DiaSemana { get; set; }

        public decimal Valor { get; set; }
        public DateTime VigenteDesde { get; set; }

        public DateTime? Creado { get; set; }
        public int? CreadoPor { get; set; }
    }
}
