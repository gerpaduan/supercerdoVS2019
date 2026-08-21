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
        // sinRestriccionDeTenant: ver Contratos/IUnitOfWork.cs para el patron general de flags
        // aditivos. Este es especifico de Usuario -- Login/OlvideMiContrasena/ResetPassword/
        // UnlockAccount corren SIN saber a que empresa pertenece el usuario todavia, asi que
        // necesitan cruzar todas las empresas en esos pocos llamados puntuales. Default false:
        // cualquier caller nuevo que no lo mencione explicitamente queda protegido por RLS
        // (Postgres) sin tener que acordarse de nada. Implementacion SQL Server: ignorado (cada
        // instalacion es de una sola empresa, no hay "otro tenant" del que aislarse). Ver
        // docs/DECISIONS.md, 2026-08-21.
        DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false);
        DataTable getUsuarioActivos();
        Entidades.Usuario getUsuarioById(int idUsuario, bool sinRestriccionDeTenant = false);
        // Chequeo global (todas las empresas) de unicidad de nombre de usuario -- el candado de
        // verdad es el indice unico de la migracion 20260821 en Postgres; esto es solo para dar
        // un mensaje de error legible antes de llegar a esa excepcion cruda. idExcluir: el
        // propio Id en una edicion, 0 en un alta.
        bool existeUsuario(string usuario, int idExcluir);
        void addOrEditUser(Entidades.Usuario oUsuarioE);
        void setSucursalUsuario(Entidades.Usuario oUsuario);
        void setPermitirLoginFueraSucursal(Entidades.Usuario oUsuario);
        void setEsUsuarioProduccion(Entidades.Usuario oUsuario);
        void ActualizarEstadoBloqueoLogin(Entidades.Usuario oUsuario, bool sinRestriccionDeTenant = false);
        List<Entidades.Usuario> BuscarUsuariosPorIdentificador(string identificador, bool soloActivos);
        void ActualizarPasswordSeguro(int idUsuario, string claveLegacy, string passwordHash, string passwordSalt, int passwordHashIterations);
        void ActualizarPasswordWebSeguro(int idUsuario, string passwordHash, string passwordSalt, int passwordHashIterations, bool sinRestriccionDeTenant = false);
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
