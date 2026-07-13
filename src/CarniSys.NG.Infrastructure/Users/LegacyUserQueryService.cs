using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Users;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyUserQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IUserQueryService
{
    public async Task<IReadOnlyCollection<UserListItem>> GetUsersAsync(
        int companyId,
        UserListQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearchText = (query.SearchText ?? string.Empty).Trim();

        const string sql = """
            SELECT
                u.id,
                u.idEmpresa,
                u.idSucursalUser,
                u.nombre,
                u.usuario,
                u.email,
                u.admin,
                u.activo,
                s.sucursal AS sucursalNombre,
                CAST(ISNULL(u.PermitirLoginFueraSucursal, 0) AS bit) AS permitirLoginFueraSucursal
            FROM dbo.Usuarios u
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = u.idSucursalUser
            WHERE u.idEmpresa = @idEmpresa
              AND (@soloActivos = 0 OR u.activo = 1)
              AND (
                    u.nombre  LIKE @texto ESCAPE '\'
                 OR u.usuario LIKE @texto ESCAPE '\'
                 OR u.email   LIKE @texto ESCAPE '\'
              )
            ORDER BY u.nombre, u.usuario;
            """;

        var items = new List<UserListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@soloActivos", SqlDbType.Bit).Value = query.OnlyActive;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(normalizedSearchText);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new UserListItem
            {
                UserId = GetInt(reader, "id"),
                CompanyId = GetInt(reader, "idEmpresa"),
                BranchId = GetInt(reader, "idSucursalUser"),
                FullName = GetString(reader, "nombre"),
                Login = GetString(reader, "usuario"),
                Email = GetString(reader, "email"),
                BranchName = GetString(reader, "sucursalNombre"),
                IsAdministrator = GetBool(reader, "admin"),
                IsActive = GetBool(reader, "activo"),
                CanLoginOutsideBranch = GetBool(reader, "permitirLoginFueraSucursal")
            });
        }

        return items;
    }

    public async Task<UserDetailItem?> GetUserByIdAsync(
        int companyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || userId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT
                u.id,
                u.idEmpresa,
                u.idSucursalUser,
                u.nombre,
                u.usuario,
                u.email,
                u.admin,
                u.activo,
                s.sucursal AS sucursalNombre,
                CAST(ISNULL(u.PermitirLoginFueraSucursal, 0) AS bit) AS permitirLoginFueraSucursal
            FROM dbo.Usuarios u
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = u.idSucursalUser
            WHERE u.idEmpresa = @idEmpresa
              AND u.id = @idUsuario;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new UserDetailItem
        {
            UserId = GetInt(reader, "id"),
            CompanyId = GetInt(reader, "idEmpresa"),
            BranchId = GetInt(reader, "idSucursalUser"),
            FullName = GetString(reader, "nombre"),
            Login = GetString(reader, "usuario"),
            Email = GetString(reader, "email"),
            BranchName = GetString(reader, "sucursalNombre"),
            IsAdministrator = GetBool(reader, "admin"),
            IsActive = GetBool(reader, "activo"),
            CanLoginOutsideBranch = GetBool(reader, "permitirLoginFueraSucursal")
        };
    }

    public async Task<UserPermissionPage?> GetUserPermissionsAsync(
        int companyId,
        int userId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || userId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT
                u.id AS idUsuario,
                u.nombre,
                u.usuario,
                f.idForm,
                f.nombreForm,
                f.descripcion,
                COALESCE(p.diasPermitidosVer, -1) AS diasPermitidosVer,
                COALESCE(p.diasPermitidosEditar, -1) AS diasPermitidosEditar,
                CAST(COALESCE(p.soloRegistrosPropios, 1) AS bit) AS soloRegistrosPropios
            FROM dbo.Usuarios u
            INNER JOIN dbo.Formularios f ON 1 = 1
            LEFT JOIN dbo.PermisosUsuarios p
                ON p.idUsuario = u.id
               AND p.idForm = f.idForm
            WHERE u.idEmpresa = @idEmpresa
              AND u.id = @idUsuario
            ORDER BY f.nombreForm;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        List<UserPermissionItem>? items = null;
        UserPermissionPage? page = null;

        while (await reader.ReadAsync(cancellationToken))
        {
            items ??= new List<UserPermissionItem>();
            page ??= new UserPermissionPage
            {
                UserId = GetInt(reader, "idUsuario"),
                UserName = GetString(reader, "nombre"),
                UserLogin = GetString(reader, "usuario"),
                Items = items
            };

            var readDays = GetInt(reader, "diasPermitidosVer");
            var editDays = GetInt(reader, "diasPermitidosEditar");

            items.Add(new UserPermissionItem
            {
                FormId = GetInt(reader, "idForm"),
                FormName = GetString(reader, "nombreForm"),
                Description = GetString(reader, "descripcion"),
                CanRead = readDays >= 0,
                ReadDays = readDays >= 0 ? readDays : 0,
                CanEdit = editDays >= 0,
                EditDays = editDays >= 0 ? editDays : 0,
                OwnRecordsOnly = GetBool(reader, "soloRegistrosPropios")
            });
        }

        return page;
    }

    private static string LikePattern(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "%";
        }

        return "%" + text
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[") + "%";
    }

    private static int GetInt(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal));
    }

    private static string GetString(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? string.Empty : Convert.ToString(record.GetValue(ordinal)) ?? string.Empty;
    }

    private static bool GetBool(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return !record.IsDBNull(ordinal) && Convert.ToBoolean(record.GetValue(ordinal));
    }
}
