using System;
using System.Globalization;
using System.Linq;

namespace Carnisys.Balanza.Agent
{
    internal static class PesoHelper
    {
        public static string LimpiarRaw(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return string.Empty;
            }

            var partes = raw
                .Replace("\0", string.Empty)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            return partes.Length > 0 ? partes[partes.Length - 1] : raw.Trim();
        }

        public static string FormatearPesoTexto(decimal peso)
        {
            return peso.ToString("0.000", CultureInfo.InvariantCulture);
        }

        public static bool TryParsePesoTexto(string texto, out decimal peso)
        {
            peso = 0m;
            if (string.IsNullOrWhiteSpace(texto))
            {
                return false;
            }

            string normalized = new string(texto
                .Trim()
                .Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-')
                .ToArray())
                .Replace(',', '.');

            return decimal.TryParse(
                normalized,
                NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out peso);
        }

        public static LecturaPeso CrearLecturaOk(BalanzaConfig config, string raw, decimal peso, bool inestable)
        {
            string pesoTexto = FormatearPesoTexto(peso);
            return new LecturaPeso
            {
                Ok = true,
                Conectada = true,
                Peso = peso,
                PesoTexto = pesoTexto,
                PesoDisplay = inestable ? pesoTexto + " i" : pesoTexto,
                Unidad = "kg",
                Estable = !inestable,
                Inestable = inestable,
                Negativo = peso < 0,
                Raw = raw ?? string.Empty,
                Marca = config != null ? config.Marca ?? string.Empty : string.Empty,
                Modelo = config != null ? config.Modelo ?? string.Empty : string.Empty,
                Puerto = config != null ? config.Puerto ?? string.Empty : string.Empty,
                FechaHora = DateTime.Now,
                Error = null
            };
        }
    }
}
