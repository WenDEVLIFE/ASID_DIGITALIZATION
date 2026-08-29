using System;
using System.Security.Cryptography;

namespace ASID.Edge.Services
{
    /// <summary>
    /// PBKDF2 password hashing (SHA-256, 100,000 iterations, 16-byte salt,
    /// 32-byte hash) stored as a single self-describing string in the format
    /// "{iterations}.{saltBase64}.{hashBase64}".
    /// </summary>
    public static class PasswordHasher
    {
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string stored)
        {
            if (string.IsNullOrEmpty(stored))
            {
                return false;
            }

            string[] parts = stored.Split('.');
            if (parts.Length != 3)
            {
                return false;
            }

            if (!int.TryParse(parts[0], out int iterations) || iterations <= 0)
            {
                return false;
            }

            byte[] salt;
            byte[] expected;
            try
            {
                salt = Convert.FromBase64String(parts[1]);
                expected = Convert.FromBase64String(parts[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] actual = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expected.Length);

            return FixedTimeEquals(actual, expected);
        }

        // Constant-time comparison: never early-exit on a mismatched byte.
        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            int diff = 0;
            for (int i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
