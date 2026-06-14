using LifeHub.Application.Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace LifeHub.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int _iterations = 100_000;
        private const int _saltSize = 16;
        private const int _keySize = 32;

        public string Hash(string password)
        {
            var salt = new byte[_saltSize];
            RandomNumberGenerator.Fill(salt);

            var subkey = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, _iterations, _keySize);
            return $"{_iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(subkey)}";
        }

        public bool Verify(string hashedPassword, string password)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword)) return false;

            var parts = hashedPassword.Split('.', 3);
            if (parts.Length != 3) return false;

            if (!int.TryParse(parts[0], out var iterations)) return false;
            var salt = Convert.FromBase64String(parts[1]);
            var expected = Convert.FromBase64String(parts[2]);

            var actual = KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA256, iterations, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
    }
}
