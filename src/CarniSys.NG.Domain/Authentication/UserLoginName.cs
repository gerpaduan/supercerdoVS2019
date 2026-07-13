namespace CarniSys.NG.Domain.Authentication;

public sealed record UserLoginName
{
    public UserLoginName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        Value = value.Trim();
    }

    public string Value { get; }

    public override string ToString() => Value;
}
