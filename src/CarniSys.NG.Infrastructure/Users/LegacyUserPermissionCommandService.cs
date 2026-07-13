using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Users;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyUserPermissionCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IUserPermissionCommandService
{
    public async Task<bool> SaveUserPermissionsAsync(
        SaveUserPermissionsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0 || request.UserId <= 0 || request.Items.Count == 0)
        {
            return false;
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        if (!await UserBelongsToCompanyAsync(connection, request.CompanyId, request.UserId, cancellationToken))
        {
            return false;
        }

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        const string sql = """
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
            END
            """;

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.CommandType = CommandType.Text;

        var pUserId = command.Parameters.Add("@idUsuario", SqlDbType.Int);
        var pFormId = command.Parameters.Add("@idForm", SqlDbType.Int);
        var pReadDays = command.Parameters.Add("@diasVer", SqlDbType.Int);
        var pEditDays = command.Parameters.Add("@diasEditar", SqlDbType.Int);
        var pOwnRecords = command.Parameters.Add("@soloPropios", SqlDbType.Bit);

        pUserId.Value = request.UserId;

        foreach (var item in request.Items)
        {
            pFormId.Value = item.FormId;
            pReadDays.Value = item.CanRead ? Math.Max(0, item.ReadDays) : -1;
            pEditDays.Value = item.CanEdit ? Math.Max(0, item.EditDays) : -1;
            pOwnRecords.Value = item.CanEdit ? item.OwnRecordsOnly : true;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static async Task<bool> UserBelongsToCompanyAsync(SqlConnection connection, int companyId, int userId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(1)
            FROM Usuarios
            WHERE id = @idUsuario
              AND idEmpresa = @idEmpresa;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idUsuario", SqlDbType.Int).Value = userId;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }
}
