using System;
using System.Collections.Generic;
using System.Linq;

namespace Negocio
{
    // Politica de clave segura y reglas del PIN de acceso rapido (ver docs/DECISIONS.md, "Login por
    // CUIT, clave rapida (PIN) y politica de clave"). Un unico validador compartido por todos los
    // puntos donde se fija una clave (cambio propio, reset por mail, alta/edicion por admin, panel
    // super-admin), asi la regla no se desincroniza entre pantallas.
    //
    // Clave segura (definida por el usuario): minimo 8 caracteres, al menos una letra, un numero y
    // un caracter especial. NO se exige mezcla de mayusculas/minusculas. Sin espacios, distinta del
    // usuario y fuera de una lista corta de claves comunes. Aplica a claves NUEVAS o cambiadas: las
    // existentes no se invalidan.
    public static class PoliticaClave
    {
        public const int LargoMinimoClave = 8;
        public const int LargoMaximoClave = 128;

        public const int LargoMinimoPin = 4;
        public const int LargoMaximoPin = 6;

        // Lista corta a proposito (no es un diccionario): solo los patrones que mas se prueban en
        // los ataques de fuerza bruta contra claves "con numero y simbolo" que igual son triviales.
        private static readonly HashSet<string> ClavesComunes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "password1!", "contraseña1!", "contrasena1!", "12345678!", "123456789!", "qwerty123!",
            "admin123!", "carnisys1!", "carnisys123!", "bienvenido1!", "bienvenido123!", "abcd1234!",
            "password123!", "passw0rd!", "p@ssw0rd", "p@ssword1", "welcome1!", "carniceria1!"
        };

        // Devuelve null si la clave es valida; si no, el mensaje (en espanol, para el usuario final)
        // de lo que falta. usuario: nombre de acceso del usuario, para impedir que la clave sea
        // igual a el (puede ser null/vacio si todavia no se conoce).
        public static string ValidarClave(string clave, string usuario = null)
        {
            clave = clave ?? "";

            if (clave.Length < LargoMinimoClave || clave.Length > LargoMaximoClave)
                return "La contraseña debe tener entre " + LargoMinimoClave + " y " + LargoMaximoClave + " caracteres.";

            if (clave.Any(char.IsWhiteSpace))
                return "La contraseña no puede contener espacios en blanco.";

            if (!clave.Any(char.IsLetter))
                return "La contraseña debe incluir al menos una letra.";

            if (!clave.Any(char.IsDigit))
                return "La contraseña debe incluir al menos un número.";

            // "caracter especial" = cualquier cosa que no sea letra ni digito (simbolos y puntuacion).
            if (!clave.Any(c => !char.IsLetterOrDigit(c)))
                return "La contraseña debe incluir al menos un carácter especial (por ejemplo ! # $ % & * @).";

            if (!string.IsNullOrWhiteSpace(usuario) && string.Equals(clave, usuario.Trim(), StringComparison.OrdinalIgnoreCase))
                return "La contraseña no puede ser igual al nombre de usuario.";

            if (ClavesComunes.Contains(clave))
                return "Esa contraseña es demasiado común. Elegí otra.";

            return null;
        }

        // Devuelve null si el PIN es valido; si no, el mensaje. idUsuario: se rechaza un PIN igual al
        // id del usuario (el caso "clave = 26" que motivo esta funcionalidad: adivinable por quien
        // vea la lista de usuarios, porque los ids son secuenciales).
        public static string ValidarPin(string pin, int idUsuario = 0)
        {
            pin = pin ?? "";

            if (pin.Length < LargoMinimoPin || pin.Length > LargoMaximoPin || !pin.All(c => c >= '0' && c <= '9'))
                return "El PIN debe tener entre " + LargoMinimoPin + " y " + LargoMaximoPin + " dígitos numéricos.";

            if (pin.All(c => c == pin[0]))
                return "El PIN no puede tener todos los dígitos iguales (por ejemplo 1111).";

            if (EsSecuencia(pin))
                return "El PIN no puede ser una secuencia (por ejemplo 1234 o 4321).";

            if (idUsuario > 0 && pin == idUsuario.ToString(System.Globalization.CultureInfo.InvariantCulture))
                return "El PIN no puede ser el número de tu usuario.";

            return null;
        }

        // true si el texto "parece un PIN" (solo digitos, 4 a 6): el login lo prueba primero como PIN
        // cuando el dispositivo es seguro. No dice si es VALIDO, solo si vale la pena intentarlo.
        public static bool TieneFormaDePin(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return false;
            if (texto.Length < LargoMinimoPin || texto.Length > LargoMaximoPin) return false;
            return texto.All(c => c >= '0' && c <= '9');
        }

        // Secuencia ascendente o descendente de a un digito (1234, 9876...).
        private static bool EsSecuencia(string pin)
        {
            bool asciende = true, desciende = true;
            for (int i = 1; i < pin.Length; i++)
            {
                if (pin[i] != pin[i - 1] + 1) asciende = false;
                if (pin[i] != pin[i - 1] - 1) desciende = false;
            }
            return asciende || desciende;
        }
    }
}
