using System;
using System.Collections.Generic;

namespace Negocio
{
    // Reglas de negocio de las passkeys (huella) de un usuario. Solo Postgres: no hay constructor
    // contra SQL Server. Ver docs/DECISIONS.md "Login por huella (passkeys)".
    public class UsuarioPasskey
    {
        // Largo maximo del nombre que se muestra en "Mi huella".
        public const int LargoMaximoNombre = 60;

        // Tope de huellas por usuario: evita que una cuenta acumule credenciales sin control.
        public const int MaximoPorUsuario = 10;

        private readonly Contratos.IUsuarioPasskeyRepository oPasskeyD;

        public UsuarioPasskey(Contratos.IUsuarioPasskeyRepository repositorio)
        {
            oPasskeyD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
        }

        public List<Entidades.UsuarioPasskey> ListarPorUsuario(int idUsuario, int idEmpresa)
        {
            return oPasskeyD.ListarPorUsuario(idUsuario, idEmpresa);
        }

        // Registra una passkey nueva. Si el usuario ya tenia otras, reusa su mismo userHandle
        // (WebAuthn exige un unico userHandle por usuario en el mismo sitio); si es la primera,
        // genera uno aleatorio (nunca el id ni datos personales).
        public Entidades.UsuarioPasskey Registrar(
            int idUsuario, int idEmpresa, byte[] credentialId, byte[] publicKey, long signCount,
            byte[] userHandle, Guid? aaguid, string transports, string nombre)
        {
            if (credentialId == null || credentialId.Length == 0) throw new ArgumentException("Falta el identificador de la credencial.", nameof(credentialId));
            if (publicKey == null || publicKey.Length == 0) throw new ArgumentException("Falta la clave pública.", nameof(publicKey));
            if (userHandle == null || userHandle.Length == 0) throw new ArgumentException("Falta el identificador de usuario.", nameof(userHandle));

            nombre = (nombre ?? "").Trim();
            if (nombre.Length == 0) nombre = "Mi huella";
            if (nombre.Length > LargoMaximoNombre) nombre = nombre.Substring(0, LargoMaximoNombre);

            if (oPasskeyD.ListarPorUsuario(idUsuario, idEmpresa).Count >= MaximoPorUsuario)
                throw new InvalidOperationException("Alcanzaste el máximo de " + MaximoPorUsuario + " huellas registradas. Eliminá alguna para agregar otra.");

            var passkey = new Entidades.UsuarioPasskey
            {
                IdUsuario = idUsuario,
                IdEmpresa = idEmpresa,
                CredentialId = credentialId,
                PublicKey = publicKey,
                SignCount = signCount,
                UserHandle = userHandle,
                Aaguid = aaguid,
                Transports = transports,
                Nombre = nombre,
                FechaAltaUtc = DateTime.UtcNow
            };
            oPasskeyD.Agregar(passkey);
            return passkey;
        }

        // userHandle ya asignado a este usuario (el de su primera passkey) o null si no tiene ninguna.
        public byte[] ObtenerUserHandleExistente(int idUsuario, int idEmpresa)
        {
            var existentes = oPasskeyD.ListarPorUsuario(idUsuario, idEmpresa);
            return existentes.Count > 0 ? existentes[0].UserHandle : null;
        }

        public bool Eliminar(int id, int idUsuario, int idEmpresa)
        {
            return oPasskeyD.Eliminar(id, idUsuario, idEmpresa);
        }

        // Login: busca la credencial cruzando empresas (el tenant sale de la propia fila).
        public Entidades.UsuarioPasskey ObtenerPorCredentialIdSinTenant(byte[] credentialId)
        {
            return oPasskeyD.ObtenerPorCredentialIdSinTenant(credentialId);
        }

        public void RegistrarUsoSinTenant(int id, long signCount)
        {
            oPasskeyD.RegistrarUsoSinTenant(id, signCount, DateTime.UtcNow);
        }
    }
}
