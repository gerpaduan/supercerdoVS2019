using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Users;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyUserCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IUserCommandService
{
    public async Task<SaveUserResult> SaveUserAsync(SaveUserRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0)
        {
            return SaveUserResult.Failure("La empresa actual no es valida.");
        }

        var fullName = (request.FullName ?? string.Empty).Trim();
        var login = (request.Login ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(fullName))
        {
            return SaveUserResult.Failure("El nombre es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(login))
        {
            return SaveUserResult.Failure("El usuario es obligatorio.");
        }

        if (request.UserId <= 0 && string.IsNullOrWhiteSpace(password))
        {
            return SaveUserResult.Failure("La clave es obligatoria para un usuario nuevo.");
        }

        if (password.Contains(' '))
        {
            return SaveUserResult.Failure("La clave no puede contener espacios en blanco.");
        }

        if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
        {
            return SaveUserResult.Failure("El email no es valido.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        var originalUser = request.UserId > 0
            ? await GetUserAsync(connection, request.CompanyId, request.UserId, cancellationToken)
            : null;

        if (request.UserId > 0 && originalUser is null)
        {
            return SaveUserResult.Failure("No se encontro el usuario a modificar.");
        }

        if (originalUser is not null && string.Equals(originalUser.Login, "admin", StringComparison.OrdinalIgnoreCase))
        {
            return SaveUserResult.Failure("El usuario Admin es reservado para el desarrollador del sistema.");
        }

        if (request.BranchId > 0 && !await BranchBelongsToCompanyAsync(connection, request.CompanyId, request.BranchId, cancellationToken))
        {
            return SaveUserResult.Failure("La sucursal seleccionada no es valida.");
        }

        if (await LoginExistsAsync(connection, request.CompanyId, login, request.UserId, cancellationToken))
        {
            return SaveUserResult.Failure("Ya existe un usuario con ese nombre de acceso.");
        }

        if (!string.IsNullOrWhiteSpace(email) && await EmailExistsAsync(connection, request.CompanyId, email, request.UserId, cancellationToken))
        {
            return SaveUserResult.Failure("Ya existe un usuario con ese email.");
        }

        var persistedUserId = await SaveBaseUserAsync(
            connection,
            request,
            originalUser?.LegacyPassword ?? string.Empty,
            fullName,
            login,
            email,
            cancellationToken);

        if (persistedUserId <= 0)
        {
            return SaveUserResult.Failure("No fue posible guardar el usuario.");
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            var hash = LegacyPasswordSecurity.HashPassword(password.Trim());
            await UpdateWebPasswordAsync(connection, persistedUserId, hash.Hash, hash.Salt, hash.Iterations, cancellationToken);
        }

        await UpdateBranchAsync(connection, persistedUserId, request.BranchId, cancellationToken);
        await UpdateLoginOutsideBranchAsync(connection, persistedUserId, request.CanLoginOutsideBranch, cancellationToken);

        return SaveUserResult.Ok(persistedUserId);
    }

    private static async Task<LegacyEditableUser?> GetUserAsync(SqlConnection connection, int companyId, int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 id, idEmpresa, usuario, clave
            FROM dbo.Usuarios
            WHERE id = @idUsuario
              AND idEmpresa = @idEmpresa;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new LegacyEditableUser(
            Convert.ToInt32(reader["id"]),
            Convert.ToString(reader["usuario"]) ?? string.Empty,
            Convert.ToString(reader["clave"]) ?? string.Empty);
    }

    private static async Task<bool> BranchBelongsToCompanyAsync(SqlConnection connection, int companyId, int branchId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Sucursal
            WHERE idSucursal = @idSucursal
              AND idEmpresa = @idEmpresa;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> LoginExistsAsync(SqlConnection connection, int companyId, string login, int currentUserId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Usuarios
            WHERE idEmpresa = @idEmpresa
              AND LOWER(ISNULL(usuario, '')) = LOWER(@usuario)
              AND id <> @idUsuario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = login;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = currentUserId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private static async Task<bool> EmailExistsAsync(SqlConnection connection, int companyId, string email, int currentUserId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM dbo.Usuarios
            WHERE idEmpresa = @idEmpresa
              AND LOWER(ISNULL(email, '')) = LOWER(@email)
              AND id <> @idUsuario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 120).Value = email;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = currentUserId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private static async Task<int> SaveBaseUserAsync(
        SqlConnection connection,
        SaveUserRequest request,
        string legacyPassword,
        string fullName,
        string login,
        string email,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.addOrEditUser
                @id,
                @nombre,
                @usuario,
                @email,
                @clave,
                @admin,
                @activo,
                @colorForm;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@id", SqlDbType.Int).Value = request.UserId;
        command.Parameters.Add("@nombre", SqlDbType.NVarChar, 120).Value = fullName;
        command.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = login;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 120).Value = email;
        command.Parameters.Add("@clave", SqlDbType.NVarChar, 200).Value = request.UserId > 0 ? legacyPassword : request.Password.Trim();
        command.Parameters.Add("@admin", SqlDbType.Bit).Value = request.IsAdministrator;
        command.Parameters.Add("@activo", SqlDbType.Bit).Value = request.IsActive;
        command.Parameters.Add("@colorForm", SqlDbType.NVarChar, 50).Value = "SteelBlue";

        await command.ExecuteNonQueryAsync(cancellationToken);

        if (request.UserId > 0)
        {
            return request.UserId;
        }

        return await FindUserIdByLoginAsync(connection, request.CompanyId, login, cancellationToken);
    }

    private static async Task<int> FindUserIdByLoginAsync(SqlConnection connection, int companyId, string login, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 id
            FROM dbo.Usuarios
            WHERE idEmpresa = @idEmpresa
              AND LOWER(ISNULL(usuario, '')) = LOWER(@usuario)
            ORDER BY id DESC;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@usuario", SqlDbType.NVarChar, 50).Value = login;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static async Task UpdateWebPasswordAsync(SqlConnection connection, int userId, string hash, string salt, int iterations, CancellationToken cancellationToken)
    {
        var setClauses = new List<string>();

        if (await ColumnExistsAsync(connection, "Usuarios", "passwordHash", cancellationToken))
        {
            setClauses.Add("passwordHash = @passwordHash");
        }

        if (await ColumnExistsAsync(connection, "Usuarios", "passwordSalt", cancellationToken))
        {
            setClauses.Add("passwordSalt = @passwordSalt");
        }

        if (await ColumnExistsAsync(connection, "Usuarios", "passwordHashIterations", cancellationToken))
        {
            setClauses.Add("passwordHashIterations = @passwordHashIterations");
        }

        if (await ColumnExistsAsync(connection, "Usuarios", "passwordUpdatedAtUtc", cancellationToken))
        {
            setClauses.Add("passwordUpdatedAtUtc = SYSUTCDATETIME()");
        }

        if (setClauses.Count == 0)
        {
            return;
        }

        var sql = "UPDATE Usuarios SET " + string.Join(", ", setClauses) + " WHERE id = @idUsuario;";

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@passwordHash", SqlDbType.NVarChar, 256).Value = hash;
        command.Parameters.Add("@passwordSalt", SqlDbType.NVarChar, 256).Value = salt;
        command.Parameters.Add("@passwordHashIterations", SqlDbType.Int).Value = iterations;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateBranchAsync(SqlConnection connection, int userId, int branchId, CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Usuarios
            SET idSucursalUser = @idSucursal
            WHERE id = @idUsuario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateLoginOutsideBranchAsync(SqlConnection connection, int userId, bool canLoginOutsideBranch, CancellationToken cancellationToken)
    {
        if (!await ColumnExistsAsync(connection, "Usuarios", "PermitirLoginFueraSucursal", cancellationToken))
        {
            return;
        }

        const string sql = """
            UPDATE dbo.Usuarios
            SET PermitirLoginFueraSucursal = @permitir
            WHERE id = @idUsuario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@permitir", SqlDbType.Bit).Value = canLoginOutsideBranch;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> ColumnExistsAsync(SqlConnection connection, string tableName, string columnName, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM sys.columns
            WHERE object_id = OBJECT_ID(@tableName)
              AND name = @columnName;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tableName", SqlDbType.NVarChar, 128).Value = "dbo." + tableName;
        command.Parameters.Add("@columnName", SqlDbType.NVarChar, 128).Value = columnName;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mail = new System.Net.Mail.MailAddress(email);
            return string.Equals(mail.Address, email, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private sealed record LegacyEditableUser(int UserId, string Login, string LegacyPassword);
}
