using System;
using System.Collections.Generic;
using System.Data;

namespace NegocioTests.Fakes
{
    // Repositorio de usuarios en memoria para los tests de bloqueo por intentos y clave rapida (PIN).
    // Solo implementa lo que usan esos tests; el resto lanza NotImplementedException a proposito
    // (si un test nuevo lo necesita, que falle fuerte en vez de devolver datos inventados).
    public sealed class FakeUsuarioRepository : Contratos.IUsuarioRepository
    {
        // Ultimo estado persistido por ActualizarEstadoBloqueoLogin (copia, no la misma instancia).
        public Entidades.Usuario UltimoEstadoBloqueo;
        public int VecesActualizoBloqueo;

        // Ultimo PIN persistido por ActualizarPin.
        public string PinHashGuardado = "";
        public string PinSaltGuardado = "";
        public int PinIteracionesGuardadas;

        public void ActualizarEstadoBloqueoLogin(Entidades.Usuario oUsuario, bool sinRestriccionDeTenant = false)
        {
            VecesActualizoBloqueo++;
            UltimoEstadoBloqueo = new Entidades.Usuario
            {
                Id = oUsuario.Id,
                IntentosFallidosLogin = oUsuario.IntentosFallidosLogin,
                Bloqueado = oUsuario.Bloqueado,
                FechaBloqueoUtc = oUsuario.FechaBloqueoUtc,
                IntentosFallidosNoSeguro = oUsuario.IntentosFallidosNoSeguro,
                BloqueadoNoSeguro = oUsuario.BloqueadoNoSeguro,
                FechaBloqueoNoSeguroUtc = oUsuario.FechaBloqueoNoSeguroUtc
            };
        }

        public void ActualizarPin(int idUsuario, string pinHash, string pinSalt, int pinHashIterations, bool sinRestriccionDeTenant = false)
        {
            PinHashGuardado = pinHash;
            PinSaltGuardado = pinSalt;
            PinIteracionesGuardadas = pinHashIterations;
        }

        public List<Entidades.Usuario> ListaBasica = new List<Entidades.Usuario>();
        public bool? UltimoPedidoAdmin;

        public List<Entidades.Usuario> ListarActivosBasico(int idEmpresa, bool admin)
        {
            UltimoPedidoAdmin = admin;
            return ListaBasica;
        }

        public DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false) => throw new NotImplementedException();
        public DataTable getUsuarioActivos() => throw new NotImplementedException();
        public Entidades.Usuario getUsuarioById(int idUsuario, bool sinRestriccionDeTenant = false) => throw new NotImplementedException();
        public bool existeUsuario(string usuario, int idExcluir) => throw new NotImplementedException();
        public void addOrEditUser(Entidades.Usuario oUsuarioE) => throw new NotImplementedException();
        public void setSucursalUsuario(Entidades.Usuario oUsuario) => throw new NotImplementedException();
        public void setPermitirLoginFueraSucursal(Entidades.Usuario oUsuario) => throw new NotImplementedException();
        public void setEsUsuarioProduccion(Entidades.Usuario oUsuario) => throw new NotImplementedException();
        public void setRequiereDispositivoSeguro(Entidades.Usuario oUsuario) => throw new NotImplementedException();
        public List<Entidades.Usuario> BuscarUsuariosPorIdentificador(string identificador, bool soloActivos) => throw new NotImplementedException();
        public void ActualizarPasswordSeguro(int idUsuario, string claveLegacy, string passwordHash, string passwordSalt, int passwordHashIterations) => throw new NotImplementedException();
        public void ActualizarPasswordWebSeguro(int idUsuario, string passwordHash, string passwordSalt, int passwordHashIterations, bool sinRestriccionDeTenant = false) => throw new NotImplementedException();
        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario) => throw new NotImplementedException();
        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos) => throw new NotImplementedException();
        public void CrearTokenRecuperacion(Entidades.UsuarioPasswordResetToken token) => throw new NotImplementedException();
        public Entidades.UsuarioPasswordResetToken ObtenerTokenRecuperacion(string tokenHash) => throw new NotImplementedException();
        public void MarcarTokenRecuperacionComoUsado(int idToken) => throw new NotImplementedException();
        public void InvalidarTokensPendientesUsuario(int idUsuario, string proposito) => throw new NotImplementedException();
        public void RegistrarLoginUbicacion(Entidades.LoginUbicacionLog log) => throw new NotImplementedException();
        public DataTable obtenerLoginUbicacionLog(int idEmpresa, DateTime desde, DateTime hasta) => throw new NotImplementedException();
    }
}
