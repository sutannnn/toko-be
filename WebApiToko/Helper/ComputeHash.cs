using System.Security.Cryptography;
using System.Text;
namespace WebApiToko.Helper
{
    public static class HashHelper
    {
        public static string ComputeSha256Hash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
    }
}
