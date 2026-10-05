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

        public Entidades.Usuario getUsuarioById(int idUsuario, bool sinRestriccionDeTenant = false)
        {
            return oUsuarioD.getUsuarioById(idUsuario, sinRestriccionDeTenant);
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
                    user.RequiereDispositivoSeguro = GetOptionalBool(drUsuario, "RequiereDispositivoSeguro");
                    user.IntentosFallidosLogin = GetOptionalInt(drUsuario, "intentosFallidosLogin");
                    user.Bloqueado = GetOptionalBool(drUsuario, "bloqueado");
                    user.FechaBloqueoUtc = GetOptionalDateTime(drUsuario, "fechaBloqueoUtc");
                    user.IntentosFallidosNoSeguro = GetOptionalInt(drUsuario, "intentosFallidosNoSeguro");
                    user.BloqueadoNoSeguro = GetOptionalBool(drUsuario, "bloqueadoNoSeguro");
                    user.FechaBloqueoNoSeguroUtc = GetOptionalDateTime(drUsuario, "fechaBloqueoNoSeguroUtc");
                    user.PinHash = GetOptionalString(drUsuario, "pinHash");
                    user.PinSalt = GetOptionalString(drUsuario, "pinSalt");
                    user.PinHashIterations = GetOptionalInt(drUsuario, "pinHashIterations");
                    user.PinUpdatedAtUtc = GetOptionalDateTime(drUsuario, "pinUpdatedAtUtc");

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

        // ===== Formula secreta (2026-10-04, ver docs/DECISIONS.md "Formula secreta con re-login") =====

        // Resultado de pedir que un usuario se autentique para ver una formula secreta. Motivo es SOLO
        // para auditoria (distingue "clave incorrecta" de "sin permiso"): al usuario final se le muestra
        // siempre el mismo mensaje generico, para no revelar que usuarios existen ni cuales tienen permiso.
        public sealed class ResultadoAutorizacionFormula
        {
            public bool Autorizado { get; set; }
            public Entidades.Usuario Autorizador { get; set; }
            // Id del usuario tipeado cuando existe (aunque la clave haya sido incorrecta); null si no existe.
            public int? IdUsuarioCandidato { get; set; }
            public string Motivo { get; set; }
        }

        // Quien puede ver una formula secreta: el que tiene el permiso de ver formulas (consulta) o el de
        // ingresar/editar formulas (Admin pasa siempre, via tienePermiso), y ademas esta activo y sin
        // bloqueo de cuenta.
        public bool PuedeVerFormulaSecreta(Entidades.Usuario candidato)
        {
            if (candidato == null || !candidato.Activo) return false;
            if (candidato.Bloqueado || candidato.BloqueadoNoSeguro) return false;

            return tienePermiso(candidato, Entidades.Permisos.Elaborado.VerFormulas, DateTime.Today, -1)
                || tienePermiso(candidato, Entidades.Permisos.Elaborado.IngresoFormula, DateTime.Today, candidato.Id);
        }

        // Usuarios activos de la empresa que pueden autorizar "Ver formula" (alimenta el selector de usuario).
        // convertDatatableToList ya carga los permisos de cada usuario, por eso se puede evaluar aca sin
        // consultas extra. Es solo comodidad de UI: la autorizacion real se vuelve a validar en
        // AutorizarVerFormulaSecreta.
        public List<Entidades.Usuario> ObtenerUsuariosConPermisoVerFormula()
        {
            obtenerUsuarios(true);
            return (listUsuarios ?? new List<Entidades.Usuario>())
                .Where(PuedeVerFormulaSecreta)
                .OrderBy(u => u.Nombre, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Step-up de "Ver formula": valida usuario + clave tipeados y que tenga permiso. No cuenta intentos
        // fallidos sobre la cuenta (eso lo hace el rate limit por sesion del controller): un operador no
        // debe poder bloquear la cuenta de quien tiene el permiso adivinandole la clave.
        public ResultadoAutorizacionFormula AutorizarVerFormulaSecreta(string usuario, string clave)
        {
            if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(clave))
                return new ResultadoAutorizacionFormula { Motivo = "Datos incompletos" };

            var candidato = ObtenerUsuarioPorIdentificador(usuario);
            if (candidato == null)
                return new ResultadoAutorizacionFormula { Motivo = "Usuario inexistente" };

            var resultado = new ResultadoAutorizacionFormula { IdUsuarioCandidato = candidato.Id };
            if (!candidato.Activo) { resultado.Motivo = "Cuenta inactiva"; return resultado; }
            if (candidato.Bloqueado || candidato.BloqueadoNoSeguro) { resultado.Motivo = "Cuenta bloqueada"; return resultado; }

            var validado = ValidarUsuarioWeb(usuario, clave);
            if (validado == null) { resultado.Motivo = "Clave incorrecta"; return resultado; }
            if (!PuedeVerFormulaSecreta(validado)) { resultado.Motivo = "Sin permiso para ver formulas"; return resultado; }

            resultado.Autorizado = true;
            resultado.Autorizador = validado;
            return resultado;
        }

        // Incrementa el contador de intentos fallidos y, si llega a maxIntentos, bloquea la
        // cuenta (Bloqueado=true + FechaBloqueoUtc). Devuelve true solo en el momento exacto de
        // la transicion a bloqueado -- el caller usa eso para mandar el mail de desbloqueo UNA
        // sola vez por bloqueo, no en cada reintento posterior (ver LoginController.Index POST).
        // sinRestriccionDeTenant=true: LoginController lo llama tras un intento fallido, con
        // oUsuarioN todavia sin re-escopear a la empresa real del candidato (ver
        // Contratos/IUsuarioRepository.cs).
        public bool RegistrarIntentoFallido(Entidades.Usuario usuario, int maxIntentos, bool sinRestriccionDeTenant = false)
        {
            return RegistrarIntentoFallido(usuario, maxIntentos, dispositivoSeguro: true, sinRestriccionDeTenant: sinRestriccionDeTenant).SeAcabaDeBloquear;
        }

        // Resultado de registrar un intento fallido, ya con el conteo segun el origen del intento.
        public sealed class ResultadoIntentoFallido
        {
            // Fallos acumulados en el contador del origen (seguro / no seguro).
            public int IntentosFallidos { get; set; }
            // Intentos que le quedan antes del bloqueo (0 si ya quedo bloqueado).
            public int IntentosRestantes { get; set; }
            // true solo en el intento exacto que dispara el bloqueo.
            public bool SeAcabaDeBloquear { get; set; }
            // true si el bloqueo es de la cuenta completa (origen seguro); false si solo bloquea los
            // logins desde dispositivos no seguros.
            public bool BloqueoCuentaCompleta { get; set; }
        }

        // Cuenta un intento fallido en el contador del ORIGEN del intento (2026-10-02, ver
        // docs/DECISIONS.md "Login por CUIT, clave rapida (PIN) y politica de clave"):
        //  - dispositivoSeguro=true  -> el propio usuario equivocandose: a los maxIntentos se
        //    bloquea la cuenta completa (Bloqueado), con desbloqueo por mail o por un admin.
        //  - dispositivoSeguro=false -> puede ser cualquiera adivinando: a los maxIntentos solo se
        //    bloquean los logins desde dispositivos NO seguros (BloqueadoNoSeguro); el dueño sigue
        //    entrando desde su dispositivo seguro. Asi un desconocido no puede dejar afuera a nadie.
        // sinRestriccionDeTenant: ver arriba (LoginController todavia sin re-escopear a la empresa).
        public ResultadoIntentoFallido RegistrarIntentoFallido(Entidades.Usuario usuario, int maxIntentos, bool dispositivoSeguro, bool sinRestriccionDeTenant = false)
        {
            if (usuario == null) return new ResultadoIntentoFallido();

            var resultado = new ResultadoIntentoFallido { BloqueoCuentaCompleta = dispositivoSeguro };

            if (dispositivoSeguro)
            {
                usuario.IntentosFallidosLogin++;
                resultado.IntentosFallidos = usuario.IntentosFallidosLogin;
                resultado.SeAcabaDeBloquear = usuario.IntentosFallidosLogin >= maxIntentos;

                if (resultado.SeAcabaDeBloquear)
                {
                    usuario.Bloqueado = true;
                    usuario.FechaBloqueoUtc = DateTime.UtcNow;
                }
            }
            else
            {
                usuario.IntentosFallidosNoSeguro++;
                resultado.IntentosFallidos = usuario.IntentosFallidosNoSeguro;
                resultado.SeAcabaDeBloquear = usuario.IntentosFallidosNoSeguro >= maxIntentos;

                if (resultado.SeAcabaDeBloquear)
                {
                    usuario.BloqueadoNoSeguro = true;
                    usuario.FechaBloqueoNoSeguroUtc = DateTime.UtcNow;
                }
            }

            resultado.IntentosRestantes = Math.Max(0, maxIntentos - resultado.IntentosFallidos);
            oUsuarioD.ActualizarEstadoBloqueoLogin(usuario, sinRestriccionDeTenant);
            return resultado;
        }

        // Aviso de "te quedan N intentos": recien a partir del 3.er error (pedido del usuario);
        // antes de eso no se muestra para no alarmar por un tipeo. Devuelve null si no corresponde.
        public const int FalloDesdeElQueSeAvisaIntentosRestantes = 3;

        public static int? IntentosRestantesParaAvisar(ResultadoIntentoFallido resultado)
        {
            if (resultado == null || resultado.SeAcabaDeBloquear) return null;
            return resultado.IntentosFallidos >= FalloDesdeElQueSeAvisaIntentosRestantes ? resultado.IntentosRestantes : (int?)null;
        }

        // Resetea el contador tras un login exitoso -- higiene, no se acarrean intentos viejos.
        // Sin sinRestriccionDeTenant: se llama despues de re-escopear oUsuarioN a la empresa real.
        // Compat: sin origen resetea el contador del dispositivo seguro (comportamiento anterior).
        public void RegistrarLoginExitoso(Entidades.Usuario usuario)
        {
            RegistrarLoginExitoso(usuario, dispositivoSeguro: true);
        }

        // Resetea el contador del origen desde el que se logueo (un login exitoso desde un
        // dispositivo seguro NO borra los intentos fallidos acumulados desde dispositivos no
        // seguros: si alguien esta adivinando la clave, el dueño entrar bien no lo detiene).
        public void RegistrarLoginExitoso(Entidades.Usuario usuario, bool dispositivoSeguro)
        {
            if (usuario == null) return;

            if (dispositivoSeguro)
            {
                if (usuario.IntentosFallidosLogin == 0) return;
                usuario.IntentosFallidosLogin = 0;
            }
            else
            {
                if (usuario.IntentosFallidosNoSeguro == 0) return;
                usuario.IntentosFallidosNoSeguro = 0;
            }

            oUsuarioD.ActualizarEstadoBloqueoLogin(usuario);
        }

        // Desbloquea una cuenta -- usado tanto por el link de email (LoginController.UnlockAccount,
        // sinRestriccionDeTenant=true: tenant desconocido hasta resolver el token) como por un
        // admin (UsuariosController.DesbloquearUsuario, empresa ya conocida). Limpia AMBOS
        // contadores/bloqueos (seguro y no seguro).
        public void DesbloquearUsuario(int idUsuario, bool sinRestriccionDeTenant = false)
        {
            oUsuarioD.ActualizarEstadoBloqueoLogin(new Entidades.Usuario
            {
                Id = idUsuario,
                IntentosFallidosLogin = 0,
                Bloqueado = false,
                FechaBloqueoUtc = null,
                IntentosFallidosNoSeguro = 0,
                BloqueadoNoSeguro = false,
                FechaBloqueoNoSeguroUtc = null
            }, sinRestriccionDeTenant);
        }

        // ---------------------------------------------------------------------------------
        // Clave rapida (PIN): 4-6 digitos, valida SOLO desde un dispositivo seguro (esa condicion la
        // aplica el LoginController, no esta capa). Hash PBKDF2 aparte de la clave.
        // ---------------------------------------------------------------------------------

        // true si el PIN tipeado coincide con el hash guardado. Sin PIN configurado -> false.
        public bool VerificarPin(Entidades.Usuario usuario, string pin)
        {
            if (usuario == null || !usuario.TienePin || string.IsNullOrEmpty(pin))
                return false;

            return PasswordSecurity.VerifyPassword(pin, usuario.PinHash, usuario.PinSalt, usuario.PinHashIterations);
        }

        // Alta/cambio de PIN. Valida formato y reglas (PoliticaClave.ValidarPin) y lanza
        // ArgumentException con el mensaje para el usuario si no cumple.
        public void ActualizarPin(int idUsuario, string pin, bool sinRestriccionDeTenant = false)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("Usuario inválido.", nameof(idUsuario));

            string error = PoliticaClave.ValidarPin(pin, idUsuario);
            if (error != null)
                throw new ArgumentException(error); // sin paramName: el mensaje se le muestra tal cual al usuario

            var hash = PasswordSecurity.HashPassword(pin);
            oUsuarioD.ActualizarPin(idUsuario, hash.Hash, hash.Salt, hash.Iterations, sinRestriccionDeTenant);
        }

        public void QuitarPin(int idUsuario, bool sinRestriccionDeTenant = false)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("Usuario inválido.", nameof(idUsuario));

            oUsuarioD.ActualizarPin(idUsuario, string.Empty, string.Empty, 0, sinRestriccionDeTenant);
        }

        // Lista liviana (id, nombre, usuario) de activos de una empresa, para el login por CUIT en
        // dispositivo seguro: primero los no admin y despues los administradores (2026-10-04, a pedido
        // del usuario: los admin tambien aparecen). Sin clave, hash ni permisos. El PIN sigue sin valer
        // para admin (ver LoginController.ProcesarLoginAsync): ellos ingresan con su contraseña.
        public List<Entidades.Usuario> ListarUsuariosParaLogin(int idEmpresa)
        {
            var lista = new List<Entidades.Usuario>(oUsuarioD.ListarActivosBasico(idEmpresa, false) ?? new List<Entidades.Usuario>());
            lista.AddRange(oUsuarioD.ListarActivosBasico(idEmpresa, true) ?? new List<Entidades.Usuario>());
            return lista;
        }

        // Administradores activos de una empresa (con su mail): para avisarles de solicitudes de
        // autorizacion de dispositivo. Solo datos basicos, igual que ListarUsuariosParaLogin.
        public List<Entidades.Usuario> ListarAdministradoresActivos(int idEmpresa)
        {
            return oUsuarioD.ListarActivosBasico(idEmpresa, true);
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

        public bool existeUsuario(string usuario, int idExcluir)
        {
            return oUsuarioD.existeUsuario(usuario, idExcluir);
        }

        // Chequeo global de unicidad antes de guardar -- mensaje legible; el candado de verdad
        // es el indice unico de Postgres (migracion 20260821) mas la restriccion natural de
        // "una sola empresa por base" en SQL Server. Necesario porque el login busca por
        // usuario/email cruzando todas las empresas (ver Contratos/IUsuarioRepository.cs): dos
        // empresas con el mismo nombre de usuario hacen que el login sea ambiguo.
        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            if (oUsuarioE == null) throw new ArgumentNullException(nameof(oUsuarioE));

            if (existeUsuario(oUsuarioE.User, oUsuarioE.Id))
                throw new InvalidOperationException(
                    $"Ya existe un usuario con el nombre \"{oUsuarioE.User}\" (en esta empresa o en otra). Elegí otro nombre de usuario.");

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

        public void ActualizarPasswordWebSeguro(int idUsuario, string nuevaClave, bool sinRestriccionDeTenant = false)
        {
            if (idUsuario <= 0)
                throw new ArgumentException("Usuario inválido.", nameof(idUsuario));

            if (string.IsNullOrWhiteSpace(nuevaClave))
                throw new ArgumentException("La nueva clave es obligatoria.", nameof(nuevaClave));

            var hashResult = PasswordSecurity.HashPassword(nuevaClave.Trim());
            oUsuarioD.ActualizarPasswordWebSeguro(idUsuario, hashResult.Hash, hashResult.Salt, hashResult.Iterations, sinRestriccionDeTenant);
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

        public void setRequiereDispositivoSeguro(Entidades.Usuario oUsuario)
        {
            oUsuarioD.setRequiereDispositivoSeguro(oUsuario);
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
