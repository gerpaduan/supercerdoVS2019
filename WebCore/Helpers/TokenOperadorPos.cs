using Microsoft.AspNetCore.DataProtection;

namespace WebCore.Helpers
{
    // Token firmado "esta pestana de POS la opera este usuario" (2026-10-21, ver docs/DECISIONS.md "Bloqueo por
    // inactividad: recuperar al operador"). Con la cuenta de produccion el operador vive en la Session del servidor
    // (en memoria, vence a los 20 min sin pedidos y se pierde al reiniciar). Si vence con el POS bloqueado, el
    // desbloqueo no sabe de quien es la clave y habia que volver a elegir operador. La pagina recibe este token al
    // cargar (cuando el servidor SI conoce al operador) y lo devuelve al desbloquear: el servidor lo verifica (no se
    // puede fabricar ni cambiar el usuario o la pestana), exige la clave de ESE usuario y recien ahi lo vuelve a
    // registrar en la Session. Un companero no puede tomar la pestana con su propia clave.
    public static class TokenOperadorPos
    {
        // Purpose de Data Protection: aisla este token de cualquier otro dato protegido de la app.
        private const string Proposito = "WebCore.PosBloqueo.OperadorPestana.v1";

        // Igual a la duracion de la cookie de autenticacion (Program.cs, 12 h): mas alla de eso la sesion ya no existe.
        public static readonly TimeSpan Vigencia = TimeSpan.FromHours(12);

        private const char Separador = '|';

        // Devuelve el token del operador para esa pestana, o "" si faltan datos.
        public static string Crear(IDataProtectionProvider proveedor, string posInstanceId, int idOperador)
        {
            if (proveedor == null || string.IsNullOrWhiteSpace(posInstanceId) || idOperador <= 0)
                return "";

            return Protector(proveedor).Protect(posInstanceId + Separador + idOperador, Vigencia);
        }

        // Id del operador del token, o 0 si el token falta, esta vencido, fue alterado o es de otra pestana.
        public static int Leer(IDataProtectionProvider proveedor, string token, string posInstanceId)
        {
            if (proveedor == null || string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(posInstanceId))
                return 0;

            string contenido;
            try
            {
                contenido = Protector(proveedor).Unprotect(token);
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                // Token invalido (alterado, vencido o de otras claves de Data Protection): se trata como "sin token".
                return 0;
            }

            int corte = contenido.LastIndexOf(Separador);
            if (corte <= 0) return 0;

            if (!string.Equals(contenido.Substring(0, corte), posInstanceId, StringComparison.Ordinal))
                return 0;

            return int.TryParse(contenido.Substring(corte + 1), out int idOperador) && idOperador > 0 ? idOperador : 0;
        }

        private static ITimeLimitedDataProtector Protector(IDataProtectionProvider proveedor)
        {
            return proveedor.CreateProtector(Proposito).ToTimeLimitedDataProtector();
        }
    }
}
