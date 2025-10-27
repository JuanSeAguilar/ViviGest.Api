using System.Security.Cryptography;
namespace ViviGest.Api.Services
{
    public interface IPasswordService
    {
        void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
    }

    public class PasswordService : IPasswordService
    {
        // parámetros seguros para PBKDF2
        private const int SaltSize = 32;      // 256 bits
        private const int HashSize = 64;      // 512 bits
        private const int Iterations = 100000;

        public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            salt = RandomNumberGenerator.GetBytes(SaltSize);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA512);
            hash = pbkdf2.GetBytes(HashSize);
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA512);
            var computed = pbkdf2.GetBytes(storedHash.Length);
            return CryptographicOperations.FixedTimeEquals(computed, storedHash);
        }
    }
}

