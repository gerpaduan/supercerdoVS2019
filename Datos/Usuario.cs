using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Utilidades;

namespace Datos
{
    public class Usuario : Contratos.IUsuarioRepository
    {
        private readonly IEmpresaContext _empresa; private readonly IParametrosContext _param;

        public Usuario(IEmpresaContext empresa, IParametrosContext param = null)
        {
            _empresa = empresa ?? throw new ArgumentNullException(nameof(empresa)); _param = param;
        }

        public DataTable obtenerUsuarios(bool soloActivos, bool filtroEmpresa = true, bool soloAdmin = false)
        {
            string sql;

            if (soloAdmin)
            {
                sql = "SELECT * FROM Usuarios WHERE usuario = @usuario";
                return Db.DataTable(
                    _empresa,
                    sql,
                    CommandType.Text,
                    setParams: p =>
                    {
                        p.Add("@usuario", SqlDbType.NVarChar, 50).Value = "admin";
                    }
                );
            }
            var where = new List<string>();

            if (filtroEmpresa)
                where.Add("idEmpresa = @idEmpresa");

            if (soloActivos)
                where.Add("activo = 1");

            sql = "SELECT * FROM Usuarios"
                + (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "");

            return Db.DataTable(_empresa, sql, CommandType.Text, setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa);
        }

        public DataTable getUsuarioActivos()
        {
            const string sql = "SELECT nombre, usuario, clave FROM Usuarios WHERE activo = 1 AND idEmpresa = @idEmpresa";
            return Db.DataTable(_empresa, sql, CommandType.Text, setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = _empresa.IdEmpresa);
        }

        // sinRestriccionDeTenant: ignorado -- cada instalacion de SQL Server es de una sola
        // empresa, no hay "otro tenant" del que aislarse (ver Contratos/IUsuarioRepository.cs).
        public Entidades.Usuario getUsuarioById(int idUsuario, bool sinRestriccionDeTenant = false)
        {
            const string sql = "SELECT * FROM Usuarios WHERE id = @id";

            Entidades.Usuario usuario = null;
            int idSucursal = 0;
            int idEmpresa = 0;

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;

                using (var dr = cmd.ExecuteReader())
                {
                    if (!dr.Read())
                        return null;

                    usuario = MapUsuario(dr);
                    idSucursal = usuario.IdSucursal;
                    idEmpresa = usuario.IdEmpresa;
                }
            }

            var oSucursalD = new Datos.Sucursal(_empresa);
            usuario.Sucursal = idSucursal > 0 ? oSucursalD.findById(idSucursal) : null;
            usuario.Empresa = idEmpresa > 0 ? oSucursalD.findEmpresaById(idEmpresa) : null;

            return usuario;
        }

        // Chequeo global de unicidad de usuario -- en SQL Server "global" es trivialmente "esta
        // instalacion" (una sola empresa por base), asi que alcanza con un SELECT normal.
        public bool existeUsuario(string usuario, int idExcluir)
        {
            usuario = (usuario ?? "").Trim();
            if (usuario.Length == 0) return false;

            const string sql = "SELECT COUNT(*) FROM Usuarios WHERE LOWER(usuario) = LOWER(@usuario) AND id <> @idExcluir";
            object resultado = Db.Scalar(_empresa, sql, CommandType.Text, setParams: p =>
            {
                p.Add("@usuario", SqlDbType.NVarChar, 50).Value = usuario;
                p.Add("@idExcluir", SqlDbType.Int).Value = idExcluir;
            });
            return Convert.ToInt32(resultado) > 0;
        }

        public void addOrEditUser(Entidades.Usuario oUsuarioE)
        {
            if (oUsuarioE == null) throw new ArgumentNullException(nameof(oUsuarioE));

            Db.NonQuery(
                _empresa,
                "addOrEditUser",
                CommandType.StoredProcedure,
                setParams: p =>
                {
                    p.Add("@id", SqlDbType.Int).Value = oUsuarioE.Id;
                    p.Add("@nombre", SqlDbType.NVarChar, 120).Value = oUsuarioE.Nombre ?? "";
                    p.Add("@usuario", SqlDbType.NVarChar, 50).Value = oUsuarioE.User ?? "";
                    p.Add("@email", SqlDbType.NVarChar, 120).Value = oUsuarioE.Email ?? "";
                    p.Add("@clave", SqlDbType.NVarChar, 200).Value = oUsuarioE.Clave ?? "";
                    p.Add("@admin", SqlDbType.Bit).Value = oUsuarioE.Admin;
                    p.Add("@activo", SqlDbType.Bit).Value = oUsuarioE.Activo;
                    p.Add("@colorForm", SqlDbType.NVarChar, 50).Value = oUsuarioE.ColorForm ?? "";
                }
            );
        }

        public void setSucursalUsuario(Entidades.Usuario oUsuario)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));

            const string sql = @"
                UPDATE Usuarios
                SET idSucursalUser = @idSucursal
                WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = oUsuario.Id;
                    p.Add("@idSucursal", SqlDbType.Int).Value = oUsuario.IdSucursal;
                }
            );
        }

        public void setPermitirLoginFueraSucursal(Entidades.Usuario oUsuario)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));
            if (!ExisteColumnaUsuarios("PermitirLoginFueraSucursal"))
                return;

            const string sql = @"
                UPDATE Usuarios
                SET PermitirLoginFueraSucursal = @permitir
                WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = oUsuario.Id;
                    p.Add("@permitir", SqlDbType.Bit).Value = oUsuario.PermitirLoginFueraSucursal;
                }
            );
        }

        public void setEsUsuarioProduccion(Entidades.Usuario oUsuario)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));
            if (!ExisteColumnaUsuarios("esUsuarioProduccion"))
                return;

            const string sql = @"
                UPDATE Usuarios
                SET esUsuarioProduccion = @esUsuarioProduccion
                WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = oUsuario.Id;
                    p.Add("@esUsuarioProduccion", SqlDbType.Bit).Value = oUsuario.EsUsuarioProduccion;
                }
            );
        }

        // sinRestriccionDeTenant: ignorado, ver getUsuarioById.
        public void ActualizarEstadoBloqueoLogin(Entidades.Usuario oUsuario, bool sinRestriccionDeTenant = false)
        {
            if (oUsuario == null) throw new ArgumentNullException(nameof(oUsuario));
            if (!ExisteColumnaUsuarios("bloqueado"))
                return;

            const string sql = @"
                UPDATE Usuarios
                SET intentosFallidosLogin = @intentosFallidosLogin,
                    bloqueado = @bloqueado,
                    fechaBloqueoUtc = @fechaBloqueoUtc
                WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = oUsuario.Id;
                    p.Add("@intentosFallidosLogin", SqlDbType.Int).Value = oUsuario.IntentosFallidosLogin;
                    p.Add("@bloqueado", SqlDbType.Bit).Value = oUsuario.Bloqueado;
                    p.Add("@fechaBloqueoUtc", SqlDbType.DateTime2).Value = (object)oUsuario.FechaBloqueoUtc ?? DBNull.Value;
                }
            );
        }

        public List<Entidades.PermisosUsuarios> getPermisosUsuario(int idUsuario)
        {
            const string query = @"
                SELECT 
                    f.idForm,
                    f.nombreForm,
                    f.descripcion,
                    f.formConsulta,
                    f.formEdicion,
                    f.formEdicionExtra1,
                    f.formEdicionExtra2,
                    COALESCE(p.diasPermitidosVer, -1) AS diasPermitidosVer,
                    COALESCE(p.diasPermitidosEditar, -1) AS diasPermitidosEditar,
                    CAST(COALESCE(p.soloRegistrosPropios, 1) AS bit) AS soloRegistrosPropios
                FROM Formularios f
                LEFT JOIN PermisosUsuarios p 
                    ON f.idForm = p.idForm AND p.idUsuario = @idUsuario
                ORDER BY f.idForm;";

            return Db.Reader(
                _empresa,
                query,
                CommandType.Text,
                map: dr =>
                {
                    return new Entidades.PermisosUsuarios
                    {
                        IdUsuario = idUsuario,
                        IdForm = Convert.ToInt32(dr["idForm"]),
                        DiasPermitidosVer = dr["diasPermitidosVer"] == DBNull.Value ? -1 : Convert.ToInt32(dr["diasPermitidosVer"]),
                        DiasPermitidosEditar = dr["diasPermitidosEditar"] == DBNull.Value ? -1 : Convert.ToInt32(dr["diasPermitidosEditar"]),
                        SoloRegistrosPropios = dr["soloRegistrosPropios"] == DBNull.Value ? true : Convert.ToBoolean(dr["soloRegistrosPropios"]),
                        Formulario = new Entidades.Formulario
                        {
                            IdForm = Convert.ToInt32(dr["idForm"]),
                            NombreForm = Convert.ToString(dr["nombreForm"]),
                            Descripcion = dr["descripcion"] == DBNull.Value ? "" : Convert.ToString(dr["descripcion"]),
                            FormConsulta = dr["formConsulta"] == DBNull.Value ? "" : Convert.ToString(dr["formConsulta"]),
                            FormEdicion = dr["formEdicion"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicion"]),
                            FormEdicionExtra1 = dr["formEdicionExtra1"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicionExtra1"]),
                            FormEdicionExtra2 = dr["formEdicionExtra2"] == DBNull.Value ? "" : Convert.ToString(dr["formEdicionExtra2"])
                        }
                    };
                },
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                }
            );
        }

        public void AddOrEditPermisos(List<Entidades.PermisosUsuarios> permisos)
        {
            if (permisos == null) throw new ArgumentNullException(nameof(permisos));
            if (permisos.Count == 0) return;

            const string query = @"
                IF EXISTS (SELECT 1 FROM PermisosUsuarios WHERE idUsuario = @idUsuario AND idForm = @idForm)
                BEGIN
                    UPDATE PermisosUsuarios
                    SET diasPermitidosVer = @diasVer,
                        diasPermitidosEditar = @diasEditar,
                        soloRegistrosPropios = @soloPropios
                    WHERE idUsuario = @idUsuario AND idForm = @idForm
                END
                ELSE
                BEGIN
                    INSERT INTO PermisosUsuarios (idUsuario, idForm, diasPermitidosVer, diasPermitidosEditar, soloRegistrosPropios)
                    VALUES (@idUsuario, @idForm, @diasVer, @diasEditar, @soloPropios)
                END";

            using (var con = Db.Open(_empresa))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandTimeout = Conexion.timeOut;

                var pIdUsuario = cmd.Parameters.Add("@idUsuario", SqlDbType.Int);
                var pIdForm = cmd.Parameters.Add("@idForm", SqlDbType.Int);
                var pDiasVer = cmd.Parameters.Add("@diasVer", SqlDbType.Int);
                var pDiasEditar = cmd.Parameters.Add("@diasEditar", SqlDbType.Int);
                var pSoloPropios = cmd.Parameters.Add("@soloPropios", SqlDbType.Bit);

                foreach (var permiso in permisos)
                {
                    pIdUsuario.Value = permiso.IdUsuario;
                    pIdForm.Value = permiso.IdForm;
                    pDiasVer.Value = permiso.DiasPermitidosVer;
                    pDiasEditar.Value = permiso.DiasPermitidosEditar;
                    pSoloPropios.Value = permiso.SoloRegistrosPropios;

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Entidades.Usuario> BuscarUsuariosPorIdentificador(string identificador, bool soloActivos)
        {
            identificador = (identificador ?? string.Empty).Trim();
            if (identificador.Length == 0)
                return new List<Entidades.Usuario>();

            const string sql = @"
                SELECT *
                FROM Usuarios
                WHERE (@soloActivos = 0 OR activo = 1)
                  AND (
                    LOWER(ISNULL(usuario, '')) = LOWER(@identificador)
                    OR LOWER(ISNULL(email, '')) = LOWER(@identificador)
                  )
                ORDER BY activo DESC, id ASC";

            return Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => MapUsuario(dr),
                setParams: p =>
                {
                    p.Add("@identificador", SqlDbType.NVarChar, 120).Value = identificador;
                    p.Add("@soloActivos", SqlDbType.Bit).Value = soloActivos;
                }
            );
        }

        public void ActualizarPasswordSeguro(int idUsuario, string claveLegacy, string passwordHash, string passwordSalt, int passwordHashIterations)
        {
            var setClauses = new List<string> { "clave = @clave" };

            if (ExisteColumnaUsuarios("passwordHash"))
                setClauses.Add("passwordHash = @passwordHash");

            if (ExisteColumnaUsuarios("passwordSalt"))
                setClauses.Add("passwordSalt = @passwordSalt");

            if (ExisteColumnaUsuarios("passwordHashIterations"))
                setClauses.Add("passwordHashIterations = @passwordHashIterations");

            if (ExisteColumnaUsuarios("passwordUpdatedAtUtc"))
                setClauses.Add("passwordUpdatedAtUtc = SYSUTCDATETIME()");

            string sql = "UPDATE Usuarios SET " + string.Join(", ", setClauses) + " WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                    p.Add("@clave", SqlDbType.NVarChar, 200).Value = claveLegacy ?? string.Empty;
                    p.Add("@passwordHash", SqlDbType.NVarChar, 256).Value = passwordHash ?? string.Empty;
                    p.Add("@passwordSalt", SqlDbType.NVarChar, 256).Value = passwordSalt ?? string.Empty;
                    p.Add("@passwordHashIterations", SqlDbType.Int).Value = passwordHashIterations;
                }
            );
        }

        // sinRestriccionDeTenant: ignorado, ver getUsuarioById.
        public void ActualizarPasswordWebSeguro(int idUsuario, string passwordHash, string passwordSalt, int passwordHashIterations, bool sinRestriccionDeTenant = false)
        {
            var setClauses = new List<string>();

            if (ExisteColumnaUsuarios("passwordHash"))
                setClauses.Add("passwordHash = @passwordHash");

            if (ExisteColumnaUsuarios("passwordSalt"))
                setClauses.Add("passwordSalt = @passwordSalt");

            if (ExisteColumnaUsuarios("passwordHashIterations"))
                setClauses.Add("passwordHashIterations = @passwordHashIterations");

            if (ExisteColumnaUsuarios("passwordUpdatedAtUtc"))
                setClauses.Add("passwordUpdatedAtUtc = SYSUTCDATETIME()");

            if (setClauses.Count == 0)
                throw new InvalidOperationException("La base de datos no tiene configuradas las columnas de password seguro para Web.");

            string sql = "UPDATE Usuarios SET " + string.Join(", ", setClauses) + " WHERE id = @idUsuario;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                    p.Add("@passwordHash", SqlDbType.NVarChar, 256).Value = passwordHash ?? string.Empty;
                    p.Add("@passwordSalt", SqlDbType.NVarChar, 256).Value = passwordSalt ?? string.Empty;
                    p.Add("@passwordHashIterations", SqlDbType.Int).Value = passwordHashIterations;
                }
            );
        }

        public void CrearTokenRecuperacion(Entidades.UsuarioPasswordResetToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));

            const string sql = @"
                INSERT INTO UsuarioPasswordResetTokens
                (
                    idUsuario,
                    idEmpresa,
                    tokenHash,
                    fechaCreacionUtc,
                    fechaExpiracionUtc,
                    usado,
                    fechaUsoUtc,
                    identificadorSolicitado,
                    emailDestino,
                    proposito
                )
                VALUES
                (
                    @idUsuario,
                    @idEmpresa,
                    @tokenHash,
                    @fechaCreacionUtc,
                    @fechaExpiracionUtc,
                    @usado,
                    @fechaUsoUtc,
                    @identificadorSolicitado,
                    @emailDestino,
                    @proposito
                );";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = token.IdUsuario;
                    p.Add("@idEmpresa", SqlDbType.Int).Value = token.IdEmpresa;
                    p.Add("@tokenHash", SqlDbType.NVarChar, 128).Value = token.TokenHash ?? string.Empty;
                    p.Add("@fechaCreacionUtc", SqlDbType.DateTime2).Value = token.FechaCreacionUtc;
                    p.Add("@fechaExpiracionUtc", SqlDbType.DateTime2).Value = token.FechaExpiracionUtc;
                    p.Add("@usado", SqlDbType.Bit).Value = token.Usado;
                    p.Add("@fechaUsoUtc", SqlDbType.DateTime2).Value = (object)token.FechaUsoUtc ?? DBNull.Value;
                    p.Add("@identificadorSolicitado", SqlDbType.NVarChar, 120).Value = token.IdentificadorSolicitado ?? string.Empty;
                    p.Add("@emailDestino", SqlDbType.NVarChar, 120).Value = token.EmailDestino ?? string.Empty;
                    p.Add("@proposito", SqlDbType.NVarChar, 20).Value = string.IsNullOrWhiteSpace(token.Proposito) ? "reset" : token.Proposito;
                }
            );
        }

        public Entidades.UsuarioPasswordResetToken ObtenerTokenRecuperacion(string tokenHash)
        {
            const string sql = @"
                SELECT TOP 1 *
                FROM UsuarioPasswordResetTokens
                WHERE tokenHash = @tokenHash
                ORDER BY id DESC";

            var list = Db.Reader(
                _empresa,
                sql,
                CommandType.Text,
                dr => new Entidades.UsuarioPasswordResetToken
                {
                    Id = Convert.ToInt32(dr["id"]),
                    IdUsuario = Convert.ToInt32(dr["idUsuario"]),
                    IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                    TokenHash = Convert.ToString(dr["tokenHash"]),
                    FechaCreacionUtc = Convert.ToDateTime(dr["fechaCreacionUtc"]),
                    FechaExpiracionUtc = Convert.ToDateTime(dr["fechaExpiracionUtc"]),
                    Usado = dr["usado"] != DBNull.Value && Convert.ToBoolean(dr["usado"]),
                    FechaUsoUtc = dr["fechaUsoUtc"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["fechaUsoUtc"]),
                    IdentificadorSolicitado = dr["identificadorSolicitado"] == DBNull.Value ? "" : Convert.ToString(dr["identificadorSolicitado"]),
                    EmailDestino = dr["emailDestino"] == DBNull.Value ? "" : Convert.ToString(dr["emailDestino"]),
                    Proposito = HasColumn(dr, "proposito") && dr["proposito"] != DBNull.Value ? Convert.ToString(dr["proposito"]) : "reset"
                },
                setParams: p => p.Add("@tokenHash", SqlDbType.NVarChar, 128).Value = tokenHash ?? string.Empty
            );

            return list.Count > 0 ? list[0] : null;
        }

        public void MarcarTokenRecuperacionComoUsado(int idToken)
        {
            const string sql = @"
                UPDATE UsuarioPasswordResetTokens
                SET usado = 1,
                    fechaUsoUtc = SYSUTCDATETIME()
                WHERE id = @idToken;";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@idToken", SqlDbType.Int).Value = idToken
            );
        }

        public void InvalidarTokensPendientesUsuario(int idUsuario, string proposito)
        {
            const string sql = @"
                UPDATE UsuarioPasswordResetTokens
                SET usado = 1,
                    fechaUsoUtc = ISNULL(fechaUsoUtc, SYSUTCDATETIME())
                WHERE idUsuario = @idUsuario
                  AND proposito = @proposito
                  AND usado = 0
                  AND fechaExpiracionUtc >= SYSUTCDATETIME();";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                    p.Add("@proposito", SqlDbType.NVarChar, 20).Value = string.IsNullOrWhiteSpace(proposito) ? "reset" : proposito;
                }
            );
        }

        public void RegistrarLoginUbicacion(Entidades.LoginUbicacionLog log)
        {
            if (log == null) throw new ArgumentNullException(nameof(log));

            const string sql = @"
                INSERT INTO LoginUbicacionLog
                (
                    IdUsuario,
                    IdSucursal,
                    FechaHora,
                    Latitud,
                    Longitud,
                    PrecisionMetros,
                    DistanciaMetros,
                    Permitido,
                    Motivo,
                    Ip
                )
                VALUES
                (
                    @IdUsuario,
                    @IdSucursal,
                    @FechaHora,
                    @Latitud,
                    @Longitud,
                    @PrecisionMetros,
                    @DistanciaMetros,
                    @Permitido,
                    @Motivo,
                    @Ip
                );";

            Db.NonQuery(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@IdUsuario", SqlDbType.Int).Value = log.IdUsuario;
                    p.Add("@IdSucursal", SqlDbType.Int).Value = log.IdSucursal;
                    p.Add("@FechaHora", SqlDbType.DateTime).Value = log.FechaHora;
                    p.Add("@Latitud", SqlDbType.Decimal).Value = (object)log.Latitud ?? DBNull.Value;
                    p.Add("@Longitud", SqlDbType.Decimal).Value = (object)log.Longitud ?? DBNull.Value;
                    p.Add("@PrecisionMetros", SqlDbType.Decimal).Value = (object)log.PrecisionMetros ?? DBNull.Value;
                    p.Add("@DistanciaMetros", SqlDbType.Decimal).Value = (object)log.DistanciaMetros ?? DBNull.Value;
                    p.Add("@Permitido", SqlDbType.Bit).Value = log.Permitido;
                    p.Add("@Motivo", SqlDbType.NVarChar, 300).Value = log.Motivo ?? string.Empty;
                    p.Add("@Ip", SqlDbType.NVarChar, 100).Value = log.Ip ?? string.Empty;

                    p["@Latitud"].Precision = 10;
                    p["@Latitud"].Scale = 7;
                    p["@Longitud"].Precision = 10;
                    p["@Longitud"].Scale = 7;
                    p["@PrecisionMetros"].Precision = 10;
                    p["@PrecisionMetros"].Scale = 2;
                    p["@DistanciaMetros"].Precision = 10;
                    p["@DistanciaMetros"].Scale = 2;
                }
            );
        }

        // Auditoria de accesos (pantalla "Auditoria de accesos", gateada por el permiso de crear
        // usuarios -- ver AuditoriaLoginController). Antes de esto la tabla LoginUbicacionLog solo
        // se escribia, nunca se leia desde ningun lado. TOP 500 como limite defensivo: es un
        // listado de auditoria para revisar a ojo, no un reporte exportable sin paginar.
        public DataTable obtenerLoginUbicacionLog(int idEmpresa, DateTime desde, DateTime hasta)
        {
            const string sql = @"
                SELECT TOP 500
                    l.IdUsuario,
                    u.nombre AS UsuarioNombre,
                    l.IdSucursal,
                    s.sucursal AS SucursalNombre,
                    l.FechaHora,
                    l.Latitud,
                    l.Longitud,
                    l.PrecisionMetros,
                    l.DistanciaMetros,
                    l.Permitido,
                    l.Motivo,
                    l.Ip
                FROM LoginUbicacionLog l
                INNER JOIN Usuarios u ON u.id = l.IdUsuario
                LEFT JOIN Sucursal s ON s.idSucursal = l.IdSucursal
                WHERE u.idEmpresa = @idEmpresa
                  AND l.FechaHora BETWEEN @desde AND @hasta
                ORDER BY l.FechaHora DESC;";

            return Db.DataTable(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                    p.Add("@desde", SqlDbType.DateTime).Value = desde;
                    p.Add("@hasta", SqlDbType.DateTime).Value = hasta;
                }
            );
        }

        private Entidades.Usuario MapUsuario(SqlDataReader dr)
        {
            return new Entidades.Usuario
            {
                Id = Convert.ToInt32(dr["id"]),
                Nombre = dr["nombre"] == DBNull.Value ? "" : Convert.ToString(dr["nombre"]),
                User = dr["usuario"] == DBNull.Value ? "" : Convert.ToString(dr["usuario"]),
                Clave = dr["clave"] == DBNull.Value ? "" : Convert.ToString(dr["clave"]),
                Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                PasswordHash = GetOptionalString(dr, "passwordHash"),
                PasswordSalt = GetOptionalString(dr, "passwordSalt"),
                PasswordHashIterations = GetOptionalInt(dr, "passwordHashIterations"),
                PasswordUpdatedAtUtc = GetOptionalDateTime(dr, "passwordUpdatedAtUtc"),
                Admin = dr["admin"] != DBNull.Value && Convert.ToBoolean(dr["admin"]),
                Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"]),
                ColorForm = dr["colorForm"] == DBNull.Value ? "" : Convert.ToString(dr["colorForm"]),
                IdSucursal = dr["idSucursalUser"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursalUser"]),
                IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                PermitirLoginFueraSucursal = GetOptionalBool(dr, "PermitirLoginFueraSucursal"),
                EsUsuarioProduccion = GetOptionalBool(dr, "esUsuarioProduccion"),
                IntentosFallidosLogin = GetOptionalInt(dr, "intentosFallidosLogin"),
                Bloqueado = GetOptionalBool(dr, "bloqueado"),
                FechaBloqueoUtc = GetOptionalDateTime(dr, "fechaBloqueoUtc")
            };
        }

        private static bool HasColumn(IDataRecord dr, string columnName)
        {
            for (var i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string GetOptionalString(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return string.Empty;

            object value = dr[columnName];
            return value == DBNull.Value ? string.Empty : Convert.ToString(value);
        }

        private static int GetOptionalInt(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return 0;

            object value = dr[columnName];
            return value == DBNull.Value ? 0 : Convert.ToInt32(value);
        }

        private static DateTime? GetOptionalDateTime(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return null;

            object value = dr[columnName];
            return value == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(value);
        }

        private static bool GetOptionalBool(IDataRecord dr, string columnName)
        {
            if (!HasColumn(dr, columnName))
                return false;

            object value = dr[columnName];
            return value != DBNull.Value && Convert.ToBoolean(value);
        }

        private bool ExisteColumnaUsuarios(string columnName)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM sys.columns
                WHERE object_id = OBJECT_ID('dbo.Usuarios')
                  AND name = @columnName;";

            object result = Db.Scalar(
                _empresa,
                sql,
                CommandType.Text,
                setParams: p => p.Add("@columnName", SqlDbType.NVarChar, 128).Value = columnName ?? string.Empty
            );

            return result != null && result != DBNull.Value && Convert.ToInt32(result) > 0;
        }
    }
}
