// Configuracion de la eliminacion de Movimientos (appSettings de WebCore.dll.config, clave "Movimientos:ReplicaEntreBases").
// Mientras dos bases (Servidor SM y San Lorenzo) se repliquen los movimientos entre si (idMovOrigen), eliminar uno NO borra su
// copia en la otra base y deja el stock mal: en ese caso Eliminar queda deshabilitado en pantalla Y en el servidor.
// Default sin la clave: con SQL Server se asume que HAY replica (bloqueado: un olvido no lo habilita por accidente); con
// Postgres no hay replica. Cuando se unifiquen las bases se pone "false" y no hace falta tocar codigo.
// Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
namespace WebCore.Helpers
{
    public static class EliminacionRegistroSettings
    {
        public const string ClaveReplicaMovimientos = "Movimientos:ReplicaEntreBases";

        public static bool ReplicaMovimientosEntreBases
        {
            get
            {
                string texto = (System.Configuration.ConfigurationManager.AppSettings[ClaveReplicaMovimientos] ?? "").Trim();
                if (bool.TryParse(texto, out bool configurado)) return configurado;
                return !WebCore.Infrastructure.NegocioFactory.UsarPostgres;
            }
        }

        public const string MensajeReplicaMovimientos =
            "Los movimientos de esta base se replican con otra sucursal y la copia no se elimina sola: por ahora no se pueden eliminar desde acá.";
    }
}
