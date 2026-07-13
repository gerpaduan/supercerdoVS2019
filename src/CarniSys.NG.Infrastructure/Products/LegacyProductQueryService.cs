using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Products;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyProductQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IProductQueryService
{
    public async Task<IReadOnlyCollection<ProductListItem>> GetProductsAsync(
        int companyId,
        ProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return Array.Empty<ProductListItem>();
        }

        var normalizedSearchText = (query.SearchText ?? string.Empty).Trim();
        var normalizedType = (query.Type ?? string.Empty).Trim();
        var skip = Math.Max(0, query.Skip);
        var take = query.Take <= 0 ? 50 : Math.Min(query.Take, 100);

        const string sql = """
            SELECT
                c.idCorte,
                c.codigo,
                c.corte,
                c.tipo,
                c.precioKg,
                c.habilitado,
                c.pesable,
                c.creado,
                c.actualizado,
                m.razonSocial AS MarcaNombre
            FROM dbo.Corte c
            LEFT JOIN dbo.Personas m ON c.idMarca = m.idPersona
            WHERE c.idEmpresa = @idEmpresa
              AND (@texto = '' OR c.corte LIKE @buscar OR CAST(c.codigo AS NVARCHAR(50)) LIKE @buscar)
              AND (@tipo = '' OR c.tipo = @tipo)
            ORDER BY c.codigo ASC
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
            """;

        var items = new List<ProductListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 100).Value = normalizedSearchText;
        command.Parameters.Add("@buscar", SqlDbType.NVarChar, 110).Value = $"%{normalizedSearchText}%";
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = normalizedType;
        command.Parameters.Add("@skip", SqlDbType.Int).Value = skip;
        command.Parameters.Add("@take", SqlDbType.Int).Value = take;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductListItem
            {
                ProductId = GetInt(reader, "idCorte"),
                Code = GetLong(reader, "codigo"),
                Description = GetString(reader, "corte"),
                Type = GetString(reader, "tipo"),
                BrandName = GetString(reader, "MarcaNombre"),
                PricePerKilogram = GetDecimal(reader, "precioKg"),
                Enabled = GetBool(reader, "habilitado"),
                Weighable = GetBool(reader, "pesable"),
                CreatedAt = GetDateTime(reader, "creado"),
                UpdatedAt = GetNullableDateTime(reader, "actualizado")
            });
        }

        return items;
    }

    public async Task<ProductDetailItem?> GetProductByIdAsync(
        int companyId,
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || productId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1
                c.idCorte,
                c.idEmpresa,
                c.codigo,
                c.corte,
                c.tipo,
                c.idMarca,
                c.precioKg,
                c.pesable,
                c.promedio,
                c.idAlicuotaIva,
                c.alicuotaIva,
                c.puntoStock,
                c.enCierreStock,
                c.habilitado,
                c.ingresoRapidoEmbutido,
                c.nivel,
                c.independiente,
                c.idCorteMaestro,
                cm.corte AS CorteMaestroNombre,
                c.porcentaje,
                c.porcentajeHueso,
                c.creado,
                c.actualizado,
                m.razonSocial AS MarcaNombre
            FROM dbo.Corte c
            LEFT JOIN dbo.Personas m ON c.idMarca = m.idPersona
            LEFT JOIN dbo.Corte cm ON cm.idCorte = c.idCorteMaestro
            WHERE c.idEmpresa = @idEmpresa
              AND c.idCorte = @idCorte;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@idCorte", SqlDbType.Int).Value = productId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var bonePercentage = GetDecimal(reader, "porcentajeHueso");
        var isPresentation = bonePercentage > 100m;

        return new ProductDetailItem
        {
            ProductId = GetInt(reader, "idCorte"),
            CompanyId = GetInt(reader, "idEmpresa"),
            Code = GetLong(reader, "codigo"),
            Description = GetString(reader, "corte"),
            Type = GetString(reader, "tipo"),
            BrandId = GetNullableInt(reader, "idMarca"),
            BrandName = GetString(reader, "MarcaNombre"),
            PricePerKilogram = GetDecimal(reader, "precioKg"),
            Weighable = GetBool(reader, "pesable"),
            AverageWeight = GetDecimal(reader, "promedio"),
            VatRateId = GetInt(reader, "idAlicuotaIva"),
            VatRate = GetDecimal(reader, "alicuotaIva"),
            StockPoint = GetInt(reader, "puntoStock"),
            IncludedInStockClosing = GetBool(reader, "enCierreStock"),
            Enabled = GetBool(reader, "habilitado"),
            QuickElaboratedEntry = GetBool(reader, "ingresoRapidoEmbutido"),
            Level = GetInt(reader, "nivel"),
            Independent = GetInt(reader, "independiente") == 1,
            MasterProductId = GetNullableInt(reader, "idCorteMaestro"),
            MasterProductName = GetString(reader, "CorteMaestroNombre"),
            Percentage = GetDecimal(reader, "porcentaje"),
            BonePercentage = bonePercentage,
            CutMode = ResolveCutMode(GetNullableInt(reader, "idCorteMaestro"), isPresentation),
            PresentationUnits = isPresentation ? ((100m + bonePercentage) / 100m) : null,
            CreatedAt = GetDateTime(reader, "creado"),
            UpdatedAt = GetNullableDateTime(reader, "actualizado")
        };
    }

    public async Task<bool> UpdateProductBasicsAsync(
        ProductEditRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0 || request.ProductId <= 0)
        {
            return false;
        }

        const string sql = """
            UPDATE dbo.Corte
            SET
                idMarca = @idMarca,
                precioKg = @precioKg,
                pesable = @pesable,
                promedio = @promedio,
                puntoStock = @puntoStock,
                enCierreStock = @enCierreStock,
                habilitado = @habilitado,
                ingresoRapidoEmbutido = @ingresoRapidoEmbutido,
                actualizado = GETDATE()
            WHERE idCorte = @idCorte
              AND idEmpresa = @idEmpresa;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection, (SqlTransaction)transaction);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCorte", SqlDbType.Int).Value = request.ProductId;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = request.CompanyId;
        command.Parameters.Add("@idMarca", SqlDbType.Int).Value = request.BrandId.HasValue && request.BrandId.Value > 0 ? request.BrandId.Value : 0;
        command.Parameters.Add("@precioKg", SqlDbType.Decimal).Value = request.PricePerKilogram;
        command.Parameters.Add("@pesable", SqlDbType.Bit).Value = request.Weighable;
        command.Parameters.Add("@promedio", SqlDbType.Decimal).Value = request.AverageWeight;
        command.Parameters.Add("@puntoStock", SqlDbType.Int).Value = request.StockPoint;
        command.Parameters.Add("@enCierreStock", SqlDbType.Bit).Value = request.IncludedInStockClosing;
        command.Parameters.Add("@habilitado", SqlDbType.Bit).Value = request.Enabled;
        command.Parameters.Add("@ingresoRapidoEmbutido", SqlDbType.Bit).Value = request.QuickElaboratedEntry;

        command.Parameters["@precioKg"].Precision = 18;
        command.Parameters["@precioKg"].Scale = 2;
        command.Parameters["@promedio"].Precision = 18;
        command.Parameters["@promedio"].Scale = 3;

        var affectedRows = await command.ExecuteNonQueryAsync(cancellationToken);
        if (affectedRows <= 0)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        await EnsureBranchStockPointTableAsync(connection, (SqlTransaction)transaction, cancellationToken);
        await SaveBranchStockPointsAsync(connection, (SqlTransaction)transaction, request, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    public async Task<ProductBranchStockPointResult> GetBranchStockPointsAsync(
        int companyId,
        int productId,
        int legacyStockPoint,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || productId <= 0)
        {
            return new ProductBranchStockPointResult
            {
                HasCustomBranchPoints = false,
                Items = Array.Empty<ProductBranchStockPointItem>()
            };
        }

        const string branchSql = """
            SELECT idSucursal, sucursal
            FROM dbo.Sucursal
            WHERE idEmpresa = @idEmpresa
            ORDER BY sucursal;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        var branches = new List<(int BranchId, string BranchName)>();
        await using (var branchCommand = new SqlCommand(branchSql, connection))
        {
            branchCommand.CommandType = CommandType.Text;
            branchCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

            await using var branchReader = await branchCommand.ExecuteReaderAsync(cancellationToken);
            while (await branchReader.ReadAsync(cancellationToken))
            {
                branches.Add((
                    branchReader["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(branchReader["idSucursal"]),
                    branchReader["sucursal"] == DBNull.Value ? string.Empty : Convert.ToString(branchReader["sucursal"]) ?? string.Empty));
            }
        }

        var customPoints = new Dictionary<int, int>();
        var hasCustomBranchPoints = false;

        const string pointSql = """
            IF OBJECT_ID('dbo.ProductStockPointByBranchNG', 'U') IS NULL
            BEGIN
                SELECT CAST(0 AS int) AS IdSucursal, CAST(0 AS int) AS PuntoStock
                WHERE 1 = 0;
                RETURN;
            END;

            SELECT IdSucursal, PuntoStock
            FROM dbo.ProductStockPointByBranchNG
            WHERE IdEmpresa = @idEmpresa
              AND IdCorte = @idCorte;
            """;

        await using (var pointCommand = new SqlCommand(pointSql, connection))
        {
            pointCommand.CommandType = CommandType.Text;
            pointCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
            pointCommand.Parameters.Add("@idCorte", SqlDbType.Int).Value = productId;

            await using var pointReader = await pointCommand.ExecuteReaderAsync(cancellationToken);
            while (await pointReader.ReadAsync(cancellationToken))
            {
                hasCustomBranchPoints = true;
                var branchId = pointReader["IdSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(pointReader["IdSucursal"]);
                var stockPoint = pointReader["PuntoStock"] == DBNull.Value ? 0 : Convert.ToInt32(pointReader["PuntoStock"]);
                customPoints[branchId] = stockPoint;
            }
        }

        return new ProductBranchStockPointResult
        {
            HasCustomBranchPoints = hasCustomBranchPoints,
            Items = branches.Select(branch => new ProductBranchStockPointItem
            {
                BranchId = branch.BranchId,
                BranchName = branch.BranchName,
                StockPoint = hasCustomBranchPoints
                    ? customPoints.GetValueOrDefault(branch.BranchId, 0)
                    : legacyStockPoint
            }).ToArray()
        };
    }

    public async Task<IReadOnlyCollection<ProductListItem>> GetGlobalProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearchText = (query.SearchText ?? string.Empty).Trim();
        var normalizedType = (query.Type ?? string.Empty).Trim();

        const string sql = """
            SELECT
                c.idCorte,
                c.codigo,
                c.corte,
                c.tipo,
                c.precioKg,
                c.habilitado,
                c.pesable,
                c.creado,
                c.actualizado
            FROM dbo.Corte c
            WHERE c.idEmpresa = 0
              AND (@texto = '' OR c.corte LIKE @buscar OR CAST(c.codigo AS NVARCHAR(50)) LIKE @buscar)
              AND (@tipo = '' OR c.tipo = @tipo)
            ORDER BY c.codigo ASC;
            """;

        var items = new List<ProductListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 100).Value = normalizedSearchText;
        command.Parameters.Add("@buscar", SqlDbType.NVarChar, 110).Value = $"%{normalizedSearchText}%";
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 50).Value = normalizedType;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductListItem
            {
                ProductId = GetInt(reader, "idCorte"),
                Code = GetLong(reader, "codigo"),
                Description = GetString(reader, "corte"),
                Type = GetString(reader, "tipo"),
                BrandName = string.Empty,
                PricePerKilogram = GetDecimal(reader, "precioKg"),
                Enabled = GetBool(reader, "habilitado"),
                Weighable = GetBool(reader, "pesable"),
                CreatedAt = GetDateTime(reader, "creado"),
                UpdatedAt = GetNullableDateTime(reader, "actualizado")
            });
        }

        return items;
    }

    public async Task<ProductDetailItem?> GetGlobalProductByIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1
                c.idCorte,
                c.idEmpresa,
                c.codigo,
                c.corte,
                c.tipo,
                c.precioKg,
                c.pesable,
                c.promedio,
                c.idAlicuotaIva,
                c.alicuotaIva,
                c.puntoStock,
                c.enCierreStock,
                c.habilitado,
                c.ingresoRapidoEmbutido,
                c.nivel,
                c.independiente,
                c.idCorteMaestro,
                cm.corte AS CorteMaestroNombre,
                c.porcentaje,
                c.porcentajeHueso,
                c.creado,
                c.actualizado
            FROM dbo.Corte c
            LEFT JOIN dbo.Corte cm ON cm.idCorte = c.idCorteMaestro
            WHERE c.idEmpresa = 0
              AND c.idCorte = @idCorte;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idCorte", SqlDbType.Int).Value = productId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        var bonePercentage = GetDecimal(reader, "porcentajeHueso");
        var isPresentation = bonePercentage > 100m;

        return new ProductDetailItem
        {
            ProductId = GetInt(reader, "idCorte"),
            CompanyId = GetInt(reader, "idEmpresa"),
            Code = GetLong(reader, "codigo"),
            Description = GetString(reader, "corte"),
            Type = GetString(reader, "tipo"),
            BrandId = null,
            BrandName = string.Empty,
            PricePerKilogram = GetDecimal(reader, "precioKg"),
            Weighable = GetBool(reader, "pesable"),
            AverageWeight = GetDecimal(reader, "promedio"),
            VatRateId = GetInt(reader, "idAlicuotaIva"),
            VatRate = GetDecimal(reader, "alicuotaIva"),
            StockPoint = GetInt(reader, "puntoStock"),
            IncludedInStockClosing = GetBool(reader, "enCierreStock"),
            Enabled = GetBool(reader, "habilitado"),
            QuickElaboratedEntry = GetBool(reader, "ingresoRapidoEmbutido"),
            Level = GetInt(reader, "nivel"),
            Independent = GetInt(reader, "independiente") == 1,
            MasterProductId = GetNullableInt(reader, "idCorteMaestro"),
            MasterProductName = GetString(reader, "CorteMaestroNombre"),
            Percentage = GetDecimal(reader, "porcentaje"),
            BonePercentage = bonePercentage,
            CutMode = ResolveCutMode(GetNullableInt(reader, "idCorteMaestro"), isPresentation),
            PresentationUnits = isPresentation ? ((100m + bonePercentage) / 100m) : null,
            CreatedAt = GetDateTime(reader, "creado"),
            UpdatedAt = GetNullableDateTime(reader, "actualizado")
        };
    }

    public async Task<IReadOnlyCollection<ProductTypeListItem>> GetCompanyProductTypesAsync(
        int companyId,
        ProductTypeListQuery query,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return [];
        }

        const string sql = """
            SELECT tipo, orden, creado AS Creado, actualizado AS Actualizado, reservadoSistema AS Reservado
            FROM dbo.TiposProducto
            WHERE (reservadoSistema = 1 OR idEmpresa = @idEmpresa)
              AND (@buscar IS NULL OR tipo LIKE @buscar)
            ORDER BY orden, tipo;
            """;

        return await GetProductTypesAsync(
            sql,
            command =>
            {
                command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
                AddSearchParameter(command, query.SearchText);
            },
            false,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<ProductTypeListItem>> GetGlobalProductTypesAsync(
        ProductTypeListQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT tipo, orden, creado AS Creado, actualizado AS Actualizado, reservadoSistema AS Reservado
            FROM dbo.TiposProducto
            WHERE idEmpresa = 0
              AND reservadoSistema = 0
              AND (@buscar IS NULL OR tipo LIKE @buscar)
            ORDER BY orden, tipo;
            """;

        return await GetProductTypesAsync(
            sql,
            command => AddSearchParameter(command, query.SearchText),
            true,
            cancellationToken);
    }

    private static int GetInt(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0 : Convert.ToInt32(record.GetValue(ordinal));
    }

    private static int? GetNullableInt(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? null : Convert.ToInt32(record.GetValue(ordinal));
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

    private static decimal GetDecimal(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? 0m : Convert.ToDecimal(record.GetValue(ordinal));
    }

    private static bool GetBool(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return !record.IsDBNull(ordinal) && Convert.ToBoolean(record.GetValue(ordinal));
    }

    private static DateTime GetDateTime(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? DateTime.MinValue : Convert.ToDateTime(record.GetValue(ordinal));
    }

    private static DateTime? GetNullableDateTime(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? null : Convert.ToDateTime(record.GetValue(ordinal));
    }

    private async Task<IReadOnlyCollection<ProductTypeListItem>> GetProductTypesAsync(
        string sql,
        Action<SqlCommand> configureCommand,
        bool isGlobal,
        CancellationToken cancellationToken)
    {
        var items = new List<ProductTypeListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        configureCommand(command);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new ProductTypeListItem
            {
                TypeName = GetString(reader, "tipo"),
                SortOrder = GetInt(reader, "orden"),
                IsReserved = GetBool(reader, "Reservado"),
                CreatedAt = GetNullableDateTime(reader, "Creado"),
                UpdatedAt = GetNullableDateTime(reader, "Actualizado"),
                IsGlobal = isGlobal
            });
        }

        return items;
    }

    private static void AddSearchParameter(SqlCommand command, string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            command.Parameters.Add("@buscar", SqlDbType.NVarChar, 200).Value = DBNull.Value;
            return;
        }

        command.Parameters.Add("@buscar", SqlDbType.NVarChar, 200).Value = "%" + searchText.Trim() + "%";
    }

    private static async Task EnsureBranchStockPointTableAsync(SqlConnection connection, SqlTransaction transaction, CancellationToken cancellationToken)
    {
        const string sql = """
            IF OBJECT_ID('dbo.ProductStockPointByBranchNG', 'U') IS NULL
            BEGIN
                CREATE TABLE dbo.ProductStockPointByBranchNG
                (
                    IdProductStockPointByBranchNG INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    IdEmpresa INT NOT NULL,
                    IdCorte INT NOT NULL,
                    IdSucursal INT NOT NULL,
                    PuntoStock INT NOT NULL CONSTRAINT DF_ProductStockPointByBranchNG_PuntoStock DEFAULT (0),
                    Creado DATETIME NOT NULL CONSTRAINT DF_ProductStockPointByBranchNG_Creado DEFAULT (GETDATE()),
                    Actualizado DATETIME NULL
                );

                CREATE UNIQUE INDEX UX_ProductStockPointByBranchNG_Empresa_Corte_Sucursal
                    ON dbo.ProductStockPointByBranchNG(IdEmpresa, IdCorte, IdSucursal);
            END;
            """;

        await using var command = new SqlCommand(sql, connection, transaction);
        command.CommandType = CommandType.Text;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveBranchStockPointsAsync(
        SqlConnection connection,
        SqlTransaction transaction,
        ProductEditRequest request,
        CancellationToken cancellationToken)
    {
        const string branchSql = """
            SELECT idSucursal
            FROM dbo.Sucursal
            WHERE idEmpresa = @idEmpresa;
            """;

        var branchIds = new List<int>();
        await using (var branchCommand = new SqlCommand(branchSql, connection, transaction))
        {
            branchCommand.CommandType = CommandType.Text;
            branchCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = request.CompanyId;

            await using var branchReader = await branchCommand.ExecuteReaderAsync(cancellationToken);
            while (await branchReader.ReadAsync(cancellationToken))
            {
                branchIds.Add(branchReader["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(branchReader["idSucursal"]));
            }
        }

        var branchPointMap = (request.BranchStockPoints ?? Array.Empty<ProductBranchStockPointItem>())
            .Where(x => x.BranchId > 0)
            .GroupBy(x => x.BranchId)
            .ToDictionary(g => g.Key, g => Math.Max(0, g.Last().StockPoint));

        foreach (var branchId in branchIds)
        {
            var stockPoint = request.UseBranchStockPoints
                ? branchPointMap.GetValueOrDefault(branchId, 0)
                : request.StockPoint;

            const string mergeSql = """
                MERGE dbo.ProductStockPointByBranchNG AS target
                USING (SELECT @idEmpresa AS IdEmpresa, @idCorte AS IdCorte, @idSucursal AS IdSucursal) AS source
                    ON target.IdEmpresa = source.IdEmpresa
                   AND target.IdCorte = source.IdCorte
                   AND target.IdSucursal = source.IdSucursal
                WHEN MATCHED THEN
                    UPDATE SET PuntoStock = @puntoStock, Actualizado = GETDATE()
                WHEN NOT MATCHED THEN
                    INSERT (IdEmpresa, IdCorte, IdSucursal, PuntoStock)
                    VALUES (@idEmpresa, @idCorte, @idSucursal, @puntoStock);
                """;

            await using var mergeCommand = new SqlCommand(mergeSql, connection, transaction);
            mergeCommand.CommandType = CommandType.Text;
            mergeCommand.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = request.CompanyId;
            mergeCommand.Parameters.Add("@idCorte", SqlDbType.Int).Value = request.ProductId;
            mergeCommand.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;
            mergeCommand.Parameters.Add("@puntoStock", SqlDbType.Int).Value = stockPoint;
            await mergeCommand.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    private static string ResolveCutMode(int? masterProductId, bool isPresentation)
    {
        if (!masterProductId.HasValue || masterProductId.Value <= 0)
        {
            return "Ninguno";
        }

        return isPresentation ? "Presentacion" : "CorteMaestro";
    }
}
