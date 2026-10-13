using System;
using System.Linq;

namespace Entidades
{
    // Normalizacion y validacion de CUIT/CUIL (11 digitos + digito verificador modulo 11). Un solo lugar: antes habia
    // 4 copias privadas de NormalizarCuit sin validador de digito verificador. Lo usan los titulares fiscales
    // (titulares fiscales, 2026-10-09, ver docs/DECISIONS.md).
    public static class CuitHelper
    {
        private static readonly int[] Pesos = { 5, 4, 3, 2, 7, 6, 5, 4, 3, 2 };

        // Solo los digitos del texto ("20-30621078-6" -> "20306210786"); null -> "".
        public static string Normalizar(string cuit)
        {
            if (string.IsNullOrWhiteSpace(cuit)) return "";
            return new string(cuit.Where(char.IsDigit).ToArray());
        }

        // true si, normalizado, son exactamente 11 digitos y el digito verificador coincide.
        public static bool EsValido(string cuit)
        {
            string digitos = Normalizar(cuit);
            if (digitos.Length != 11) return false;

            int suma = 0;
            for (int i = 0; i < 10; i++)
                suma += (digitos[i] - '0') * Pesos[i];

            int resto = suma % 11;
            int esperado = resto == 0 ? 0 : 11 - resto;
            // resto == 1 daria 10: ARCA nunca emite un CUIT asi (cambia el prefijo y recalcula), no es valido.
            if (esperado == 10) return false;

            return esperado == (digitos[10] - '0');
        }

        // CUIT numerico o 0 si no es valido.
        public static long ParsearValido(string cuit)
        {
            if (!EsValido(cuit)) return 0;
            return long.Parse(Normalizar(cuit));
        }
    }
}
