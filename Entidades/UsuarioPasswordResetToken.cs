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
    }
}
