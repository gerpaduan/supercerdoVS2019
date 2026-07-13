using System.Data;
using System.Data.SqlClient;
using CarniSys.NG.Application.Companies;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyBranchLookupService(ILegacyConnectionStringResolver connectionStringResolver) : IBranchLookupService
{
    public async Task<IReadOnlyCollection<BranchLookupItem>> GetBranchesAsync(int companyId, CancellationToken cancellationToken = default)
    {
        if (companyId <= 0)
        {
            return [];
        }

        const string sql = """
            SELECT idSucursal, sucursal
            FROM dbo.Sucursal
            WHERE idEmpresa = @idEmpresa
            ORDER BY sucursal;
            """;

        var items = new List<BranchLookupItem>();

        await using var connection = new SqlConnection(connectionStringResolver.Resolve());
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.CommandType = CommandType.Text;
        command.Parameters.Add("@idEmpresa", SqlDbType.Int).Value = companyId;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new BranchLookupItem
            {
                BranchId = reader["idSucursal"] == DBNull.Value ? 0 : Convert.ToInt32(reader["idSucursal"]),
                BranchName = reader["sucursal"] == DBNull.Value ? string.Empty : Convert.ToString(reader["sucursal"]) ?? string.Empty
            });
        }

        return items;
    }
}
