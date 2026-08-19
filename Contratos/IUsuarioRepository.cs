using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja Datos.Usuario completo (19/19 metodos): CRUD/login core (Etapa 13a), Permisos
    // (Etapa 13b), Recuperacion de contrasena (Etapa 13c), Auditoria de ubicacion (Etapa 13d).
    // Ver docs/DECISIONS.md.
    public interface IUsuarioRepository
    {
        DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false);
        DataTable getUsuarioActivos();
        Entidades.Usuario getUsuarioById(int idUsuario);
        void addOrEditUser(Entidades.Usuario oUsuarioE);
        void setSucursalUsuario(Entidades.Usuario oUsuario);
        void setPermitirLoginFueraSucursal(Entidades.Usuario oUsuario);
        void setEsUsuarioProduccion(Entidades.Usuario oUsuario);
        void ActualizarEstadoBloqueoLogin(Entidades.Usuario oUsuario);
        List<Entidades.Usuario> BuscarUsuariosPorIdentificador(string identificador, bool soloActivos);
        void ActualizarPasswordSeguro(int idUsuario, string claveLegacy, string passwordHash, string passwordSalt, int passwordHashIterations);
        void ActualizarPasswordWebSeguro(int idUsuario, string passwordHash, string passwordSalt, int passwordHashIterations);
        List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario);
        void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos);
        void CrearTokenRecuperacion(Entidades.UsuarioPasswordResetToken token);
        Entidades.UsuarioPasswordResetToken ObtenerTokenRecuperacion(string tokenHash);
        void MarcarTokenRecuperacionComoUsado(int idToken);
        void InvalidarTokensPendientesUsuario(int idUsuario, string proposito);
        void RegistrarLoginUbicacion(Entidades.LoginUbicacionLog log);
        DataTable obtenerLoginUbicacionLog(int idEmpresa, DateTime desde, DateTime hasta);
    }
}
