using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Companies;
using CarniSys.NG.Application.Movements;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyMovementQueryService(
    ILegacyConnectionStringResolver connectionStringResolver,
    IBranchLookupService branchLookupService) : IMovementQueryService
{
    public async Task<IReadOnlyCollection<MovementListItem>> GetMovementsAsync(
        int companyId,
        MovementListQuery query,
        CancellationToken cancellationToken = default)
    {
        var originBranchName = await ResolveBranchFilterNameAsync(companyId, query.OriginBranchId, cancellationToken);
        var destinationBranchName = await ResolveBranchFilterNameAsync(companyId, query.DestinationBranchId, cancellationToken);

        var rows = await GetMovementRowsAsync(
            originBranchName,
            destinationBranchName,
            query.DateFrom,
            query.DateTo,
            cancellationToken);

        var movementIds = rows
            .Select(row => GetInt(row, "Id Movimiento", "idMovimiento", "movimiento"))
            .Where(id => id > 0)
            .Distinct()
            .ToArray();

        var totalsByMovement = await GetTotalsByMovementAsync(movementIds, cancellationToken);

        return rows
            .Select(row =>
            {
                var movementId = GetInt(row, "Id Movimiento", "idMovimiento", "movimiento");
                var totalUnits = GetDecimal(row, "totalUnidad", "cantUnidad", "Cant Prod.", "cantProd", "kgCorte");
                var totalWeightKg = GetDecimal(row, "totalKilos", "cantKg", "cantKgs", "kg", "kgCorte");

                if (movementId > 0 && totalsByMovement.TryGetValue(movementId, out var totals))
                {
                    if (totalUnits == 0m)
                    {
                        totalUnits = totals.TotalUnits;
                    }

                    if (totalWeightKg == 0m)
                    {
                        totalWeightKg = totals.TotalWeightKg;
                    }
                }

                return new MovementListItem
                {
                    MovementId = movementId,
                    MovementDate = GetDateTime(row, "Fecha Movimiento", "fechaMovimiento"),
                    OriginBranchName = GetString(row, "Origen", "sucursalOrigen", "origen"),
                    DestinationBranchName = GetString(row, "Destino", "sucursalDestino", "destino"),
                    OriginMovementReference = GetString(row, "Id Origen", "idMovOrigen", "deOrigen"),
                    Status = GetString(row, "Estado", "estado"),
                    TotalUnits = totalUnits,
                    TotalWeightKg = totalWeightKg,
                    Notes = GetString(row, "observaciones", "Observaciones", "obs")
                };
            })
            .OrderByDescending(x => x.MovementDate)
            .ToArray();
    }

    public async Task<MovementDetailItem?> GetMovementByIdAsync(int movementId, CancellationToken cancellationToken = default)
    {
        if (movementId <= 0)
        {
            return null;
        }

        const string movementSql = """
            EXEC dbo.cargarMovimiento @idMovimiento;
            """;

        const string linesSql = """
            EXEC dbo.cargarCortesPorMovimiento @idMovimiento, @acumulado;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var movementCommand = new SqlCommand(movementSql, connection);
        movementCommand.CommandType = CommandType.Text;
        movementCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = movementId;

        MovementHeader? header = null;
        await using (var reader = await movementCommand.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                header = new MovementHeader(
                    GetInt(reader, "idMovimiento"),
                    GetDateTime(reader, "fechaMovimiento"),
                    GetInt(reader, "idOrigen"),
                    GetString(reader, "origen"),
                    GetInt(reader, "idDestino"),
                    GetString(reader, "destino"),
                    GetNullableInt(reader, "idMovOrigen"),
                    GetString(reader, "observaciones"),
                    GetNullableDateTime(reader, "creado"),
                    GetNullableDateTime(reader, "actualizado"),
                    GetNullableInt(reader, "creadoPor"),
                    GetNullableInt(reader, "actualizadoPor"));
            }
        }

        if (header is null)
        {
            return null;
        }

        await using var linesCommand = new SqlCommand(linesSql, connection);
        linesCommand.CommandType = CommandType.Text;
        linesCommand.Parameters.Add("@idMovimiento", SqlDbType.Int).Value = movementId;
        linesCommand.Parameters.Add("@acumulado", SqlDbType.Bit).Value = false;

        var lines = new List<MovementDetailLineItem>();
        await using (var reader = await linesCommand.ExecuteReaderAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                lines.Add(new MovementDetailLineItem
                {
                    MovementLineId = GetInt(reader, "idCorteMovimiento"),
                    ProductId = GetInt(reader, "idCorte"),
                    ProductCode = GetLong(reader, "codigo"),
                    ProductName = GetString(reader, "corte"),
                    QuantityWeightKg = GetDecimal(reader, "cantKg"),
                    QuantityUnits = GetInt(reader, "cantUnidad"),
                    ScaleWeight = GetBool(reader, "pesoBalanza"),
                    AllowEntry = GetBool(reader, "permitirIngreso")
                });
            }
        }

        var userIds = new[] { header.CreatedByUserId, header.UpdatedByUserId }
            .Where(x => x.HasValue && x.Value > 0)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        var userNames = await GetUserNamesAsync(connection, userIds, cancellationToken);

        return new MovementDetailItem
        {
            MovementId = header.MovementId,
            MovementDate = header.MovementDate,
            OriginBranchId = header.OriginBranchId,
            OriginBranchName = header.OriginBranchName,
            DestinationBranchId = header.DestinationBranchId,
            DestinationBranchName = header.DestinationBranchName,
            OriginMovementId = header.OriginMovementId,
            Notes = header.Notes,
            CreatedAt = header.CreatedAt,
            CreatedByName = header.CreatedByUserId.HasValue && userNames.TryGetValue(header.CreatedByUserId.Value, out var createdBy)
                ? createdBy
                : string.Empty,
            UpdatedAt = header.UpdatedAt,
            UpdatedByName = header.UpdatedByUserId.HasValue && userNames.TryGetValue(header.UpdatedByUserId.Value, out var updatedBy)
                ? updatedBy
                : string.Empty,
            Lines = lines
        };
    }

    private async Task<string> ResolveBranchFilterNameAsync(int companyId, int branchId, CancellationToken cancellationToken)
    {
        if (branchId <= 0)
        {
            return string.Empty;
        }

        var branches = await branchLookupService.GetBranchesAsync(companyId, cancellationToken);
        return branches.FirstOrDefault(x => x.BranchId == branchId)?.BranchName ?? string.Empty;
    }

    private async Task<List<MovementRow>> GetMovementRowsAsync(
        string originBranchName,
        string destinationBranchName,
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.obtenerMovimientos @sucOrigen, @sucDestino, @fechaDesde, @fechaHasta, @texto;
            """;

        var items = new List<MovementRow>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@sucOrigen", SqlDbType.NVarChar, 200).Value = originBranchName;
        command.Parameters.Add("@sucDestino", SqlDbType.NVarChar, 200).Value = destinationBranchName;
        command.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = dateFrom;
        command.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = dateTo;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = string.Empty;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new MovementRow(reader));
        }

        return items;
    }

    private async Task<Dictionary<int, MovementTotals>> GetTotalsByMovementAsync(IReadOnlyCollection<int> movementIds, CancellationToken cancellationToken)
    {
        var ids = movementIds.Where(x => x > 0).Distinct().ToArray();
        var results = new Dictionary<int, MovementTotals>();
        if (ids.Length == 0)
        {
            return results;
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        const int batchSize = 500;
        for (var i = 0; i < ids.Length; i += batchSize)
        {
            var batch = ids.Skip(i).Take(batchSize).ToArray();
            var sql = """
                SELECT CPM.idMovimientos AS idMovimiento,
                       SUM(ISNULL(CPM.cantUnidad, 0)) AS totalUnidad,
                       SUM(ISNULL(CPM.cantKg, 0)) AS totalKilos
                FROM dbo.CortePorMovimiento CPM
                WHERE CPM.idMovimientos IN (
            """ + string.Join(",", batch) + """
                )
                GROUP BY CPM.idMovimientos;
                """;

            await using var command = new SqlCommand(sql, connection);
            command.CommandType = CommandType.Text;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var movementId = GetInt(reader, "idMovimiento");
                results[movementId] = new MovementTotals(
                    GetDecimal(reader, "totalUnidad"),
                    GetDecimal(reader, "totalKilos"));
            }
        }

        return results;
    }

    private static async Task<Dictionary<int, string>> GetUserNamesAsync(SqlConnection connection, IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x > 0).Distinct().ToArray();
        var result = new Dictionary<int, string>();
        if (ids.Length == 0)
        {
            return result;
        }

        var sql = """
            SELECT id, nombre
            FROM dbo.Usuarios
            WHERE id IN (
        """ + string.Join(",", ids) + """
            );
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result[GetInt(reader, "id")] = GetString(reader, "nombre");
        }

        return result;
    }

    private static int GetInt(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal));
            }
        }

        return 0;
    }

    private static int? GetNullableInt(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? null : Convert.ToInt32(record.GetValue(ordinal));
            }
        }

        return null;
    }

    private static long GetLong(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? 0L : Convert.ToInt64(record.GetValue(ordinal));
            }
        }

        return 0L;
    }

    private static decimal GetDecimal(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(record.GetValue(ordinal));
            }
        }

        return 0m;
    }

    private static string GetString(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? string.Empty : Convert.ToString(record.GetValue(ordinal)) ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static bool GetBool(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return !record.IsDBNull(ordinal) && Convert.ToBoolean(record.GetValue(ordinal));
            }
        }

        return false;
    }

    private static DateTime GetDateTime(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? DateTime.MinValue : Convert.ToDateTime(record.GetValue(ordinal));
            }
        }

        return DateTime.MinValue;
    }

    private static DateTime? GetNullableDateTime(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? null : Convert.ToDateTime(record.GetValue(ordinal));
            }
        }

        return null;
    }

    private static int TryGetOrdinal(IDataRecord record, string columnName)
    {
        for (var index = 0; index < record.FieldCount; index++)
        {
            if (string.Equals(record.GetName(index), columnName, StringComparison.OrdinalIgnoreCase))
            {
                return index;
            }
        }

        return -1;
    }

    private sealed record MovementTotals(decimal TotalUnits, decimal TotalWeightKg);

    private sealed record MovementHeader(
        int MovementId,
        DateTime MovementDate,
        int OriginBranchId,
        string OriginBranchName,
        int DestinationBranchId,
        string DestinationBranchName,
        int? OriginMovementId,
        string Notes,
        DateTime? CreatedAt,
        DateTime? UpdatedAt,
        int? CreatedByUserId,
        int? UpdatedByUserId);

    private sealed class MovementRow : IDataRecord
    {
        private readonly List<string> _names = [];
        private readonly List<object?> _values = [];

        public MovementRow(IDataRecord source)
        {
            for (var index = 0; index < source.FieldCount; index++)
            {
                _names.Add(source.GetName(index));
                _values.Add(source.IsDBNull(index) ? null : source.GetValue(index));
            }
        }

        public int FieldCount => _names.Count;
        public object this[int i] => _values[i] ?? DBNull.Value;
        public object this[string name] => _values[GetOrdinal(name)] ?? DBNull.Value;
        public bool GetBoolean(int i) => Convert.ToBoolean(_values[i]);
        public byte GetByte(int i) => Convert.ToByte(_values[i]);
        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public char GetChar(int i) => Convert.ToChar(_values[i]);
        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length) => throw new NotSupportedException();
        public IDataReader GetData(int i) => throw new NotSupportedException();
        public string GetDataTypeName(int i) => GetFieldType(i).Name;
        public DateTime GetDateTime(int i) => Convert.ToDateTime(_values[i]);
        public decimal GetDecimal(int i) => Convert.ToDecimal(_values[i]);
        public double GetDouble(int i) => Convert.ToDouble(_values[i]);
        public Type GetFieldType(int i) => (_values[i] ?? string.Empty).GetType();
        public float GetFloat(int i) => Convert.ToSingle(_values[i]);
        public Guid GetGuid(int i) => _values[i] is Guid guid ? guid : Guid.Empty;
        public short GetInt16(int i) => Convert.ToInt16(_values[i]);
        public int GetInt32(int i) => Convert.ToInt32(_values[i]);
        public long GetInt64(int i) => Convert.ToInt64(_values[i]);
        public string GetName(int i) => _names[i];
        public int GetOrdinal(string name) => _names.FindIndex(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase));
        public string GetString(int i) => Convert.ToString(_values[i]) ?? string.Empty;
        public object GetValue(int i) => _values[i] ?? DBNull.Value;
        public int GetValues(object[] values)
        {
            var count = Math.Min(values.Length, _values.Count);
            for (var i = 0; i < count; i++)
            {
                values[i] = _values[i] ?? DBNull.Value;
            }

            return count;
        }
        public bool IsDBNull(int i) => _values[i] is null || _values[i] == DBNull.Value;
    }
}
