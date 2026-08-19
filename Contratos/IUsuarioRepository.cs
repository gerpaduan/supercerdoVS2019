using System;
using System.Collections.Generic;
using System.Data;

namespace Contratos
{
    // Espeja el bloque CRUD/login core de Datos.Usuario (Etapa 13a). getPermisosUsuario/
    // AddOrEditPermisos (Permisos), CrearTokenRecuperacion/ObtenerTokenRecuperacion/
    // MarcarTokenRecuperacionComoUsado/InvalidarTokensPendientesUsuario (recuperacion de
    // contrasena) y RegistrarLoginUbicacion/obtenerLoginUbicacionLog (auditoria de ubicacion)
    // quedan fuera de esta interfaz -- se agregan en etapas futuras (13b/13c/13d). Ver
    // docs/DECISIONS.md.
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
    }
}
