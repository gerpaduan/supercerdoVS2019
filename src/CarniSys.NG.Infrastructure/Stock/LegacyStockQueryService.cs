using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Stock;
using System.Globalization;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyStockQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IStockQueryService
{
    public async Task<StockMatrixResult> GetStockMatrixAsync(
        int companyId,
        StockMatrixQuery query,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return EmptyResult("No hay una empresa activa para consultar stock.");
        }

        var normalizedSearchText = (query.SearchText ?? string.Empty).Trim();
        var normalizedType = (query.Type ?? string.Empty).Trim();
        var normalizedState = NormalizeState(query.StockState);
        var rows = new List<FlatStockRow>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand("dbo.a_ExistenciaStockPorSucursales", connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = normalizedSearchText;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = query.BranchId;
        command.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = query.UntilDate.HasValue ? query.UntilDate.Value : DBNull.Value;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = normalizedType;
        command.Parameters.Add("@idProveedor", SqlDbType.Int).Value = 0;
        command.Parameters.Add("@idMarca", SqlDbType.Int).Value = 0;
        command.Parameters.Add("@idCorte", SqlDbType.Int).Value = 0;
        command.Parameters.Add("@soloConStock", SqlDbType.Bit).Value = query.OnlyWithStock;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var row = new FlatStockRow
            {
                ProductId = GetInt(reader, "idCorte"),
                Code = GetLong(reader, "Codigo"),
                Description = GetString(reader, "Corte"),
                BranchId = GetInt(reader, "idSucursal"),
                BranchName = GetString(reader, "Sucursal"),
                LastStockClosingDate = GetNullableDateTime(reader, "FechaUltimoCierre"),
                StockInitial = GetDecimal(reader, "StockInicial"),
                TotalEntries = GetDecimal(reader, "TotalIngresos"),
                TotalExits = GetDecimal(reader, "TotalEgresos"),
                StockActual = GetDecimal(reader, "StockActual"),
                StockPoint = GetDecimal(reader, "PuntoStock"),
                StockState = NormalizeState(GetString(reader, "EstadoStock"))
            };

            row.DifferenceFromPoint = row.StockActual - row.StockPoint;
            rows.Add(row);
        }

        await ApplyBranchStockPointsAsync(connection, companyId, rows, cancellationToken);

        if (normalizedState != "TODOS")
        {
            rows = rows.Where(x => x.StockState == normalizedState).ToList();
        }

        if (rows.Count == 0)
        {
            return EmptyResult("No se encontraron datos para los filtros indicados.");
        }

        var columns = rows
            .GroupBy(x => new { x.BranchId, x.BranchName })
            .Select(g => new StockBranchColumnItem
            {
                BranchId = g.Key.BranchId,
                BranchName = g.Key.BranchName
            })
            .OrderBy(x => x.BranchName)
            .ToArray();

        var items = rows
            .GroupBy(x => new { x.ProductId, x.Code, x.Description })
            .OrderBy(g => g.Key.Code)
            .ThenBy(g => g.Key.Description)
            .Select(group =>
            {
                var branchMap = group.ToDictionary(x => x.BranchId);
                var details = new List<StockBranchDetailItem>(columns.Length);
                var cells = new List<StockBranchCellItem>(columns.Length);

                foreach (var column in columns)
                {
                    if (!branchMap.TryGetValue(column.BranchId, out var row))
                    {
                        row = new FlatStockRow
                        {
                            BranchId = column.BranchId,
                            BranchName = column.BranchName,
                            StockState = "SIN STOCK"
                        };
                    }

                    details.Add(new StockBranchDetailItem
                    {
                        BranchId = row.BranchId,
                        BranchName = row.BranchName,
                        LastStockClosingDate = row.LastStockClosingDate,
                        StockInitial = row.StockInitial,
                        TotalEntries = row.TotalEntries,
                        TotalExits = row.TotalExits,
                        StockActual = row.StockActual,
                        StockPoint = row.StockPoint,
                        DifferenceFromPoint = row.DifferenceFromPoint,
                        StockState = row.StockState
                    });

                    cells.Add(new StockBranchCellItem
                    {
                        BranchId = row.BranchId,
                        BranchName = row.BranchName,
                        StockActual = row.StockActual,
                        StockState = row.StockState
                    });
                }

                return new StockProductMatrixItem
                {
                    ProductId = group.Key.ProductId,
                    Code = group.Key.Code,
                    Description = group.Key.Description,
                    Cells = cells,
                    Details = details,
                    HasPositiveStock = details.Any(x => x.StockActual > 0)
                };
            })
            .ToArray();

        return new StockMatrixResult
        {
            Columns = columns,
            Items = items,
            Message = items.Length == 0 ? "No se encontraron datos para los filtros indicados." : string.Empty
        };
    }

    public async Task<StockEditDetailItem?> GetStockEditByIdAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || stockId <= 0)
        {
            return null;
        }

        const string headerSql = """
            SELECT TOP 1
                c.idCompra,
                c.tipoCompra,
                c.idSucursal,
                s.sucursal,
                c.fechaCompra,
                c.observaciones,
                c.estado,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.cantMedias,
                c.kgsMedias,
                c.idPesajeAjustado,
                c.creado,
                c.actualizado,
                c.creadoPor,
                c.actualizadoPor
            FROM dbo.Compras c
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = c.idSucursal
            LEFT JOIN dbo.Personas p ON p.idPersona = c.idProveedor
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra;
            """;

        const string linesSql = """
            SELECT
                cpc.idCorte,
                corte.codigo,
                corte.corte,
                cpc.cantKg,
                cpc.balanza,
                cpc.creado,
                corte.pesable,
                cpc.idCortePorCompra
            FROM dbo.CortePorCompra cpc
            INNER JOIN dbo.Compras c ON c.idCompra = cpc.idCompra
            INNER JOIN dbo.Corte corte ON corte.idCorte = cpc.idCorte
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra
            ORDER BY cpc.creado, cpc.idCortePorCompra;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        StockEditHeader? header = null;
        await using (var headerCommand = new SqlCommand(headerSql, connection))
        {
            headerCommand.CommandType = CommandType.Text;
            headerCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
            headerCommand.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

            await using var reader = await headerCommand.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                header = new StockEditHeader(
                    GetInt(reader, "idCompra"),
                    GetString(reader, "tipoCompra"),
                    GetInt(reader, "idSucursal"),
                    GetString(reader, "sucursal"),
                    GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
                    GetString(reader, "observaciones"),
                    GetString(reader, "estado"),
                    GetInt(reader, "idProveedor"),
                    GetString(reader, "razonSocial"),
                    GetString(reader, "cuit"),
                    GetNullableInt(reader, "cantMedias"),
                    GetNullableDecimal(reader, "kgsMedias"),
                    GetNullableInt(reader, "idPesajeAjustado"),
                    FormatDateTime(GetNullableDateTime(reader, "creado")),
                    string.Empty,
                    FormatDateTime(GetNullableDateTime(reader, "actualizado")),
                    string.Empty,
                    GetNullableInt(reader, "creadoPor"),
                    GetNullableInt(reader, "actualizadoPor"));
            }
        }

        if (header is null)
        {
            return null;
        }

        var lines = new List<StockEditLineItem>();
        await using (var linesCommand = new SqlCommand(linesSql, connection))
        {
            linesCommand.CommandType = CommandType.Text;
            linesCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
            linesCommand.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

            await using var reader = await linesCommand.ExecuteReaderAsync(cancellationToken);
            var index = 0;
            while (await reader.ReadAsync(cancellationToken))
            {
                index++;
                lines.Add(new StockEditLineItem
                {
                    Index = index,
                    ProductId = GetInt(reader, "idCorte"),
                    Code = GetLong(reader, "codigo"),
                    ProductName = GetString(reader, "corte"),
                    QuantityKg = GetDecimal(reader, "cantKg"),
                    ScaleWeight = GetBool(reader, "balanza"),
                    CreatedLabel = FormatDateTime(GetNullableDateTime(reader, "creado")),
                    IsWeighable = GetBool(reader, "pesable")
                });
            }
        }

        var userIds = new[] { header.CreatedByUserId, header.UpdatedByUserId }
            .Where(x => x.HasValue && x.Value > 0)
            .Select(x => x!.Value)
            .Distinct()
            .ToArray();
        var userNames = await GetUserNamesAsync(connection, companyId, userIds, cancellationToken);
        StockWeighingPurchaseLookupItem? linkedPurchase = null;
        StockWeighingPurchaseLookupItem? adjustedWeighing = null;
        if (string.Equals(header.StockOperationType, "Pesaje Cortes", StringComparison.OrdinalIgnoreCase)
            && header.LinkedWeighingId.HasValue
            && header.LinkedWeighingId.Value > 0)
        {
            linkedPurchase = await GetWeighingPurchaseByIdAsync(companyId, header.LinkedWeighingId.Value, cancellationToken);
        }
        else if (string.Equals(header.StockOperationType, "Ajuste Stock", StringComparison.OrdinalIgnoreCase)
            && header.LinkedWeighingId.HasValue
            && header.LinkedWeighingId.Value > 0)
        {
            adjustedWeighing = await GetWeighingPurchaseByIdAsync(companyId, header.LinkedWeighingId.Value, cancellationToken);
        }

        return new StockEditDetailItem
        {
            StockId = header.StockId,
            StockOperationType = header.StockOperationType,
            BranchId = header.BranchId,
            BranchName = header.BranchName,
            OperationDate = header.OperationDate,
            Notes = header.Notes,
            Status = header.Status,
            SupplierId = header.SupplierId,
            SupplierName = header.SupplierName,
            SupplierTaxId = header.SupplierTaxId,
            HalfCarcassCount = header.HalfCarcassCount,
            HalfCarcassWeightKg = header.HalfCarcassWeightKg,
            LinkedWeighingId = header.LinkedWeighingId,
            LinkedPurchaseDate = linkedPurchase?.OperationDate,
            LinkedPurchaseSupplierName = linkedPurchase?.SupplierName ?? string.Empty,
            LinkedPurchaseHalfCarcassCount = linkedPurchase?.HalfCarcassCount,
            LinkedPurchaseWeightKg = linkedPurchase?.HalfCarcassWeightKg,
            AdjustedWeighingDate = adjustedWeighing?.OperationDate,
            AdjustedWeighingSupplierName = adjustedWeighing?.SupplierName ?? string.Empty,
            AdjustedWeighingHalfCarcassCount = adjustedWeighing?.HalfCarcassCount,
            AdjustedWeighingWeightKg = adjustedWeighing?.HalfCarcassWeightKg,
            CreatedAtLabel = header.CreatedAtLabel,
            CreatedByLabel = header.CreatedByUserId.HasValue && userNames.TryGetValue(header.CreatedByUserId.Value, out var createdBy)
                ? createdBy
                : string.Empty,
            UpdatedAtLabel = header.UpdatedAtLabel,
            UpdatedByLabel = header.UpdatedByUserId.HasValue && userNames.TryGetValue(header.UpdatedByUserId.Value, out var updatedBy)
                ? updatedBy
                : string.Empty,
            Lines = lines
        };
    }

    public async Task<IReadOnlyCollection<StockWeighingPurchaseLookupItem>> GetWeighingPurchasesAsync(
        int companyId,
        StockWeighingPurchaseQuery query,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || query.BranchId <= 0)
        {
            return Array.Empty<StockWeighingPurchaseLookupItem>();
        }

        const string sql = """
            SELECT
                c.idCompra,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.fechaCompra,
                c.tipoCompra,
                c.cantMedias,
                c.kgsMedias,
                s.sucursal,
                ISNULL(SUM(cpc.cantKg), 0) AS totalKg
            FROM dbo.Compras c
            LEFT JOIN dbo.Personas p ON p.idPersona = c.idProveedor
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = c.idSucursal
            LEFT JOIN dbo.CortePorCompra cpc ON cpc.idCompra = c.idCompra
            WHERE c.idEmpresa = @idEmpresa
              AND c.idSucursal = @idSucursal
              AND c.idCompra <> @idCompraActual
              AND (
                    (@soloPesajes = 1 AND c.tipoCompra = 'Pesaje Cortes')
                    OR (@soloPesajes = 0 AND c.tipoCompra IN ('Media Res', 'Cortes', 'Pesaje Cortes'))
                  )
              AND c.fechaCompra >= @fechaDesde
              AND c.fechaCompra < @fechaHasta
              AND (@proveedor = '' OR p.razonSocial LIKE '%' + @proveedor + '%')
            GROUP BY
                c.idCompra,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.fechaCompra,
                c.tipoCompra,
                c.cantMedias,
                c.kgsMedias,
                s.sucursal
            ORDER BY c.fechaCompra DESC, c.idCompra DESC;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = query.BranchId;
        command.Parameters.Add("@idCompraActual", SqlDbType.Int).Value = query.CurrentStockId;
        command.Parameters.Add("@soloPesajes", SqlDbType.Bit).Value = query.OnlyWeighings;
        command.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = (query.FromDate ?? DateTime.Today.AddDays(-7)).Date;
        command.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = ((query.ToDate ?? DateTime.Today).Date).AddDays(1);
        command.Parameters.Add("@proveedor", SqlDbType.NVarChar, 200).Value = (query.SupplierSearchText ?? string.Empty).Trim();

        var items = new List<StockWeighingPurchaseLookupItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new StockWeighingPurchaseLookupItem
            {
                StockId = GetInt(reader, "idCompra"),
                SupplierId = GetNullableInt(reader, "idProveedor") ?? 0,
                SupplierName = GetString(reader, "razonSocial"),
                SupplierTaxId = GetString(reader, "cuit"),
                OperationDate = GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
                StockOperationType = GetString(reader, "tipoCompra"),
                HalfCarcassCount = GetNullableInt(reader, "cantMedias"),
                HalfCarcassWeightKg = GetNullableDecimal(reader, "kgsMedias"),
                TotalQuantityKg = GetDecimal(reader, "totalKg")
            });
        }

        return items;
    }

    public async Task<StockWeighingPurchaseLookupItem?> GetWeighingPurchaseByIdAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || stockId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT
                c.idCompra,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.fechaCompra,
                c.tipoCompra,
                c.cantMedias,
                c.kgsMedias,
                ISNULL(SUM(cpc.cantKg), 0) AS totalKg
            FROM dbo.Compras c
            LEFT JOIN dbo.Personas p ON p.idPersona = c.idProveedor
            LEFT JOIN dbo.CortePorCompra cpc ON cpc.idCompra = c.idCompra
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra
            GROUP BY
                c.idCompra,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.fechaCompra,
                c.tipoCompra,
                c.cantMedias,
                c.kgsMedias;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new StockWeighingPurchaseLookupItem
        {
            StockId = GetInt(reader, "idCompra"),
            SupplierId = GetNullableInt(reader, "idProveedor") ?? 0,
            SupplierName = GetString(reader, "razonSocial"),
            SupplierTaxId = GetString(reader, "cuit"),
            OperationDate = GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
            StockOperationType = GetString(reader, "tipoCompra"),
            HalfCarcassCount = GetNullableInt(reader, "cantMedias"),
            HalfCarcassWeightKg = GetNullableDecimal(reader, "kgsMedias"),
            TotalQuantityKg = GetDecimal(reader, "totalKg")
        };
    }

    public async Task<StockWeighingPercentagesItem?> GetWeighingPercentagesAsync(
        int companyId,
        int stockId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || stockId <= 0)
        {
            return null;
        }

        const string weighingSql = """
            SELECT TOP 1
                c.idCompra,
                c.tipoCompra,
                c.cantMedias,
                c.kgsMedias,
                c.creado,
                c.actualizado
            FROM dbo.Compras c
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        WeighingHeader? weighing = null;
        await using (var command = new SqlCommand(weighingSql, connection))
        {
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
            command.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                weighing = new WeighingHeader(
                    GetInt(reader, "idCompra"),
                    GetString(reader, "tipoCompra"),
                    GetNullableInt(reader, "cantMedias"),
                    GetNullableDecimal(reader, "kgsMedias"),
                    GetNullableDateTime(reader, "creado"),
                    GetNullableDateTime(reader, "actualizado"));
            }
        }

        if (weighing is null || !string.Equals(weighing.StockOperationType, "Pesaje Cortes", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!weighing.HalfCarcassCount.HasValue || weighing.HalfCarcassCount.Value <= 0
            || !weighing.HalfCarcassWeightKg.HasValue || weighing.HalfCarcassWeightKg.Value <= 0)
        {
            return new StockWeighingPercentagesItem
            {
                HasRequiredMediaData = false
            };
        }

        var adjustmentId = await GetAdjustmentIdForWeighingAsync(connection, stockId, cancellationToken);
        var adjustment = adjustmentId > 0
            ? await GetPurchaseAuditAsync(connection, companyId, adjustmentId, cancellationToken)
            : null;

        var averageHalfCarcassesTable = await ExecuteTableStoredProcedureAsync(connection, "getPromMedias", stockId, cancellationToken);
        var cutPercentagesTable = await ExecuteTableStoredProcedureAsync(connection, "getPorcCortesEnMedias", stockId, cancellationToken);
        NormalizeCutPercentagesTable(cutPercentagesTable);

        return new StockWeighingPercentagesItem
        {
            HasRequiredMediaData = true,
            Status = ResolveAdjustmentStatus(weighing, adjustment),
            AverageHalfCarcassesTable = BuildModalTable(averageHalfCarcassesTable, hideProductId: false, threeDecimalsFromColumnIndex: -1),
            CutPercentagesTable = BuildModalTable(cutPercentagesTable, hideProductId: true, threeDecimalsFromColumnIndex: 2)
        };
    }

    public async Task<IReadOnlyCollection<StockMissingClosingProductItem>> GetMissingClosingProductsAsync(
        int companyId,
        int branchId,
        DateTime operationDate,
        IReadOnlyCollection<long> loadedCodes,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || branchId <= 0)
        {
            return Array.Empty<StockMissingClosingProductItem>();
        }

        const string productSql = """
            SELECT
                c.idCorte,
                c.codigo,
                c.corte,
                c.pesable,
                c.promedio
            FROM dbo.Corte c
            WHERE c.idEmpresa = @idEmpresa
              AND c.enCierreStock = 1
            ORDER BY c.codigo, c.corte;
            """;

        var loadedCodeSet = new HashSet<long>((loadedCodes ?? Array.Empty<long>()).Where(x => x > 0));
        var stockByProductId = new Dictionary<int, decimal>();
        var items = new List<StockMissingClosingProductItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using (var stockCommand = new SqlCommand("dbo.a_ExistenciaStockPorSucursales", connection))
        {
            stockCommand.CommandType = CommandType.StoredProcedure;
            stockCommand.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = string.Empty;
            stockCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
            stockCommand.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
            stockCommand.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = operationDate;
            stockCommand.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = string.Empty;
            stockCommand.Parameters.Add("@idProveedor", SqlDbType.Int).Value = 0;
            stockCommand.Parameters.Add("@idMarca", SqlDbType.Int).Value = 0;
            stockCommand.Parameters.Add("@idCorte", SqlDbType.Int).Value = 0;
            stockCommand.Parameters.Add("@soloConStock", SqlDbType.Bit).Value = false;

            await using var stockReader = await stockCommand.ExecuteReaderAsync(cancellationToken);
            while (await stockReader.ReadAsync(cancellationToken))
            {
                var productId = GetInt(stockReader, "idCorte");
                if (productId <= 0)
                {
                    continue;
                }

                stockByProductId[productId] = GetDecimal(stockReader, "StockActual");
            }
        }

        await using var productCommand = new SqlCommand(productSql, connection);
        productCommand.CommandType = CommandType.Text;
        productCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var productReader = await productCommand.ExecuteReaderAsync(cancellationToken);
        while (await productReader.ReadAsync(cancellationToken))
        {
            var code = GetLong(productReader, "codigo");
            if (code <= 0 || loadedCodeSet.Contains(code))
            {
                continue;
            }

            var productId = GetInt(productReader, "idCorte");
            items.Add(new StockMissingClosingProductItem
            {
                ProductId = productId,
                Code = code,
                ProductName = GetString(productReader, "corte"),
                CurrentStock = stockByProductId.GetValueOrDefault(productId, 0m),
                Weighable = GetBool(productReader, "pesable"),
                AverageWeight = GetDecimal(productReader, "promedio")
            });
        }

        return items;
    }

    private static StockMatrixResult EmptyResult(string message)
    {
        return new StockMatrixResult
        {
            Columns = Array.Empty<StockBranchColumnItem>(),
            Items = Array.Empty<StockProductMatrixItem>(),
            Message = message
        };
    }

    private static string NormalizeState(string? state)
    {
        var normalized = (state ?? string.Empty).Trim().ToUpperInvariant();
        return string.IsNullOrWhiteSpace(normalized) ? "TODOS" : normalized;
    }

    private static int GetInt(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? 0 : Convert.ToInt32(reader[columnName]);
    }

    private static long GetLong(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? 0L : Convert.ToInt64(reader[columnName]);
    }

    private static string GetString(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? string.Empty : Convert.ToString(reader[columnName]) ?? string.Empty;
    }

    private static decimal GetDecimal(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? 0m : Convert.ToDecimal(reader[columnName]);
    }

    private static bool GetBool(SqlDataReader reader, string columnName)
    {
        return reader[columnName] != DBNull.Value && Convert.ToBoolean(reader[columnName]);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? null : Convert.ToDateTime(reader[columnName]);
    }

    private static int? GetNullableInt(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? null : Convert.ToInt32(reader[columnName]);
    }

    private static decimal? GetNullableDecimal(SqlDataReader reader, string columnName)
    {
        return reader[columnName] == DBNull.Value ? null : Convert.ToDecimal(reader[columnName]);
    }

    private sealed class FlatStockRow
    {
        public int ProductId { get; init; }

        public long Code { get; init; }

        public string Description { get; init; } = string.Empty;

        public int BranchId { get; init; }

        public string BranchName { get; init; } = string.Empty;

        public DateTime? LastStockClosingDate { get; set; }

        public decimal StockInitial { get; set; }

        public decimal TotalEntries { get; set; }

        public decimal TotalExits { get; set; }

        public decimal StockActual { get; set; }

        public decimal StockPoint { get; set; }

        public decimal DifferenceFromPoint { get; set; }

        public string StockState { get; set; } = string.Empty;
    }

    private static async Task ApplyBranchStockPointsAsync(
        SqlConnection connection,
        int companyId,
        List<FlatStockRow> rows,
        CancellationToken cancellationToken)
    {
        if (rows.Count == 0)
        {
            return;
        }

        var productIds = rows.Select(x => x.ProductId).Where(x => x > 0).Distinct().ToList();
        if (productIds.Count == 0)
        {
            foreach (var row in rows)
            {
                row.StockState = CalculateStockState(row.StockActual, row.StockPoint);
            }

            return;
        }

        var parameterNames = productIds.Select((_, index) => "@productId" + index).ToArray();
        var sql = """
            IF OBJECT_ID('dbo.ProductStockPointByBranchNG', 'U') IS NULL
            BEGIN
                SELECT CAST(0 AS int) AS IdCorte, CAST(0 AS int) AS IdSucursal, CAST(0 AS int) AS PuntoStock
                WHERE 1 = 0;
                RETURN;
            END;
            SELECT IdCorte, IdSucursal, PuntoStock
            FROM dbo.ProductStockPointByBranchNG
            WHERE IdEmpresa = @idEmpresa
              AND IdCorte IN ({0});
            """;
        sql = string.Format(sql, string.Join(", ", parameterNames));

        var customPoints = new Dictionary<(int ProductId, int BranchId), decimal>();
        var productsWithCustomPoints = new HashSet<int>();

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        for (var index = 0; index < productIds.Count; index++)
        {
            command.Parameters.Add(parameterNames[index], SqlDbType.Int).Value = productIds[index];
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var productId = reader["IdCorte"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IdCorte"]);
            var branchId = reader["IdSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(reader["IdSucursal"]);
            var stockPoint = reader["PuntoStock"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["PuntoStock"]);

            customPoints[(productId, branchId)] = stockPoint;
            productsWithCustomPoints.Add(productId);
        }

        foreach (var row in rows)
        {
            if (productsWithCustomPoints.Contains(row.ProductId))
            {
                row.StockPoint = customPoints.GetValueOrDefault((row.ProductId, row.BranchId), 0m);
            }

            row.DifferenceFromPoint = row.StockActual - row.StockPoint;
            row.StockState = CalculateStockState(row.StockActual, row.StockPoint);
        }
    }

    private static string CalculateStockState(decimal stockActual, decimal stockPoint)
    {
        if (stockActual < 0)
        {
            return "NEGATIVO";
        }

        if (stockPoint > 0 && stockActual <= stockPoint)
        {
            return "BAJO";
        }

        if (stockActual == 0)
        {
            return "SIN STOCK";
        }

        return "OK";
    }

    private static string FormatDateTime(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("dd/MM/yyyy HH:mm") : string.Empty;
    }

    private static async Task<DataTable> ExecuteTableStoredProcedureAsync(
        SqlConnection connection,
        string storedProcedureName,
        int stockId,
        CancellationToken cancellationToken)
    {
        var table = new DataTable();

        await using var command = new SqlCommand(storedProcedureName, connection);
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.Add("@id", SqlDbType.Int).Value = stockId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        table.Load(reader);
        return table;
    }

    private static async Task<int> GetAdjustmentIdForWeighingAsync(
        SqlConnection connection,
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

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = "Ajuste Stock";
        command.Parameters.Add("@idPesaje", SqlDbType.Int).Value = weighingId;
        command.Parameters.Add("@nroRemito", SqlDbType.NVarChar, 50).Value = weighingId.ToString(CultureInfo.InvariantCulture);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result == null || result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    private static async Task<PurchaseAuditItem?> GetPurchaseAuditAsync(
        SqlConnection connection,
        int companyId,
        int stockId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                c.idCompra,
                c.creado,
                c.actualizado
            FROM dbo.Compras c
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = stockId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new PurchaseAuditItem(
            GetInt(reader, "idCompra"),
            GetNullableDateTime(reader, "creado"),
            GetNullableDateTime(reader, "actualizado"));
    }

    private static string ResolveAdjustmentStatus(WeighingHeader weighing, PurchaseAuditItem? adjustment)
    {
        if (adjustment is null)
        {
            return "No Realizado";
        }

        if (!weighing.UpdatedAt.HasValue)
        {
            return "Actualizado";
        }

        if (!adjustment.UpdatedAt.HasValue)
        {
            return adjustment.CreatedAt.HasValue && adjustment.CreatedAt.Value > weighing.UpdatedAt.Value
                ? "Actualizado"
                : "No Actualizado";
        }

        return weighing.UpdatedAt.Value > adjustment.UpdatedAt.Value
            ? "No Actualizado"
            : "Actualizado";
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

    private static StockModalTableItem BuildModalTable(
        DataTable table,
        bool hideProductId,
        int threeDecimalsFromColumnIndex)
    {
        var columns = new List<StockModalTableColumnItem>();
        var rows = new List<string[]>();

        for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
        {
            var column = table.Columns[columnIndex];
            columns.Add(new StockModalTableColumnItem
            {
                Name = column.ColumnName,
                Hidden = hideProductId && string.Equals(column.ColumnName, "idCorte", StringComparison.OrdinalIgnoreCase),
                RightAligned = IsNumeric(column.DataType) || (threeDecimalsFromColumnIndex >= 0 && columnIndex >= threeDecimalsFromColumnIndex),
                ThreeDecimalFormat = threeDecimalsFromColumnIndex >= 0 && columnIndex >= threeDecimalsFromColumnIndex
            });
        }

        foreach (DataRow row in table.Rows)
        {
            var values = new string[table.Columns.Count];
            for (var columnIndex = 0; columnIndex < table.Columns.Count; columnIndex++)
            {
                values[columnIndex] = FormatTableCell(row[columnIndex], table.Columns[columnIndex], columns[columnIndex].ThreeDecimalFormat);
            }

            rows.Add(values);
        }

        return new StockModalTableItem
        {
            Columns = columns,
            Rows = rows
        };
    }

    private static string FormatTableCell(object value, DataColumn column, bool threeDecimalFormat)
    {
        if (value == null || value == DBNull.Value)
        {
            return string.Empty;
        }

        var culture = CultureInfo.GetCultureInfo("es-AR");

        if (IsNumeric(column.DataType))
        {
            var number = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
            return number.ToString(threeDecimalFormat ? "F3" : "0.###", culture);
        }

        if (threeDecimalFormat && TryConvertToDecimal(value, out var flexibleNumber))
        {
            return flexibleNumber.ToString("F3", culture);
        }

        if (column.DataType == typeof(DateTime) && DateTime.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var parsedDate))
        {
            return parsedDate.ToString("dd/MM/yyyy HH:mm", culture);
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private static bool IsNumeric(Type type)
    {
        return type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(int)
            || type == typeof(long)
            || type == typeof(short)
            || type == typeof(byte);
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

    private static async Task<Dictionary<int, string>> GetUserNamesAsync(
        SqlConnection connection,
        int companyId,
        IReadOnlyCollection<int> userIds,
        CancellationToken cancellationToken)
    {
        var ids = userIds.Where(x => x > 0).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<int, string>();
        }

        var parameterNames = ids.Select((_, index) => "@userId" + index).ToArray();
        var sql = """
            SELECT id, nombre
            FROM dbo.Usuarios
            WHERE idEmpresa = @idEmpresa
              AND id IN ({0});
            """;
        sql = string.Format(sql, string.Join(", ", parameterNames));

        var items = new Dictionary<int, string>();

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        for (var index = 0; index < ids.Length; index++)
        {
            command.Parameters.Add(parameterNames[index], SqlDbType.Int).Value = ids[index];
        }

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items[GetInt(reader, "id")] = GetString(reader, "nombre");
        }

        return items;
    }

    private sealed record StockEditHeader(
        int StockId,
        string StockOperationType,
        int BranchId,
        string BranchName,
        DateTime OperationDate,
        string Notes,
        string Status,
        int SupplierId,
        string SupplierName,
        string SupplierTaxId,
        int? HalfCarcassCount,
        decimal? HalfCarcassWeightKg,
        int? LinkedWeighingId,
        string CreatedAtLabel,
        string CreatedByLabel,
        string UpdatedAtLabel,
        string UpdatedByLabel,
        int? CreatedByUserId,
        int? UpdatedByUserId);

    private sealed record WeighingHeader(
        int StockId,
        string StockOperationType,
        int? HalfCarcassCount,
        decimal? HalfCarcassWeightKg,
        DateTime? CreatedAt,
        DateTime? UpdatedAt);

    private sealed record PurchaseAuditItem(
        int StockId,
        DateTime? CreatedAt,
        DateTime? UpdatedAt);
}
