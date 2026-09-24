// Configuracion de AFIP/ARCA leida de App.config (appSettings), mismo mecanismo que PosBorradorSettings:
//   Afip:Produccion:WsaaUrl / WsfeUrl / PadronUrl y Afip:Homologacion:WsaaUrl / WsfeUrl / PadronUrl
//     -> URLs de los web services por entorno. Si la clave falta, vale el endpoint oficial de AFIP.
//   Afip:DiasAviso -> umbrales (en dias, separados por coma) para avisar que el certificado esta por
//     vencer. Default 60,30,15. El mayor define desde cuando se considera "por vencer".
// El entorno (homologacion/produccion) de cada empresa NO se configura aca: es Empresa.Entorno_HOMO_PROD.
using System.Globalization;
using AFIP;

namespace WebCore.Helpers
{
    public static class AfipSettings
    {
        private static readonly int[] DiasAvisoDefault = { 60, 30, 15 };

        // Config de URLs por entorno, con los oficiales como base (AfipConfig.PorDefecto) y pisados por App.config.
        public static AfipConfig ConfigBase()
        {
            var config = AfipConfig.PorDefecto();
            Pisar(config.Produccion, "Afip:Produccion");
            Pisar(config.Homologacion, "Afip:Homologacion");
            return config;
        }

        // Umbrales de aviso de vencimiento, ordenados de mayor a menor y sin repetidos.
        public static int[] DiasAviso
        {
            get
            {
                string texto = Leer("Afip:DiasAviso").Trim();
                if (texto.Length == 0) return DiasAvisoDefault;

                var dias = new System.Collections.Generic.SortedSet<int>(System.Collections.Generic.Comparer<int>.Create((a, b) => b.CompareTo(a)));
                foreach (string parte in texto.Split(','))
                {
                    if (int.TryParse(parte.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int d) && d > 0 && d <= 3650)
                        dias.Add(d);
                }
                if (dias.Count == 0) return DiasAvisoDefault;
                var resultado = new int[dias.Count];
                dias.CopyTo(resultado);
                return resultado;
            }
        }

        // ---- Padron compartido de la plataforma (ver WebCore.Services.PadronAfipGateway) ----

        // Interruptor general: false = nadie usa el certificado de la plataforma, cada empresa consulta con el suyo.
        public static bool PadronUsarPlataforma => LeerBool("Afip:Padron:UsarPlataforma", true);

        // Tope de consultas de padron por empresa por hora (cada alta de cliente hace una; muy por encima del uso real).
        public static int PadronMaxPorHoraEmpresa => LeerEntero("Afip:Padron:MaxPorHoraEmpresa", 30, 1, 1000);

        // Horas que se recuerda un resultado exitoso por empresa y CUIT consultado (evita repetir la consulta a ARCA).
        public static int PadronCacheHoras => LeerEntero("Afip:Padron:CacheHoras", 24, 1, 168);

        // Fallos tecnicos seguidos con el certificado de la plataforma antes de cortarlo un rato.
        public static int PadronFallosParaCortar => LeerEntero("Afip:Padron:FallosParaCortar", 3, 1, 20);

        // Minutos que dura ese corte (mientras tanto se usa el certificado de la empresa, si lo tiene).
        public static int PadronMinutosCorte => LeerEntero("Afip:Padron:MinutosCorte", 10, 1, 1440);

        private static bool LeerBool(string clave, bool porDefecto)
        {
            string valor = Leer(clave).Trim();
            if (valor.Length == 0) return porDefecto;
            return string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Lee un entero de config; si falta, es invalido o cae fuera de [min, max] usa el default (o el limite).
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            if (!int.TryParse(Leer(clave).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }

        private static void Pisar(AfipEndpoints destino, string prefijo)
        {
            destino.WsaaUrl = LeerUrl(prefijo + ":WsaaUrl", destino.WsaaUrl);
            destino.WsfeUrl = LeerUrl(prefijo + ":WsfeUrl", destino.WsfeUrl);
            destino.PadronUrl = LeerUrl(prefijo + ":PadronUrl", destino.PadronUrl);
        }

        // Solo acepta URLs https absolutas: un valor mal escrito cae al default en vez de romper la facturacion.
        private static string LeerUrl(string clave, string porDefecto)
        {
            string valor = Leer(clave).Trim();
            if (valor.Length == 0) return porDefecto;
            if (System.Uri.TryCreate(valor, System.UriKind.Absolute, out var uri) && uri.Scheme == System.Uri.UriSchemeHttps)
                return valor;
            return porDefecto;
        }

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }
    }
}
