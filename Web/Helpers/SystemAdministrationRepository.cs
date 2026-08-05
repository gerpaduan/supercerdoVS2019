using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Utilidades;
using Web.Models;

namespace Web.Helpers
{
    public class SystemAdministrationRepository
    {
        private readonly IEmpresaContext _empresaRaiz;

        public SystemAdministrationRepository()
        {
            _empresaRaiz = new EmpresaContextNulo();
        }

        public bool EsSuperAdmin(int idUsuario)
        {
            if (idUsuario <= 0)
                return false;

            using (var con = Db.OpenAdmin(_empresaRaiz))
            {
                string columnName = GetExistingColumnName(con, null, "Usuarios", "superadmin");
                if (string.IsNullOrWhiteSpace(columnName))
                    return false;

                using (var cmd = new SqlCommand("SELECT CAST(ISNULL(" + QuoteSqlIdentifier(columnName) + ", 0) AS bit) FROM dbo.Usuarios WHERE id = @id", con))
                {
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = idUsuario;
                    object value = cmd.ExecuteScalar();
                    return value != null && value != DBNull.Value && Convert.ToBoolean(value);
                }
            }
        }

        private static string QuoteSqlIdentifier(string identifier)
        {
            if (string.IsNullOrWhiteSpace(identifier) || !Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
                throw new InvalidOperationException("El identificador SQL no es válido.");

            return "[" + identifier.Replace("]", "]]") + "]";
        }

        public bool TablaSucursalTieneTelefono()
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            {
                return !string.IsNullOrWhiteSpace(GetExistingColumnName(con, null, "Sucursal", "telefono"));
            }
        }

        public bool TablaSucursalTieneActiva()
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            {
                return !string.IsNullOrWhiteSpace(GetExistingColumnName(con, null, "Sucursal", "activa"));
            }
        }

        public List<SystemAdministrationEmpresaResumenVm> ObtenerEmpresas()
        {
            const string sql = @"
                SELECT idEmpresa, razonSocialAfip, nombreFantasia, cuit, condicionIVA, telefono, email, activa
                FROM dbo.Empresas
                ORDER BY activa DESC, razonSocialAfip ASC, idEmpresa ASC;";

            return Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationEmpresaResumenVm
                {
                    IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                    RazonSocialAfip = dr["razonSocialAfip"] == DBNull.Value ? "" : Convert.ToString(dr["razonSocialAfip"]),
                    NombreFantasia = dr["nombreFantasia"] == DBNull.Value ? "" : Convert.ToString(dr["nombreFantasia"]),
                    Cuit = dr["cuit"] == DBNull.Value ? 0 : Convert.ToInt64(dr["cuit"]),
                    CondicionIVA = dr["condicionIVA"] == DBNull.Value ? "" : Convert.ToString(dr["condicionIVA"]),
                    Telefono = dr["telefono"] == DBNull.Value ? "" : Convert.ToString(dr["telefono"]),
                    Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                    Activa = dr["activa"] != DBNull.Value && Convert.ToByte(dr["activa"]) == 1
                }
            ,
                openConnection: Db.OpenAdmin
            );
        }

        public SystemAdministrationEmpresaEditVm ObtenerEmpresa(int idEmpresa)
        {
            const string sql = "SELECT TOP 1 * FROM dbo.Empresas WHERE idEmpresa = @idEmpresa";

            var list = Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationEmpresaEditVm
                {
                    EsEdicion = true,
                    IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                    RazonSocialAfip = dr["razonSocialAfip"] == DBNull.Value ? "" : Convert.ToString(dr["razonSocialAfip"]),
                    Cuit = dr["cuit"] == DBNull.Value ? "" : Convert.ToString(dr["cuit"]),
                    NombreFantasia = dr["nombreFantasia"] == DBNull.Value ? "" : Convert.ToString(dr["nombreFantasia"]),
                    Slogan1 = dr["slogan1"] == DBNull.Value ? "" : Convert.ToString(dr["slogan1"]),
                    Slogan2 = dr["slogan2"] == DBNull.Value ? "" : Convert.ToString(dr["slogan2"]),
                    Slogan3 = dr["slogan3"] == DBNull.Value ? "" : Convert.ToString(dr["slogan3"]),
                    Iibb = dr["iibb"] == DBNull.Value ? "" : Convert.ToString(dr["iibb"]),
                    CondicionIVA = dr["condicionIVA"] == DBNull.Value ? "" : Convert.ToString(dr["condicionIVA"]),
                    InicioActividad = dr["inicioActividad"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(dr["inicioActividad"]),
                    TenantSlug = dr["tenantSlug"] == DBNull.Value ? "" : Convert.ToString(dr["tenantSlug"]),
                    Domicilio = dr["domicilio"] == DBNull.Value ? "" : Convert.ToString(dr["domicilio"]),
                    Ciudad = dr["ciudad"] == DBNull.Value ? "" : Convert.ToString(dr["ciudad"]),
                    Pais = dr["pais"] == DBNull.Value ? "" : Convert.ToString(dr["pais"]),
                    Telefono = dr["telefono"] == DBNull.Value ? "" : Convert.ToString(dr["telefono"]),
                    Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                    BasePath = dr["basePath"] == DBNull.Value ? "" : Convert.ToString(dr["basePath"]),
                    EsRRII = dr["esRRII"] != DBNull.Value && Convert.ToByte(dr["esRRII"]) == 1,
                    NombreCertificadoPfx = dr["nombreCertificado_pfx"] == DBNull.Value ? "" : Convert.ToString(dr["nombreCertificado_pfx"]),
                    EntornoHomoProd = dr["entorno_HOMO_PROD"] == DBNull.Value ? "" : Convert.ToString(dr["entorno_HOMO_PROD"]),
                    BaseDatosNombre = dr["baseDatosNombre"] == DBNull.Value ? "" : Convert.ToString(dr["baseDatosNombre"]),
                    Activa = dr["activa"] != DBNull.Value && Convert.ToByte(dr["activa"]) == 1,
                    Observaciones = HasColumn(dr, "observaciones") && dr["observaciones"] != DBNull.Value ? Convert.ToString(dr["observaciones"]) : ""
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa,
                openConnection: Db.OpenAdmin
            );

            return list.FirstOrDefault();
        }

        public int CrearEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    int idEmpresa = CrearEmpresaInterna(con, tx, model);
                    tx.Commit();
                    return idEmpresa;
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }
        }

        public void ActualizarEmpresa(SystemAdministrationEmpresaEditVm model)
        {
            const string sql = @"
                UPDATE dbo.Empresas
                SET razonSocialAfip = @razonSocialAfip,
                    cuit = @cuit,
                    nombreFantasia = @nombreFantasia,
                    slogan1 = @slogan1,
                    slogan2 = @slogan2,
                    slogan3 = @slogan3,
                    iibb = @iibb,
                    condicionIVA = @condicionIVA,
                    inicioActividad = @inicioActividad,
                    tenantSlug = @tenantSlug,
                    domicilio = @domicilio,
                    ciudad = @ciudad,
                    pais = @pais,
                    telefono = @telefono,
                    email = @email,
                    basePath = @basePath,
                    esRRII = @esRRII,
                    nombreCertificado_pfx = @nombreCertificado_pfx,
                    entorno_HOMO_PROD = @entorno_HOMO_PROD,
                    baseDatosNombre = @baseDatosNombre,
                    activa = @activa,
                    observaciones = @observaciones
                WHERE idEmpresa = @idEmpresa;";

            Db.NonQuery(
                _empresaRaiz,
                sql,
                CommandType.Text,
                setParams: p => SetEmpresaParams(p, model, false),
                openConnection: Db.OpenAdmin
            );
        }

        public List<SystemAdministrationSucursalResumenVm> ObtenerSucursales(int idEmpresa = 0)
        {
            string sql = @"
                SELECT s.*, e.razonSocialAfip
                FROM dbo.Sucursal s
                INNER JOIN dbo.Empresas e ON e.idEmpresa = s.idEmpresa
                WHERE (@idEmpresa = 0 OR s.idEmpresa = @idEmpresa)
                ORDER BY e.razonSocialAfip ASC, s.sucursal ASC, s.idSucursal ASC;";

            bool tieneTelefono = TablaSucursalTieneTelefono();
            bool tieneActiva = TablaSucursalTieneActiva();

            return Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationSucursalResumenVm
                {
                    IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                    IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                    EmpresaNombre = dr["razonSocialAfip"] == DBNull.Value ? "" : Convert.ToString(dr["razonSocialAfip"]),
                    Sucursal = dr["sucursal"] == DBNull.Value ? "" : Convert.ToString(dr["sucursal"]),
                    Direccion = dr["direccion"] == DBNull.Value ? "" : Convert.ToString(dr["direccion"]),
                    Localidad = dr["localidad"] == DBNull.Value ? "" : Convert.ToString(dr["localidad"]),
                    CodPuntoVentaAfip = dr["codPuntoVentaAfip"] == DBNull.Value ? 0 : Convert.ToInt32(dr["codPuntoVentaAfip"]),
                    Telefono = tieneTelefono && HasColumn(dr, "telefono") && dr["telefono"] != DBNull.Value ? Convert.ToString(dr["telefono"]) : "",
                    Activa = tieneActiva && HasColumn(dr, "activa") ? dr["activa"] != DBNull.Value && Convert.ToByte(dr["activa"]) == 1 : true
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa,
                openConnection: Db.OpenAdmin
            );
        }

        public SystemAdministrationSucursalEditVm ObtenerSucursal(int idSucursal)
        {
            bool tieneTelefono = TablaSucursalTieneTelefono();
            bool tieneActiva = TablaSucursalTieneActiva();

            const string sql = "SELECT TOP 1 * FROM dbo.Sucursal WHERE idSucursal = @idSucursal";
            var list = Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationSucursalEditVm
                {
                    EsEdicion = true,
                    IdSucursal = Convert.ToInt32(dr["idSucursal"]),
                    IdEmpresa = Convert.ToInt32(dr["idEmpresa"]),
                    Sucursal = dr["sucursal"] == DBNull.Value ? "" : Convert.ToString(dr["sucursal"]),
                    Direccion = dr["direccion"] == DBNull.Value ? "" : Convert.ToString(dr["direccion"]),
                    Localidad = dr["localidad"] == DBNull.Value ? "" : Convert.ToString(dr["localidad"]),
                    Provincia = dr["provincia"] == DBNull.Value ? "" : Convert.ToString(dr["provincia"]),
                    Pais = dr["pais"] == DBNull.Value ? "" : Convert.ToString(dr["pais"]),
                    CodPuntoVentaAfip = dr["codPuntoVentaAfip"] == DBNull.Value ? (int?)null : Convert.ToInt32(dr["codPuntoVentaAfip"]),
                    Telefono = tieneTelefono && HasColumn(dr, "telefono") && dr["telefono"] != DBNull.Value ? Convert.ToString(dr["telefono"]) : "",
                    Activa = tieneActiva && HasColumn(dr, "activa") ? dr["activa"] != DBNull.Value && Convert.ToByte(dr["activa"]) == 1 : true,
                    Observaciones = HasColumn(dr, "observaciones") && dr["observaciones"] != DBNull.Value ? Convert.ToString(dr["observaciones"]) : "",
                    TieneTelefono = tieneTelefono,
                    TieneActiva = tieneActiva
                },
                setParams: p => p.Add("@idSucursal", SqlDbType.Int).Value = idSucursal,
                openConnection: Db.OpenAdmin
            );

            return list.FirstOrDefault();
        }

        public int CrearSucursal(SystemAdministrationSucursalEditVm model)
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                var columns = GetTableColumns(con, tx, "Sucursal");
                var columnNames = new List<string> { "sucursal", "idEmpresa", "direccion", "localidad", "provincia", "pais", "codPuntoVentaAfip" };
                var valueNames = new List<string> { "@sucursal", "@idEmpresa", "@direccion", "@localidad", "@provincia", "@pais", "@codPuntoVentaAfip" };

                if (columns.Contains("telefono"))
                {
                    columnNames.Add("telefono");
                    valueNames.Add("@telefono");
                }

                if (columns.Contains("activa"))
                {
                    columnNames.Add("activa");
                    valueNames.Add("@activa");
                }

                if (columns.Contains("creado"))
                {
                    columnNames.Add("creado");
                    valueNames.Add("@creado");
                }

                if (columns.Contains("observaciones"))
                {
                    columnNames.Add("observaciones");
                    valueNames.Add("@observaciones");
                }

                string sql = "INSERT INTO dbo.Sucursal (" + string.Join(", ", columnNames) + ") OUTPUT INSERTED.idSucursal VALUES (" + string.Join(", ", valueNames) + ");";

                int idSucursalNueva;
                using (var cmd = new SqlCommand(sql, con, tx))
                {
                    SetSucursalParams(cmd.Parameters, model, columns);
                    object value = cmd.ExecuteScalar();
                    idSucursalNueva = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
                }

                if (idSucursalNueva > 0)
                {
                    SembrarPuntoStockSucursalNueva(con, tx, model.IdEmpresa, idSucursalNueva, model.IdSucursalOrigenPuntoStock);
                }

                tx.Commit();
                return idSucursalNueva;
            }
        }

        // Al crear una sucursal nueva, cada producto existente de la empresa necesita una fila
        // en dbo.CortePuntoStockSucursal para esa sucursal (ver Datos/CortePuntoStockSucursal.cs,
        // caso simetrico para el alta de producto). Corre en la misma transaccion que el INSERT
        // de la sucursal: si algo falla, no queda ni la sucursal ni los puntos de stock a medias.
        private void SembrarPuntoStockSucursalNueva(SqlConnection con, SqlTransaction tx, int idEmpresa, int idSucursalNueva, int? idSucursalOrigen)
        {
            bool copiarDeSucursalExistente = idSucursalOrigen.HasValue && idSucursalOrigen.Value > 0;

            string sql = copiarDeSucursalExistente
                ? @"
                    INSERT INTO dbo.CortePuntoStockSucursal (idEmpresa, idCorte, idSucursal, puntoStock)
                    SELECT c.idEmpresa, c.idCorte, @idSucursalNueva, ISNULL(origen.puntoStock, 0)
                    FROM dbo.Corte c
                    LEFT JOIN dbo.CortePuntoStockSucursal origen
                        ON origen.idCorte = c.idCorte AND origen.idSucursal = @idSucursalOrigen
                    WHERE c.idEmpresa = @idEmpresa;"
                : @"
                    INSERT INTO dbo.CortePuntoStockSucursal (idEmpresa, idCorte, idSucursal, puntoStock)
                    SELECT c.idEmpresa, c.idCorte, @idSucursalNueva, 0
                    FROM dbo.Corte c
                    WHERE c.idEmpresa = @idEmpresa;";

            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.CommandTimeout = Conexion.timeOut;
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                cmd.Parameters.Add("@idSucursalNueva", SqlDbType.Int).Value = idSucursalNueva;
                if (copiarDeSucursalExistente)
                {
                    cmd.Parameters.Add("@idSucursalOrigen", SqlDbType.Int).Value = idSucursalOrigen.Value;
                }

                cmd.ExecuteNonQuery();
            }
        }

        public void ActualizarSucursal(SystemAdministrationSucursalEditVm model)
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                var columns = GetTableColumns(con, tx, "Sucursal");
                var sets = new List<string>
                {
                    "sucursal = @sucursal",
                    "idEmpresa = @idEmpresa",
                    "direccion = @direccion",
                    "localidad = @localidad",
                    "provincia = @provincia",
                    "pais = @pais",
                    "codPuntoVentaAfip = @codPuntoVentaAfip"
                };

                if (columns.Contains("telefono"))
                    sets.Add("telefono = @telefono");
                if (columns.Contains("activa"))
                    sets.Add("activa = @activa");
                if (columns.Contains("observaciones"))
                    sets.Add("observaciones = @observaciones");

                string sql = "UPDATE dbo.Sucursal SET " + string.Join(", ", sets) + " WHERE idSucursal = @idSucursal;";

                using (var cmd = new SqlCommand(sql, con, tx))
                {
                    SetSucursalParams(cmd.Parameters, model, columns);
                    cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = model.IdSucursal;
                    cmd.ExecuteNonQuery();
                    tx.Commit();
                }
            }
        }

        public List<SystemAdministrationUsuarioResumenVm> ObtenerUsuarios(int idEmpresa = 0)
        {
            const string sql = @"
                SELECT u.*, e.razonSocialAfip, s.sucursal
                FROM dbo.Usuarios u
                LEFT JOIN dbo.Empresas e ON e.idEmpresa = u.idEmpresa
                LEFT JOIN dbo.Sucursal s ON s.idSucursal = u.idSucursalUser
                WHERE (@idEmpresa = 0 OR u.idEmpresa = @idEmpresa)
                ORDER BY e.razonSocialAfip ASC, u.nombre ASC, u.usuario ASC;";

            return Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationUsuarioResumenVm
                {
                    Id = Convert.ToInt32(dr["id"]),
                    IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                    EmpresaNombre = dr["razonSocialAfip"] == DBNull.Value ? "" : Convert.ToString(dr["razonSocialAfip"]),
                    IdSucursalUser = dr["idSucursalUser"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursalUser"]),
                    SucursalNombre = dr["sucursal"] == DBNull.Value ? "" : Convert.ToString(dr["sucursal"]),
                    Nombre = dr["nombre"] == DBNull.Value ? "" : Convert.ToString(dr["nombre"]),
                    Usuario = dr["usuario"] == DBNull.Value ? "" : Convert.ToString(dr["usuario"]),
                    Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                    Admin = dr["admin"] != DBNull.Value && Convert.ToBoolean(dr["admin"]),
                    Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"])
                },
                setParams: p => p.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa,
                openConnection: Db.OpenAdmin
            );
        }

        public SystemAdministrationUsuarioEditVm ObtenerUsuario(int idUsuario)
        {
            const string sql = "SELECT TOP 1 * FROM dbo.Usuarios WHERE id = @id";

            var list = Db.Reader(
                _empresaRaiz,
                sql,
                CommandType.Text,
                dr => new SystemAdministrationUsuarioEditVm
                {
                    EsEdicion = true,
                    Id = Convert.ToInt32(dr["id"]),
                    Nombre = dr["nombre"] == DBNull.Value ? "" : Convert.ToString(dr["nombre"]),
                    Usuario = dr["usuario"] == DBNull.Value ? "" : Convert.ToString(dr["usuario"]),
                    Email = dr["email"] == DBNull.Value ? "" : Convert.ToString(dr["email"]),
                    IdEmpresa = dr["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idEmpresa"]),
                    IdSucursalUser = dr["idSucursalUser"] == DBNull.Value ? 0 : Convert.ToInt32(dr["idSucursalUser"]),
                    Admin = dr["admin"] != DBNull.Value && Convert.ToBoolean(dr["admin"]),
                    Activo = dr["activo"] != DBNull.Value && Convert.ToBoolean(dr["activo"]),
                    PermitirLoginFueraSucursal = HasColumn(dr, "PermitirLoginFueraSucursal") && dr["PermitirLoginFueraSucursal"] != DBNull.Value && Convert.ToBoolean(dr["PermitirLoginFueraSucursal"])
                },
                setParams: p => p.Add("@id", SqlDbType.Int).Value = idUsuario,
                openConnection: Db.OpenAdmin
            );

            return list.FirstOrDefault();
        }

        public int CrearUsuario(SystemAdministrationUsuarioEditVm model)
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                var columns = GetTableColumns(con, tx, "Usuarios");
                int idUsuario = CrearUsuarioInterno(con, tx, columns, model);
                tx.Commit();
                return idUsuario;
            }
        }

        public void ActualizarUsuario(SystemAdministrationUsuarioEditVm model)
        {
            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                var columns = GetTableColumns(con, tx, "Usuarios");
                var sets = new List<string>
                {
                    "nombre = @nombre",
                    "usuario = @usuario",
                    "email = @email",
                    "admin = @admin",
                    "activo = @activo",
                    "idEmpresa = @idEmpresa",
                    "idSucursalUser = @idSucursalUser"
                };

                if (columns.Contains("colorform"))
                    sets.Add("colorForm = @colorForm");

                if (columns.Contains("permitirloginfuerasucursal"))
                    sets.Add("PermitirLoginFueraSucursal = @permitirLoginFueraSucursal");

                if (!string.IsNullOrWhiteSpace(model.Clave))
                    sets.Add("clave = @clave");

                string sql = "UPDATE dbo.Usuarios SET " + string.Join(", ", sets) + " WHERE id = @id;";

                using (var cmd = new SqlCommand(sql, con, tx))
                {
                    SetUsuarioParams(cmd.Parameters, model, columns, string.IsNullOrWhiteSpace(model.Clave));
                    cmd.Parameters.Add("@id", SqlDbType.Int).Value = model.Id;
                    cmd.ExecuteNonQuery();
                }

                if (!string.IsNullOrWhiteSpace(model.Clave))
                    ActualizarPasswordWebSeguro(con, tx, columns, model.Id, model.Clave);

                tx.Commit();
            }
        }

        public int CrearAltaRapida(SystemAdministrationAltaRapidaVm model)
        {
            int idEmpresa;

            using (var con = Db.OpenAdmin(_empresaRaiz))
            using (var tx = con.BeginTransaction())
            {
                try
                {
                    idEmpresa = CrearEmpresaInterna(con, tx, model.Empresa);
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    throw;
                }
            }

            using (var conEmpresa = Db.Open(new EmpresaContextFijo(idEmpresa)))
            using (var txEmpresa = conEmpresa.BeginTransaction())
            {
                try
                {
                    int idSucursal = ObtenerSucursalDefaultEmpresa(conEmpresa, txEmpresa, idEmpresa);
                    if (idSucursal <= 0)
                        throw new InvalidOperationException("No se pudo generar la sucursal inicial de la empresa.");

                    model.Sucursal.IdSucursal = idSucursal;
                    model.Sucursal.IdEmpresa = idEmpresa;
                    ActualizarSucursalInterna(conEmpresa, txEmpresa, model.Sucursal);

                    var columnsUsuarios = GetTableColumns(conEmpresa, txEmpresa, "Usuarios");
                    model.Usuario.IdEmpresa = idEmpresa;
                    model.Usuario.IdSucursalUser = idSucursal;
                    int idUsuario = CrearUsuarioInterno(conEmpresa, txEmpresa, columnsUsuarios, model.Usuario);

                    txEmpresa.Commit();
                    return idUsuario;
                }
                catch
                {
                    txEmpresa.Rollback();
                    throw;
                }
            }
        }

        public bool ExisteCuit(long cuit, int idEmpresaExcluir)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Empresas
                WHERE cuit = @cuit
                  AND (@idEmpresaExcluir <= 0 OR idEmpresa <> @idEmpresaExcluir);";

            object value = Db.Scalar(
                _empresaRaiz,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@cuit", SqlDbType.BigInt).Value = cuit;
                    p.Add("@idEmpresaExcluir", SqlDbType.Int).Value = idEmpresaExcluir;
                },
                openConnection: Db.OpenAdmin
            );

            return value != null && value != DBNull.Value && Convert.ToInt32(value) > 0;
        }

        public bool ExisteUsuario(string usuario, int idUsuarioExcluir)
        {
            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Usuarios
                WHERE LOWER(ISNULL(usuario, '')) = LOWER(@usuario)
                  AND (@idUsuarioExcluir <= 0 OR id <> @idUsuarioExcluir);";

            object value = Db.Scalar(
                _empresaRaiz,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@usuario", SqlDbType.NVarChar, 100).Value = usuario ?? "";
                    p.Add("@idUsuarioExcluir", SqlDbType.Int).Value = idUsuarioExcluir;
                },
                openConnection: Db.OpenAdmin
            );

            return value != null && value != DBNull.Value && Convert.ToInt32(value) > 0;
        }

        public bool ExisteEmail(string email, int idUsuarioExcluir)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            const string sql = @"
                SELECT COUNT(1)
                FROM dbo.Usuarios
                WHERE LOWER(ISNULL(email, '')) = LOWER(@email)
                  AND (@idUsuarioExcluir <= 0 OR id <> @idUsuarioExcluir);";

            object value = Db.Scalar(
                _empresaRaiz,
                sql,
                CommandType.Text,
                setParams: p =>
                {
                    p.Add("@email", SqlDbType.NVarChar, 150).Value = email ?? "";
                    p.Add("@idUsuarioExcluir", SqlDbType.Int).Value = idUsuarioExcluir;
                },
                openConnection: Db.OpenAdmin
            );

            return value != null && value != DBNull.Value && Convert.ToInt32(value) > 0;
        }

        public List<SelectListItem> ObtenerEmpresasSelectList(int idSeleccionado = 0, bool incluirTodas = false)
        {
            var items = ObtenerEmpresas()
                .Select(x => new SelectListItem
                {
                    Value = x.IdEmpresa.ToString(),
                    Text = string.IsNullOrWhiteSpace(x.NombreFantasia) ? x.RazonSocialAfip : (x.NombreFantasia + " (" + x.RazonSocialAfip + ")"),
                    Selected = x.IdEmpresa == idSeleccionado
                })
                .ToList();

            if (incluirTodas)
            {
                items.Insert(0, new SelectListItem
                {
                    Value = "0",
                    Text = "Todas",
                    Selected = idSeleccionado <= 0
                });
            }

            return items;
        }

        public List<SelectListItem> ObtenerSucursalesSelectList(int idEmpresa, int idSeleccionado = 0)
        {
            return ObtenerSucursales(idEmpresa)
                .Where(x => idEmpresa <= 0 || x.IdEmpresa == idEmpresa)
                .Select(x => new SelectListItem
                {
                    Value = x.IdSucursal.ToString(),
                    Text = x.Sucursal,
                    Selected = x.IdSucursal == idSeleccionado
                })
                .ToList();
        }

        private int CrearEmpresaInterna(SqlConnection con, SqlTransaction tx, SystemAdministrationEmpresaEditVm model)
        {
            using (var cmd = new SqlCommand("dbo.AA_AltaEmpresa", con, tx))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = Conexion.timeOut;
                SetEmpresaParams(cmd.Parameters, model, true);

                var output = cmd.Parameters.Add("@idEmpresa", SqlDbType.Int);
                output.Direction = ParameterDirection.Output;

                cmd.ExecuteNonQuery();

                return output.Value == null || output.Value == DBNull.Value ? 0 : Convert.ToInt32(output.Value);
            }
        }

        private int ObtenerSucursalDefaultEmpresa(SqlConnection con, SqlTransaction tx, int idEmpresa)
        {
            using (var cmd = new SqlCommand("SELECT TOP 1 idSucursal FROM dbo.Sucursal WHERE idEmpresa = @idEmpresa ORDER BY idSucursal DESC", con, tx))
            {
                cmd.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = idEmpresa;
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }
        }

        private void ActualizarSucursalInterna(SqlConnection con, SqlTransaction tx, SystemAdministrationSucursalEditVm model)
        {
            var columns = GetTableColumns(con, tx, "Sucursal");
            var sets = new List<string>
            {
                "sucursal = @sucursal",
                "idEmpresa = @idEmpresa",
                "direccion = @direccion",
                "localidad = @localidad",
                "provincia = @provincia",
                "pais = @pais",
                "codPuntoVentaAfip = @codPuntoVentaAfip"
            };

            if (columns.Contains("telefono"))
                sets.Add("telefono = @telefono");
            if (columns.Contains("activa"))
                sets.Add("activa = @activa");
            if (columns.Contains("observaciones"))
                sets.Add("observaciones = @observaciones");

            string sql = "UPDATE dbo.Sucursal SET " + string.Join(", ", sets) + " WHERE idSucursal = @idSucursal;";

            using (var cmd = new SqlCommand(sql, con, tx))
            {
                SetSucursalParams(cmd.Parameters, model, columns);
                cmd.Parameters.Add("@idSucursal", SqlDbType.Int).Value = model.IdSucursal;
                cmd.ExecuteNonQuery();
            }
        }

        private int CrearUsuarioInterno(SqlConnection con, SqlTransaction tx, HashSet<string> columns, SystemAdministrationUsuarioEditVm model)
        {
            var columnNames = new List<string>
            {
                "id",
                "nombre",
                "usuario",
                "email",
                "clave",
                "admin",
                "activo",
                "idEmpresa",
                "idSucursalUser"
            };

            var valueNames = new List<string>
            {
                "@id",
                "@nombre",
                "@usuario",
                "@email",
                "@clave",
                "@admin",
                "@activo",
                "@idEmpresa",
                "@idSucursalUser"
            };

            if (columns.Contains("colorform"))
            {
                columnNames.Add("colorForm");
                valueNames.Add("@colorForm");
            }

            if (columns.Contains("permitirloginfuerasucursal"))
            {
                columnNames.Add("PermitirLoginFueraSucursal");
                valueNames.Add("@permitirLoginFueraSucursal");
            }

            string sql = "INSERT INTO dbo.Usuarios (" + string.Join(", ", columnNames) + ") OUTPUT INSERTED.id VALUES (" + string.Join(", ", valueNames) + ");";

            int idUsuario;
            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = ObtenerProximoIdUsuario(con, tx);
                SetUsuarioParams(cmd.Parameters, model, columns, false);
                object value = cmd.ExecuteScalar();
                idUsuario = value == null || value == DBNull.Value ? 0 : Convert.ToInt32(value);
            }

            if (idUsuario > 0 && !string.IsNullOrWhiteSpace(model.Clave))
                ActualizarPasswordWebSeguro(con, tx, columns, idUsuario, model.Clave);

            return idUsuario;
        }

        private int ObtenerProximoIdUsuario(SqlConnection con, SqlTransaction tx)
        {
            using (var cmd = new SqlCommand("SELECT ISNULL(MAX(id), 0) + 1 FROM dbo.Usuarios WITH (UPDLOCK, HOLDLOCK)", con, tx))
            {
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? 1 : Convert.ToInt32(value);
            }
        }

        private void ActualizarPasswordWebSeguro(SqlConnection con, SqlTransaction tx, HashSet<string> columns, int idUsuario, string clave)
        {
            if (!columns.Contains("passwordhash") || !columns.Contains("passwordsalt") || !columns.Contains("passwordhashiterations"))
                return;

            var hash = PasswordSecurity.HashPassword((clave ?? "").Trim());
            var sets = new List<string>
            {
                "passwordHash = @passwordHash",
                "passwordSalt = @passwordSalt",
                "passwordHashIterations = @passwordHashIterations"
            };

            if (columns.Contains("passwordupdatedatutc"))
                sets.Add("passwordUpdatedAtUtc = SYSUTCDATETIME()");

            string sql = "UPDATE dbo.Usuarios SET " + string.Join(", ", sets) + " WHERE id = @idUsuario;";

            using (var cmd = new SqlCommand(sql, con, tx))
            {
                cmd.Parameters.Add("@idUsuario", SqlDbType.Int).Value = idUsuario;
                cmd.Parameters.Add("@passwordHash", SqlDbType.NVarChar, 256).Value = hash.Hash;
                cmd.Parameters.Add("@passwordSalt", SqlDbType.NVarChar, 256).Value = hash.Salt;
                cmd.Parameters.Add("@passwordHashIterations", SqlDbType.Int).Value = hash.Iterations;
                cmd.ExecuteNonQuery();
            }
        }

        private void SetEmpresaParams(SqlParameterCollection p, SystemAdministrationEmpresaEditVm model, bool incluirSalida)
        {
            if (!incluirSalida)
                p.Add("@idEmpresa", SqlDbType.Int).Value = model.IdEmpresa;

            p.Add("@razonSocialAfip", SqlDbType.NVarChar, 100).Value = (object)(model.RazonSocialAfip ?? "").Trim() ?? DBNull.Value;
            p.Add("@cuit", SqlDbType.BigInt).Value = ParseLong(model.Cuit);
            p.Add("@nombreFantasia", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.NombreFantasia, (model.RazonSocialAfip ?? "").Trim());
            p.Add("@slogan1", SqlDbType.NVarChar).Value = NullIfEmpty(model.Slogan1);
            p.Add("@slogan2", SqlDbType.NVarChar).Value = NullIfEmpty(model.Slogan2);
            p.Add("@slogan3", SqlDbType.NVarChar).Value = NullIfEmpty(model.Slogan3);
            p.Add("@iibb", SqlDbType.BigInt).Value = ParseLong(model.Iibb);
            p.Add("@condicionIVA", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.CondicionIVA);
            p.Add("@inicioActividad", SqlDbType.Date).Value = (object)model.InicioActividad ?? DBNull.Value;
            p.Add("@tenantSlug", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.TenantSlug, BuildSlug(model.RazonSocialAfip));
            p.Add("@domicilio", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Domicilio);
            p.Add("@ciudad", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Ciudad);
            p.Add("@pais", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Pais, "Argentina");
            p.Add("@telefono", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Telefono);
            p.Add("@email", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Email);
            p.Add("@basePath", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.BasePath, BuildBasePath(model.TenantSlug, model.RazonSocialAfip));
            p.Add("@esRRII", SqlDbType.TinyInt).Value = model.EsRRII ? 1 : 0;
            p.Add("@nombreCertificado_pfx", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.NombreCertificadoPfx);
            p.Add("@entorno_HOMO_PROD", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.EntornoHomoProd);
            p.Add("@baseDatosNombre", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.BaseDatosNombre);
            p.Add("@activa", SqlDbType.TinyInt).Value = model.Activa ? 1 : 0;
            p.Add("@creado", SqlDbType.Date).Value = DateTime.Today;
            p.Add("@observaciones", SqlDbType.NVarChar).Value = NullIfEmpty(model.Observaciones);
        }

        private void SetSucursalParams(SqlParameterCollection p, SystemAdministrationSucursalEditVm model, HashSet<string> columns)
        {
            p.Add("@sucursal", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Sucursal);
            p.Add("@idEmpresa", SqlDbType.Int).Value = model.IdEmpresa;
            p.Add("@direccion", SqlDbType.NVarChar, 150).Value = NullIfEmpty(model.Direccion);
            p.Add("@localidad", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Localidad);
            p.Add("@provincia", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Provincia);
            p.Add("@pais", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Pais, "Argentina");
            p.Add("@codPuntoVentaAfip", SqlDbType.Int).Value = (object)model.CodPuntoVentaAfip ?? DBNull.Value;

            if (columns.Contains("telefono"))
                p.Add("@telefono", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Telefono);

            if (columns.Contains("activa"))
                p.Add("@activa", SqlDbType.TinyInt).Value = model.Activa ? 1 : 0;

            if (columns.Contains("creado"))
                p.Add("@creado", SqlDbType.Date).Value = DateTime.Today;

            if (columns.Contains("observaciones"))
                p.Add("@observaciones", SqlDbType.NVarChar).Value = NullIfEmpty(model.Observaciones);
        }

        private void SetUsuarioParams(SqlParameterCollection p, SystemAdministrationUsuarioEditVm model, HashSet<string> columns, bool omitirClave)
        {
            p.Add("@nombre", SqlDbType.NVarChar, 120).Value = NullIfEmpty(model.Nombre);
            p.Add("@usuario", SqlDbType.NVarChar, 100).Value = NullIfEmpty(model.Usuario);
            p.Add("@email", SqlDbType.NVarChar, 120).Value = NullIfEmpty(model.Email);
            p.Add("@admin", SqlDbType.Bit).Value = model.Admin;
            p.Add("@activo", SqlDbType.Bit).Value = model.Activo;
            p.Add("@idEmpresa", SqlDbType.Int).Value = model.IdEmpresa;
            p.Add("@idSucursalUser", SqlDbType.Int).Value = model.IdSucursalUser <= 0 ? 0 : model.IdSucursalUser;

            if (!omitirClave)
                p.Add("@clave", SqlDbType.NVarChar, 200).Value = NullIfEmpty(model.Clave, model.Clave);

            if (columns.Contains("colorform"))
                p.Add("@colorForm", SqlDbType.NVarChar, 50).Value = "SteelBlue";

            if (columns.Contains("permitirloginfuerasucursal"))
                p.Add("@permitirLoginFueraSucursal", SqlDbType.Bit).Value = model.PermitirLoginFueraSucursal;
        }

        private HashSet<string> GetTableColumns(SqlConnection con, SqlTransaction tx, string tableName)
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            using (var cmd = new SqlCommand("SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(@tableName)", con, tx))
            {
                cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 256).Value = "dbo." + tableName;

                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        result.Add(Convert.ToString(dr["name"]).ToLowerInvariant());
                    }
                }
            }

            return result;
        }

        private string GetExistingColumnName(SqlConnection con, SqlTransaction tx, string tableName, string logicalColumnName)
        {
            using (var cmd = new SqlCommand(
                "SELECT TOP 1 name FROM sys.columns WHERE object_id = OBJECT_ID(@tableName) AND LOWER(name) = @columnName",
                con,
                tx))
            {
                cmd.Parameters.Add("@tableName", SqlDbType.NVarChar, 256).Value = "dbo." + tableName;
                cmd.Parameters.Add("@columnName", SqlDbType.NVarChar, 128).Value = (logicalColumnName ?? "").Trim().ToLowerInvariant();
                object value = cmd.ExecuteScalar();
                return value == null || value == DBNull.Value ? "" : Convert.ToString(value);
            }
        }

        private static bool HasColumn(IDataRecord dr, string columnName)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                if (string.Equals(dr.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static object NullIfEmpty(string value, string fallback = null)
        {
            string final = string.IsNullOrWhiteSpace(value) ? fallback : value;
            return string.IsNullOrWhiteSpace(final) ? (object)DBNull.Value : final.Trim();
        }

        private static object ParseLong(string value)
        {
            long parsed;
            return long.TryParse((value ?? "").Trim(), out parsed) && parsed > 0
                ? (object)parsed
                : DBNull.Value;
        }

        private static string BuildSlug(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var chars = value.Trim().ToLowerInvariant().Select(c =>
            {
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    return c;
                return '-';
            }).ToArray();

            string slug = new string(chars);
            while (slug.Contains("--"))
                slug = slug.Replace("--", "-");

            return slug.Trim('-');
        }

        private static string BuildBasePath(string tenantSlug, string razonSocial)
        {
            string slug = BuildSlug(tenantSlug);
            if (string.IsNullOrWhiteSpace(slug))
                slug = BuildSlug(razonSocial);

            return string.IsNullOrWhiteSpace(slug) ? null : "/" + slug;
        }

        private sealed class EmpresaContextFijo : IEmpresaContext
        {
            public int IdEmpresa { get; private set; }

            public EmpresaContextFijo(int idEmpresa)
            {
                if (idEmpresa <= 0)
                    throw new InvalidOperationException("IdEmpresa invalido para el contexto de sesion.");

                IdEmpresa = idEmpresa;
            }
        }
    }
}
