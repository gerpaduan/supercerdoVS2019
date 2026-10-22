// Revalidacion de usuario y contraseña para ELIMINAR registros (Compras, Stock, Movimientos), aunque la persona ya este
// logueada. Ver docs/DECISIONS.md "Eliminar compras, stock y movimientos".
// Reglas: solo usuario+contraseña (el PIN y la huella no valen); sin ventana de "ya me valide hace un rato" (se pide en cada
// eliminacion); con una sesion normal el usuario validado tiene que ser el MISMO de la sesion; con la cuenta compartida de
// produccion vale cualquier usuario activo de la empresa (la persona real). 5 intentos y bloqueo (el mismo limitador por
// sesion que usan el POS, Compras y Cierre de Caja). La clave nunca se loguea ni se guarda.
using Microsoft.AspNetCore.Http;

namespace WebCore.Helpers
{
    public static class ReautenticacionEliminacion
    {
        public const string MensajeCredencialesIncorrectas = "Usuario o contraseña incorrectos.";

        public sealed class Resultado
        {
            public bool Ok { get; set; }

            // Persona que acaba de validar su clave (con sus Permisos cargados, listos para Usuario.tienePermiso).
            public Entidades.Usuario? Usuario { get; set; }

            public string Mensaje { get; set; } = "";
            public bool Bloqueado { get; set; }
            public int SegundosBloqueo { get; set; }

            public static Resultado Error(string mensaje) => new Resultado { Ok = false, Mensaje = mensaje };
        }

        public static Resultado Validar(ISession session, Entidades.Usuario usuarioSesion, int idUsuario, string clave, Negocio.Usuario oUsuarioN)
        {
            if (usuarioSesion == null) return Resultado.Error("Sesión no válida.");

            string sessionId = ObtenerSessionIdEstable(session);
            if (PosOperadorStepUpRateLimiter.IsBlocked(sessionId, out var retryAfter))
                return new Resultado { Ok = false, Bloqueado = true, SegundosBloqueo = (int)System.Math.Ceiling(retryAfter.TotalSeconds), Mensaje = "Demasiados intentos." };

            if (idUsuario <= 0 || string.IsNullOrEmpty(clave))
                return Fallo(sessionId, MensajeCredencialesIncorrectas);

            // Sesion normal: solo se puede confirmar con el propio usuario. Si el pedido trae otro id es una manipulacion.
            if (!usuarioSesion.EsUsuarioProduccion && idUsuario != usuarioSesion.Id)
                return Fallo(sessionId, "Para eliminar tenés que confirmar tu propio usuario y contraseña.");

            var candidato = oUsuarioN.getUsuarioById(idUsuario);
            if (candidato == null || !candidato.Activo || candidato.IdEmpresa != usuarioSesion.IdEmpresa)
                return Fallo(sessionId, MensajeCredencialesIncorrectas);

            var validado = oUsuarioN.ValidarUsuarioWeb(candidato.User, clave);
            if (validado == null || !validado.Activo || validado.Id != candidato.Id)
                return Fallo(sessionId, MensajeCredencialesIncorrectas);

            PosOperadorStepUpRateLimiter.Reset(sessionId);
            return new Resultado { Ok = true, Usuario = validado };
        }

        private static Resultado Fallo(string sessionId, string mensajeBase)
        {
            var fallo = PosOperadorStepUpRateLimiter.RegisterFailure(sessionId);
            if (fallo.SeBloqueo)
                return new Resultado { Ok = false, Bloqueado = true, SegundosBloqueo = fallo.SegundosBloqueo, Mensaje = fallo.MensajeBloqueo };

            return Resultado.Error(fallo.ConAviso(mensajeBase));
        }

        // ASP.NET Core no manda la cookie de sesion hasta el primer write: sin esto el limitador (por sesion) nunca acumula
        // (mismo motivo que ComprasController.ObtenerSessionIdEstable).
        private static string ObtenerSessionIdEstable(ISession session)
        {
            if (string.IsNullOrEmpty(session.GetString("_estable")))
                session.SetString("_estable", "1");
            return session.Id;
        }
    }
}
