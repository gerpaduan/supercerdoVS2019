// Fechas y horas de un movimiento entre sucursales (2026-11-11, ver docs/DECISIONS.md "Movimientos: tiempo de envio,
// llegada y hora de carga por linea"): calcula la llegada estimada (hora de guardado + tiempo de envio) y resuelve la hora
// de carga de cada linea. Funciones puras (sin base ni reloj propio): reciben "ahora" para poder probarse.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class MovimientoTiempos
    {
        // Tope del tiempo de envio: un dia. Un valor mayor casi seguro es un error de tipeo (el parametro lo carga el admin).
        public const int MinutosEnvioMaximo = 1440;

        // Mismo formato que la hora de carga de las lineas de Stock (StockLineaVm.CreadoTexto).
        public const string FormatoCreado = "dd/MM/yyyy HH:mm";

        // Minutos de envio dentro de [0, MinutosEnvioMaximo]; un negativo o un valor fuera de rango no rompe el guardado.
        public static int LimitarMinutos(int minutos)
        {
            if (minutos < 0) return 0;
            if (minutos > MinutosEnvioMaximo) return MinutosEnvioMaximo;
            return minutos;
        }

        // Llegada estimada de un movimiento NUEVO: hora de guardado (la del servidor) mas el tiempo de envio.
        public static DateTime CalcularLlegada(DateTime ahora, int minutosEnvio)
        {
            return ahora.AddMinutes(LimitarMinutos(minutosEnvio));
        }

        // Hora de carga de una linea a partir del texto que manda la pantalla ("dd/MM/yyyy HH:mm"). Null si esta vacio o no se
        // puede leer. Una hora posterior a "ahora" (reloj de la PC del operador adelantado) se corrige a "ahora": una linea no
        // puede haberse cargado despues de guardarse.
        public static DateTime? LeerCreadoLinea(string? texto, DateTime ahora)
        {
            if (!DateTime.TryParseExact((texto ?? "").Trim(), FormatoCreado, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime creado))
                return null;

            return creado > ahora ? ahora : creado;
        }

        // Hora de carga que se guarda para una linea.
        // - Linea nueva (sin id): la que informa la pantalla, o "ahora" si no vino o no se pudo leer.
        // - Linea que ya existia (idCorteMovimiento > 0, edicion): conserva la suya; si no tenia (movimiento anterior al cambio)
        //   queda sin hora: NO se le pone la de la edicion, seria inventar un dato.
        public static DateTime? ResolverCreadoLinea(string? texto, int idCorteMovimiento, DateTime ahora)
        {
            DateTime? leido = LeerCreadoLinea(texto, ahora);
            if (leido.HasValue) return leido;
            return idCorteMovimiento > 0 ? null : ahora;
        }

        // Inicio del movimiento: la hora de su primera linea cargada (la menor). Null si ninguna tiene hora.
        public static DateTime? CalcularInicio(IEnumerable<DateTime?> horasLineas)
        {
            DateTime? inicio = null;
            foreach (DateTime? hora in horasLineas ?? Enumerable.Empty<DateTime?>())
            {
                if (hora.HasValue && (!inicio.HasValue || hora.Value < inicio.Value))
                    inicio = hora;
            }
            return inicio;
        }
    }
}
