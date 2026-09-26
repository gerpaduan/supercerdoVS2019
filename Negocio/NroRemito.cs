using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Negocio
{
    // Numero de remito de los expendios del sector REMITOS y su leyenda en las observaciones
    // de la venta. Ver docs/DECISIONS.md (2026-09-26).
    //
    // Formato PPPP-NNNNNNNN: prefijo = id de la sucursal a 4 digitos (la empresa puede no tener
    // punto de venta AFIP, y no hay otro codigo de sucursal) y correlativo de 8 digitos, propio de
    // cada sucursal. Se sugiere en el POS y el usuario lo puede editar a mano: lo unico que se
    // exige es que sea unico por sucursal (indice unico en la base).
    public static class NroRemito
    {
        public const string PrefijoLeyendaObservaciones = "REMITOS ASOCIADOS: ";
        public const string SeparadorRemitos = " / ";

        private const int DigitosPrefijo = 4;
        private const int DigitosCorrelativo = 8;

        // Largo maximo aceptado al editarlo a mano (la columna es text; esto es cordura de UI).
        public const int LargoMaximo = 30;

        public static string Prefijo(int idSucursal)
        {
            return idSucursal.ToString("D" + DigitosPrefijo, CultureInfo.InvariantCulture);
        }

        public static string Formatear(int idSucursal, long correlativo)
        {
            return Prefijo(idSucursal) + "-" + correlativo.ToString("D" + DigitosCorrelativo, CultureInfo.InvariantCulture);
        }

        // Sugerencia: el siguiente al ultimo correlativo usado en la sucursal.
        public static string Siguiente(int idSucursal, long ultimoCorrelativo)
        {
            return Formatear(idSucursal, Math.Max(0, ultimoCorrelativo) + 1);
        }

        // Recorta y compacta espacios; null -> "". No cambia mayusculas ni el formato: el usuario
        // puede cargar el numero del remito de papel tal cual.
        public static string Normalizar(string nroRemito)
        {
            return Regex.Replace((nroRemito ?? "").Trim(), @"\s+", " ");
        }

        // Correlativo de un numero con el formato de la sucursal (PPPP-NNNNNNNN). Devuelve false
        // si tiene otro formato o pertenece a otra sucursal: esos no cuentan para la sugerencia.
        public static bool TryLeerCorrelativo(string nroRemito, int idSucursal, out long correlativo)
        {
            correlativo = 0;
            string texto = Normalizar(nroRemito);
            string prefijo = Prefijo(idSucursal) + "-";

            if (!texto.StartsWith(prefijo, StringComparison.Ordinal))
                return false;

            string resto = texto.Substring(prefijo.Length);
            return resto.Length > 0
                   && resto.All(c => c >= '0' && c <= '9')
                   && long.TryParse(resto, NumberStyles.None, CultureInfo.InvariantCulture, out correlativo);
        }

        // "REMITOS ASOCIADOS: a / b / c" (barra de por medio si hay mas de uno), o "" si no hay
        // ninguno. Sin repetidos, en el orden recibido.
        public static string ComponerLeyenda(IEnumerable<string> nrosRemito)
        {
            var nros = (nrosRemito ?? Enumerable.Empty<string>())
                .Select(Normalizar)
                .Where(n => n.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return nros.Count == 0 ? "" : PrefijoLeyendaObservaciones + string.Join(SeparadorRemitos, nros);
        }

        // Observaciones de la venta con la leyenda de remitos: conserva lo que escribio el
        // usuario y deja UNA sola leyenda como ultima linea. Si la venta ya tenia una (se esta
        // modificando), se suman los remitos nuevos a los que ya figuraban -- un remito asociado
        // a una venta no se desasocia al modificarla, y guardar varias veces no duplica nada.
        // Sin remitos nuevos devuelve las observaciones tal cual.
        public static string AplicarAObservaciones(string observaciones, IEnumerable<string> nrosRemito)
        {
            string original = observaciones ?? "";
            var nuevos = (nrosRemito ?? Enumerable.Empty<string>()).Select(Normalizar).Where(n => n.Length > 0).ToList();
            if (nuevos.Count == 0)
                return original;

            string marca = PrefijoLeyendaObservaciones.Trim();
            var lineas = original.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n').ToList();

            // Remitos que ya figuraban en una leyenda anterior (van primero, en su orden).
            var existentes = lineas
                .Where(l => l.TrimStart().StartsWith(marca, StringComparison.OrdinalIgnoreCase))
                .SelectMany(l => l.TrimStart().Substring(marca.Length).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(Normalizar)
                .Where(n => n.Length > 0);

            string leyenda = ComponerLeyenda(existentes.Concat(nuevos));

            string sinLeyenda = string.Join("\n", lineas
                .Where(l => !l.TrimStart().StartsWith(marca, StringComparison.OrdinalIgnoreCase))).TrimEnd();

            return sinLeyenda.Length == 0 ? leyenda : sinLeyenda + "\n" + leyenda;
        }
    }
}
