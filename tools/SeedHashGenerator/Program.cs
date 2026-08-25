using System;
using System.Security.Cryptography;
using System.Text;

namespace SeedHashGenerator
{
    /// <summary>
    /// Regenerates the PBKDF2 seed-user hashes embedded in the SQL DDL scripts.
    /// Fixed salts keep the output deterministic and diffable; runtime
    /// <c>PasswordHasher.Hash</c> still uses a random salt for real users.
    /// Plaintext credentials are dev-only and appear only in comments/docs.
    /// </summary>
    internal static class Program
    {
        private const int Iterations = 100_000;
        private const int HashSize = 32;

        private static readonly (string Username, string Password, string Role, string Salt)[] Seeds =
        {
            ("rpingkian", "1234", "operator", "rpingkian.seed.1"),
            ("vsendrijas", "5678", "qa", "vsendrijas.seed2"),
            ("cordonez", "4567", "supervisor", "cordonez.seed.03")
        };

        private static void Main()
        {
            foreach (var seed in Seeds)
            {
                byte[] salt = Encoding.ASCII.GetBytes(seed.Salt);

                byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                    seed.Password,
                    salt,
                    Iterations,
                    HashAlgorithmName.SHA256,
                    HashSize);

                Console.WriteLine(
                    $"{seed.Username} | {seed.Role} | " +
                    $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}");
            }
        }
    }
}
