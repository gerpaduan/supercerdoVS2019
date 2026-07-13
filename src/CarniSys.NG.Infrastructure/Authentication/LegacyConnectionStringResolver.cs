using System.Xml.Linq;
using Microsoft.Extensions.Options;

namespace CarniSys.NG.Infrastructure;

internal sealed class LegacyConnectionStringResolver(IOptions<LegacyAuthenticationOptions> options) : ILegacyConnectionStringResolver
{
    public string Resolve()
    {
        var configuredConnectionString = options.Value.ConnectionString;
        if (!string.IsNullOrWhiteSpace(configuredConnectionString))
        {
            return configuredConnectionString;
        }

        if (!options.Value.FallbackToWebConfig)
        {
            throw new InvalidOperationException("LegacyAuthentication:ConnectionString no esta configurado.");
        }

        var webConfigFullPath = Path.GetFullPath(options.Value.WebConfigPath, AppContext.BaseDirectory);
        if (!File.Exists(webConfigFullPath))
        {
            throw new InvalidOperationException($"No se encontro Web.config legacy en '{webConfigFullPath}'.");
        }

        var document = XDocument.Load(webConfigFullPath);
        var connectionString = document
            .Descendants("connectionStrings")
            .Elements("add")
            .FirstOrDefault(x => string.Equals((string?)x.Attribute("name"), options.Value.WebConfigConnectionName, StringComparison.OrdinalIgnoreCase))
            ?.Attribute("connectionString")
            ?.Value;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException($"No se encontro la connection string '{options.Value.WebConfigConnectionName}' en '{webConfigFullPath}'.");
        }

        return connectionString;
    }
}
