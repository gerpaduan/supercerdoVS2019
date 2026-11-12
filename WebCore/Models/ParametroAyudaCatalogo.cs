// Textos de ayuda de la pantalla Parametros: titulo claro, explicacion, grupo y unidad de cada
// parametro de empresa. Es solo presentacion: lo que se guarda sigue siendo el nombre tecnico y
// su valor. Detalle de la pantalla en docs/11-manual-de-uso/pantallas/Parametros.Index.md.
// Al sumar un parametro visible en la web, agregar su entrada aca (y en el manual de uso).
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCore.Models
{
    /// <summary>Texto de ayuda de un parametro: como se muestra y que hace, sin tocar su valor.</summary>
    /// <param name="Titulo">Nombre legible (el tecnico se muestra aparte, en chico).</param>
    /// <param name="Explicacion">Que cambia en el sistema segun el valor.</param>
    /// <param name="Grupo">Seccion de la pantalla donde se muestra (ver ParametroAyudaCatalogo.Grupo*).</param>
    /// <param name="Unidad">Unidad del valor ("$", "dias") o null si no corresponde.</param>
    /// <param name="Nota">Aclaracion corta bajo el nombre tecnico (valor por defecto, ejemplo) o null.</param>
    /// <param name="Editor">Editor especial del valor (ver ParametroAyudaCatalogo.Editor*) o null para el comun segun el tipo.</param>
    /// <param name="EnteroMaxDigitos">Si no es null, el valor es un numero entero positivo de hasta esa cantidad de digitos (o vacio).</param>
    public sealed record ParametroAyuda(string Titulo, string Explicacion, string Grupo, string? Unidad = null, string? Nota = null, string? Editor = null, int? EnteroMaxDigitos = null);

    public static class ParametroAyudaCatalogo
    {
        public const string GrupoPuntoDeVenta = "Punto de venta (POS)";
        public const string GrupoTickets = "Tickets";
        public const string GrupoFechas = "Fechas y listados";
        public const string GrupoOtros = "Otros";

        // Largo maximo de un codigo de producto (EAN-13): los codigos de corte.codigo se guardan como bigint.
        public const int EnteroMaxDigitos13 = 13;

        // Editor del valor: una casilla por forma de pago (parametro formasPagoFacturaAutomatica).
        public const string EditorFormasPago = "formasPago";

        // Orden en que se muestran los grupos en la pantalla (los que no tengan filas no se dibujan).
        public static readonly IReadOnlyList<string> OrdenGrupos = new[]
        {
            GrupoPuntoDeVenta, GrupoTickets, GrupoFechas, GrupoOtros
        };

        private static readonly Dictionary<string, ParametroAyuda> Entradas =
            new Dictionary<string, ParametroAyuda>(StringComparer.OrdinalIgnoreCase)
            {
                ["saldoMinimoInformar"] = new ParametroAyuda(
                    "Saldo mínimo a informar",
                    "Si el saldo de cuenta corriente del cliente es igual o menor a este monto (en deuda o a favor), el POS no lo muestra ni ofrece sumarlo al ticket. Con 0 se informa cualquier saldo distinto de cero.",
                    GrupoPuntoDeVenta, "$", "Si queda vacío se usa 100."),

                // El usuario confirmo (2026-11-10) que es un entero de hasta 13 digitos porque se compara con el codigo
                // que se ingresa en el POS (corte.codigo es bigint; Negocio.Corte.ObtenerProductoGenerico lo lee con GetLong).
                // TODO(claude): confirmar el formato exacto que tipea el vendedor en el POS (cantXcodigo / importe con punto /
                // sufijo G<n>); el texto se apoya en Negocio/BarcodeInterpreter.InterpretarCodigoGenerico.
                ["codProdGenerico"] = new ParametroAyuda(
                    "Código del producto de precio libre",
                    "Código del producto genérico, el mismo que se compara con lo que se ingresa en el POS y en el punto de expendio para vender un importe suelto: el vendedor escribe el precio y se carga este producto con ese precio. Con el sufijo G seguido de un número se usa el producto cuyo código es este más ese número.",
                    GrupoPuntoDeVenta, null, "Número entero de hasta 13 dígitos. Si queda vacío, no hay producto de precio libre.",
                    null, EnteroMaxDigitos13),

                ["formasPagoFacturaAutomatica"] = new ParametroAyuda(
                    "Formas de pago que abren la factura al cobrar",
                    "Al cobrar en el POS con una de las formas tildadas, se abre la factura electrónica de la venta. Con las demás se pasa directo a «Venta completada» y la factura se puede hacer después. Cuenta corriente sin cobro nunca abre la factura; en un pago mixto se abre si alguna de las dos partes está tildada. Requiere certificado de AFIP.",
                    GrupoPuntoDeVenta, null, "Sin cargar: Débito, Crédito, QR y Transferencia.", EditorFormasPago),

                ["ocultarEncabezadoTicketNoFiscal"] = new ParametroAyuda(
                    "Ocultar datos de la empresa en el ticket no fiscal",
                    "Con Sí, el ticket de venta X (no fiscal) del POS no imprime el nombre de la empresa ni los slogans. El ticket facturado, los PDF y los demás tickets no cambian.",
                    GrupoTickets, null, "Sin cargar equivale a No."),

                ["diasLimitFechaDesde"] = new ParametroAyuda(
                    "Días hacia atrás por defecto",
                    "Desde qué fecha arrancan los listados de Stock, Elaborados y Movimientos, y hasta cuántos días atrás se puede llevar la fecha de una venta en el POS.",
                    GrupoFechas, "días", "Ejemplo: 7 = la última semana."),

                // 2026-11-11: lo lee MovimientosController (CrearModeloNuevo y Guardar) via Helpers/MovimientoTiempos.
                ["minutosEnvioMovimiento"] = new ParametroAyuda(
                    "Tiempo de envío de un movimiento",
                    "Minutos que tarda un traslado entre sucursales. Al guardar un movimiento nuevo, su fecha queda en la hora actual más estos minutos (la llegada estimada) y recién a esa hora cuenta en el stock de las dos sucursales. En cada movimiento se puede ajustar antes de guardar. Con 0 la fecha es la hora en que se guarda.",
                    GrupoFechas, "min", "Si queda vacío se usa 0. Máximo 1440 (un día)."),
            };

        /// <summary>
        /// Valida un entero positivo de hasta <paramref name="maxDigitos"/> digitos (solo 0-9). Vacio es valido (el
        /// sistema usa el default del parametro). Devuelve "" si esta bien (y el valor normalizado, sin ceros a la
        /// izquierda) o el mensaje de error.
        /// </summary>
        public static string ValidarEntero(string? valor, int maxDigitos, out string normalizado)
        {
            normalizado = (valor ?? "").Trim();
            if (normalizado.Length == 0) return "";

            string mensaje = "Ingrese un número entero mayor a cero de hasta " + maxDigitos + " dígitos, o deje el campo vacío.";

            // Solo digitos ASCII (char.IsDigit aceptaria digitos de otros alfabetos) y sin pasarse del largo.
            if (normalizado.Length > maxDigitos || !normalizado.All(c => c >= '0' && c <= '9')) return mensaje;
            if (!long.TryParse(normalizado, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out long numero) || numero <= 0)
                return mensaje;

            normalizado = numero.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return "";
        }

        /// <summary>
        /// Ayuda del parametro. Si el parametro no esta en el catalogo (alta nueva en la base global),
        /// se muestra igual con su nombre tecnico y la descripcion de la base, en el grupo "Otros".
        /// </summary>
        public static ParametroAyuda Obtener(string? nombre, string? descripcionBase)
        {
            if (!string.IsNullOrWhiteSpace(nombre) && Entradas.TryGetValue(nombre.Trim(), out var ayuda))
                return ayuda;

            string titulo = string.IsNullOrWhiteSpace(nombre) ? "(sin nombre)" : nombre.Trim();
            return new ParametroAyuda(titulo, (descripcionBase ?? "").Trim(), GrupoOtros);
        }
    }
}
