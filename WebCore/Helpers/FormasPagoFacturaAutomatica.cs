// Parametro por empresa "formasPagoFacturaAutomatica": que formas de pago abren la factura electronica apenas se
// cobra en el POS (Ventas/POS). Guarda/lee el valor del parametro y arma la lista que usa el JS del POS.
// Sin valor cargado rige el comportamiento historico (todo lo electronico, no el efectivo). Detalle en
// docs/DECISIONS.md (2026-11-10) y docs/11-manual-de-uso/pantallas/Parametros.Index.md.
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebCore.Helpers
{
    public static class FormasPagoFacturaAutomatica
    {
        /// <summary>Valor guardado cuando la empresa eligio que ninguna forma de pago abra la factura sola.</summary>
        public const string Ninguna = "Ninguna";

        /// <summary>
        /// Formas de pago que se pueden elegir, con el nombre normalizado que usa el JS del POS
        /// (pos-forma-pago-precios.js normalizeFormaPago) y la etiqueta que se muestra. Cta. Cte. no esta:
        /// una venta a cuenta corriente sin cobro nunca abre la factura.
        /// </summary>
        public static readonly IReadOnlyList<(string Clave, string Etiqueta)> Opciones = new[]
        {
            ("Efectivo", "Efectivo"),
            ("Debito", "Débito"),
            ("Credito", "Crédito"),
            ("Qr", "QR"),
            ("Transferencia", "Transferencia")
        };

        /// <summary>Lo que regia antes de existir el parametro: todo lo electronico, no el efectivo.</summary>
        public static readonly IReadOnlyList<string> PorDefecto = new[] { "Debito", "Credito", "Qr", "Transferencia" };

        /// <summary>
        /// Interpreta el valor guardado. Null o vacio = por defecto; "Ninguna" = ninguna. Ignora nombres
        /// desconocidos y mayusculas. Devuelve las claves en el orden de <see cref="Opciones"/>.
        /// </summary>
        public static IReadOnlyList<string> Parse(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor)) return PorDefecto;

            var texto = valor.Trim();
            if (string.Equals(texto, Ninguna, StringComparison.OrdinalIgnoreCase)) return Array.Empty<string>();

            var pedidas = texto.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var elegidas = Opciones
                .Select(o => o.Clave)
                .Where(clave => pedidas.Any(p => string.Equals(p, clave, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            // Un valor con solo nombres desconocidos (ej. editado a mano en la base) se trata como no cargado,
            // para no apagar la factura automatica por un error de tipeo.
            return elegidas.Count == 0 ? PorDefecto : elegidas;
        }

        /// <summary>CSV en el orden de <see cref="Opciones"/>, o "Ninguna" si no hay ninguna elegida.</summary>
        public static string Serializar(IEnumerable<string>? seleccionadas)
        {
            var elegidas = (seleccionadas ?? Enumerable.Empty<string>()).ToList();
            var ordenadas = Opciones
                .Select(o => o.Clave)
                .Where(clave => elegidas.Any(e => string.Equals(e?.Trim(), clave, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            return ordenadas.Count == 0 ? Ninguna : string.Join(",", ordenadas);
        }
    }
}
