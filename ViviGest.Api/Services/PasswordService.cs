using System;
using System.Security.Cryptography;
using System.Text;

namespace ViviGest.Api.Services
{
    public interface IPasswordService
    {
        void CreatePasswordHash(string password, out byte[] hash, out byte[] salt);
        bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
    }

    public class PasswordService : IPasswordService
    {
        public void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using (var hmac = new HMACSHA512())
            {
                salt = hmac.Key;
                hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
        {
            Console.WriteLine($"🔐 DEBUG VerifyPassword:");
            Console.WriteLine($"   Password: {password}");
            Console.WriteLine($"   StoredHash length: {storedHash?.Length}");
            Console.WriteLine($"   StoredSalt length: {storedSalt?.Length}");

            if (storedHash == null || storedSalt == null)
                return false;

            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                //hola
                // Comparación segura
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
    }
}