// IP real del cliente para el login, la auditoria y los rate limiters (2026-10-02, Fase 4 de
// docs/DECISIONS.md "Login por CUIT, clave rapida (PIN) y politica de clave"). Delgado a proposito:
// la regla (confiar en X-Forwarded-For solo si la conexion viene de un proxy de confianza) vive en
// Negocio.ClientIpResolver, donde tiene tests. Los proxies de confianza adicionales a las redes
// privadas se configuran en Security:TrustedProxies (App.config / WebCore.dll.config del host), no
// se hardcodean: al cambiar de servidor, si el proxy nuevo esta en una IP publica distinta hay que
// cargarla ahi (ver docs/RUNBOOK.md).
namespace WebCore.Helpers
{
    public static class ClientIp
    {
        public const string ClaveConfigProxiesConfiables = "Security:TrustedProxies";

        public static string Obtener(HttpContext contexto)
        {
            string? configurado = System.Configuration.ConfigurationManager.AppSettings[ClaveConfigProxiesConfiables];

            return Negocio.ClientIpResolver.Resolve(
                contexto.Connection.RemoteIpAddress,
                contexto.Request.Headers["X-Forwarded-For"].ToString(),
                Negocio.ClientIpResolver.ParsearConfiguracion(configurado));
        }
    }
}
