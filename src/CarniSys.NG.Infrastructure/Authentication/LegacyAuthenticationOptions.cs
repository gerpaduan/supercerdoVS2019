namespace CarniSys.NG.Infrastructure;

public sealed class LegacyAuthenticationOptions
{
    public const string SectionName = "LegacyAuthentication";

    public string? ConnectionString { get; set; }

    public bool FallbackToWebConfig { get; set; } = true;

    public string WebConfigPath { get; set; } = "..\\..\\..\\..\\Web\\Web.config";

    public string WebConfigConnectionName { get; set; } = "ConexionPrincipal";
}
