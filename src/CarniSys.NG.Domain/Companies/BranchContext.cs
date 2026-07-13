namespace CarniSys.NG.Domain.Companies;

public sealed class BranchContext
{
    public BranchContext(int branchId, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(branchId);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        BranchId = branchId;
        Name = name.Trim();
    }

    public int BranchId { get; }

    public string Name { get; }
}
