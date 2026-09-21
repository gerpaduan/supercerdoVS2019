using System;
using System.Collections.Generic;

namespace Contratos
{
    // Passkeys WebAuthn de los usuarios (solo Postgres, tabla usuariopasskeys). Los metodos
    // "SinTenant" los usa el login por huella, que corre ANTES de saber a que empresa pertenece
    // el usuario: el tenant sale de la propia credencial.
    public interface IUsuarioPasskeyRepository
    {
        List<Entidades.UsuarioPasskey> ListarPorUsuario(int idUsuario, int idEmpresa);
        void Agregar(Entidades.UsuarioPasskey passkey);

        // true si borro una fila (solo la borra si pertenece a ese usuario y empresa).
        bool Eliminar(int id, int idUsuario, int idEmpresa);

        // Busca cruzando todas las empresas (login). null si no existe.
        Entidades.UsuarioPasskey ObtenerPorCredentialIdSinTenant(byte[] credentialId);

        // Actualiza contador y fecha de ultimo uso tras un login exitoso (login, sin tenant).
        void RegistrarUsoSinTenant(int id, long signCount, DateTime usoUtc);
    }
}
