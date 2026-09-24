// Configuracion de los borradores en servidor de Compras/Stock/Movimientos/Embutidos, leida de
// App.config (appSettings, claves "BorradorGenerico:*"). Mismo mecanismo que PosBorradorSettings.
// Clase separada (no se generaliza PosBorradorSettings): esa clase ya mezcla 3 cosas especificas de
// POS (ventas en curso, advertencia de producto sin agregar y sus umbrales); sumarle un cuarto bloque
// para modulos ajenos la convertiria en un god-object de configuracion. Nada hardcodeado en el codigo
// de negocio: los umbrales viven aca como constantes nombradas y se pisan por config. Ver
// docs/DECISIONS.md "Borradores de Compras/Stock/Movimientos/Embutidos".
using System.Globalization;
using WebCore.Infrastructure;

namespace WebCore.Helpers
{
    public static class BorradorGenericoSettings
    {
        // Valores por defecto (los que aplican si la clave no esta en App.config).
        private const int LatidoSegundosDefault = 20;
        private const int MinutosSinLatidoInterrumpidaDefault = 5;
        private const int DiasRetencionFinalizadasDefault = 7;
        private const int MaxLineasDefault = 300;
        private const int MaxPayloadKbDefault = 512;

        // En Postgres: prendido por defecto (la clave lo apaga con "false"). En SQL Server: APAGADO por
        // defecto (opt-in con BorradorGenerico:Habilitado=true) porque hay que haber corrido antes
        // Datos/DB-Procedures/20260923-Create_Borradores.sql en ese servidor. Cuando esta apagado,
        // Compras/Stock/Movimientos/Embutidos siguen con el borrador local (localStorage) y el respaldo
        // por captura de pantalla (CapturaRespaldo) de siempre; cuando esta prendido esos dos mecanismos
        // locales se desactivan (el servidor pasa a ser la unica fuente de recuperacion).
        public static bool Habilitado =>
            LeerBool("BorradorGenerico:Habilitado", NegocioFactory.UsarPostgres);

        // Cada cuantos segundos el formulario avisa al servidor que sigue vivo.
        public static int LatidoSegundos => LeerEntero("BorradorGenerico:LatidoSegundos", LatidoSegundosDefault, 5, 300);

        // Sin latido durante estos minutos, un borrador ACTIVO se considera interrumpido (margen por el
        // throttling de timers de los navegadores en pestanas en segundo plano).
        public static int MinutosSinLatidoInterrumpida => LeerEntero("BorradorGenerico:MinutosSinLatidoInterrumpida", MinutosSinLatidoInterrumpidaDefault, 1, 240);

        // Dias que se conservan los borradores FINALIZADA (solo sirven para idempotencia).
        public static int DiasRetencionFinalizadas => LeerEntero("BorradorGenerico:DiasRetencionFinalizadas", DiasRetencionFinalizadasDefault, 1, 365);

        public static int MaxLineas => LeerEntero("BorradorGenerico:MaxLineas", MaxLineasDefault, 10, 2000);
        public static int MaxPayloadKb => LeerEntero("BorradorGenerico:MaxPayloadKb", MaxPayloadKbDefault, 32, 4096);

        private static string Leer(string clave)
        {
            return System.Configuration.ConfigurationManager.AppSettings[clave] ?? "";
        }

        private static bool LeerBool(string clave, bool porDefecto)
        {
            string valor = Leer(clave).Trim();
            if (valor.Length == 0) return porDefecto;
            return string.Equals(valor, "true", StringComparison.OrdinalIgnoreCase);
        }

        // Lee un entero de config; si falta, es invalido o cae fuera de [min, max], usa el default
        // (o el limite) en vez de romper la pantalla por una clave mal escrita.
        private static int LeerEntero(string clave, int porDefecto, int min, int max)
        {
            if (!int.TryParse(Leer(clave).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int valor))
                return porDefecto;
            if (valor < min) return min;
            if (valor > max) return max;
            return valor;
        }
    }
}
