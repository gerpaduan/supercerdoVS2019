using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.People;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyPersonCommandService(ILegacyConnectionStringResolver connectionStringResolver) : IPersonCommandService
{
    public async Task<PersonSaveResult> SavePersonAsync(
        PersonSaveRequest request,
        CancellationToken cancellationToken = default)
    {
        var identification = NormalizeText(request.Identification, upper: true);
        var businessName = NormalizeText(request.BusinessName, upper: true);
        var address = NormalizeText(request.Address, upper: true);
        var city = NormalizeText(request.City, upper: true);
        var taxId = NormalizeTaxId(request.TaxId);
        var phone = (request.Phone ?? string.Empty).Trim();
        var email = (request.Email ?? string.Empty).Trim();
        var notes = (request.Notes ?? string.Empty).Trim();
        var isInsert = request.PersonId <= 0;

        if (string.IsNullOrWhiteSpace(identification))
        {
            return PersonSaveResult.Failure("La identificacion es obligatoria.");
        }

        if (string.IsNullOrWhiteSpace(businessName))
        {
            return PersonSaveResult.Failure("La razon social es obligatoria.");
        }

        if (request.VatId <= 0)
        {
            return PersonSaveResult.Failure("Seleccione una condicion frente al IVA.");
        }

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        ExistingPerson? existingPerson = null;
        if (!isInsert)
        {
            existingPerson = await GetPersonAsync(connection, request.PersonId, cancellationToken);
            if (existingPerson is null)
            {
                return PersonSaveResult.Failure("No se encontro la persona a modificar.");
            }

            if (!CanModifyPerson(request.CompanyId, existingPerson.CompanyId))
            {
                return PersonSaveResult.Failure("No tiene permisos para modificar personas globales.");
            }

            var hasMovements = await HasSalesOrPurchasesAsync(connection, request.PersonId, cancellationToken);
            if (hasMovements &&
                !request.IsAdministrator &&
                ProtectedFieldsChanged(identification, businessName, taxId, existingPerson))
            {
                return PersonSaveResult.Failure("Esta persona ya tiene compras o ventas registradas. Por seguridad, solo un administrador puede modificar Razon Social, CUIT o Identificacion.");
            }
        }

        var duplicatePersonId = await FindPersonIdByTaxIdAsync(connection, taxId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(taxId) &&
            duplicatePersonId > 0 &&
            duplicatePersonId != request.PersonId)
        {
            return PersonSaveResult.Failure("El CUIT ingresado ya existe para otra persona.");
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
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = request.PersonId;
        command.Parameters.Add("@identificacion", SqlDbType.NVarChar, 200).Value = identification;
        command.Parameters.Add("@razonSocial", SqlDbType.NVarChar, 200).Value = businessName;
        command.Parameters.Add("@idIva", SqlDbType.Int).Value = request.VatId;
        command.Parameters.Add("@cuit", SqlDbType.NVarChar, 50).Value = taxId;
        command.Parameters.Add("@telefono", SqlDbType.NVarChar, 100).Value = phone;
        command.Parameters.Add("@email", SqlDbType.NVarChar, 120).Value = string.IsNullOrWhiteSpace(email)
            ? DBNull.Value
            : email;
        command.Parameters.Add("@domicilio", SqlDbType.NVarChar, 200).Value = address;
        command.Parameters.Add("@ciudad", SqlDbType.NVarChar, 100).Value = city;
        command.Parameters.Add("@otrosDatos", SqlDbType.NVarChar, -1).Value = notes;
        command.Parameters.Add("@tipo", SqlDbType.NVarChar, 100).Value = string.Empty;
        command.Parameters.Add("@ctaCte", SqlDbType.Bit).Value = request.CanManageCurrentAccount
            ? request.HasCurrentAccount
            : existingPerson?.HasCurrentAccount ?? false;
        command.Parameters.Add("@bonificacion", SqlDbType.Decimal).Value = request.Discount;
        command.Parameters.Add("@marca", SqlDbType.Bit).Value = false;
        command.Parameters.Add("@idPropietario", SqlDbType.Int).Value = DBNull.Value;
        command.Parameters["@bonificacion"].Precision = 18;
        command.Parameters["@bonificacion"].Scale = 2;

        await command.ExecuteNonQueryAsync(cancellationToken);
        return PersonSaveResult.Ok();
    }

    private static async Task<ExistingPerson?> GetPersonAsync(SqlConnection connection, int personId, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT TOP 1
                idPersona,
                idEmpresa,
                identificacion,
                razonSocial,
                cuit,
                ctaCte
            FROM dbo.Personas
            WHERE idPersona = @idPersona
              AND marca = 0;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = personId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new ExistingPerson(
            reader["idPersona"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idPersona"]),
            reader["idEmpresa"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idEmpresa"]),
            Convert.ToString(reader["identificacion"]) ?? string.Empty,
            Convert.ToString(reader["razonSocial"]) ?? string.Empty,
            Convert.ToString(reader["cuit"]) ?? string.Empty,
            reader["ctaCte"] != DBNull.Value && Convert.ToBoolean(reader["ctaCte"]));
    }

    private static async Task<int> FindPersonIdByTaxIdAsync(SqlConnection connection, string taxId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(taxId) || !long.TryParse(taxId, out _))
        {
            return 0;
        }

        const string sql = """
            SELECT TOP 1 idPersona
            FROM dbo.Personas
            WHERE REPLACE(ISNULL(cuit, ''), '-', '') = @cuit;
            """;

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@cuit", SqlDbType.NVarChar, 50).Value = taxId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is null || result == DBNull.Value ? 0 : Convert.ToInt32(result);
    }

    private static async Task<bool> HasSalesOrPurchasesAsync(SqlConnection connection, int personId, CancellationToken cancellationToken)
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
        command.Parameters.Add("@idPersona", SqlDbType.Int).Value = personId;

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is not null && result != DBNull.Value && Convert.ToInt32(result) == 1;
    }

    private static bool ProtectedFieldsChanged(string identification, string businessName, string taxId, ExistingPerson existingPerson)
    {
        return !string.Equals(identification, NormalizeText(existingPerson.Identification, upper: true), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(businessName, NormalizeText(existingPerson.BusinessName, upper: true), StringComparison.OrdinalIgnoreCase)
            || !string.Equals(taxId, NormalizeTaxId(existingPerson.TaxId), StringComparison.OrdinalIgnoreCase);
    }

    private static bool CanModifyPerson(int currentCompanyId, int personCompanyId)
    {
        return !(currentCompanyId > 0 && personCompanyId == 0);
    }

    private static string NormalizeText(string? value, bool upper)
    {
        var normalized = (value ?? string.Empty).Trim();
        return upper ? normalized.ToUpperInvariant() : normalized;
    }

    private static string NormalizeTaxId(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Replace("-", string.Empty).Replace(" ", string.Empty);
    }

    private sealed record ExistingPerson(
        int PersonId,
        int CompanyId,
        string Identification,
        string BusinessName,
        string TaxId,
        bool HasCurrentAccount);
}
