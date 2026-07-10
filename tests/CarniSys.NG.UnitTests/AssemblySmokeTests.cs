namespace CarniSys.NG.UnitTests;

public class AssemblySmokeTests
{
    [Fact]
    public void DomainAssemblyReference_Exists()
    {
        Assert.NotNull(typeof(CarniSys.NG.Domain.AssemblyReference));
    }
}
