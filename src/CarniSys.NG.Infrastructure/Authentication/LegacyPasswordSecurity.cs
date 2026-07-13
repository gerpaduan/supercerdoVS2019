using System.Security.Cryptography;

namespace CarniSys.NG.Infrastructure;

internal static class LegacyPasswordSecurity
{
    private const int DefaultIterations = 100000;

    public static PasswordHashResult HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, DefaultIterations, HashAlgorithmName.SHA256, 32);

        return new PasswordHashResult(
            Convert.ToBase64String(hash),
            Convert.ToBase64String(salt),
            DefaultIterations);
    }

    public static bool VerifyPassword(string password, string? hashBase64, string? saltBase64, int iterations)
    {
        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(hashBase64)
            || string.IsNullOrWhiteSpace(saltBase64))
        {
            return false;
        }

        if (iterations <= 0)
        {
            iterations = DefaultIterations;
        }

        try
        {
            var salt = Convert.FromBase64String(saltBase64);
            var expectedHash = Convert.FromBase64String(hashBase64);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedHash.Length);
            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch
        {
            return false;
        }
    }

    public sealed record PasswordHashResult(string Hash, string Salt, int Iterations);
}
