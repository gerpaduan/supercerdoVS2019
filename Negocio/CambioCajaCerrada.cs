using System;

namespace Negocio
{
    // Regla pura (sin acceso a datos, testeable) para decidir si guardar un registro que cae en una caja YA CERRADA
    // requiere confirmacion del usuario: ventas, compras, pagos/cobros y egresos de caja (2026-10-06, ver
    // docs/DECISIONS.md "Cambios en cajas cerradas y reapertura").
    // Pide confirmacion cuando el cambio altera lo que el cierre guardo: monto, forma de pago/tipo, fecha o sucursal
    // (o es un alta / una eliminacion). Un cambio de solo observaciones, cliente, etc. NO la pide (igual se registra).
    public static class CambioCajaCerrada
    {
        // Tolerancia para comparar importes en float/double (centavos).
        public const double ToleranciaImporte = 0.005;

        public static bool HuboCambioQueAfectaCierre(
            double importeAnterior, double importeNuevo,
            string formaPagoAnterior, string formaPagoNueva,
            DateTime? fechaAnterior, DateTime? fechaNueva,
            int sucursalAnterior, int sucursalNueva)
        {
            if (Math.Abs(importeAnterior - importeNuevo) >= ToleranciaImporte) return true;
            if (!string.Equals((formaPagoAnterior ?? "").Trim(), (formaPagoNueva ?? "").Trim(), StringComparison.OrdinalIgnoreCase)) return true;
            if (sucursalAnterior != sucursalNueva) return true;
            return CambioFecha(fechaAnterior, fechaNueva);
        }

        // Compara a nivel de MINUTO: el input datetime-local no manda segundos y la base si, asi que guardar sin tocar la
        // fecha no debe contar como "cambio de fecha".
        public static bool CambioFecha(DateTime? fechaAnterior, DateTime? fechaNueva)
        {
            if (!fechaAnterior.HasValue && !fechaNueva.HasValue) return false;
            if (!fechaAnterior.HasValue || !fechaNueva.HasValue) return true;
            return Truncar(fechaAnterior.Value) != Truncar(fechaNueva.Value);
        }

        private static DateTime Truncar(DateTime fecha)
        {
            return new DateTime(fecha.Year, fecha.Month, fecha.Day, fecha.Hour, fecha.Minute, 0);
        }
    }
}
