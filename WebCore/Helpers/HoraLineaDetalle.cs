using System;
using System.Globalization;

namespace WebCore.Helpers
{
    // Hora de carga de cada linea en el detalle desplegable de Compras y Stock (tarjeta de celular, 2026-10-27, pedido
    // explicito): si la linea se cargo el mismo dia que el registro se muestra solo la hora; si fue otro dia, la fecha y la
    // hora (con el anio solo si tambien cambia) y se marca OtroDia para resaltarla. Ver docs/DECISIONS.md (2026-10-27).
    public static class HoraLineaDetalle
    {
        // Formato en que los controllers arman CreadoTexto (FormatearFechaHora de Compras/Stock).
        private const string FormatoCreado = "dd/MM/yyyy HH:mm";

        public readonly record struct Resultado(string Texto, bool OtroDia);

        // creadoTexto: fecha/hora de la linea como la arma el controller ("-" o vacio si no hay dato).
        // fechaRegistro: fecha del registro (la compra o el movimiento de stock); null = se muestra solo la hora.
        // Devuelve Texto vacio si la fecha de la linea no se puede leer (la vista no muestra nada).
        public static Resultado Formatear(string creadoTexto, DateTime? fechaRegistro)
        {
            if (!DateTime.TryParseExact((creadoTexto ?? "").Trim(), FormatoCreado, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out DateTime creado))
                return new Resultado("", false);

            if (!fechaRegistro.HasValue || creado.Date == fechaRegistro.Value.Date)
                return new Resultado(creado.ToString("HH:mm", CultureInfo.InvariantCulture), false);

            string formato = creado.Year == fechaRegistro.Value.Year ? "dd/MM HH:mm" : "dd/MM/yy HH:mm";
            return new Resultado(creado.ToString(formato, CultureInfo.InvariantCulture), true);
        }
    }
}
