using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Stock;
using System.Globalization;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyStockCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IStockCommandService
{
    public async Task<StockSaveResult> SaveStockAsync(
        StockSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0 || request.UserId <= 0)
        {
            return StockSaveResult.Failure("No fue posible resolver la empresa o el usuario actual.");
        }

        if (request.BranchId <= 0)
        {
            return StockSaveResult.Failure("Debe seleccionar una sucursal valida.");
        }

        var normalizedType = (request.StockOperationType ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedType))
        {
            return StockSaveResult.Failure("Debe indicar el tipo de movimiento de stock.");
        }

        var lines = (request.Lines ?? [])
            .Where(x => x is not null)
            .ToArray();

        if (lines.Length == 0)
        {
            return StockSaveResult.Failure("Debe agregar al menos una linea al movimiento.");
        }

        if (lines.Any(x => x.ProductId <= 0))
        {
            return StockSaveResult.Failure("Todas las lineas deben tener un producto valido.");
        }

        if (!string.Equals(normalizedType, "Cierre Stock", StringComparison.OrdinalIgnoreCase)
            && lines.Any(x => x.QuantityKg == 0))
        {
            return StockSaveResult.Failure("Todas las lineas deben tener una cantidad distinta de cero.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        if (!await BranchBelongsToCompanyAsync(connection, request.CompanyId, request.BranchId, cancellationToken))
        {
            return StockSaveResult.Failure("La sucursal no pertenece a la empresa actual.");
        }

        if (!await AllProductsBelongToCompanyAsync(connection, request.CompanyId, lines.Select(x => x.ProductId).Distinct().ToArray(), cancellationToken))
        {
            return StockSaveResult.Failure("Uno o mas productos no pertenecen a la empresa actual.");
        }

        if (request.StockId > 0 && !await StockExistsAsync(connection, request.CompanyId, request.StockId, cancellationToken))
        {
            return StockSaveResult.Failure("No se encontro el movimiento de stock a modificar.");
        }

        var supplierId = await ResolveSupplierIdAsync(connection, request.CompanyId, request.SupplierId, cancellationToken);
        if (supplierId <= 0)
        {
            return StockSaveResult.Failure("No se pudo resolver el proveedor para el movimiento.");
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var stockId = await SaveHeaderAsync(connection, transaction, request, supplierId, cancellationToken);
            await SaveLinesAsync(connection, transaction, request, stockId, lines, cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return StockSaveResult.Ok(stockId);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<StockGenerateWeighingAdjustmentResult> GenerateWeighingAdjustmentAsync(
        StockGenerateWeighingAdjustmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0 || request.UserId <= 0 || request.WeighingId <= 0)
        {
            return StockGenerateWeighingAdjustmentResult.Failure("No fue posible resolver la empresa, el usuario o el pesaje actual.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        var weighing = await GetWeighingForAdjustmentAsync(connection, request.CompanyId, request.WeighingId, cancellationToken);
        if (weighing is null || !string.Equals(weighing.StockOperationType, "Pesaje Cortes", StringComparison.OrdinalIgnoreCase))
        {
            return StockGenerateWeighingAdjustmentResult.Failure("No se encontro el pesaje seleccionado.");
        }

        if (!weighing.HalfCarcassCount.HasValue || weighing.HalfCarcassCount.Value <= 0
            || !weighing.HalfCarcassWeightKg.HasValue || weighing.HalfCarcassWeightKg.Value <= 0)
        {
            return StockGenerateWeighingAdjustmentResult.Failure("El pesaje no tiene registrado KgsMedias y CantMedias. Ingrese KgsMedias y CantMedias, presione Guardar y vuelva a intentarlo.");
        }

        var supplierId = weighing.SupplierId > 0
            ? weighing.SupplierId
            : await ResolveSupplierIdAsync(connection, request.CompanyId, 0, cancellationToken);
        if (supplierId <= 0)
        {
            return StockGenerateWeighingAdjustmentResult.Failure("No se pudo resolver el proveedor para generar el ajuste.");
        }

        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            var adjustmentId = await GetAdjustmentIdForWeighingAsync(connection, transaction, request.WeighingId, cancellationToken);
            var existingAdjustment = adjustmentId > 0
                ? await GetAdjustmentForUpdateAsync(connection, transaction, request.CompanyId, adjustmentId, cancellationToken)
                : null;

            var adjustmentNotes = existingAdjustment?.Notes ?? string.Empty;
            if (string.Equals(adjustmentNotes.Trim(), "ID Pesaje: " + request.WeighingId.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
            {
                adjustmentNotes = string.Empty;
            }

            adjustmentId = await SaveAdjustmentHeaderAsync(
                connection,
                transaction,
                new AdjustmentHeaderSaveItem
                {
                    AdjustmentId = adjustmentId,
                    WeighingId = request.WeighingId,
                    OperationDate = weighing.OperationDate,
                    SupplierId = supplierId,
                    HalfCarcassCount = weighing.HalfCarcassCount,
                    HalfCarcassWeightKg = weighing.HalfCarcassWeightKg,
                    BranchId = weighing.BranchId,
                    Notes = adjustmentNotes,
                    UserId = request.UserId
                },
                cancellationToken);

            if (adjustmentId <= 0)
            {
                throw new InvalidOperationException("No se pudo registrar el encabezado del ajuste.");
            }

            if (existingAdjustment is not null)
            {
                await ClearAdjustmentLinesAsync(connection, transaction, adjustmentId, cancellationToken);
            }

            var differences = await ExecuteStoredProcedureTableAsync(connection, transaction, "getPorcCortesEnMedias", request.WeighingId, cancellationToken);
            NormalizeCutPercentagesTable(differences);

            foreach (DataRow row in differences.Rows)
            {
                if (!differences.Columns.Contains("idCorte") || row["idCorte"] == DBNull.Value)
                {
                    continue;
                }

                if (!int.TryParse(Convert.ToString(row["idCorte"], CultureInfo.InvariantCulture), out var productId) || productId <= 0)
                {
                    continue;
                }

                if (!TryConvertToDecimal(row["Dif."], out var quantityKg))
                {
                    throw new InvalidOperationException("No se pudo interpretar la diferencia de uno de los productos.");
                }

                await InsertAdjustmentLineAsync(
                    connection,
                    transaction,
                    adjustmentId,
                    productId,
                    weighing.BranchId,
                    quantityKg,
                    request.UserId,
                    cancellationToken);
            }

            await UpdateWeighingStatusAsync(connection, transaction, request.WeighingId, "Actualizado", cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return StockGenerateWeighingAdjustmentResult.Ok(adjustmentId, "Actualizado");
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

        var parameterNames = productIds.Select((_, index) => "@productId" + index).ToArray();
        var sql = """
            SELECT COUNT(*)
            FROM dbo.Corte
            WHERE idEmpresa = @idEmpresa
              AND idCorte IN ({0});
            """;
        sql = string.Format(sql, string.Join(", ", parameterNames));

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        for (var index = 0; index < parameterNames.Length; index++)
        {
            command.Parameters.Add(parameterNames[index], SqlDbType.Int).Value = productIds.ElementAt(index);
        }

        var result = await command.ExecuteScalarAsync(cancellationToken);
        var found = result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
        return found == productIds.Count;
    }

    private static async Task<bool> StockExistsAsync(
        SqlConnection connection,
        int companyId,
        int stockId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 idCompra
            FROM dbo.Compras
            WHERE idEmpresa = @idEmpresa
              AND idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value;
    }

    private static async Task<int> ResolveSupplierIdAsync(
        SqlConnection connection,
        int companyId,
        int supplierId,
        CancellationToken cancellationToken)
    {
        if (supplierId > 0)
        {
            return supplierId;
        }

        const string sql = """
            SELECT TOP 1 TRY_CONVERT(int, ep.valor)
            FROM dbo.Parametros p
            LEFT JOIN dbo.EmpresaParametros ep
                ON ep.idParametro = p.idParametro
               AND ep.idEmpresa = @idEmpresa
            WHERE p.nombre = 'idIndefinido';
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static async Task<int> SaveHeaderAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        StockSaveRequest request,
        int supplierId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.addOrEditCompra
                @idCompra,
                @nroRemito,
                @fechaCompra,
                @idProveedor,
                @cantMedias,
                @kgsMedias,
                @estado,
                @observaciones,
                @tipoCompra,
                @idSucursal,
                @creadoPor,
                @actualizadoPor,
                @enCtaCte,
                @idPesajeAjustado;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = request.StockId;
        command.Parameters.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = string.Empty;
        command.Parameters.Add("@fechaCompra", SqlDbType.DateTime).Value = request.OperationDate;
        command.Parameters.Add("@idProveedor", SqlDbType.Int).Value = supplierId;
        command.Parameters.Add("@cantMedias", SqlDbType.Int).Value = request.HalfCarcassCount.HasValue ? (object)request.HalfCarcassCount.Value : DBNull.Value;
        command.Parameters.Add("@kgsMedias", SqlDbType.Int).Value = request.HalfCarcassWeightKg.HasValue ? (object)Convert.ToInt32(Math.Round(request.HalfCarcassWeightKg.Value, MidpointRounding.AwayFromZero)) : DBNull.Value;
        command.Parameters.Add("@estado", SqlDbType.NVarChar, 50).Value = string.Empty;
        command.Parameters.Add("@observaciones", SqlDbType.NVarChar).Value = (request.Notes ?? string.Empty).Trim();
        command.Parameters.Add("@tipoCompra", SqlDbType.NVarChar, 50).Value = request.StockOperationType;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = request.BranchId;
        command.Parameters.Add("@creadoPor", SqlDbType.Int).Value = request.UserId;
        command.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = request.StockId > 0 ? (object)request.UserId : DBNull.Value;
        command.Parameters.Add("@enCtaCte", SqlDbType.TinyInt).Value = 0;
        command.Parameters.Add("@idPesajeAjustado", SqlDbType.Int).Value = request.LinkedWeighingId.HasValue ? (object)request.LinkedWeighingId.Value : DBNull.Value;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static async Task SaveLinesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        StockSaveRequest request,
        int stockId,
        IReadOnlyCollection<StockSaveLineRequest> lines,
        CancellationToken cancellationToken)
    {
        const string insertSql = """
            EXEC dbo.agregarCortePorCompra
                @idCompra,
                @idCorte,
                @idSucursal,
                @precioKg,
                @cantKg,
                @balanza,
                @creado,
                @creadoPor;
            """;

        foreach (var line in lines)
        {
            var quantityKg = NormalizeLineQuantity(request.StockOperationType, line.QuantityKg);

            await using var command = new SqlCommand(insertSql, connection, transaction);
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;
            command.Parameters.Add("@idCorte", SqlDbType.Int).Value = line.ProductId;
            command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = request.BranchId;
            command.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = 0m;
            command.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = quantityKg;
            command.Parameters.Add("@balanza", SqlDbType.Bit).Value = line.ScaleWeight;
            command.Parameters.Add("@creado", SqlDbType.DateTime).Value = DateTime.Now;
            command.Parameters.Add("@creadoPor", SqlDbType.Int).Value = request.UserId;
            command.Parameters["@precioKg"].Precision = 18;
            command.Parameters["@precioKg"].Scale = 2;
            command.Parameters["@cantKg"].Precision = 18;
            command.Parameters["@cantKg"].Scale = 3;

            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static decimal NormalizeLineQuantity(string stockOperationType, decimal quantityKg)
    {
        if (string.Equals(stockOperationType, "Egreso Stock", StringComparison.OrdinalIgnoreCase) && quantityKg > 0)
        {
            return quantityKg * -1m;
        }

        return quantityKg;
    }

    private static async Task<WeighingAdjustmentSourceItem?> GetWeighingForAdjustmentAsync(
        SqlConnection connection,
        int companyId,
        int weighingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                c.idCompra,
                c.tipoCompra,
                c.fechaCompra,
                c.idProveedor,
                c.cantMedias,
                c.kgsMedias,
                c.idSucursal
            FROM dbo.Compras c
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = weighingId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new WeighingAdjustmentSourceItem(
            GetInt(reader, "idCompra"),
            GetString(reader, "tipoCompra"),
            GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
            GetNullableInt(reader, "idProveedor") ?? 0,
            GetNullableInt(reader, "cantMedias"),
            GetNullableDecimal(reader, "kgsMedias"),
            GetInt(reader, "idSucursal"));
    }

    private static async Task<int> GetAdjustmentIdForWeighingAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int weighingId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 c.idCompra
            FROM dbo.Compras c
            WHERE c.tipoCompra = @tipo
              AND (c.idPesajeAjustado = @idPesaje OR c.nroRemito = @nroRemito)
            ORDER BY CASE WHEN c.idPesajeAjustado = @idPesaje THEN 0 ELSE 1 END, c.idCompra DESC;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = "Ajuste Stock";
        command.Parameters.Add("@idPesaje", SqlDbType.Int).Value = weighingId;
        command.Parameters.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = weighingId.ToString(CultureInfo.InvariantCulture);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task<AdjustmentUpdateItem?> GetAdjustmentForUpdateAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int companyId,
        int adjustmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                c.idCompra,
                c.observaciones
            FROM dbo.Compras c
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = adjustmentId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new AdjustmentUpdateItem(
            GetInt(reader, "idCompra"),
            GetString(reader, "observaciones"));
    }

    private static async Task<int> SaveAdjustmentHeaderAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        AdjustmentHeaderSaveItem item,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.addOrEditCompra
                @idCompra,
                @nroRemito,
                @fechaCompra,
                @idProveedor,
                @cantMedias,
                @kgsMedias,
                @estado,
                @observaciones,
                @tipoCompra,
                @idSucursal,
                @creadoPor,
                @actualizadoPor,
                @enCtaCte,
                @idPesajeAjustado;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = item.AdjustmentId;
        command.Parameters.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = item.WeighingId.ToString(CultureInfo.InvariantCulture);
        command.Parameters.Add("@fechaCompra", SqlDbType.DateTime).Value = item.OperationDate;
        command.Parameters.Add("@idProveedor", SqlDbType.Int).Value = item.SupplierId;
        command.Parameters.Add("@cantMedias", SqlDbType.Int).Value = item.HalfCarcassCount.HasValue ? (object)item.HalfCarcassCount.Value : DBNull.Value;
        command.Parameters.Add("@kgsMedias", SqlDbType.Int).Value = item.HalfCarcassWeightKg.HasValue ? (object)Convert.ToInt32(Math.Round(item.HalfCarcassWeightKg.Value, MidpointRounding.AwayFromZero)) : DBNull.Value;
        command.Parameters.Add("@estado", SqlDbType.NVarChar, 50).Value = string.Empty;
        command.Parameters.Add("@observaciones", SqlDbType.NVarChar).Value = (item.Notes ?? string.Empty).Trim();
        command.Parameters.Add("@tipoCompra", SqlDbType.NVarChar, 50).Value = "Ajuste Stock";
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = item.BranchId;
        command.Parameters.Add("@creadoPor", SqlDbType.Int).Value = item.UserId;
        command.Parameters.Add("@actualizadoPor", SqlDbType.Int).Value = item.AdjustmentId > 0 ? (object)item.UserId : DBNull.Value;
        command.Parameters.Add("@enCtaCte", SqlDbType.TinyInt).Value = 0;
        command.Parameters.Add("@idPesajeAjustado", SqlDbType.Int).Value = item.WeighingId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task ClearAdjustmentLinesAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int adjustmentId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            DELETE FROM dbo.CortePorCompra
            WHERE idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = adjustmentId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<DataTable> ExecuteStoredProcedureTableAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        string storedProcedureName,
        int stockId,
        CancellationToken cancellationToken)
    {
        var table = new DataTable();
        await using var command = new SqlCommand(storedProcedureName, connection, transaction);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@id", SqlDbType.Int).Value = stockId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        table.Load(reader);
        return table;
    }

    private static void NormalizeCutPercentagesTable(DataTable table)
    {
        if (table.Rows.Count == 0 || !table.Columns.Contains("Gan."))
        {
            return;
        }

        decimal totalGain = 0m;
        var lastRowIndex = table.Rows.Count - 1;

        for (var rowIndex = 0; rowIndex < table.Rows.Count; rowIndex++)
        {
            if (rowIndex == lastRowIndex)
            {
                table.Rows[rowIndex]["Gan."] = totalGain;
                if (table.Columns.Contains("Codigo"))
                {
                    table.Rows[rowIndex]["Codigo"] = DBNull.Value;
                }

                continue;
            }

            if (TryConvertToDecimal(table.Rows[rowIndex]["Gan."], out var rowGain))
            {
                totalGain += rowGain;
            }
        }
    }

    private static async Task InsertAdjustmentLineAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int adjustmentId,
        int productId,
        int branchId,
        decimal quantityKg,
        int userId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            EXEC dbo.agregarCortePorCompra
                @idCompra,
                @idCorte,
                @idSucursal,
                @precioKg,
                @cantKg,
                @balanza,
                @creado,
                @creadoPor;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = adjustmentId;
        command.Parameters.Add("@idCorte", SqlDbType.Int).Value = productId;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
        command.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = 0m;
        command.Parameters.Add("@cantKg", SqlDbType.Decimal).Value = quantityKg;
        command.Parameters.Add("@balanza", SqlDbType.Bit).Value = false;
        command.Parameters.Add("@creado", SqlDbType.DateTime).Value = DateTime.Now;
        command.Parameters.Add("@creadoPor", SqlDbType.Int).Value = userId;
        command.Parameters["@precioKg"].Precision = 18;
        command.Parameters["@precioKg"].Scale = 2;
        command.Parameters["@cantKg"].Precision = 18;
        command.Parameters["@cantKg"].Scale = 3;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task UpdateWeighingStatusAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        int weighingId,
        string status,
        CancellationToken cancellationToken)
    {
        const string sql = """
            UPDATE dbo.Compras
            SET estado = @estado
            WHERE idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@estado", SqlDbType.NVarChar, 50).Value = status;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = weighingId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static bool TryConvertToDecimal(object? value, out decimal number)
    {
        number = 0m;
        if (value == null || value == DBNull.Value)
        {
            return false;
        }

        switch (value)
        {
            case decimal decimalValue:
                number = decimalValue;
                return true;
            case double doubleValue:
                number = Convert.ToDecimal(doubleValue, CultureInfo.InvariantCulture);
                return true;
            case float floatValue:
                number = Convert.ToDecimal(floatValue, CultureInfo.InvariantCulture);
                return true;
            case int intValue:
                number = intValue;
                return true;
            case long longValue:
                number = longValue;
                return true;
        }

        return decimal.TryParse(
            Convert.ToString(value, CultureInfo.InvariantCulture),
            NumberStyles.Any,
            CultureInfo.InvariantCulture,
            out number)
            || decimal.TryParse(
                Convert.ToString(value, CultureInfo.InvariantCulture),
                NumberStyles.Any,
                CultureInfo.GetCultureInfo("es-AR"),
                out number);
    }

    private static int GetInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : Convert.ToInt32(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : Convert.ToDecimal(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : Convert.ToDateTime(reader.GetValue(ordinal), CultureInfo.InvariantCulture);
    }

    private static string GetString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? string.Empty : Convert.ToString(reader.GetValue(ordinal), CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private sealed record WeighingAdjustmentSourceItem(
        int StockId,
        string StockOperationType,
        DateTime OperationDate,
        int SupplierId,
        int? HalfCarcassCount,
        decimal? HalfCarcassWeightKg,
        int BranchId);

    private sealed record AdjustmentUpdateItem(
        int AdjustmentId,
        string Notes);

    private sealed class AdjustmentHeaderSaveItem
    {
        public int AdjustmentId { get; init; }

        public int WeighingId { get; init; }

        public DateTime OperationDate { get; init; }

        public int SupplierId { get; init; }

        public int? HalfCarcassCount { get; init; }

        public decimal? HalfCarcassWeightKg { get; init; }

        public int BranchId { get; init; }

        public string Notes { get; init; } = string.Empty;

        public int UserId { get; init; }
    }
}
