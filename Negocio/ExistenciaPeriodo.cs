// Regla del "Periodo" de Existencia por sucursales: la fecha "desde" (obligatoria) y la fecha "hasta" (opcional) entre las que se cuentan
// las ventas y el uso en elaborados. Lo comparten el filtro "Con movimiento" y las sugerencias de "Ingresar pedido".
// Funcion pura (sin base de datos ni reloj propio: recibe "ahora") para poder probarla. Los mensajes son para el usuario final.
// Detalle y porques: docs/DECISIONS.md (2026-10-20, "Periodo con fecha hasta").
using System;

namespace Negocio
{
    public static class ExistenciaPeriodo
    {
        // Tope hacia atras del "desde": evita consultas de historia absurdas.
        public const int MaxDiasHaciaAtras = 3650;

        public const string MensajeDesdeObligatorio = "Elegí la fecha desde la que se cuentan las ventas y el uso en elaborados.";
        public const string MensajeDesdeFuturo = "La fecha del período no puede ser futura.";
        public const string MensajeHastaFuturo = "La fecha hasta no puede ser futura.";
        public const string MensajeHastaAnterior = "La fecha hasta no puede ser anterior a la fecha desde.";

        // Valida el periodo (input externo). Devuelve el mensaje de error para el usuario o null si esta bien. En ese caso:
        //  - desdeFecha: el "desde" sin hora;
        //  - limiteSuperior: null si no hay "hasta" (se cuenta hasta ahora), o el ultimo instante del dia "hasta" (el dia entero
        //    entra: elegir 31/12 incluye las ventas de las 23:59 del 31/12). Es lo que se pasa como tope a la consulta de actividad.
        // Un periodo de un solo dia (hasta = desde) es valido. La hora que traiga cada fecha se ignora.
        public static string Validar(DateTime? desde, DateTime? hasta, DateTime ahora, out DateTime desdeFecha, out DateTime? limiteSuperior)
        {
            desdeFecha = DateTime.MinValue;
            limiteSuperior = null;

            if (!desde.HasValue)
                return MensajeDesdeObligatorio;

            desdeFecha = desde.Value.Date;
            if (desdeFecha > ahora)
                return MensajeDesdeFuturo;
            if (desdeFecha < ahora.Date.AddDays(-MaxDiasHaciaAtras))
                return "La fecha del período no puede ser anterior a " + MaxDiasHaciaAtras + " días.";

            if (!hasta.HasValue)
                return null;

            DateTime hastaFecha = hasta.Value.Date;
            if (hastaFecha > ahora.Date)
                return MensajeHastaFuturo;
            if (hastaFecha < desdeFecha)
                return MensajeHastaAnterior;

            // Ultimo instante del dia (la base guarda microsegundos: el redondeo de la ultima fraccion no cambia el dia).
            limiteSuperior = hastaFecha.AddDays(1).AddTicks(-1);
            return null;
        }
    }
}
