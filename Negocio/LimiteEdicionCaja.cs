using System;

namespace Negocio
{
    // Regla de fecha para modificar desde el POS (2026-11-02, ver docs/DECISIONS.md "POS: permisos por funcion"):
    // un operador puede modificar ventas y pagos recien desde el inicio de su caja abierta, y ademas no mas atras
    // de los "dias editar" de su permiso. Rige el limite mas reciente de los dos: con la caja abierta hace 3 dias
    // y un permiso de 4 dias el limite es la apertura (3 dias); con la caja abierta hace 3 dias y un permiso de 0
    // dias el limite es hoy. Es logica pura (sin base ni sesion) para poder testearla.
    public static class LimiteEdicionCaja
    {
        // Fecha minima editable, o null si no se puede editar nada.
        //  - inicioCaja: apertura de la caja abierta del operador (null = no tiene caja abierta -> no edita).
        //  - minimaPorDias: hoy menos los "dias editar" del permiso. null = el permiso no acota por dias (por
        //    ejemplo el permiso sin limite); en ese caso rige solo la apertura de la caja.
        public static DateTime? Calcular(DateTime? inicioCaja, DateTime? minimaPorDias)
        {
            if (!inicioCaja.HasValue) return null;
            if (!minimaPorDias.HasValue) return inicioCaja.Value;
            return inicioCaja.Value > minimaPorDias.Value ? inicioCaja.Value : minimaPorDias.Value;
        }

        // true si la fecha cae dentro de lo editable. Sin limite (sin caja) no se edita.
        public static bool PuedeEditar(DateTime fecha, DateTime? inicioCaja, DateTime? minimaPorDias)
        {
            var limite = Calcular(inicioCaja, minimaPorDias);
            return limite.HasValue && fecha >= limite.Value;
        }
    }
}
