using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Authentication;
using CarniSys.NG.Domain.Authentication;
using CarniSys.NG.Domain.Companies;
using CarniSys.NG.Domain.Permissions;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyAuthenticationService(ILegacyConnectionStringResolver connectionStringResolver) : IAuthenticationService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
            {
                return LoginResult.Failure("Usuario y contrasena son obligatorios.");
            }

            var normalizedIdentifier = request.UserName.Trim().ToLowerInvariant();

            await using var connection = new SqlConnection(connectionStringResolver.Resolve());
            await connection.OpenAsync(cancellationToken);

            var userRecord = await GetUserAsync(connection, normalizedIdentifier, cancellationToken);
            if (userRecord is null)
            {
                return LoginResult.Failure("Usuario o contrasena incorrectos.");
            }

            if (!MatchesPassword(userRecord, request.Password))
            {
                return LoginResult.Failure("Usuario o contrasena incorrectos.");
            }

            if (!userRecord.IsActive)
            {
                return LoginResult.Failure("No fue posible iniciar sesion. El usuario esta inactivo.");
            }

            var company = await GetCompanyAsync(connection, userRecord.CompanyId, cancellationToken);
            var branch = await GetBranchAsync(connection, userRecord.BranchId, cancellationToken);
            var permissions = await GetPermissionsAsync(connection, userRecord.UserId, cancellationToken);

            if (company is null)
            {
                return LoginResult.Failure("El usuario no tiene empresa valida.");
            }

            var authenticatedUser = new AuthenticatedUser(
                userRecord.UserId,
                new UserLoginName(userRecord.Login),
                string.IsNullOrWhiteSpace(userRecord.DisplayName) ? userRecord.Login : userRecord.DisplayName,
                company,
                branch ?? new BranchContext(userRecord.BranchId > 0 ? userRecord.BranchId : 1, branch?.Name ?? "Seleccione Sucursal"),
                permissions,
                userRecord.IsAdministrator);

            return LoginResult.Success(authenticatedUser);
        }
        catch (InvalidOperationException)
        {
            return LoginResult.Failure("No fue posible resolver la configuracion de acceso al sistema actual.");
        }
        catch (SqlException)
        {
            return LoginResult.Failure("No fue posible conectar con la base actual de CarniSys.");
        }
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        if (userId <= 0)
        {
            return null;
        }

        try
        {
            await using var connection = new SqlConnection(connectionStringResolver.Resolve());
            await connection.OpenAsync(cancellationToken);

            var userRecord = await GetUserByIdAsync(connection, userId, cancellationToken);
            if (userRecord is null || !userRecord.IsActive)
            {
                return null;
            }

            var company = await GetCompanyAsync(connection, userRecord.CompanyId, cancellationToken);
            var branch = await GetBranchAsync(connection, userRecord.BranchId, cancellationToken);
            var permissions = await GetPermissionsAsync(connection, userRecord.UserId, cancellationToken);

            if (company is null)
            {
                return null;
            }

            return new AuthenticatedUser(
                userRecord.UserId,
                new UserLoginName(userRecord.Login),
                string.IsNullOrWhiteSpace(userRecord.DisplayName) ? userRecord.Login : userRecord.DisplayName,
                company,
                branch ?? new BranchContext(userRecord.BranchId > 0 ? userRecord.BranchId : 1, branch?.Name ?? "Seleccione Sucursal"),
                permissions,
                userRecord.IsAdministrator);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (SqlException)
        {
            return null;
        }
    }

    private static bool MatchesPassword(LegacyUserRecord userRecord, string password)
    {
        if (LegacyPasswordSecurity.VerifyPassword(password, userRecord.PasswordHash, userRecord.PasswordSalt, userRecord.PasswordHashIterations))
        {
            return true;
        }

        return string.Equals(userRecord.LegacyPassword ?? string.Empty, password, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<LegacyUserRecord?> GetUserAsync(SqlConnection connection, string normalizedIdentifier, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 *
            FROM Usuarios
            WHERE LOWER(ISNULL(usuario, '')) = @identificador
               OR LOWER(ISNULL(email, '')) = @identificador
            ORDER BY activo DESC, id ASC;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@identificador", SqlDbType.NVarChar, 120).Value = normalizedIdentifier;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new LegacyUserRecord(
            GetInt(reader, "id"),
            GetString(reader, "usuario"),
            GetString(reader, "nombre"),
            GetString(reader, "clave"),
            GetString(reader, "passwordHash"),
            GetString(reader, "passwordSalt"),
            GetInt(reader, "passwordHashIterations"),
            GetBool(reader, "activo"),
            GetBool(reader, "admin"),
            GetInt(reader, "idEmpresa"),
            GetInt(reader, "idSucursalUser"));
    }

    private static async Task<LegacyUserRecord?> GetUserByIdAsync(SqlConnection connection, int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 *
            FROM Usuarios
            WHERE id = @idUsuario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new LegacyUserRecord(
            GetInt(reader, "id"),
            GetString(reader, "usuario"),
            GetString(reader, "nombre"),
            GetString(reader, "clave"),
            GetString(reader, "passwordHash"),
            GetString(reader, "passwordSalt"),
            GetInt(reader, "passwordHashIterations"),
            GetBool(reader, "activo"),
            GetBool(reader, "admin"),
            GetInt(reader, "idEmpresa"),
            GetInt(reader, "idSucursalUser"));
    }

    private static async Task<CompanyContext?> GetCompanyAsync(SqlConnection connection, int companyId, CancellationToken cancellationToken)
    {
        if (companyId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1 idEmpresa, razonSocialAfip, nombreFantasia
            FROM Empresas
            WHERE idEmpresa = @idEmpresa;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var name = GetString(reader, "razonSocialAfip");
        if (string.IsNullOrWhiteSpace(name))
        {
            name = GetString(reader, "nombreFantasia");
        }

        return new CompanyContext(companyId, string.IsNullOrWhiteSpace(name) ? $"Empresa {companyId}" : name);
    }

    private static async Task<BranchContext?> GetBranchAsync(SqlConnection connection, int branchId, CancellationToken cancellationToken)
    {
        if (branchId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1 idSucursal, sucursal
            FROM Sucursal
            WHERE idSucursal = @idSucursal;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BranchContext(branchId, GetString(reader, "sucursal"));
    }

    private static async Task<IReadOnlyCollection<PermissionGrant>> GetPermissionsAsync(SqlConnection connection, int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT 
                f.idForm,
                f.formConsulta,
                f.formEdicion,
                f.formEdicionExtra1,
                f.formEdicionExtra2,
                COALESCE(p.diasPermitidosVer, -1) AS diasPermitidosVer,
                COALESCE(p.diasPermitidosEditar, -1) AS diasPermitidosEditar,
                CAST(COALESCE(p.soloRegistrosPropios, 1) AS bit) AS soloRegistrosPropios
            FROM Formularios f
            LEFT JOIN PermisosUsuarios p ON f.idForm = p.idForm AND p.idUsuario = @idUsuario
            ORDER BY f.idForm;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;

        var grants = new Dictionary<string, PermissionGrant>(StringComparer.OrdinalIgnoreCase);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var readDays = GetInt(reader, "diasPermitidosVer", -1);
            var editDays = GetInt(reader, "diasPermitidosEditar", -1);
            var ownershipScope = GetBool(reader, "soloRegistrosPropios")
                ? RecordOwnershipScope.Own
                : RecordOwnershipScope.All;

            MergeGrant(grants, GetString(reader, "formConsulta"), readDays, -1, ownershipScope);
            MergeGrant(grants, GetString(reader, "formEdicion"), -1, editDays, ownershipScope);
            MergeGrant(grants, GetString(reader, "formEdicionExtra1"), -1, editDays, ownershipScope);
            MergeGrant(grants, GetString(reader, "formEdicionExtra2"), -1, editDays, ownershipScope);
        }

        return grants.Values.ToArray();
    }

    private static void MergeGrant(
        IDictionary<string, PermissionGrant> grants,
        string resource,
        int readWindowInDays,
        int editWindowInDays,
        RecordOwnershipScope ownershipScope)
    {
        if (string.IsNullOrWhiteSpace(resource))
        {
            return;
        }

        if (!grants.TryGetValue(resource, out var existingGrant))
        {
            grants[resource] = new PermissionGrant(resource, readWindowInDays, editWindowInDays, ownershipScope);
            return;
        }

        grants[resource] = new PermissionGrant(
            resource,
            Math.Max(existingGrant.ReadWindowInDays, readWindowInDays),
            Math.Max(existingGrant.EditWindowInDays, editWindowInDays),
            existingGrant.OwnershipScope == RecordOwnershipScope.All || ownershipScope == RecordOwnershipScope.All
                ? RecordOwnershipScope.All
                : RecordOwnershipScope.Own);
    }

    private static int GetInt(IDataRecord record, string columnName, int defaultValue = 0)
    {
        var ordinal = GetOrdinal(record, columnName);
        if (ordinal < 0 || record.IsDBNull(ordinal))
        {
            return defaultValue;
        }

        return Convert.ToInt32(record.GetValue(ordinal));
    }

    private static string GetString(IDataRecord record, string columnName)
    {
        var ordinal = GetOrdinal(record, columnName);
        if (ordinal < 0 || record.IsDBNull(ordinal))
        {
            return string.Empty;
        }

        return Convert.ToString(record.GetValue(ordinal)) ?? string.Empty;
    }

    private static bool GetBool(IDataRecord record, string columnName)
    {
        var ordinal = GetOrdinal(record, columnName);
        return ordinal >= 0 && !record.IsDBNull(ordinal) && Convert.ToBoolean(record.GetValue(ordinal));
    }

    private static int GetOrdinal(IDataRecord record, string columnName)
    {
        for (var i = 0; i < record.FieldCount; i++)
        {
            if (string.Equals(record.GetName(i), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    private sealed record LegacyUserRecord(
        int UserId,
        string Login,
        string DisplayName,
        string LegacyPassword,
        string PasswordHash,
        string PasswordSalt,
        int PasswordHashIterations,
        bool IsActive,
        bool IsAdministrator,
        int CompanyId,
        int BranchId);
}
