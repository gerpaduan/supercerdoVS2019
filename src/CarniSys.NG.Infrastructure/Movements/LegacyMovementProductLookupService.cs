using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyMovementProductLookupService(ILegacyConnectionStringResolver connectionStringResolver) : IMovementProductLookupService
{
    public async Task<IReadOnlyCollection<MovementProductLookupItem>> GetQuickProductsAsync(
        int companyId,
        int maxCode,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || maxCode < 0 || limit <= 0)
        {
            return [];
        }

        const string sql = """
            SELECT TOP (@limite)
                idCorte,
                codigo,
                corte,
                tipo,
                pesable,
                promedio
            FROM dbo.Corte
            WHERE idEmpresa = @idEmpresa
              AND codigo BETWEEN 0 AND @maxCodigo
            ORDER BY codigo ASC;
            """;

        var items = new List<MovementProductLookupItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@limite", SqlDbType.Int).Value = limit;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@maxCodigo", SqlDbType.BigInt).Value = maxCode;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(MapProduct(reader));
        }

        return items;
    }

    public async Task<MovementProductLookupItem?> FindProductByCodeAsync(
        int companyId,
        long code,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || code < 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1
                idCorte,
                codigo,
                corte,
                tipo,
                pesable,
                promedio
            FROM dbo.Corte
            WHERE idEmpresa = @idEmpresa
              AND codigo = @codigo;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@codigo", SqlDbType.BigInt).Value = code;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return MapProduct(reader);
    }

    private static MovementProductLookupItem MapProduct(IDataRecord record)
    {
        return new MovementProductLookupItem
        {
            ProductId = GetInt(record, "idCorte"),
            Code = GetLong(record, "codigo"),
            Description = GetString(record, "corte"),
            Type = GetString(record, "tipo"),
            Weighable = GetBool(record, "pesable"),
            AverageWeight = GetDecimal(record, "promedio")
        };
    }

    private static int GetInt(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal));
    }

    private static long GetLong(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0L : Convert.ToInt64(record.GetValue(ordinal));
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

    private static decimal GetDecimal(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(record.GetValue(ordinal));
    }
}
