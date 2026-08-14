using System;

namespace Entidades
{
    public class UsuarioPasswordResetToken
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }
        public string TokenHash { get; set; }
        public DateTime FechaCreacionUtc { get; set; }
        public DateTime FechaExpiracionUtc { get; set; }
        public bool Usado { get; set; }
        public DateTime? FechaUsoUtc { get; set; }
        public string IdentificadorSolicitado { get; set; }
        public string EmailDestino { get; set; }

        // "reset" (recuperación de contraseña) o "unlock" (desbloqueo de cuenta) -- misma tabla,
        // mismo mecanismo de token hasheado + expiración + un solo uso, discriminado por
        // propósito para que un token de un flujo no sirva para el otro.
        public string Proposito { get; set; } = "reset";
    }
}
