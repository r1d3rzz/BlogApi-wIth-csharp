using BlogApi.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Cryptography;
using System.Text;

namespace BlogApi.Helpers
{
    public class PasswordManager
    {

        public static string GenerateSalt()
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }
            return Convert.ToBase64String(salt);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] combinedBytes = new byte[saltBytes.Length + passwordBytes.Length];

            Buffer.BlockCopy(saltBytes, 0, combinedBytes, 0, saltBytes.Length);
            Buffer.BlockCopy(passwordBytes, 0, combinedBytes, saltBytes.Length, passwordBytes.Length);

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(combinedBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        public static bool VerifyPassword(string password, string salt, string hashedPassword)
        {
            string newHash = HashPassword(password, salt);
            return newHash + salt == hashedPassword;
        }

        public static bool IsTruePassword(string Password, string hashed_Password)
        {
            string hashedPassword = hashed_Password;
            var passwordArr = hashedPassword.Split("=");
            var salt = passwordArr[1] + "==";
            return VerifyPassword(Password, salt, hashedPassword);
        }
    }
}
