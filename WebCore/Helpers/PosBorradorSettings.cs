// Configuracion de las ventas en curso del POS (borrador en servidor), de la advertencia "producto
// pesado sin agregar" y de la campana del admin, leida de App.config (appSettings, claves
// "PosBorrador:*"). Mismo mecanismo que PasskeySettings. Nada hardcodeado en el codigo de negocio:
// los umbrales viven aca como constantes nombradas y se pisan por config. Ver docs/DECISIONS.md
// "Ventas en curso: borrador en servidor y advertencias del POS".
using System.Globalization;
using WebCore.Infrastructure;

namespace WebCore.Helpers
{
    public static class PosBorradorSettings
    {
        // Valores por defecto (los que aplican si la clave no esta en App.config).
        private const int LatidoSegundosDefault = 20;
        private const int MinutosSinLatidoInterrumpidaDefault = 5;
        private const int DiasRetencionFinalizadasDefault = 7;
        private const int MaxLineasDefault = 300;
        private const int MaxPayloadKbDefault = 512;
        private const int SegundosProductoSinAgregarDefault = 7;
        private const int SegundosCantidadCeroConfirmacionDefault = 3;

        // En Postgres: prendido por defecto (la clave lo apaga con "false"). En SQL Server: APAGADO por
        // defecto (opt-in con PosBorrador:Habilitado=true) porque hay que haber corrido antes
        // Datos/DB-Procedures/20260923-Create_Borradores.sql en ese servidor; con la clave prendida y las
        // tablas sin crear, cada autoguardado fallaria.
        public static bool Habilitado =>
            LeerBool("PosBorrador:Habilitado", NegocioFactory.UsarPostgres);

        // Notificaciones al admin (campana, feed de Actividades con avisos, detalle) y "producto sin
        // agregar": solo existen en Postgres (tablas notificaciones/ventaproductosinagregar). En SQL Server
        // son etapa 2: los repositorios de Datos/ las tienen como no-op y estos consumidores se apagan.
        public static bool SoportaNotificaciones => NegocioFactory.UsarPostgres;

        // La campana del admin y todo lo que cuelga de notificaciones: ventas en curso o borradores
        // genericos habilitados Y motor con tabla notificaciones. Ver SoportaNotificaciones.
        public static bool CampanaAdminHabilitada =>
            SoportaNotificaciones && (Habilitado || BorradorGenericoSettings.Habilitado);

        // La advertencia de producto sin agregar se puede apagar sola, sin apagar el resguardo de ventas.
        public static bool AdvertenciaProductoSinAgregarHabilitada =>
            Habilitado && SoportaNotificaciones && LeerBool("PosBorrador:AdvertenciaProductoSinAgregarHabilitada", true);

        // Cada cuantos segundos el POS avisa al servidor que la venta sigue viva.
        public static int LatidoSegundos => LeerEntero("PosBorrador:LatidoSegundos", LatidoSegundosDefault, 5, 300);

        // Sin latido durante estos minutos, una venta ACTIVA se considera interrumpida (margen por el
        // throttling de timers de los navegadores en pestanas en segundo plano).
        public static int MinutosSinLatidoInterrumpida => LeerEntero("PosBorrador:MinutosSinLatidoInterrumpida", MinutosSinLatidoInterrumpidaDefault, 1, 240);

        // Dias que se conservan las ventas FINALIZADAS (solo sirven para idempotencia).
        public static int DiasRetencionFinalizadas => LeerEntero("PosBorrador:DiasRetencionFinalizadas", DiasRetencionFinalizadasDefault, 1, 365);

        public static int MaxLineas => LeerEntero("PosBorrador:MaxLineas", MaxLineasDefault, 10, 2000);
        public static int MaxPayloadKb => LeerEntero("PosBorrador:MaxPayloadKb", MaxPayloadKbDefault, 32, 4096);

        // Segundos de peso/cantidad estable en pantalla a partir de los cuales un producto que sale
        // sin agregarse se registra como advertencia.
        public static int SegundosProductoSinAgregar => LeerEntero("PosBorrador:SegundosProductoSinAgregar", SegundosProductoSinAgregarDefault, 2, 120);

        // Segundos que la cantidad debe quedar en 0/vacia para contarla como "salida" (evita disparar
        // por una tara o un salto momentaneo de la balanza).
        public static int SegundosCantidadCeroConfirmacion => LeerEntero("PosBorrador:SegundosCantidadCeroConfirmacion", SegundosCantidadCeroConfirmacionDefault, 1, 30);

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
        // (o el limite) en vez de romper el POS por una clave mal escrita.
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
