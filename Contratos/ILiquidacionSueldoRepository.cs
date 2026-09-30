using System;
using System.Collections.Generic;

namespace Contratos
{
    // Liquidaciones de sueldo (Entidades/LiquidacionSueldo.cs). Solo Postgres (ver
    // IEmpleadoRepository.cs). IniciarUnitOfWork: misma mecanica que
    // ICuentaCorrienteRepository.IniciarUnitOfWork -- Negocio.LiquidacionSueldo.Confirmar necesita
    // que el alta de liquidacionsueldo+detalle y el crearMovCtaCte (Negocio.CuentaCorriente) queden
    // en una unica transaccion.
    public interface ILiquidacionSueldoRepository
    {
        IUnitOfWork IniciarUnitOfWork();

        List<Entidades.LiquidacionSueldo> ListarPorEmpleado(int idEmpleado);
        Entidades.LiquidacionSueldo ObtenerPorId(int id, int idEmpresa);

        // Liquidacion Confirmada del mismo empleado cuyo rango se superpone con [desde, hasta],
        // excluyendo idExcluir (para re-validar al confirmar). Null si no hay solapamiento.
        Entidades.LiquidacionSueldo ObtenerSolapada(int idEmpleado, DateTime desde, DateTime hasta, int idExcluir);

        // Ultima liquidacion Confirmada del empleado (por PeriodoHasta descendente) -- para armar
        // la "jornada activa" en Mi Jornada (todo lo marcado desde ahi hasta hoy).
        Entidades.LiquidacionSueldo ObtenerUltimaConfirmada(int idEmpleado);

        // true si existe una liquidacion Confirmada del empleado cuyo rango cubre esa fecha --
        // bloquea la edicion retroactiva de un RegistroJornada de esa fecha.
        bool EstaCubiertaPorLiquidacionConfirmada(int idEmpleado, DateTime fecha);

        // Inserta liquidacionsueldo + liquidacionsueldodetalle (liquidacion.Detalle) en la misma
        // transaccion del unitOfWork recibido. Devuelve el id nuevo.
        int Agregar(Entidades.LiquidacionSueldo liquidacion, IUnitOfWork unitOfWork);

        // Marca Estado=Anulada -- el asiento opuesto en la cta cte lo genera
        // Negocio.CuentaCorriente.crearMovCtaCte(crearMovCtaCte: false, ...) por separado, en la
        // misma transaccion.
        void Anular(int idLiquidacion, int idEmpresa, IUnitOfWork unitOfWork);
    }
}
