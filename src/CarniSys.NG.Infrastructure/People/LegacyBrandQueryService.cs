using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.People;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyBrandQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IBrandQueryService
{
    public async Task<IReadOnlyCollection<BrandListItem>> GetBrandsAsync(
        BrandListQuery query,
        CancellationToken cancellationToken = default)
    {
        var skip = query.Skip < 0 ? 0 : query.Skip;
        var take = query.Take <= 0 ? 50 : Math.Min(query.Take, 100);

        const string sql = """
            SELECT
                p.idPersona,
                p.idEmpresa,
                p.razonSocial AS Marca,
                p.otrosDatos AS otrosDatos,
                p.idPropietario,
                prop.razonSocial AS Propietario,
                prop.cuit AS cuit,
                prop.telefono AS telefono,
                prop.domicilio AS domicilio,
                prop.ciudad AS ciudad
            FROM dbo.Personas p
            LEFT JOIN dbo.Personas prop ON p.idPropietario = prop.idPersona
            WHERE p.marca = 1
              AND (p.identificacion LIKE @texto ESCAPE '\' OR p.razonSocial LIKE @texto ESCAPE '\')
            ORDER BY p.razonSocial
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
            """;

        var items = new List<BrandListItem>();
        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(query.SearchText ?? string.Empty);
        command.Parameters.Add("@skip", SqlDbType.Int).Value = skip;
        command.Parameters.Add("@take", SqlDbType.Int).Value = take;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new BrandListItem
            {
                BrandId = GetInt(reader, "idPersona"),
                CompanyId = GetInt(reader, "idEmpresa"),
                BrandName = GetString(reader, "Marca"),
                Notes = GetString(reader, "otrosDatos"),
                OwnerName = GetString(reader, "Propietario"),
                OwnerTaxId = GetString(reader, "cuit"),
                OwnerPhone = GetString(reader, "telefono"),
                OwnerAddress = GetString(reader, "domicilio"),
                OwnerCity = GetString(reader, "ciudad")
            });
        }

        return items;
    }

    public async Task<BrandDetailItem?> GetBrandByIdAsync(
        int brandId,
        CancellationToken cancellationToken = default)
    {
        if (brandId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT TOP 1
                p.idPersona,
                p.idEmpresa,
                p.razonSocial AS Marca,
                p.otrosDatos AS otrosDatos,
                p.idPropietario,
                prop.razonSocial AS Propietario,
                prop.cuit AS cuit,
                prop.telefono AS telefono,
                prop.domicilio AS domicilio,
                prop.ciudad AS ciudad
            FROM dbo.Personas p
            LEFT JOIN dbo.Personas prop ON p.idPropietario = prop.idPersona
            WHERE p.idPersona = @idPersona
              AND p.marca = 1;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = brandId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new BrandDetailItem
        {
            BrandId = GetInt(reader, "idPersona"),
            CompanyId = GetInt(reader, "idEmpresa"),
            BrandName = GetString(reader, "Marca"),
            Notes = GetString(reader, "otrosDatos"),
            OwnerId = GetNullableInt(reader, "idPropietario"),
            OwnerName = GetString(reader, "Propietario"),
            OwnerTaxId = GetString(reader, "cuit"),
            OwnerPhone = GetString(reader, "telefono"),
            OwnerAddress = GetString(reader, "domicilio"),
            OwnerCity = GetString(reader, "ciudad")
        };
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

    private static string GetString(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? string.Empty : Convert.ToString(record.GetValue(ordinal)) ?? string.Empty;
    }
}
