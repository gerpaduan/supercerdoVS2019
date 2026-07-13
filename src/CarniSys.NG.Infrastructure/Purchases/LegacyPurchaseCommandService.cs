using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Purchases;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyPurchaseCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IPurchaseCommandService
{
    public async Task<PurchaseSaveResult> SavePurchaseAsync(PurchaseSaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.CompanyId <= 0)
        {
            return new PurchaseSaveResult { ErrorMessage = "La empresa actual no es valida." };
        }

        if (request.BranchId <= 0)
        {
            return new PurchaseSaveResult { ErrorMessage = "Debe seleccionar una sucursal." };
        }

        if (request.SupplierId <= 0)
        {
            return new PurchaseSaveResult { ErrorMessage = "Debe seleccionar un proveedor." };
        }

        if (request.Lines.Count == 0)
        {
            return new PurchaseSaveResult { ErrorMessage = "Debe ingresar al menos una linea." };
        }

        var purchaseType = NormalizePurchaseType(request.PurchaseType);
        if (string.IsNullOrWhiteSpace(purchaseType))
        {
            return new PurchaseSaveResult { ErrorMessage = "Debe seleccionar un tipo de compra." };
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            if (!await BranchExistsAsync(request.BranchId, connection, transaction, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "La sucursal seleccionada no existe." };
            }

            if (!await SupplierExistsAsync(request.SupplierId, connection, transaction, cancellationToken))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "El proveedor seleccionado no existe." };
            }

            var invalidProductId = await FindInvalidProductIdAsync(request.Lines, connection, transaction, cancellationToken);
            if (invalidProductId.HasValue)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = $"El producto {invalidProductId.Value} no existe." };
            }

            var existing = request.PurchaseId > 0
                ? await LoadExistingPurchaseAsync(request.PurchaseId, connection, transaction, cancellationToken)
                : null;

            if (request.PurchaseId > 0 && existing is null)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "No se encontro la compra a modificar." };
            }

            if (existing is not null
                && !string.Equals(existing.PurchaseType, purchaseType, StringComparison.OrdinalIgnoreCase))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "No se puede cambiar el tipo de una compra existente." };
            }

            if (existing is not null && IsCancelledStatus(existing.Status))
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "No se puede editar una compra anulada." };
            }

            var purchaseId = await SaveHeaderAsync(request, existing, purchaseType, connection, transaction, cancellationToken);
            if (purchaseId <= 0)
            {
                await transaction.RollbackAsync(cancellationToken);
                return new PurchaseSaveResult { ErrorMessage = "No se pudo obtener el identificador de la compra guardada." };
            }

            foreach (var line in request.Lines)
            {
                if (string.Equals(purchaseType, "Media Res", StringComparison.OrdinalIgnoreCase))
                {
                    await SaveHalfCarcassLineAsync(purchaseId, request.BranchId, line, connection, transaction, cancellationToken);
                }
                else
                {
                    await SaveCutLineAsync(purchaseId, request.BranchId, request.UserId, line, connection, transaction, cancellationToken);
                }
            }

            await transaction.CommitAsync(cancellationToken);

            return new PurchaseSaveResult
            {
                Success = true,
                PurchaseId = purchaseId
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            return new PurchaseSaveResult
            {
                ErrorMessage = $"No se pudo guardar la compra. {ex.Message}"
            };
        }
    }

    private static async Task<int> SaveHeaderAsync(
        PurchaseSaveRequest request,
        ExistingPurchaseState? existing,
        string purchaseType,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("addOrEditCompra", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@idCompra", request.PurchaseId);
        command.Parameters.AddWithValue("@nroRemito", request.ReceiptNumber);
        command.Parameters.AddWithValue("@fechaCompra", request.PurchaseDate);
        command.Parameters.AddWithValue("@idProveedor", request.SupplierId);
        command.Parameters.AddWithValue("@estado", existing?.Status ?? string.Empty);
        command.Parameters.AddWithValue("@observaciones", request.Notes);
        command.Parameters.AddWithValue("@tipoCompra", purchaseType);
        command.Parameters.AddWithValue("@cantMedias", string.Equals(purchaseType, "Media Res", StringComparison.OrdinalIgnoreCase)
            ? request.HalfCarcassCount ?? request.Lines.Count
            : 0);
        command.Parameters.AddWithValue("@kgsMedias", string.Equals(purchaseType, "Media Res", StringComparison.OrdinalIgnoreCase)
            ? request.Lines.Sum(x => x.QuantityKg)
            : 0m);
        command.Parameters.Add("@idPesajeAjustado", SqlDbType.Int).Value = DBNull.Value;
        command.Parameters.AddWithValue("@enCtaCte", request.CurrentAccount);
        command.Parameters.AddWithValue("@idSucursal", request.BranchId);
        command.Parameters.AddWithValue("@creadoPor", existing?.CreatedByUserId ?? request.UserId);
        command.Parameters.AddWithValue("@actualizadoPor", existing is null ? 0 : request.UserId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value
            ? request.PurchaseId
            : Convert.ToInt32(result);
    }

    private static async Task SaveCutLineAsync(
        int purchaseId,
        int branchId,
        int userId,
        PurchaseSaveLineRequest line,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("agregarCortePorCompra", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@idCompra", purchaseId);
        command.Parameters.AddWithValue("@idCorte", line.ProductId);
        command.Parameters.AddWithValue("@idSucursal", branchId);
        command.Parameters.AddWithValue("@precioKg", line.PricePerKg);
        command.Parameters.AddWithValue("@cantKg", line.QuantityKg);
        command.Parameters.AddWithValue("@balanza", false);
        command.Parameters.AddWithValue("@creado", DateTime.Now);
        command.Parameters.AddWithValue("@creadoPor", userId);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task SaveHalfCarcassLineAsync(
        int purchaseId,
        int branchId,
        PurchaseSaveLineRequest line,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        await using var command = new SqlCommand("agregarMediaRes", connection, transaction)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.AddWithValue("@idCompra", purchaseId);
        command.Parameters.AddWithValue("@nroTropa", line.TroopNumber);
        command.Parameters.AddWithValue("@idSucursal", branchId);
        command.Parameters.AddWithValue("@precioMedia", line.PricePerKg);
        command.Parameters.AddWithValue("@kgMedia", line.QuantityKg);

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static async Task<bool> BranchExistsAsync(
        int branchId,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) idSucursal
            FROM dbo.Sucursal
            WHERE idSucursal = @idSucursal;
            """;

        await using var command = new SqlCommand(sql, connection, transaction)
        {
            CommandType = CommandType.Text
        };
        command.Parameters.Add("@idSucursal", SqlDbType.Int).Value = branchId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value;
    }

    private static async Task<bool> SupplierExistsAsync(
        int supplierId,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) idPersona
            FROM dbo.Personas
            WHERE idPersona = @idPersona;
            """;

        await using var command = new SqlCommand(sql, connection, transaction)
        {
            CommandType = CommandType.Text
        };
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = supplierId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value;
    }

    private static async Task<int?> FindInvalidProductIdAsync(
        IReadOnlyCollection<PurchaseSaveLineRequest> lines,
        SqlConnection connection,
        SqlTransaction transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1) idCorte
            FROM dbo.Corte
            WHERE idCorte = @idCorte;
            """;

        foreach (var line in lines)
        {
            if (string.Equals(line.LineType, "MediaRes", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await using var command = new SqlCommand(sql, connection, transaction)
            {
                CommandType = CommandType.Text
            };
            command.Parameters.Add("@idCorte", SqlDbType.Int).Value = line.ProductId;

            var result = await command.ExecuteScalarAsync(cancellationToken);
            if (result is null || result == DBNull.Value)
            {
                return line.ProductId;
            }
        }

        return null;
    }

    private static async Task<ExistingPurchaseState?> LoadExistingPurchaseAsync(
        int purchaseId,
        SqlConnection connection,
        SqlTransaction? transaction,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP (1)
                ISNULL(tipoCompra, '') AS tipoCompra,
                ISNULL(estado, '') AS estado,
                ISNULL(creadoPor, 0) AS creadoPor
            FROM dbo.Compras
            WHERE idCompra = @idCompra;
            """;

        await using var command = new SqlCommand(sql, connection, transaction)
        {
            CommandType = CommandType.Text
        };
        command.Parameters.Add("@idCompra", SqlDbType.Int).Value = purchaseId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ExistingPurchaseState
        {
            PurchaseType = reader.GetString(reader.GetOrdinal("tipoCompra")),
            Status = reader.GetString(reader.GetOrdinal("estado")),
            CreatedByUserId = reader.GetInt32(reader.GetOrdinal("creadoPor"))
        };
    }

    private sealed class ExistingPurchaseState
    {
        public string PurchaseType { get; init; } = string.Empty;

        public string Status { get; init; } = string.Empty;

        public int CreatedByUserId { get; init; }
    }

    private static bool IsCancelledStatus(string? status)
    {
        var normalized = (status ?? string.Empty).Trim();
        return normalized.Equals("Anulado", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Anulada", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePurchaseType(string? purchaseType)
    {
        var normalized = (purchaseType ?? string.Empty).Trim();
        return normalized;
    }
}
