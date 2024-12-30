using System.Security.Cryptography;
using System.Text;

namespace Gpt.Labs.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ConvertCrLfToLf(this string value)
        {
            return value.Replace("\r\n", "\n");
        }

        public static string ComputeSha256Hash(this string rawData)
        {
            // Create a SHA256
            using (var sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array
                var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}
