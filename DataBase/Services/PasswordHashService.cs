using System.Security.Cryptography;
using System.Text;

namespace DataBase.Services
{
    public static class PasswordHashService
    {
        public static string HashPassword(string password)
        {
            var sha = SHA256.Create();
            var asByteArray = Encoding.Default.GetBytes(password);
            var hashedPassword = sha.ComputeHash(asByteArray);
            return Convert.ToBase64String(hashedPassword);
        }

        public static bool VerifyPassword(string inputPassword, string hashedPassword)
        {
            string inputPasswordHash = HashPassword(inputPassword);
            return inputPasswordHash == hashedPassword;
        }
    }
}