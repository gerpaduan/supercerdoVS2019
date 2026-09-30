using System;
using System.Collections.Generic;

namespace Contratos
{
    // Tarifa estandar/plantilla del comercio (Entidades/TarifaGeneral.cs) -- nunca se usa
    // automaticamente para calcular una liquidacion, solo para prellenar la grilla de tarifas al
    // dar de alta un empleado. Solo Postgres (ver IEmpleadoRepository.cs).
    public interface ITarifaGeneralRepository
    {
        List<Entidades.TarifaGeneral> Listar(int idEmpresa);
        void Agregar(Entidades.TarifaGeneral tarifa);

        // Tarifas vigentes a hoy para una forma de liquidacion (una por Turno/DiaSemana), para
        // prellenar la grilla del boton "Copiar tarifa estandar".
        List<Entidades.TarifaGeneral> ListarVigentes(int idEmpresa, Entidades.Empleado.formaLiquidacion forma);
    }
}
