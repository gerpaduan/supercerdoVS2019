using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Products;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyProductTypeCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IProductTypeCommandService
{
    public async Task<ProductTypeSaveResult> SaveCompanyProductTypeAsync(
        ProductTypeEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0)
        {
            return ProductTypeSaveResult.Failure("La empresa actual no es valida.");
        }

        var typeName = (request.TypeName ?? string.Empty).Trim();
        var originalTypeName = (request.OriginalTypeName ?? string.Empty).Trim();
        var isInsert = string.IsNullOrWhiteSpace(originalTypeName);

        if (string.IsNullOrWhiteSpace(typeName))
        {
            return ProductTypeSaveResult.Failure("El campo Tipo no puede ser vacio.");
        }

        if (request.SortOrder <= 0)
        {
            return ProductTypeSaveResult.Failure("El campo Orden debe ser un numero entero mayor a cero.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        if (!isInsert)
        {
            var existingType = await GetCompanyProductTypeAsync(connection, request.CompanyId, originalTypeName, cancellationToken);
            if (existingType is null)
            {
                return ProductTypeSaveResult.Failure("No se encontro el tipo de producto seleccionado.");
            }

            if (existingType.IsReserved)
            {
                return ProductTypeSaveResult.Failure("El tipo seleccionado es reservado por el sistema y no puede ser modificado.");
            }
        }
        else
        {
            if (await DuplicatedTypeExistsAsync(connection, request.CompanyId, typeName, cancellationToken))
            {
                return ProductTypeSaveResult.Failure("Ya existe un Tipo con el mismo nombre.");
            }
        }

        var sql = isInsert
            ? """
              INSERT INTO dbo.TiposProducto (tipo, orden, reservadoSistema, creado, idEmpresa)
              VALUES (@tipo, @orden, @reservadoSistema, @creado, @idEmpresa);
              """
            : """
              UPDATE dbo.TiposProducto
              SET tipo = @tipo,
                  orden = @orden,
                  actualizado = @actualizado
              WHERE tipo = @tipoOriginal
                AND reservadoSistema = 0
                AND idEmpresa = @idEmpresa;

              UPDATE dbo.Corte
              SET tipo = @tipo
              WHERE tipo = @tipoOriginal
                AND idEmpresa = @idEmpresa;
              """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = typeName;
        command.Parameters.Add("@tipoOriginal", SqlDbType.NVarChar, 100).Value = originalTypeName;
        command.Parameters.Add("@orden", SqlDbType.Int).Value = request.SortOrder;
        command.Parameters.Add("@reservadoSistema", SqlDbType.Bit).Value = false;
        command.Parameters.Add("@creado", SqlDbType.DateTime).Value = DateTime.Now;
        command.Parameters.Add("@actualizado", SqlDbType.DateTime).Value = DateTime.Now;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = request.CompanyId;

        await command.ExecuteNonQueryAsync(cancellationToken);
        return ProductTypeSaveResult.Ok();
    }

    public async Task<ProductTypeSaveResult> ImportGlobalProductTypesAsync(
        int companyId,
        IReadOnlyCollection<string> typeNames,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ProductTypeSaveResult.Failure("La empresa actual no es valida.");
        }

        var normalizedTypes = (typeNames ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedTypes.Count == 0)
        {
            return ProductTypeSaveResult.Failure("Seleccione al menos un tipo global para importar.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            foreach (var typeName in normalizedTypes)
            {
                if (await CompanyTypeExistsAsync(connection, (SqlTransaction)transaction, companyId, typeName, cancellationToken))
                {
                    continue;
                }

                const string insertSql = """
                    INSERT INTO dbo.TiposProducto (tipo, orden, reservadoSistema, creado, idEmpresa)
                    SELECT TOP 1 tipo, orden, 0, @creado, @idEmpresa
                    FROM dbo.TiposProducto
                    WHERE idEmpresa = 0
                      AND reservadoSistema = 0
                      AND LTRIM(RTRIM(tipo)) = LTRIM(RTRIM(@tipo));
                    """;

                await using var command = new SqlCommand(insertSql, connection, (SqlTransaction)transaction);
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = typeName;
                command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
                command.Parameters.Add("@creado", SqlDbType.DateTime).Value = DateTime.Now;

                var inserted = await command.ExecuteNonQueryAsync(cancellationToken);
                if (inserted <= 0)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return ProductTypeSaveResult.Failure($"No se pudo importar el tipo de producto \"{typeName}\" desde el catalogo global.");
                }
            }

            await transaction.CommitAsync(cancellationToken);
            return ProductTypeSaveResult.Ok();
        }
        catch
        {
            try
            {
                await transaction.RollbackAsync(cancellationToken);
            }
            catch
            {
            }

            throw;
        }
    }

    public async Task<ProductTypeSaveResult> DeleteCompanyProductTypeAsync(
        int companyId,
        string typeName,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return ProductTypeSaveResult.Failure("La empresa actual no es valida.");
        }

        var normalizedTypeName = (typeName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedTypeName))
        {
            return ProductTypeSaveResult.Failure("No se encontro el tipo de producto seleccionado.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        var existingType = await GetCompanyProductTypeAsync(connection, companyId, normalizedTypeName, cancellationToken);
        if (existingType is null)
        {
            return ProductTypeSaveResult.Failure("No se encontro el tipo de producto seleccionado.");
        }

        if (existingType.IsReserved)
        {
            return ProductTypeSaveResult.Failure("El tipo seleccionado es reservado por el sistema y no puede eliminarse.");
        }

        const string checkSql = """
            SELECT COUNT(*)
            FROM dbo.Corte
            WHERE tipo = @tipo
              AND idEmpresa = @idEmpresa;
            """;

        await using (var checkCommand = new SqlCommand(checkSql, connection))
        {
            checkCommand.CommandType = CommandType.Text;
            checkCommand.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = normalizedTypeName;
            checkCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

            var relatedCuts = await checkCommand.ExecuteScalarAsync(cancellationToken);
            if (relatedCuts is not null && relatedCuts != DBNull.Value && Convert.ToInt32(relatedCuts) > 0)
            {
                return ProductTypeSaveResult.Failure("Existen Productos/Cortes con el Tipo que quiere eliminar. Para poder eliminar el Tipo debe cambiar todos los Productos/Cortes asociados a este.");
            }
        }

        const string deleteSql = """
            DELETE FROM dbo.TiposProducto
            WHERE tipo = @tipo
              AND reservadoSistema = 0
              AND idEmpresa = @idEmpresa;
            """;

        await using var deleteCommand = new SqlCommand(deleteSql, connection);
        deleteCommand.CommandType = CommandType.Text;
        deleteCommand.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = normalizedTypeName;
        deleteCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
        return ProductTypeSaveResult.Ok();
    }

    private static async Task<bool> DuplicatedTypeExistsAsync(
        SqlConnection connection,
        int companyId,
        string typeName,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.TiposProducto
            WHERE LTRIM(RTRIM(tipo)) = LTRIM(RTRIM(@tipo))
              AND (reservadoSistema = 1 OR idEmpresa = @idEmpresa);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = typeName;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private static async Task<ExistingProductType?> GetCompanyProductTypeAsync(
        SqlConnection connection,
        int companyId,
        string typeName,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 tipo, reservadoSistema
            FROM dbo.TiposProducto
            WHERE tipo = @tipo
              AND (reservadoSistema = 1 OR idEmpresa = @idEmpresa);
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = typeName;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ExistingProductType(
            Convert.ToString(reader["tipo"]) ?? string.Empty,
            reader["reservadoSistema"] != DBNull.Value && Convert.ToBoolean(reader["reservadoSistema"]));
    }

    private static async Task<bool> CompanyTypeExistsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int companyId,
        string typeName,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.TiposProducto
            WHERE idEmpresa = @idEmpresa
              AND LTRIM(RTRIM(tipo)) = LTRIM(RTRIM(@tipo));
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = typeName;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private sealed record ExistingProductType(string TypeName, bool IsReserved);
}
