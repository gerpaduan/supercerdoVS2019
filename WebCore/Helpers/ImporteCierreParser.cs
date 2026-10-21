// Lectura ESTRICTA de los importes que el encargado carga al cerrar una caja (Caja Cierre e Importe Retirado).
// Problema que resuelve: CajasController.ParseFloat devuelve 0 ante un texto vacio o basura, asi que un campo mal
// cargado se guardaba como $0 sin avisar. Aca un valor que no es un numero finito se rechaza.
// El navegador manda el importe con punto decimal y 2 decimales (ej. "1234.50"); se tolera la coma por las dudas.
using System.Globalization;

namespace WebCore.Helpers
{
    public static class ImporteCierreParser
    {
        // true solo si el texto es un numero finito. Vacio, solo espacios, letras, "NaN" o "Infinity" => false.
        public static bool TryParse(string texto, out float importe)
        {
            importe = 0;
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            // Sin separador de miles: "1.234,56" no es el formato que manda el navegador y daria un valor ambiguo.
            string normalizado = texto.Trim().Replace(",", ".");
            if (!float.TryParse(normalizado, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out float valor))
                return false;

            // float.TryParse acepta "NaN"/"Infinity" con la cultura invariante: no son importes.
            if (!float.IsFinite(valor))
                return false;

            importe = valor;
            return true;
        }
    }
}
