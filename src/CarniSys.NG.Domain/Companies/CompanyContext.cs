namespace CarniSys.NG.Domain.Companies;

public sealed class CompanyContext
{
    public CompanyContext(int companyId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(companyId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        CompanyId = companyId;
        Name = name.Trim();
    }

    public int CompanyId { get; }

    public string Name { get; }
}
