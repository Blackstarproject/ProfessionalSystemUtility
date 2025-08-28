using System;
using System.Security.Cryptography;
namespace ProfessionalSystemUtility
{
    public static class PasswordHelper
    {
        private const int KeySize = 256;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithm = HashAlgorithmName.SHA256;
        public static string HashPassword(string password, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltBytes, Iterations, _hashAlgorithm))
            {
                byte[] hash = rfc2898DeriveBytes.GetBytes(KeySize / 8);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
