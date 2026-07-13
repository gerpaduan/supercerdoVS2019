using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Purchases;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyPurchaseQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IPurchaseQueryService
{
    public async Task<IReadOnlyCollection<PurchaseListItem>> GetPurchasesAsync(
        PurchaseListQuery query,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                c.idCompra,
                c.fechaCompra,
                c.tipoCompra,
                c.idSucursal,
                s.sucursal,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.nroRemito,
                c.observaciones,
                ISNULL(c.cantMedias, 0) AS cantMedias,
                CAST(
                    ISNULL(
                        CASE
                            WHEN c.tipoCompra = 'Media Res' THEN
                                (SELECT SUM(CAST(ISNULL(mr.kgMedia, 0) AS decimal(18, 3)))
                                 FROM dbo.MediaRes mr
                                 WHERE mr.idCompra = c.idCompra)
                            ELSE
                                (SELECT SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(18, 3)))
                                 FROM dbo.CortePorCompra cpc
                                 WHERE cpc.idCompra = c.idCompra)
                        END,
                        0
                    ) AS decimal(18, 3)
                ) AS totalKg,
                CAST(
                    ISNULL(
                        CASE
                            WHEN c.tipoCompra = 'Media Res' THEN
                                (SELECT SUM(CAST(ISNULL(mr.kgMedia, 0) AS decimal(18, 3)) * CAST(ISNULL(mr.precioMedia, 0) AS decimal(18, 2)))
                                 FROM dbo.MediaRes mr
                                 WHERE mr.idCompra = c.idCompra)
                            ELSE
                                (SELECT SUM(CAST(ISNULL(cpc.cantKg, 0) AS decimal(18, 3)) * CAST(ISNULL(cpc.precioKg, 0) AS decimal(18, 2)))
                                 FROM dbo.CortePorCompra cpc
                                 WHERE cpc.idCompra = c.idCompra)
                        END,
                        0
                    ) AS decimal(18, 2)
                ) AS totalImporte
            FROM dbo.Compras c
            LEFT JOIN dbo.Personas p ON p.idPersona = c.idProveedor
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = c.idSucursal
            WHERE (@idSucursal <= 0 OR c.idSucursal = @idSucursal)
              AND c.fechaCompra >= @fechaDesde
              AND c.fechaCompra < @fechaHasta
              AND (@tipoCompra = '' OR c.tipoCompra = @tipoCompra)
              AND (
                    @texto = ''
                    OR CONVERT(nvarchar(20), c.idCompra) LIKE @textoLike
                    OR ISNULL(p.razonSocial, '') LIKE @textoLike
                    OR ISNULL(c.nroRemito, '') LIKE @textoLike
                    OR ISNULL(c.observaciones, '') LIKE @textoLike
                  )
            ORDER BY c.fechaCompra DESC, c.idCompra DESC;
            """;

        var items = new List<PurchaseListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = query.BranchId;
        command.Parameters.Add("@fechaDesde", SqlDbType.DateTime).Value = query.DateFrom.Date;
        command.Parameters.Add("@fechaHasta", SqlDbType.DateTime).Value = query.DateTo.Date.AddDays(1);
        command.Parameters.Add("@tipoCompra", SqlDbType.NVarChar, 50).Value = NormalizePurchaseType(query.PurchaseType);
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = (query.SearchText ?? string.Empty).Trim();
        command.Parameters.Add("@textoLike", SqlDbType.NVarChar, 210).Value = "%" + (query.SearchText ?? string.Empty).Trim() + "%";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PurchaseListItem
            {
                PurchaseId = GetInt(reader, "idCompra"),
                PurchaseDate = GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
                PurchaseType = GetString(reader, "tipoCompra"),
                BranchId = GetInt(reader, "idSucursal"),
                BranchName = GetString(reader, "sucursal"),
                SupplierId = GetInt(reader, "idProveedor"),
                SupplierName = GetString(reader, "razonSocial"),
                SupplierTaxId = GetString(reader, "cuit"),
                ReceiptNumber = GetString(reader, "nroRemito"),
                Notes = GetString(reader, "observaciones"),
                HalfCarcassCount = GetInt(reader, "cantMedias"),
                TotalKg = GetDecimal(reader, "totalKg"),
                TotalAmount = GetDecimal(reader, "totalImporte")
            });
        }

        return items;
    }

    public async Task<PurchaseDetailItem?> GetPurchaseByIdAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        if (purchaseId <= 0)
        {
            return null;
        }

        const string headerSql = """
            SELECT
                c.idCompra,
                c.fechaCompra,
                c.tipoCompra,
                c.idSucursal,
                s.sucursal,
                c.idProveedor,
                p.razonSocial,
                p.cuit,
                c.nroRemito,
                c.observaciones,
                c.estado,
                c.enCtaCte,
                c.cantMedias,
                c.kgsMedias,
                c.creado,
                ISNULL(c.creadoPor, 0) AS creadoPor,
                c.actualizado,
                uc.nombre AS creadoPorNombre,
                uu.nombre AS actualizadoPorNombre
            FROM dbo.Compras c
            LEFT JOIN dbo.Personas p ON p.idPersona = c.idProveedor
            LEFT JOIN dbo.Sucursal s ON s.idSucursal = c.idSucursal
            LEFT JOIN dbo.Usuarios uc ON uc.id = c.creadoPor
            LEFT JOIN dbo.Usuarios uu ON uu.id = c.actualizadoPor
            WHERE c.idCompra = @idCompra;
            """;

        const string cutsSql = """
            SELECT
                cpc.idCortePorCompra,
                cpc.idCorte,
                co.codigo,
                co.corte,
                CAST(ISNULL(cpc.cantKg, 0) AS decimal(18, 3)) AS cantKg,
                CAST(ISNULL(cpc.precioKg, 0) AS decimal(18, 2)) AS precioKg,
                CAST(ISNULL(cpc.cantKg, 0) * ISNULL(cpc.precioKg, 0) AS decimal(18, 2)) AS totalLinea,
                ISNULL(cpc.balanza, 0) AS balanza
            FROM dbo.CortePorCompra cpc
            INNER JOIN dbo.Corte co ON co.idCorte = cpc.idCorte
            WHERE cpc.idCompra = @idCompra
            ORDER BY cpc.creado, cpc.idCortePorCompra;
            """;

        const string halfCarcassSql = """
            SELECT
                mr.idMedia,
                mr.nroTropa,
                CAST(ISNULL(mr.kgMedia, 0) AS decimal(18, 3)) AS kgMedia,
                CAST(ISNULL(mr.precioMedia, 0) AS decimal(18, 2)) AS precioMedia,
                CAST(ISNULL(mr.kgMedia, 0) * ISNULL(mr.precioMedia, 0) AS decimal(18, 2)) AS totalLinea
            FROM dbo.MediaRes mr
            WHERE mr.idCompra = @idCompra
            ORDER BY mr.idMedia;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var headerCommand = new SqlCommand(headerSql, connection);
        headerCommand.CommandType = CommandType.Text;
        headerCommand.Parameters.Add("@idCompra", SqlDbType.Int).Value = purchaseId;

        PurchaseHeader? header = null;
        await using (var reader = await headerCommand.ExecuteReaderAsync(cancellationToken))
        {
            if (await reader.ReadAsync(cancellationToken))
            {
                header = new PurchaseHeader(
                    GetInt(reader, "idCompra"),
                    GetNullableDateTime(reader, "fechaCompra") ?? DateTime.Now,
                    GetString(reader, "tipoCompra"),
                    GetInt(reader, "idSucursal"),
                    GetString(reader, "sucursal"),
                    GetInt(reader, "idProveedor"),
                    GetString(reader, "razonSocial"),
                    GetString(reader, "cuit"),
                    GetString(reader, "nroRemito"),
                    GetString(reader, "observaciones"),
                    GetString(reader, "estado"),
                    GetBool(reader, "enCtaCte"),
                    GetNullableInt(reader, "cantMedias"),
                    GetNullableDecimal(reader, "kgsMedias"),
                    GetNullableDateTime(reader, "creado"),
                    GetInt(reader, "creadoPor"),
                    GetNullableDateTime(reader, "actualizado"),
                    GetString(reader, "creadoPorNombre"),
                    GetString(reader, "actualizadoPorNombre"));
            }
        }

        if (header is null)
        {
            return null;
        }

        var lines = new List<PurchaseDetailLineItem>();
        if (string.Equals(header.PurchaseType, "Media Res", StringComparison.OrdinalIgnoreCase))
        {
            await using var linesCommand = new SqlCommand(halfCarcassSql, connection);
            linesCommand.CommandType = CommandType.Text;
            linesCommand.Parameters.Add("@idCompra", SqlDbType.Int).Value = purchaseId;

            await using var reader = await linesCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                lines.Add(new PurchaseDetailLineItem
                {
                    LineId = GetInt(reader, "idMedia"),
                    LineType = "MediaRes",
                    Code = GetString(reader, "nroTropa"),
                    ProductName = "Media Res",
                    QuantityKg = GetDecimal(reader, "kgMedia"),
                    Price = GetDecimal(reader, "precioMedia"),
                    Total = GetDecimal(reader, "totalLinea"),
                    ScaleWeight = false
                });
            }
        }
        else
        {
            await using var linesCommand = new SqlCommand(cutsSql, connection);
            linesCommand.CommandType = CommandType.Text;
            linesCommand.Parameters.Add("@idCompra", SqlDbType.Int).Value = purchaseId;

            await using var reader = await linesCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                lines.Add(new PurchaseDetailLineItem
                {
                    LineId = GetInt(reader, "idCortePorCompra"),
                    LineType = "Corte",
                    ProductId = GetNullableInt(reader, "idCorte"),
                    Code = GetString(reader, "codigo"),
                    ProductName = GetString(reader, "corte"),
                    QuantityKg = GetDecimal(reader, "cantKg"),
                    Price = GetDecimal(reader, "precioKg"),
                    Total = GetDecimal(reader, "totalLinea"),
                    ScaleWeight = GetBool(reader, "balanza")
                });
            }
        }

        return new PurchaseDetailItem
        {
            PurchaseId = header.PurchaseId,
            PurchaseDate = header.PurchaseDate,
            PurchaseType = header.PurchaseType,
            BranchId = header.BranchId,
            BranchName = header.BranchName,
            SupplierId = header.SupplierId,
            SupplierName = header.SupplierName,
            SupplierTaxId = header.SupplierTaxId,
            ReceiptNumber = header.ReceiptNumber,
            Notes = header.Notes,
            Status = header.Status,
            CurrentAccount = header.CurrentAccount,
            HalfCarcassCount = header.HalfCarcassCount,
            HalfCarcassWeightKg = header.HalfCarcassWeightKg,
            TotalKg = lines.Sum(x => x.QuantityKg),
            TotalAmount = lines.Sum(x => x.Total),
            CreatedAt = header.CreatedAt,
            CreatedByUserId = header.CreatedByUserId,
            CreatedByName = header.CreatedByName,
            UpdatedAt = header.UpdatedAt,
            UpdatedByName = header.UpdatedByName,
            Lines = lines
        };
    }

    private static string NormalizePurchaseType(string? purchaseType)
    {
        var normalized = (purchaseType ?? string.Empty).Trim();
        return string.Equals(normalized, "Todos", StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : normalized;
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

    private static decimal? GetNullableDecimal(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                return record.IsDBNull(ordinal) ? null : Convert.ToDecimal(record.GetValue(ordinal));
            }
        }

        return null;
    }

    private static bool GetBool(IDataRecord record, params string[] columnNames)
    {
        foreach (var columnName in columnNames)
        {
            var ordinal = TryGetOrdinal(record, columnName);
            if (ordinal >= 0)
            {
                if (record.IsDBNull(ordinal))
                {
                    return false;
                }

                var value = record.GetValue(ordinal);
                return value switch
                {
                    bool boolValue => boolValue,
                    byte byteValue => byteValue != 0,
                    short shortValue => shortValue != 0,
                    int intValue => intValue != 0,
                    long longValue => longValue != 0,
                    _ => Convert.ToBoolean(value)
                };
            }
        }

        return false;
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

    private sealed record PurchaseHeader(
        int PurchaseId,
        DateTime PurchaseDate,
        string PurchaseType,
        int BranchId,
        string BranchName,
        int SupplierId,
        string SupplierName,
        string SupplierTaxId,
        string ReceiptNumber,
        string Notes,
        string Status,
        bool CurrentAccount,
        int? HalfCarcassCount,
        decimal? HalfCarcassWeightKg,
        DateTime? CreatedAt,
        int CreatedByUserId,
        DateTime? UpdatedAt,
        string CreatedByName,
        string UpdatedByName);
}
