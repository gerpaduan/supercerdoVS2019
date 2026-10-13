using System;

namespace Negocio
{
    // Regla de autorizacion (2026-10-13, ver docs/DECISIONS.md "Eliminar pagos y cobros de la propia caja abierta"):
    // quien tiene una caja abierta puede ELIMINAR (sin el permiso de edicion de pagos) los pagos y cobros que
    // cargo el mismo en esa caja. El contra-asiento y el egreso opuesto caen en esa misma caja abierta, asi que no
    // hay una caja cerrada que quede inconsistente. Es logica pura (sin base ni sesion) para poder testearla.
    public static class PagoEnCajaAbierta
    {
        // true si el pago/cobro pertenece a la caja abierta del operador:
        //  - la caja esta abierta (inicio informado, sin cierre) y es de la misma sucursal que el pago;
        //  - el pago se CREO a partir de la apertura de esa caja (se usa la fecha de creacion, no la fecha contable
        //    del pago, igual que la validacion de caja al finalizar una venta);
        //  - lo cargo el operador: si el pago tiene egreso de caja, el egreso debe estar a nombre del operador (en
        //    una cuenta de produccion compartida el creador del pago es la cuenta, no la persona); si no tiene
        //    egreso, el creador del pago debe ser el operador. Sin egreso y con cuenta compartida no se puede
        //    saber quien fue, y se deniega (queda el permiso de edicion de pagos).
        public static bool Evaluar(
            DateTime? pagoCreado, int pagoIdCreadoPor, int pagoIdSucursal,
            int? egresoIdCreadoPor,
            int idOperador,
            DateTime? cajaInicio, DateTime? cajaCierre, int cajaIdSucursal)
        {
            if (idOperador <= 0) return false;

            // Caja abierta de la misma sucursal.
            if (!cajaInicio.HasValue || cajaCierre.HasValue) return false;
            if (pagoIdSucursal <= 0 || pagoIdSucursal != cajaIdSucursal) return false;

            // Creado despues de la apertura (sin fecha de creacion no se puede afirmar nada).
            if (!pagoCreado.HasValue || pagoCreado.Value < cajaInicio.Value) return false;

            // Lo cargo el operador.
            if (egresoIdCreadoPor.HasValue) return egresoIdCreadoPor.Value == idOperador;
            return pagoIdCreadoPor == idOperador;
        }
    }
}
