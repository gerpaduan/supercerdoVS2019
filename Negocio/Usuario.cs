using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Utilidades;
using System.Runtime.Remoting;

namespace Negocio
{
    public class Usuario
    {
        public DataTable dtUsuarios;
        List<Entidades.Usuario> listUsuarios;

        // Datos.Usuario (19/19 metodos) implementa Contratos.IUsuarioRepository completo desde
        // la Etapa 13d -- oUsuarioD puede ser SQL Server o Postgres. Ver docs/DECISIONS.md.
        private readonly Contratos.IUsuarioRepository oUsuarioD;
        private readonly IEmpresaContext _empresa;private readonly IParametrosContext _param;

        // Repo de Sucursal usado SOLO para enriquecer user.Sucursal/user.Empresa en
        // validarUsuario/ValidarUsuarioWeb/ObtenerUsuarioPorIdentificador (login). Optativo,
        // default null -- si no se inyecta, cae a Datos.Sucursal (SQL Server) como siempre,
        // mismo patron ya usado en Negocio.Corte para CortePuntoStockSucursal. Se agrego al
        // cablear LoginController (gap real encontrado: sin esto, el enriquecimiento de
        // Sucursal/Empresa post-login siempre pegaba a SQL Server aunque oUsuarioD fuera
        // Postgres). Ver docs/DECISIONS.md.
        private readonly Contratos.ISucursalRepository _sucursalRepo;

        // Constructor existente: SIN CAMBIOS de comportamiento.
        public Usuario(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa;_param = param;
            oUsuarioD = new Datos.Usuario(empresa);
        }

        // Constructor nuevo, aditivo: inyecta cualquier implementacion de IUsuarioRepository
        // (ej. DatosPostgres.UsuarioPg), mas opcionalmente el repo de Sucursal que usan los 3
        // metodos de login para resolver Sucursal/Empresa del usuario encontrado.
        public Usuario(Contratos.IUsuarioRepository repositorio, IEmpresaContext empresa, IParametrosContext param = null, Contratos.ISucursalRepository sucursalRepositorio = null)
        {
            _empresa = empresa; _param = param;
            oUsuarioD = repositorio ?? throw new ArgumentNullException(nameof(repositorio));
            _sucursalRepo = sucursalRepositorio;
        }

        private Contratos.ISucursalRepository ObtenerSucursalRepo()
        {
            return _sucursalRepo ?? new Datos.Sucursal(_empresa);
        }

        public DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false)
        {
            dtUsuarios = oUsuarioD.obtenerUsuarios(soloActivos, filtroEmpresa, soloAdmin);
            convertDatatableToList();
            return dtUsuarios;
        }

        public DataTable getUsuarioActivos()
        {
            DataTable dtUserActivos;
            
            dtUserActivos = oUsuarioD.getUsuarioActivos();
            return dtUserActivos;
        }

        public Entidades.Usuario getUsuarioById(int idUsuario)
        {
            return oUsuarioD.getUsuarioById(idUsuario);
        }

        public DataTable obtenerUsuariosConTodos(bool soloActivos, bool filtroEmpresa = true)
        {
            dtUsuarios = obtenerUsuarios(soloActivos, filtroEmpresa);
            DataRow drTodos = dtUsuarios.NewRow();
            drTodos["id"] = -1;
            drTodos["nombre"] = "Todos";
            dtUsuarios.Rows.Add(drTodos);
            dtUsuarios.DefaultView.Sort = "id";

            return dtUsuarios;
        }

        public List<Entidades.Usuario> convertDatatableToList()
        {
            if (dtUsuarios == null || (listUsuarios != null && listUsuarios.Count != dtUsuarios.Rows.Count))
            {
                obtenerUsuarios(false, false);
            }
            if (dtUsuarios.Rows.Count > 0)
            {
                listUsuarios = new List<Entidades.Usuario>();
                Entidades.Usuario user;
                foreach (DataRow drUsuario in dtUsuarios.Rows)
                {
                    user = new Entidades.Usuario();
                    user.Id = Convert.ToInt32(drUsuario["id"]);
                    user.Nombre = Convert.ToString(drUsuario["nombre"]);
                    user.User = Convert.ToString(drUsuario["usuario"]);
                    user.Clave = Convert.ToString(drUsuario["clave"]);
                    user.Email = Convert.ToString(drUsuario["email"]);
                    user.PasswordHash = GetOptionalString(drUsuario, "passwordHash");
                    user.PasswordSalt = GetOptionalString(drUsuario, "passwordSalt");
                    user.PasswordHashIterations = GetOptionalInt(drUsuario, "passwordHashIterations");
                    user.PasswordUpdatedAtUtc = GetOptionalDateTime(drUsuario, "passwordUpdatedAtUtc");
                    user.Admin = Convert.ToBoolean(drUsuario["admin"]);
                    user.Activo = Convert.ToBoolean(drUsuario["activo"]);
                    user.ColorForm = Convert.ToString(drUsuario["colorForm"]);
                    user.IdSucursal = drUsuario["idSucursalUser"] == DBNull.Value
                                            ? 0
                                            : Convert.ToInt32(drUsuario["idSucursalUser"]);
                    user.IdEmpresa = drUsuario["idEmpresa"] == DBNull.Value
                                                        ? 0
                                                        : Convert.ToInt32(drUsuario["idEmpresa"]);
                    user.PermitirLoginFueraSucursal = GetOptionalBool(drUsuario, "PermitirLoginFueraSucursal");
                    user.EsUsuarioProduccion = GetOptionalBool(drUsuario, "esUsuarioProduccion");
                    user.IntentosFallidosLogin = GetOptionalInt(drUsuario, "intentosFallidosLogin");
                    user.Bloqueado = GetOptionalBool(drUsuario, "bloqueado");
                    user.FechaBloqueoUtc = GetOptionalDateTime(drUsuario, "fechaBloqueoUtc");

                    user.Permisos = oUsuarioD.getPermisosUsuario(user.Id);

                    listUsuarios.Add(user);
                }
            }
            return listUsuarios;
        }

        public List<Entidades.Usuario> listaUsuario()
        {
            return convertDatatableToList();
        }

        /// <summary>
        /// validar si existe usuario. El valor del parametro puede ser el nombre de usuario o el email
        /// valida los dos
        /// </summary>
        /// <param name="usuario">usuario o email</param>
        /// <param name="clave"></param>
        /// <param name="soloNombreUsuario"></param>
        /// <returns></returns>
        public Entidades.Usuario validarUsuario(string usuario, string clave, bool soloNombreUsuario)
        {
            usuario = (usuario ?? "").Trim().ToLowerInvariant();
            clave = clave ?? "";

            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();

                if (listUsuarios == null)
                    return userEncontrado;
            }


            Contratos.ISucursalRepository oSucursalD = ObtenerSucursalRepo();

            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (soloNombreUsuario)
                {
                    if (CoincideIdentificador(user, usuario))
                    {
                        userEncontrado = new Entidades.Usuario();
                        userEncontrado = user;
                        userEncontrado.Sucursal = userEncontrado.IdSucursal > 0
                            ? oSucursalD.findById(userEncontrado.IdSucursal)
                            : null;
                        userEncontrado.Empresa = userEncontrado.IdEmpresa > 0
                            ? oSucursalD.findEmpresaById(userEncontrado.IdEmpresa)
                            : null;
                        break;
                    }
                }
                else
                {
                    if (CoincideIdentificador(user, usuario)
                        && PasswordMatches(user, clave))
                    {
                        userEncontrado = new Entidades.Usuario();
                        userEncontrado = user;
                        userEncontrado.Sucursal = userEncontrado.IdSucursal > 0
                            ? oSucursalD.findById(userEncontrado.IdSucursal)
                            : null;
                        userEncontrado.Empresa = userEncontrado.IdEmpresa > 0
                            ? oSucursalD.findEmpresaById(userEncontrado.IdEmpresa)
                            : null;
                        break;
                    }
                }
            }
            return userEncontrado;
        }

        public Entidades.Usuario ValidarUsuarioWeb(string usuario, string clave)
        {
            usuario = (usuario ?? "").Trim().ToLowerInvariant();
            clave = clave ?? "";

            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();

                if (listUsuarios == null)
                    return userEncontrado;
            }

            Contratos.ISucursalRepository oSucursalD = ObtenerSucursalRepo();

            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (!CoincideIdentificador(user, usuario))
                    continue;

                if (!PasswordMatchesForWeb(user, clave))
                    continue;

                userEncontrado = new Entidades.Usuario();
                userEncontrado = user;
                userEncontrado.Sucursal = userEncontrado.IdSucursal > 0
                    ? oSucursalD.findById(userEncontrado.IdSucursal)
                    : null;
                userEncontrado.Empresa = userEncontrado.IdEmpresa > 0
                    ? oSucursalD.findEmpresaById(userEncontrado.IdEmpresa)
                    : null;
                break;
            }

            return userEncontrado;
        }

        // Misma logica de matching que ValidarUsuarioWeb (CoincideIdentificador, mismo scope de
        // empresa) pero SIN chequear contraseña -- necesario para poder mirar Bloqueado/Activo
        // antes de intentar validar la clave en LoginController.
        public Entidades.Usuario ObtenerUsuarioPorIdentificador(string usuario)
        {
            usuario = (usuario ?? "").Trim().ToLowerInvariant();

            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
                if (listUsuarios == null)
                    return null;
            }

            Contratos.ISucursalRepository oSucursalD = ObtenerSucursalRepo();

            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (!CoincideIdentificador(user, usuario))
                    continue;

                var userEncontrado = user;
                userEncontrado.Sucursal = userEncontrado.IdSucursal > 0
                    ? oSucursalD.findById(userEncontrado.IdSucursal)
                    : null;
                userEncontrado.Empresa = userEncontrado.IdEmpresa > 0
                    ? oSucursalD.findEmpresaById(userEncontrado.IdEmpresa)
                    : null;
                return userEncontrado;
            }

            return null;
        }

        // Incrementa el contador de intentos fallidos y, si llega a maxIntentos, bloquea la
        // cuenta (Bloqueado=true + FechaBloqueoUtc). Devuelve true solo en el momento exacto de
        // la transicion a bloqueado -- el caller usa eso para mandar el mail de desbloqueo UNA
        // sola vez por bloqueo, no en cada reintento posterior (ver LoginController.Index POST).
        public bool RegistrarIntentoFallido(Entidades.Usuario usuario, int maxIntentos)
        {
            if (usuario == null) return false;

            usuario.IntentosFallidosLogin++;
            bool seAcabaDeBoquear = usuario.IntentosFallidosLogin >= maxIntentos;

            if (seAcabaDeBoquear)
            {
                usuario.Bloqueado = true;
                usuario.FechaBloqueoUtc = DateTime.UtcNow;
            }

            oUsuarioD.ActualizarEstadoBloqueoLogin(usuario);
            return seAcabaDeBoquear;
        }

        // Resetea el contador tras un login exitoso -- higiene, no se acarrean intentos viejos.
        public void RegistrarLoginExitoso(Entidades.Usuario usuario)
        {
            if (usuario == null || usuario.IntentosFallidosLogin == 0) return;

            usuario.IntentosFallidosLogin = 0;
            oUsuarioD.ActualizarEstadoBloqueoLogin(usuario);
        }

        // Desbloquea una cuenta -- usado tanto por el link de email (LoginController.UnlockAccount)
        // como por un admin (UsuariosController.DesbloquearUsuario).
        public void DesbloquearUsuario(int idUsuario)
        {
            oUsuarioD.ActualizarEstadoBloqueoLogin(new Entidades.Usuario
            {
                Id = idUsuario,
                IntentosFallidosLogin = 0,
                Bloqueado = false,
                FechaBloqueoUtc = null
            });
        }

        public Entidades.Usuario getUser(string usuario)
        {
            convertDatatableToList();

            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
            }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (user.User.Equals(usuario))
                {
                    userEncontrado = new Entidades.Usuario();
                    userEncontrado = user;
                }
            }
            return userEncontrado;
        }

        public Entidades.Usuario getUserById(int idUsuario)
        {
            Entidades.Usuario userEncontrado = null;
            if (listUsuarios == null)
            {
                listUsuarios = convertDatatableToList();
            }
            foreach (Entidades.Usuario user in listUsuarios)
            {
                if (user.Id.Equals(idUsuario))
                {
                    userEncontrado = new Entidades.Usuario();
                    userEncontrado = user;
                }
            }
            return userEncontrado;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            oUsuarioD.addOrEditUser(oUsuarioE);
        }

        public List<Entidades.Usuario> BuscarUsuariosPorIdentificador(string identificador, bool soloActivos)
        {
            return oUsuarioD.BuscarUsuariosPorIdentificador(identificador, soloActivos);
        }

        public void ActualizarPasswordSeguro(int idUsuario, string nuevaClave)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("Usuario inválido.", nameof(idUsuario));

            if (string.IsNullOrWhiteSpace(nuevaClave))
                throw new ArgumentException("La nueva clave es obligatoria.", nameof(nuevaClave));

            var hashResult = PasswordSecurity.HashPassword(nuevaClave.Trim());
            oUsuarioD.ActualizarPasswordSeguro(idUsuario, nuevaClave.Trim(), hashResult.Hash, hashResult.Salt, hashResult.Iterations);
        }

        public void ActualizarPasswordWebSeguro(int idUsuario, string nuevaClave)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("Usuario inválido.", nameof(idUsuario));

            if (string.IsNullOrWhiteSpace(nuevaClave))
                throw new ArgumentException("La nueva clave es obligatoria.", nameof(nuevaClave));

            var hashResult = PasswordSecurity.HashPassword(nuevaClave.Trim());
            oUsuarioD.ActualizarPasswordWebSeguro(idUsuario, hashResult.Hash, hashResult.Salt, hashResult.Iterations);
        }

        public void CrearTokenRecuperacion(Entidades.UsuarioPasswordResetToken token)
        {
            oUsuarioD.CrearTokenRecuperacion(token);
        }

        public Entidades.UsuarioPasswordResetToken ObtenerTokenRecuperacion(string tokenHash)
        {
            return oUsuarioD.ObtenerTokenRecuperacion(tokenHash);
        }

        public void MarcarTokenRecuperacionComoUsado(int idToken)
        {
            oUsuarioD.MarcarTokenRecuperacionComoUsado(idToken);
        }

        public void InvalidarTokensPendientesUsuario(int idUsuario, string proposito)
        {
            oUsuarioD.InvalidarTokensPendientesUsuario(idUsuario, proposito);
        }


        public void setSucursalUsuario(Entidades.Usuario oUsuario)
        {
            
            oUsuarioD.setSucursalUsuario(oUsuario);
        }

        public void setPermitirLoginFueraSucursal(Entidades.Usuario oUsuario)
        {
            oUsuarioD.setPermitirLoginFueraSucursal(oUsuario);
        }

        public void setEsUsuarioProduccion(Entidades.Usuario oUsuario)
        {
            oUsuarioD.setEsUsuarioProduccion(oUsuario);
        }

        public void RegistrarLoginUbicacion(Entidades.LoginUbicacionLog log)
        {
            oUsuarioD.RegistrarLoginUbicacion(log);
        }

        public DataTable obtenerLoginUbicacionLog(int idEmpresa, DateTime desde, DateTime hasta)
        {
            return oUsuarioD.obtenerLoginUbicacionLog(idEmpresa, desde, hasta);
        }

        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario)
        {
            return oUsuarioD.getPermisosUsuario(idUsuario);
        }
        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {
            oUsuarioD.AddOrEditPermisos(permisos);
        }

        /// <summary>
        /// Se valida si el oUsuario tiene permiso en el formulario, por defecto pasar Fecha Actual,
        /// idCreador, pasar -1 si no se quiere verificar la Edicion
        /// </summary>
        /// <param name="oUser"></param>
        /// <param name="nombreForm"></param>
        /// <param name="fechaDesde"></param>
        /// <returns></returns>
        public bool tienePermiso(Entidades.Usuario oUser, string nombreForm, DateTime fechaDesde, int idCreador)
        {
            bool permisoVer = false, permisoEditar = false;

            nombreForm = nombreForm.ToUpper();

            if (oUser == null)
                return false;

            if (oUser.Admin)
                return true;

            foreach (var permiso in oUser.Permisos)
            {

                if (idCreador >= 0)
                {
                    if ((permiso.Formulario.FormEdicion.Contains(nombreForm) ||
                        permiso.Formulario.FormEdicionExtra1.Contains(nombreForm) ||
                        permiso.Formulario.FormEdicionExtra2.Contains(nombreForm)))
                    {
                        bool d = false;
                    }

                    //permisoEditar = (permiso.Formulario.FormEdicion.Contains(nombreForm) ||
                    //    permiso.Formulario.FormEdicionExtra1.Contains(nombreForm) ||
                    //    permiso.Formulario.FormEdicionExtra2.Contains(nombreForm)) &&
                    //    DateTime.Today.AddDays(-permiso.DiasPermitidosEditar) <= fechaDesde;
                    

                    //Actualizacion: 26/11/2025 No sé porque habia puesto contains en vez de equal
                    //la actualizacion pasa todo a minusculas y equals
                    permisoEditar = (permiso.Formulario.FormEdicion.ToUpper().Equals(nombreForm) ||
                        permiso.Formulario.FormEdicionExtra1.ToUpper().Equals(nombreForm) ||
                        permiso.Formulario.FormEdicionExtra2.ToUpper().Equals(nombreForm)) &&
                        DateTime.Today.AddDays(-permiso.DiasPermitidosEditar) <= fechaDesde;

                    //si permisoEditar es TRUE, se verifica q sea que permita editar todos ó haya creado el registro
                    if (permisoEditar)
                    {
                        permisoEditar = !permiso.SoloRegistrosPropios || permiso.SoloRegistrosPropios && oUser.Id == idCreador;
                        return permisoEditar;
                    }
                }
                else
                {
                    permisoVer = permiso.Formulario.FormConsulta.ToUpper().Equals(nombreForm) &&
                        DateTime.Today.AddDays(-permiso.DiasPermitidosVer) <= fechaDesde;

                    if (permisoVer)
                    {
                        string d = ";";
                    }
                }
                
                //si idCreador mayor o igual a 0 se debe validar la edicion

                if (permisoVer || permisoEditar)
                    break;
            }
            return permisoVer || permisoEditar;
        }

        private bool CoincideIdentificador(Entidades.Usuario user, string usuario)
        {
            return string.Equals((user.User ?? "").Trim(), usuario, StringComparison.OrdinalIgnoreCase)
                || string.Equals((user.Email ?? "").Trim(), usuario, StringComparison.OrdinalIgnoreCase);
        }

        private bool PasswordMatches(Entidades.Usuario user, string claveTexto)
        {
            bool coincideHash = !string.IsNullOrWhiteSpace(user.PasswordHash)
                && !string.IsNullOrWhiteSpace(user.PasswordSalt)
                && PasswordSecurity.VerifyPassword(claveTexto, user.PasswordHash, user.PasswordSalt, user.PasswordHashIterations);

            if (coincideHash)
                return true;

            bool coincideLegacy = string.Equals(user.Clave ?? "", claveTexto, StringComparison.OrdinalIgnoreCase);
            if (coincideLegacy && string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                try
                {
                    ActualizarPasswordSeguro(user.Id, user.Clave ?? claveTexto);
                }
                catch
                {
                    // La migración puede no estar aplicada todavía en algunas bases.
                }
            }

            return coincideLegacy;
        }

        private bool PasswordMatchesForWeb(Entidades.Usuario user, string claveTexto)
        {
            bool coincideHash = !string.IsNullOrWhiteSpace(user.PasswordHash)
                && !string.IsNullOrWhiteSpace(user.PasswordSalt)
                && PasswordSecurity.VerifyPassword(claveTexto, user.PasswordHash, user.PasswordSalt, user.PasswordHashIterations);

            if (coincideHash)
                return true;

            bool coincideLegacy = string.Equals(user.Clave ?? "", claveTexto, StringComparison.OrdinalIgnoreCase);
            if (coincideLegacy && string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                try
                {
                    ActualizarPasswordWebSeguro(user.Id, claveTexto);
                }
                catch
                {
                    // La base puede no tener todavía aplicada la migración web segura.
                }
            }

            return coincideLegacy;
        }

        private static string GetOptionalString(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return "";

            object value = row[columnName];
            return value == DBNull.Value ? "" : Convert.ToString(value);
        }

        private static int GetOptionalInt(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return 0;

            object value = row[columnName];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static DateTime? GetOptionalDateTime(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return null;

            object value = row[columnName];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private static bool GetOptionalBool(DataRow row, string columnName)
        {
            if (row == null || !row.Table.Columns.Contains(columnName))
                return false;

            object value = row[columnName];
            return value != DBNull.Value && Convert.ToBoolean(value);
        }
    }
}
