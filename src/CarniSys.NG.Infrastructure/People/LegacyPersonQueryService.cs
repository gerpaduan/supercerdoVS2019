using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.People;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyPersonQueryService(ILegacyConnectionStringResolver connectionStringResolver) : IPersonQueryService
{
    public async Task<IReadOnlyCollection<PersonListItem>> GetPeopleAsync(
        int companyId,
        PersonListQuery query,
        CancellationToken cancellationToken = default)
    {
        var normalizedSearchText = (query.SearchText ?? string.Empty).Trim();
        var skip = Math.Max(0, query.Skip);
        var take = query.Take <= 0 ? 50 : Math.Min(query.Take, 100);

        const string sql = """
            SELECT  
                p.idPersona,
                p.idEmpresa,
                p.identificacion AS nombreIdentif,
                p.razonSocial,
                i.abrev AS iva,
                p.cuit,
                p.telefono,
                p.ctaCte,
                p.bonificacion,
                p.domicilio,
                p.ciudad,
                p.otrosDatos
            FROM dbo.Personas p
            LEFT JOIN dbo.Iva i ON i.id = p.idIva
            WHERE p.marca = 0
              AND (@idEmpresa = 0 OR p.idEmpresa = 0 OR p.idEmpresa = @idEmpresa)
              AND (
                    p.identificacion LIKE @texto ESCAPE '\'
                 OR p.razonSocial    LIKE @texto ESCAPE '\'
                 OR p.cuit           LIKE @texto ESCAPE '\'
              )
            ORDER BY p.idEmpresa, p.razonSocial, p.identificacion;
            OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY;
            """;

        var items = new List<PersonListItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;
        command.Parameters.Add("@texto", SqlDbType.NVarChar, 200).Value = LikePattern(normalizedSearchText);
        command.Parameters.Add("@skip", SqlDbType.Int).Value = skip;
        command.Parameters.Add("@take", SqlDbType.Int).Value = take;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PersonListItem
            {
                PersonId = GetInt(reader, "idPersona"),
                CompanyId = GetInt(reader, "idEmpresa"),
                Identification = GetString(reader, "nombreIdentif"),
                BusinessName = GetString(reader, "razonSocial"),
                VatLabel = GetString(reader, "iva"),
                TaxId = GetString(reader, "cuit"),
                Phone = GetString(reader, "telefono"),
                Address = GetString(reader, "domicilio"),
                City = GetString(reader, "ciudad"),
                HasCurrentAccount = GetBool(reader, "ctaCte"),
                Discount = GetDecimal(reader, "bonificacion"),
                Notes = GetString(reader, "otrosDatos")
            });
        }

        return items;
    }

    public async Task<PersonDetailItem?> GetPersonByIdAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return null;
        }

        const string sql = """
            SELECT 
                p.idPersona,
                p.identificacion,
                p.razonSocial,
                p.tipo,
                p.otrosDatos,
                p.ctaCte,
                p.bonificacion,
                p.cuit,
                p.telefono,
                p.email,
                p.domicilio,
                p.ciudad,
                p.marca,
                p.idEmpresa,
                p.idPropietario,
                p.creado,
                p.idIva,
                i.iva,
                prop.razonSocial AS PropietarioNombre
            FROM dbo.Personas p
            LEFT JOIN dbo.Iva i ON i.id = p.idIva
            LEFT JOIN dbo.Personas prop ON prop.idPersona = p.idPropietario
            WHERE p.idPersona = @idPersona;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = personId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new PersonDetailItem
        {
            PersonId = GetInt(reader, "idPersona"),
            CompanyId = GetInt(reader, "idEmpresa"),
            Identification = GetString(reader, "identificacion"),
            BusinessName = GetString(reader, "razonSocial"),
            VatId = GetInt(reader, "idIva"),
            VatLabel = GetString(reader, "iva"),
            TaxId = GetString(reader, "cuit"),
            Phone = GetString(reader, "telefono"),
            Email = GetString(reader, "email"),
            Address = GetString(reader, "domicilio"),
            City = GetString(reader, "ciudad"),
            HasCurrentAccount = GetBool(reader, "ctaCte"),
            Discount = GetDecimal(reader, "bonificacion"),
            Notes = GetString(reader, "otrosDatos"),
            IsBrand = GetBool(reader, "marca"),
            OwnerId = GetNullableInt(reader, "idPropietario"),
            OwnerName = GetString(reader, "PropietarioNombre"),
            CreatedAt = GetDateTime(reader, "creado")
        };
    }

    public async Task<IReadOnlyCollection<PersonVatOption>> GetVatOptionsAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, iva
            FROM dbo.Iva
            ORDER BY id;
            """;

        var items = new List<PersonVatOption>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new PersonVatOption
            {
                VatId = GetInt(reader, "id"),
                Label = GetString(reader, "iva")
            });
        }

        return items;
    }

    public async Task<bool> HasSalesOrPurchasesAsync(int personId, CancellationToken cancellationToken = default)
    {
        if (personId <= 0)
        {
            return false;
        }

        const string sql = """
            SELECT CASE
                WHEN EXISTS (SELECT 1 FROM dbo.Ventas WHERE idPersona = @idPersona)
                  OR EXISTS (SELECT 1 FROM dbo.Compras WHERE idProveedor = @idPersona)
                THEN 1 ELSE 0
            END;
            """;

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = personId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) == 1;
    }

    private static string LikePattern(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "%";
        }

        return "%" + text
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

    private static DateTime GetDateTime(IDataRecord record, string columnName)
    {
        var ordinal = record.GetOrdinal(columnName);
        return record.IsDBNull(ordinal) ? DateTime.MinValue : Convert.ToDateTime(record.GetValue(ordinal));
    }
}
