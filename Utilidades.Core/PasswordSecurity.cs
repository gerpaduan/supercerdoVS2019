// TODO(claude): copia temporal, ver comentario en IEmpresaContext.cs de esta carpeta.
using System;
using System.Security.Cryptography;

namespace Utilidades
{
    public static class PasswordSecurity
    {
        public const int DefaultIterations = 100000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public sealed class PasswordHashResult
        {
            public string Hash { get; set; }
            public string Salt { get; set; }
            public int Iterations { get; set; }
        }

        public static PasswordHashResult HashPassword(string password, int iterations = DefaultIterations)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña es obligatoria.", nameof(password));

            if (iterations <= 0)
                iterations = DefaultIterations;

            using (var rng = RandomNumberGenerator.Create())
            {
                var salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var hash = deriveBytes.GetBytes(HashSize);
                    return new PasswordHashResult
                    {
                        Hash = Convert.ToBase64String(hash),
                        Salt = Convert.ToBase64String(salt),
                        Iterations = iterations
                    };
                }
            }
        }

        public static bool VerifyPassword(string password, string hashBase64, string saltBase64, int iterations)
        {
            if (string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(hashBase64) ||
                string.IsNullOrWhiteSpace(saltBase64))
            {
                return false;
            }

            if (iterations <= 0)
                iterations = DefaultIterations;

            try
            {
                var salt = Convert.FromBase64String(saltBase64);
                var expectedHash = Convert.FromBase64String(hashBase64);

                using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256))
                {
                    var actualHash = deriveBytes.GetBytes(expectedHash.Length);
                    return FixedTimeEquals(actualHash, expectedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        public static string GenerateToken(int byteLength = 32)
        {
            if (byteLength <= 0)
                byteLength = 32;

            using (var rng = RandomNumberGenerator.Create())
            {
                var buffer = new byte[byteLength];
                rng.GetBytes(buffer);
                return ToUrlSafeBase64(buffer);
            }
        }

        public static string ComputeSha256Base64(string value)
        {
            if (value == null)
                value = string.Empty;

            using (var sha = SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(value);
                return Convert.ToBase64String(sha.ComputeHash(bytes));
            }
        }

        // PKCE (RFC 7636): code_challenge = BASE64URL(SHA256(ASCII(code_verifier))). Usado por
        // Negocio.MercadoPagoOAuthClient -- el code_verifier en si se genera con GenerateToken
        // (32 bytes -> 43 caracteres, el minimo que exige el RFC).
        public static string ComputeSha256UrlSafeBase64(string value)
        {
            if (value == null)
                value = string.Empty;

            using (var sha = SHA256.Create())
            {
                var bytes = System.Text.Encoding.ASCII.GetBytes(value);
                return ToUrlSafeBase64(sha.ComputeHash(bytes));
            }
        }

        private static string ToUrlSafeBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static bool FixedTimeEquals(byte[] a, byte[] b)
        {
            if (a == null || b == null || a.Length != b.Length)
                return false;

            var diff = 0;
            for (var i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];

            return diff == 0;
        }
    }
}
