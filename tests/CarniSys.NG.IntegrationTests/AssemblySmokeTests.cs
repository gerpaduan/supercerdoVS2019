namespace CarniSys.NG.IntegrationTests;

public class AssemblySmokeTests
{
    [Fact]
    public void InfrastructureAssemblyReference_Exists()
    {
        Assert.NotNull(typeof(CarniSys.NG.Infrastructure.AssemblyReference));
    }
}
