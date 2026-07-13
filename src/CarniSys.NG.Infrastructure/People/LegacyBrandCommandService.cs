using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using CarniSys.NG.Application.People;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyBrandCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IBrandCommandService
{
public async Task<BrandSaveResult> SaveBrandAsync(
        BrandSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var brandName = (request.BrandName ?? string.Empty).Trim();
        var notes = (request.Notes ?? string.Empty).Trim();
        var isInsert = request.BrandId <= 0;

        if (string.IsNullOrWhiteSpace(brandName))
        {
            return BrandSaveResult.Failure("El campo Nombre Marca no puede estar vacio.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        ExistingBrand? existingBrand = null;
        if (!isInsert)
        {
            existingBrand = await GetBrandAsync(connection, request.BrandId, cancellationToken);
            if (existingBrand is null)
            {
                return BrandSaveResult.Failure("No se encontro la marca seleccionada.");
            }

            if (!request.IsAdministrator &&
                !string.Equals(existingBrand.BrandName.Trim(), brandName, StringComparison.OrdinalIgnoreCase))
            {
                return BrandSaveResult.Failure("Solo los administradores pueden modificar el nombre de una marca existente.");
            }
        }

        if (request.OwnerId.HasValue && request.OwnerId.Value > 0)
        {
            var owner = await GetOwnerAsync(connection, request.OwnerId.Value, cancellationToken);
            if (owner is null)
            {
                return BrandSaveResult.Failure("No se encontro la persona seleccionada como propietaria.");
            }
        }

        var similarBrandsWarning = await BuildSimilarBrandsWarningAsync(connection, brandName, request.BrandId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(similarBrandsWarning) && !request.ConfirmSimilarBrands)
        {
            return BrandSaveResult.Failure(similarBrandsWarning, requiresConfirmation: true);
        }

        const string sql = """
            EXEC dbo.addOrEditPersona
                @idPersona,
                @identificacion,
                @razonSocial,
                @idIva,
                @cuit,
                @telefono,
                @email,
                @domicilio,
                @ciudad,
                @otrosDatos,
                @tipo,
                @ctaCte,
                @bonificacion,
                @marca,
                @idPropietario;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = request.BrandId;
        command.Parameters.Add("@identificacion", SqlDbType.NVarChar, 200).Value = brandName;
        command.Parameters.Add("@razonSocial", SqlDbType.NVarChar, 200).Value = brandName;
        command.Parameters.Add("@idIva", SqlDbType.Int).Value = 0;
        command.Parameters.Add("@cuit", SqlDbType.NVarChar, 50).Value = string.Empty;
        command.Parameters.Add("@telefono", SqlDbType.NVarChar, 100).Value = string.Empty;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 120).Value = DBNull.Value;
        command.Parameters.Add("@domicilio", SqlDbType.NVarChar, 200).Value = string.Empty;
        command.Parameters.Add("@ciudad", SqlDbType.NVarChar, 100).Value = string.Empty;
        command.Parameters.Add("@otrosDatos", SqlDbType.NVarChar, -1).Value = notes;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = string.Empty;
        command.Parameters.Add("@ctaCte", SqlDbType.Bit).Value = false;
        command.Parameters.Add("@bonificacion", SqlDbType.Decimal).Value = 0m;
        command.Parameters.Add("@marca", SqlDbType.Bit).Value = true;
        command.Parameters.Add("@idPropietario", SqlDbType.Int).Value = request.OwnerId.HasValue && request.OwnerId.Value > 0
            ? request.OwnerId.Value
            : DBNull.Value;
        command.Parameters["@bonificacion"].Precision = 18;
        command.Parameters["@bonificacion"].Scale = 2;

        await command.ExecuteNonQueryAsync(cancellationToken);
        return BrandSaveResult.Ok();
    }

    public async Task<BrandSaveResult> DeleteBrandAsync(
        int companyId,
        int brandId,
        CancellationToken cancellationToken = default)
    {
        if (companyId <= 0 || brandId <= 0)
        {
            return BrandSaveResult.Failure("No se encontro la marca seleccionada.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        var existingBrand = await GetBrandAsync(connection, brandId, cancellationToken);
        if (existingBrand is null)
        {
            return BrandSaveResult.Failure("No se encontro la marca seleccionada.");
        }

        if (existingBrand.CompanyId <= 0 || existingBrand.CompanyId != companyId)
        {
            return BrandSaveResult.Failure("La marca seleccionada no pertenece a la empresa actual.");
        }

        if (await HasSalesOrPurchasesAsync(connection, brandId, cancellationToken))
        {
            return BrandSaveResult.Failure("No se puede eliminar la marca porque tiene compras o ventas asociadas.");
        }

        if (await IsUsedByProductsAsync(connection, brandId, cancellationToken))
        {
            return BrandSaveResult.Failure("No se puede eliminar la marca porque esta asociada a productos/cortes.");
        }

        const string sql = """
            EXEC dbo.eliminarPersona @idPersona;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = brandId;
        await command.ExecuteNonQueryAsync(cancellationToken);

        return BrandSaveResult.Ok();
    }

    private static async Task<ExistingBrand?> GetBrandAsync(SqlConnection connection, int brandId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 idPersona, razonSocial, otrosDatos, idPropietario, idEmpresa
            FROM dbo.Personas
            WHERE idPersona = @idPersona
              AND marca = 1;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = brandId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ExistingBrand(
            reader["idPersona"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idPersona"]),
            Convert.ToString(reader["razonSocial"]) ?? string.Empty,
            Convert.ToString(reader["otrosDatos"]) ?? string.Empty,
            reader["idPropietario"] == DBNull.Value ? null : Convert.ToInt32(reader["idPropietario"]),
            reader["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idEmpresa"]));
    }

    private static async Task<int?> GetOwnerAsync(SqlConnection connection, int ownerId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1 idPersona
            FROM dbo.Personas
            WHERE idPersona = @idPersona;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = ownerId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? null : Convert.ToInt32(result);
    }

    private static async Task<string> BuildSimilarBrandsWarningAsync(
        SqlConnection connection,
        string brandName,
        int currentBrandId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(brandName))
        {
            return string.Empty;
        }

        var articles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "el", "la", "los", "las", "un", "una", "unos", "unas",
            "de", "del", "en", "y", "por", "para", "con"
        };

        var words = Regex.Split(brandName.Trim(), "\\s+");
        var matches = new List<(string Brand, string Owner)>();

        foreach (var rawWord in words)
        {
            var word = (rawWord ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(word) || articles.Contains(word))
            {
                continue;
            }

            const string sql = """
                SELECT
                    p.razonSocial AS Marca,
                    prop.razonSocial AS Propietario
                FROM dbo.Personas p
                LEFT JOIN dbo.Personas prop ON p.idPropietario = prop.idPersona
                WHERE p.idPersona <> @idMarca
                  AND p.marca = 1
                  AND p.razonSocial COLLATE Latin1_General_CI_AI LIKE @texto;
                """;

            await using var command = new SqlCommand(sql, connection);
            command.CommandType = CommandType.Text;
            command.Parameters.Add("@idMarca", SqlDbType.Int).Value = currentBrandId;
            command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(word);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var brand = Convert.ToString(reader["Marca"]) ?? string.Empty;
                var owner = reader["Propietario"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Propietario"]) ?? string.Empty;

                if (!matches.Any(x =>
                        string.Equals(x.Brand, brand, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.Owner, owner, StringComparison.OrdinalIgnoreCase)))
                {
                    matches.Add((brand, owner));
                }
            }
        }

        if (matches.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>
        {
            "Ya existen marcas parecidas:",
            string.Empty
        };

        foreach (var match in matches)
        {
            lines.Add(string.IsNullOrWhiteSpace(match.Owner)
                ? "• " + match.Brand
                : "• " + match.Brand + " | Propietario: " + match.Owner);
        }

        lines.Add(string.Empty);
        lines.Add("¿Desea guardar la marca igualmente?");

        return string.Join(Environment.NewLine, lines);
    }

    private static string LikePattern(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "%";
        }

        return "%" + text.Trim()
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_")
            .Replace("[", @"\[") + "%";
    }

    private static async Task<bool> HasSalesOrPurchasesAsync(SqlConnection connection, int brandId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT CASE
                WHEN EXISTS (SELECT 1 FROM dbo.Ventas WHERE idPersona = @idPersona)
                  OR EXISTS (SELECT 1 FROM dbo.Compras WHERE idProveedor = @idPersona)
                THEN 1 ELSE 0
            END;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = brandId;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) == 1;
    }

    private static async Task<bool> IsUsedByProductsAsync(SqlConnection connection, int brandId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.Corte
            WHERE idMarca = @idMarca;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idMarca", SqlDbType.Int).Value = brandId;
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) > 0;
    }

    private sealed record ExistingBrand(int BrandId, string BrandName, string Notes, int? OwnerId, int CompanyId);
}
