// Autorizacion de supervisor para operar sobre un borrador (venta en curso, o formulario de Compras/
// Stock/Movimientos/Embutidos) de OTRO usuario. Extraido de la logica que ya tenia
// VentasController.Borradores.cs (ResolverAutorizacionSupervisor/EsSupervisor) para que
// BorradoresGenericoController la reuse sin duplicar el mismo mecanismo de step-up + rate limit. No
// depende de MVC (no arma JsonResult): el controller que llama decide como responder segun el
// Resultado. Ver docs/DECISIONS.md "Borradores de Compras/Stock/Movimientos/Embutidos".
using WebCore.Models.DTO;

namespace WebCore.Helpers
{
    public static class AutorizacionSupervisorHelper
    {
        public class Resultado
        {
            public bool Autorizado { get; set; }

            // Nombre de quien autorizo con su clave (o del propio operador, si ya es supervisor). Null
            // si no hizo falta autorizacion (el borrador era del propio operador).
            public string NombreSupervisor { get; set; }

            public bool RequiereSupervisor { get; set; }
            public bool Bloqueado { get; set; }
            public string Mensaje { get; set; }
        }

        // Un operador es supervisor si es admin o tiene el permiso de cerrar caja (mismo criterio que
        // el step-up de Cierre de Caja y del POS).
        public static bool EsSupervisor(Entidades.Usuario usuario, Negocio.Usuario oUsuarioN)
        {
            if (usuario == null || !usuario.Activo) return false;
            return usuario.Admin || oUsuarioN.tienePermiso(usuario, Entidades.Permisos.Caja.CerrarCaja, DateTime.Today, -1);
        }

        // Resuelve quien autoriza a operar sobre un borrador de OTRO usuario:
        //  - si es del propio operador, no hace falta autorizacion;
        //  - si el operador ya es supervisor/admin, se autoriza solo;
        //  - si no, hace falta usuario+clave de un supervisor (con rate limit por sesion, sessionId).
        // sustantivoDueño describe lo que se esta protegiendo en el mensaje al usuario (ej. "una venta",
        // "un borrador de compra").
        public static Resultado Resolver(int idOperadorDueño, string nombreOperadorDueño, string sustantivoDueño,
            Entidades.Usuario operadorActual, Negocio.Usuario oUsuarioN, SupervisorAutorizacionDto supervisor, string sessionId)
        {
            if (idOperadorDueño == operadorActual.Id)
                return new Resultado { Autorizado = true };

            if (EsSupervisor(operadorActual, oUsuarioN))
                return new Resultado { Autorizado = true, NombreSupervisor = operadorActual.Nombre };

            string dueño = string.IsNullOrWhiteSpace(nombreOperadorDueño) ? "otro usuario" : nombreOperadorDueño;

            if (supervisor == null || string.IsNullOrWhiteSpace(supervisor.Usuario) || string.IsNullOrWhiteSpace(supervisor.Clave))
            {
                return new Resultado
                {
                    Autorizado = false,
                    RequiereSupervisor = true,
                    Mensaje = "Es " + sustantivoDueño + " de " + dueño + ": hace falta la autorización de un supervisor."
                };
            }

            if (PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
            {
                return new Resultado
                {
                    Autorizado = false,
                    RequiereSupervisor = true,
                    Bloqueado = true,
                    Mensaje = "Demasiados intentos. Esperá " + (int)Math.Ceiling(retryAfter.TotalSeconds) + " segundos."
                };
            }

            var validado = oUsuarioN.ValidarUsuarioWeb(supervisor.Usuario, supervisor.Clave);
            if (validado == null || !EsSupervisor(validado, oUsuarioN))
            {
                PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
                return new Resultado
                {
                    Autorizado = false,
                    RequiereSupervisor = true,
                    Mensaje = "Usuario o contraseña incorrectos, o sin permiso de supervisor."
                };
            }

            PosOperadorStepUpRateLimiter.Reset(sessionId);
            return new Resultado { Autorizado = true, NombreSupervisor = validado.Nombre };
        }
    }
}
