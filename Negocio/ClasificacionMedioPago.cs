using System;
using System.Globalization;
using System.Text;

namespace Negocio
{
    // Clasifica un pago a proveedor en "bancarizado" (deja rastro bancario) o "efectivo", para el control de lo pagado contra
    // lo facturado (Compras: titular propio y comprobantes del proveedor, 2026-11-09, ver docs/DECISIONS.md).
    // Es el UNICO lugar donde se decide: ni el SQL ni las vistas repiten esta regla.
    //  - Bancarizado: transferencia, debito, credito, QR y cheques (propios y de terceros).
    //  - Efectivo: efectivo y "Otro" (decision del usuario). Una forma desconocida cuenta como efectivo (no se asume rastro bancario).
    //  - Efectivo + Cheques: la parte en efectivo (pagos.efectivo) va a efectivo y el resto a bancarizado.
    // La base trae variantes historicas ("Eftvo+Cheque", "Trasferencia"): se normaliza el texto antes de comparar.
    // Codigo C# 7.3: Negocio tambien compila para net472.
    public static class ClasificacionMedioPago
    {
        public static void Clasificar(string formaPago, decimal importe, decimal efectivoDelPago,
            out decimal bancarizado, out decimal enEfectivo)
        {
            string forma = Normalizar(formaPago);

            bool tieneCheque = forma.Contains("cheque");
            bool tieneEfectivo = forma.Contains("efectivo") || forma.Contains("eftvo");

            if (tieneCheque && tieneEfectivo)
            {
                // Mixto: lo cobrado en efectivo no puede pasar del importe ni ser negativo.
                decimal parteEfectivo = Math.Max(0m, Math.Min(efectivoDelPago, importe));
                enEfectivo = parteEfectivo;
                bancarizado = importe - parteEfectivo;
                return;
            }

            if (tieneCheque || EsMedioElectronico(forma))
            {
                bancarizado = importe;
                enEfectivo = 0m;
                return;
            }

            // Efectivo, "Otro" y cualquier valor desconocido.
            bancarizado = 0m;
            enEfectivo = importe;
        }

        private static bool EsMedioElectronico(string forma)
        {
            // "trasfer" cubre la variante mal escrita que existe en los datos historicos.
            return forma.Contains("transfer") || forma.Contains("trasfer")
                || forma.Contains("debito") || forma.Contains("credito")
                || forma == "qr" || forma.Contains("tarjeta");
        }

        // Minusculas, sin tildes y sin espacios ni signos: "Eftvo+Cheque" -> "eftvocheque".
        private static string Normalizar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";
            string descompuesto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in descompuesto)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
