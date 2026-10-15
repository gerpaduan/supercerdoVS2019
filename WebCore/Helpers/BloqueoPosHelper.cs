using System.Security.Claims;
using WebCore.Infrastructure;

namespace WebCore.Helpers
{
    // Bloqueo por inactividad del POS (2026-10-15, ver docs/DECISIONS.md "Bloqueo por inactividad"):
    // decide cuantos minutos de inactividad tolera el VENDEDOR ACTIVO del POS antes de bloquearse
    // (0 = nunca). El vendedor activo es el operador elegido en la pestana si la cuenta de la cookie
    // es de produccion, o el propio usuario logueado en cualquier otro caso. Lo usan
    // VentasController.POS y PuntosExpendioController.POS; la regla pura vive en Negocio.PoliticaInactividad.
    public static class BloqueoPosHelper
    {
        public static int MinutosPara(ClaimsPrincipal principalDeLaCookie, Entidades.Usuario vendedorActivo)
        {
            if (vendedorActivo == null)
                return 0;

            // El claim lo fija el login segun el dispositivo seguro ("No bloquear sesion").
            bool dispositivoSinBloqueo = principalDeLaCookie != null
                && principalDeLaCookie.HasClaim(SesionClaims.SinBloqueoInactividad, "true");

            // Se evalua EsUsuarioProduccion del vendedor (no de la cookie): el operador real de una
            // cuenta de produccion es un empleado normal y SI se bloquea.
            return Negocio.PoliticaInactividad.MinutosEfectivos(
                vendedorActivo.EsUsuarioProduccion, vendedorActivo.MinutosInactividad, dispositivoSinBloqueo);
        }
    }
}
