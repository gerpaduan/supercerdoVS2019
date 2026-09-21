using System;

namespace Entidades
{
    // Passkey (huella / Windows Hello / celular) registrada por un usuario para ingresar sin
    // contraseña -- WebAuthn, ver docs/DECISIONS.md "Login por huella (passkeys)". Solo se guarda
    // la CLAVE PUBLICA: la huella nunca sale del dispositivo. Solo existe en Postgres.
    public class UsuarioPasskey
    {
        public int Id { get; set; }
        public int IdUsuario { get; set; }
        public int IdEmpresa { get; set; }

        // Identificador de la credencial que informa el autenticador (unico en todo el sistema).
        public byte[] CredentialId { get; set; }

        // Clave publica COSE con la que se verifica la firma en cada login.
        public byte[] PublicKey { get; set; }

        // Contador de firmas del autenticador: si un login trae un valor menor o igual al
        // guardado (y no es 0), la credencial pudo haber sido clonada.
        public long SignCount { get; set; }

        // Identificador opaco del usuario para WebAuthn (aleatorio, sin datos personales); es el
        // mismo para todas las passkeys de un usuario.
        public byte[] UserHandle { get; set; }

        public Guid? Aaguid { get; set; }
        public string Transports { get; set; }

        // Nombre que le pone el usuario para reconocerla (ej. "Notebook de caja").
        public string Nombre { get; set; }
        public DateTime FechaAltaUtc { get; set; }
        public DateTime? UltimoUsoUtc { get; set; }
    }
}
