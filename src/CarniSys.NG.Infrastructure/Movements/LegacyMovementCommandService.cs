using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyMovementCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IMovementCommandService
{
    public async Task<MovementSaveResult> SaveMovementAsync(
        MovementSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0 || request.UserId <= 0)
        {
            return MovementSaveResult.Failure("No fue posible resolver la empresa o el usuario actual.");
        }

        if (request.OriginBranchId <= 0 || request.DestinationBranchId <= 0)
        {
            return MovementSaveResult.Failure("Debe seleccionar la sucursal origen y destino.");
        }

        if (request.OriginBranchId == request.DestinationBranchId)
        {
            return MovementSaveResult.Failure("La sucursal origen y destino deben ser diferentes.");
        }

        var lines = (request.Lines ?? [])
            .Where(x => x is not null)
            .ToArray();

        if (lines.Length == 0)
        {
            return MovementSaveResult.Failure("Debe agregar al menos un producto al movimiento.");
        }

        if (lines.Any(x => x.ProductId <= 0))
        {
            return MovementSaveResult.Failure("Todas las lineas deben tener un producto valido.");
        }

        if (lines.Any(x => x.QuantityUnits < 0))
        {
            return MovementSaveResult.Failure("Las cantidades en unidades no pueden ser negativas.");
        }

        if (lines.Any(x => x.QuantityWeightKg <= 0))
        {
            return MovementSaveResult.Failure("Todas las lineas deben tener kilos mayores a cero.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        if (!await BranchBelongsToCompanyAsync(connection, request.CompanyId, request.OriginBranchId, cancellationToken) ||
            !await BranchBelongsToCompanyAsync(connection, request.CompanyId, request.DestinationBranchId, cancellationToken))
        {
            return MovementSaveResult.Failure("La sucursal origen o destino no pertenece a la empresa actual.");
        }

        if (!await AllProductsBelongToCompanyAsync(connection, request.CompanyId, lines.Select(x => x.ProductId).Distinct().ToArray(), cancellationToken))
        {
            return MovementSaveResult.Failure("Uno o mas productos no pertenecen a la empresa actual.");
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var movementId = request.MovementId;

            if (movementId > 0 && !await MovementExistsAsync(connection, transaction, movementId, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return MovementSaveResult.Failure("No se encontro el movimiento a modificar.");
            }

            movementId = await SaveHeaderAsync(connection, transaction, request, cancellationToken);
            await ReplaceLinesAsync(connection, transaction, movementId, lines, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return MovementSaveResult.Ok(movementId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private static async Task<bool> BranchBelongsToCompanyAsync(
        SqlConnection connection,
        int companyId,
        int branchId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 idSucursal
            FROM dbo.Sucursal
            WHERE idSucursal = @idSucursal
              AND idEmpresa = @idEmpresa;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value;
    }

    private static async Task<bool> AllProductsBelongToCompanyAsync(
        SqlConnection connection,
        int companyId,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        if (productIds.Count == 0)
        {
            return false;
        }

        var sql = """
            SELECT COUNT(*)
            FROM dbo.Corte
            WHERE idEmpresa = @idEmpresa
              AND idCorte IN (
        """ + string.Join(",", productIds) + """
              );
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var found = result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        return found == productIds.Count;
    }

    private static async Task<bool> MovementExistsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int movementId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.cargarMovimiento @idMovimiento;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = movementId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken);
    }

    private static async Task<int> SaveHeaderAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        MovementSaveRequest request,
        CancellationToken cancellationToken)
    {
        if (request.MovementId <= 0)
        {
            const string insertSql = """
                EXEC dbo.addOrEditMovimiento
                    @idMovimiento,
                    @fechaMovimiento,
                    @sucursalOrigen,
                    @sucursalDestino,
                    @observaciones,
                    @creadoPor;
                """;

            await using var insertCommand = new SqlCommand(insertSql, connection, transaction);
            insertCommand.CommandType = CommandType.Text;
            insertCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = request.MovementId;
            insertCommand.Parameters.Add("@fechaMovimiento", SqlDbType.DateTime).Value = request.MovementDate;
            insertCommand.Parameters.Add("@sucursalOrigen", SqlDbType.Int).Value = request.OriginBranchId;
            insertCommand.Parameters.Add("@sucursalDestino", SqlDbType.Int).Value = request.DestinationBranchId;
            insertCommand.Parameters.Add("@observaciones", SqlDbType.NVarChar, 250).Value = (request.Notes ?? string.Empty).Trim();
            insertCommand.Parameters.Add("@creadoPor", SqlDbType.Int).Value = request.UserId;

            var result = await insertCommand.ExecuteScalarAsync(cancellationToken);
            return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        }

        const string updateSql = """
            EXEC dbo.addOrEditMovimiento
                @idMovimiento,
                @fechaMovimiento,
                @sucursalOrigen,
                @sucursalDestino,
                @observaciones,
                @creadoPor,
                @actualizadoPor;
            """;

        await using var updateCommand = new SqlCommand(updateSql, connection, transaction);
        updateCommand.CommandType = CommandType.Text;
        updateCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = request.MovementId;
        updateCommand.Parameters.Add("@fechaMovimiento", SqlDbType.DateTime).Value = request.MovementDate;
        updateCommand.Parameters.Add("@sucursalOrigen", SqlDbType.Int).Value = request.OriginBranchId;
        updateCommand.Parameters.Add("@sucursalDestino", SqlDbType.Int).Value = request.DestinationBranchId;
        updateCommand.Parameters.Add("@observaciones", SqlDbType.NVarChar, 250).Value = (request.Notes ?? string.Empty).Trim();
        updateCommand.Parameters.Add("@creadoPor", SqlDbType.Int).Value = request.UserId;
        updateCommand.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = request.UserId;

        await updateCommand.ExecuteNonQueryAsync(cancellationToken);
        return request.MovementId;
    }

    private static async Task ReplaceLinesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int movementId,
        IReadOnlyCollection<MovementSaveLineRequest> lines,
        CancellationToken cancellationToken)
    {
        const string deleteSql = """
            EXEC dbo.quitarCortesPorMovimiento @idMovimiento;
            """;

        await using (var deleteCommand = new SqlCommand(deleteSql, connection, transaction))
        {
            deleteCommand.CommandType = CommandType.Text;
            deleteCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = movementId;
            await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        const string insertSql = """
            EXEC dbo.agregarCortePorMovimiento
                @idMovimiento,
                @idCorte,
                @cantKg,
                @cantUnidad,
                @pesoBalanza,
                @permitirIngreso;
            """;

        foreach (var line in lines)
        {
            await using var insertCommand = new SqlCommand(insertSql, connection, transaction);
            insertCommand.CommandType = CommandType.Text;
            insertCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = movementId;
            insertCommand.Parameters.Add("@idCorte", SqlDbType.Int).Value = line.ProductId;
            insertCommand.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = line.QuantityWeightKg;
            insertCommand.Parameters.Add("@cantUnidad", SqlDbType.Int).Value = line.QuantityUnits;
            insertCommand.Parameters.Add("@pesoBalanza", SqlDbType.Bit).Value = line.ScaleWeight;
            insertCommand.Parameters.Add("@permitirIngreso", SqlDbType.Bit).Value = line.AllowEntry;
            insertCommand.Parameters["@cantKg"].Precision = 18;
            insertCommand.Parameters["@cantKg"].Scale = 3;

            await insertCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }
}
